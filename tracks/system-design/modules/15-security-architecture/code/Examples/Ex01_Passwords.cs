// Example: password hashing with PBKDF2, a random salt and a constant-time comparison.
// Module: system-design/15-security-architecture (basics)
// Tests: dotnet test --filter "FullyQualifiedName~Password"
// In an ASP.NET Core application use ASP.NET Core Identity instead of your own code.
using System.Security.Cryptography;

namespace SecurityDesign.Examples;

public static class Passwords
{
    private const int Iterations = 600_000; // OWASP recommendation for PBKDF2-HMAC-SHA256

    public static string Hash(string password, int iterations = Iterations)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(16);
        byte[] key = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, 32);

        // The parameters are stored with the hash, so they can be raised later.
        return $"{iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(key)}";
    }

    public static bool Verify(string password, string stored)
    {
        string[] parts = stored.Split('.');
        byte[] expected = Convert.FromBase64String(parts[2]);
        byte[] actual = Rfc2898DeriveBytes.Pbkdf2(
            password, Convert.FromBase64String(parts[1]), int.Parse(parts[0]), HashAlgorithmName.SHA256, expected.Length);

        return CryptographicOperations.FixedTimeEquals(actual, expected);
    }
}
