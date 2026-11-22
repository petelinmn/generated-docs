---
title: "Exercises: Reflection and source generators"
lang: en
track: csharp
kind: module
doc: exercises
slug: 20-reflection-and-source-generators
part: 3
tags: [reflection, attributes, expression-trees, plugins, source-generators]
status: draft
updated: 2026-11-22
---

# Exercises: Reflection and source generators

🌐 [Русский](exercises.ru.md) · **English**

> Solve it yourself before opening the hints. The solutions were verified by running (Release). Exercise 5 needs two projects: a generator (`netstandard2.0`, the package `Microsoft.CodeAnalysis.CSharp` 4.14.0) and an application. There are no runnable versions in `code/` yet (level 5 does not exist).

## 1. 🟢 Type properties

**Task:** print the names and types of the public properties of the class `Person(string name, int age)`.

**Expected result:**

```text
Name:String
Age:Int32
```

<details><summary>Hint</summary>

`typeof(Person).GetProperties()` and `p.Name`, `p.PropertyType.Name`.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 1: list the public properties of a type.
foreach (var p in typeof(Person).GetProperties())
    Console.WriteLine($"{p.Name}:{p.PropertyType.Name}");
```

Verified: the output matches.

</details>

## 2. 🟢 Captions from attributes

**Task:** declare an attribute `[Label("text")]` for properties; for each property of `Person` print the caption, and if there is none — the property name.

**Expected result:**

```text
Full name
Age
```

<details><summary>Hint</summary>

`[AttributeUsage(AttributeTargets.Property)]`, `p.GetCustomAttribute<LabelAttribute>()?.Text ?? p.Name`.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 2: read a custom attribute with a fallback.
[AttributeUsage(AttributeTargets.Property)]
public class LabelAttribute(string text) : Attribute { public string Text { get; } = text; }

public class Person(string name, int age)
{
    [Label("Full name")] public string Name { get; set; } = name;
    public int Age { get; set; } = age;
}

foreach (var p in typeof(Person).GetProperties())
    Console.WriteLine(p.GetCustomAttribute<LabelAttribute>()?.Text ?? p.Name);
```

Verified: `Full name`, `Age`.

</details>

## 3. 🟡 Copying properties: reflection and compiled expressions

**Task:** write `Mapper.Copy<TFrom, TTo>(from, to)` that copies same-named writable properties of one type; then `CopyFast` with a cache of a delegate built from an expression tree. Compare allocations.

**Expected result:**

```text
Name and Age are copied, Extra stays null
reflective variant: 64 B; compiled: 0 B
```

<details><summary>Hint</summary>

For the compiled variant you need a static class `Cache<TFrom, TTo>` with an `Action<TFrom, TTo>` field built via `Expression.Assign(Expression.Property(t, tp), Expression.Property(f, sp))` and `Expression.Block`.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 3: reflection copy versus a compiled, cached copier.
public static class Mapper
{
    public static void Copy<TFrom, TTo>(TFrom from, TTo to)
    {
        foreach (var sp in typeof(TFrom).GetProperties())
        {
            var tp = typeof(TTo).GetProperty(sp.Name);
            if (tp is { CanWrite: true } && tp.PropertyType == sp.PropertyType) tp.SetValue(to, sp.GetValue(from));
        }
    }

    private static class Cache<TFrom, TTo>
    {
        public static readonly Action<TFrom, TTo> Copier = Build();
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

Verified: both methods copied `Name` and `Age`; allocations — `64 B` and `0 B`. In Native AOT `Compile()` is unavailable — a generator is needed there.

</details>

## 4. 🟡 Loading plugins

**Task:** write `PluginLoader.Load<T>(Assembly)`: it finds non-abstract classes with a public parameterless constructor that implement `T` and creates them, ordered by type name.

**Expected result:** for `Alpha`, `Beta` and the abstract `Gamma` — `alpha`, `beta`.

<details><summary>Hint</summary>

`asm.GetTypes().Where(t => typeof(T).IsAssignableFrom(t) && t is { IsClass: true, IsAbstract: false } && t.GetConstructor(Type.EmptyTypes) is not null)`.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 4: discover and instantiate implementations.
public static class PluginLoader
{
    public static IEnumerable<T> Load<T>(Assembly asm) =>
        asm.GetTypes()
           .Where(t => typeof(T).IsAssignableFrom(t) && t is { IsClass: true, IsAbstract: false } && t.GetConstructor(Type.EmptyTypes) is not null)
           .OrderBy(t => t.Name)
           .Select(t => (T)Activator.CreateInstance(t)!);
}
```

Verified: `alpha`, `beta`. For types with dependencies create them through DI, not `Activator`.

</details>

## 5. 🔴 Your own source generator

**Task:** write an `[AutoToString]` generator: for a `partial` class with this attribute it creates a `ToString()` that prints the class name and the values of the public properties, for example `Person { Name = Ann, Age = 30 }`. The attribute must be generated too.

**Expected result:**

```text
Person { Name = Ann, Age = 30 }
Point { X = 1, Y = 2 }     (a class in the namespace Shapes)
```

<details><summary>Hint</summary>

`IIncrementalGenerator`; `RegisterPostInitializationOutput` for the attribute; `ForAttributeWithMetadataName("Gen.AutoToStringAttribute", …)`; double the braces in the generated interpolated string (`{{`/`}}`). The generator project is `netstandard2.0`, in the application — `<ProjectReference … OutputItemType="Analyzer" ReferenceOutputAssembly="false" />`.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 5: an incremental generator that adds ToString to marked partial classes.
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
        context.RegisterPostInitializationOutput(ctx => ctx.AddSource("AutoToStringAttribute.g.cs", SourceText.From(AttributeSource, Encoding.UTF8)));

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

Verified: an application with `[AutoToString] public partial class Person` prints `Person { Name = Ann, Age = 30 }`, and `Shapes.Point` — `Point { X = 1, Y = 2 }`. The private property did not make it into the output.

</details>
