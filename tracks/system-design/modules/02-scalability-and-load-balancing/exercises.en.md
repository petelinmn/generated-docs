---
title: "Exercises: scaling and load balancing"
lang: en
track: system-design
kind: module
doc: exercises
slug: 02-scalability-and-load-balancing
part: 1
tags: [system-design, scaling, scale-out, load-balancing, consistent-hashing, yarp]
versions: ".NET 11, C# 15; Yarp.ReverseProxy 2.3.0"
status: draft
updated: 2026-11-26
---

# Exercises: scaling and load balancing

🌐 [Русский](exercises.ru.md) · **English**

> Solve it yourself before opening the hints. The solutions run from [code/Solutions/](code/Solutions/), their tests are in [code/Tests/](code/Tests/).

## 1. 🟢 Health-aware round robin

**Task:** pick only healthy nodes in a circle; if there are none, return `null`.

**Expected result:**

```text
nodes a b c, b is down -> a c a c
```

<details><summary>Hint</summary>

What happens to the neighbour of a dead node if you simply skip it in the circle?

</details>

<details><summary>Solution</summary>

```csharp
public string? Pick()
{
    // Filter first, then rotate: otherwise the neighbour of a dead node gets double load.
    string[] healthy = [.. nodes.Where(isHealthy)];
    if (healthy.Length == 0) return null;
    uint i = unchecked((uint)Interlocked.Increment(ref _next));
    return healthy[(int)(i % (uint)healthy.Length)];
}
```

This is a teaching version: an array is allocated on every pick. In production the healthy list is updated when state changes. File: [code/Solutions/Exercises.cs](code/Solutions/Exercises.cs).

</details>

## 2. 🟡 Share of moved keys

**Task:** from a list of keys and two placement functions ("before" and "after"), return the share of keys that changed node.

**Expected result:**

```text
keys a b; before: x x; after: x y -> 0.5
```

<details><summary>Hint</summary>

`Count` with a predicate and division in `double`.

</details>

<details><summary>Solution</summary>

```csharp
public static double MovedFraction(IReadOnlyList<string> keys, Func<string, string> before, Func<string, string> after) =>
    (double)keys.Count(k => before(k) != after(k)) / keys.Count;
```

Plug in `HashRing.NodeFor` and `ModuloSharding.ShardFor` to compare the approaches.

</details>

## 3. 🔴 The optimal number of nodes

**Task:** find the `n` from 1 to `max` with the highest throughput under USL.

**Expected result:**

```text
(contention 0.05, coherency 0.001, max 200) -> 31
```

<details><summary>Hint</summary>

`Enumerable.Range` and `MaxBy`.

</details>

<details><summary>Solution</summary>

```csharp
public static int BestNodeCount(double contention, double coherency, int max) =>
    Enumerable.Range(1, max).MaxBy(n => ScalingLaws.Universal(contention, coherency, n));
```

Past the peak, every new node reduces overall throughput.

</details>
