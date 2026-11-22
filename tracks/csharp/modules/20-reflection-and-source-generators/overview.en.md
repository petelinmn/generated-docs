---
title: "Reflection and source generators"
lang: en
track: csharp
kind: module
doc: overview
slug: 20-reflection-and-source-generators
part: 3
levels: [1, 2, 3, 4]
tags: [reflection, attributes, expression-trees, source-generators, trimming, native-aot]
versions: ".NET 11, C# 15; Microsoft.CodeAnalysis.CSharp 4.14.0"
status: draft
updated: 2026-11-22
---

# Reflection and source generators

🌐 [Русский](overview.ru.md) · **English**

## In a nutshell

Reflection reads type metadata at run time: find members, create an object, call a method by name, read attributes. Expression trees represent lambdas as data that can be examined and compiled. Source generators do the same ahead of time — at compile time they add source files, so they need no reflection at run time and are compatible with Native AOT. Reflection is flexible but slower and incompatible with trimming without annotations; generators are fast but harder to develop.

## Key ideas

- **`Type` and `BindingFlags`:** `GetProperties`, `GetMethod`, `Activator.CreateInstance`; names are case-sensitive, private members only with `NonPublic`.
- **Attributes** are metadata; read with `GetCustomAttribute<T>`; they do nothing themselves.
- **Cost:** `MethodInfo.Invoke` allocates memory (72 B in the verified measurement), a cached delegate — 0; cache `MethodInfo` and delegates.
- **Expression trees:** `Expression<Func<…>>` is data; `Compile()` gives a delegate (unavailable in Native AOT).
- **Source generators:** `IIncrementalGenerator` in a `netstandard2.0` project, attached as an analyzer; they only add files; the class must be `partial`.
- **Ready generators:** `[LoggerMessage]`, `[GeneratedRegex]`, `JsonSerializerContext`.
- **Trimming and AOT:** reflection by strings breaks trimming — annotations (`DynamicallyAccessedMembers`) or generators.

## Minimal example

```csharp
[AttributeUsage(AttributeTargets.Property)]
public class LabelAttribute(string text) : Attribute { public string Text { get; } = text; }

public class Person(string name, int age)
{
    [Label("Full name")] public string Name { get; set; } = name;
    public int Age { get; set; } = age;
}

foreach (var p in typeof(Person).GetProperties())
    Console.WriteLine(p.GetCustomAttribute<LabelAttribute>()?.Text ?? p.Name);
// Full name
// Age
```

## When to use and when not

- ✅ Plugins, serializers, ORMs, DI, test frameworks — reflection over metadata.
- ✅ A generator: when the set of types is known at compile time and you need speed and AOT.
- ✅ Expression trees: analyzing lambdas (a property name, SQL) and fast dynamic delegates.
- ❌ Reflection in a hot path without a cache.
- ❌ Accessing private members in ordinary code.
- ❌ Reflection by string names in an application with trimming/AOT without annotations.

## Levels of this material

| Level | What's inside |
|---|---|
| [1. Basics](basics.en.md) | `Type`, creating and calling, private members, attributes, generics, type discovery, expression trees, the idea of generators |
| [2. Advanced](advanced.en.md) | cost and caching, compiling expressions, plugins, `IIncrementalGenerator` (a working example), ready generators, debugging |
| [3. Expert](expert.en.md) | metadata, lookup rules, how `Invoke` executes, trimming and AOT annotations, assembly loading, generator internals, 13 interview questions |
| [4. Self-check](quiz.en.md) | 11 questions and [5 exercises](exercises.en.md) with solutions |
| 5. Runnable examples | not generated yet — `/learn csharp 20-reflection-and-source-generators level 5` |
| 6. Tests | not generated yet — `/learn csharp 20-reflection-and-source-generators level 6` |

## Related materials

- Prerequisites: [09 · Delegates, events, lambdas](../09-delegates-events-lambdas/overview.en.md), [06 · Collections and generics](../06-collections-and-generics/overview.en.md)
- Nearby: [19 · Performance](../19-performance/overview.en.md), [07 · LINQ](../07-linq/overview.en.md)
- Next: [21 · Application architecture](../21-architecture/overview.en.md)
- `[LoggerMessage]` and DI: [13 · DI, configuration, logging](../13-di-config-logging/overview.en.md)
- The same idea in other tracks: [Python](../../../python/overview.en.md) — introspection
- Recipes and libraries: [Dapper](../../libraries/dapper/overview.en.md), [FluentValidation](../../libraries/fluentvalidation/overview.en.md), [Mapperly](../../libraries/mapperly/overview.en.md), [NSubstitute](../../libraries/nsubstitute/overview.en.md), [Refit](../../libraries/refit/overview.en.md)
