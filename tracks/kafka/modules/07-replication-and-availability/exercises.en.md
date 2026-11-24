---
title: "Exercises: replication and availability"
lang: en
track: kafka
kind: module
doc: exercises
slug: 07-replication-and-availability
part: 2
tags: [kafka, replication, isr, min-insync-replicas, leader-election]
status: draft
updated: 2026-11-24
---

# Exercises: replication and availability

🌐 [Русский](exercises.ru.md) · **English**

> Solve on your own; hints and solutions are under spoilers. You need a disposable cluster of three Apache Kafka 4.3.1 brokers in Docker (combined KRaft nodes, see [how the code was verified](basics.en.md#how-the-code-was-verified)) and `confluent-kafka`. Afterwards: `docker rm -f kafka1 kafka2 kafka3; docker network rm kfk`.

## 1. 🟢 The leader layout

**Task:** create a topic of three partitions with RF=3 and `min.insync.replicas=2`. Print the leaders, replicas and ISR.

**Expected output:** leaders spread across brokers, each partition's ISR holds three brokers.

<details><summary>Hint</summary>

`kafka-topics.sh --describe --topic T` or `AdminClient.list_topics`.

</details>

<details><summary>Solution</summary>

See the [basics](basics.en.md#replication).

</details>

## 2. 🟢 A leader failure

**Task:** stop the broker that leads partition 0 and after 8 seconds look at the leader and ISR. Write records with `acks=all`.

**Expected output:** a new leader, an ISR of two brokers, the write succeeds.

<details><summary>Hint</summary>

`docker stop kafkaN`; connect to the live brokers.

</details>

<details><summary>Solution</summary>

See the [basics](basics.en.md#a-leader-failure).

</details>

## 3. 🟡 Too few replicas

**Task:** stop a second broker. Write records with `acks=all` and `acks=1` (`message.timeout.ms=15000`).

**Expected output:** `acks=all` — `_MSG_TIMED_OUT`; `acks=1` — `ok`.

<details><summary>Hint</summary>

Connect to the remaining broker.

</details>

<details><summary>Solution</summary>

See the [basics](basics.en.md#acks-and-mininsyncreplicas).

</details>

## 4. 🟡 Recovery

**Task:** start the stopped brokers and after 25 seconds print the leaders and ISR of all partitions.

**Expected output:** a full ISR; leaders did not necessarily return to the preferred ones.

<details><summary>Hint</summary>

`--under-replicated-partitions` must be empty.

</details>

<details><summary>Solution</summary>

See the [basics](basics.en.md#recovery).

</details>

## 5. 🔴 The controller quorum

**Task:** leave one broker of three alive and try to create a topic with an 8-second timeout. Then start the others.

**Expected output:** a timeout; after the start the quorum recovered.

<details><summary>Hint</summary>

`create_topics([...], request_timeout=8, operation_timeout=8)`.

</details>

<details><summary>Solution</summary>

See the [advanced theory](advanced.en.md#the-controller-quorum).

</details>
