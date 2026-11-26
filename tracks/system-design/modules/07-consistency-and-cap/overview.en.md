---
title: "Consistency and CAP"
lang: en
track: system-design
kind: module
doc: overview
slug: 07-consistency-and-cap
part: 2
levels: [1, 2, 3, 4, 5, 6]
tags: [system-design, consistency, cap-theorem, isolation-levels, mvcc, write-skew, crdt]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-26
---

# Consistency and CAP

🌐 [Русский](overview.ru.md) · **English**

## In a nutshell

When data is read and written concurrently and from several copies, you have to decide what exactly each party sees. Consistency models and isolation levels are a set of promises of different strength and cost.

## Key ideas

- Consistency of copies and isolation of transactions are different questions.
- During a network partition you have to choose: refuse or diverge.
- Without a partition the choice between latency and consistency remains (PACELC).
- Snapshot isolation is not serializable: write skew is possible.
- Optimistic concurrency requires retrying the transaction.
- Session guarantees are often enough and cheaper than strict ones.
- CRDTs converge without coordination but do not fit every rule.

## Minimal example

```csharp
public static bool HasQuorum(int reachable, int n) => reachable > n / 2;
```

## When to use and when not

- ✅ Money, stock levels, uniqueness: strong guarantees are needed.
- ✅ Choosing a database and an isolation level.
- ❌ Demanding strict consistency for a view counter.
- ❌ Relying on an isolation level without checking what it means in your DBMS.

## Levels of this material

| Level | What's inside |
|---|---|
| [1. Basics](basics.en.md) | consistency models, CAP, anomalies |
| [2. Advanced](advanced.en.md) | isolation levels, write skew, PACELC, session guarantees |
| [3. Expert](expert.en.md) | reference, gotchas, questions |
| [4. Self-check](quiz.en.md) | a quiz and [exercises](exercises.en.md) |
| [5. Runnable examples](code/Examples/) | MVCC, a network partition, sessions |
| [6. Tests](code/Tests/) | `dotnet test` inside `code/` |

## Related materials

- Prerequisites: [replication and partitioning](../06-replication-and-partitioning/overview.en.md)
- Next: [messaging and events](../08-messaging-and-events/overview.en.md)
- In other tracks: [EF Core: concurrency and transactions](../../../csharp/modules/15-ef-core/overview.en.md), [concurrency in C#](../../../csharp/modules/18-concurrency/overview.en.md)
- [Distributed transactions](../10-distributed-transactions/overview.en.md)
- [Consensus and coordination](../12-consensus-and-coordination/overview.en.md)
- [Event sourcing and CQRS](../13-event-sourcing-and-cqrs/overview.en.md)
- [Multi-region and recovery](../16-multi-region-and-dr/overview.en.md)
- [Transactions in PostgreSQL](../../../postgresql/modules/06-transactions/overview.en.md)
