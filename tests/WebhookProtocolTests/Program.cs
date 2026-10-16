using System.Net;
using System.Security.Cryptography;
using System.Text;
using MapIdeaHub.BirSign.NetCoreExtension;
using MapIdeaHub.BirSign.SharedKernel;
using MapIdeaHub.BirSign.SharedKernel.Dtos;
using Microsoft.AspNetCore.Http;

const string path = "/api/birsign/webhook";
const string payload = """{"eventType":"DepartmentDeleted","departmentId":"aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa","eventId":"bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"}""";
const string historicSecret = "legacy-secret-which-is-no-longer-used";

var invocationCount = 0;
WebhookEvent? lastEvent = null;
var middleware = new WebhookMiddleware(
    _ => Task.CompletedTask,
    path,
    (_, webhookEvent) =>
    {
        ++invocationCount;
        lastEvent = webhookEvent;
        return Task.CompletedTask;
    },
    historicSecret); // Legacy constructor argument must not turn on HMAC validation.

{
    var request = NewRequest(payload);
    await middleware.InvokeAsync(request);
    Check(request.Response.StatusCode == StatusCodes.Status200OK, "unsigned webhook accepted despite legacy secret");
    Check(invocationCount == 1, "webhook handler executed once");
    Check(lastEvent is DepartmentDeletedEvent, "polymorphic event contract preserved");
    Check(lastEvent?.EventId == Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), "EventId preserved for idempotency");
}
{
    var malformed = NewRequest("not-json");
    await middleware.InvokeAsync(malformed);
    Check(malformed.Response.StatusCode == StatusCodes.Status400BadRequest, "invalid JSON rejected");
    Check(invocationCount == 1, "invalid JSON never reaches webhook handler");
}
{
    var unknown = NewRequest("""{"eventType":"UnknownFutureEvent"}""");
    await middleware.InvokeAsync(unknown);
    Check(unknown.Response.StatusCode == StatusCodes.Status400BadRequest, "unknown event type rejected");
}
{
    var tooLarge = NewRequest(payload);
    tooLarge.Request.ContentLength = 1024 * 1024 + 1;
    await middleware.InvokeAsync(tooLarge);
    Check(tooLarge.Response.StatusCode == StatusCodes.Status413PayloadTooLarge, "oversized webhook rejected");
}
var fallback = 0;
var fallbackMiddleware = new WebhookMiddleware(
    _ => { ++fallback; return Task.CompletedTask; },
    path,
    (_, _) => throw new Exception("must not run"));
{
    var wrongPath = NewRequest(payload);
    wrongPath.Request.Path = "/not-birsign";
    await fallbackMiddleware.InvokeAsync(wrongPath);
    Check(fallback == 1, "non-webhook request continues the middleware pipeline");
}
{
    var wrongVerb = NewRequest(payload);
    wrongVerb.Request.Method = HttpMethods.Get;
    await fallbackMiddleware.InvokeAsync(wrongVerb);
    Check(fallback == 2, "GET request does not invoke webhook handler");
}
var failingMiddleware = new WebhookMiddleware(
    _ => Task.CompletedTask,
    path,
    (_, _) => throw new InvalidOperationException("private database info"));
{
    var failing = NewRequest(payload);
    await failingMiddleware.InvokeAsync(failing);
    Check(failing.Response.StatusCode == StatusCodes.Status500InternalServerError, "handler failures return 500 for sender retry");
    Check(failing.Response.Body.Length == 0, "handler exceptions do not leak details");
}

// The public legacy helper remains callable, but neither receiver uses it.
var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(historicSecret)))
{
    var signature = "sha256=" + Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(timestamp + "." + payload))).ToLowerInvariant();
    Check(WebhookSignature.Verify(historicSecret, timestamp, signature, payload),
        "public legacy signature helper retained for API compatibility");
}
Console.WriteLine("ALL UNSIGNED WEBHOOK PROTOCOL TESTS PASSED");

static DefaultHttpContext NewRequest(string payload)
{
    var context = new DefaultHttpContext();
    context.Request.Path = "/api/birsign/webhook";
    context.Request.Method = HttpMethods.Post;
    context.Request.ContentType = "application/json";
    context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(payload));
    context.Response.Body = new MemoryStream();
    return context;
}

static void Check(bool condition, string reason)
{
    if (!condition) throw new InvalidOperationException(reason);
    Console.WriteLine("PASS: " + reason);
}
