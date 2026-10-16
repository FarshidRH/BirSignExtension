using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace MapIdeaHub.BirSign.SharedKernel
{
    /// <summary>Legacy HMAC helper retained for API compatibility. BirSign no longer sends signatures and receivers no longer verify them.</summary>
    public static class WebhookSignature
    {
        public static bool Verify(string secret, string timestamp, string signature, string body)
        {
            if (string.IsNullOrWhiteSpace(secret) || string.IsNullOrWhiteSpace(timestamp) ||
                string.IsNullOrWhiteSpace(signature) || !signature.StartsWith("sha256=", StringComparison.Ordinal))
                return false;
            if (!long.TryParse(timestamp, NumberStyles.None, CultureInfo.InvariantCulture, out var seconds))
                return false;
            DateTimeOffset sentAt;
            try { sentAt = DateTimeOffset.FromUnixTimeSeconds(seconds); }
            catch (ArgumentOutOfRangeException) { return false; }
            if ((DateTimeOffset.UtcNow - sentAt).Duration() > TimeSpan.FromMinutes(5))
                return false;

            byte[] expected;
            try { expected = Hex(signature.Substring(7)); }
            catch (FormatException) { return false; }
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret)))
            {
                var actual = hmac.ComputeHash(Encoding.UTF8.GetBytes(timestamp + "." + body));
                if (expected.Length != actual.Length) return false;
                var difference = 0;
                for (var i = 0; i < actual.Length; i++) difference |= expected[i] ^ actual[i];
                return difference == 0;
            }
        }

        private static byte[] Hex(string input)
        {
            if (input.Length != 64) throw new FormatException("Invalid HMAC length");
            var bytes = new byte[input.Length / 2];
            for (var i = 0; i < bytes.Length; i++)
                bytes[i] = byte.Parse(input.Substring(i * 2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            return bytes;
        }
    }
}
