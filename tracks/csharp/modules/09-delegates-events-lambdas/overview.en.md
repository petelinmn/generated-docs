---
title: "Delegates, events, lambdas"
lang: en
track: csharp
kind: module
doc: overview
slug: 09-delegates-events-lambdas
part: 1
levels: [1, 2, 3]
tags: [delegates, lambdas, events, func-action, multicast, closures, event-handler, callbacks]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-22
---

# Delegates, events, lambdas

🌐 [Русский](overview.ru.md) · **English**

## In a nutshell

A delegate is a type-safe reference to a method: it can be stored in a variable, passed to other methods and called later. A lambda expression is a short form of an anonymous method that you assign to a delegate (`Func`, `Action`, `Predicate`). An event is a delegate protected so that from outside you can only subscribe and unsubscribe, while the publisher itself raises it. LINQ, handlers in UI and web frameworks, callbacks and asynchronous code are built on these three features.

## Key ideas

- **A delegate is a sealed class** (derived from `MulticastDelegate`) with a target and a method; the ready-made types are `Func`, `Action`, `Predicate`, `EventHandler<T>`.
- **Multicast:** `+=`/`-=` create a new delegate; the result is the last method's; an exception interrupts the chain.
- **Lambdas:** an expression or a block, a natural type (`var`), a `static` lambda does not capture variables.
- **A closure captures the variable, not the value:** the compiler moves it into a field of a hidden class, one per scope — it may keep extra objects alive.
- **Cost:** a non-capturing lambda and a static method group — 0 bytes (cache), a capturing one — 80 bytes, an instance method — 56 bytes per conversion.
- **Events:** a delegate field plus `add`/`remove`; raise only through `?.Invoke`; a subscriber is held by the publisher — unsubscribe.
- **You cannot unsubscribe with an "identical" lambda:** delegates are equal by method and target.

## Minimal example

```csharp
public class Counter
{
    public event Action? Clicked;                  // the event

    public void Click() => Clicked?.Invoke();      // only the owner raises it
}

var c = new Counter();
c.Clicked += () => Console.WriteLine("clicked");   // subscribe
c.Click();                                         // clicked
```

## When to use and when not

- ✅ Passing behavior to a method: sorting, filtering, callbacks, LINQ.
- ✅ Notifications between objects — `event` and `EventHandler<T>`.
- ✅ `static` lambdas and delegates created once in hot code.
- ❌ A public delegate field instead of `event`.
- ❌ Subscribing to a long-lived object without unsubscribing.
- ❌ Exceptions from handlers of multicast delegates: they interrupt the chain.

## Levels of this material

| Level | What's inside |
|---|---|
| [1. Basics](basics.en.md) | delegates, `Func`/`Action`/`Predicate`, lambdas, delegates as parameters, multicast, events and the `EventHandler` pattern, closures |
| [2. Advanced](advanced.en.md) | multicast delegates, the natural type and `static` lambdas, closures and memory allocations, events in detail, thread safety, variance |
| [3. Expert](expert.en.md) | how a delegate works, hidden classes and the lambda cache, events as a field and methods, function pointers, reference, gotchas, 16 interview questions |
| 4. Self-check | not generated yet — `/learn csharp 09-delegates-events-lambdas level 4` |
| 5. Runnable examples | not generated yet — `/learn csharp 09-delegates-events-lambdas level 5` |
| 6. Tests | not generated yet — `/learn csharp 09-delegates-events-lambdas level 6` |

## Related materials

- Prerequisites: [03 · Control flow and methods](../03-control-flow-and-methods/overview.en.md), [04 · OOP: classes and objects](../04-oop-basics/overview.en.md)
- LINQ on delegates: [07 · LINQ](../07-linq/overview.en.md)
- Exceptions in handlers: [08 · Exceptions](../08-exceptions/overview.en.md)
- Leaks through events: [12 · Memory and GC](../12-memory-and-gc/overview.en.md)
- Asynchronous lambdas and `Task`: [10 · Async/await](../10-async-await/overview.en.md)
- Default lambda parameters, `static` lambdas: [11 · Modern C#](../11-modern-csharp/overview.en.md)
- The same in other tracks: [Python](../../../python/overview.en.md) — functions as objects, closures
