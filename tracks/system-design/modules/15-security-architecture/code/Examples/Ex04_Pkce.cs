// Example: PKCE (RFC 7636): the verifier proves that the same client finishes the flow it started.
// Module: system-design/15-security-architecture (expert)
// Tests: dotnet test --filter "FullyQualifiedName~Pkce"
using System.Buffers.Text;
using System.Security.Cryptography;
using System.Text;

namespace SecurityDesign.Examples;

public static class Pkce
{
    // 32 random bytes give 43 characters, the minimum length allowed.
    public static string CreateVerifier() => Base64Url.EncodeToString(RandomNumberGenerator.GetBytes(32));

    public static string Challenge(string verifier) =>
        Base64Url.EncodeToString(SHA256.HashData(Encoding.ASCII.GetBytes(verifier)));
}
