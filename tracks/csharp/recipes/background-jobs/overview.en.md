---
title: "Background jobs"
lang: en
track: csharp
kind: recipe
doc: overview
slug: background-jobs
part: 2
levels: [1, 2, 3, 4]
tags: [background-service, hosted-service, channel, periodic-timer, graceful-shutdown, outbox]
versions: ".NET 11, C# 15; Microsoft.Extensions.Hosting 11.0"
status: draft
updated: 2026-11-22
---

# Background jobs

🌐 [Русский](overview.ru.md) · **English**

## In a nutshell

A recipe: do work outside an HTTP request — periodically (`BackgroundService` + `PeriodicTimer`) and from a queue (`Channel<T>`). A worker lives with the application, receives a shutdown token and creates a scope for scoped dependencies. An in-memory queue is simple but does not survive a restart and is not distributed: critical jobs need an outbox or a broker.

## Key ideas

- `AddHostedService<T>()` + `BackgroundService.ExecuteAsync(ct)`.
- `PeriodicTimer` — periodicity without accumulating ticks.
- A bounded `Channel<T>` — a queue with backpressure.
- A scope per unit of work through `IServiceScopeFactory`.
- An unhandled exception stops the host by default.
- Graceful shutdown: `ct`, finish the work, `Complete()`; `ShutdownTimeout` does not kill the worker.
- Several instances = several timers and queues.

## Minimal example

```csharp
class TickWorker(Log log) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(400));
        try { while (await timer.WaitForNextTickAsync(ct)) log.Lines.Enqueue("tick"); }
        catch (OperationCanceledException) { }
    }
}
builder.Services.AddHostedService<TickWorker>();
```

## When to use and when not

- ✅ Periodic cleanup, synchronization, cache warm-up.
- ✅ "Reply now, process later" for non-critical jobs.
- ✅ Processing a queue with bounded parallelism.
- ❌ Jobs that must not be lost (you need an outbox/broker).
- ❌ A job must run exactly once across several instances — without a lock.
- ❌ Heavy work that hurts request serving — move it to a separate process.

## Levels of this material

| Level | What's inside |
|---|---|
| [1. Minimal](basics.en.md) | `BackgroundService`, `PeriodicTimer`, `Channel<T>`, backpressure |
| [2. Production](advanced.en.md) | scoped dependencies, exceptions, graceful shutdown, parallelism |
| [3. Scale and edges](expert.en.md) | durable queues, several instances, gotchas, 10 interview questions |
| [4. Self-check](quiz.en.md) | 10 questions and [4 exercises](exercises.en.md) with solutions |
| 5. Runnable examples | not generated yet — `/learn csharp background-jobs level 5` |
| 6. Tests | not generated yet — `/learn csharp background-jobs level 6` |

## Related materials

- Prerequisites: [10 · async/await](../../modules/10-async-await/overview.en.md), [13 · DI, configuration, logging](../../modules/13-di-config-logging/overview.en.md)
- Nearby: [18 · Concurrency](../../modules/18-concurrency/overview.en.md), [Rate limiting](../rate-limiting/overview.en.md)
- In other tracks: [Python](../../../python/overview.en.md) — background tasks in asyncio; [Kubernetes · Pod](../../../kubernetes/modules/04-pods/overview.en.md) — `SIGTERM` and pod termination that a worker's graceful shutdown relies on; [Kubernetes · Other workloads](../../../kubernetes/modules/12-workloads/overview.en.md) — Job and CronJob as an external alternative
- Recipes and libraries: [Serilog](../../libraries/serilog/overview.en.md), [Caching](../caching/overview.en.md), [File upload](../file-upload/overview.en.md), [Resilient HttpClient](../http-client-resilience/overview.en.md), [Real-time with SignalR](../signalr-realtime/overview.en.md), [MassTransit](../../libraries/masstransit/overview.en.md) — when a queue is needed between services
- [System design: messaging and events](../../../system-design/modules/08-messaging-and-events/overview.en.md)
