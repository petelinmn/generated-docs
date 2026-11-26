---
title: "Replication and partitioning"
lang: en
track: system-design
kind: module
doc: overview
slug: 06-replication-and-partitioning
part: 2
levels: [1, 2, 3, 4, 5, 6]
tags: [system-design, replication, partitioning, shard, quorum, replication-lag, version-vector]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-26
---

# Replication and partitioning

🌐 [Русский](overview.ru.md) · **English**

## In a nutshell

Replication keeps copies of the same data on several nodes: for fault tolerance and reads. Partitioning (sharding) splits data between nodes: for volume and writes. Usually both are needed at once.

## Key ideas

- Most often one leader writes and replicas replay its log.
- Asynchronous replication is faster, but a replica lags and data may be lost on failover.
- Read-your-writes requires knowing the position of your write.
- Quorums with `W + R > N` guarantee an overlap, but not everything else.
- The partition key determines load distribution and query cost.
- A hot key is not cured by adding nodes.
- Concurrent writes must be detected, not overwritten by clock.

## Minimal example

```csharp
public static bool Overlaps(int n, int w, int r) => w + r > n;
```

## When to use and when not

- ✅ Replication: almost always for important data.
- ✅ Partitioning: when volume or writes do not fit on a node.
- ❌ Sharding early without an estimate.
- ❌ Treating a replica as a backup: a delete is replicated too.

## Levels of this material

| Level | What's inside |
|---|---|
| [1. Basics](basics.en.md) | leader and replicas, lag, kinds of partitioning |
| [2. Advanced](advanced.en.md) | synchrony, read-your-writes, quorums, hot keys |
| [3. Expert](expert.en.md) | conflicts, version vectors, gotchas, questions |
| [4. Self-check](quiz.en.md) | a quiz and [exercises](exercises.en.md) |
| [5. Runnable examples](code/Examples/) | lag, quorums, partitions, version vectors |
| [6. Tests](code/Tests/) | `dotnet test` inside `code/` |

## Related materials

- Prerequisites: [data storage](../04-data-storage/overview.en.md), [consistent hashing](../02-scalability-and-load-balancing/overview.en.md)
- In other tracks: [Kafka replication](../../../kafka/modules/07-replication-and-availability/overview.en.md), [RabbitMQ clustering](../../../rabbitmq/modules/08-clustering/overview.en.md), [OpenSearch shards](../../../opensearch/modules/07-cluster-and-shards/overview.en.md)
- [Consistency and CAP](../07-consistency-and-cap/overview.en.md)
