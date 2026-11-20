---
title: "Exercises: Control flow and methods"
lang: en
track: csharp
kind: module
doc: exercises
slug: 03-control-flow-and-methods
part: 1
tags: [switch-expression, ref-out-in, loops, local-functions]
status: verified
updated: 2026-11-20
---

# Exercises: Control flow and methods

🌐 [Русский](exercises.ru.md) · **English**

> Solve it yourself before opening the hints. Solutions are under spoilers, runnable versions are in [code/Solutions/Solutions.cs](code/Solutions/Solutions.cs), their tests are in [code/Tests/SolutionsTests.cs](code/Tests/SolutionsTests.cs).

## 1. 🟢 Grade from a score

**Task:** write `Grades.Grade(int score)`, which returns a grade: 90–100 → `A`, 80–89 → `B`, 70–79 → `C`, 60–69 → `D`, 0–59 → `F`. For scores outside 0–100 the method throws `ArgumentOutOfRangeException`. Use a switch expression.

**Expected result:**

```text
Grade(90)  → "A"
Grade(59)  → "F"
Grade(101) → ArgumentOutOfRangeException
```

<details><summary>Hint</summary>

Put the range check as the first arm: `< 0 or > 100 => throw …`. The remaining arms go from high to low, and each one can rely on the previous ones not having matched.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 1: a switch expression with relational patterns and a guard for invalid input.
public static class Grades
{
    public static string Grade(int score) => score switch
    {
        < 0 or > 100 => throw new ArgumentOutOfRangeException(nameof(score), score, "Score must be 0..100"),
        >= 90 => "A",
        >= 80 => "B",
        >= 70 => "C",
        >= 60 => "D",
        _ => "F",
    };
}
```

The `Grades` class in [Solutions.cs](code/Solutions/Solutions.cs). `throw` can be used as an expression right inside a switch arm.

</details>

## 2. 🟡 Parsing coordinates

**Task:** write `Coordinates.TryParse(string? input, out int x, out int y)` for strings like `"3,-4"`. The method returns `true` and fills `x` and `y` if the string contains exactly two integers separated by a comma, and `false` otherwise.

**Expected result:**

```text
TryParse("3,-4", out x, out y) → true, x = 3, y = -4
TryParse("3,4,5", …)           → false
TryParse(null, …)              → false
```

<details><summary>Hint</summary>

`out` parameters must be assigned on every exit path — assign `0` to them at the start. `int.TryParse` with `out x` assigns the value directly.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 2: the Try-pattern with out parameters.
public static class Coordinates
{
    public static bool TryParse(string? input, out int x, out int y)
    {
        x = 0;
        y = 0;
        if (input is null)
        {
            return false;
        }
        string[] parts = input.Split(',');
        return parts.Length == 2
            && int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out x)
            && int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out y);
    }
}
```

The `Coordinates` class in [Solutions.cs](code/Solutions/Solutions.cs). An `&&` chain stops at the first `false`, so the second `TryParse` does not run if the first one failed.

</details>

## 3. 🟡 Counting primes

**Task:** write `Primes.CountUpTo(int limit)`, which counts the prime numbers from 2 to `limit` inclusive. Make the "is it prime" check a `static` local function.

**Expected result:**

```text
CountUpTo(10)  → 4    (2, 3, 5, 7)
CountUpTo(100) → 25
CountUpTo(1)   → 0
```

<details><summary>Hint</summary>

It is enough to check divisors up to the square root: the loop condition is `divisor * divisor <= n`.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 3: loops plus a static local function.
public static class Primes
{
    public static int CountUpTo(int limit)
    {
        int count = 0;
        for (int n = 2; n <= limit; n++)
        {
            if (IsPrime(n))
            {
                count++;
            }
        }
        return count;

        static bool IsPrime(int n)
        {
            for (int divisor = 2; divisor * divisor <= n; divisor++)
            {
                if (n % divisor == 0)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
```

The `Primes` class in [Solutions.cs](code/Solutions/Solutions.cs).

</details>

## 4. 🔴 A countdown without IEnumerable

**Task:** create a `Countdown` struct that can be iterated with `foreach`. `new Countdown(3)` yields `3, 2, 1`, `new Countdown(0)` yields nothing. Do not implement `IEnumerable` and do not create heap objects.

**Expected result:**

```text
foreach (int n in new Countdown(3)) → 3, 2, 1
```

<details><summary>Hint</summary>

Look at [Ex10_DuckTypedForeach.cs](code/Examples/Ex10_DuckTypedForeach.cs): you need a `GetEnumerator()` method that returns a struct with `MoveNext()` and `Current`.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 4: a foreach-able type without IEnumerable and without heap allocations.
public readonly struct Countdown
{
    private readonly int _from;

    public Countdown(int from) => _from = from;

    public Enumerator GetEnumerator() => new(_from);

    public struct Enumerator
    {
        private int _current;

        public Enumerator(int from) => _current = from + 1;

        public readonly int Current => _current;

        public bool MoveNext() => --_current > 0;
    }
}
```

The `Countdown` struct in [Solutions.cs](code/Solutions/Solutions.cs). The enumerator starts at `from + 1`, and `MoveNext` first decrements the value, then checks it.

</details>
