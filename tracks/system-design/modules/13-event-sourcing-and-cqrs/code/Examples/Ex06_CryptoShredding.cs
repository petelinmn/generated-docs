// Example: crypto-shredding: personal data is encrypted with a per-subject key; deleting the key erases it.
// Module: system-design/13-event-sourcing-and-cqrs (expert)
// Tests: dotnet test --filter "FullyQualifiedName~Shredding"
// Keys live in memory here; in production use a key management service and its audit.
using System.Security.Cryptography;
using System.Text;

namespace EventSourcing.Examples;

public sealed class ShreddingVault
{
    private const int NonceSize = 12, TagSize = 16;
    private readonly Dictionary<string, byte[]> _keys = [];

    // Layout: nonce | tag | ciphertext.
    public byte[] Protect(string subject, string value)
    {
        if (!_keys.TryGetValue(subject, out byte[]? key)) _keys[subject] = key = RandomNumberGenerator.GetBytes(32);

        byte[] plain = Encoding.UTF8.GetBytes(value);
        var result = new byte[NonceSize + TagSize + plain.Length];
        Span<byte> nonce = result.AsSpan(0, NonceSize);
        RandomNumberGenerator.Fill(nonce); // never reuse a nonce with the same key

        using var aes = new AesGcm(key, TagSize);
        // The subject is authenticated too, so a record cannot be moved to another person.
        aes.Encrypt(nonce, plain, result.AsSpan(NonceSize + TagSize), result.AsSpan(NonceSize, TagSize),
            Encoding.UTF8.GetBytes(subject));
        return result;
    }

    public string? Reveal(string subject, byte[] data)
    {
        if (!_keys.TryGetValue(subject, out byte[]? key)) return null; // forgotten

        var plain = new byte[data.Length - NonceSize - TagSize];
        using var aes = new AesGcm(key, TagSize);
        aes.Decrypt(data.AsSpan(0, NonceSize), data.AsSpan(NonceSize + TagSize), data.AsSpan(NonceSize, TagSize),
            plain, Encoding.UTF8.GetBytes(subject));
        return Encoding.UTF8.GetString(plain);
    }

    public void Forget(string subject) => _keys.Remove(subject);
}
