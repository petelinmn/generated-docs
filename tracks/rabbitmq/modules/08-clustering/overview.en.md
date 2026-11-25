---
title: "Clustering and high availability"
lang: en
track: rabbitmq
kind: module
doc: overview
slug: 08-clustering
part: 2
levels: [1, 2, 3, 4]
tags: [rabbitmq, cluster, high-availability, quorum, maintenance-mode, node-failure]
versions: "RabbitMQ 4.4.1; AMQP 0-9-1; Python pika 1.5.0"
status: draft
updated: 2026-11-24
---

# Clustering and high availability

🌐 [Русский](overview.ru.md) · **English**

## In short

A cluster is several RabbitMQ nodes sharing metadata: a client connects to any of them and operations on a queue are forwarded to the node where it lives. High availability comes from **quorum queues**: they are replicated and work while a majority of replicas is reachable. A classic queue lives on one node and is unavailable when it fails.

## Key ideas

- **Metadata is shared, queue data is not.**
- **A node joins like this:** `stop_app`, `join_cluster rabbit@first`, `start_app`.
- **A node failure:** a classic queue is unavailable (`404 … process is stopped by supervisor`), quorum keeps working.
- **Majority:** 3 nodes survive 1 failure, 5 survive 2; avoid two nodes and even counts.
- **Without a majority a write is not confirmed** (`timeout`), not rejected.
- **Before maintenance:** `check_if_node_is_quorum_critical`, `rabbitmq-upgrade drain`, then `revive`.
- **The rig is for teaching** (three containers on one machine): network partitions were not reproduced.

## Minimal example

```bash
docker exec rmq2 rabbitmqctl stop_app
docker exec rmq2 rabbitmqctl join_cluster rabbit@rmq1
docker exec rmq2 rabbitmqctl start_app
```

Everything was run on three RabbitMQ 4.4.1 containers (see the [basic theory](basics.en.md)).

## When to use and when not

| Need | Solution |
|---|---|
| Survive a node failure without losing messages | a 3-node cluster + quorum + persistent |
| A simple single installation | one node, accepting the risk |
| Link independent brokers | Federation/Shovel ([roadmap](../../overview.en.md)) |

- ❌ Do not build a two-node cluster "for reliability".
- ❌ Do not treat a classic queue as highly available.

## Material levels

| Level | What is inside |
|---|---|
| [1. Basic theory](basics.en.md) | a cluster, where a queue lives, a node failure, majority |
| [2. Advanced theory](advanced.en.md) | how many nodes, maintenance mode, criticality, replica state |
| [3. Expert theory](expert.en.md) | reference of commands and the failure matrix, gotchas, interview questions |
| [4. Self-check](quiz.en.md) | quiz and [exercises](exercises.en.md) |
| 5. Runnable examples | not created yet — `/learn rabbitmq 08-clustering level 5` |
| 6. Tests | not created yet — `/learn rabbitmq 08-clustering level 6` |

## Related materials

- Prerequisites: [07 · Queue types](../07-queue-types/overview.en.md)
- Next: [09 · Monitoring and operations](../09-monitoring/overview.en.md)
- Analogues in other tracks: [Kubernetes](../../../kubernetes/overview.en.md)
- [Cluster and shards in OpenSearch](../../../opensearch/modules/07-cluster-and-shards/overview.en.md)
