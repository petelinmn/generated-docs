// Example: a struct is copied by value, a class is shared by reference.
// Module: csharp/02-types-and-variables (basics)
// Tests: dotnet test --filter "FullyQualifiedName~CopySemantics"
namespace Examples;

// A struct is a value type: assignment copies the whole value.
public struct PointStruct
{
    public int X;
    public int Y;
}

// A class is a reference type: assignment copies the reference, both variables see one object.
public class PointClass
{
    public int X;
    public int Y;
}

public static class CopySemantics
{
    public static (int Original, int Copy) MutateStructCopy()
    {
        var original = new PointStruct { X = 1 };
        var copy = original;
        copy.X = 99;
        return (original.X, copy.X);
    }

    public static (int Original, int Copy) MutateClassCopy()
    {
        var original = new PointClass { X = 1 };
        var copy = original;
        copy.X = 99;
        return (original.X, copy.X);
    }

    // The same rule applies to arguments: the method gets a copy of the struct, but the same object.
    public static void Reset(PointStruct point) => point.X = 0;

    public static void Reset(PointClass point) => point.X = 0;
}
