---
title: "Quiz: clustering and high availability"
lang: en
track: rabbitmq
kind: module
doc: quiz
slug: 08-clustering
part: 2
tags: [rabbitmq, cluster, quorum, high-availability, maintenance-mode]
status: draft
updated: 2026-11-24
---

# Quiz: clustering and high availability

🌐 [Русский](quiz.ru.md) · **English**

> Answer on your own first, then open the answer. The label shows the question level: 🟢 [basic](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 What do all nodes of a cluster share?

- A) The data of all queues
- B) Metadata: users, vhosts, exchanges, queues, bindings
- C) Only the Erlang cookie
- D) Nothing

<details><summary>Answer</summary>

**B.** Queue data depends on the type and lives on particular nodes.
→ [Basics](basics.en.md#what-a-cluster-is)

</details>

## 2. 🟢 What happens to a classic queue when its node fails?

- A) It moves to another node
- B) It becomes unavailable (in the test `404 … process is stopped by supervisor`)
- C) It turns into a quorum queue
- D) Messages are deleted forever

<details><summary>Answer</summary>

**B.** Persistent messages returned with the node.
→ [Basics](basics.en.md#a-node-failure)

</details>

## 3. 🟢 Which queue type survives a node failure?

- A) classic
- B) quorum
- C) exclusive
- D) Any durable

<details><summary>Answer</summary>

**B.**
→ [Basics](basics.en.md#a-node-failure)

</details>

## 4. 🟢 In what order do you join a node?

- A) `join_cluster`, `start_app`
- B) `stop_app`, `join_cluster`, `start_app`
- C) `start_app`, `join_cluster`
- D) `reset`, `stop_app`

<details><summary>Answer</summary>

**B.**
→ [Basics](basics.en.md#what-a-cluster-is)

</details>

## 5. 🟡 A three-node cluster, a quorum queue on all. Two nodes are stopped. What about a publish?

- A) `ack`
- B) `nack`
- C) No confirm (`timeout`)
- D) The channel closes with `406`

<details><summary>Answer</summary>

**C.** There is no majority.
→ [Basics](basics.en.md#majority-and-quorum)

</details>

## 6. 🟡 How many failures does a five-node cluster survive?

- A) 1
- B) 2
- C) 3
- D) 4

<details><summary>Answer</summary>

**B.** The majority is 3 of 5.
→ [Advanced theory](advanced.en.md#how-many-nodes-you-need)

</details>

## 7. 🟡 What does `rabbitmq-upgrade drain` do?

- A) Empties queues
- B) Puts the node into maintenance mode: it no longer serves clients
- C) Removes the node from the cluster
- D) Stops Erlang

<details><summary>Answer</summary>

**B.** `revive` returns the node.
→ [Advanced theory](advanced.en.md#maintenance-mode)

</details>

## 8. 🟡 Choose all true statements

- [ ] A) A client can work with a classic queue through any node
- [ ] B) A two-node cluster survives the failure of one node for quorum queues
- [ ] C) `check_if_node_is_quorum_critical` helps decide whether a node may be stopped
- [ ] D) Four nodes survive as many failures as three

<details><summary>Answer</summary>

**A, C, D.** B is wrong: of two nodes, after one fails there is no majority.
→ [Advanced theory](advanced.en.md)

</details>

## 9. 🟡 What does `rabbitmq-queues quorum_status` show?

- A) Memory metrics
- B) Raft replica state: leader/follower, log indexes, `Term`
- C) A list of connections
- D) The policy configuration

<details><summary>Answer</summary>

**B.**
→ [Advanced theory](advanced.en.md#replica-state)

</details>

## 10. 🔴 Find the mistake

> "For reliability we deployed two RabbitMQ nodes in a cluster and declared a quorum queue."

<details><summary>Answer</summary>

With two nodes the majority is both: the failure of either takes away the queue's ability to accept writes. You need three (or five) nodes.
→ [Advanced theory](advanced.en.md#how-many-nodes-you-need)

</details>

## 11. 🔴 A client gets no `ack` for a publish to a quorum queue with two of three nodes stopped. What should a correct client do?

- A) Wait forever
- B) Treat the message as delivered
- C) Wait for the confirm with a timeout and republish after recovery
- D) Publish to a classic queue

<details><summary>Answer</summary>

**C.** An unconfirmed publish is "unknown"; an idempotent consumer is needed ([module 05](../05-reliability/advanced.en.md)).
→ [Gotcha 3](expert.en.md#3-a-timeout-not-an-error)

</details>

## 12. 🔴 Open question

You must upgrade RabbitMQ in a three-node cluster without downtime. Describe the order of actions and the checks.

<details><summary>Key points of a good answer</summary>

- One node at a time: `check_if_node_is_quorum_critical`, `drain`, stop, upgrade, start, `revive`.
- Make sure the replicas are in sync (`quorum_status`) before the next node.
- Clients can reconnect; publishes use confirms and retries.
- Important queues are quorum; a classic queue on the node being upgraded will be unavailable.
- What was not verified here: version compatibility during an upgrade and a full rolling upgrade.

</details>

→ [Advanced theory](advanced.en.md#maintenance-mode)
