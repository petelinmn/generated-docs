---
title: "Exercises: consistency and CAP"
lang: en
track: system-design
kind: module
doc: exercises
slug: 07-consistency-and-cap
part: 2
tags: [system-design, consistency, cap-theorem, isolation-levels, mvcc, write-skew, crdt]
versions: ".NET 11, C# 15"
status: verified
updated: 2026-11-27
---

# Exercises: consistency and CAP

🌐 [Русский](exercises.ru.md) · **English**

> Solve it yourself before opening the hints. The solutions run from [code/Solutions/](code/Solutions/), their tests are in [code/Tests/](code/Tests/).

## 1. 🟢 Is there a majority

**Task:** can a group of `reachable` nodes keep working in a cluster of `n`.

**Expected result:**

```text
(2, 3) -> true; (2, 4) -> false; (3, 5) -> true
```

<details><summary>Hint</summary>

Half is not enough.

</details>

<details><summary>Solution</summary>

```csharp
public static bool HasQuorum(int reachable, int n) => reachable > n / 2;
```

That is why an even number of nodes adds no resilience. File: [code/Solutions/Exercises.cs](code/Solutions/Exercises.cs).

</details>

## 2. 🟡 Retrying a transaction

**Task:** repeat an attempt until success and return the number of the successful one; after the limit, throw.

**Expected result:**

```text
succeeds on the third call -> 3; never succeeds -> InvalidOperationException
```

<details><summary>Hint</summary>

A limit is mandatory.

</details>

<details><summary>Solution</summary>

```csharp
for (int i = 1; i <= maxAttempts; i++)
    if (attempt()) return i;
throw new InvalidOperationException($"No success after {maxAttempts} attempts.");
```

In real code add a delay with a random component.

</details>

## 3. 🔴 G-Counter

**Task:** a counter that grows on different nodes and converges whatever the order of merges.

**Expected result:**

```text
a: +1, b: +2; merge twice in any order -> 3 on both
```

<details><summary>Hint</summary>

Store each node's contribution separately.

</details>

<details><summary>Solution</summary>

```csharp
public void Merge(GCounter other)
{
    // Maximum, not sum: merging the same state twice must change nothing.
    foreach (var (key, count) in other._counts)
        _counts[key] = Math.Max(_counts.GetValueOrDefault(key), count);
}
```

Such a counter cannot be decreased; that needs a second one.

</details>
