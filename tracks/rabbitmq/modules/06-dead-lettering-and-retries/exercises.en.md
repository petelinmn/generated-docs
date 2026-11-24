---
title: "Exercises: dead-lettering and retries"
lang: en
track: rabbitmq
kind: module
doc: exercises
slug: 06-dead-lettering-and-retries
part: 2
tags: [rabbitmq, dead-letter-exchange, x-death, retry]
status: draft
updated: 2026-11-24
---

# Exercises: dead-lettering and retries

🌐 [Русский](exercises.ru.md) · **English**

> Solve on your own; hints and solutions are under spoilers. The solutions were run against a real RabbitMQ 4.4.1 with pika 1.5.0 using the helpers `get` and `show` from the [basic theory](basics.en.md#how-the-code-was-verified). Delete the queues and exchanges you create.

## 1. 🟢 A rejected message

**Task:** create a work queue `work` with a DLX `dlx` (key `dead`) and a queue `dead`. Publish a message, get and reject it (`requeue=False`), then read it from `dead` and print the reason from `x-death`.

**Expected output:** reason `rejected`, queue `work`, routing key `dead`.

<details><summary>Hint</summary>

`x-dead-letter-exchange` and `x-dead-letter-routing-key` are arguments of the queue `work`.

</details>

<details><summary>Solution</summary>

See the code in the [basic theory](basics.en.md#setup).

</details>

## 2. 🟢 No DLX

**Task:** repeat exercise 1 for a queue **without** a DLX and print its depth after `reject`.

**Expected output:** `0`.

<details><summary>Hint</summary>

Wait 0.3 s before `queue_declare(passive=True)`.

</details>

<details><summary>Solution</summary>

See the [basic theory](basics.en.md#without-a-dlx-the-message-vanishes).

</details>

## 3. 🟡 Reasons

**Task:** make a message "die" in two other ways: by a queue TTL of 200 ms and by `x-max-length: 1`. Print the reasons.

**Expected output:** `expired` and `maxlen`; with `maxlen` the message `first` reaches the DLX.

<details><summary>Hint</summary>

A queue with `x-max-length: 1` and two publishes evicts the first.

</details>

<details><summary>Solution</summary>

See the [basic theory](basics.en.md#other-reasons).

</details>

## 4. 🟡 A delayed retry

**Task:** build queues `main` (DLX `rx`, key `retry`), `retry` (TTL 300 ms, default DLX, key `main`) and `parking`. The consumer rejects the message until the rejection count reaches three, then moves it to `parking`. Print the attempt times.

**Expected output:** four attempts about 0.3 s apart, one message in `parking`.

<details><summary>Hint</summary>

The rejection count is the `count` of the `x-death` record with queue `main`.

</details>

<details><summary>Solution</summary>

See the code in the [advanced theory](advanced.en.md#a-delayed-retry).

</details>

## 5. 🔴 A DLX with no route

**Task:** declare a queue with `x-dead-letter-exchange: "nowhere"` (the exchange does not exist), reject a message in it and print the depth and the channel state. Suggest how to notice such a loss in production.

**Expected output:** depth `0`, channel open.

<details><summary>Hint</summary>

The broker reports no error; an external signal is needed.

</details>

<details><summary>Solution</summary>

The check showed: `orphan DLX depth: 0 open: True`. You can notice the loss by creating the topology (the DLX, the inspection queue and the binding) at application start and reconciling counts: published, confirmed, processed, in the inspection queue. More in the [advanced theory](advanced.en.md#a-dlx-with-no-route).

</details>
