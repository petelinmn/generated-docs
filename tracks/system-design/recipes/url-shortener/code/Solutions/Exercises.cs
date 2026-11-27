// Solutions for the exercises of the recipe system-design/recipes/url-shortener.
// Tests: dotnet test --filter "FullyQualifiedName~Solutions"
using System.Security.Cryptography;
using System.Text;

namespace Shortener.Solutions;

// Exercise 1: a check character catches a mistyped code before any lookup.
public static class CheckedCode
{
    private const string Alphabet = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";

    public static string Append(string body) => body + Checksum(body);

    public static bool IsValid(string code) => code.Length > 1 && Checksum(code[..^1]) == code[^1];

    private static char Checksum(string body)
    {
        int sum = 0;
        for (int i = 0; i < body.Length; i++)
        {
            int value = Alphabet.IndexOf(body[i]);
            if (value < 0) return '\0';
            // Position weights make most swaps of neighbours visible.
            sum = (sum + value * (i + 1)) % Alphabet.Length;
        }

        return Alphabet[sum];
    }
}

// Exercise 2: the same owner and address give the same code.
public sealed class IdempotentCreator(Func<string> nextCode)
{
    private readonly Dictionary<string, string> _codes = [];
    private readonly Lock _gate = new();

    public string Create(string owner, string url)
    {
        // The separator cannot appear in an owner id, so different pairs do not merge.
        string key = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes($"{owner}\n{url}")));
        lock (_gate)
        {
            if (!_codes.TryGetValue(key, out string? code))
                _codes[key] = code = nextCode();
            return code;
        }
    }
}

// Exercise 3: the probability that a new random code is already taken.
public static class Keyspace
{
    public static double Size(int length) => Math.Pow(62, length);

    public static double CollisionChance(long stored, int length) => stored / Size(length);
}
