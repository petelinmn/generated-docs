// Solution: exercise 2 — which assemblies an assembly depends on.
// Module: csharp/01-dotnet-platform (exercises)
// Tests: dotnet test --filter "FullyQualifiedName~AssemblyInspector"
using System.Reflection;

namespace Solutions;

public static class AssemblyInspector
{
    // References are recorded in the assembly metadata at compile time.
    public static IReadOnlyList<string> GetReferencedAssemblyNames(Assembly assembly) =>
        assembly.GetReferencedAssemblies()
            .Select(reference => reference.Name)
            .OfType<string>()
            .Distinct()
            .Order(StringComparer.Ordinal)
            .ToList();
}
