---
title: "Messaging and events: advanced"
lang: en
track: system-design
kind: module
doc: advanced
slug: 08-messaging-and-events
part: 2
tags: [system-design, message-broker, events, delivery-semantics, idempotency, backpressure, poison-message]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-26
---

# Messaging and events: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

## Delivery semantics

| Semantics | When to ack | On failure |
|---|---|---|
| at-most-once | before processing | the message is lost |
| at-least-once | after processing | it repeats |
| "effect once" | after, plus deduplication | the repeat is harmless |

```csharp
// In a real system the id and the effect are saved in one transaction.
public void ApplyOnce(Message message)
{
    if (_processed.Add(message.Id)) Balance += message.Amount;
}
```

Three messages of 10, 20 and 30 with a crash on the second give: ack before processing — 40, ack after without deduplication — 80, with deduplication — 60. File: [Ex02_Delivery.cs](code/Examples/Ex02_Delivery.cs).

## Ordering

Order is usually preserved only within one partition or a queue with one consumer; retries and parallel consumers break it. Messages of one entity are routed by key to one partition.

## Backpressure

```csharp
public static Channel<int> Create(int capacity, BoundedChannelFullMode mode) =>
    Channel.CreateBounded<int>(new BoundedChannelOptions(capacity) { FullMode = mode });
```

| `FullMode` | What happens when full | What remains (capacity 2, writing 1–5) |
|---|---|---|
| `Wait` (default) | `WriteAsync` waits, `TryWrite` returns `false` | 1, 2 |
| `DropOldest` | the oldest is thrown away | 4, 5 |
| `DropNewest` | the newest in the buffer is thrown away | 1, 5 |
| `DropWrite` | the item being written is thrown away | 1, 2 |

File: [Ex01_Backpressure.cs](code/Examples/Ex01_Backpressure.cs).

## Dead letter

```csharp
// Requeue at the tail so a failing message does not block the others.
if (item.Attempt < maxAttempts) _queue.Enqueue((item.Body, item.Attempt + 1));
else DeadLetters.Add(item.Body);
```

A message that can never be processed (a poison message) goes to a separate queue for investigation after the limit. File: [Ex04_DeadLetter.cs](code/Examples/Ex04_DeadLetter.cs).

## Best practices

- Give a message an identifier, a type and a schema version.
- Monitor queue depth, lag and the age of the oldest message.
- Change the schema compatibly.
- Have a procedure for investigating and resending from the dead letter queue.

## Common mistakes

- ❌ Writing to the database and publishing separately → ✅ an outbox: the event in the same transaction, published by a separate process.
- ❌ An unbounded in-memory queue → ✅ a bounded one with an explicit policy.
- ❌ Retrying forever → ✅ a limit and a delay.
- ❌ Deduplication only in memory → ✅ in durable storage.

## Comparison with other tracks

- [Dead lettering and retries in RabbitMQ](../../../rabbitmq/modules/06-dead-lettering-and-retries/overview.en.md).
- [Delivery semantics in Kafka](../../../kafka/modules/05-delivery-semantics/overview.en.md).
- [MassTransit](../../../csharp/libraries/masstransit/overview.en.md).

## Related materials

- [Basics](basics.en.md)
- [Expert theory](expert.en.md)

## Sources

- [Channels in .NET](https://learn.microsoft.com/dotnet/core/extensions/channels)
- [RabbitMQ: confirms](https://www.rabbitmq.com/docs/confirms)
