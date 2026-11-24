---
title: "Reliability"
lang: en
track: rabbitmq
kind: module
doc: overview
slug: 05-reliability
part: 2
levels: [1, 2, 3, 4]
tags: [rabbitmq, publisher-confirms, mandatory, transactions, at-least-once, idempotency]
versions: "RabbitMQ 4.4.1; AMQP 0-9-1; Python pika 1.5.0"
status: draft
updated: 2026-11-24
---

# Reliability

🌐 [Русский](overview.ru.md) · **English**

## In short

Reliable delivery is a chain of four links: **publisher → broker** (publisher confirms), **route** (`mandatory`), **storage** (a durable queue and persistent messages) and **consumer** (manual ack after the work). Together they give "at least once" delivery: a message is not lost but may arrive twice, so the consumer must be idempotent.

## Key ideas

- **A confirm means "the broker accepted", not "processed".**
- **An `ack` also arrives for a message with no route** unless `mandatory` is set; with `mandatory` a `312 NO_ROUTE` return comes first.
- **A `nack` from the broker** — for example a full queue with `reject-publish`.
- **A channel error** (a nonexistent exchange) closes the channel and no confirm arrives: wait with a timeout.
- **Pipelined confirms** are hundreds of times faster than one by one (0.048 s against 13.4 s per 300 messages in the test).
- **Transactions** exist only in `BlockingConnection` and are costly; use confirms.
- **Idempotency by `message_id`**, stored in a database, covers duplicates.

## Minimal example

```python
pub = Confirmed(ch)        # helper from the basics, wraps confirm_delivery
verdict = pub.publish("", "rel", b"a", pika.BasicProperties(delivery_mode=2), mandatory=True)
print(verdict)             # ack
```

Everything was run against a real RabbitMQ 4.4.1 in Docker (see the [basic theory](basics.en.md)).

## When to use and when not

| Need | Mechanism |
|---|---|
| Know the broker accepted the message | confirms |
| Know the message had a route | `mandatory` |
| Survive a broker restart | durable + persistent |
| Not lose on a consumer crash | manual ack |
| Survive a retry | idempotency |

- ❌ Do not use one-by-one confirms at high volume: use a pipeline.
- ❌ Do not rely on transactions instead of confirms.

## Material levels

| Level | What is inside |
|---|---|
| [1. Basic theory](basics.en.md) | the links of reliability, confirms, mandatory, "at least once" |
| [2. Advanced theory](advanced.en.md) | nack, errors, throughput, transactions, idempotency |
| [3. Expert theory](expert.en.md) | reference of broker replies and the pika API, gotchas, interview questions |
| [4. Self-check](quiz.en.md) | quiz and [exercises](exercises.en.md) |
| 5. Runnable examples | not created yet — `/learn rabbitmq 05-reliability level 5` |
| 6. Tests | not created yet — `/learn rabbitmq 05-reliability level 6` |

## Related materials

- Prerequisites: [03 · Queues and delivery](../03-queues-and-delivery/overview.en.md), [04 · Messaging patterns](../04-messaging-patterns/overview.en.md)
- Next: [06 · Dead-lettering and retries](../06-dead-lettering-and-retries/overview.en.md)
- Analogues in other tracks: [C# background jobs](../../../csharp/recipes/background-jobs/overview.en.md)
