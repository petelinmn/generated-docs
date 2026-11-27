---
title: "Messaging and events: basics"
lang: en
track: system-design
kind: module
doc: basics
slug: 08-messaging-and-events
part: 2
tags: [system-design, message-broker, events, delivery-semantics, idempotency, backpressure, poison-message]
versions: ".NET 11, C# 15"
status: verified
updated: 2026-11-27
---

# Messaging and events: basics

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** async/await · **Time:** ~2 h

## Why it matters

If placing an order synchronously calls payment, stock and email, an email failure breaks the order, and the latencies add up. Messages let you accept the order at once and do the rest later.

## Commands and events

| | Command | Event |
|---|---|---|
| Meaning | "do it" | "it happened" |
| Name | `ChargeCard` | `OrderPaid` |
| Receiver | one owner | any number of subscribers |

## Queue and log

| | Queue | Log |
|---|---|---|
| After processing | the message is removed | it stays until retention expires |
| Position | kept by the broker | the consumer group's offset |
| Reading again | usually no | yes |
| Example | [RabbitMQ](../../../rabbitmq/modules/01-messaging-basics/overview.en.md) | [Kafka](../../../kafka/modules/01-kafka-basics/overview.en.md) |

```csharp
public IReadOnlyList<string> Poll(string group, int max = 100)
{
    int offset = _offsets.GetValueOrDefault(group);
    return _events.GetRange(offset, Math.Min(max, _events.Count - offset));
}
```

Reading does not remove events: the `billing` group has caught up while `search` lags by two, and either can rewind. File: [Ex03_Log.cs](code/Examples/Ex03_Log.cs).

## Acknowledgements

The consumer acknowledges (acks) processing. Until it does, the broker treats the message as unfinished and may deliver it again. In RabbitMQ automatic acknowledgement is "fire and forget" and is unsafe for important data.

## Beginner mistakes

- ❌ Acknowledging before processing → ✅ after.
- ❌ Assuming a message arrives exactly once → ✅ be ready for a repeat.
- ❌ Putting a tens-of-megabytes object into a message → ✅ a link.
- ❌ An event named as an order → ✅ past tense.

## Related materials

- [Advanced theory](advanced.en.md)
- [Background jobs in .NET](../../../csharp/recipes/background-jobs/overview.en.md)

## Sources

- [RabbitMQ: confirms](https://www.rabbitmq.com/docs/confirms)
- [Competing Consumers pattern](https://learn.microsoft.com/azure/architecture/patterns/competing-consumers)
