---
title: "Exercises: data storage"
lang: en
track: system-design
kind: module
doc: exercises
slug: 04-data-storage
part: 1
tags: [system-design, data-modeling, database-index, b-tree, lsm-tree, query-plan, nosql]
versions: ".NET 11, C# 15; Microsoft.Data.Sqlite 11.0.0"
status: verified
updated: 2026-11-26
---

# Exercises: data storage

🌐 [Русский](exercises.ru.md) · **English**

> Solve it yourself before opening the hints. The solutions run from [code/Solutions/](code/Solutions/), their tests are in [code/Tests/](code/Tests/).

## 1. 🟢 A key for an access pattern

**Task:** build a string key whose ordinary order is "by chat, newest first within a chat".

**Expected result:**

```text
chat 1 at 200, chat 1 at 100, chat 2 at 100, chat 10 at 100
```

<details><summary>Hint</summary>

Pad numbers with zeros to a fixed width. To put new ones first, subtract the time from the maximum.

</details>

<details><summary>Solution</summary>

```csharp
public static string MessageKey(long chatId, long sentAtUnixMs) =>
    string.Create(CultureInfo.InvariantCulture, $"{chatId:D19}#{long.MaxValue - sentAtUnixMs:D19}");
```

Compare ordinally; the solution assumes non-negative numbers. File: [code/Solutions/Exercises.cs](code/Solutions/Exercises.cs).

</details>

## 2. 🟡 Merging segments

**Task:** merge an older and a newer segment; the newer value wins, tombstones are kept or dropped by a flag.

**Expected result:**

```text
older a=1 b=2, newer b=null c=3 -> keep: a b c; drop: a c
```

<details><summary>Hint</summary>

When is dropping a tombstone dangerous?

</details>

<details><summary>Solution</summary>

```csharp
var result = new SortedDictionary<string, string?>(StringComparer.Ordinal);
foreach (var (key, value) in older) result[key] = value;
foreach (var (key, value) in newer) result[key] = value;

if (dropTombstones)
    foreach (string key in result.Where(e => e.Value is null).Select(e => e.Key).ToList())
        result.Remove(key);

return result;
```

A tombstone may be dropped only if no older segments remain, otherwise the deleted value "comes back".

</details>

## 3. 🔴 Write amplification

**Task:** from the volume of user data and all disk writes, compute the write amplification.

**Expected result:**

```text
100 user bytes; writes 100, 100, 200 -> 4
```

<details><summary>Hint</summary>

Add up all writes: the log, the memtable flush, compaction.

</details>

<details><summary>Solution</summary>

```csharp
public static double WriteAmplification(long userBytes, params long[] diskWrites) =>
    (double)diskWrites.Sum() / userBytes;
```

The more aggressive the compaction, the lower the read and space amplification, but the more writing.

</details>
