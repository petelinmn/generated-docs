// Solutions to the exercises of system-design/17-design-case-studies.
// Tests: dotnet test --filter "FullyQualifiedName~Solutions"
namespace CaseStudies.Solutions;

public static class Exercises
{
    private const string Alphabet = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";

    // Exercise 1: decode a base62 code that came from a user.
    public static bool TryDecode(string code, out long value)
    {
        value = 0;
        if (code.Length is 0 or > 11) return false;

        foreach (char c in code)
        {
            int digit = Alphabet.IndexOf(c);
            if (digit < 0 || value > (long.MaxValue - digit) / 62)
            {
                value = 0;
                return false;
            }
            value = value * 62 + digit;
        }
        return true;
    }

    // Exercise 2: how many characters are needed for `count` different codes.
    public static int CodeLength(long count)
    {
        int length = 1;
        for (long capacity = 62; capacity < count; capacity *= 62) length++;
        return length;
    }

    // Exercise 3: chance of at least one collision among `n` random codes out of `space` (birthday bound).
    public static double CollisionProbability(long n, double space) =>
        -double.ExpM1(-(double)n * (n - 1) / (2 * space));
}
