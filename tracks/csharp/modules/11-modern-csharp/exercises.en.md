---
title: "Exercises: Modern C#"
lang: en
track: csharp
kind: module
doc: exercises
slug: 11-modern-csharp
part: 2
tags: [pattern-matching, switch-expression, collection-expressions, field-keyword, list-patterns, extension-members, union-types]
status: draft
updated: 2026-11-22
---

# Exercises: Modern C#

🌐 [Русский](exercises.ru.md) · **English**

> Solve it yourself before opening the hints. Solutions are under spoilers. The code was verified by running it; there are no runnable versions in `code/` yet (level 5 does not exist).

## 1. 🟢 Grade by score

**Task:** write `static string Grade(int score)` using a `switch` expression: below 0 or above 100 — `"invalid"`, from 90 — `"A"`, from 75 — `"B"`, from 50 — `"C"`, otherwise `"F"`.

**Expected result:**

```text
-1 → invalid, 101 → invalid, 95 → A, 80 → B, 60 → C, 10 → F
```

<details><summary>Hint</summary>

Put the pattern `< 0 or > 100` first. Then the relational patterns in descending order of bounds: `>= 90`, `>= 75`, `>= 50`, and `_`.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 1: relational patterns in a switch expression.
static string Grade(int score) => score switch
{
    < 0 or > 100 => "invalid",
    >= 90 => "A",
    >= 75 => "B",
    >= 50 => "C",
    _ => "F",
};
```

Verified: the result is `invalid invalid A B C F`. Arm order matters: the bounds descend so that `95` does not fall into a weaker arm.

</details>

## 2. 🟢 Merging collections

**Task:** write `static int[] Merge(int[] a, List<int> b)`: it returns the elements of `a`, then the number `0` as a separator, then the elements of `b`. Use a collection expression.

**Expected result:**

```text
Merge([1, 2], [3, 4]) → [1, 2, 0, 3, 4]
Merge([], [])         → [0]
```

<details><summary>Hint</summary>

The spread `..` inserts the elements of another collection: `[..a, 0, ..b]`.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 2: collection expression with spreads.
static int[] Merge(int[] a, List<int> b) => [..a, 0, ..b];
```

Verified: for empty inputs the result length is `1`. The target type `int[]` determines that an array is created.

</details>

## 3. 🟡 Temperature with validation

**Task:** write a class `Temperature` with a `double Celsius` property that does not allow setting a value below absolute zero (−273.15): a too-small value is replaced with −273.15. Do not declare a separate field.

**Expected result:**

```text
new Temperature { Celsius = -500 }.Celsius == -273.15  → True
Celsius = 25                                            → 25
```

<details><summary>Hint</summary>

The `field` keyword (C# 14) refers to the automatic backing field: `get; set => field = ...`.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 3: validation in a property setter via the field keyword.
class Temperature
{
    public double Celsius { get; set => field = Math.Max(value, -273.15); }
}
```

Verified: `-500` becomes `-273.15`, `25` is kept. The getter stays automatic.

</details>

## 4. 🟡 Command parsing

**Task:** write `static string Command(string[] args)` with list patterns: an empty array — `"no args"`; `["help"]` — `"help"`; `["add", x]` — `"add x"`; `["add", x, ...rest]` — `"add x and N more"`; any command whose last argument is `--force` — `"<command> (forced)"`; everything else — `"unknown"`.

**Expected result:**

```text
[]                       → no args
["help"]                 → help
["add", "x"]             → add x
["add", "x", "y", "z"]   → add x and 2 more
["rm", "a", "--force"]   → rm (forced)
["zzz"]                  → unknown
```

<details><summary>Hint</summary>

More specific arms go above general ones: `["add", var item]` before `["add", var first, .. var rest]`. For the last element: `[var cmd, .., "--force"]`.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 4: list patterns for command-line parsing.
static string Command(string[] args) => args switch
{
    [] => "no args",
    ["help"] => "help",
    ["add", var item] => $"add {item}",
    ["add", var first, .. var rest] => $"add {first} and {rest.Length} more",
    [var cmd, .., "--force"] => $"{cmd} (forced)",
    _ => "unknown",
};
```

Verified on the six inputs from the table. `["add", "x"]` does not reach the fourth arm because the third one is above it.

</details>

## 5. 🔴 Extensions via extension members

**Task:** write `extension` blocks for: (a) a property `bool IsPalindrome` on `string` — ignoring case and everything except letters and digits; (b) a **static** member `string.Repeat(string value, int count)`; (c) a property `double Median` on `IEnumerable<int>` (for an empty sequence — `InvalidOperationException`).

**Expected result:**

```text
"A man, a plan, a canal: Panama".IsPalindrome  → True
"hello".IsPalindrome                           → False
string.Repeat("ab", 3)                         → ababab
new[] { 3, 1, 2 }.Median                       → 2
```

<details><summary>Hint</summary>

`static class StringExt { extension(string s) { public bool IsPalindrome => …; public static string Repeat(string value, int count) => …; } extension(IEnumerable<int> src) { … } }`. For the median, sort the elements (`Order()`).

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 5: extension properties and a static extension member (C# 14).
static class StringExt
{
    extension(string s)
    {
        public bool IsPalindrome
        {
            get
            {
                var letters = s.Where(char.IsLetterOrDigit).Select(char.ToLowerInvariant).ToArray();
                return letters.SequenceEqual(letters.Reverse());
            }
        }
        public static string Repeat(string value, int count) => string.Concat(Enumerable.Repeat(value, count));
    }

    extension(IEnumerable<int> src)
    {
        public double Median
        {
            get
            {
                var sorted = src.Order().ToArray();
                if (sorted.Length == 0) throw new InvalidOperationException("Empty sequence.");
                int mid = sorted.Length / 2;
                return sorted.Length % 2 == 1 ? sorted[mid] : (sorted[mid - 1] + sorted[mid]) / 2.0;
            }
        }
    }
}
```

Verified: the results match the table. The static member `Repeat` is called as `string.Repeat(...)` although the `string` type was not modified.

</details>

## 6. 🔴 A result as a union

**Task:** declare `record Ok<T>(T Value)`, `record Error(string Message)` and a generic union `Result<T>` of these two cases (C# 15). Write `static string Show(Result<int> r)` as a `switch` without a `_` arm: for `Ok` — `"ok <value>"`, for `Error` — `"error <message>"`.

**Expected result:**

```text
new Ok<int>(5)    → ok 5
new Error("bad")  → error bad
```

<details><summary>Hint</summary>

`public union Result<T>(Ok<T>, Error);`. A case value converts implicitly to the union, and a `switch` with the positional patterns `Ok<int>(var v)` and `Error(var m)` is exhaustive.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 6: a generic union (C# 15) and an exhaustive switch.
public record Ok<T>(T Value);
public record Error(string Message);
public union Result<T>(Ok<T>, Error);

static string Show(Result<int> r) => r switch
{
    Ok<int>(var v) => $"ok {v}",
    Error(var m) => $"error {m}",
};
```

Verified on SDK 11.0.100: for both values `ok 5` and `error bad` are printed with no warnings. If one arm is removed, the compiler gives `CS8509`.

</details>
