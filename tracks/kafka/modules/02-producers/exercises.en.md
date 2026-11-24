---
title: "Exercises: producers"
lang: en
track: kafka
kind: module
doc: exercises
slug: 02-producers
part: 1
tags: [kafka, producer, partitioning, acks, idempotence, compression]
status: draft
updated: 2026-11-24
---

# Exercises: producers

🌐 [Русский](exercises.ru.md) · **English**

> Solve on your own; hints and solutions are under spoilers. You need a disposable `apache/kafka:4.3.1` broker and `confluent-kafka`. The solutions were run on Apache Kafka 4.3.1. Your numbers will differ.

## 1. 🟢 An explicit partition

**Task:** send three records with one key, giving `partition=2`. Print the partitions from the callbacks.

**Expected output:** `[2, 2, 2]`.

<details><summary>Hint</summary>

`produce(topic, key=…, value=…, partition=2, on_delivery=cb)`.

</details>

<details><summary>Solution</summary>

See the [basics](basics.en.md#an-explicit-partition).

</details>

## 2. 🟢 What `acks=0` does

**Task:** send 20,000 records with `acks=0`, `1` and `all` and print the offset from the report of the first records.

**Expected output:** with `acks=0` the offset is `None`, with `1` and `all` numbers.

<details><summary>Hint</summary>

Collect `msg.offset()` in the callback.

</details>

<details><summary>Solution</summary>

See the [basics](basics.en.md#broker-acknowledgements-acks).

</details>

## 3. 🟡 Idempotence

**Task:** create a producer with `enable.idempotence=True` and `acks=1`, then with `acks=all`. Print the result.

**Expected output:** the first attempt — a `KafkaException` with `_INVALID_ARG`; the second — success.

<details><summary>Hint</summary>

The error occurs when creating the `Producer`, not when sending.

</details>

<details><summary>Solution</summary>

See the [advanced theory](advanced.en.md#the-idempotent-producer).

</details>

## 4. 🟡 The price of `linger.ms`

**Task:** measure the delivery latency of one record at `linger.ms=0` and `100` (five measurements each).

**Expected output:** about 1 ms against about 100 ms.

<details><summary>Hint</summary>

`produce` with `on_delivery`, then `poll(0.001)` until it fires.

</details>

<details><summary>Solution</summary>

See the [advanced theory](advanced.en.md#batches-and-delay-lingerms).

</details>

## 5. 🔴 Delivery errors

**Task:** (a) send a 2 MB record; (b) send a record with a producer connected to a nonexistent broker (`localhost:1`, `message.timeout.ms=3000`). Determine where and with what code each error shows.

**Expected output:** (a) `MSG_SIZE_TOO_LARGE` at `produce`; (b) `_MSG_TIMED_OUT` in the callback.

<details><summary>Hint</summary>

Wrap `produce` in `try/except KafkaException`; for (b) call `flush(10)`.

</details>

<details><summary>Solution</summary>

See the [basics](basics.en.md#errors).

</details>
