// Example: how Nullable<T> boxes, and what nullable reference annotations look like at run time.
// Module: csharp/02-types-and-variables (expert)
// Tests: dotnet test --filter "FullyQualifiedName~NullableInternals"
using System.Reflection;

namespace Examples;

public static class NullableInternals
{
    // Boxing an int? boxes the underlying int, or produces a null reference when there is no value.
    public static Type? BoxedTypeOf(int? value)
    {
        object? boxed = value;
        return boxed?.GetType();
    }

    public static int TakesNullable(string? text) => text?.Length ?? 0;

    public static int TakesNonNullable(string text) => text.Length;

    // string? and string are the same runtime type; the annotation is only metadata for the compiler.
    public static Type ParameterType(string methodName) => FirstParameter(methodName).ParameterType;

    // The annotation can still be read through reflection.
    public static NullabilityState ParameterNullability(string methodName) =>
        new NullabilityInfoContext().Create(FirstParameter(methodName)).ReadState;

    private static ParameterInfo FirstParameter(string methodName) =>
        typeof(NullableInternals).GetMethod(methodName)!.GetParameters()[0];
}
