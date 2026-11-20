// Example: nullable value types (int?) and nullable reference types (string?).
// Module: csharp/02-types-and-variables (basics and advanced)
// Tests: dotnet test --filter "FullyQualifiedName~NullHandling"
namespace Examples;

public static class NullHandling
{
    // int? is Nullable<int>: a value type that can also hold "no value".
    public static int? FindIndex(int[] values, int target)
    {
        int index = Array.IndexOf(values, target);
        return index >= 0 ? index : null;
    }

    // ?. yields null instead of throwing, ?? supplies a fallback value.
    public static int LengthOrZero(string? text) => text?.Length ?? 0;

    // ??= assigns only when the variable is null.
    public static string EnsureName(string? name)
    {
        name ??= "Anonymous";
        return name;
    }

    // Without the null check the compiler warns: CS8602 "Dereference of a possibly null reference".
    public static string Shout(string? text)
    {
        if (text is null)
        {
            return "";
        }
        return text.ToUpperInvariant(); // flow analysis knows that text is not null here
    }
}
