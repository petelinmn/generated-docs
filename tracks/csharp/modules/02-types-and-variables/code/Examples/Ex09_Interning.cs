// Example: string literals are interned; strings built at run time are not.
// Module: csharp/02-types-and-variables (expert)
// Tests: dotnet test --filter "FullyQualifiedName~Interning"
namespace Examples;

public static class Interning
{
    // "hel" + "lo" is a compile-time constant, so both variables point to the same interned literal.
    public static bool LiteralsShareInstance()
    {
        string a = "hello";
        string b = "hel" + "lo";
        return ReferenceEquals(a, b);
    }

    // Concatenation at run time creates a new string object.
    public static bool RuntimeConcatSharesInstance(string prefix)
    {
        string a = "hello";
        string b = prefix + "lo";
        return ReferenceEquals(a, b);
    }

    // string.Intern returns the instance from the intern pool.
    public static bool InternedConcatSharesInstance(string prefix)
    {
        string a = "hello";
        string b = string.Intern(prefix + "lo");
        return ReferenceEquals(a, b);
    }
}
