---
title: "Exercises: topics, retention and compaction"
lang: en
track: kafka
kind: module
doc: exercises
slug: 04-topics-and-retention
part: 1
tags: [kafka, retention, log-compaction, tombstone, delete-records]
status: draft
updated: 2026-11-24
---

# Exercises: topics, retention and compaction

🌐 [Русский](exercises.ru.md) · **English**

> Solve on your own; hints and solutions are under spoilers. You need a disposable `apache/kafka:4.3.1` broker with accelerated checks (see [how the code was verified](basics.en.md#how-the-code-was-verified)) and `confluent-kafka`. The solutions were run on Apache Kafka 4.3.1.

## 1. 🟢 Retention

**Task:** create a topic with `retention.ms=4000` and `segment.ms=1500`, write five messages and watch `(low, high)` after 8 seconds.

**Expected output:** `(0, 5)`, then `(5, 5)`.

<details><summary>Hint</summary>

`get_watermark_offsets` on a `Consumer` with any `group.id`.

</details>

<details><summary>Solution</summary>

See the [basics](basics.en.md#retention).

</details>

## 2. 🟢 Manual deletion

**Task:** write ten messages and move `low_watermark` to 6 with `kafka-delete-records.sh`.

**Expected output:** `(0, 10)`, then `(6, 10)`.

<details><summary>Hint</summary>

JSON: `{"partitions": [{"topic": "dr", "partition": 0, "offset": 6}], "version": 1}`; put the file into the container with `docker cp`.

</details>

<details><summary>Solution</summary>

See the [basics](basics.en.md#deleting-records-by-hand).

</details>

## 3. 🟡 Compaction

**Task:** create a topic with `cleanup.policy=compact`, update three keys four times each and read the topic after cleaning.

**Expected output:** one latest record per key, with the original offsets (9, 10, 11).

<details><summary>Hint</summary>

You need closed segments: `segment.ms=1000` and a couple of writes after a pause; wait 8–10 seconds.

</details>

<details><summary>Solution</summary>

See the [advanced theory](advanced.en.md#log-compaction).

</details>

## 4. 🟡 A tombstone

**Task:** in a compact topic delete the key `k2` with a record whose value is `None` and read the topic.

**Expected output:** the old values of `k2` are gone, the record `(k2, None)` remains.

<details><summary>Hint</summary>

`produce(topic, key=b"k2", value=None)`.

</details>

<details><summary>Solution</summary>

See the [advanced theory](advanced.en.md#tombstones).

</details>

## 5. 🔴 Adding partitions

**Task:** a topic of three partitions, eight keys `user0…user7`. Write them, raise the number of partitions to six and write again with a **new** producer. Which keys changed partition? What does the old producer do?

**Expected output:** some keys (in the check `user0`, `user2`, `user3`, `user5`) in other partitions; the old producer writes the old way.

<details><summary>Hint</summary>

`AdminClient.create_partitions([NewPartitions(t, 6)])`; keep the "old" producer from before the increase.

</details>

<details><summary>Solution</summary>

See the [advanced theory](advanced.en.md#adding-partitions-and-keys).

</details>
