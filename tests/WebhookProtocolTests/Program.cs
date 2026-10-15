using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using MapIdeaHub.BirSign.SharedKernel;
using MapIdeaHub.BirSign.SharedKernel.Dtos;

const string secret = "test-secret-only";
const string payload = "{\"eventType\":\"DepartmentDeleted\",\"departmentId\":\"aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa\",\"eventId\":\"bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb\"}";
var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
string Sign(string body, string time)
{
    using var h = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
    return "sha256=" + Convert.ToHexString(h.ComputeHash(Encoding.UTF8.GetBytes(time + "." + body))).ToLowerInvariant();
}
void Check(bool condition, string reason)
{
    if (!condition) throw new Exception(reason);
    Console.WriteLine("PASS: " + reason);
}
var signature = Sign(payload, timestamp);
Check(WebhookSignature.Verify(secret, timestamp, signature, payload), "valid signature");
Check(!WebhookSignature.Verify(secret, timestamp, signature, payload + " "), "modified body denied");
Check(!WebhookSignature.Verify("other-secret", timestamp, signature, payload), "wrong secret denied");
Check(!WebhookSignature.Verify(secret, timestamp, "sha256=xx", payload), "malformed signature denied");
var oldTimestamp = DateTimeOffset.UtcNow.AddMinutes(-6).ToUnixTimeSeconds().ToString();
Check(!WebhookSignature.Verify(secret, oldTimestamp, Sign(payload, oldTimestamp), payload), "old timestamp denied");
Check(!WebhookSignature.Verify(secret, long.MaxValue.ToString(), signature, payload), "overflow timestamp denied");
var parsed = JsonSerializer.Deserialize<WebhookEvent>(payload, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
Check(parsed is DepartmentDeletedEvent, "polymorphic event format backward compatible");
Check(parsed?.EventId == Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), "event id preserved");
Console.WriteLine("ALL SIGNATURE AND CONTRACT TESTS PASSED");
