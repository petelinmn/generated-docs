---
title: "Control flow and methods: advanced"
lang: en
track: csharp
kind: module
doc: advanced
slug: 03-control-flow-and-methods
part: 1
tags: [control-flow, switch-expression, pattern-matching, loops, methods, ref-out-in, overloading, local-functions, closures]
versions: ".NET 11, C# 15"
status: verified
updated: 2026-11-20
---

# Control flow and methods: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md) · [exercises](exercises.en.md) · [5–6. Code and tests](code/)

## Parameter modifiers: ref, out, in, params

By default an argument is passed **by value**: the method gets a copy of the value (for reference types, a copy of the reference; see [02 · Types and variables](../02-types-and-variables/basics.en.md)). Modifiers change that:

```csharp
// ref: the method works with the caller's variable itself, not with a copy.
public static void Swap(ref int a, ref int b) => (a, b) = (b, a);

// out: the method must assign the value before returning. This is the Try-pattern.
public static bool TryDivide(int dividend, int divisor, out int quotient)
{
    if (divisor == 0)
    {
        quotient = 0;
        return false;
    }
    quotient = dividend / divisor;
    return true;
}

// in: passed by reference but read-only, so a large struct is not copied.
public static double Length(in Vector3D v) => Math.Sqrt(v.X * v.X + v.Y * v.Y + v.Z * v.Z);

// params collections (C# 13): params over a span, no array allocation at the call site.
public static int SumSpan(params ReadOnlySpan<int> values)
{
    int sum = 0;
    foreach (int value in values)
    {
        sum += value;
    }
    return sum;
}
```

| Modifier | Passing | Method can change it | The caller writes |
|---|---|---|---|
| none | a copy of the value | only the copy | `M(x)` |
| `ref` | a reference to the variable | yes | `M(ref x)`, the variable is initialized |
| `out` | a reference to the variable | must assign it | `M(out var x)` |
| `in` | a read-only reference | no | `M(in x)` or `M(x)` |
| `params` | a set of arguments | — | `M(1, 2, 3)` |

The **Try** pattern (`bool TryXxx(…, out T result)`) is the standard way to report failure without an exception: `int.TryParse`, `Dictionary.TryGetValue`. Since C# 13, `params` works not only with arrays but also with collections, including `ReadOnlySpan<T>`, which removes the array allocation. Code: [Ex04_ParameterModifiers.cs](code/Examples/Ex04_ParameterModifiers.cs).

For small types (`int`, `double`, small structs), `ref` and `in` bring no speed gain: copying a few bytes costs no more than passing a reference, and access through a reference adds indirection. `in` makes sense for large structs, and preferably a `readonly struct` — why is explained in the [expert theory](expert.en.md).

## Method overloading

Methods with the same name but different parameters are overloads. The compiler picks an overload **at compile time** from the argument types and prefers the "better" conversion:

```csharp
public static class Overloading
{
    // The parameter is unused on purpose: only the chosen overload matters.
    public static string Describe(int value) => "int";

    public static string Describe(long value) => "long";

    public static string Describe(double value) => "double";

    public static string Describe(string? value) => "string";

    public static string Describe(object? value) => "object";
}
```

| Call | Chosen | Why |
|---|---|---|
| `Describe(5)` | `int` | exact match |
| `Describe(5L)` | `long` | exact match |
| `Describe(5f)` | `double` | `float` → `double` is better than boxing to `object` |
| `Describe('a')` | `int` | `char` converts implicitly to `int`, `long`, `double`; `int` is the "narrowest" |
| `Describe((short)5)` | `int` | the same rule |
| `Describe(5m)` | `object` | there is no implicit `decimal` → `double`, only boxing is left |
| `Describe(null)` | `string` | `string` is more specific than `object` |

The "better conversion target" rule: of two applicable overloads, the one whose parameter type converts implicitly to the other's type, but not the other way round, wins (`int` → `long` exists, `long` → `int` does not). Code: [Ex05_Overloading.cs](code/Examples/Ex05_Overloading.cs).

## Local functions

```csharp
// The local function Fib is visible only inside Fibonacci and can use its locals (memo).
public static long Fibonacci(int n)
{
    ArgumentOutOfRangeException.ThrowIfNegative(n);
    var memo = new Dictionary<int, long>();
    return Fib(n);

    long Fib(int k)
    {
        if (k < 2)
        {
            return k;
        }
        if (memo.TryGetValue(k, out long cached))
        {
            return cached;
        }
        long result = Fib(k - 1) + Fib(k - 2);
        memo[k] = result;
        return result;
    }
}

// A static local function cannot capture locals: an accidental capture becomes a compile error.
public static int SumOfSquares(int[] values)
{
    int total = 0;
    foreach (int value in values)
    {
        total += Square(value);
    }
    return total;

    static int Square(int x) => x * x;
}
```

A local function is declared inside a method and is visible only there. Unlike a lambda, it can be recursive without tricks, can be `static` (then capturing variables is a compile error) and does not need a delegate. `Fibonacci(50)` → `12586269025`. Code: [Ex06_LocalFunctions.cs](code/Examples/Ex06_LocalFunctions.cs).

## Loop pitfalls

```csharp
// Wrong: changing a List<T> invalidates its enumerator, the next MoveNext throws.
public static void RemoveNegativesWrong(List<int> values)
{
    foreach (int value in values)
    {
        if (value < 0)
        {
            values.Remove(value);
        }
    }
}

// foreach creates a fresh variable for every iteration (since C# 5), so each closure sees its own value.
public static int[] CaptureInForeach()
{
    var getters = new List<Func<int>>();
    foreach (int i in new[] { 1, 2, 3 })
    {
        getters.Add(() => i);
    }
    return getters.Select(get => get()).ToArray();
}

// for has a single variable for the whole loop: every closure sees its final value.
public static int[] CaptureInFor()
{
    var getters = new List<Func<int>>();
    for (int i = 1; i <= 3; i++)
    {
        getters.Add(() => i);
    }
    return getters.Select(get => get()).ToArray();
}
```

- `RemoveNegativesWrong([1, -2, 3])` throws `InvalidOperationException`: a collection cannot be changed while `foreach` is iterating it. The right way is to walk backwards by index or call `RemoveAll`.
- `CaptureInForeach()` → `[1, 2, 3]`, while `CaptureInFor()` → `[4, 4, 4]`. A lambda captures the **variable**, not its value. In `foreach` the variable is new on every iteration; in `for` there is one variable for the whole loop, and after the loop it equals `4`.

Code: [Ex07_LoopPitfalls.cs](code/Examples/Ex07_LoopPitfalls.cs).

## Common mistakes

- ❌ Changing a `List<T>` inside a `foreach` over it → ✅ a backwards `for` loop or `RemoveAll`.
- ❌ Capturing a `for` variable in a lambda and expecting the current value → ✅ copy it to a local variable inside the body or use `foreach`.
- ❌ `Math.Abs(int.MinValue)` → ✅ remember that `int.MinValue` has no positive counterpart: the method throws `OverflowException` (a test checks this for `DigitSum(int.MinValue)`).
- ❌ Returning "magic" values (`-1`, `null`) on a parse error → ✅ the Try pattern with `out`.
- ❌ Overloads that differ only in numeric types → ✅ remember the selection rules; sometimes different method names are clearer.
- ❌ Long `if/else if` chains over one value → ✅ a switch expression.

## Related materials

- Prerequisites: [02 · Types and variables](../02-types-and-variables/overview.en.md)
- Earlier: [01 · The .NET platform and dotnet CLI](../01-dotnet-platform/overview.en.md)
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
