// Example: validating client certificates against a set of trusted roots, so a root can be rotated.
// Module: system-design/15-security-architecture (expert)
// Tests: dotnet test --filter "FullyQualifiedName~Certificate"
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace SecurityDesign.Examples;

public static class Certificates
{
    public static X509Certificate2 CreateRoot(string name, DateTimeOffset now)
    {
        using var key = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        var request = new CertificateRequest($"CN={name}", key, HashAlgorithmName.SHA256);
        request.CertificateExtensions.Add(new X509BasicConstraintsExtension(true, false, 0, true));
        return request.CreateSelfSigned(now.AddDays(-1), now.AddDays(365));
    }

    public static X509Certificate2 Issue(X509Certificate2 root, string name, DateTimeOffset now, int days = 30)
    {
        using var key = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        var request = new CertificateRequest($"CN={name}", key, HashAlgorithmName.SHA256);
        return request.Create(root, now.AddDays(-1), now.AddDays(days), RandomNumberGenerator.GetBytes(8));
    }

    public static bool IsTrusted(X509Certificate2 certificate, DateTimeOffset at, params X509Certificate2[] trustedRoots)
    {
        using var chain = new X509Chain();
        chain.ChainPolicy.TrustMode = X509ChainTrustMode.CustomRootTrust; // not the machine store
        chain.ChainPolicy.CustomTrustStore.AddRange(trustedRoots);
        chain.ChainPolicy.VerificationTime = at.UtcDateTime;
        // Test certificates have no revocation service. Do not copy this into production.
        chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
        return chain.Build(certificate);
    }
}
