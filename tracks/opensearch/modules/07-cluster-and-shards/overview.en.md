---
title: "Cluster and shards"
lang: en
track: opensearch
kind: module
doc: overview
slug: 07-cluster-and-shards
part: 2
tags: [cluster, shard, replica, shard-allocation, high-availability]
versions: "OpenSearch 3.10.1; opensearch-py 3.2.0"
levels: [1, 2, 3, 4]
status: draft
updated: 2026-11-25
---

# Cluster and shards

🌐 [Русский](overview.ru.md) · **English**

## In a nutshell

How OpenSearch spreads an index over nodes, survives failures and how many shards to create.

## Key ideas

- The primary count is fixed at creation; replicas can change any time.
- Copies of a shard never share a node.
- Changing the state needs a majority of cluster-manager-eligible nodes.
- `_cluster/allocation/explain` tells why a shard is unassigned.
- Replicas are not a backup.

## Minimal example

```python
client.cat.shards(h="index,shard,prirep,state,node", v=True)
client.cluster.allocation_explain(body={"index": "orders", "shard": 0, "primary": False})
```

## When to use

When the cluster has more than one node or you ask "how many shards". For local development one node without replicas is enough.

## Material levels

| Level | File | Add with |
|---|---|---|
| 1. Fundamentals | [basics.en.md](basics.en.md) | |
| 2. Advanced theory | [advanced.en.md](advanced.en.md) | |
| 3. Expert theory | [expert.en.md](expert.en.md) | |
| 4. Self-check | [quiz.en.md](quiz.en.md), [exercises.en.md](exercises.en.md) | |
| 5. Runnable examples | — | `/learn opensearch 07-cluster-and-shards level 5` |
| 6. Tests | — | `/learn opensearch 07-cluster-and-shards level 6` |

## Related materials

- [OpenSearch track](../../overview.en.md)
- [05 · Indexing at scale](../05-indexing-at-scale/overview.en.md)
- [Replication and availability in Kafka](../../../kafka/modules/07-replication-and-availability/overview.en.md) — a counterpart
- [RabbitMQ clustering](../../../rabbitmq/modules/08-clustering/overview.en.md) — majority and quorum
- [08 · Index lifecycle](../08-index-lifecycle/overview.en.md)
