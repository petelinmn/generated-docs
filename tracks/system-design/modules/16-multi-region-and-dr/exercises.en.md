---
title: "Exercises: multi-region and recovery"
lang: en
track: system-design
kind: module
doc: exercises
slug: 16-multi-region-and-dr
part: 3
tags: [system-design, disaster-recovery, multi-region, failover, rpo-rto, backups, crdt]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-26
---

# Exercises: multi-region and recovery

🌐 [Русский](exercises.ru.md) · **English**

> Solve it yourself before opening the hints. The solutions run from [code/Solutions/](code/Solutions/), their tests are in [code/Tests/](code/Tests/).

## 1. 🟢 Choosing a region

**Task:** return the nearest healthy region of the required jurisdiction.

**Expected result:**

```text
US 10 ms, EU down 80 ms, EU 120 ms; jurisdiction EU -> the 120 ms region
```

<details><summary>Hint</summary>

Filter first, then `MinBy`.

</details>

<details><summary>Solution</summary>

```csharp
public static Region? Route(IEnumerable<Region> regions, string jurisdiction) =>
    regions.Where(r => r.Healthy && r.Jurisdiction == jurisdiction).MinBy(r => r.LatencyMs);
```

Refusing is better than breaking data location requirements. File: [code/Solutions/Exercises.cs](code/Solutions/Exercises.cs).

</details>

## 2. 🟡 The cheapest strategy

**Task:** from the target RTO and measured values pick the cheapest suitable strategy.

**Expected result:**

```text
RTO 30 min with the test numbers -> PilotLight
```

<details><summary>Hint</summary>

The order of the enum values sets the price. What do you return if none fits?

</details>

<details><summary>Solution</summary>

```csharp
Enum.GetValues<Strategy>() // declared from the cheapest to the most expensive
    .Where(s => achievable.TryGetValue(s, out TimeSpan t) && t <= rto)
    .Cast<Strategy?>()
    .FirstOrDefault();
```

`Cast<Strategy?>` makes "not found" a `null` rather than the first strategy.

</details>

## 3. 🔴 Backup retention

**Task:** keep the newest `daily` backups and the newest backup of each of the last `monthly` months.

**Expected result:**

```text
Jan 30, Jan 31, Feb 27, Feb 28, Mar 1 with daily 2, monthly 3 -> Jan 31, Feb 28, Mar 1
```

<details><summary>Hint</summary>

After sorting in descending order the first element of each group is the newest.

</details>

<details><summary>Solution</summary>

```csharp
DateOnly[] newestFirst = [.. backups.OrderDescending()];
var keep = new HashSet<DateOnly>(newestFirst.Take(daily));
keep.UnionWith(newestFirst.GroupBy(d => (d.Year, d.Month)).Take(monthly).Select(g => g.First()));
```

Deleting backups is irreversible: list them first, delete afterwards.

</details>
