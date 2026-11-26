---
title: "Replication and partitioning: advanced"
lang: en
track: system-design
kind: module
doc: advanced
slug: 06-replication-and-partitioning
part: 2
tags: [system-design, replication, partitioning, shard, quorum, replication-lag, version-vector]
versions: ".NET 11, C# 15"
status: verified
updated: 2026-11-26
---

# Replication and partitioning: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

## Synchronous and asynchronous replication

Synchronous: the leader acknowledges a write after the replica — less loss, more latency, and an unavailable replica may stop writes. Asynchronous: fast, but the latest writes may vanish when the leader fails. In PostgreSQL this is set by `synchronous_commit` (default `on`) together with `synchronous_standby_names`.

## Read-your-writes

The client remembers the log position of its write; a replica is suitable only if it has already applied it.

```csharp
public static string? ReadYourWrites(Leader leader, Follower follower, string key, long lastWritePosition) =>
    follower.AppliedPosition >= lastWritePosition ? follower.Read(key) : leader.Read(key);
```

## Quorums

Of N copies, W acknowledge a write and a read asks R. With `W + R > N` the sets intersect, and the newest acknowledged version is among the answers.

```csharp
public (long Version, string? Value) Read(params int[] asked) =>
    asked.Select(i => _replicas[i]).MaxBy(r => r.Version);
```

| N | W | R | Failures tolerated for both operations |
|---|---|---|---|
| 3 | 2 | 2 | 1 |
| 5 | 3 | 3 | 2 |
| 3 | 3 | 1 | 0 |

File: [Ex02_Quorum.cs](code/Examples/Ex02_Quorum.cs).

## Hot keys

One key gets a disproportionate number of requests. A salt splits it into several, and the reader queries them all.

```csharp
public static string Salted(string key, long sequence, int buckets) => $"{key}#{sequence % buckets}";
```

## Rebalancing

`hash % N` moves a lot of data when N changes: with 100,000 keys, going from 4 to 8 partitions moved 50.2%. Use [consistent hashing](../02-scalability-and-load-balancing/advanced.en.md) or create many logical partitions in advance and move them whole.

## Best practices

- Pick a key so that frequent queries hit one partition.
- Watch lag and partition skew.
- Rehearse failover.
- Remember the limits: in PostgreSQL a unique key of a partitioned table must include all partition key columns.

## Common mistakes

- ❌ Cross-shard transactions and joins on a hot path → ✅ a key that keeps related data together.
- ❌ Automatic failover without protection against two leaders → ✅ fencing.
- ❌ Treating `W + R > N` as a full consistency guarantee → ✅ see the expert level.
- ❌ Changing the shard key "later" → ✅ that is a migration of all data.

## Comparison with other tracks

- ISR and `min.insync.replicas` — [Kafka](../../../kafka/modules/07-replication-and-availability/overview.en.md).
- Quorum queues — [RabbitMQ](../../../rabbitmq/modules/08-clustering/overview.en.md).
- Primary and replica shards — [OpenSearch](../../../opensearch/modules/07-cluster-and-shards/overview.en.md).

## Related materials

- [Basics](basics.en.md)
- [Expert theory](expert.en.md)

## Sources

- [PostgreSQL: WAL settings](https://www.postgresql.org/docs/current/runtime-config-wal.html)
- [PostgreSQL: table partitioning](https://www.postgresql.org/docs/current/ddl-partitioning.html)
