---
title: "MassTransit"
lang: en
track: csharp
kind: library
doc: overview
slug: masstransit
part: 3
levels: [1, 2, 3, 4, 5, 6]
tags: [masstransit, messaging, message-bus, consumer, saga, rabbitmq, test-harness, retry]
versions: ".NET 11, C# 15; MassTransit 8.5.12"
status: draft
updated: 2026-11-25
---

# MassTransit

🌐 [Русский](overview.ru.md) · **English**

## In a nutshell

MassTransit is a messaging library for .NET. It hides the transport (RabbitMQ, Azure Service Bus, Amazon SQS, Kafka, and "in memory" for tests) behind one model: a **message**, a **bus**, an **endpoint**, a **consumer**. Out of the box come retries, an error queue, request-response, state-machine sagas and a test harness. Everything was verified by running on the **8.x line (8.5.12)**: version 9 is commercial, and using it in production needs a licence.

## Key ideas

- **A contract is a type in a namespace** (`record Shop.OrderSubmitted`); without a namespace publishing fails.
- **`Publish` is an event for all subscribers, `Send` is a command to one endpoint.**
- **A consumer is `IConsumer<T>`;** default queue names are `OrderSubmitted`, `kebab-case` is enabled explicitly.
- **Dispose the container with `await using`.**
- **Retries** (`UseMessageRetry`) run in the process; once exhausted — `Fault<T>` and a queue `<name>_error` with `MT-Fault-*` headers.
- **RabbitMQ:** a message-type exchange → an endpoint exchange → a queue.
- **The test harness:** tests with no `Task.Delay`; state-machine sagas (`MassTransitStateMachine`).
- **8.x has anonymous usage telemetry** (it can be disabled).

## Minimal example

```csharp
await using var sp = new ServiceCollection()
    .AddMassTransit(x => { x.AddConsumer<OrderSubmittedConsumer>(); x.UsingInMemory((ctx, c) => c.ConfigureEndpoints(ctx)); })
    .BuildServiceProvider();
var bus = sp.GetRequiredService<IBusControl>();
await bus.StartAsync();
await bus.Publish(new OrderSubmitted(Guid.NewGuid(), "alice", 19.99m));
```

Everything was run on .NET 11 and MassTransit 8.5.12, RabbitMQ — 4.4.1 in Docker (see the [basics](basics.en.md)).

## When to use and when not

- ✅ Several services exchange events and commands through a broker.
- ✅ You need retries, an error queue, sagas and convenient tests.
- ✅ You want to change the transport without rewriting handler code.
- ❌ An in-process queue (`Channel<T>`) is enough — see [background jobs](../../recipes/background-jobs/overview.en.md).
- ❌ You need full control of the broker's protocol.
- ❌ The v9 licence does not suit you (then v8 or another library).

## Running the code

```bash
cd tracks/csharp/libraries/masstransit/code
dotnet test
```

11 tests; 10 need nothing but the .NET 11 SDK, and the RabbitMQ integration test is skipped when there is no broker. To run it start a disposable broker: `docker run -d --name rabbit -p 5672:5672 rabbitmq:4-management`.

## Levels of this material

| Level | What's inside |
|---|---|
| [1. Basics](basics.en.md) | a message, a consumer, an in-memory bus, Publish and Send, queue names |
| [2. Advanced](advanced.en.md) | retries, `Fault<T>` and `_error`, request-response, concurrency, RabbitMQ topology, the test harness |
| [3. Expert](expert.en.md) | reference, sagas, internals, versions and licence, 12 interview questions |
| [4. Self-check](quiz.en.md) | 10 questions and [4 exercises](exercises.en.md) with solutions |
| 5. [Runnable examples](code/Examples/) | 6 examples and [exercise solutions](code/Solutions/Exercises.cs) |
| 6. [Tests](code/Tests/) | xUnit v3 on the test harness and one integration test with RabbitMQ |

## Related materials

- Prerequisites: [13 · DI, configuration, logging](../../modules/13-di-config-logging/overview.en.md), [10 · async/await](../../modules/10-async-await/overview.en.md)
- Nearby: [Background jobs](../../recipes/background-jobs/overview.en.md), [Polly](../polly/overview.en.md), [Testcontainers](../testcontainers/overview.en.md)
- In other tracks: [RabbitMQ](../../../rabbitmq/overview.en.md) — the broker under the transport; [Kafka](../../../kafka/overview.en.md) — an event log
