// Example: envelope encryption and key rotation; and a repository that enforces the tenant.
// Module: system-design/15-security-architecture (expert)
// Tests: dotnet test --filter "FullyQualifiedName~Envelope|FullyQualifiedName~Tenant"
using System.Security.Cryptography;

namespace SecurityDesign.Examples;

public sealed record Envelope(int KeyVersion, byte[] WrappedKey, byte[] Data);

// The key encryption keys stay "inside": in a real system they never leave a KMS or an HSM.
public sealed class KeyRing
{
    private readonly List<byte[]> _keys = [RandomNumberGenerator.GetBytes(32)];

    public int Current => _keys.Count;

    public void Rotate() => _keys.Add(RandomNumberGenerator.GetBytes(32));

    public Envelope Encrypt(byte[] plain)
    {
        byte[] dataKey = RandomNumberGenerator.GetBytes(32); // a fresh key for every object
        return new(Current, Seal(_keys[^1], dataKey), Seal(dataKey, plain));
    }

    public byte[] Decrypt(Envelope e) => Open(Open(_keys[e.KeyVersion - 1], e.WrappedKey), e.Data);

    // Rotation re-wraps the small data key; the data itself is not touched.
    public Envelope Rewrap(Envelope e) =>
        e with { KeyVersion = Current, WrappedKey = Seal(_keys[^1], Open(_keys[e.KeyVersion - 1], e.WrappedKey)) };

    // Layout: nonce | tag | ciphertext.
    private static byte[] Seal(byte[] key, byte[] plain)
    {
        var result = new byte[28 + plain.Length];
        RandomNumberGenerator.Fill(result.AsSpan(0, 12));
        using var aes = new AesGcm(key, 16);
        aes.Encrypt(result.AsSpan(0, 12), plain, result.AsSpan(28), result.AsSpan(12, 16));
        return result;
    }

    private static byte[] Open(byte[] key, byte[] data)
    {
        var plain = new byte[data.Length - 28];
        using var aes = new AesGcm(key, 16);
        aes.Decrypt(data.AsSpan(0, 12), data.AsSpan(28), data.AsSpan(12, 16), plain);
        return plain;
    }
}

public sealed record Invoice(string TenantId, int Number, decimal Amount);

// The tenant is a constructor argument taken from the authenticated context, never from the request body.
public sealed class TenantInvoices(Dictionary<(string Tenant, int Number), Invoice> table, string tenantId)
{
    public Invoice? Find(int number) => table.GetValueOrDefault((tenantId, number));

    public void Save(Invoice invoice)
    {
        if (invoice.TenantId != tenantId) throw new UnauthorizedAccessException("Wrong tenant.");
        table[(tenantId, invoice.Number)] = invoice;
    }
}
