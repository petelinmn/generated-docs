---
title: "Clustering and high availability: basics"
lang: en
track: rabbitmq
kind: module
doc: basics
slug: 08-clustering
part: 2
tags: [rabbitmq, cluster, high-availability, quorum-queue, node-failure, join-cluster]
versions: "RabbitMQ 4.4.1; AMQP 0-9-1; Python pika 1.5.0"
status: draft
updated: 2026-11-24
---

# Clustering and high availability: basics

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [07 · Queue types](../07-queue-types/basics.en.md) · **Time:** ~1.5 h

## How the code was verified

A three-node cluster was built in Docker: network `rmqnet`, containers `rmq1`, `rmq2`, `rmq3` (image `rabbitmq:4-management`, RabbitMQ 4.4.1, Erlang 29.1.2) with the same Erlang cookie. The nodes' AMQP ports are published as 5681, 5682, 5683. The client is pika 1.5.0. Everything below is actual output; **this is a single-machine teaching rig**: it shows the logic of failures but not real network faults between machines. The helper `conn(port)` opens a connection to a node, `Pub` is a confirmed publish from [module 05](../05-reliability/basics.en.md#how-the-code-was-verified).

```bash
docker network create rmqnet
docker run -d --name rmq1 --hostname rmq1 --network rmqnet \
  -e RABBITMQ_ERLANG_COOKIE=secretcookie -e RABBITMQ_NODENAME=rabbit@rmq1 \
  -p 5681:5672 rabbitmq:4-management
# the same for rmq2 (5682) and rmq3 (5683)
```

> The cookie in the example is for teaching. In a real system it is a secret: whoever knows it controls the node.

## What a cluster is

A cluster is several RabbitMQ nodes joined into one logical system: they share metadata (users, vhosts, exchanges, queues, bindings) and a client can connect to any node. Nodes join with these commands:

```bash
docker exec rmq2 rabbitmqctl stop_app
docker exec rmq2 rabbitmqctl join_cluster rabbit@rmq1
docker exec rmq2 rabbitmqctl start_app
```

```text
Clustering node rabbit@rmq2 with rabbit@rmq1
```

`rabbitmqctl cluster_status` shows the membership:

```text
Disk Nodes
rabbit@rmq1
rabbit@rmq2
rabbit@rmq3

Running Nodes
rabbit@rmq1
rabbit@rmq2
rabbit@rmq3
```

## Where a queue lives

Metadata is shared, but **the queue itself** lives on particular nodes. A classic queue on one (where it was declared); a quorum queue on several, visible in `list_queues`:

```text
name  type     leader        members
cl    classic  rabbit@rmq1   [rabbit@rmq1]
qu    quorum   rabbit@rmq2   [rabbit@rmq2, rabbit@rmq3, rabbit@rmq1]
```

A client can work with a queue through any node: connecting to `rmq2` showed the depth of the classic queue living on `rmq1`:

```text
via node2 depth: 1
```

The node you connect to forwards operations to the node where the queue lives.

## A node failure

Stop `rmq1` (`docker stop rmq1`), where the classic queue `cl` lives, while the quorum queue `qu` is replicated on all three nodes.

```text
classic while node1 down: ChannelClosedByBroker (404, "NOT_FOUND - queue 'cl' in vhost '/' process is stopped by supervisor")
quorum depth with node1 down: 2
publish to quorum with node1 down: ack
consumed: ['q1', 'q2', 'q3']
```

- **Classic:** the queue is unavailable while only `rmq3` is up (404). After `docker start rmq1` the persistent message came back: `classic message back: 1`.
- **Quorum:** publishing and reading continue on the two remaining nodes, no messages lost (`q1`, `q2` and the new `q3`).

That is the point of quorum queues: they survive a node failure.

## Majority and quorum

A quorum queue works while a **majority** of replicas is reachable (2 of 3). Stop `rmq2` too, leaving 1 node of 3:

```text
publish with 1 of 3 nodes: timeout
```

No confirm: without a majority the queue accepts no writes. After the nodes were started:

```text
publish after recovery: ack
```

## Common beginner mistakes

- ❌ Assuming a cluster replicates every queue automatically → ✅ classic stays on one node; quorum and stream are replicated.
- ❌ A two-node cluster "for safety" → ✅ if one of two fails there is no majority; use three or more.
- ❌ Connecting clients to one node only → ✅ a node list or a load balancer, plus reconnection.
- ❌ Keeping the teaching Erlang cookie → ✅ a secret, identical across the cluster's nodes.
- ❌ Believing a cluster prevents data loss when messages are not persistent → ✅ durable + persistent + quorum.

## Related materials

- Next: [advanced theory](advanced.en.md) — node maintenance, quorum criticality
- Before: [07 · Queue types](../07-queue-types/basics.en.md)
- Next module: [09 · Monitoring and operations](../09-monitoring/overview.en.md)
- Analogue in another track: [Kubernetes](../../../kubernetes/overview.en.md)

## Sources

- [Clustering Guide](https://www.rabbitmq.com/docs/clustering)
- [Quorum Queues](https://www.rabbitmq.com/docs/quorum-queues)
- [Reliability Guide](https://www.rabbitmq.com/docs/reliability)
