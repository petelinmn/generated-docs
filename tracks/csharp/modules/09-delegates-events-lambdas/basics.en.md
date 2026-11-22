---
title: "Delegates, events, lambdas: basics"
lang: en
track: csharp
kind: module
doc: basics
slug: 09-delegates-events-lambdas
part: 1
tags: [delegates, lambdas, events, func-action, multicast, closures, event-handler, callbacks]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-22
---

# Delegates, events, lambdas: basics

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [03 · Control flow and methods](../03-control-flow-and-methods/overview.en.md), [04 · OOP: classes and objects](../04-oop-basics/overview.en.md).

## Why it matters

Sometimes you need to pass a method an **action**, not data: "sort by this rule", "call this when the download finishes", "apply to every element". That requires references to methods. In C# three related features provide them:

- a **delegate** is a type that describes a method signature; a variable of such a type holds a reference to a method;
- a **lambda expression** is a short form of an anonymous method that you assign to a delegate;
- an **event** is a protected way to notify subscribers: an object "publishes" an event and others "subscribe" to it.

They underlie [LINQ](../07-linq/overview.en.md), handlers in UI and web frameworks, callbacks and asynchronous code.

## Delegates

A delegate is declared like a method with the `delegate` keyword:

```csharp
delegate int Calc(int a, int b);

static int Multiply(int a, int b) => a * b;

Calc mul = Multiply;                    // a reference to the method
Calc add = (a, b) => a + b;             // a lambda
Console.WriteLine(add(2, 3));           // 5
Console.WriteLine(mul(2, 3));           // 6
Console.WriteLine(add.Invoke(1, 1));    // 2 — the same as add(1, 1)
```

A delegate is a **reference type** whose value is a method (and, for an instance method, also the object it is called on). Only a method with a compatible signature (the same parameters and return type) can be assigned. Verified: for the delegate `mul` the `Method.Name` property is `Multiply` and `Target` is `null` because the method is static.

### Ready-made types: Func, Action, Predicate

You rarely need your own delegates: the library already has generic ones.

| Type | Signature | Example |
|---|---|---|
| `Action`, `Action<T1,…>` | returns `void` | `Action<string> print = s => Console.WriteLine(s);` |
| `Func<…, TResult>` | returns a value; the **last** type parameter is the result | `Func<int, int> sq = x => x * x;` |
| `Predicate<T>` | returns `bool` | `Predicate<int> pos = x => x > 0;` |
| `Comparison<T>` | compares two values | `Comparison<string> cmp = (a, b) => a.Length - b.Length;` |
| `EventHandler`, `EventHandler<TEventArgs>` | standard event handlers | see below |

## Lambda expressions

A lambda is an anonymous function: `parameters => body`. The body is an expression or a block:

```csharp
Func<int, int> sq = x => x * x;                       // expression body
Func<int, int, int> max = (a, b) => a > b ? a : b;    // several parameters
Func<int, string> describe = n =>                     // statement body
{
    if (n < 0) return "negative";
    return n == 0 ? "zero" : "positive";
};
Action hello = () => Console.WriteLine("hi");         // no parameters
```

Parameter types are usually inferred from the delegate. Since C# 10 a lambda has a "natural" type, so you can write `var`:

```csharp
var natural = (int x) => x + 1;     // Func<int, int>
```

Verified: the type of `natural` is `Func<int,int>`. Since C# 12 default parameters are allowed: `(int x, int y = 10) => x + y` — the call `withDefault(1)` gave `11`.

## Delegates as parameters

The main use is to pass behavior to a method:

```csharp
static List<int> Filter(List<int> source, Predicate<int> keep)
{
    var result = new List<int>();
    foreach (var x in source)
        if (keep(x)) result.Add(x);
    return result;
}

var evens = Filter([1, 2, 3, 4], x => x % 2 == 0);    // 2, 4
```

This is how `List<T>.Sort(Comparison<T>)`, `RemoveAll(Predicate<T>)` and all the [LINQ](../07-linq/basics.en.md) operators (`Where(x => …)`, `Select(x => …)`) work.

## Multicast delegates

A delegate can hold **several** methods. The operators `+=` and `-=` add and remove them; on a call they run in order:

```csharp
Action act = () => Console.Write("A");
act += () => Console.Write("B");
act += () => Console.Write("C");
act();                                  // ABC
```

Verified: the log showed `A B C`. If a multicast delegate has a return value, the result is the value of the **last** method (`3` for `1`, `2`, `3`). Details in [advanced](advanced.en.md#multicast-delegates).

## Events

An **event** is a delegate wrapped so that from outside you can only subscribe (`+=`) and unsubscribe (`-=`), while only the **publisher class itself** can raise it.

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

If there are no subscribers, `Clicked` is `null`: hence the call through `?.Invoke()` (verified: a call with no subscribers does not throw, while a direct call of a `null` delegate throws `NullReferenceException`).

From outside you cannot raise or assign an event: `c.Clicked();` and `c.Clicked = null;` are compile error CS0070. That is what distinguishes an event from an ordinary public delegate field.

### The standard event pattern

.NET has a unified form: a handler receives the **sender** and the **event data**.

```csharp
public class ValueEventArgs(int value) : EventArgs
{
    public int Value { get; } = value;
}

public class Publisher
{
    public event EventHandler<ValueEventArgs>? Changed;

    public void Set(int v) => Changed?.Invoke(this, new ValueEventArgs(v));
}

var pub = new Publisher();
pub.Changed += (sender, e) => Console.WriteLine($"changed: {e.Value}");
pub.Set(5);                                        // changed: 5
```

`EventHandler<TEventArgs>` has the signature `void (object? sender, TEventArgs e)`. For an event without data use `EventHandler`.

## Closures

A lambda can use **local variables** of the surrounding method. This is called a closure: the lambda remembers not the value but **the variable itself**:

```csharp
int counter = 0;
Action inc = () => counter++;
inc();
inc();
Console.WriteLine(counter);     // 2 — the lambda changed the outer variable
```

That is why lambdas created in a `for` loop see the final value of the loop variable, and LINQ queries read variables at execution time — details in [07 · LINQ](../07-linq/advanced.en.md#closures-the-variable-is-read-at-execution-time) and in [advanced](advanced.en.md#closures-and-their-cost).

## Beginner mistakes

- ❌ Calling a delegate or event without a `null` check → ✅ `handler?.Invoke(...)`.
- ❌ A public delegate field instead of `event` → ✅ `event`: from outside it cannot be raised or nulled.
- ❌ Subscribing to an event of a long-lived object and never unsubscribing → ✅ a memory leak ([12 · Memory and GC](../12-memory-and-gc/advanced.en.md#memory-leaks-in-managed-code)): unsubscribe with `-=`.
- ❌ Trying to unsubscribe with another copy of the same lambda → ✅ store the delegate in a variable.
- ❌ Expecting a closure to store the value → ✅ it stores the variable.
- ❌ Writing your own delegate type when `Func`/`Action` fits → ✅ the ready-made types.

## Related materials

- Next: [advanced](advanced.en.md) — multicast delegates, closures and allocations, events in detail, thread safety
- LINQ built on delegates: [07 · LINQ](../07-linq/overview.en.md)
- Methods and parameters: [03 · Control flow and methods](../03-control-flow-and-methods/overview.en.md)
- Exceptions in handlers: [08 · Exceptions](../08-exceptions/overview.en.md)
- The same in other tracks: [Python](../../../python/overview.en.md) — functions as objects, `lambda`, closures

## Sources

- [Delegates](https://learn.microsoft.com/dotnet/csharp/programming-guide/delegates/)
- [Lambda expressions](https://learn.microsoft.com/dotnet/csharp/language-reference/operators/lambda-expressions)
- [Events](https://learn.microsoft.com/dotnet/csharp/programming-guide/events/)
- [Func](https://learn.microsoft.com/dotnet/api/system.func-2)
- [Action](https://learn.microsoft.com/dotnet/api/system.action)
- [The standard .NET event pattern](https://learn.microsoft.com/dotnet/csharp/event-pattern)
