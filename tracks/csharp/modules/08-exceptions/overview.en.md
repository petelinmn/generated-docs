---
title: "Exceptions"
lang: en
track: csharp
kind: module
doc: overview
slug: 08-exceptions
part: 1
levels: [1, 2, 3, 4]
tags: [exceptions, try-catch-finally, throw, custom-exceptions, exception-hierarchy, argument-validation, exception-filters, rethrow, inner-exception]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-22
---

# Exceptions

🌐 [Русский](overview.ru.md) · **English**

## In a nutshell

An exception is an object that describes an error, interrupts normal execution and is passed to the nearest handler up the stack. A `try` block protects code, `catch` handles exceptions of the needed type, and `finally` always runs. Exceptions are for contract violations and unforeseen failures; for expected situations the `TryXxx` pattern is better: a throw is expensive.

## Key ideas

- **`try`/`catch`/`finally`:** the first matching `catch` runs (specific types above general ones), `finally` — always.
- **The hierarchy:** all inherit from `Exception`; catch specific types, not `Exception`.
- **`throw;` preserves the stack, `throw ex;` loses it:** different IL instructions (`rethrow` and `throw`).
- **`when` filters** run before the `finally` of inner blocks (two-pass handling) and let you log without catching.
- **`finally` gotchas:** the return value is fixed before it, an exception in it replaces the original, `return` in it is forbidden.
- **`InnerException`:** when wrapping an exception, pass the cause.
- **Cost:** a throw is orders of magnitude more expensive than an ordinary return; for expected failures use `TryParse`, `TryGetValue`.

## Minimal example

```csharp
try
{
    Console.WriteLine("try");
    throw new InvalidOperationException("boom");
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"catch: {ex.Message}");
}
finally
{
    Console.WriteLine("finally");
}
// try, catch: boom, finally
```

## When to use and when not

- ✅ A method's contract is violated: a wrong argument, an invalid state.
- ✅ The operation cannot fulfil its purpose (the file is missing, the network is unavailable).
- ✅ `using` to release resources; `ArgumentNullException.ThrowIfNull` to validate arguments.
- ❌ Exceptions for ordinary control flow and expected results.
- ❌ An empty `catch`; `throw ex;`; exceptions from `finally` and `Dispose`.
- ❌ `catch (Exception)` where it is unknown what to do.

## Levels of this material

| Level | What's inside |
|---|---|
| [1. Basics](basics.en.md) | `try`/`catch`/`finally`, the hierarchy, common exceptions, properties, argument validation, `using`, custom exceptions, wrapping, when to throw |
| [2. Advanced](advanced.en.md) | `when` filters, `throw` and `throw ex`, wrapping and `InnerException`, `finally` gotchas, `AggregateException`, designing exceptions, performance, the Try pattern |
| [3. Expert](expert.en.md) | handling clauses in IL, two-pass handling, what an exception holds, uncatchable failures, reference, gotchas, 17 interview questions |
| [4. Self-check](quiz.en.md) | 11 questions and [5 exercises](exercises.en.md) with solutions |
| 5. Runnable examples | not generated yet — `/learn csharp 08-exceptions level 5` |
| 6. Tests | not generated yet — `/learn csharp 08-exceptions level 6` |

## Related materials

- Prerequisites: [03 · Control flow and methods](../03-control-flow-and-methods/overview.en.md), [04 · OOP: classes and objects](../04-oop-basics/overview.en.md)
- Releasing resources: [12 · Memory and GC](../12-memory-and-gc/overview.en.md)
- Exceptions in multithreaded code: [18 · Concurrency](../18-concurrency/overview.en.md)
- Exceptions in asynchronous code, `AggregateException`: [10 · Async/await](../10-async-await/overview.en.md)
- The same in other tracks: [Python](../../../python/overview.en.md) — `try`/`except`, `raise from`
- Recipes and libraries: [Polly](../../libraries/polly/overview.en.md)
