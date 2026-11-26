// Example: a fitness function: module boundaries checked by a test.
// Module: system-design/11-architecture-styles (advanced)
// Tests: dotnet test --filter "FullyQualifiedName~Boundary"
using System.Reflection;

namespace Architecture.Examples;

public static class BoundaryRules
{
    private const BindingFlags All =
        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;

    // Looks at signatures only (fields, constructor and method parameters, return types).
    // A type used only inside a method body is not found: use an IL-based tool for that.
    public static IEnumerable<string> Violations(Assembly assembly, string module, string forbidden) =>
        from type in assembly.GetTypes()
        where type.Namespace == module
        from used in SignatureTypes(type)
        where used.Namespace == forbidden
        select $"{type.Name} -> {used.Name}";

    private static IEnumerable<Type> SignatureTypes(Type type) =>
        type.GetFields(All).Select(f => f.FieldType)
            .Concat(type.GetConstructors(All).SelectMany(c => c.GetParameters()).Select(p => p.ParameterType))
            .Concat(type.GetMethods(All).SelectMany(m => m.GetParameters().Select(p => p.ParameterType).Append(m.ReturnType)))
            .Distinct();
}
