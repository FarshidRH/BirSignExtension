using System.Text;
using MapIdeaHub.BirSign.NetCoreExtension;
using MapIdeaHub.BirSign.SharedKernel.Dtos;
using Microsoft.AspNetCore.Http;

const string path = "/api/birsign/webhook";
const string payload = """{"eventType":"DepartmentDeleted","departmentId":"aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa","eventId":"bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"}""";

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
    });

{
    var request = NewRequest(payload);
    await middleware.InvokeAsync(request);
    Check(request.Response.StatusCode == StatusCodes.Status200OK, "unsigned webhook accepted with original public constructor");
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
{
    var withType = NewRequest("""{"eventType":"DepartmentUpdated","department":{"id":"aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa","title":"ICT","organizationType":1},"eventId":"cccccccc-cccc-cccc-cccc-cccccccccccc"}""");
    await middleware.InvokeAsync(withType);
    Check(lastEvent is DepartmentUpdatedEvent updated &&
          updated.Department.OrganizationType == MapIdeaHub.BirSign.SharedKernel.Enums.OrganizationType.Deputy,
          "department webhook deserializes typed organization enum");
}
{
    var fromOldApi = System.Text.Json.JsonSerializer.Deserialize<DepartmentApiDto>(
        """{"id":"aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa","title":"Legacy"}""",
        new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    Check(fromOldApi?.OrganizationType is null,
        "older API payload without organizationType stays distinguishable from Organization=0");
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

Check(typeof(WebhookMiddleware).GetConstructors().Length == 1 &&
      typeof(WebhookMiddleware).GetConstructors()[0].GetParameters().Length == 3,
      "ASP.NET Core WebhookMiddleware retains original three-parameter constructor");
Check(typeof(WebhookStartupFilter).GetConstructors().Length == 1 &&
      typeof(WebhookStartupFilter).GetConstructors()[0].GetParameters().Length == 2,
      "StartupFilter retains original two-parameter constructor");
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
