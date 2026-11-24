---
title: "MassTransit: advanced"
lang: en
track: csharp
kind: library
doc: advanced
slug: masstransit
part: 3
tags: [masstransit, retry, fault, error-queue, request-response, rabbitmq, test-harness, concurrency]
versions: ".NET 11, C# 15; MassTransit 8.5.12"
status: draft
updated: 2026-11-24
---

# MassTransit: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

Builds on the [basics](basics.en.md). The code was run on .NET 11 with MassTransit 8.5.12; RabbitMQ — 4.4.1 in Docker. The helper `Build(...)` creates a `ServiceCollection` with `AddMassTransit` and an in-memory bus.

## Retries

A consumer's transient failure is cured by a retry: `UseMessageRetry` on an endpoint.

```csharp
cfg.ReceiveEndpoint("flaky", e =>
{
    e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromMilliseconds(100)));
    e.ConfigureConsumer<FlakyConsumer>(ctx);
});
```

The consumer fails on the first two attempts:

```text
[FlakyConsumer] attempt 1 (retry attempt header: 0)
[FlakyConsumer] attempt 2 (retry attempt header: 1)
[FlakyConsumer] attempt 3 (retry attempt header: 2)
[FlakyConsumer] success
```

The same `Consume` was called three times, the retry number is available as `context.GetRetryAttempt()`. Retries run **in the process**, on the same message, without returning it to the queue. So the handler must be idempotent: partly done work will be repeated.

Policies: `Immediate(n)`, `Interval(n, pause)`, `Exponential(...)` and others; `Handle<T>`/`Ignore<T>` choose the exceptions to retry. Compare [Polly](../polly/overview.en.md): the same idea at the endpoint level.

## Errors, `Fault<T>` and the `_error` queue

When retries are exhausted the message is **not lost**: MassTransit publishes a `Fault<T>` event and moves the message to an error queue.

```csharp
e.UseMessageRetry(r => r.Immediate(2));      // always failing consumer
```

```text
[FaultObserver] Fault<OrderSubmitted>: System.InvalidOperationException : boom
attempts: 3
```

Three calls (the first and two retries), then a subscriber `IConsumer<Fault<OrderSubmitted>>` got `Fault<OrderSubmitted>`. In RabbitMQ the message landed in the queue **`fails_error`** (for the endpoint `fails`) with diagnostic headers:

```text
MT-Reason: fault
MT-Fault-ConsumerType: Shop.AlwaysFailsConsumer
MT-Fault-ExceptionType: System.InvalidOperationException
MT-Fault-Message: boom
MT-Fault-MessageType: Shop.OrderSubmitted
MT-Fault-RetryCount: 2
MT-Fault-InputAddress: rabbitmq://localhost/fails
MT-Fault-StackTrace: ...
MT-Host-MassTransitVersion: 8.5.12.0   (and other MT-Host-*)
```

The error queue is the analogue of a dead-letter queue ([RabbitMQ: DLX](../../../rabbitmq/modules/06-dead-lettering-and-retries/overview.en.md)): it shows what broke and the message can be returned to processing by hand. Together with the exchange `fails_error` the broker got the fault-event exchange `MassTransit:Fault--Shop:OrderSubmitted--`.

> ⚠️ Verify: the behaviour of `UseMessageRedelivery` (a delayed retry), `_skipped` queues for messages with no suitable consumer and `UseInMemoryOutbox` was not verified here.

## Request and response

`IRequestClient<TRequest>` sends a request and waits for a typed response:

```csharp
public class CheckOrderConsumer : IConsumer<CheckOrder>
{
    public Task Consume(ConsumeContext<CheckOrder> context) =>
        context.RespondAsync(new OrderStatus(context.Message.OrderId, "Accepted"));
}

x.AddRequestClient<CheckOrder>();
var resp = await client.GetResponse<OrderStatus>(new CheckOrder(id));
```

```text
response: Accepted
no consumer -> RequestTimeoutException: Timeout waiting for response, RequestId: c0200000-...
```

With no consumer the client gets a `RequestTimeoutException` after the set timeout (in the experiment `RequestTimeout.After(s: 2)`). Request-response couples services in time just like RPC ([RabbitMQ: patterns](../../../rabbitmq/modules/04-messaging-patterns/overview.en.md)): set timeouts and do not overuse it.

> ⚠️ Verify: the default request timeout (30 seconds per the documentation) was not verified here.

## Concurrency

`ConcurrentMessageLimit` bounds how many messages an endpoint processes at once. 8 messages of 200 ms:

```text
ConcurrentMessageLimit=1: max parallel = 1, total 1051 ms
ConcurrentMessageLimit=4: max parallel = 4, total 744 ms
```

At limit 1 messages are processed one after another, at 4 up to four at once. The default value was not measured here: only the limits 1 and 4 were checked. Since processing can run in parallel, state shared between calls needs synchronisation.

## RabbitMQ topology

For the RabbitMQ transport:

```csharp
x.SetKebabCaseEndpointNameFormatter();
x.UsingRabbitMq((ctx, c) =>
{
    c.Host("localhost", "/", h => { h.Username("guest"); h.Password("guest"); });
    c.ConfigureEndpoints(ctx);
});
```

After publishing `OrderSubmitted` for two consumers the broker held:

```text
exchanges Shop:OrderSubmitted fanout
exchanges audit fanout
exchanges order-submitted fanout
queues audit classic (x-queue-type: classic)
queues order-submitted classic (x-queue-type: classic)
binding Shop:OrderSubmitted -> audit exchange
binding Shop:OrderSubmitted -> order-submitted exchange
binding audit -> audit queue
binding order-submitted -> order-submitted queue
```

The chain is: the message-type exchange `Shop:OrderSubmitted` (fanout) → the endpoint's exchange (fanout) → the queue. The publisher knows only the message type, and each consumer has its own queue ([RabbitMQ: exchanges](../../../rabbitmq/modules/02-exchanges-and-routing/overview.en.md)). The queues are `classic`.

> ⚠️ Verify: quorum queues (`SetQuorumQueue`) and other RabbitMQ transport settings were not verified.

## The test harness

For tests there is `AddMassTransitTestHarness` — an in-memory bus with observation of messages:

```csharp
services.AddMassTransitTestHarness(x => x.AddConsumer<OrderSubmittedConsumer>());
await using var sp = services.BuildServiceProvider(true);
var harness = sp.GetRequiredService<ITestHarness>();
await harness.Start();
await harness.Bus.Publish(order);
Console.WriteLine(await harness.Published.Any<OrderSubmitted>());
Console.WriteLine(await harness.Consumed.Any<OrderSubmitted>());
var ch = harness.GetConsumerHarness<OrderSubmittedConsumer>();
Console.WriteLine(await ch.Consumed.Any<OrderSubmitted>());
```

```text
published: True
consumed : True
consumer harness consumed: True
```

`Published`, `Consumed`, `Sent` and a single consumer's harness (`GetConsumerHarness`) wait for events with a timeout: a test need not be "put to sleep" with `Task.Delay`. Details and sagas are in the [expert theory](expert.en.md).

## Usage telemetry

8.x has anonymous **usage telemetry**: per the documentation the data is sent once, five minutes after the bus starts; it is disabled by `DisableUsageTelemetry()` in the configuration or by the environment variable `MASSTRANSIT_USAGE_TELEMETRY=false`. The experiments lasted under five minutes, and sending was not verified.

> ⚠️ Verify: the content of the data sent was not verified here; disable telemetry if your organisation's policy requires it.

## Best practices

- Consumers are idempotent: retries and at-least-once delivery.
- Set a retry policy and watch the `_error` queue.
- For tests use the test harness, not `Task.Delay`.
- Commands are `Send`, events are `Publish`; message types live in stable namespaces (they are part of the address).
- Do not put excess data or mutable references into messages.
- `kebab-case` for queue names (or one shared formatter across the team).

## Common mistakes

- ❌ Not configuring retries: the message goes straight to `_error` → ✅ `UseMessageRetry`.
- ❌ Renaming a message's namespace → ✅ the exchange address changes; old and new versions stop seeing each other.
- ❌ A `GetResponse` without a timeout → ✅ `RequestTimeout`.
- ❌ Tests built on `Task.Delay` → ✅ `harness.Consumed.Any<T>()`.

## Related materials

- Before: [basics](basics.en.md)
- Next: [expert level](expert.en.md) — sagas, internals, gotchas
- [Polly](../polly/overview.en.md) — retries outside a broker; [Testcontainers](../testcontainers/overview.en.md) — a real broker in tests
- [RabbitMQ: reliability](../../../rabbitmq/modules/05-reliability/overview.en.md)

## Sources

- [MassTransit: exceptions and retry](https://masstransit.io/documentation/concepts/exceptions)
- [MassTransit: request-response](https://masstransit.io/documentation/concepts/requests)
- [MassTransit: RabbitMQ transport](https://masstransit.io/documentation/transports/rabbitmq)
- [MassTransit: testing](https://masstransit.io/documentation/concepts/testing)
- [MassTransit: usage telemetry](https://masstransit.io/documentation/configuration/usage-telemetry)
