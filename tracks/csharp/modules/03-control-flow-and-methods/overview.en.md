---
title: "Control flow and methods"
lang: en
track: csharp
kind: module
doc: overview
slug: 03-control-flow-and-methods
part: 1
levels: [1, 2, 3, 4, 5, 6]
tags: [control-flow, switch-expression, pattern-matching, loops, methods, ref-out-in, overloading, local-functions, closures]
versions: ".NET 11, C# 15"
status: verified
updated: 2026-11-20
---

# Control flow and methods

🌐 [Русский](overview.ru.md) · **English**

## In a nutshell

Control flow statements decide which code runs and how many times: `if` and `switch` branches, `for`, `while`, `do` and `foreach` loops. Methods package code into named blocks with parameters. Modern C# adds switch expressions with patterns, the `ref`/`out`/`in` modifiers and local functions to the classics.

## Key ideas

- **`switch` without fall-through:** each section ends with `break`, `return`, `throw` or `goto`.
- **A switch expression** (`x switch { … }`) returns a value. Arms are checked in order; there are relational (`< 0`) and logical (`and`, `or`) patterns.
- **`foreach`** is the main way to iterate a collection. The collection must not be changed during iteration. Since C# 15, `break` and `continue` can name the label of an outer loop.
- **A lambda captures a variable, not a value:** `foreach` has its own variable per iteration, `for` has one for the whole loop.
- **`ref`, `out`, `in`** pass an argument by reference. `out` underlies the `TryXxx` pattern, `in` protects large structs from copying.
- **The overload** is chosen at compile time by the "better" type conversion.
- **Local functions** are helper methods inside a method. `static` forbids them from capturing variables.

## Minimal example

```csharp
// A switch expression over a tuple (C# 8+): the first matching arm wins.
public static string FizzBuzz(int n) => (n % 3, n % 5) switch
{
    (0, 0) => "FizzBuzz",
    (0, _) => "Fizz",
    (_, 0) => "Buzz",
    _ => n.ToString(CultureInfo.InvariantCulture),
};
```

`FizzBuzz(15)` → `FizzBuzz`, `FizzBuzz(9)` → `Fizz`, `FizzBuzz(7)` → `7`.

## When to use and when not

- ✅ A switch expression — when a result has to be chosen by value.
- ✅ `foreach` — for iterating; `for` — when an index or a backwards walk is needed.
- ✅ The Try pattern with `out` — when failure is a normal outcome (parsing input).
- ✅ `static` local functions — for helpers that do not need the method's context.
- ❌ `ref` for "speed" with small types: copying an `int` is cheaper than indirect access.
- ❌ Changing a collection inside a `foreach` over it.
- ❌ Suppressing the non-exhaustive `switch` warning instead of writing an explicit `_` arm.

## Levels of this material

| Level | What's inside |
|---|---|
| [1. Basics](basics.en.md) | `if`, `switch`, switch expressions, four kinds of loops, labels for `break`/`continue` (C# 15), methods, optional and named arguments, `params` |
| [2. Advanced](advanced.en.md) | `ref`, `out`, `in`, `params ReadOnlySpan<T>`, overload selection, local functions, loop and closure pitfalls |
| [3. Expert](expert.en.md) | `SwitchExpressionException`, ref returns, pattern-based `foreach`, recursion and the stack, default values at the call site; a reference for the topic, gotchas, interview questions |
| [4. Self-check](quiz.en.md) | a quiz and [exercises](exercises.en.md) with solutions |
| [5. Runnable examples](code/) | `code/Examples/` and `code/Solutions/` — the `Code.slnx` solution |
| [6. Tests](code/) | `code/Tests/` — `dotnet test` inside `code/` |


## Related materials

- Prerequisites: [02 · Types and variables](../02-types-and-variables/overview.en.md)
- Earlier: [01 · The .NET platform and dotnet CLI](../01-dotnet-platform/overview.en.md)
- Patterns and `switch` expressions: [11 · Modern C#](../11-modern-csharp/overview.en.md)
- The same idea in other tracks: [Python](../../../python/overview.en.md) — `if`/`match`, `for … in`, `*args`/`**kwargs`
- Recipes and libraries: [Spectre.Console](../../libraries/spectre-console/overview.en.md)
