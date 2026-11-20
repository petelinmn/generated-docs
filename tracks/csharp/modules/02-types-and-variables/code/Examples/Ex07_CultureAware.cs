// Example: string casing, comparison and number formatting depend on the culture.
// Module: csharp/02-types-and-variables (advanced)
// Tests: dotnet test --filter "FullyQualifiedName~CultureAware"
using System.Globalization;

namespace Examples;

public static class CultureAware
{
    // In Turkish, the uppercase of "i" is "İ" (U+0130), not "I".
    public static string ToUpperTurkish(string text) => text.ToUpper(CultureInfo.GetCultureInfo("tr-TR"));

    public static string ToUpperInvariant(string text) => text.ToUpperInvariant();

    // Ordinal comparison ignores the culture: the right choice for keys, identifiers, file names.
    public static bool EqualsIgnoreCase(string a, string b) => string.Equals(a, b, StringComparison.OrdinalIgnoreCase);

    public static string FormatInGerman(double value) => value.ToString("N2", CultureInfo.GetCultureInfo("de-DE"));

    public static string FormatInvariant(double value) => value.ToString("N2", CultureInfo.InvariantCulture);
}
