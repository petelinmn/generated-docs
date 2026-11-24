---
title: "Exercises: clustering and high availability"
lang: en
track: rabbitmq
kind: module
doc: exercises
slug: 08-clustering
part: 2
tags: [rabbitmq, cluster, quorum, high-availability, maintenance-mode]
status: draft
updated: 2026-11-24
---

# Exercises: clustering and high availability

🌐 [Русский](exercises.ru.md) · **English**

> Solve on your own; hints and solutions are under spoilers. You need Docker and the three-node rig from the [basic theory](basics.en.md#how-the-code-was-verified) (a disposable teaching setup; at the end remove the containers and the network: `docker rm -f rmq1 rmq2 rmq3; docker network rm rmqnet`).

## 1. 🟢 Build a cluster

**Task:** start three containers, join `rmq2` and `rmq3` to `rmq1` and print `cluster_status`.

**Expected output:** `Running Nodes` lists three nodes `rabbit@rmq1`, `rabbit@rmq2`, `rabbit@rmq3`.

<details><summary>Hint</summary>

On the joining nodes: `stop_app`, `join_cluster rabbit@rmq1`, `start_app`; all share the same `RABBITMQ_ERLANG_COOKIE`.

</details>

<details><summary>Solution</summary>

The commands are in the [basics](basics.en.md#what-a-cluster-is).

</details>

## 2. 🟢 Where queues live

**Task:** declare a classic queue `cl` on `rmq1` and a quorum queue `qu`. Print `rabbitmqctl list_queues name type leader members`.

**Expected output:** `cl` has one member `rabbit@rmq1`, `qu` has three members and a leader.

<details><summary>Hint</summary>

Connect to port 5681 (node `rmq1`).

</details>

<details><summary>Solution</summary>

See the output in the [basics](basics.en.md#where-a-queue-lives).

</details>

## 3. 🟡 One node fails

**Task:** publish a message to each queue, stop `rmq1` and connect to `rmq3`. Check the availability of `cl` and `qu`, publish and read messages from `qu`. Then start `rmq1` and check `cl`.

**Expected output:** `cl` is unavailable (404), `qu` works; after `rmq1` starts the message in `cl` is intact.

<details><summary>Hint</summary>

Publish to `cl` with `delivery_mode=2`; after `docker start rmq1` wait for the node to come up.

</details>

<details><summary>Solution</summary>

See the [basics](basics.en.md#a-node-failure).

</details>

## 4. 🟡 Losing the majority

**Task:** stop `rmq1` and `rmq2` and publish to `qu` with a confirm and a 6 s timeout. Then bring the nodes back and repeat.

**Expected output:** `timeout`, and after recovery `ack`.

<details><summary>Hint</summary>

Use the `Pub` helper from [module 05](../05-reliability/basics.en.md#how-the-code-was-verified).

</details>

<details><summary>Solution</summary>

See the [basics](basics.en.md#majority-and-quorum).

</details>

## 5. 🔴 Safe maintenance

**Task:** run `check_if_node_is_quorum_critical` on `rmq3`, then `rabbitmq-upgrade drain`, check `cluster_status` and bring the node back with `revive`.

**Expected output:** the node is "not critical", then `marked for maintenance`, then `not under maintenance` again.

<details><summary>Hint</summary>

The status is the `Maintenance status` section of `cluster_status`.

</details>

<details><summary>Solution</summary>

See the [advanced theory](advanced.en.md#maintenance-mode).

</details>
