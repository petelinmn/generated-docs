---
title: "Control flow and methods: expert"
lang: en
track: csharp
kind: module
doc: expert
slug: 03-control-flow-and-methods
part: 1
tags: [control-flow, switch-expression, pattern-matching, loops, methods, ref-out-in, overloading, local-functions, closures]
versions: ".NET 11, C# 15"
status: verified
updated: 2026-11-20
---

# Control flow and methods: expert

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · **3. Expert** · [4. Quiz](quiz.en.md) · [exercises](exercises.en.md) · [5–6. Code and tests](code/)

## Exhaustive switch and SwitchExpressionException

An enum (`enum`) is a set of named constants on top of an integer type. An `enum` variable can hold **any** value of that type, not only the declared ones:

```csharp
// An enum variable can hold any value of its underlying type: (TrafficLight)42 is legal.
// The compiler warns with CS8524 and emits a throw of SwitchExpressionException for unmatched input.
#pragma warning disable CS8524
public static string Action(TrafficLight light) => light switch
{
    TrafficLight.Red => "stop",
    TrafficLight.Yellow => "wait",
    TrafficLight.Green => "go",
};
#pragma warning restore CS8524

// An explicit discard arm documents what happens with unexpected values.
public static string SafeAction(TrafficLight light) => light switch
{
    TrafficLight.Red => "stop",
    TrafficLight.Yellow => "wait",
    TrafficLight.Green => "go",
    _ => throw new ArgumentOutOfRangeException(nameof(light), light, "Unknown traffic light"),
};
```

- If a switch expression does not cover all values, the compiler warns (CS8509, and CS8524 for unnamed `enum` values) and generates an arm that throws `System.Runtime.CompilerServices.SwitchExpressionException`.
- `Action((TrafficLight)42)` throws `SwitchExpressionException`, while `SafeAction((TrafficLight)42)` throws a clear `ArgumentOutOfRangeException`.
- Takeaway: an explicit `_ => throw …` arm is better than a suppressed warning. The `#pragma` here is only for the demonstration.

Code: [Ex08_SwitchExhaustiveness.cs](code/Examples/Ex08_SwitchExhaustiveness.cs).

C# 15 adds two kinds of types for which a `switch` can be exhaustive without a `_` arm:

- **union types** — a value is one of several case types, and the compiler checks that a switch expression covers all of them;
- **closed hierarchies** — a class with the `closed` modifier can be derived from only within its own assembly, so the compiler knows all derived classes, and a `switch` over such a hierarchy needs no default arm.

Both are covered in module 11, "Modern C#".

## Ref returns

A method can return not a value but a **reference to a storage location**:

```csharp
// Returns a reference to the array element itself, so the caller can change it in place.
public static ref int FindFirst(int[] values, int target)
{
    for (int i = 0; i < values.Length; i++)
    {
        if (values[i] == target)
        {
            return ref values[i];
        }
    }
    throw new InvalidOperationException($"{target} not found");
}
```

The calling code: `ref int slot = ref RefReturns.FindFirst(values, 2); slot = 20;` — and the array `[1, 2, 3]` becomes `[1, 20, 3]`. A reference can be returned only to something that outlives the method: an array element, an object field, a `ref` parameter. Not to a local variable — the compiler will not allow it. `Span<T>` and high-performance APIs are built on this mechanism (module 12a "Span and Memory", not created yet). Code: [Ex09_RefReturns.cs](code/Examples/Ex09_RefReturns.cs).

## How foreach finds an enumerator

`foreach` does not require the `IEnumerable` interface. The compiler looks for a **pattern**: a public `GetEnumerator()` method whose result has `MoveNext()` and a `Current` property:

```csharp
// No interfaces at all: foreach finds GetEnumerator, MoveNext and Current by name.
// The enumerator is a struct, so the loop allocates nothing on the heap.
public readonly struct EvenNumbers
{
    private readonly int _max;

    public EvenNumbers(int max) => _max = max;

    public Enumerator GetEnumerator() => new(_max);

    public struct Enumerator
    {
        private readonly int _max;
        private int _current;

        public Enumerator(int max)
        {
            _max = max;
            _current = -2;
        }

        public readonly int Current => _current;

        public bool MoveNext()
        {
            _current += 2;
            return _current <= _max;
        }
    }
}
```

`foreach (int n in new EvenNumbers(7))` yields `0, 2, 4, 6`. Why this matters:

- `List<T>.Enumerator` is also a struct, so `foreach` over a `List<T>` creates no heap object. If the list is cast to `IEnumerable<T>`, the enumerator gets boxed.
- The compiler turns `foreach` over an array into a plain index loop.
- `await` (`GetAwaiter`) and deconstruction (`Deconstruct`) work by pattern in the same way.

Code: [Ex10_DuckTypedForeach.cs](code/Examples/Ex10_DuckTypedForeach.cs). Applying the pattern yourself is [exercise 4](exercises.en.md).

## Recursion and the stack

- Every method call takes a stack frame. Recursion that is too deep leads to `StackOverflowException`, and it **cannot be caught**: the process terminates.
- C# does not guarantee tail-call optimization, so deep recursion is rewritten as a loop or uses an explicit stack (`Stack<T>`).
- Memoization (as in `Fibonacci`) turns exponential recursion into linear, but does not reduce the stack depth.

## The trap: default values are compiled into the call site

The value of an optional parameter is substituted **into the code of the calling assembly** when that assembly is compiled. If library v2 changes `greeting = "Hello"` to `greeting = "Hi"` and the app is not recompiled, it keeps passing `"Hello"`. That is why public library APIs add a new overload instead of changing a default value.

`const` constants behave the same way: their value is also substituted into the calling assembly's code — see [module 02 expert theory](../02-types-and-variables/expert.en.md).

## Reference

### Jump statements

| Statement | What it does |
|---|---|
| `break` | leaves the nearest loop or `switch` |
| `break label;` (C# 15) | leaves the loop or `switch` with that label |
| `continue` / `continue label;` (C# 15) | moves to the next iteration of the nearest or labeled loop |
| `return` | leaves the method |
| `throw` | throws an exception; since C# 7 it can be an expression: `x ?? throw …`, a switch expression arm |
| `goto label;` | jumps to a label in the same method |
| `goto case value;` / `goto default;` | jumps to another `switch` section |
| `yield return` / `yield break` | in iterators (covered together with LINQ, module 07) |

An example of `goto case`: `switch (1)` with sections `case 1: … goto case 2;`, `case 2: … goto default;` takes the path `one>two>default`.

### The switch statement in full

- Labels are constants or patterns: `case int n when n > 0:`, `case null:`, `case string { Length: 0 }:`. A `when` guard refines the pattern.
- A section with code cannot fall through into the next one: error CS0163. Several labels in a row on one section are allowed.
- `default` can be placed anywhere; logically it runs last.
- A `switch` on a string compares ordinally and case-sensitively.

### The switch expression in full

- Arms `pattern when condition => expression` are checked in order.
- An arm already covered by previous ones is error CS8510 (e.g. `> 10` after `> 0`).
- A non-exhaustive switch gives warning CS8509 (CS8524 for unnamed `enum` values) and `SwitchExpressionException` at run time.
- `throw` can be used as an arm's result.

### Loops in full

| Form | Note |
|---|---|
| `for (init; condition; step)` | all three parts are optional: `for (;;)` is an infinite loop |
| `while (condition)` / `do … while (condition);` | check before / after the body |
| `foreach (var x in collection)` | by the `GetEnumerator` pattern; over `null` it throws `NullReferenceException` |
| `foreach (ref var x in span)` | changing `Span<T>` elements in place: `[1, 2, 3]` → `[10, 20, 30]` after `item *= 10` |
| `await foreach` | asynchronous streams (module 10) |
| `label: for …` + `break label;` | leaving nested loops (C# 15) |

### Parameters in full

| Declaration | Passing | Note |
|---|---|---|
| `T x` | by value | a copy of the value or of the reference |
| `ref T x` | by reference | the argument is initialized, the method can change it |
| `out T x` | by reference | the method must assign it |
| `in T x` | by reference, read-only | for large structs |
| `ref readonly T x` (C# 12) | by reference, read-only | the caller passes `ref` or `in`: `ReadOnlyRef(ref number)` and `ReadOnlyRef(in number)` give the same result |
| `params T[] x`, `params ReadOnlySpan<T> x` (C# 13) | a variable number of arguments | |
| `T x = value` | optional | the value is substituted at the call site |
| `scoped ref T x` (C# 11) | limits the lifetime of the reference | for `ref struct` and `Span<T>` (module 12a) |
| `this T x` | the first parameter of an extension method | module 11 |

Overloads that differ only in `ref` and `out` are forbidden (CS0663). Overloads "by value" and "by `ref`" are allowed: `M(1)` calls the by-value version, `M(ref field)` the `ref` version.

### Caller information attributes

| Attribute | What the compiler substitutes |
|---|---|
| `[CallerMemberName]` | the name of the calling member; from top-level statements — `<Main>$` |
| `[CallerFilePath]`, `[CallerLineNumber]` | the file path and line number of the call |
| `[CallerArgumentExpression(nameof(arg))]` (C# 10) | the text of an argument expression: `Check(2 + 2 == 5)` → `failed: 2 + 2 == 5` |

A parameter with such an attribute must be optional. `ArgumentNullException.ThrowIfNull` and logging are built on this mechanism.

### Arguments

- Positional, named, and a mix of both. A named argument may appear before the end if it is in its own position (C# 7.2): `Range(start: 1, 5)` → `1..5`.
- Arguments and operands are evaluated **left to right**: the call `Three(Log("a", 1), Log("b", 2), Log("c", 3))` records the order `a,b,c`.

### Overload resolution rules

| Situation | What is chosen |
|---|---|
| an exact match and a conversion | the exact match |
| two conversions | the "better target": the type that converts implicitly to the other (`int` is better than `long`) |
| `Pick(int)` and `Pick(params int[])`, call `Pick(1)` | the version without `params` |
| `Generic<T>(T)` and `Generic(object)`, call `Generic(5)` | the generic one: `T = int` is an exact match, no boxing |
| `Opt(int)` and `Opt(int, int b = 0)`, call `Opt(1)` | the version that needs no default values |
| `M(string?)` and `M(object?)`, call `M(null)` | `string` — the more specific type |
| `M(string?)` and `M(int[]?)`, call `M(null)` | error CS0121: an ambiguous call |
| an instance method and an extension method with the same signature | the instance method; the extension is not even considered |

### Declarations and statements from other modules

Declarations inside a method: local variables (`var` or an explicit type), local constants `const`, ref variables (`ref int slot = ref …`, see the "Ref returns" section) and `scoped ref`.

| Statement | Where it is covered |
|---|---|
| `try` / `catch` / `finally`, `throw` | module 08 · Exceptions |
| `checked` / `unchecked` | [02 · Types and variables](../02-types-and-variables/basics.en.md) |
| `yield return` / `yield break` | module 07 · LINQ |
| `await`, `await foreach`, `await using` | module 10 · Async/await |
| `using`, `fixed`, `unsafe` | [12 · Memory and GC](../12-memory-and-gc/expert.en.md#pinning) |
| `lock` | module 18 · Concurrency |

## Gotchas

### foreach over null

```csharp
int[]? none = null;
foreach (var x in none!) { }   // NullReferenceException
```

`foreach` does not check the collection for `null`. Protection: `foreach (var x in none ?? [])`.

### A switch on a string is case-sensitive

`"Yes" switch { "yes" => "affirmative", _ => "unknown" }` gives `unknown`. For a case-insensitive comparison, normalize the input (`ToLowerInvariant()`) or use `when string.Equals(…, StringComparison.OrdinalIgnoreCase)`.

### in and a mutable struct: a defensive copy

```csharp
struct Counter
{
    public int Value;
    public void Increment() => Value++;
}

static void IncrementViaIn(in Counter c) => c.Increment();
static void IncrementViaRef(ref Counter c) => c.Increment();
```

After `IncrementViaIn(in counter)` the value is still `0`, after `IncrementViaRef(ref counter)` it is `1`. An `in` parameter cannot be changed, so the compiler calls the mutating method on a **defensive copy** and silently discards it. Takeaway: `in` is for a `readonly struct`; otherwise the copy also costs time. The same trap applies to a `SpinLock` in a `readonly` field: see [18 · Multithreading](../18-concurrency/expert.en.md).

### params and an explicit null

`Sum(null!)` for `Sum(params int[] values)` passes `null` rather than an empty array, and the `foreach` inside throws `NullReferenceException`.

### A generic overload beats object

For `Generic<T>(T value)` and `Generic(object value)`, the call `Generic(5)` picks `generic<Int32>`. The generic version with `T = int` is an exact match, while `object` would require boxing.

### break inside a switch in a loop

```csharp
for (int i = 0; i < 3; i++)
{
    iterations++;
    switch (i)
    {
        case 0:
            break;   // leaves only the switch
    }
}
```

The loop runs 3 times: `break` belongs to the `switch`, not to the loop. With a label (C# 15), `break loop;` leaves the loop after the first iteration.

### A floating-point counter

Adding `0.1f` ten times gives `1.0000001`, not `1`. The loop `for (float f = 0; f != 1; f += 0.1f)` never ends. Counters are integers; the condition for fractional values is `<`, not `!=`.

### & and && for bool

`false && Side()` does not call `Side`, while `false & Side()` does: after both expressions the call counter is 1. `&` and `|` on `bool` evaluate both operands.

### k++ + ++k

With `k = 1`, the expression `k++ + ++k` equals `4`, and `k` becomes `3`. C# guarantees left-to-right evaluation: `k++` gives 1 (and `k` = 2), then `++k` gives 3.

### A closure sees later changes

```csharp
int captured = 1;
Func<int> read = () => captured;
captured = 2;
read();   // 2
```

A lambda captures the variable, not its value at the moment of creation.

### CallerMemberName in top-level statements

When called from top-level statements, `[CallerMemberName]` substitutes `<Main>$` — the name of the method the compiler generated for them.

## Trade-offs: overloads or optional parameters

| | Overloads | Optional parameters |
|---|---|---|
| Amount of code | more | less |
| Changing a default value in a library | safe | requires recompiling the callers |
| Different logic for different types | yes | no |
| Readability of the call | depends on the names | named arguments make the call self-documenting |

## Interview questions

**1. How does `ref` differ from `out` and from `in`?**

<details><summary>Answer</summary>

All three pass a reference to a variable. `ref` — the argument must be initialized, the method can change it. `out` — the method must assign a value, no initialization is needed. `in` — read-only, intended for large structs.

</details>

**2. Why does `for` with lambdas give `[4, 4, 4]`, while `foreach` gives `[1, 2, 3]`?**

<details><summary>Answer</summary>

A lambda captures the variable. `for` has one variable for the whole loop, and after the loop it equals 4. In `foreach`, since C# 5, the variable is created anew on every iteration.

</details>

**3. How does the compiler pick the overload for `Describe('a')` with `Describe(int)`, `Describe(long)`, `Describe(double)`, `Describe(object)`?**

<details><summary>Answer</summary>

`char` converts implicitly to all the numeric options. The better target is `int`: it converts implicitly to `long` and `double`, but not the other way round. `object` requires boxing and loses to any numeric conversion.

</details>

**4. Does a type need `IEnumerable` to work in `foreach`?**

<details><summary>Answer</summary>

No. An accessible `GetEnumerator()` whose result has `MoveNext()` and `Current` is enough. That is how `List<T>` is iterated without allocations: its enumerator is a struct.

</details>

**5. What happens with a non-exhaustive switch expression?**

<details><summary>Answer</summary>

The compiler gives warning CS8509 (or CS8524 for unnamed `enum` values) and generates an arm that throws `SwitchExpressionException`. An explicit `_ => throw …` arm with a clear exception is better.

</details>

**6. Why can't `StackOverflowException` be caught?**

<details><summary>Answer</summary>

On a stack overflow the runtime terminates the process, and `catch` handlers do not run. Protection is limiting the recursion depth, rewriting it as a loop, or using an explicit `Stack<T>`.

</details>

**7. Why is changing a parameter's default value in a public method dangerous?**

<details><summary>Answer</summary>

The value is substituted into the calling assembly's code when that assembly is compiled. Without recompiling, consumers keep passing the old value. Adding an overload is safer.

</details>

**8. 🪤 Why did calling a method on an `in` parameter not change the struct?**

<details><summary>Answer</summary>

If the struct is not `readonly`, the compiler cannot prove that the method does not change it, so it calls the method on a defensive copy. The changes are lost. Fixes: a `readonly struct`, `readonly` methods, or `ref` instead of `in`.

</details>

**9. 🪤 What is `k++ + ++k` when `k = 1`?**

<details><summary>Answer</summary>

4, and after the expression `k` equals 3. In C# the evaluation order of operands is defined — left to right — unlike C and C++, where such an expression is undefined behavior.

</details>

**10. 🪤 There are `M<T>(T x)` and `M(object x)`. What does `M(5)` call?**

<details><summary>Answer</summary>

The generic version: type inference gives `T = int`, which is an exact match, while `object` requires boxing. Many people expect `object`.

</details>

**11. How do you leave two nested loops at once?**

<details><summary>Answer</summary>

Since C# 15 — `break label;` with the label of the outer loop. Before that, people used `goto`, a flag variable, or moved the loops into a separate method and left with `return`.

</details>

**12. How does `&&` differ from `&` for `bool`?**

<details><summary>Answer</summary>

`&&` evaluates the right operand only if the left one is `true` (short-circuit). `&` always evaluates both. The difference matters when the right operand has a side effect or is guarded by a check: `x != null && x.Length > 0`.

</details>

**13. Can you declare the overloads `M(ref int x)` and `M(out int x)`?**

<details><summary>Answer</summary>

No, error CS0663: at the CLR level `ref` and `out` are the same thing. `M(int x)` and `M(ref int x)` can be declared together.

</details>

**14. What is `[CallerArgumentExpression]` and where is it used?**

<details><summary>Answer</summary>

An attribute (C# 10) that passes the text of another argument's expression into an optional parameter. That is how `ArgumentNullException.ThrowIfNull(user)` knows the name `user`, and custom checks print the condition: `failed: 2 + 2 == 5`.

</details>

## Cheat sheet

| Task | How |
|---|---|
| Choose a result by value | `x switch { 1 => "a", _ => "b" }` |
| Ranges in a switch | `< 0 => …`, `>= 0 and < 10 => …` |
| Several values | `case A: case B:` or `A or B => …` |
| Return success and a value | `bool TryX(…, out T result)` |
| Swap two variables | `(a, b) = (b, a)` |
| Avoid copying a large struct | an `in` parameter |
| Variable number of arguments | `params int[]` or `params ReadOnlySpan<int>` |
| Remove items from a list | `list.RemoveAll(predicate)` |
| Helper function inside a method | `static int Helper(int x) => …` at the end of the method |

## Related materials

- Prerequisites: [02 · Types and variables](../02-types-and-variables/overview.en.md)
- Earlier: [01 · The .NET platform and dotnet CLI](../01-dotnet-platform/overview.en.md)
- `using`, `fixed` and pinning: [12 · Memory and GC](../12-memory-and-gc/expert.en.md#pinning)
- The same idea in other tracks: [Python](../../../python/overview.en.md) — `match`, `for … in` loops, `*args` and `**kwargs` arguments

## Sources

- [Selection statements: if, switch](https://learn.microsoft.com/dotnet/csharp/language-reference/statements/selection-statements)
- [Switch expression](https://learn.microsoft.com/dotnet/csharp/language-reference/operators/switch-expression)
- [Patterns](https://learn.microsoft.com/dotnet/csharp/language-reference/operators/patterns)
- [Iteration statements](https://learn.microsoft.com/dotnet/csharp/language-reference/statements/iteration-statements)
- [Method parameters: ref, out, in, params](https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/method-parameters)
- [Named and optional arguments](https://learn.microsoft.com/dotnet/csharp/programming-guide/classes-and-structs/named-and-optional-arguments)
- [Local functions](https://learn.microsoft.com/dotnet/csharp/programming-guide/classes-and-structs/local-functions)
- [Ref returns and ref variables](https://learn.microsoft.com/dotnet/csharp/language-reference/statements/declarations#reference-variables)
- [Jump statements](https://learn.microsoft.com/dotnet/csharp/language-reference/statements/jump-statements)
- [Overload resolution (specification)](https://learn.microsoft.com/dotnet/csharp/language-reference/language-specification/expressions)
- [Caller information attributes](https://learn.microsoft.com/dotnet/csharp/language-reference/attributes/caller-information)
- [What's new in C# 15](https://learn.microsoft.com/dotnet/csharp/whats-new/csharp-15)
