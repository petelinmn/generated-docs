// Example: receiving a webhook: verify the signature, drop repeats, answer fast and process later.
// Module: system-design/05-api-design (expert)
// Tests: dotnet test --filter "FullyQualifiedName~Webhook"
using System.Security.Cryptography;
using System.Text;

namespace ApiDesign.Examples;

public static class WebhookSignature
{
    // The format of GitHub's X-Hub-Signature-256 header.
    public static string Sign(string secret, ReadOnlySpan<byte> body) =>
        "sha256=" + Convert.ToHexStringLower(HMACSHA256.HashData(Encoding.UTF8.GetBytes(secret), body));

    // Sign the raw bytes as received: re-serialized JSON may differ.
    public static bool Verify(string secret, ReadOnlySpan<byte> body, string header) =>
        CryptographicOperations.FixedTimeEquals(
            Encoding.ASCII.GetBytes(Sign(secret, body)), Encoding.ASCII.GetBytes(header));
}

public enum WebhookResult { Accepted, Duplicate, Rejected }

// In memory. In production the delivery id and the queued work are stored in one transaction.
public sealed class WebhookReceiver(string secret)
{
    private readonly HashSet<string> _deliveries = [];

    public Queue<string> Work { get; } = new();

    public WebhookResult Receive(string deliveryId, byte[] body, string signature)
    {
        if (!WebhookSignature.Verify(secret, body, signature)) return WebhookResult.Rejected;
        if (!_deliveries.Add(deliveryId)) return WebhookResult.Duplicate;

        Work.Enqueue(Encoding.UTF8.GetString(body)); // processed by a worker, not here
        return WebhookResult.Accepted;
    }
}
