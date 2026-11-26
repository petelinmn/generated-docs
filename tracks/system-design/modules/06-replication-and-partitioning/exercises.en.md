---
title: "Exercises: replication and partitioning"
lang: en
track: system-design
kind: module
doc: exercises
slug: 06-replication-and-partitioning
part: 2
tags: [system-design, replication, partitioning, shard, quorum, replication-lag, version-vector]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-26
---

# Exercises: replication and partitioning

🌐 [Русский](exercises.ru.md) · **English**

> Solve it yourself before opening the hints. The solutions run from [code/Solutions/](code/Solutions/), their tests are in [code/Tests/](code/Tests/).

## 1. 🟢 The minimum read quorum

**Task:** from N and W compute the smallest R that overlaps; reject an invalid W.

**Expected result:**

```text
(3, 2) -> 2; (5, 1) -> 5; (5, 5) -> 1
```

<details><summary>Hint</summary>

The condition `W + R > N` for integers.

</details>

<details><summary>Solution</summary>

```csharp
public static int MinReadQuorum(int n, int w)
{
    if (w < 1 || w > n) throw new ArgumentOutOfRangeException(nameof(w));
    return n - w + 1;
}
```

The smaller W, the more expensive and less available reads are. File: [code/Solutions/Exercises.cs](code/Solutions/Exercises.cs).

</details>

## 2. 🟡 Merging version vectors

**Task:** build a vector that has seen everything both have seen.

**Expected result:**

```text
{a:2, b:1} + {b:3, c:1} -> {a:2, b:3, c:1}
```

<details><summary>Hint</summary>

The maximum per node.

</details>

<details><summary>Solution</summary>

```csharp
a.Keys.Union(b.Keys).ToDictionary(
    node => node,
    node => Math.Max(a.GetValueOrDefault(node), b.GetValueOrDefault(node)));
```

Merging vectors does not merge values: what to do with two versions of the data is up to the application.

</details>

## 3. 🔴 Partition skew

**Task:** the load of the busiest partition divided by the average.

**Expected result:**

```text
[0, 0, 0, 1000] -> 4
```

<details><summary>Hint</summary>

What do you return when there is no load?

</details>

<details><summary>Solution</summary>

```csharp
double total = load.Sum();
return total == 0 ? 1 : load.Max() / (total / load.Count);
```

A value of 1 means perfect evenness, the number of partitions means everything goes into one.

</details>
