---
title: "Exercises: reliability"
lang: en
track: rabbitmq
kind: module
doc: exercises
slug: 05-reliability
part: 2
tags: [rabbitmq, publisher-confirms, mandatory, idempotency]
status: draft
updated: 2026-11-24
---

# Exercises: reliability

🌐 [Русский](exercises.ru.md) · **English**

> Solve on your own; hints and solutions are under spoilers. The solutions were run against a real RabbitMQ 4.4.1 with pika 1.5.0 using the `Confirmed` helper from the [basic theory](basics.en.md#how-the-code-was-verified). Delete the queues and exchanges you create.

## 1. 🟢 The first confirm

**Task:** declare a durable queue `rel`, enable confirms and publish a persistent message. Print the broker's verdict.

**Expected output:** `ack` (the time depends on the machine).

<details><summary>Hint</summary>

`pub = Confirmed(ch)`, then `pub.publish("", "rel", b"a", pika.BasicProperties(delivery_mode=2))`.

</details>

<details><summary>Solution</summary>

See the code in the [basic theory](basics.en.md#publisher-confirms).

</details>

## 2. 🟢 Mandatory

**Task:** create a direct exchange `d` with no bindings and publish to it twice: with `mandatory=True` and without. Print the verdict and the returns list for each case.

**Expected output:**

```text
unroutable mandatory: ack [(312, 'NO_ROUTE')]
unroutable no mandatory: ack []
```

<details><summary>Hint</summary>

Clear `pub.returned` between the attempts.

</details>

<details><summary>Solution</summary>

See the code in the [basic theory](basics.en.md#mandatory-and-basicreturn).

</details>

## 3. 🟡 A nack from a full queue

**Task:** declare a queue `lim` with `x-max-length: 1` and `x-overflow: reject-publish` and publish two messages. Print both verdicts.

**Expected output:** `first: ack second: nack`.

<details><summary>Hint</summary>

The second message does not fit because the queue is full.

</details>

<details><summary>Solution</summary>

See the code in the [advanced theory](advanced.en.md#a-nack-from-the-broker).

</details>

## 4. 🟡 An idempotent consumer

**Task:** publish three messages with the same `message_id="order-42"` to a queue `idem` and write a consumer that "does the work" only once.

**Expected output:** `processed: ['order-42']`.

<details><summary>Hint</summary>

Keep a set of seen `message_id`s; ack **all** three messages.

</details>

<details><summary>Solution</summary>

See the code in the [advanced theory](advanced.en.md#idempotent-consumer).

</details>

## 5. 🔴 Pipelined confirms

**Task:** publish 300 messages without waiting for a confirm after each and wait until the callback has counted 300 `ack`s. Compare the time with the one-by-one variant.

**Expected output:** all 300 confirmed; the pipeline is orders of magnitude faster (numbers depend on the environment: 0.048 s against 13.4 s in the check).

<details><summary>Hint</summary>

`multiple=True` means "all tags up to and including the given one"; count `delivery_tag - acked`.

</details>

<details><summary>Solution</summary>

See the code in the [advanced theory](advanced.en.md#throughput). For production code keep a set of unconfirmed tags (from `on_publish`) and republish on `nack` or a disconnect.

</details>
