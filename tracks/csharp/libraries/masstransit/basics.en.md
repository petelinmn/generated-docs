---
title: "MassTransit: basics"
lang: en
track: csharp
kind: library
doc: basics
slug: masstransit
part: 3
tags: [masstransit, messaging, message-bus, consumer, publish-subscribe, in-memory-transport, dependency-injection]
versions: ".NET 11, C# 15; MassTransit 8.5.12"
status: draft
updated: 2026-11-25
---

# MassTransit: basics

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md) · [5–6. Code and tests](code/)

**Prerequisites:** [13 · DI, configuration, logging](../../modules/13-di-config-logging/overview.en.md), [10 · async/await](../../modules/10-async-await/overview.en.md); familiarity with message brokers helps ([RabbitMQ](../../../rabbitmq/overview.en.md)). **Time:** ~45 min.

## How the code was verified

All code was run on the .NET 11 SDK 11.0.100 (`net11.0`) with MassTransit 8.5.12; outputs are copied from the runs. The in-memory transport needs no broker; the RabbitMQ examples were run against a real RabbitMQ 4.4.1 in Docker (`rabbitmq:4-management`). All messages live in the namespace `Shop`:

```csharp
namespace Shop;

public record OrderSubmitted(Guid OrderId, string Customer, decimal Amount);
```

> **Version and licence.** This material describes the **8.x** line (8.5.12) — the last open one. According to the developers' announcement version **9** became commercial: production use needs a licence. Check the terms on MassTransit's official pages; version 9 was not run in this material.

## What MassTransit is

**MassTransit** is a messaging library for .NET: it hides the transport (RabbitMQ, Azure Service Bus, Amazon SQS, Kafka and others, plus "in memory" for tests) behind one model: a **message**, a **bus**, an **endpoint** (a receiving queue) and a **consumer**. On top come retries, error handling, request-response and sagas.

```bash
dotnet add package MassTransit --version 8.5.12
dotnet add package MassTransit.RabbitMQ --version 8.5.12     # only for the RabbitMQ transport
```

## A message contract

A message is an ordinary type (a `record` is handy). The type **must live in a namespace**: a type in the global namespace gives an error on publish.

```text
System.ArgumentException: Messages types must have a valid namespace: OrderSubmitted (Parameter 'T')
```

The type name together with its namespace becomes part of the address (the message's "topic"): `Shop:OrderSubmitted`.

## A consumer

A consumer implements `IConsumer<TMessage>`:

```csharp
public class OrderSubmittedConsumer : IConsumer<OrderSubmitted>
{
    public Task Consume(ConsumeContext<OrderSubmitted> context)
    {
        Console.WriteLine($"{context.Message.Customer} {context.Message.Amount} | MessageId set: {context.MessageId.HasValue}");
        return Task.CompletedTask;
    }
}
```

`ConsumeContext<T>` holds the message itself (`Message`), service data (`MessageId`, `SourceAddress`, headers) and methods for responding and publishing.

## An in-memory bus

```csharp
await using var sp = new ServiceCollection()
    .AddMassTransit(x =>
    {
        x.AddConsumer<OrderSubmittedConsumer>();
        x.UsingInMemory((ctx, cfg) => cfg.ConfigureEndpoints(ctx));
    })
    .BuildServiceProvider();

var bus = sp.GetRequiredService<IBusControl>();
await bus.StartAsync();
await bus.Publish(new OrderSubmitted(Guid.NewGuid(), "alice", 19.99m));
await Task.Delay(500);
await bus.StopAsync();
```

```text
bus address: loopback://localhost/<host>_Mt_bus_<id>
[OrderSubmittedConsumer] alice 19,99 | MessageId set: True | Source: /<host>_Mt_bus_<id>
```

- `AddMassTransit` registers the bus and consumers in the dependency-injection container.
- `ConfigureEndpoints` creates a receiving endpoint for each consumer.
- The bus is started by `StartAsync` (in an application with a `Host` a hosted service does it) and stopped by `StopAsync`.
- The message arrived, and `MessageId` was assigned automatically.

> **Gotcha:** dispose the container with **`await using`**. With a plain `using var sp` you get `InvalidOperationException: 'MassTransit.UsageTracking.UsageTracker' type only implements IAsyncDisposable. Use DisposeAsync to dispose the container.` — some MassTransit services support only asynchronous disposal.

Code: [Messages.cs](code/Examples/Messages.cs), [Ex01_PublishSend.cs](code/Examples/Ex01_PublishSend.cs).

## Publish and Send

| Method | Addressee | When |
|---|---|---|
| `Publish` | every subscriber of the message type | an **event**: "it happened" |
| `Send` | one particular endpoint | a **command**: "do it" |

Two consumers (`OrderSubmittedConsumer`, `AuditConsumer`) on one message type:

```csharp
await bus.Publish(order);                                         // to every subscriber
var ep = await bus.GetSendEndpoint(new Uri("queue:audit"));
await ep.Send(order);                                             // to one endpoint
```

```text
-- Publish (all subscribers):
[AuditConsumer] saw 4f54732d
[OrderSubmittedConsumer] alice 19,99 | MessageId set: True | Source: /<host>_Mt_bus_<id>
-- Send to queue:audit:
[AuditConsumer] saw 4f54732d
```

`Publish` reached both consumers, `Send` to `queue:audit` only one. The output order between consumers is not guaranteed.

Consumer code: [Ex01_PublishSend.cs](code/Examples/Ex01_PublishSend.cs); the check that both get a `Publish` and one gets a `Send`: [ExamplesTests.cs](code/Tests/ExamplesTests.cs).

## Endpoint names

A consumer's queue name is built from the class name without the `Consumer` suffix. The default formatter leaves `OrderSubmitted` (in the test harness log: `Endpoint Ready: loopback://localhost/OrderSubmitted`). For the familiar `kebab-case` enable it explicitly:

```csharp
x.SetKebabCaseEndpointNameFormatter();      // OrderSubmittedConsumer -> order-submitted
```

With this formatter RabbitMQ got the queues `order-submitted` and `audit` ([advanced theory](advanced.en.md#rabbitmq-topology)).

## Common beginner mistakes

- ❌ A message in the global namespace → ✅ any namespace.
- ❌ `using var sp` instead of `await using` → ✅ `await using`.
- ❌ `Publish` when you need a command for one recipient → ✅ `Send`.
- ❌ Expecting order between different consumers → ✅ order is not guaranteed.
- ❌ Taking v9 for production without checking the licence → ✅ v8 or a v9 licence.

## Related materials

- Next: [advanced level](advanced.en.md) — retries, errors, request-response, RabbitMQ
- [Background jobs](../../recipes/background-jobs/overview.en.md) — an in-process queue versus a broker
- [RabbitMQ](../../../rabbitmq/overview.en.md) — the broker under MassTransit; [Kafka](../../../kafka/overview.en.md) — an event log

## Sources

- [MassTransit: documentation](https://masstransit.io/documentation/concepts)
- [MassTransit: getting started](https://masstransit.io/quick-starts/in-memory)
- [Announcing MassTransit v9](https://masstransit.io/introduction/v9-announcement)
