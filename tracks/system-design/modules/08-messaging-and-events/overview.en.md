---
title: "Messaging and events"
lang: en
track: system-design
kind: module
doc: overview
slug: 08-messaging-and-events
part: 2
levels: [1, 2, 3, 4, 5, 6]
tags: [system-design, message-broker, events, delivery-semantics, idempotency, backpressure, poison-message]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-26
---

# Messaging and events

🌐 [Русский](overview.ru.md) · **English**

## In a nutshell

Instead of a direct call, the sender puts a message into a broker and the receiver processes it when it can. This decouples services and smooths peaks, but brings duplicates, delay and ordering questions.

## Key ideas

- A command asks to do something, an event reports that something happened.
- A queue hands out work, a log keeps history.
- The moment of acknowledgement decides whether a message is lost or repeated.
- An idempotent consumer makes a repeat harmless.
- Ordering is guaranteed only within narrow bounds.
- Backpressure stops a fast producer from drowning the consumer.
- A bad message goes to a dead letter queue rather than being retried forever.

## Minimal example

```csharp
if (_processed.Add(message.Id)) Balance += message.Amount;
```

## When to use and when not

- ✅ The work can be done later.
- ✅ Several independent receivers of one event.
- ✅ Uneven load.
- ❌ The client needs the result immediately.
- ❌ A small application with no reason to be distributed.

## Levels of this material

| Level | What's inside |
|---|---|
| [1. Basics](basics.en.md) | commands and events, queue and log, acknowledgements |
| [2. Advanced](advanced.en.md) | delivery semantics, ordering, backpressure, dead letter |
| [3. Expert](expert.en.md) | reference, Channels gotchas, questions |
| [4. Self-check](quiz.en.md) | a quiz and [exercises](exercises.en.md) |
| [5. Runnable examples](code/Examples/) | Channels, delivery, a log, dead letter |
| [6. Tests](code/Tests/) | `dotnet test` inside `code/` |

## Related materials

- Prerequisites: [consistency and CAP](../07-consistency-and-cap/overview.en.md), [API design](../05-api-design/overview.en.md)
- Next: [reliability patterns](../09-reliability-patterns/overview.en.md)
- In other tracks: [RabbitMQ](../../../rabbitmq/modules/01-messaging-basics/overview.en.md), [Kafka](../../../kafka/modules/05-delivery-semantics/overview.en.md), [MassTransit](../../../csharp/libraries/masstransit/overview.en.md), [background jobs](../../../csharp/recipes/background-jobs/overview.en.md)
- [Distributed transactions](../10-distributed-transactions/overview.en.md)
- [Architecture styles](../11-architecture-styles/overview.en.md)
- [Event sourcing and CQRS](../13-event-sourcing-and-cqrs/overview.en.md)
- [Case studies](../17-design-case-studies/overview.en.md)
