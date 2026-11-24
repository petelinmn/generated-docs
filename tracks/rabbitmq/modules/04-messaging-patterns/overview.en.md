---
title: "Messaging patterns"
lang: en
track: rabbitmq
kind: module
doc: overview
slug: 04-messaging-patterns
part: 1
levels: [1, 2, 3, 4]
tags: [rabbitmq, work-queue, publish-subscribe, routing, rpc, direct-reply-to, correlation-id]
versions: "RabbitMQ 4.4.1; AMQP 0-9-1; Python pika 1.5.0"
status: draft
updated: 2026-11-24
---

# Messaging patterns

🌐 [Русский](overview.ru.md) · **English**

## In short

Exchanges, queues and bindings combine into four typical patterns: **work queue** (one message to one worker), **publish/subscribe** (each subscriber gets a copy), **routing** (the subscriber picks keys) and **RPC** (request and reply tied by `correlation_id`). The patterns differ in who divides the messages and who copies them.

## Key ideas

- **One queue — divide; a queue per subscriber — copy.**
- **Work queue** — a durable queue, `prefetch_count=1`, ack after the work: the fast worker took 9 of 10 tasks.
- **Pub/sub** — a fanout and temporary queues (`queue_declare("", exclusive=True)`); anything published with no subscribers is lost.
- **Routing** — a direct exchange; one queue may have several bindings.
- **RPC** — `reply_to` and `correlation_id`; the broker does not interpret them; the client sets its own timeout.
- **Direct reply-to** (`amq.rabbitmq.reply-to`) — no per-client queue; needs `auto_ack=True` and consuming before publishing.
- **`expiration` on the request** keeps a stale RPC from running after the server returns.

## Minimal example

```python
ch.exchange_declare("logs", "fanout")
q = ch.queue_declare("", exclusive=True).method.queue   # server-named
ch.queue_bind(q, "logs")
ch.basic_consume(q, lambda c, m, p, b: print(b), auto_ack=True)
ch.basic_publish("logs", "", b"hello")
```

Everything was run against a real RabbitMQ 4.4.1 in Docker (see the [basic theory](basics.en.md)).

## When to use and when not

| Need | Pattern |
|---|---|
| Spread heavy work | work queue |
| Notify everyone | pub/sub |
| Notify the interested | routing / topics |
| Get a result right away | RPC |
| Result not needed immediately | command + result event |

- ❌ Do not use RPC where the answer is not needed now.
- ❌ Do not make several subscribers read one queue if they each need a copy.

## Material levels

| Level | What is inside |
|---|---|
| [1. Basic theory](basics.en.md) | work queue, pub/sub, routing, RPC |
| [2. Advanced theory](advanced.en.md) | direct reply-to, RPC with a timeout, choosing a pattern |
| [3. Expert theory](expert.en.md) | reference of patterns and properties, gotchas, interview questions |
| [4. Self-check](quiz.en.md) | quiz and [exercises](exercises.en.md) |
| 5. Runnable examples | not created yet — `/learn rabbitmq 04-messaging-patterns level 5` |
| 6. Tests | not created yet — `/learn rabbitmq 04-messaging-patterns level 6` |

## Related materials

- Prerequisites: [02 · Exchanges and routing](../02-exchanges-and-routing/overview.en.md), [03 · Queues and delivery](../03-queues-and-delivery/overview.en.md)
- Next: [05 · Reliability](../05-reliability/overview.en.md)
- Analogues in other tracks: [C# background jobs](../../../csharp/recipes/background-jobs/overview.en.md)
