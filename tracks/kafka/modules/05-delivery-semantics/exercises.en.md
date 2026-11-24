---
title: "Exercises: delivery semantics"
lang: en
track: kafka
kind: module
doc: exercises
slug: 05-delivery-semantics
part: 2
tags: [kafka, exactly-once, transactions, isolation-level, fencing]
status: draft
updated: 2026-11-24
---

# Exercises: delivery semantics

🌐 [Русский](exercises.ru.md) · **English**

> Solve on your own; hints and solutions are under spoilers. You need a disposable `apache/kafka:4.3.1` broker and `confluent-kafka`. The solutions were run on Apache Kafka 4.3.1.

## 1. 🟢 Commit and abort

**Task:** in one topic write two messages in a transaction, then abort a transaction with two messages (with `flush()` before the abort), then commit a transaction with one. Print `(low, high)`.

**Expected output:** `(0, 8)`.

<details><summary>Hint</summary>

Commit and abort markers take offsets.

</details>

<details><summary>Solution</summary>

See the [basics](basics.en.md#producer-transactions).

</details>

## 2. 🟢 Read modes

**Task:** read the topic from exercise 1 with `read_committed` and `read_uncommitted`. Compare the results.

**Expected output:** the first — three committed records; the second — plus the two aborted ones.

<details><summary>Hint</summary>

The consumer parameter `isolation.level`; a read window of at least 5–8 seconds.

</details>

<details><summary>Solution</summary>

See the [basics](basics.en.md#producer-transactions).

</details>

## 3. 🟡 An open transaction

**Task:** open a transaction, write the message `in-flight` with `flush()` and, without committing, read the topic in both modes. Then commit and read again.

**Expected output:** `in-flight` is visible only in `read_uncommitted`, and after `commit_transaction()` in `read_committed` too.

<details><summary>Hint</summary>

Read before `commit_transaction()` and after.

</details>

<details><summary>Solution</summary>

See the [basics](basics.en.md#an-open-transaction).

</details>

## 4. 🟡 An "exactly once" pipeline

**Task:** a topic `in` with six records. Implement a read→upper-case→write-to-`out` loop in batches of three inside a transaction with `send_offsets_to_transaction`. Simulate a failure of the first batch (an abort) and retry.

**Expected output:** `out` (in `read_committed`) holds six records `M0…M5`, each once.

<details><summary>Hint</summary>

The offset to commit is the last record's `offset + 1`; the consumer has `enable.auto.commit=False`, `isolation.level=read_committed`.

</details>

<details><summary>Solution</summary>

See the [advanced theory](advanced.en.md#consume-process-produce-exactly-once).

</details>

## 5. 🔴 A zombie

**Task:** create two producers with one `transactional.id` and call `init_transactions()` on both. Try to run a transaction with the first. Which error is it and what do you do about it?

**Expected output:** `_FENCED`, `fatal() == True`; restart the process.

<details><summary>Hint</summary>

The error is a `KafkaException`; the code is `e.args[0].name()`.

</details>

<details><summary>Solution</summary>

See the [advanced theory](advanced.en.md#zombies-and-fencing).

</details>
