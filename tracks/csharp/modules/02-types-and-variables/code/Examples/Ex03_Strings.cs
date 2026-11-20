// Example: strings are immutable; interpolation, verbatim and raw literals, StringBuilder.
// Module: csharp/02-types-and-variables (basics)
// Tests: dotnet test --filter "FullyQualifiedName~Strings"
using System.Globalization;
using System.Text;

namespace Examples;

public static class Strings
{
    // ToUpperInvariant returns a new string; the original one never changes.
    public static (string Original, string Upper) Immutability()
    {
        string original = "hello";
        string upper = original.ToUpperInvariant();
        return (original, upper);
    }

    // Interpolation with an explicit culture, so the decimal separator does not depend on the OS settings.
    public static string PriceTag(string name, decimal price) =>
        string.Create(CultureInfo.InvariantCulture, $"{name}: {price:F2}");

    // Verbatim (@) and raw (""") literals: backslashes and quotes need no escaping.
    public const string VerbatimPath = @"C:\temp\new";

    public const string RawJson = """
        {"name": "Alice", "path": "C:\temp"}
        """;

    // StringBuilder appends into one buffer instead of creating a new string every time.
    public static string NumbersCsv(int count)
    {
        var builder = new StringBuilder();
        for (int i = 0; i < count; i++)
        {
            if (i > 0)
            {
                builder.Append(',');
            }
            builder.Append(i);
        }
        return builder.ToString();
    }
}
