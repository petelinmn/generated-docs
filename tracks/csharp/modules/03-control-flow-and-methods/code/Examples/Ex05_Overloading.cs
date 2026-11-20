// Example: which overload the compiler picks.
// Module: csharp/03-control-flow-and-methods (advanced)
// Tests: dotnet test --filter "FullyQualifiedName~Overloading"
namespace Examples;

public static class Overloading
{
    // The parameter is unused on purpose: only the chosen overload matters.
    public static string Describe(int value) => "int";

    public static string Describe(long value) => "long";

    public static string Describe(double value) => "double";

    public static string Describe(string? value) => "string";

    public static string Describe(object? value) => "object";
}
