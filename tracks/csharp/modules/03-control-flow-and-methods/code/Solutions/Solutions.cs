// Solutions to the exercises of module csharp/03-control-flow-and-methods.
// Tests: dotnet test --filter "FullyQualifiedName~Tests.S0"
using System.Globalization;

namespace Solutions;

// Exercise 1: a switch expression with relational patterns and a guard for invalid input.
public static class Grades
{
    public static string Grade(int score) => score switch
    {
        < 0 or > 100 => throw new ArgumentOutOfRangeException(nameof(score), score, "Score must be 0..100"),
        >= 90 => "A",
        >= 80 => "B",
        >= 70 => "C",
        >= 60 => "D",
        _ => "F",
    };
}

// Exercise 2: the Try-pattern with out parameters.
public static class Coordinates
{
    public static bool TryParse(string? input, out int x, out int y)
    {
        x = 0;
        y = 0;
        if (input is null)
        {
            return false;
        }
        string[] parts = input.Split(',');
        return parts.Length == 2
            && int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out x)
            && int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out y);
    }
}

// Exercise 3: loops plus a static local function.
public static class Primes
{
    public static int CountUpTo(int limit)
    {
        int count = 0;
        for (int n = 2; n <= limit; n++)
        {
            if (IsPrime(n))
            {
                count++;
            }
        }
        return count;

        static bool IsPrime(int n)
        {
            for (int divisor = 2; divisor * divisor <= n; divisor++)
            {
                if (n % divisor == 0)
                {
                    return false;
                }
            }
            return true;
        }
    }
}

// Exercise 4: a foreach-able type without IEnumerable and without heap allocations.
public readonly struct Countdown
{
    private readonly int _from;

    public Countdown(int from) => _from = from;

    public Enumerator GetEnumerator() => new(_from);

    public struct Enumerator
    {
        private int _current;

        public Enumerator(int from) => _current = from + 1;

        public readonly int Current => _current;

        public bool MoveNext() => --_current > 0;
    }
}
