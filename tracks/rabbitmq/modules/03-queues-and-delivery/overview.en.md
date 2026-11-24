---
title: "Queues and delivery"
lang: en
track: rabbitmq
kind: module
doc: overview
slug: 03-queues-and-delivery
part: 1
levels: [1, 2, 3, 4]
tags: [rabbitmq, queue, acknowledgement, prefetch, durability, message-ttl, queue-length-limit, redelivery]
versions: "RabbitMQ 4.4.1; AMQP 0-9-1; Python pika 1.5.0"
status: draft
updated: 2026-11-24
---

# Queues and delivery

🌐 [Русский](overview.ru.md) · **English**

## In short

A queue holds messages until consumers take them. How reliably and fairly that happens depends on a few switches: **acknowledgements** (`ack`, `nack`, `reject`, requeue or drop), **prefetch** (how many unacknowledged messages a consumer may hold), **durability** (a durable queue plus persistent messages survive a broker restart) and **limits** (TTL, maximum length, expiry of the queue itself).

## Key ideas

- **Ack after processing.** `basic_ack` removes the message; `basic_nack`/`basic_reject` with `requeue=True` returns it, with `requeue=False` drops it (or dead-letters it, module 06 in the [roadmap](../../overview.en.md)).
- **A consumer that dies returns its unacked messages** to the queue, marked `redelivered`.
- **Prefetch (`basic_qos`) controls fairness.** With `prefetch_count=1` a stuck consumer holds one message; with no limit it may be handed everything.
- **Durable ≠ persistent.** A durable queue survives a restart; a message survives only if it is also persistent (`delivery_mode=2`). In a restart test the persistent message stayed and the transient one was lost.
- **Limits:** `x-message-ttl`, per-message `expiration`, `x-max-length` with `x-overflow`, `x-expires`.
- **Exclusive** queues belong to one connection; **auto-delete** queues go after their last consumer.
- **Redeclaring a queue with different arguments fails** (`406`).

## Minimal example

```python
ch.queue_declare("jobs", durable=True)
ch.basic_qos(prefetch_count=1)

def on_job(ch_, method, props, body):
    try:
        handle(body)
        ch_.basic_ack(method.delivery_tag)
    except Exception:
        ch_.basic_nack(method.delivery_tag, requeue=False)

ch.basic_consume("jobs", on_job)
```

Everything was run against a real RabbitMQ 4.4.1 in Docker (see the [basic theory](basics.en.md)).

## When to use and when not

| Need | Switch |
|---|---|
| Never lose work when a worker crashes | manual ack |
| Share work evenly among workers | `prefetch_count=1` (or small) |
| Messages must survive a restart | durable queue + persistent messages |
| Drop stale messages | TTL |
| Protect memory from an unbounded queue | `x-max-length` |
| Private reply queue | exclusive or server-named queue |

- ❌ Do not rely on `auto_ack=True` for work that matters.
- ❌ Do not make queues unbounded without monitoring.

## Material levels

| Level | What is inside |
|---|---|
| [1. Basic theory](basics.en.md) | ack, nack, reject, redelivery, prefetch, durability |
| [2. Advanced theory](advanced.en.md) | TTL, length limits and overflow, expiry, exclusive and auto-delete queues, argument mismatches |
| [3. Expert theory](expert.en.md) | reference of queue arguments with observed behaviour, delivery timeout, gotchas, interview questions |
| [4. Self-check](quiz.en.md) | quiz and [exercises](exercises.en.md) |
| 5. Runnable examples | not created yet — `/learn rabbitmq 03-queues-and-delivery level 5` |
| 6. Tests | not created yet — `/learn rabbitmq 03-queues-and-delivery level 6` |

## Related materials

- Prerequisites: [01 · Messaging and the AMQP model](../01-messaging-basics/overview.en.md), [02 · Exchanges and routing](../02-exchanges-and-routing/overview.en.md)
- Next: 04 · Messaging patterns ([roadmap](../../overview.en.md))
- Analogues in other tracks: [C# background jobs](../../../csharp/recipes/background-jobs/overview.en.md)
