// Example: what the .csproj leaves in the compiled assembly, and where base types live.
// Module: csharp/01-dotnet-platform (basics)
// Tests: dotnet test --filter "FullyQualifiedName~AssemblyInfo"
using System.Reflection;
using System.Runtime.Versioning;

namespace Examples;

public static class AssemblyInfo
{
    // <TargetFramework>net11.0</TargetFramework> becomes an attribute of the compiled assembly.
    public static string? TargetFrameworkOf(Assembly assembly) =>
        assembly.GetCustomAttribute<TargetFrameworkAttribute>()?.FrameworkName;

    // Base types such as int and string live in System.Private.CoreLib, the core of the BCL.
    public static string? AssemblyNameOf(Type type) => type.Assembly.GetName().Name;
}
