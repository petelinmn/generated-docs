---
title: "Exercises: consensus and coordination"
lang: en
track: system-design
kind: module
doc: exercises
slug: 12-consensus-and-coordination
part: 3
tags: [system-design, consensus, raft, leader-election, distributed-lock, fencing-token, logical-clocks]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-26
---

# Exercises: consensus and coordination

🌐 [Русский](exercises.ru.md) · **English**

> Solve it yourself before opening the hints. The solutions run from [code/Solutions/](code/Solutions/), their tests are in [code/Tests/](code/Tests/).

## 1. 🟢 How many failures

**Task:** from the cluster size return the number of tolerated failures.

**Expected result:**

```text
3 -> 1; 4 -> 1; 5 -> 2
```

<details><summary>Hint</summary>

Integer division.

</details>

<details><summary>Solution</summary>

```csharp
public static int ToleratedFailures(int clusterSize) => (clusterSize - 1) / 2;
```

File: [code/Solutions/Exercises.cs](code/Solutions/Exercises.cs).

</details>

## 2. 🟡 A randomized election timeout

**Task:** return a duration from the interval `[min, max]`.

**Expected result:**

```text
(150 ms, 300 ms) -> a value in that range
```

<details><summary>Hint</summary>

A `TimeSpan` can be multiplied by a `double`.

</details>

<details><summary>Solution</summary>

```csharp
public static TimeSpan ElectionTimeout(TimeSpan min, TimeSpan max, Random random) =>
    min + (max - min) * random.NextDouble();
```

`Random` is passed in for repeatable tests.

</details>

## 3. 🔴 The commit index

**Task:** find the highest index the leader may declare committed.

**Expected result:**

```text
terms 1,2,4; current 4: replicated to 3 of 5 up to index 2 -> 0; up to index 3 -> 3
```

<details><summary>Hint</summary>

Two conditions: a majority and the entry's term equal to the current one.

</details>

<details><summary>Solution</summary>

```csharp
for (int n = logTerms.Count; n > committed; n--)
{
    bool majority = matchIndex.Count(m => m >= n) > matchIndex.Count / 2;
    // Entries of earlier terms are never committed by counting replicas alone.
    if (majority && logTerms[n - 1] == currentTerm) return n;
}
return committed;
```

An entry of an earlier term on a majority can still be overwritten.

</details>
