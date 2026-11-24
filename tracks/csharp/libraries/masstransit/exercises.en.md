---
title: "Exercises: MassTransit"
lang: en
track: csharp
kind: library
doc: exercises
slug: masstransit
part: 3
tags: [masstransit, consumer, retry, request-response, test-harness]
status: draft
updated: 2026-11-24
---

# Exercises: MassTransit

🌐 [Русский](exercises.ru.md) · **English**

> Solve on your own; hints and solutions are under spoilers. A `net11.0` console project with the `MassTransit` 8.5.12 package; messages in the namespace `Shop` (see the [basics](basics.en.md#how-the-code-was-verified)). The solutions were run on .NET 11.

## 1. 🟢 The first message

**Task:** create an in-memory bus with a consumer `OrderSubmittedConsumer`, publish `OrderSubmitted` and print the customer and amount. Do not forget to dispose the container properly.

**Expected output:** a line like `alice 19.99` (the number format depends on the culture).

<details><summary>Hint</summary>

`AddMassTransit` → `AddConsumer` → `UsingInMemory(... ConfigureEndpoints)`, the bus `IBusControl`; `await using`.

</details>

<details><summary>Solution</summary>

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

See the [basics](basics.en.md#an-in-memory-bus).

</details>

## 2. 🟢 An event and a command

**Task:** add a second consumer `AuditConsumer` for the same type. Publish the message, then send it as a command to the queue `audit`. How many consumers got each?

**Expected output:** `Publish` — both consumers, `Send` — only `AuditConsumer`.

<details><summary>Hint</summary>

`bus.GetSendEndpoint(new Uri("queue:audit"))`.

</details>

<details><summary>Solution</summary>

See the [basics](basics.en.md#publish-and-send).

</details>

## 3. 🟡 Retries

**Task:** the consumer `FlakyConsumer` fails on the first two attempts. Configure the endpoint for three retries at 100 ms intervals and print the attempt numbers.

**Expected output:** attempts 1, 2, 3, success on the third (retry header 0, 1, 2).

<details><summary>Hint</summary>

An explicit endpoint: `ReceiveEndpoint("flaky", e => { e.UseMessageRetry(…); e.ConfigureConsumer<FlakyConsumer>(ctx); })`.

</details>

<details><summary>Solution</summary>

See the [advanced level](advanced.en.md#retries).

</details>

## 4. 🔴 A harness test and a saga

**Task:** (a) write a consumer check on the test harness without `Task.Delay`; (b) describe a saga "submitted → paid → shipped" and check that shipping before payment leaves the saga in the `Submitted` state.

**Expected output:** (a) `published: True`, `consumed: True`; (b) `state after OrderShipped in Submitted: still Submitted`.

<details><summary>Hint</summary>

`AddMassTransitTestHarness`, `harness.Consumed.Any<T>()`, `GetSagaStateMachineHarness<TMachine, TInstance>()` and `Exists(id, m => m.Submitted)`.

</details>

<details><summary>Solution</summary>

See the [advanced level](advanced.en.md#the-test-harness) and the [expert level](expert.en.md#sagas-and-state-machines): the full state machine and checks.

</details>
