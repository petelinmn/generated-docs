---
title: "Exercises: queue types"
lang: en
track: rabbitmq
kind: module
doc: exercises
slug: 07-queue-types
part: 2
tags: [rabbitmq, queue-types, quorum-queue, stream, delivery-limit]
status: draft
updated: 2026-11-24
---

# Exercises: queue types

🌐 [Русский](exercises.ru.md) · **English**

> Solve on your own; hints and solutions are under spoilers. The solutions were run on a single RabbitMQ 4.4.1 node with pika 1.5.0. Delete the queues you create.

## 1. 🟢 Three types

**Task:** declare one durable queue of each type (`qc`, `qq`, `qs`) and print their types with `docker exec rabbit rabbitmqctl list_queues name type`.

**Expected output:** `qc classic`, `qq quorum`, `qs stream`.

<details><summary>Hint</summary>

`arguments={"x-queue-type": "quorum"}` and `"stream"`; classic needs no argument.

</details>

<details><summary>Solution</summary>

See the code in the [basics](basics.en.md#three-queue-types).

</details>

## 2. 🟢 Limits

**Task:** try to declare a quorum queue (a) with `exclusive=True`, (b) with `x-max-priority: 5`, (c) with `durable=False`. Record the code and error type for each case; make each attempt on a fresh connection.

**Expected output:** (a) `406` (channel), (b) `406` (channel), (c) `541` (connection).

<details><summary>Hint</summary>

A channel-level error closes the channel, a `541` closes the whole connection.

</details>

<details><summary>Solution</summary>

See the table in the [basics](basics.en.md#limits-of-quorum-queues-and-streams) and the `try_declare` function: it opens a separate connection, declares the queue and prints `type(e).__name__` and the start of the error text.

</details>

## 3. 🟡 Reading a stream

**Task:** publish `e0…e4` to a stream, read from `first`, then subscribe a second consumer with `next` and publish `e5`. Then read the log again from the start.

**Expected output:**

```text
stream first: ['e0', 'e1', 'e2', 'e3', 'e4']
stream next: ['e5']
stream re-read: ['e0', 'e1', 'e2', 'e3', 'e4', 'e5']
```

<details><summary>Hint</summary>

You need `basic_qos(prefetch_count=10)` and a manual `basic_ack`.

</details>

<details><summary>Solution</summary>

See the code in the [basics](basics.en.md#stream-a-log-instead-of-a-queue).

</details>

## 4. 🟡 Offsets

**Task:** publish six messages and read the stream with offsets `first`, `last`, `next` and `3`. Explain the result for `last`.

**Expected output:** `first` and `last` — all six, `next` — empty, `3` — `e3, e4, e5`.

<details><summary>Hint</summary>

`last` starts at the last chunk.

</details>

<details><summary>Solution</summary>

See the [advanced theory](advanced.en.md#stream-offsets).

</details>

## 5. 🔴 Delivery limit: reject versus nack

**Task:** create a quorum queue with `x-delivery-limit: 3` and a DLX and publish a message. For each of two variants (`basic_reject(requeue=True)` and `basic_nack(requeue=True)`) return the message via `basic_get` up to 8 times and print the number of deliveries and the depth of the inspection queue.

**Expected output:** for `reject` 4 deliveries and 1 message in the inspection queue; for `nack` 8 deliveries (we stopped) and 0.

<details><summary>Hint</summary>

`time.sleep(0.15)` between attempts; recreate the queues after each series.

</details>

<details><summary>Solution</summary>

Outputs from the check: `basic_get+reject(requeue): [(0, None, False), (1, 1, True), (2, 2, True), (3, 3, True)]`, `dead: 1`; `A get+nack: deliveries 8 dead 0`. The code is in the [advanced theory](advanced.en.md#delivery-limit).

</details>
