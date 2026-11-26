---
title: "Messaging and the AMQP model"
lang: en
track: rabbitmq
kind: module
doc: overview
slug: 01-messaging-basics
part: 1
levels: [1, 2, 3, 4]
tags: [rabbitmq, amqp, message-broker, queue, exchange, binding, channel, acknowledgement]
versions: "RabbitMQ 4.4.1; AMQP 0-9-1; Python pika 1.5.0"
status: draft
updated: 2026-11-24
---

# Messaging and the AMQP model

🌐 [Русский](overview.ru.md) · **English**

## In short

A message broker takes messages from producers and hands them to consumers, so services do not need to be alive at the same time or to know each other. In RabbitMQ (AMQP 0-9-1) a producer publishes to an **exchange**, the exchange routes the message through **bindings** into **queues**, and **consumers** take messages from the queues and **acknowledge** them. Work happens on cheap **channels** inside one long-lived **connection**.

## Key ideas

- **Producer → exchange → binding → queue → consumer.** A producer never writes to a queue directly; the nameless default exchange only looks like it does (it routes by queue name).
- **A connection is expensive, a channel is cheap.** One connection per process, channels per task.
- **`basic_get` polls, `basic_consume` pushes.** Prefer consuming.
- **Acknowledge after processing.** An unacknowledged message returns to the queue when the consumer dies, marked `redelivered`.
- **An error on a channel closes that channel**, not the connection: a passive declare of a missing queue gave `404 NOT_FOUND` and a dead channel.
- **RabbitMQ 4.4 refuses queues that are neither durable nor exclusive** and closes the connection (`541`).
- **Publishing is asynchronous:** the message count read right after a publish may lag.
- **pika 1.5.0** deprecates `BlockingConnection`; use `pika.Connection`.

## Minimal example

```python
import pika
conn = pika.Connection(pika.ConnectionParameters("localhost"))
ch = conn.channel()
ch.queue_declare("hello", durable=True)
ch.basic_publish(exchange="", routing_key="hello", body=b"Hello, RabbitMQ!")
method, props, body = ch.basic_get("hello")      # (GetOk, properties, b'Hello, RabbitMQ!')
ch.basic_ack(method.delivery_tag)
conn.close()
```

All code was run against a real RabbitMQ 4.4.1 broker in Docker (see "How the code was checked" in the [basic theory](basics.en.md)).

## When to use and when not

| Task | Choice |
|---|---|
| Decouple services in time, absorb bursts, spread work over workers | a RabbitMQ queue |
| One event for many independent subscribers | exchanges (fanout / topic), [module 02](../02-exchanges-and-routing/overview.en.md) |
| A reply is needed right now, one-to-one | a direct call; RPC over a broker is an exception ([module 04](../04-messaging-patterns/overview.en.md)) |
| A replayable log read by many consumers | a stream or a log system such as Kafka (see queue types, module 07 in the [roadmap](../../overview.en.md)) |
| Data that must be stored and queried | a database |

- ❌ Do not use a broker as a database.
- ❌ Do not assume exactly-once delivery: design for duplicates.

## Material levels

| Level | What is inside |
|---|---|
| [1. Basic theory](basics.en.md) | the model, connection and channel, first message, get and consume, acknowledgements, errors |
| [2. Advanced theory](advanced.en.md) | message properties, connection parameters and failures, asynchronous publish errors |
| [3. Expert theory](expert.en.md) | defaults and limits, ports, built-in exchanges, protocol classes, gotchas, interview questions |
| [4. Self-check](quiz.en.md) | quiz and [exercises](exercises.en.md) |
| 5. Runnable examples | not created yet — `/learn rabbitmq 01-messaging-basics level 5` |
| 6. Tests | not created yet — `/learn rabbitmq 01-messaging-basics level 6` |

## Related materials

- Prerequisites: [Python](../../../python/overview.en.md) basics
- Next: [02 · Exchanges and routing](../02-exchanges-and-routing/overview.en.md)
- Analogues in other tracks: [C# background jobs](../../../csharp/recipes/background-jobs/overview.en.md), [Linux](../../../linux/overview.en.md) (sockets)
- [System design: messaging and events](../../../system-design/modules/08-messaging-and-events/overview.en.md)
