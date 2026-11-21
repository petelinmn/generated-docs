---
title: "Types and variables: advanced"
lang: en
track: csharp
kind: module
doc: advanced
slug: 02-types-and-variables
part: 1
tags: [types, value-types, reference-types, type-conversion, strings, nullable-reference-types, boxing, floating-point]
versions: ".NET 11, C# 15"
status: verified
updated: 2026-11-20
---

# Types and variables: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md) · [exercises](exercises.en.md) · [5–6. Code and tests](code/)

## Nullable reference types

A reference variable can hold `null`, and accessing its members then throws `NullReferenceException`. Since C# 8 the compiler can track this in advance. In a project with `<Nullable>enable</Nullable>` (the default in new templates):

- `string` means "never null", `string?` means "may be null";
- accessing members of a `string?` without a check produces warning **CS8602** ("Dereference of a possibly null reference");
- assigning `null` to a `string` produces **CS8600**, an uninitialized `string` field produces **CS8618**.

Operators for working with null:

```csharp
// ?. yields null instead of throwing, ?? supplies a fallback value.
public static int LengthOrZero(string? text) => text?.Length ?? 0;

// ??= assigns only when the variable is null.
public static string EnsureName(string? name)
{
    name ??= "Anonymous";
    return name;
}

// Without the null check the compiler warns: CS8602 "Dereference of a possibly null reference".
public static string Shout(string? text)
{
    if (text is null)
    {
        return "";
    }
    return text.ToUpperInvariant(); // flow analysis knows that text is not null here
}
```

- `?.` — null-conditional: `text?.Length` is `null` if `text` is `null`.
- `??` — null-coalescing: the right-hand value if the left one is `null`.
- `??=` — assign if the variable is `null`.
- `!` — null-forgiving: `text!.Length` tells the compiler "I know it is not null here". It only suppresses the warning; there is no run-time check.
- `is null` / `is not null` — a check that cannot be broken by an `==` operator overload.

Flow analysis understands checks: after `if (text is null) return`, the compiler treats `text` as non-null.

## Boxing

When a value type is converted to `object` (or to an interface), the runtime creates a new heap object and **copies** the value into it. This is boxing. The reverse is unboxing.

```csharp
// Two boxes of the same value are two different objects with equal contents.
public static (bool SameReference, bool EqualValues) CompareBoxes()
{
    object a = 42;
    object b = 42;
    return (ReferenceEquals(a, b), a.Equals(b));
}

// == on variables of type object compares references, not values.
public static bool EqualityOperatorOnBoxes()
{
    object a = 42;
    object b = 42;
    return a == b;
}

// Unboxing must use the exact boxed type: a boxed int cannot be unboxed as long.
public static long UnboxAsLong(object boxed) => (long)boxed;
```

- `CompareBoxes()` → `(false, true)`: two different boxes with equal contents.
- `EqualityOperatorOnBoxes()` → `false`: `==` on `object` compares references.
- `UnboxAsLong(42)` → `InvalidCastException`: the box holds an `int`, but it is unboxed as `long`. The right way: `(long)(int)boxed`.

Boxing costs an allocation and GC work. It appears silently when value types are passed to APIs that take `object`. Generics (module 06) solve this problem. Code: [Ex05_Boxing.cs](code/Examples/Ex05_Boxing.cs). Boxing is also why `lock` on a value type is forbidden: see [18 · Multithreading](../18-concurrency/expert.en.md).

## double vs decimal

`float` and `double` are binary numbers following the IEEE 754 standard. Most decimal fractions (e.g. 0.1) cannot be represented exactly in binary:

```csharp
// 0.1 has no exact binary representation, so the sum is slightly off.
public static bool DoubleSumIsExact() => 0.1 + 0.2 == 0.3;

public static bool DecimalSumIsExact() => 0.1m + 0.2m == 0.3m;

// "R" prints the shortest text that round-trips to the same double.
public static string DoubleSumText() => (0.1 + 0.2).ToString("R", CultureInfo.InvariantCulture);

// Compare doubles with a tolerance, never with ==.
public static bool NearlyEqual(double a, double b, double tolerance = 1e-9) => Math.Abs(a - b) <= tolerance;

// Floating-point division by zero does not throw: it yields infinity (integer division would throw).
public static double DivideByZero(double value) => value / 0.0;
```

| Call | Result |
|---|---|
| `DoubleSumIsExact()` | `false` |
| `DecimalSumIsExact()` | `true` |
| `DoubleSumText()` | `0.30000000000000004` |
| `DivideByZero(1.0)` | `∞` (`double.PositiveInfinity`) |

The rule: money and anything that must add up to the cent goes into `decimal`. Physics, graphics and statistics go into `double`: it is faster and has a huge range. Integer division by zero throws `DivideByZeroException`, while `double` division yields infinity or `NaN`. Code: [Ex06_FloatingPoint.cs](code/Examples/Ex06_FloatingPoint.cs).

## Culture: casing, comparison, number format

Many string operations use the operating system's **current culture** by default. Code that works on your machine can break for a user from another country:

```csharp
// In Turkish, the uppercase of "i" is "İ" (U+0130), not "I".
public static string ToUpperTurkish(string text) => text.ToUpper(CultureInfo.GetCultureInfo("tr-TR"));

public static string ToUpperInvariant(string text) => text.ToUpperInvariant();

// Ordinal comparison ignores the culture: the right choice for keys, identifiers, file names.
public static bool EqualsIgnoreCase(string a, string b) => string.Equals(a, b, StringComparison.OrdinalIgnoreCase);

public static string FormatInGerman(double value) => value.ToString("N2", CultureInfo.GetCultureInfo("de-DE"));

public static string FormatInvariant(double value) => value.ToString("N2", CultureInfo.InvariantCulture);
```

- `ToUpperTurkish("i")` → `İ`, while `ToUpperInvariant("i")` → `I`.
- `FormatInGerman(1234.5)` → `1.234,50`, while `FormatInvariant(1234.5)` → `1,234.50`.

The rule: text a person reads uses the user's culture. Identifiers, keys, files, protocols and logs use `CultureInfo.InvariantCulture` and `StringComparison.Ordinal`/`OrdinalIgnoreCase`. Code: [Ex07_CultureAware.cs](code/Examples/Ex07_CultureAware.cs).

If `InvariantGlobalization` is enabled in the project, cultures are not available at all — see [the gotcha in module 01](../01-dotnet-platform/expert.en.md#invariantglobalization-breaks-cultures).

## Common mistakes

- ❌ Storing money in `double` → ✅ `decimal`.
- ❌ Comparing `double` values with `==` → ✅ compare with a tolerance (`NearlyEqual`).
- ❌ Expecting `(int)2.9` to be `3` → ✅ `(int)` drops the fraction; round with `Math.Round` and remember banker's rounding.
- ❌ `int.Parse(userInput)` → ✅ `int.TryParse` with an explicit culture.
- ❌ String concatenation in a loop → ✅ `StringBuilder` or `string.Join`.
- ❌ `ToUpper()` and `ToLower()` without a culture for comparisons → ✅ `string.Equals(a, b, StringComparison.OrdinalIgnoreCase)`.
- ❌ Silencing nullable warnings with `!` → ✅ an `is null` check or a meaningful default.
- ❌ Changing a copy of a struct and being surprised that the original did not change → ✅ remember copy semantics; mutable structs are a source of bugs.

## Related materials

- Prerequisites: [01 · The .NET platform and dotnet CLI](../01-dotnet-platform/overview.en.md) — the project, `<Nullable>enable</Nullable>`, building
- Next: [03 · Control flow and methods](../03-control-flow-and-methods/overview.en.md) — `ref`, `out`, `in` and passing value types to methods
- What boxing costs the GC: [12 · Memory and GC](../12-memory-and-gc/advanced.en.md#measuring-allocations)
- `decimal` in a database through EF Core (SQLite stores it as `TEXT`): [15 · EF Core](../15-ef-core/expert.en.md#sqlite-specifics)
- The same idea in other tracks: [Python](../../../python/overview.en.md) — dynamic typing and type annotations instead of static types

## Sources

- [C# built-in types](https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/built-in-types)
- [Value types](https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/value-types)
- [Reference types](https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/reference-types)
- [Nullable reference types](https://learn.microsoft.com/dotnet/csharp/nullable-references)
- [Boxing and unboxing](https://learn.microsoft.com/dotnet/csharp/programming-guide/types/boxing-and-unboxing)
- [Floating-point numeric types](https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/floating-point-numeric-types)
- [Best practices for comparing strings](https://learn.microsoft.com/dotnet/standard/base-types/best-practices-strings)
- [Raw string literals](https://learn.microsoft.com/dotnet/csharp/language-reference/tokens/raw-string)
- [`string.Intern`](https://learn.microsoft.com/dotnet/api/system.string.intern)
