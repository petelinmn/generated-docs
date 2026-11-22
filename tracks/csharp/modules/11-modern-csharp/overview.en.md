---
title: "Modern C#"
lang: en
track: csharp
kind: module
doc: overview
slug: 11-modern-csharp
part: 2
levels: [1, 2, 3, 4]
tags: [pattern-matching, switch-expression, primary-constructor, collection-expressions, required-members, extension-members, union-types]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-22
---

# Modern C#

🌐 [Русский](overview.ru.md) · **English**

## In a nutshell

Modern C# (versions 9–15) removes boilerplate: patterns replace chains of `if` and casts, `switch` expressions return values, primary constructors and `required`/`init` shorten type definitions, and collection expressions unify the creation of arrays and lists. The newest features — extension members (C# 14) and union types (C# 15) — extend the type system. Old code keeps working, but in new code and documentation you will meet exactly these constructs.

## Key ideas

- **Patterns:** type, constant, relational, logical (`and`/`or`/`not`), property, positional, list; used in `is` and `switch`.
- **The `switch` expression** returns a value; arms go top to bottom; incomplete coverage is warning `CS8509` and a `SwitchExpressionException` at run time.
- **Primary constructors:** the type's parameters are available throughout the body; in a `class`/`struct` they are hidden fields, not properties (in a `record` they are properties).
- **Collection expressions:** `[1, 2, ..other]` creates an array, list, `Span<T>` or a custom type by the target type.
- **`required` + `init`** give immutable objects without bulky constructors; `field` removes manual fields in properties.
- **Extension members** add properties and static members to types; `?.=` assigns only when the object is not null.
- **Union types (C# 15):** a value of one of the listed types, with `switch` exhaustiveness checking.

## Minimal example

```csharp
static string Describe(int[] xs) => xs switch
{
    [] => "empty",
    [var only] => $"one: {only}",
    [var first, .., var last] => $"{first}..{last}",
};

int[] data = [1, 2, 3];
Console.WriteLine(Describe(data));   // 1..3
```

## When to use and when not

- ✅ Parsing data by shape: commands, messages, protocols — patterns.
- ✅ DTOs and configurations — `required` + `init` or records.
- ✅ Creating collections — collection expressions.
- ❌ Complex logic inside `when` and nested patterns: move it into a method.
- ❌ Primary constructors when you need public properties with logic — declare them explicitly.
- ❌ Union types in public APIs while the feature is new and still changing.

## Levels of this material

| Level | What's inside |
|---|---|
| [1. Basics](basics.en.md) | patterns, the `switch` expression, primary constructors, collection expressions, `required`/`init`, raw strings |
| [2. Advanced](advanced.en.md) | nested and list patterns, primary-constructor subtleties, collection expression types, `field`, `?.=`, extension members, loop labels, `with(...)` |
| [3. Expert](expert.en.md) | union types, how patterns compile, a C# 8–15 version reference, gotchas, 16 interview questions |
| [4. Self-check](quiz.en.md) | 12 questions and [6 exercises](exercises.en.md) with solutions |
| 5. Runnable examples | not generated yet — `/learn csharp 11-modern-csharp level 5` |
| 6. Tests | not generated yet — `/learn csharp 11-modern-csharp level 6` |

## Related materials

- Prerequisites: [05 · Interfaces, structs, records](../05-interfaces-structs-records/overview.en.md), [06 · Collections and generics](../06-collections-and-generics/overview.en.md)
- Next: [12a · Span and Memory](../12a-span-and-memory/overview.en.md), [07 · LINQ](../07-linq/overview.en.md)
- The same idea in other tracks: [Python](../../../python/overview.en.md) — `match`/`case`
