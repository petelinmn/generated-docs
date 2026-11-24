---
title: "Exercises: Kafka basics"
lang: en
track: kafka
kind: module
doc: exercises
slug: 01-kafka-basics
part: 1
tags: [kafka, topic, partition, offset, consumer-group]
status: draft
updated: 2026-11-24
---

# Exercises: Kafka basics

🌐 [Русский](exercises.ru.md) · **English**

> Solve on your own; hints and solutions are under spoilers. You need a disposable `apache/kafka:4.3.1` broker (port 9092) and `confluent-kafka` for Python. The solutions were run on Apache Kafka 4.3.1.

## 1. 🟢 A topic and partitions

**Task:** create a topic `orders` with three partitions and print the partition numbers.

**Expected output:** `create: {'orders': None}` and `[0, 1, 2]`.

<details><summary>Hint</summary>

`AdminClient.create_topics([NewTopic(…)])`, then `list_topics`.

</details>

<details><summary>Solution</summary>

See the code in the [basics](basics.en.md#a-topic-and-partitions).

</details>

## 2. 🟢 Writing with a report

**Task:** publish six records with keys `user0` and `user1` alternating and print the partition and offset of each from the callback.

**Expected output:** each key's records in one partition, offsets 0, 1, 2.

<details><summary>Hint</summary>

`on_delivery=` and `flush()`.

</details>

<details><summary>Solution</summary>

See the code in the [basics](basics.en.md#writing).

</details>

## 3. 🟡 Reading deletes nothing

**Task:** read the topic with two different groups (`earliest`) and make sure the results match.

**Expected output:** `True`.

<details><summary>Hint</summary>

Different `group.id`s; `enable.auto.commit=False`.

</details>

<details><summary>Solution</summary>

See the code in the [basics](basics.en.md#reading-and-offsets).

</details>

## 4. 🟡 Partition bounds

**Task:** print the low and high watermark of each partition.

**Expected output:** `low=0` everywhere; `high` equals the number of records in the partition.

<details><summary>Hint</summary>

`Consumer.get_watermark_offsets(TopicPartition(topic, p), timeout=10)`.

</details>

<details><summary>Solution</summary>

See the [basics](basics.en.md#reading-and-offsets).

</details>

## 5. 🔴 Group positions

**Task:** read the topic with a new group using `earliest`, commit the positions (`commit(asynchronous=False)`), print `kafka-consumer-groups.sh --describe --group …` and read again with the same group. Explain the result.

**Expected output:** `LAG` 0 on every partition; the second read is empty.

<details><summary>Hint</summary>

`auto.offset.reset` applies only without a stored position.

</details>

<details><summary>Solution</summary>

See the [advanced theory](advanced.en.md#consumer-group-positions).

</details>
