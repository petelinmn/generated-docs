---
title: "Reflection and source generators: basics"
lang: en
track: csharp
kind: module
doc: basics
slug: 20-reflection-and-source-generators
part: 3
tags: [reflection, attributes, expression-trees, source-generators, metadata, activator]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-22
---

# Reflection and source generators: basics

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [04 · OOP: classes and objects](../04-oop-basics/overview.en.md), [06 · Collections and generics](../06-collections-and-generics/overview.en.md), [09 · Delegates, events, lambdas](../09-delegates-events-lambdas/overview.en.md). **Time:** ~1 h.

## Why it matters

Normally types and members are known at compile time. But sometimes you need to work with them **dynamically**: find all plugin classes in an assembly, map the properties of two objects, read attributes for validation or serialization. For this there is **reflection** — reading type metadata at run time and accessing members by name. The price of the flexibility is speed and the loss of compiler checks; so wherever possible part of the work is moved to compile time — **source generators**.

## Type information

Metadata is obtained through a `Type` object: `typeof(Person)`, `obj.GetType()` or `Type.GetType("name")`. Through it you reach properties, methods, constructors, fields.

```csharp
public class Person(string name, int age)
{
    public string Name { get; set; } = name;
    public int Age { get; } = age;
    public string Greet(string greeting) => $"{greeting}, {Name}";
}

var t = typeof(Person);
foreach (var p in t.GetProperties())
    Console.WriteLine($"prop {p.Name}: {p.PropertyType.Name} canWrite={p.CanWrite}");
// prop Name: String canWrite=True
// prop Age: Int32 canWrite=False
```

Verified: `GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)` returned `get_Name`, `set_Name`, `get_Age`, `Greet` — a property compiles into a pair of methods. The constructor parameters are `name`, `age` (`GetConstructors()[0].GetParameters()`).

## Creating objects and calling members

```csharp
var obj = Activator.CreateInstance(t, "Ann", 30)!;                  // new Person("Ann", 30)
t.GetProperty("Name")!.SetValue(obj, "Bob");
Console.WriteLine(t.GetProperty("Name")!.GetValue(obj));            // Bob
Console.WriteLine(t.GetMethod("Greet")!.Invoke(obj, ["Hi"]));       // Hi, Bob
```

If the called method throws an exception, reflection wraps it in a `TargetInvocationException`; the real exception is in `InnerException` (verified: `TargetInvocationException -> inner InvalidOperationException`).

## Private members

The `BindingFlags` flags specify which members to look for. `NonPublic` opens private fields and methods — this bypasses encapsulation, so it suits tests and infrastructure but not ordinary code:

```csharp
var secret = t.GetField("_secret", BindingFlags.NonPublic | BindingFlags.Instance)!;
Console.WriteLine(secret.GetValue(obj));    // 7
secret.SetValue(obj, 99);                    // changes private state
```

Verified: the private field was read (`7`), changed (`99`), and a private method was called (`hidden`).

## Attributes

An attribute is metadata attached to a type or member. A custom attribute is a class derived from `Attribute`:

```csharp
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
public class LabelAttribute(string text) : Attribute { public string Text { get; } = text; }

[Label("A person")]
public class Person(string name, int age)
{
    [Label("The name")] public string Name { get; set; } = name;
    public int Age { get; } = age;
}

foreach (var p in typeof(Person).GetProperties())
    Console.WriteLine($"{p.Name}: {p.GetCustomAttribute<LabelAttribute>()?.Text ?? "(no label)"}");
// Name: The name
// Age: (no label)
```

Attributes are read by validators, serializers, ORMs, DI frameworks: the code decides what to do from the metadata. An attribute by itself executes nothing.

## Generic types and type discovery

```csharp
var listType = typeof(List<>).MakeGenericType(typeof(int));        // List<int> built at run time
var list = (System.Collections.IList)Activator.CreateInstance(listType)!;
list.Add(5);                                                        // count = 1
```

Finding implementations of an interface in an assembly (plugins):

```csharp
var plugins = Assembly.GetExecutingAssembly().GetTypes()
    .Where(x => typeof(IPlugin).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract);
```

Verified: `PluginA`, `PluginB` were found; the abstract `PluginBase` was filtered out.

## Expression trees

A lambda assigned to the type `Expression<Func<…>>` turns not into code but into a **data structure** — a tree that can be examined and compiled:

```csharp
Expression<Func<int, int>> expr = x => x * 2 + 1;
Console.WriteLine(expr);                          // x => ((x * 2) + 1)
Console.WriteLine(expr.Body.NodeType);            // Add
Console.WriteLine(expr.Compile()(5));             // 11
```

A tree lets you, for example, extract a property name: `Expression<Func<Person, string>> sel = p => p.Name;` → `((MemberExpression)sel.Body).Member.Name` returns `Name` (verified). This is how LINQ providers (EF Core) translate lambdas into SQL ([07 · LINQ](../07-linq/overview.en.md)).

## Source generators — in brief

A **source generator** is a compiler component that analyzes code during the build and **adds new source files**. Unlike reflection, everything happens at compile time: there is no run-time cost, the code is checked by the compiler and suits Native AOT. You have already met ready generators: `[LoggerMessage]`, `[GeneratedRegex]`, `JsonSerializerContext`. More in the [advanced theory](advanced.en.md#source-generators).

## Beginner mistakes

- ❌ Reflection in a hot path ([19 · Performance](../19-performance/overview.en.md)) → ✅ cache `MethodInfo`/delegates or use a generator.
- ❌ String member names (`"Name"`) → ✅ `nameof(...)` or an expression tree: renaming will not silently break the code.
- ❌ Accessing private members in production code → ✅ only in tests and infrastructure.
- ❌ Forgetting `TargetInvocationException` → ✅ look at `InnerException`.
- ❌ Searching for types across all assemblies needlessly → ✅ limit the assembly and cache the result.
- ❌ Expecting an attribute to do something by itself → ✅ an attribute is data; the code that reads it supplies the logic.

## Related materials

- Next: [advanced theory](advanced.en.md) — cost, caching, compiling expressions, generators.
- [09 · Delegates, events, lambdas](../09-delegates-events-lambdas/overview.en.md) — delegates as a fast alternative to `Invoke`.
- [07 · LINQ](../07-linq/overview.en.md) — `Expression<T>` and `IQueryable<T>`.
- [13 · DI, configuration, logging](../13-di-config-logging/overview.en.md) — DI uses reflection for constructors.
- [Python](../../../python/overview.en.md) — introspection and decorators.

## Sources

- [Reflection in .NET](https://learn.microsoft.com/dotnet/fundamentals/reflection/overview)
- [Attributes](https://learn.microsoft.com/dotnet/csharp/advanced-topics/reflection-and-attributes/)
- [Expression trees](https://learn.microsoft.com/dotnet/csharp/advanced-topics/expression-trees/)
- [Source generators](https://learn.microsoft.com/dotnet/csharp/roslyn-sdk/source-generators-overview)
