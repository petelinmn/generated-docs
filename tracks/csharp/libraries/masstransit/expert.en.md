---
title: "MassTransit: expert"
lang: en
track: csharp
kind: library
doc: expert
slug: masstransit
part: 3
tags: [masstransit, saga, state-machine, middleware, pipeline, consumer, rabbitmq, licensing]
versions: ".NET 11, C# 15; MassTransit 8.5.12"
status: draft
updated: 2026-11-24
---

# MassTransit: expert

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · **3. Expert** · [4. Quiz](quiz.en.md)

Builds on the [basics](basics.en.md) and the [advanced level](advanced.en.md). "Verified" means observed by running on .NET 11 with MassTransit 8.5.12 (an in-memory bus and RabbitMQ 4.4.1).

## Reference

### Core concepts and API

| Element | Purpose | Verified |
|---|---|---|
| `IConsumer<T>` | a message handler | yes |
| `ConsumeContext<T>` | the message, headers, `RespondAsync`, `GetRetryAttempt()` | yes |
| `IBusControl` | start/stop the bus, `Publish`, `GetSendEndpoint` | yes |
| `IPublishEndpoint`, `ISendEndpointProvider` | publishing and sending from injected services | not tested |
| `IRequestClient<TRequest>` | request-response | yes |
| `AddMassTransit`, `AddConsumer<T>`, `UsingInMemory`, `UsingRabbitMq`, `ConfigureEndpoints` | registration and transports | yes |
| `SetKebabCaseEndpointNameFormatter()` | queue names in `kebab-case` | yes |
| `ReceiveEndpoint(name, …)`, `ConfigureConsumer<T>(ctx)` | an explicit endpoint | yes |
| `UseMessageRetry(…)`, `Immediate`, `Interval` | retries | yes |
| `ConcurrentMessageLimit` | an endpoint's concurrency | yes (1 and 4) |
| `Fault<T>` | the failure event after retries are exhausted | yes |
| `ConsumerDefinition`, `UseMessageRedelivery`, `UseInMemoryOutbox`, the Entity Framework outbox | definitions, delayed retry, outbox | not tested |

### The test harness

| Element | Verified |
|---|---|
| `AddMassTransitTestHarness`, `ITestHarness`, `harness.Start()` | yes |
| `harness.Published.Any<T>()`, `Consumed.Any<T>()`, `GetConsumerHarness<TConsumer>()` | yes |
| `GetSagaStateMachineHarness<TMachine, TInstance>()`, `Exists(id, m => m.State)`, `NotExists(id)` | yes |

### Transports

| Transport | Verified |
|---|---|
| in-memory (`UsingInMemory`) | yes |
| RabbitMQ (`MassTransit.RabbitMQ`) | yes |
| Azure Service Bus, Amazon SQS, ActiveMQ, Kafka (rider), Mediator | not tested |

## Sagas and state machines

A **saga** keeps the state of a long process and reacts to messages. In MassTransit a saga is described by a **state machine** (`MassTransitStateMachine<TInstance>`): states, events and transitions. An instance (`SagaStateMachineInstance`) holds a `CorrelationId` and the current state.

```csharp
public class OrderState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; } = "";
    public string? Customer { get; set; }
}

public class OrderStateMachine : MassTransitStateMachine<OrderState>
{
    public State Submitted { get; private set; } = null!;
    public State Paid { get; private set; } = null!;
    public Event<OrderSubmitted> SubmittedEvent { get; private set; } = null!;
    public Event<OrderPaid> PaidEvent { get; private set; } = null!;
    public Event<OrderShipped> ShippedEvent { get; private set; } = null!;

    public OrderStateMachine()
    {
        InstanceState(x => x.CurrentState);
        Event(() => SubmittedEvent, e => e.CorrelateById(c => c.Message.OrderId));
        Event(() => PaidEvent, e => e.CorrelateById(c => c.Message.OrderId));
        Event(() => ShippedEvent, e => e.CorrelateById(c => c.Message.OrderId));

        Initially(When(SubmittedEvent).Then(c => c.Saga.Customer = c.Message.Customer).TransitionTo(Submitted));
        During(Submitted, When(PaidEvent).TransitionTo(Paid));
        During(Paid, When(ShippedEvent).Finalize());
        SetCompletedWhenFinalized();
    }
}

services.AddMassTransitTestHarness(x => x.AddSagaStateMachine<OrderStateMachine, OrderState>().InMemoryRepository());
```

An experiment in the test harness: `OrderSubmitted` → `OrderShipped` (before payment) → `OrderPaid` → `OrderShipped`:

```text
exists in Submitted: True
state after OrderShipped in Submitted: still Submitted
faults published: True
exists in Paid: True
NotExists after finalize returned: (null)
customer: alice
```

- The event `OrderSubmitted` created an instance in the state `Submitted`, `Customer` was saved.
- An **out-of-order event** (`OrderShipped` in the state `Submitted`) did not advance the saga and caused a `NotAcceptedStateMachineException` (wrapping `UnhandledEventException`: "The ShippedEvent event is not handled during the Submitted state"); a `Fault<OrderShipped>` was published and the state stayed `Submitted`.
- After `OrderPaid` — `Paid`; after the second `OrderShipped` the saga is finalized and **removed** from the repository (`NotExists` returned `null`, as for an id that never existed).

Events acceptable in several states are, per the documentation, described explicitly (`During(Submitted, Paid, …)`) or ignored (`Ignore(event)`); without that an out-of-order event ends in an error, as in the experiment.

> ⚠️ Verify: persistent saga repositories (Entity Framework, MongoDB, Redis), optimistic locking, `Schedule`/`Request` inside a saga and `Ignore` were not verified here.

## Internals

- **A pipeline and filters.** Receiving a message passes through a chain of filters (middleware): retry, transaction, the consumer and so on. `UseMessageRetry` is one of the filters.
- **The topology mirrors message types.** For each published type an exchange `Namespace:Type` is created and the endpoints' exchanges are bound to it (verified on RabbitMQ).
- **An envelope.** A message travels in a JSON envelope with ids (`messageId`, `correlationId`, `conversationId`) and addresses (verified: the body of a message in the queue `fails_error` is JSON with `messageId`).
- **Errors.** A consumer exception → retries → `Fault<T>` + a move to `<queue>_error` with `MT-Fault-*` headers (verified).
- **The bus lifecycle.** `StartAsync` starts the endpoints, `StopAsync` stops them; in a host application a hosted service does it.

> ⚠️ Verify: the pipeline internals (contexts, filters), serializers (System.Text.Json, Newtonsoft, MessagePack), `MT-*` headers other than those verified, the prefetch mechanism and the `Scheduler` were not verified here.

## Versions and licence

- Here the **8.x** line (8.5.12) is used — per the developers' announcement the last open line.
- **Version 9** (9.3.0 on NuGet at the time of writing) is, per the developers' announcement, commercial: production use requires a licence.
- 8.x has anonymous **usage telemetry** (disabled by `DisableUsageTelemetry()` or `MASSTRANSIT_USAGE_TELEMETRY=false`).

> ⚠️ Verify: the exact licence terms of v8 and v9, the API differences between 8.x and 9.x and the telemetry's content were not verified here. Check the [announcement](https://masstransit.io/introduction/v9-announcement) and the official pages.

## Gotchas

### 1. Messages in the global namespace

Publishing fails: "Messages types must have a valid namespace" (verified).

### 2. `using` instead of `await using`

`InvalidOperationException` when disposing the container (verified).

### 3. Default queue names are not `kebab-case`

The default formatter leaves `OrderSubmitted`; `kebab-case` is enabled explicitly (verified).

### 4. Retries run in the process

The message is not returned to the queue between attempts (verified: the retry header 0, 1, 2), so long retries hold the processing thread.

### 5. A saga event out of order

`NotAcceptedStateMachineException` and a `Fault` (verified).

### 6. A test with no waiting

Checking "consumed" right after `Publish` without `Any<T>()` is a race: use the harness's waits.

### 7. Messages are not renamed

The namespace and type name are part of the address (verified in exchange names); renaming breaks compatibility.

### 8. Version 9 is commercial

Do not upgrade "by default".

## Trade-offs and alternatives

| Choice | Pros | Cons |
|---|---|---|
| MassTransit | one model over transports, retries, sagas, a test harness | a dependency and a licence (v9) |
| a broker client directly (RabbitMQ.Client) | full control | everything by hand |
| `Channel<T>` and background tasks | no broker | in-process only ([recipe](../../recipes/background-jobs/overview.en.md)) |
| another library (for example NServiceBus, Wolverine) | their own features | not tested here |

## Interview questions

<details><summary>1. How does `Publish` differ from `Send`?</summary>

`Publish` is an event for all subscribers of the type; `Send` is a command to a particular endpoint.

→ [Basics](basics.en.md#publish-and-send)

</details>

<details><summary>2. Trick: a message in the global namespace?</summary>

An error: the type must have a namespace.

→ [Gotcha 1](#1-messages-in-the-global-namespace)

</details>

<details><summary>3. What happens when retries are exhausted?</summary>

A `Fault<T>` is published, the message is moved to the `_error` queue with `MT-Fault-*` headers.

→ [Advanced](advanced.en.md#errors-faultt-and-the-_error-queue)

</details>

<details><summary>4. What does the RabbitMQ topology look like?</summary>

The message-type exchange → the endpoint's exchange → a queue (both fanout).

→ [Advanced](advanced.en.md#rabbitmq-topology)

</details>

<details><summary>5. How do you test a consumer without a broker?</summary>

`AddMassTransitTestHarness`, an in-memory bus, waits `Published/Consumed.Any<T>()`.

→ [Advanced](advanced.en.md#the-test-harness)

</details>

<details><summary>6. Trick: the container was disposed with `using`. What happens?</summary>

`InvalidOperationException`: you need `await using`.

→ [Gotcha 2](#2-using-instead-of-await-using)

</details>

<details><summary>7. What is a saga in MassTransit?</summary>

The state of a long process described by a state machine: states, events, transitions, correlation by `CorrelationId`.

→ [Sagas](#sagas-and-state-machines)

</details>

<details><summary>8. What happens if an event arrives in a state where the saga cannot take it?</summary>

`NotAcceptedStateMachineException`, a `Fault`; the state does not change — unless the event is ignored explicitly.

→ [Gotcha 5](#5-a-saga-event-out-of-order)

</details>

<details><summary>9. How do you safely guarantee at-least-once processing?</summary>

Idempotent consumers, retries and an error queue; for publishing from a database an outbox (not verified here).

→ [Advanced](advanced.en.md#best-practices)

</details>

<details><summary>10. What should you know about MassTransit versions?</summary>

8.x is the last open line (here 8.5.12); 9.x is commercial; 8.x has anonymous usage telemetry.

→ [Versions and licence](#versions-and-licence)

</details>

<details><summary>11. Trick: what does "a retry runs in the process" mean?</summary>

The message stays with the consumer between attempts; in the broker it is not returned to the queue.

→ [Gotcha 4](#4-retries-run-in-the-process)

</details>

<details><summary>12. When is MassTransit not needed?</summary>

When an in-process queue (`Channel<T>`) or a direct broker client is enough.

→ [Trade-offs](#trade-offs-and-alternatives)

</details>

## Cheat sheet

```csharp
services.AddMassTransit(x => {
    x.SetKebabCaseEndpointNameFormatter();
    x.AddConsumer<MyConsumer>();
    x.AddRequestClient<MyRequest>();
    x.UsingRabbitMq((ctx, c) => { c.Host(...); c.ReceiveEndpoint("q", e => { e.UseMessageRetry(r => r.Interval(3, 100ms)); e.ConfigureConsumer<MyConsumer>(ctx); }); });
});
await using var sp = services.BuildServiceProvider();   // await using!
// tests: AddMassTransitTestHarness + harness.Consumed.Any<T>()
```

- Events are `Publish`, commands are `Send`; a message type lives in a namespace.
- Retries + the `_error` queue; idempotent consumers.
- v8 is open, v9 is commercial.

## Related materials

- Previous level: [advanced](advanced.en.md)
- [Background jobs](../../recipes/background-jobs/overview.en.md), [Polly](../polly/overview.en.md), [Testcontainers](../testcontainers/overview.en.md)
- [RabbitMQ](../../../rabbitmq/overview.en.md), [Kafka](../../../kafka/overview.en.md)

## Sources

- [MassTransit: documentation](https://masstransit.io/documentation/concepts)
- [MassTransit: state machine sagas](https://masstransit.io/documentation/patterns/saga/state-machine)
- [MassTransit: testing](https://masstransit.io/documentation/concepts/testing)
- [Announcing MassTransit v9](https://masstransit.io/introduction/v9-announcement)
