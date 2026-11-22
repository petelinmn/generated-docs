---
title: "Reflection and source generators: advanced"
lang: en
track: csharp
kind: module
doc: advanced
slug: 20-reflection-and-source-generators
part: 3
tags: [reflection, expression-trees, source-generators, incremental-generator, delegate-caching, system-text-json, generated-regex]
versions: ".NET 11, C# 15; Microsoft.CodeAnalysis.CSharp 4.14.0"
status: draft
updated: 2026-11-22
---

# Reflection and source generators: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [basics](basics.en.md). **Time:** ~1.5 h.

## The cost of reflection and speeding it up

Dynamic calls are slower than direct ones and can allocate memory. Allocation per call (Release):

| Operation | Allocated |
|---|---|
| a direct read of the property `person.Name` | 0 B |
| `PropertyInfo.GetValue(person)` (a string) | 0 B |
| `MethodInfo.Invoke(person, ["x"])` | 72 B |
| a delegate created once via `Delegate.CreateDelegate` | 0 B |

`Invoke` allocates an argument array and boxes value-type arguments; `GetValue` for value types boxes the result. The lookup time (`GetProperty`, `GetMethod`) is noticeable too, so **reflection objects are cached**, and hot calls are turned into delegates:

```csharp
var greet = typeof(Person).GetMethod("Greet")!;
var del = (Func<Person, string, string>)Delegate.CreateDelegate(typeof(Func<Person, string, string>), greet);
Console.WriteLine(del(new Person("Dan", 5), "Yo"));   // Yo, Dan — a typed call without a per-call Invoke cost
```

## Compiling expressions for fast access

If access has to be built dynamically, an expression tree is compiled into a delegate once, and then the call is almost like a direct one. An example is copying same-named properties between objects:

```csharp
public static class Mapper
{
    public static void Copy<TFrom, TTo>(TFrom from, TTo to)          // reflection on every call
    {
        foreach (var sp in typeof(TFrom).GetProperties())
        {
            var tp = typeof(TTo).GetProperty(sp.Name);
            if (tp is { CanWrite: true } && tp.PropertyType == sp.PropertyType) tp.SetValue(to, sp.GetValue(from));
        }
    }

    private static class Cache<TFrom, TTo>
    {
        public static readonly Action<TFrom, TTo> Copier = Build();   // built once per pair of types
        private static Action<TFrom, TTo> Build()
        {
            var f = Expression.Parameter(typeof(TFrom), "f"); var t = Expression.Parameter(typeof(TTo), "t");
            var assigns = new List<Expression>();
            foreach (var sp in typeof(TFrom).GetProperties())
            {
                var tp = typeof(TTo).GetProperty(sp.Name);
                if (tp is { CanWrite: true } && tp.PropertyType == sp.PropertyType)
                    assigns.Add(Expression.Assign(Expression.Property(t, tp), Expression.Property(f, sp)));
            }
            return Expression.Lambda<Action<TFrom, TTo>>(Expression.Block(assigns.Count == 0 ? [Expression.Empty()] : assigns), f, t).Compile();
        }
    }
    public static void CopyFast<TFrom, TTo>(TFrom from, TTo to) => Cache<TFrom, TTo>.Copier(from, to);
}
```

Verified: both variants copied `Name` and `Age` (`Extra` stayed `null`); allocations per call — `64 B` for the reflective one and `0 B` for the compiled one. The static class `Cache<TFrom, TTo>` holds a delegate for each pair of types: the "static cache in a generic class" technique ([06 · Collections and generics](../06-collections-and-generics/expert.en.md#static-fields-of-a-generic-type)).

A limitation: `Compile()` generates code at run time — this is unavailable in Native AOT ([19 · Performance](../19-performance/expert.en.md#native-aot)); generators are needed there.

## Plugin discovery

```csharp
public static class PluginLoader
{
    public static IEnumerable<T> Load<T>(Assembly asm) =>
        asm.GetTypes()
           .Where(t => typeof(T).IsAssignableFrom(t) && t is { IsClass: true, IsAbstract: false } && t.GetConstructor(Type.EmptyTypes) is not null)
           .OrderBy(t => t.Name)
           .Select(t => (T)Activator.CreateInstance(t)!);
}
```

Verified: for `IPlugin`, `alpha` and `beta` were loaded; the abstract class and types without a public parameterless constructor were skipped. In real applications plugins are obtained through DI ([13 · DI, configuration, logging](../13-di-config-logging/overview.en.md)) and assemblies are loaded through an `AssemblyLoadContext`.

## Source generators

A generator implements `IIncrementalGenerator` and lives in a project targeting `netstandard2.0`, attached to the consumer as an **analyzer**. It runs inside the compiler, reads syntax and semantics, and adds files. A minimal generator project:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>netstandard2.0</TargetFramework>
    <IsRoslynComponent>true</IsRoslynComponent>
    <EnforceExtendedAnalyzerRules>true</EnforceExtendedAnalyzerRules>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.CodeAnalysis.CSharp" Version="4.14.0" PrivateAssets="all" />
  </ItemGroup>
</Project>
```

A generator that creates `ToString` for a class with the `[AutoToString]` attribute:

```csharp
[Generator]
public sealed class ToStringGenerator : IIncrementalGenerator
{
    private const string AttributeSource = """
        namespace Gen
        {
            [System.AttributeUsage(System.AttributeTargets.Class)]
            internal sealed class AutoToStringAttribute : System.Attribute { }
        }
        """;

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // 1) emit the marker attribute itself
        context.RegisterPostInitializationOutput(ctx => ctx.AddSource("AutoToStringAttribute.g.cs", SourceText.From(AttributeSource, Encoding.UTF8)));

        // 2) find classes marked with it
        var classes = context.SyntaxProvider.ForAttributeWithMetadataName(
            "Gen.AutoToStringAttribute",
            predicate: static (node, _) => node is ClassDeclarationSyntax,
            transform: static (ctx, _) =>
            {
                var symbol = (INamedTypeSymbol)ctx.TargetSymbol;
                var props = symbol.GetMembers().OfType<IPropertySymbol>()
                    .Where(p => p.DeclaredAccessibility == Accessibility.Public && !p.IsStatic && p.GetMethod is not null)
                    .Select(p => p.Name).ToImmutableArray();
                return (Ns: symbol.ContainingNamespace.IsGlobalNamespace ? null : symbol.ContainingNamespace.ToDisplayString(),
                        Name: symbol.Name, Props: props);
            });

        // 3) generate a partial class with ToString
        context.RegisterSourceOutput(classes, static (spc, c) =>
        {
            var sb = new StringBuilder();
            if (c.Ns is not null) sb.AppendLine($"namespace {c.Ns} {{");
            sb.AppendLine($"partial class {c.Name}");
            sb.AppendLine("{");
            sb.AppendLine("    public override string ToString() => $\"" + c.Name + " {{ " + string.Join(", ", c.Props.Select(p => $"{p} = {{{p}}}")) + " }}\";");
            sb.AppendLine("}");
            if (c.Ns is not null) sb.AppendLine("}");
            spc.AddSource($"{c.Name}.ToString.g.cs", SourceText.From(sb.ToString(), Encoding.UTF8));
        });
    }
}
```

The consumer attaches the generator with a reference with special attributes:

```xml
<ProjectReference Include="..\Gen\Gen.csproj" OutputItemType="Analyzer" ReferenceOutputAssembly="false" />
```

```csharp
using Gen;

[AutoToString]
public partial class Person
{
    public string Name { get; set; } = "";
    public int Age { get; set; }
    private int Secret { get; set; }       // private: not included
}

var p = new Person { Name = "Ann", Age = 30 };
Console.WriteLine(p);                       // Person { Name = Ann, Age = 30 }
```

Verified by running: the output is `Person { Name = Ann, Age = 30 }`; for a class in a namespace (`Shapes.Point`) — `Point { X = 1, Y = 2 }`. With `EmitCompilerGeneratedFiles` on, the generated file lies in `Generated/…/Person.ToString.g.cs` and contains:

```csharp
partial class Person
{
    public override string ToString() => $"Person {{ Name = {Name}, Age = {Age} }}";
}
```

Subtleties: the class must be `partial`; the `{{ }}` template escapes curly braces in the generated interpolated string (without it the generated code does not compile — that is what happened on the first attempt). **Incrementality**: `ForAttributeWithMetadataName` and intermediate models (value tuples, `ImmutableArray`) let the compiler cache stages, so the generator is not recomputed on every keystroke in the IDE; do not keep `ISymbol`/`SyntaxNode` in the model.

## The platform's ready generators

| Generator | What it gives |
|---|---|
| `[LoggerMessage]` | fast logging methods ([13 · DI](../13-di-config-logging/advanced.en.md#loggermessage-fast-messages)) |
| `[GeneratedRegex]` | regular expressions compiled in advance |
| `JsonSerializerContext` + `[JsonSerializable]` | JSON serialization without reflection, AOT-compatible |
| `[LibraryImport]` | native code calls |
| Request Delegate Generator | Minimal API handlers for AOT ([14](../14-aspnet-core-minimal-api/expert.en.md)) |

Verified on a JSON and Regex example: `[JsonSerializable(typeof(Item))] partial class ItemContext : JsonSerializerContext` serializes the record `Item("pen", 3)` into `{"name":"pen","qty":3}` (with `CamelCase`), and `[GeneratedRegex(@"\d+")] static partial Regex Digits();` finds `123` in `abc 123 def`. The generated files are `ItemContext.g.cs`, `ItemContext.Item.g.cs`, `RegexGenerator.g.cs` and others.

## Debugging and testing generators

- Enable `EmitCompilerGeneratedFiles` and `CompilerGeneratedFilesOutputPath` to see the result on disk.
- Generation errors show up as compile errors in the generated file (such as `CS1073` for unescaped braces).
- Generators are tested by running the compiler on a source string (`CSharpGeneratorDriver`) and comparing the output with a snapshot.
- To report warnings and errors to the user the generator reports a `Diagnostic` through `ReportDiagnostic`.

## Best practices

| Rule | Why |
|---|---|
| Cache `MethodInfo`/delegates | lookup and `Invoke` are expensive |
| Move repeated reflection into a generator | zero run-time cost, AOT |
| Only incremental generators (`IIncrementalGenerator`) | a fast IDE, caching |
| Do not keep symbols/syntax in generator models | it breaks caching |
| `nameof` instead of strings | safe renaming |
| Be careful with reflection in hot paths and untrusted code | bypassing encapsulation, security |

## Related materials

- Back: [basics](basics.en.md). Next: [expert theory](expert.en.md) — metadata internals, limitations, gotchas.
- [19 · Performance](../19-performance/overview.en.md) — cost and AOT.
- [14 · ASP.NET Core: Minimal API](../14-aspnet-core-minimal-api/overview.en.md) — AOT and generators.
- [13 · DI, configuration, logging](../13-di-config-logging/overview.en.md) — `[LoggerMessage]`.
- [11 · Modern C#](../11-modern-csharp/overview.en.md) — `partial` members.

## Sources

- [Incremental generators](https://github.com/dotnet/roslyn/blob/main/docs/features/incremental-generators.md)
- [Creating generators](https://learn.microsoft.com/dotnet/csharp/roslyn-sdk/source-generators-overview)
- [`System.Text.Json` source generation](https://learn.microsoft.com/dotnet/standard/serialization/system-text-json/source-generation)
- [`GeneratedRegexAttribute`](https://learn.microsoft.com/dotnet/standard/base-types/regular-expression-source-generators)
- [Expression trees: `Expression.Compile`](https://learn.microsoft.com/dotnet/api/system.linq.expressions.expression-1.compile)
