---
title: "Dead-lettering and retries"
lang: en
track: rabbitmq
kind: module
doc: overview
slug: 06-dead-lettering-and-retries
part: 2
levels: [1, 2, 3, 4]
tags: [rabbitmq, dead-letter-exchange, dlx, x-death, retry, poison-message]
versions: "RabbitMQ 4.4.1; AMQP 0-9-1; Python pika 1.5.0"
status: draft
updated: 2026-11-24
---

# Dead-lettering and retries

🌐 [Русский](overview.ru.md) · **English**

## In short

A message that cannot be processed must neither loop nor vanish. A **dead letter exchange (DLX)** is an exchange to which the broker sends a queue's "dead" messages: rejected without requeue (`rejected`), expired (`expired`) and evicted by length (`maxlen`). Inspection queues and delayed retries are built from them.

## Key ideas

- **A DLX is a queue argument** (`x-dead-letter-exchange`, `x-dead-letter-routing-key`); without it a `reject` with `requeue=False` just loses the message.
- **The `x-death` header** keeps the history: queue, reason, `count`.
- **Three verified reasons:** `rejected`, `expired`, `maxlen` (with `drop-head` the oldest "dies").
- **A delayed retry:** `main` → DLX → a `retry` queue with no consumers and a TTL → DLX back to `main`; after an attempt cap — to `parking`.
- **A DLX with no route** loses the message silently, with no error.
- **Count attempts from `x-death`**, filtering by queue and reason.
- **A quorum queue's `x-delivery-limit`** counts `reject(requeue=True)` and a channel crash, but not `nack(requeue=True)`.

## Minimal example

```python
ch.exchange_declare("dlx", "direct")
ch.queue_declare("dead", durable=True); ch.queue_bind("dead", "dlx", "dead")
ch.queue_declare("work", durable=True, arguments={
    "x-dead-letter-exchange": "dlx", "x-dead-letter-routing-key": "dead"})
# a consumer that fails for good does: ch.basic_reject(tag, requeue=False)
```

Everything was run against a real RabbitMQ 4.4.1 in Docker (see the [basic theory](basics.en.md)).

## When to use and when not

| Situation | What to do |
|---|---|
| An unfixable error (format) | `reject(requeue=False)` → DLX → inspection |
| A transient error (network) | a `retry` queue with a TTL, an attempt cap |
| Stale messages | TTL + a DLX for audit |
| A queue overflowing | `x-max-length` + a DLX |

- ❌ Do not requeue a failing message with `requeue=True`.
- ❌ Do not leave a DLX without a queue and a binding.

## Material levels

| Level | What is inside |
|---|---|
| [1. Basic theory](basics.en.md) | DLX, reasons, `x-death`, loss without a DLX |
| [2. Advanced theory](advanced.en.md) | delayed retry, counting attempts, a DLX with no route, delivery limit |
| [3. Expert theory](expert.en.md) | reference of arguments and headers, gotchas, interview questions |
| [4. Self-check](quiz.en.md) | quiz and [exercises](exercises.en.md) |
| 5. Runnable examples | not created yet — `/learn rabbitmq 06-dead-lettering-and-retries level 5` |
| 6. Tests | not created yet — `/learn rabbitmq 06-dead-lettering-and-retries level 6` |

## Related materials

- Prerequisites: [03 · Queues and delivery](../03-queues-and-delivery/overview.en.md), [05 · Reliability](../05-reliability/overview.en.md)
- Next: [07 · Queue types](../07-queue-types/overview.en.md)
- Analogues in other tracks: [C# background jobs](../../../csharp/recipes/background-jobs/overview.en.md)
- [System design: messaging and events](../../../system-design/modules/08-messaging-and-events/overview.en.md)
