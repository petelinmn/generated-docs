---
title: "Consensus and coordination"
lang: en
track: system-design
kind: module
doc: overview
slug: 12-consensus-and-coordination
part: 3
levels: [1, 2, 3, 4, 5, 6]
tags: [system-design, consensus, raft, leader-election, distributed-lock, fencing-token, logical-clocks]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-26
---

# Consensus and coordination

🌐 [Русский](overview.ru.md) · **English**

## In a nutshell

Several nodes need to agree: who is the leader, what the order of writes is, who owns a resource. Meanwhile nodes crash, pause and lose connectivity.

## Key ideas

- Consensus requires a majority.
- Raft splits the task into leader election and log replication.
- Terms act as a logical clock.
- A lease expires, and the holder may not notice.
- A fencing token lets storage reject the old holder.
- The order of events does not follow from clock readings.
- You do not write your own consensus: you take a ready system.

## Minimal example

```csharp
if (token < _highestToken) return false; // a holder from the past
```

## When to use and when not

- ✅ Choosing a single worker, configuration, cluster metadata.
- ✅ Understanding the guarantees of a database or broker.
- ❌ A distributed lock where a database constraint is enough.
- ❌ Your own Raft implementation in a product.

## Levels of this material

| Level | What's inside |
|---|---|
| [1. Basics](basics.en.md) | majority, leader election, lease and fencing |
| [2. Advanced](advanced.en.md) | voting and commit rules in Raft |
| [3. Expert](expert.en.md) | logical clocks, reference, gotchas, questions |
| [4. Self-check](quiz.en.md) | a quiz and [exercises](exercises.en.md) |
| [5. Runnable examples](code/Examples/) | voting, fencing, Lamport clocks |
| [6. Tests](code/Tests/) | `dotnet test` inside `code/` |

## Related materials

- Prerequisites: [replication](../06-replication-and-partitioning/overview.en.md), [consistency and CAP](../07-consistency-and-cap/overview.en.md)
- In other tracks: [Kafka replication](../../../kafka/modules/07-replication-and-availability/overview.en.md), [Kubernetes architecture](../../../kubernetes/modules/02-architecture/overview.en.md)
