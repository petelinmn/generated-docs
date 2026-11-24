---
title: "Clustering and high availability: advanced"
lang: en
track: rabbitmq
kind: module
doc: advanced
slug: 08-clustering
part: 2
tags: [rabbitmq, cluster, maintenance-mode, rolling-upgrade, quorum, network-partition]
versions: "RabbitMQ 4.4.1; AMQP 0-9-1; Python pika 1.5.0"
status: draft
updated: 2026-11-24
---

# Clustering and high availability: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

Builds on the [basic theory](basics.en.md) (the same three-node Docker rig). Everything below was obtained by running on RabbitMQ 4.4.1 unless stated otherwise.

## How many nodes you need

A quorum queue keeps working while a majority of replicas is reachable: more than half of the replica count.

| Nodes | Majority | Failures survived |
|---|---|---|
| 1 | 1 | 0 |
| 2 | 2 | 0 |
| 3 | 2 | 1 |
| 5 | 3 | 2 |

A two-node cluster is no better than one: the failure of either node removes the majority. An even number of nodes adds nothing over the previous odd one. In the three-node test stopping one node did not hinder publishing (`ack`), and stopping two stopped writes (`timeout`) until the nodes returned.

## Maintenance mode

Before an upgrade or reboot a node is put into maintenance mode: it stops accepting clients and hands off leadership.

```text
$ rabbitmq-upgrade drain
Will put node rabbit@rmq3 into maintenance mode. The node will no longer serve any client traffic!

Node: rabbit@rmq3, status: marked for maintenance

$ rabbitmq-upgrade revive
Will put node rabbit@rmq3 back into regular operating mode. The node will again serve client traffic and host primary queue replicas.

Node: rabbit@rmq3, status: not under maintenance
```

The status is visible in `cluster_status` under `Maintenance status`. A typical rolling upgrade: for each node `drain`, stop, upgrade, start, `revive`, then the next.

> ⚠️ Verify: the full procedure of a rolling upgrade between versions and the version-compatibility requirements were not verified here; only `drain`/`revive` were reproduced.

## Quorum criticality of a node

Before stopping a node check that it will not deprive some queue of its majority:

```text
$ rabbitmq-diagnostics check_if_node_is_quorum_critical
Checking if node rabbit@rmq3 is critical for quorum of any queues/streams ...
Node rabbit@rmq3 reported no queues/streams with minimum quorum
```

If queues are at their `minimum quorum`, stopping the node would take away their majority: first bring back the other nodes or move replicas.

## Replica state

`rabbitmq-queues quorum_status <queue>` prints a table: node, Raft state (`leader`/`follower`), membership type (`voter`), log indexes (`Last Log Index`, `Commit Index` …) and `Term`:

```text
Node Name     Raft State  Membership  Last Log Index  Last Written  Last Applied  Commit Index  Snapshot Index  Term  Machine Version
rabbit@rmq1   leader      voter       2               2             2             2             -1              1     9
rabbit@rmq2   follower    voter       2               2             2             2             -1              1     9
```

(The third node's row is omitted.) Matching indexes mean the replicas are in sync.

## Clients and a node failure

When a node fails, connections to it break. The client must reconnect to another node and repeat unconfirmed operations ([module 05](../05-reliability/advanced.en.md)): we did not test an automatic reconnect in `pika.Connection`. Typical schemes: a node list in the client or a load balancer in front of the cluster.

> ⚠️ Verify: pika's automatic reconnect and a load balancer's behaviour on a node failure were not verified here.

## Network partitions

A network partition splits the cluster into groups of nodes that cannot see each other. Quorum queues keep working only in the group that holds a majority.

> ⚠️ Verify: a network partition was not reproduced here (stopping a container on one machine is not the same), the `cluster_partition_handling` setting and the behaviour of classic queues under a partition were not checked.

## Best practices

- Three or five nodes (an odd number), on different hosts or zones.
- Important queues are quorum, durable messages persistent.
- Before maintenance — `check_if_node_is_quorum_critical`, then `drain`.
- Clients know all nodes and can reconnect.
- Upgrade one node at a time, watching replica sync.

## Common mistakes

- ❌ Two nodes → ✅ three or more.
- ❌ Stopping a node without checking criticality → ✅ `check_if_node_is_quorum_critical`.
- ❌ All nodes on one host/zone → ✅ separate failure domains.
- ❌ A classic queue as "highly available" → ✅ quorum.
- ❌ Expecting the client to "heal" itself → ✅ reconnect and retry.

## Related materials

- Before: [basics](basics.en.md)
- Next: [expert theory](expert.en.md)
- [07 · Queue types](../07-queue-types/advanced.en.md)
- [05 · Reliability](../05-reliability/advanced.en.md) — republishing the unconfirmed
- Next module: [09 · Monitoring and operations](../09-monitoring/overview.en.md)

## Sources

- [Clustering Guide](https://www.rabbitmq.com/docs/clustering)
- [Upgrading RabbitMQ](https://www.rabbitmq.com/docs/upgrade)
- [Quorum Queues](https://www.rabbitmq.com/docs/quorum-queues)
