---
title: "Types and variables"
lang: en
track: csharp
kind: module
doc: overview
slug: 02-types-and-variables
part: 1
levels: [1, 2, 3, 4, 5, 6]
tags: [types, value-types, reference-types, type-conversion, strings, nullable-reference-types, boxing, floating-point]
versions: ".NET 11, C# 15"
status: verified
updated: 2026-11-26
---

# Types and variables

🌐 [Русский](overview.ru.md) · **English**

## In a nutshell

C# is a statically typed language: the type of every variable is known at compile time. All types are either value types (numbers, `bool`, `char`, structs), which are copied by value, or reference types (classes, `string`, arrays), which are shared by reference. This distinction determines how assignment, passing to methods, comparison and `null` handling behave.

## Key ideas

- **`var` is not dynamic typing:** the type is inferred once, at compile time.
- **A value type is copied** on assignment and when passed to a method, **a reference type is not**: both variables point to the same object.
- **Conversions:** implicit ones lose no data, explicit ones `(int)x` may lose data. `int` overflow wraps around silently by default; `checked` turns it into an exception.
- **Strings are immutable** and are compared by contents. There are interpolation `$"…"`, verbatim `@"…"` and raw literals `"""…"""`.
- **Null:** `int?` is a separate type, `Nullable<int>`. `string?` is an annotation for the compiler that turns on warnings about possible `null`. Operators `?.`, `??`, `??=`.
- **`double` is inexact** (`0.1 + 0.2 != 0.3`); use `decimal` for money.
- **Culture** affects casing, comparison and number formatting. Identifiers and keys use `Ordinal` and `InvariantCulture`.

## Minimal example

```csharp
public static (int Original, int Copy) MutateStructCopy()
{
    var original = new PointStruct { X = 1 };
    var copy = original;
    copy.X = 99;
    return (original.X, copy.X);
}

public static (int Original, int Copy) MutateClassCopy()
{
    var original = new PointClass { X = 1 };
    var copy = original;
    copy.X = 99;
    return (original.X, copy.X);
}
```

`MutateStructCopy()` → `(1, 99)`: the struct was copied. `MutateClassCopy()` → `(99, 99)`: both variables refer to one object.

## When to use and when not

- ✅ `decimal` — for money and exact decimal arithmetic; `double` — for measurements and computations.
- ✅ `int?` — when "no value" is a normal result.
- ✅ `<Nullable>enable</Nullable>` — in every new project.
- ✅ `TryParse` with an explicit culture — for parsing input.
- ❌ Mutable structs: copying makes changes "invisible".
- ❌ The `!` operator to suppress nullable warnings without a real guarantee.
- ❌ `==` for `double` values and for boxed values.

## Levels of this material

| Level | What's inside |
|---|---|
| [1. Basics](basics.en.md) | built-in types, value and reference types, conversions, strings, `int?` |
| [2. Advanced](advanced.en.md) | nullable reference types, boxing, `double` vs `decimal`, culture, common mistakes |
| [3. Expert](expert.en.md) | where values live, struct alignment, string interning, `Nullable<T>` from the inside; a reference for the topic, gotchas, interview questions |
| [4. Self-check](quiz.en.md) | a quiz and [exercises](exercises.en.md) with solutions |
| [5. Runnable examples](code/) | `code/Examples/` and `code/Solutions/` — the `Code.slnx` solution |
| [6. Tests](code/) | `code/Tests/` — `dotnet test` inside `code/` |


## Related materials

- Prerequisites: [01 · The .NET platform and dotnet CLI](../01-dotnet-platform/overview.en.md)
- Next: [03 · Control flow and methods](../03-control-flow-and-methods/overview.en.md)
- The same idea in other tracks: [Python](../../../python/overview.en.md) — dynamic typing, where every value is an object
- [System design: design process and estimation](../../../system-design/modules/01-design-process/overview.en.md)
