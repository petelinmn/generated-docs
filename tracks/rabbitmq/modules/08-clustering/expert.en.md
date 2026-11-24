---
title: "Clustering and high availability: expert"
lang: en
track: rabbitmq
kind: module
doc: expert
slug: 08-clustering
part: 2
tags: [rabbitmq, cluster, quorum, raft, maintenance-mode, rabbitmq-queues, rabbitmqctl]
versions: "RabbitMQ 4.4.1; AMQP 0-9-1; Python pika 1.5.0"
status: draft
updated: 2026-11-24
---

# Clustering and high availability: expert

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · **3. Expert** · [4. Quiz](quiz.en.md)

Builds on the [basic](basics.en.md) and [advanced](advanced.en.md) theory. "Verified" means observed on the three-node Docker rig (RabbitMQ 4.4.1).

## Reference

### Commands

| Command | Purpose | Verified |
|---|---|---|
| `rabbitmqctl stop_app` / `start_app` | stop/start the RabbitMQ application inside a node | yes |
| `rabbitmqctl join_cluster <node>` | join a stopped node | yes |
| `rabbitmqctl cluster_status` | membership, versions, maintenance status | yes |
| `rabbitmqctl list_queues name type leader members online` | where queues live | yes |
| `rabbitmqctl set_cluster_name <name>` | set the cluster name | invoked |
| `rabbitmq-queues quorum_status <queue>` | Raft replica state | yes |
| `rabbitmq-upgrade drain` / `revive` | maintenance mode | yes |
| `rabbitmq-diagnostics check_if_node_is_quorum_critical` | a node's criticality for quorum | yes |
| `rabbitmqctl forget_cluster_node`, `rabbitmq-queues add_member`/`delete_member`/`grow`/`shrink`, `reset` | changing cluster and replica membership | not tested |

### Failure matrix (three-node rig)

| Situation | Classic (on the failed node) | Quorum (3 replicas) | Verified |
|---|---|---|---|
| 1 of 3 nodes stopped | `404 … process is stopped by supervisor` | publish `ack`, reading works | yes |
| 2 of 3 nodes stopped | unavailable | publish with no confirm (`timeout`) | yes |
| nodes returned | the persistent message is intact | publish `ack` | yes |

### `quorum_status` fields

| Field | Meaning |
|---|---|
| `Raft State` | `leader` / `follower` |
| `Membership` | `voter` |
| `Last Log Index`, `Last Written`, `Last Applied`, `Commit Index` | progress of the Raft log |
| `Snapshot Index` | snapshot position |
| `Term` | the "epoch" number of leader elections |
| `Machine Version` | version of the queue state machine |

## Internals

- **Metadata is shared in a cluster,** queue data is not: classic lives on one node, quorum on replicas with a leader.
- **A write to a quorum queue** is confirmed once a majority of replicas accept it (seen indirectly: with 1 of 3 nodes there is no confirm, with 2 of 3 there is).
- **A queue's leader can be on any node:** in the test the quorum queue's leader was first on `rmq1` and ended up on `rmq2` after the failures and returns.
- **Maintenance mode** takes a node out of client service and hands off leadership.
- **The client's entry node forwards** operations to the node where the queue lives (verified with a classic queue's depth).

> ⚠️ Verify: details of Raft leader election, behaviour under a network partition, `cluster_partition_handling`, peer discovery and running a cluster on Kubernetes were not verified here.

## Gotchas

### 1. Classic is not replicated

A node failure makes a classic queue unavailable (`404 … process is stopped by supervisor`); messages return with the node if they are persistent (verified).

### 2. Two of three nodes is the boundary

Stopping two of three nodes stops writes to a quorum queue (`timeout`).

### 3. A timeout, not an error

Without a majority a publish is not rejected, it is just never confirmed: the client must wait for the confirm with a timeout ([module 05](../05-reliability/expert.en.md#gotchas)).

### 4. The cookie

Cluster nodes must share an Erlang cookie; it is a secret.

### 5. The order `stop_app` → `join_cluster` → `start_app`

The documentation requires stopping the application (`stop_app`) on the joining node. We also ran `join_cluster` without it on a fresh node: the command printed only `Clustering node …`, and we did not establish the final result without `stop_app`. Follow the documented order.

> ⚠️ Verify: what exactly `join_cluster` does without a prior `stop_app` was not established here.

### 6. An even number of nodes

Four nodes survive as many failures as three (the majority is 3 of 4).

## Trade-offs and alternatives

| Choice | Pros | Cons |
|---|---|---|
| A three-node cluster | survives one node failure | three nodes, more resources |
| One node | simple | a single point of failure |
| Federation / Shovel | links independent brokers | not a cluster, see the [roadmap](../../overview.en.md) |
| A Kubernetes operator | automation | an extra layer ([roadmap](../../overview.en.md)) |

## Interview questions

<details><summary>1. What do cluster nodes share?</summary>

Metadata: users, vhosts, exchanges, queues, bindings. Queue data lives on particular nodes.

→ [Basics](basics.en.md#what-a-cluster-is)

</details>

<details><summary>2. Trick: is a classic queue replicated in a three-node cluster?</summary>

No: it lives on one node and is unavailable when that node fails.

→ [Gotcha 1](#1-classic-is-not-replicated)

</details>

<details><summary>3. How many failures does a three/five-node cluster survive?</summary>

One / two: a majority is needed.

→ [Advanced theory](advanced.en.md#how-many-nodes-you-need)

</details>

<details><summary>4. What happens to a publish if there is no majority?</summary>

No confirm (`timeout`); the message is not accepted.

→ [Basics](basics.en.md#majority-and-quorum)

</details>

<details><summary>5. Why maintenance mode?</summary>

To take a node out of service safely before an upgrade or reboot: it stops serving clients and hands off leadership.

→ [Advanced theory](advanced.en.md#maintenance-mode)

</details>

<details><summary>6. Trick: is a two-node cluster more reliable than one node?</summary>

No: the failure of either node removes the majority.

→ [Advanced theory](advanced.en.md#how-many-nodes-you-need)

</details>

<details><summary>7. How do you know whether a node can be stopped?</summary>

`rabbitmq-diagnostics check_if_node_is_quorum_critical`.

→ [Advanced theory](advanced.en.md#quorum-criticality-of-a-node)

</details>

<details><summary>8. How do you join a node to a cluster?</summary>

`stop_app`, `join_cluster rabbit@first`, `start_app`.

→ [Basics](basics.en.md#what-a-cluster-is)

</details>

<details><summary>9. Trick: a client is connected to `rmq2`, the classic queue is on `rmq1`. Does it work?</summary>

Yes, `rmq2` forwards the operations (the depth was read); but if `rmq1` fails the queue is unavailable.

→ [Basics](basics.en.md#where-a-queue-lives)

</details>

<details><summary>10. How do you see where a quorum queue's leader is?</summary>

`rabbitmqctl list_queues name type leader members` or `rabbitmq-queues quorum_status`.

→ [Reference](#commands)

</details>

<details><summary>11. Why an odd number of nodes?</summary>

An even number adds no fault tolerance: 4 nodes survive as many failures as 3.

→ [Gotcha 6](#6-an-even-number-of-nodes)

</details>

<details><summary>12. Trick: after the node returned, was the classic queue's persistent message lost?</summary>

No: `classic message back: 1`.

→ [Basics](basics.en.md#a-node-failure)

</details>

## Cheat sheet

```text
join      : stop_app ; join_cluster rabbit@first ; start_app
status    : cluster_status ; list_queues name type leader members ; rabbitmq-queues quorum_status Q
maintain  : check_if_node_is_quorum_critical ; rabbitmq-upgrade drain ; ... ; revive
majority  : nodes/2 + 1  (3 -> 2, 5 -> 3)
```

- Quorum for what matters, three or more odd nodes.
- No majority means no writes: wait for the confirm with a timeout.

## Related materials

- Previous level: [advanced theory](advanced.en.md)
- Before: [07 · Queue types](../07-queue-types/expert.en.md)
- Next module: [09 · Monitoring and operations](../09-monitoring/overview.en.md)

## Sources

- [Clustering Guide](https://www.rabbitmq.com/docs/clustering)
- [Quorum Queues](https://www.rabbitmq.com/docs/quorum-queues)
- [Upgrading RabbitMQ](https://www.rabbitmq.com/docs/upgrade)
