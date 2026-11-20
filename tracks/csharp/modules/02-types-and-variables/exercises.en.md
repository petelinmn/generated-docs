---
title: "Exercises: Types and variables"
lang: en
track: csharp
kind: module
doc: exercises
slug: 02-types-and-variables
part: 1
tags: [type-conversion, nullable-reference-types, overflow, struct-layout]
status: verified
updated: 2026-11-20
---

# Exercises: Types and variables

🌐 [Русский](exercises.ru.md) · **English**

> Solve it yourself before opening the hints. Solutions are under spoilers, runnable versions are in [code/Solutions/Solutions.cs](code/Solutions/Solutions.cs), their tests are in [code/Tests/SolutionsTests.cs](code/Tests/SolutionsTests.cs).

## 1. 🟢 Parsing a temperature

**Task:** write `Temperature.ParseCelsius(string? input)`, which returns a `double?`: the number if the string is a valid decimal written with a dot, and `null` otherwise. Add `Temperature.ToFahrenheit(double celsius)`.

**Expected result:**

```text
ParseCelsius("36.6")  → 36.6
ParseCelsius("36,6")  → null
ParseCelsius(null)    → null
ToFahrenheit(100)     → 212
```

<details><summary>Hint</summary>

`double.TryParse` with `NumberStyles.Float` and `CultureInfo.InvariantCulture`. Formula: `F = C × 9 / 5 + 32`.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 1: parse user input safely and convert units.
public static class Temperature
{
    // Invariant culture: "36.6" must parse the same way on every machine.
    public static double? ParseCelsius(string? input) =>
        double.TryParse(input, NumberStyles.Float, CultureInfo.InvariantCulture, out double value)
            ? value
            : null;

    public static double ToFahrenheit(double celsius) => celsius * 9 / 5 + 32;
}
```

The `Temperature` class in [Solutions.cs](code/Solutions/Solutions.cs). The explicit culture matters: without it, `"36.6"` will not parse on a machine with a Russian locale, where the decimal separator is a comma.

</details>

## 2. 🟡 A sum without silent overflow

**Task:** write `SafeMath.CheckedSum(int[] values)`, which throws `OverflowException` on overflow, and `SafeMath.WideSum(int[] values)`, which returns a `long` and does not overflow.

**Expected result:**

```text
CheckedSum([1, 2, 3])             → 6
CheckedSum([int.MaxValue, 1])     → OverflowException
WideSum([int.MaxValue, int.MaxValue]) → 4294967294
```

<details><summary>Hint</summary>

`checked(sum + value)` inside the loop. For `WideSum` a `long` accumulator is enough: `int` widens implicitly.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 2: detect overflow instead of returning a wrong sum.
public static class SafeMath
{
    public static int CheckedSum(int[] values)
    {
        int sum = 0;
        foreach (int value in values)
        {
            sum = checked(sum + value);
        }
        return sum;
    }

    // A wider accumulator avoids the overflow altogether.
    public static long WideSum(int[] values)
    {
        long sum = 0;
        foreach (int value in values)
        {
            sum += value;
        }
        return sum;
    }
}
```

The `SafeMath` class in [Solutions.cs](code/Solutions/Solutions.cs).

</details>

## 3. 🟡 Normalizing a name

**Task:** write `Names.Normalize(string? name)`. For `null`, an empty string or a whitespace-only string the method returns `"Anonymous"`. Otherwise it trims the spaces, makes the first letter uppercase and the rest lowercase (culture-independent).

**Expected result:**

```text
Normalize(null)      → "Anonymous"
Normalize("  alice ") → "Alice"
Normalize("bOB")     → "Bob"
```

<details><summary>Hint</summary>

`string.IsNullOrWhiteSpace` — after this check the compiler treats `name` as non-null. The range `trimmed[1..]` gives the tail of the string.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 3: handle nullable input explicitly.
public static class Names
{
    public static string Normalize(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return "Anonymous";
        }
        string trimmed = name.Trim();
        return char.ToUpperInvariant(trimmed[0]) + trimmed[1..].ToLowerInvariant();
    }
}
```

The `Names` class in [Solutions.cs](code/Solutions/Solutions.cs). Note that there are no nullable warnings even though the `!` operator is not used.

</details>

## 4. 🔴 A compact struct

**Task:** the struct `Wasteful { byte A; long B; byte C; }` takes 24 bytes. Declare a struct `Compact` with the same three fields so that `Unsafe.SizeOf<Compact>()` is 16.

**Expected result:**

```text
Unsafe.SizeOf<Wasteful>() → 24
Unsafe.SizeOf<Compact>()  → 16
```

<details><summary>Hint</summary>

Draw the layout byte by byte: a `long` must start at an offset divisible by 8. Where do the empty bytes appear?

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 4: the same fields in a different order take less memory.
public struct Wasteful
{
    public byte A;
    public long B;
    public byte C;
}

// Largest fields first: the two bytes share one 8-byte slot after the long.
public struct Compact
{
    public long B;
    public byte A;
    public byte C;
}
```

The `Wasteful` and `Compact` structs in [Solutions.cs](code/Solutions/Solutions.cs). In `Wasteful`, `A` is followed by 7 bytes of padding, then `B` (8), then `C` and 7 more bytes: 24 in total. In `Compact` the `long` comes first, the two `byte` fields take offsets 8 and 9, and the size is rounded up to a multiple of 8: 16.

</details>
