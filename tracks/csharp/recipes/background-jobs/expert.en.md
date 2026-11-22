---
title: "Background jobs: scale and edges"
lang: en
track: csharp
kind: recipe
doc: expert
slug: background-jobs
part: 2
tags: [background-service, hangfire, quartz, outbox, durable-queue, interview-questions]
versions: ".NET 11, C# 15; Microsoft.Extensions.Hosting 11.0"
status: draft
updated: 2026-11-22
---

# Background jobs: scale and edges

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Minimal](basics.en.md) · [2. Production](advanced.en.md) · **3. Scale and edges** · [4. Quiz](quiz.en.md)

**Prerequisites:** [minimal](basics.en.md) and [production](advanced.en.md). **Time:** ~30 min.

## Scale: when `Channel<T>` is not enough

An in-process queue **does not survive a restart**: on a crash or deploy all unprocessed jobs are lost. And each application instance has its own queue.

| Need | Solution |
|---|---|
| jobs must not be lost | a durable queue: a DB table (outbox), a broker (RabbitMQ, Azure Service Bus, Kafka) |
| a schedule (cron), retries, a dashboard | scheduler libraries (Hangfire, Quartz.NET) |
| several instances, a job runs once | a DB lock/lease or a shared broker with competing consumers |
| long and resource-hungry jobs | a separate worker process (Worker Service, container, Kubernetes Job/CronJob) |
| only "fire and forget", loss is acceptable | `Channel<T>` + `BackgroundService` |

> ⚠️ Verify: the capabilities of Hangfire/Quartz.NET (storages, API) were not run in this material — choose by their documentation.

## Edges and gotchas

**1. A periodic timer and several instances.** A periodic job starts in every instance; without a lock the cleanup/mailing runs N times.

**2. `PeriodicTimer` does not accumulate ticks.** If processing is longer than the period, the missed ticks do not pile up — the next `WaitForNextTickAsync` completes at the nearest tick; there is no storm of ticks.

**3. An exception kills the host by default.** Verified: `StopHost` stops the application; `Ignore` leaves the worker dead without noise ([production](advanced.en.md#exceptions-in-a-worker)).

**4. `ShutdownTimeout` does not kill the worker.** Verified: `StopAsync` returned in ~300 ms without an exception, and a worker that ignores the token kept running.

**5. Losing an item on shutdown.** After `Writer.Complete()` the reader drains the rest (verified: `2,3`), but on a hard crash the in-memory queue is lost.

**6. An unbounded queue.** A slow consumer — memory grows up to `OutOfMemory`; bound the capacity and choose `FullMode` deliberately (`Wait` — pressure on the producer, `DropOldest` — old items are lost, verified: `3,4` remained).

**7. `async void` and "forgotten" tasks.** A task started without `await` loses its exception; a hosted service gives the lifecycle an owner.

**8. Scoped in a singleton.** Injecting `DbContext` into a worker directly is a lifetime error; use `IServiceScopeFactory`.

**9. A long synchronous part of `ExecuteAsync`.** Verified on .NET 11: `StartAsync` does not block; on other versions the behavior differs ([production](advanced.en.md#starting-a-worker)).

## Trade-offs and alternatives

| Approach | Pros | Cons |
|---|---|---|
| `BackgroundService` + `Channel<T>` | simple, no dependencies | not durable, not distributed |
| Outbox + worker | atomic with the DB transaction, not lost | needs a table and polling |
| A message broker | scale, retries, service decoupling | infrastructure |
| Hangfire/Quartz | schedule, retries, dashboard | a dependency and storage |
| Kubernetes CronJob | isolation, separate scaling | operational complexity |

## Interview questions

<details><summary>1. How do you run work in the background in ASP.NET Core?</summary>

A hosted service (`IHostedService`/`BackgroundService`) registered through `AddHostedService`; a queue — `Channel<T>`; periodicity — `PeriodicTimer`.

</details>

<details><summary>2. How does `BackgroundService` differ from `IHostedService`?</summary>

`BackgroundService` is a ready base: it implements `StartAsync/StopAsync`, and you write `ExecuteAsync(CancellationToken)`; `IHostedService` is the low-level interface with `StartAsync`/`StopAsync`.

</details>

<details><summary>3. How do you get a `DbContext` in a worker?</summary>

Through `IServiceScopeFactory.CreateScope()` per unit of work (verified: the scoped service was obtained from the scope).

</details>

<details><summary>4. What happens if `ExecuteAsync` throws?</summary>

By default the host stops (`StopHost`, verified: `ApplicationStopping`); with `Ignore` the worker silently dies.

</details>

<details><summary>5. How do you stop a worker gracefully?</summary>

Cancel `ct`, exit on `OperationCanceledException`, finish the current work, for a queue — `Complete()`; waiting is bounded by `HostOptions.ShutdownTimeout`.

</details>

<details><summary>6. Why is `PeriodicTimer` better than `Timer`/`Task.Delay` in a loop?</summary>

Asynchronous waiting, cancellation support, no accumulated ticks, no overlapping calls (the next tick waits for processing to finish).

</details>

<details><summary>7. What does a bounded `Channel<T>` do when full?</summary>

It depends on `FullMode`: `Wait` — `WriteAsync` waits (`TryWrite` returns `false`, verified), `DropOldest/DropNewest/DropWrite` — drops items.

</details>

<details><summary>8. Why is an in-memory queue not enough for critical jobs?</summary>

It is lost on a restart/crash and belongs to one instance; you need a durable queue (outbox, broker).

</details>

<details><summary>9. Trick question: the app runs in 3 instances, a `BackgroundService` sends emails hourly. How many emails does a customer get?</summary>

Three: the timer runs in every instance. You need a lock/lease, a single worker process or a scheduler with coordination.

</details>

<details><summary>10. Trick question: `ShutdownTimeout` elapsed — is the worker killed?</summary>

No. The host stops waiting (verified: `StopAsync` returned without an exception in ~300 ms), but a worker that ignores the token keeps running until the process exits.

</details>

## Related materials

- Back: [production](advanced.en.md). Check yourself: [quiz](quiz.en.md) and [exercises](exercises.en.md).
- [21 · Application architecture](../../modules/21-architecture/overview.en.md) — outbox and service boundaries.
- [18 · Concurrency](../../modules/18-concurrency/overview.en.md) — `Channel<T>`.
- [Kubernetes](../../../kubernetes/overview.en.md) — Jobs and CronJobs for separate workers.

## Sources

- [Hosted services](https://learn.microsoft.com/aspnet/core/fundamentals/host/hosted-services)
- [Channels](https://learn.microsoft.com/dotnet/core/extensions/channels)
- [Worker Services](https://learn.microsoft.com/dotnet/core/extensions/workers)
