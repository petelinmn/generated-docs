---
title: "Exercises: flow control and performance"
lang: en
track: rabbitmq
kind: module
doc: exercises
slug: 10-performance
part: 3
tags: [rabbitmq, performance, prefetch, publisher-confirms, disk-alarm]
status: draft
updated: 2026-11-24
---

# Exercises: flow control and performance

🌐 [Русский](exercises.ru.md) · **English**

> Solve on your own; hints and solutions are under spoilers. Your numbers will differ: compare the **ratios**. Work on a disposable broker; restore disk thresholds afterwards.

## 1. 🟢 The price of confirms

**Task:** publish 20,000 messages of 100 bytes (a) without confirms and (b) with pipelined confirms. Compare the speed.

**Expected output:** (a) noticeably higher (in the check 62k against 40k messages/s).

<details><summary>Hint</summary>

`confirm_delivery(callback)` and counting `ack`s with `multiple` in mind ([module 05](../05-reliability/advanced.en.md#throughput)).

</details>

<details><summary>Solution</summary>

See the [basics](basics.en.md#publishing).

</details>

## 2. 🟢 Prefetch

**Task:** publish 10,000 persistent messages and read them with a consumer using `prefetch_count` 1, 10, 100, 1000, acking each. Print the speed.

**Expected output:** speed rises with prefetch (in the check 1.8 → 9.9 → 24.2 → 29.4k messages/s).

<details><summary>Hint</summary>

`basic_qos(prefetch_count=N)` before `basic_consume`.

</details>

<details><summary>Solution</summary>

See the [basics](basics.en.md#reading-prefetch-and-acks).

</details>

## 3. 🟡 Multiple acks

**Task:** repeat the reading with prefetch 1000, acking every hundredth message with `multiple=True`. Compare with acking each.

**Expected output:** about twice as fast (66.8 against 29.4k messages/s in the check).

<details><summary>Hint</summary>

Ack when `cnt % 100 == 0` and at the end.

</details>

<details><summary>Solution</summary>

See the [advanced theory](advanced.en.md#multiple-acks).

</details>

## 4. 🟡 Message size

**Task:** publish 3000 persistent messages each of 100 B, 10 KB and 100 KB with confirms. Print messages/s and MB/s.

**Expected output:** messages/s falls, MB/s rises.

<details><summary>Hint</summary>

MB/s = `n * size / time / 1e6`.

</details>

<details><summary>Solution</summary>

See the [basics](basics.en.md#message-size).

</details>

## 5. 🔴 A disk alarm

**Task:** on a disposable broker find the free space, set `rabbitmqctl set_disk_free_limit` above it, publish a message and catch the `Connection.Blocked` reason. Restore `50000000`.

**Expected output:** `['low on disk']`, then `reported no alarms`.

<details><summary>Hint</summary>

Free space is visible in the metric `rabbitmq_disk_space_available_bytes`. The limit must be above it.

</details>

<details><summary>Solution</summary>

See the [advanced theory](advanced.en.md#the-disk-alarm). Do not leave a large limit on the broker.

</details>
