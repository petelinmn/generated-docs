// Solutions to the exercises of module csharp/02-types-and-variables.
// Tests: dotnet test --filter "FullyQualifiedName~Tests.S0"
using System.Globalization;

namespace Solutions;

// Exercise 1: parse user input safely and convert units.
public static class Temperature
{
    // Invariant culture: "36.6" must parse the same way on every machine.
    public static double? ParseCelsius(string? input) =>
        double.TryParse(input, NumberStyles.Float, CultureInfo.InvariantCulture, out double value)
            ? value
            : null;

    public static double ToFahrenheit(double celsius) => celsius * 9 / 5 + 32;
}

// Exercise 2: detect overflow instead of returning a wrong sum.
public static class SafeMath
{
    public static int CheckedSum(int[] values)
    {
        int sum = 0;
        foreach (int value in values)
        {
            sum = checked(sum + value);
        }
        return sum;
    }

    // A wider accumulator avoids the overflow altogether.
    public static long WideSum(int[] values)
    {
        long sum = 0;
        foreach (int value in values)
        {
            sum += value;
        }
        return sum;
    }
}

// Exercise 3: handle nullable input explicitly.
public static class Names
{
    public static string Normalize(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return "Anonymous";
        }
        string trimmed = name.Trim();
        return char.ToUpperInvariant(trimmed[0]) + trimmed[1..].ToLowerInvariant();
    }
}

// Exercise 4: the same fields in a different order take less memory.
public struct Wasteful
{
    public byte A;
    public long B;
    public byte C;
}

// Largest fields first: the two bytes share one 8-byte slot after the long.
public struct Compact
{
    public long B;
    public byte A;
    public byte C;
}
