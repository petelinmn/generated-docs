---
title: "Queue types"
lang: en
track: rabbitmq
kind: module
doc: overview
slug: 07-queue-types
part: 2
levels: [1, 2, 3, 4]
tags: [rabbitmq, queue-types, classic-queue, quorum-queue, stream, delivery-limit]
versions: "RabbitMQ 4.4.1; AMQP 0-9-1; Python pika 1.5.0"
status: draft
updated: 2026-11-24
---

# Queue types

🌐 [Русский](overview.ru.md) · **English**

## In short

RabbitMQ 4.4 has three queue types, chosen by the `x-queue-type` argument: **classic** (the default, one node), **quorum** (a replicated consensus-based queue for data that must not be lost) and **stream** (a non-destructive log read by offset). Each has its own limits, and a queue's type cannot be changed.

## Key ideas

- **The type is set at declaration**; redeclaring with another type gives `406`.
- **Quorum:** durable only (else `541`), no `exclusive` or `x-max-priority` (`406`), `x-message-ttl` and a DLX are allowed.
- **Stream:** reading does not delete messages; it needs `basic_qos` and a manual ack (`auto_ack` — `406`); `x-stream-offset`: `first`, `last`, `next`, a number.
- **`last` is the last chunk**, not the last message.
- **The delivery limit (`x-delivery-limit`)** counts `reject(requeue=True)` and a channel crash; **`nack(requeue=True)` was not counted in the test** — the message went in circles.
- **Replication was not verified** (one node): see [module 08](../08-clustering/overview.en.md).

## Minimal example

```python
ch.queue_declare("qq", durable=True, arguments={
    "x-queue-type": "quorum", "x-delivery-limit": 3,
    "x-dead-letter-exchange": "", "x-dead-letter-routing-key": "qqd"})
```

Everything was run against a real RabbitMQ 4.4.1 in Docker (see the [basic theory](basics.en.md)).

## When to use and when not

| Task | Type |
|---|---|
| Simple fast queues | classic |
| Tasks that must not be lost on a node failure | quorum |
| Events with re-reading and many consumers | stream |

- ❌ Do not rely on `nack(requeue=True)` + a delivery limit.
- ❌ Do not try to change an existing queue's type.

## Material levels

| Level | What is inside |
|---|---|
| [1. Basic theory](basics.en.md) | three types, limits, a stream as a log |
| [2. Advanced theory](advanced.en.md) | the delivery limit, stream offsets |
| [3. Expert theory](expert.en.md) | reference of features per type, gotchas, interview questions |
| [4. Self-check](quiz.en.md) | quiz and [exercises](exercises.en.md) |
| 5. Runnable examples | not created yet — `/learn rabbitmq 07-queue-types level 5` |
| 6. Tests | not created yet — `/learn rabbitmq 07-queue-types level 6` |

## Related materials

- Prerequisites: [03 · Queues and delivery](../03-queues-and-delivery/overview.en.md), [06 · Dead-lettering and retries](../06-dead-lettering-and-retries/overview.en.md)
- Next: [08 · Clustering and high availability](../08-clustering/overview.en.md)
- Analogues in other tracks: [C# background jobs](../../../csharp/recipes/background-jobs/overview.en.md)
