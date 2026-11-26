// Example: HMAC-signed requests with a timestamp and a nonce against replay.
// Module: system-design/15-security-architecture (advanced)
// Tests: dotnet test --filter "FullyQualifiedName~Signed"
using System.Security.Cryptography;
using System.Text;

namespace SecurityDesign.Examples;

public sealed record SignedRequest(string Method, string Path, string Body, long Timestamp, string Nonce, string Signature);

public sealed class RequestSigner(byte[] key, TimeProvider clock)
{
    private static readonly TimeSpan MaxAge = TimeSpan.FromMinutes(5);
    private readonly HashSet<string> _usedNonces = []; // shared and expiring storage in a real system

    public SignedRequest Sign(string method, string path, string body)
    {
        long now = clock.GetUtcNow().ToUnixTimeSeconds();
        string nonce = Convert.ToHexString(RandomNumberGenerator.GetBytes(16));
        return new(method, path, body, now, nonce, Compute(method, path, body, now, nonce));
    }

    public bool Verify(SignedRequest r)
    {
        byte[] expected = Encoding.ASCII.GetBytes(Compute(r.Method, r.Path, r.Body, r.Timestamp, r.Nonce));
        if (!CryptographicOperations.FixedTimeEquals(expected, Encoding.ASCII.GetBytes(r.Signature))) return false;

        TimeSpan age = clock.GetUtcNow() - DateTimeOffset.FromUnixTimeSeconds(r.Timestamp);
        if (age.Duration() > MaxAge) return false;

        return _usedNonces.Add(r.Nonce); // only a valid signature may consume a nonce
    }

    // Every part is covered. The body is hashed so that separators inside it cannot shift fields.
    private string Compute(string method, string path, string body, long timestamp, string nonce)
    {
        string bodyHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(body)));
        string text = $"{method}\n{path}\n{timestamp}\n{nonce}\n{bodyHash}";
        return Convert.ToHexString(HMACSHA256.HashData(key, Encoding.UTF8.GetBytes(text)));
    }
}
