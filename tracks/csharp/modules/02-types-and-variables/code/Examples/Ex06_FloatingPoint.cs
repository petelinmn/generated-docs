// Example: double is binary and inexact, decimal is decimal and exact for money.
// Module: csharp/02-types-and-variables (advanced)
// Tests: dotnet test --filter "FullyQualifiedName~FloatingPoint"
using System.Globalization;

namespace Examples;

public static class FloatingPoint
{
    // 0.1 has no exact binary representation, so the sum is slightly off.
    public static bool DoubleSumIsExact() => 0.1 + 0.2 == 0.3;

    public static bool DecimalSumIsExact() => 0.1m + 0.2m == 0.3m;

    // "R" prints the shortest text that round-trips to the same double.
    public static string DoubleSumText() => (0.1 + 0.2).ToString("R", CultureInfo.InvariantCulture);

    // Compare doubles with a tolerance, never with ==.
    public static bool NearlyEqual(double a, double b, double tolerance = 1e-9) => Math.Abs(a - b) <= tolerance;

    // Floating-point division by zero does not throw: it yields infinity (integer division would throw).
    public static double DivideByZero(double value) => value / 0.0;
}
