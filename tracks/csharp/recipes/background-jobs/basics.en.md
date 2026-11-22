---
title: "Background jobs: the minimal solution"
lang: en
track: csharp
kind: recipe
doc: basics
slug: background-jobs
part: 2
tags: [background-service, hosted-service, periodic-timer, channel, aspnet-core]
versions: ".NET 11, C# 15; Microsoft.Extensions.Hosting 11.0"
status: draft
updated: 2026-11-22
---

# Background jobs: the minimal solution

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Minimal** · [2. Production](advanced.en.md) · [3. Scale and edges](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [10 · async/await](../../modules/10-async-await/overview.en.md), [13 · DI, configuration, logging](../../modules/13-di-config-logging/overview.en.md). **Time:** ~30 min.

## The task

Do work outside an HTTP request: periodically (cleanup, synchronization) and from a queue (send an email, process a file) without blocking the client's response. .NET has hosted services for this: they start and stop together with the application.

## Periodic work: `BackgroundService` + `PeriodicTimer`

```csharp
class TickWorker(Log log) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(400));
        try { while (await timer.WaitForNextTickAsync(ct)) log.Lines.Enqueue("tick"); }
        catch (OperationCanceledException) { }   // normal shutdown
        log.Lines.Enqueue("stop tick");
    }
}

builder.Services.AddHostedService<TickWorker>();
```

`ExecuteAsync` lives as long as the application runs; the token `ct` is cancelled on shutdown. `PeriodicTimer` waits for the next tick asynchronously and does not accumulate missed ticks if processing is longer than the period.

## A queue: `Channel<T>`

The endpoint puts a job into the queue and replies at once; the worker takes jobs one at a time:

```csharp
builder.Services.AddSingleton(Channel.CreateBounded<string>(new BoundedChannelOptions(2) { FullMode = BoundedChannelFullMode.Wait }));
builder.Services.AddHostedService<QueueWorker>();

app.MapPost("/enqueue/{x}", async (string x, Channel<string> ch) => { await ch.Writer.WriteAsync(x); return "queued " + x; });

class QueueWorker(Channel<string> ch, Log log) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        await foreach (var item in ch.Reader.ReadAllAsync(ct))
        {
            await Task.Delay(300, CancellationToken.None);   // the work
            log.Lines.Enqueue("done " + item);
        }
    }
}
```

Verified by running (Kestrel + `HttpClient`, three POSTs in a row): the replies are `queued a`, `queued b`, `queued c`; the worker log shows `tick` and `done a`, `done b`, `done c` one by one — the requests returned at once, the work was done later. After `StopAsync` both workers finished cleanly (`stop tick` was written).

## A bounded queue and backpressure

`CreateBounded(2)` is a queue for 2 items; when it is full, `FullMode` decides the behavior. Verified:

| Operation | Result |
|---|---|
| `TryWrite` ×3 into a channel of capacity 2 | `True True False` |
| `await WriteAsync(3)` when the queue is full | stays pending until the reader takes an item (after `ReadAsync` — it completed) |
| `FullMode = DropOldest`, wrote 1..4 | `3,4` remained |
| `Writer.Complete()` and then `ReadAllAsync` | the reader drains the rest (`2,3`) and finishes |

An unbounded queue (`CreateUnbounded`) with a slow worker grows without limit — a memory leak.

## Beginner mistakes

- ❌ `Task.Run(() => …)` "for later" in a request handler → ✅ the work is lost on shutdown and there is no control; use a hosted service.
- ❌ An exception in a worker without `try/catch` → ✅ by default it stops the whole host ([production](advanced.en.md#exceptions-in-a-worker)).
- ❌ A scoped service (for example `DbContext`) in the worker constructor → ✅ create a scope through `IServiceScopeFactory` ([production](advanced.en.md#scoped-dependencies)).
- ❌ Ignoring `CancellationToken` → ✅ pass it to every wait.
- ❌ An unbounded queue → ✅ `CreateBounded`.

## Related materials

- Next: [production](advanced.en.md) — scoped dependencies, exceptions, graceful shutdown, several consumers.
- [10 · async/await](../../modules/10-async-await/overview.en.md) — cancellation and `await`.
- [13 · DI, configuration, logging](../../modules/13-di-config-logging/overview.en.md) — service lifetimes and hosting.
- [18 · Concurrency](../../modules/18-concurrency/overview.en.md) — `Channel<T>` and synchronization primitives.
- [Python](../../../python/overview.en.md) — background tasks in asyncio.

## Sources

- [Background tasks with hosted services](https://learn.microsoft.com/aspnet/core/fundamentals/host/hosted-services)
- [`BackgroundService`](https://learn.microsoft.com/dotnet/api/microsoft.extensions.hosting.backgroundservice)
- [Channels](https://learn.microsoft.com/dotnet/core/extensions/channels)
- [`PeriodicTimer`](https://learn.microsoft.com/dotnet/api/system.threading.periodictimer)
