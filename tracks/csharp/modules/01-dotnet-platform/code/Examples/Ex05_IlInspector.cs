// Example: reading the IL that the C# compiler emitted for a method.
// Module: csharp/01-dotnet-platform (expert)
// Tests: dotnet test --filter "FullyQualifiedName~IlInspector"
using System.Reflection;

namespace Examples;

public static class IlInspector
{
    public static int Add(int a, int b) => a + b;

    // The raw IL body of a method, exactly as it is stored in the assembly.
    public static byte[] GetIl(MethodInfo method) =>
        method.GetMethodBody()?.GetILAsByteArray() ?? [];

    public static string AddIlAsHex() =>
        Convert.ToHexString(GetIl(typeof(IlInspector).GetMethod(nameof(Add))!));
}
