// Example: implicit and explicit conversions, overflow, parsing, rounding.
// Module: csharp/02-types-and-variables (basics)
// Tests: dotnet test --filter "FullyQualifiedName~Conversions"
using System.Globalization;

namespace Examples;

public static class Conversions
{
    // Implicit: no data can be lost (int -> long).
    public static long Widen(int value) => value;

    // Explicit: the fractional part is truncated toward zero, not rounded.
    public static int Truncate(double value) => (int)value;

    // Unchecked by default: an overflow silently wraps around.
    public static int AddUnchecked(int a, int b) => a + b;

    // checked turns an overflow into an OverflowException.
    public static int AddChecked(int a, int b) => checked(a + b);

    // TryParse reports failure with false instead of throwing like int.Parse.
    public static int? ParseOrNull(string text) =>
        int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value) ? value : null;

    // Math.Round uses banker's rounding by default: a tie goes to the nearest even number.
    public static double RoundDefault(double value) => Math.Round(value);

    public static double RoundAwayFromZero(double value) => Math.Round(value, MidpointRounding.AwayFromZero);
}
