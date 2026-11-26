---
title: "Replication and partitioning: basics"
lang: en
track: system-design
kind: module
doc: basics
slug: 06-replication-and-partitioning
part: 2
tags: [system-design, replication, partitioning, shard, quorum, replication-lag, version-vector]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-26
---

# Replication and partitioning: basics

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** databases, transactions in outline · **Time:** ~2 h

## Why it matters

One node will fail some day, and some day the data will stop fitting on it. Replication answers the first, partitioning the second.

## Leader and replicas

The leader accepts writes and keeps a log of changes; replicas apply it in order. Reads may be served by replicas.

| Scheme | Who accepts writes | Feature |
|---|---|---|
| Single leader | one node | no write conflicts, but failover is needed |
| Multi-leader | several nodes | conflicts |
| Leaderless | the client writes to several replicas | quorums and repair of copies |

## Replica lag

```csharp
public void Replicate(int maxEntries = int.MaxValue)
{
    while (AppliedPosition < leader.Log.Count && maxEntries-- > 0)
    {
        var (key, value) = leader.Log[(int)AppliedPosition];
        _data[key] = value;
        AppliedPosition++;
    }
}
```

Until `Replicate` is called, the replica returns the old value. This models replication lag: a user saved their profile, refreshed the page and saw the previous name. File: [Ex01_ReplicationLag.cs](code/Examples/Ex01_ReplicationLag.cs).

## Partitioning

| Kind | What it splits |
|---|---|
| Horizontal (sharding) | rows: one schema, each shard has its subset |
| Vertical | fields of one entity by usage pattern |
| Functional | data by business area |

## By range or by hash

```csharp
public static int ByRange(long key, params long[] upperBounds)
{
    for (int i = 0; i < upperBounds.Length; i++)
        if (key < upperBounds[i]) return i;
    return upperBounds.Length;
}
```

Ranges are handy for "from — to" queries, but with growing keys (time, a counter) all new writes go to the last partition: keys 3000–3999 with bounds 1000, 2000, 3000 give the load `0, 0, 0, 1000`. A hash spreads them as `234, 246, 266, 254`, but a range query visits all partitions. File: [Ex03_Partitioning.cs](code/Examples/Ex03_Partitioning.cs).

## Beginner mistakes

- ❌ A replica instead of a backup → ✅ you need both.
- ❌ Reading from a replica right after your own write → ✅ account for lag.
- ❌ A partition key with few values (country, status) → ✅ high cardinality.
- ❌ Sharding "for the future" → ✅ estimate first.

## Related materials

- [Advanced theory](advanced.en.md)
- [Replication in Kafka](../../../kafka/modules/07-replication-and-availability/overview.en.md)

## Sources

- [Azure: data partitioning](https://learn.microsoft.com/azure/architecture/best-practices/data-partitioning)
- [PostgreSQL: table partitioning](https://www.postgresql.org/docs/current/ddl-partitioning.html)
