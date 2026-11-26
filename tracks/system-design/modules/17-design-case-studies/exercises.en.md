---
title: "Exercises: case studies"
lang: en
track: system-design
kind: module
doc: exercises
slug: 17-design-case-studies
part: 3
tags: [system-design, case-studies, url-shortener, news-feed, notifications, id-generation]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-26
---

# Exercises: case studies

🌐 [Русский](exercises.ru.md) · **English**

> Solve it yourself before opening the hints. The solutions run from [code/Solutions/](code/Solutions/), their tests are in [code/Tests/](code/Tests/).

## 1. 🟢 Code length

**Task:** how many base62 characters are needed for `count` different codes.

**Expected result:**

```text
62 -> 1; 63 -> 2; 3_500_000_000_000 -> 7
```

<details><summary>Hint</summary>

Multiply the capacity by 62.

</details>

<details><summary>Solution</summary>

```csharp
int length = 1;
for (long capacity = 62; capacity < count; capacity *= 62) length++;
return length;
```

For values near the upper bound of `long` the multiplication overflows; real problems are far from that. File: [code/Solutions/Exercises.cs](code/Solutions/Exercises.cs).

</details>

## 2. 🟡 Decoding a user-supplied code

**Task:** decode base62, rejecting foreign characters, an empty string and overflow.

**Expected result:**

```text
"10" -> 62; "AzL8n0Y58m7" -> long.MaxValue; "AzL8n0Y58m8", "ab-", "" -> false
```

<details><summary>Hint</summary>

Check for overflow before multiplying.

</details>

<details><summary>Solution</summary>

```csharp
int digit = Alphabet.IndexOf(c);
if (digit < 0 || value > (long.MaxValue - digit) / 62)
{
    value = 0;
    return false;
}
value = value * 62 + digit;
```

This is the body of the loop over characters. Codes with leading zeros give the same number: reject them if that matters.

</details>

## 3. 🔴 Collision probability

**Task:** the approximate probability of at least one collision among `n` random codes out of `space`.

**Expected result:**

```text
n = 1_000_000, space = 62^7 -> about 0.13
```

<details><summary>Hint</summary>

`1 - exp(-n(n-1) / (2 space))`.

</details>

<details><summary>Solution</summary>

```csharp
public static double CollisionProbability(long n, double space) =>
    -double.ExpM1(-(double)n * (n - 1) / (2 * space));
```

`ExpM1` is more accurate than `1 - Math.Exp` for very small probabilities.

</details>
