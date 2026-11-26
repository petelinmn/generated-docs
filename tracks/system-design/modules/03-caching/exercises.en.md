---
title: "Exercises: caching"
lang: en
track: system-design
kind: module
doc: exercises
slug: 03-caching
part: 1
tags: [system-design, caching, cache-aside, cache-invalidation, cache-stampede, lru-cache, hybrid-cache]
versions: ".NET 11, C# 15; Microsoft.Extensions.Caching.Hybrid 11.0.1"
status: draft
updated: 2026-11-26
---

# Exercises: caching

🌐 [Русский](exercises.ru.md) · **English**

> Solve it yourself before opening the hints. The solutions run from [code/Solutions/](code/Solutions/), their tests are in [code/Tests/](code/Tests/).

## 1. 🟢 Load on the source

**Task:** from the total QPS and the hit ratio, compute how many requests reach the source.

**Expected result:**

```text
(10000, 0.99) -> 100
```

<details><summary>Hint</summary>

Only misses reach the source.

</details>

<details><summary>Solution</summary>

```csharp
public static double OriginQps(double totalQps, double hitRatio) => totalQps * (1 - hitRatio);
```

File: [code/Solutions/Exercises.cs](code/Solutions/Exercises.cs).

</details>

## 2. 🟡 Entry freshness

**Task:** from the age, TTL and stale window, return `Fresh`, `Stale` or `Missing`.

**Expected result:**

```text
ttl 10 s, window 5 s: 9 -> Fresh, 10 -> Stale, 15 -> Missing
```

<details><summary>Hint</summary>

The bounds are strict: exactly at the TTL the entry is no longer fresh.

</details>

<details><summary>Solution</summary>

```csharp
public static Freshness Classify(TimeSpan age, TimeSpan ttl, TimeSpan staleWindow) =>
    age < ttl ? Freshness.Fresh
    : age < ttl + staleWindow ? Freshness.Stale
    : Freshness.Missing;
```

A `Stale` value is returned immediately and a background refresh is started.

</details>

## 3. 🔴 A versioned cache

**Task:** a write whose version is not newer than the stored one must be ignored, including under concurrent calls.

**Expected result:**

```text
Set(1, version 2, "new"); Set(1, version 1, "old"); Get(1) -> "new"
```

<details><summary>Hint</summary>

`ConcurrentDictionary.AddOrUpdate` recomputes the value if it changed underneath you.

</details>

<details><summary>Solution</summary>

```csharp
public void Set(TKey key, long version, TValue value) =>
    _entries.AddOrUpdate(
        key,
        (version, value),
        (_, current) => version > current.Version ? (version, value) : current);
```

The update delegate may run several times, so it has no side effects. After a key is removed the version is forgotten: protection against a race with deletion needs a tombstone entry with a version.

</details>
