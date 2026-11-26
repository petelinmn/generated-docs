---
title: "Consistency and CAP: basics"
lang: en
track: system-design
kind: module
doc: basics
slug: 07-consistency-and-cap
part: 2
tags: [system-design, consistency, cap-theorem, isolation-levels, mvcc, write-skew, crdt]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-26
---

# Consistency and CAP: basics

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** transactions, replication · **Time:** ~2 h

## Why it matters

Two people buy the last ticket. A replica does not know about the purchase yet. To decide what should happen, you need precise words for guarantees.

## Two different "consistencies"

| Term | About |
|---|---|
| C in ACID | a transaction preserves the rules of the data |
| C in CAP | linearizability: the system behaves like a single copy |
| Isolation | what concurrent transactions see of each other |

## Consistency models

| Model | Promise |
|---|---|
| Linearizability | once a write completes, everyone reads the new value |
| Causal | causally related writes are seen in the right order |
| Session guarantees | one client sees no contradictions |
| Eventual | with no new writes the copies converge some day |

## CAP

If the network splits the nodes (P), the system cannot both answer every request (A) and behave like a single copy (C). CAP is not about picking "two of three" in normal operation: partitions happen on their own, and you choose the behavior during them.

```csharp
public bool TryWrite(int node, string value)
{
    if (mode == Mode.Cp && !HasMajority(node)) return false;

    foreach (int i in Reachable(node)) _values[i] = value;
    return true;
}
```

In CP mode a node in the minority refuses, in AP mode both sides accept writes and diverge: after the connection is restored one has `left`, the other `right`. File: [Ex02_Partition.cs](code/Examples/Ex02_Partition.cs). This is a simplified model, not a replication protocol.

## Transaction anomalies

| Anomaly | What happens |
|---|---|
| Dirty read | uncommitted data is visible |
| Nonrepeatable read | rereading a row gave something else |
| Phantom read | repeating a query gave a different set of rows |
| Lost update | one change overwrote another |
| Write skew | each checked the rule, together they broke it |

## Beginner mistakes

- ❌ "We have transactions, so there are no races" → ✅ it depends on the isolation level.
- ❌ "We chose CA" → ✅ partitions happen anyway.
- ❌ Read, compute and write without protection → ✅ an atomic operation or a version check.
- ❌ Treating eventual consistency as "almost instant" → ✅ it promises no deadline.

## Related materials

- [Advanced theory](advanced.en.md)
- [Replication and partitioning](../06-replication-and-partitioning/overview.en.md)

## Sources

- [PostgreSQL: transaction isolation](https://www.postgresql.org/docs/current/transaction-iso.html)
