---
title: "Exercises: consumers and groups"
lang: en
track: kafka
kind: module
doc: exercises
slug: 03-consumers-and-groups
part: 1
tags: [kafka, consumer, consumer-group, offset-commit, rebalance]
status: draft
updated: 2026-11-24
---

# Exercises: consumers and groups

🌐 [Русский](exercises.ru.md) · **English**

> Solve on your own; hints and solutions are under spoilers. You need a disposable `apache/kafka:4.3.1` broker and `confluent-kafka`. The solutions were run on Apache Kafka 4.3.1; a topic `g1` with three partitions and 30 records.

## 1. 🟢 Dividing partitions

**Task:** start one consumer of a group, then a second, and print each one's assigned partitions.

**Expected output:** one member — `[0, 1, 2]`; two — `[0, 1]` and `[2]` (the order may differ).

<details><summary>Hint</summary>

`pump` is a loop of `poll(0.2)` on each consumer; a rebalance takes a few seconds.

</details>

<details><summary>Solution</summary>

See the [basics](basics.en.md#a-consumer-group).

</details>

## 2. 🟢 An extra consumer

**Task:** add two more consumers to the group (four in total) and print the assignments.

**Expected output:** one of the four has no partitions.

<details><summary>Hint</summary>

A partition is assigned to one group member.

</details>

<details><summary>Solution</summary>

See the [basics](basics.en.md#more-consumers-than-partitions).

</details>

## 3. 🟡 Committing

**Task:** read nine records (three per partition) with a group that has auto-commit off. Print `committed` before and after `commit`.

**Expected output:** `-1001` on each partition before, `3` after.

<details><summary>Hint</summary>

`c.committed([TopicPartition(t, p) for p in range(3)], timeout=10)`.

</details>

<details><summary>Solution</summary>

See the [basics](basics.en.md#committing-offsets).

</details>

## 4. 🟡 A repeat without a commit

**Task:** read six records, close the consumer without `commit` and start it again in the same group. Compare the records.

**Expected output:** the same six records.

<details><summary>Hint</summary>

`enable.auto.commit=False`.

</details>

<details><summary>Solution</summary>

See the [basics](basics.en.md#what-happens-without-a-commit).

</details>

## 5. 🔴 Cooperative rebalancing

**Task:** compare consumer A's `on_revoke` events when B joins under the default strategy and `cooperative-sticky`.

**Expected output:** by default all of A's partitions are revoked; with `cooperative-sticky` only the one moving to B.

<details><summary>Hint</summary>

`partition.assignment.strategy=cooperative-sticky`; record the events in a list.

</details>

<details><summary>Solution</summary>

See the [advanced theory](advanced.en.md#cooperative-rebalancing).

</details>
