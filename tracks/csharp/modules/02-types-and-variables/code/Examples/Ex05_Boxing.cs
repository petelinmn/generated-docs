// Example: boxing copies a value type into a new heap object.
// Module: csharp/02-types-and-variables (advanced)
// Tests: dotnet test --filter "FullyQualifiedName~Boxing"
namespace Examples;

public static class Boxing
{
    // Two boxes of the same value are two different objects with equal contents.
    public static (bool SameReference, bool EqualValues) CompareBoxes()
    {
        object a = 42;
        object b = 42;
        return (ReferenceEquals(a, b), a.Equals(b));
    }

    // == on variables of type object compares references, not values.
    public static bool EqualityOperatorOnBoxes()
    {
        object a = 42;
        object b = 42;
        return a == b;
    }

    // Unboxing must use the exact boxed type: a boxed int cannot be unboxed as long.
    public static long UnboxAsLong(object boxed) => (long)boxed;
}
