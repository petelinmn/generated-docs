---
title: "Background jobs: production"
lang: en
track: csharp
kind: recipe
doc: advanced
slug: background-jobs
part: 2
tags: [background-service, graceful-shutdown, service-scope, host-options, channel]
versions: ".NET 11, C# 15; Microsoft.Extensions.Hosting 11.0"
status: draft
updated: 2026-11-22
---

# Background jobs: production

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Minimal](basics.en.md) · **2. Production** · [3. Scale and edges](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [the minimal solution](basics.en.md). **Time:** ~45 min.

## Scoped dependencies

A hosted service is a singleton, so a scoped service (for example `DbContext`) is not injected into its constructor. Create a scope for each unit of work:

```csharp
class ScopedWorker(IServiceScopeFactory f) : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken ct)
    {
        using var scope = f.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<Db>();
        // use db
        return Task.CompletedTask;
    }
}
```

Verified: the scoped `Db` was obtained from the scope. Injecting a scoped service straight into a singleton is a "captive dependency" ([13](../../modules/13-di-config-logging/advanced.en.md#the-captive-dependency-problem)); with scope validation enabled it is an error at startup.

## Exceptions in a worker

An unhandled exception from `ExecuteAsync` by default **stops the whole host** (`BackgroundServiceExceptionBehavior.StopHost`). Verified on a worker throwing `InvalidOperationException`:

| Setting | What happened |
|---|---|
| default (`StopHost`) | `ApplicationStopping` fired, stop was requested (`True`) |
| `o.BackgroundServiceExceptionBehavior = Ignore` | stop was not requested (`False`), the worker simply died |

```csharp
builder.Services.Configure<HostOptions>(o =>
    o.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore);
```

`Ignore` is dangerous: the worker silently stops working. It is better to catch exceptions **inside the loop** per unit of work, log them and go on:

```csharp
while (await timer.WaitForNextTickAsync(ct))
{
    try { await DoWorkAsync(ct); }
    catch (Exception e) when (e is not OperationCanceledException) { logger.LogError(e, "tick failed"); }
}
```

## Graceful shutdown

On shutdown the host cancels `ct` and waits for workers up to `HostOptions.ShutdownTimeout`. A worker should:

1. pass `ct` to every wait and exit cleanly on `OperationCanceledException`;
2. finish the current unit of work (in the queue example `Task.Delay(…, CancellationToken.None)` finishes the item already started);
3. for a queue — call `Writer.Complete()` and drain the rest if jobs must not be lost.

Verified: a worker ignoring the token (`Task.Delay(5000, CancellationToken.None)`) with `ShutdownTimeout = 300 ms` — `StopAsync` returned in about 303 ms **without an exception**: the host stopped waiting, while the worker itself keeps running until the process exits. The default timeout is not infinite, so long jobs must be interruptible.

## Several consumers and parallelism

A single `QueueWorker` processes items sequentially. For parallelism run N readers of one channel (each item goes to exactly one) or `Parallel.ForEachAsync` over `ReadAllAsync`:

```csharp
await Parallel.ForEachAsync(ch.Reader.ReadAllAsync(ct),
    new ParallelOptions { MaxDegreeOfParallelism = 4, CancellationToken = ct },
    async (item, token) => await HandleAsync(item, token));
```

The pool is bounded by `MaxDegreeOfParallelism`; together with a bounded channel this gives backpressure: the producer waits until the consumers clear the queue.

## Starting a worker

Verified on .NET 11: the host's `StartAsync` completed in ~6 ms although the synchronous part of `ExecuteAsync` took 700 ms (`Thread.Sleep`) — it ran after `StartAsync` had returned.

> ⚠️ Verify: documentation for earlier versions says the synchronous part of `ExecuteAsync` blocks the start of the other services. The .NET 11 behavior is verified, other versions are not; still begin a worker with its first `await`.

## Best practices

| Rule | Why |
|---|---|
| `try/catch` inside the loop per unit of work | one failure must not kill the worker |
| Pass `ct` everywhere | fast shutdown |
| A bounded channel | backpressure instead of memory growth |
| A scope per unit of work | the right `DbContext` lifetime |
| Log start, end, failures | observability ([22](../../modules/22-observability/overview.en.md)) |
| Idempotent jobs | repeats after a restart do no harm |
| No `Task.Run` from a request | lost work on shutdown |

## Related materials

- Back: [minimal](basics.en.md). Next: [scale and edges](expert.en.md).
- [13 · DI, configuration, logging](../../modules/13-di-config-logging/overview.en.md) — scopes and options.
- [18 · Concurrency](../../modules/18-concurrency/overview.en.md) — `Channel<T>`, `Parallel.ForEachAsync`.
- [Rate limiting](../rate-limiting/overview.en.md) — protecting the entry while the queue drains.

## Sources

- [Hosted services](https://learn.microsoft.com/aspnet/core/fundamentals/host/hosted-services)
- [`HostOptions`](https://learn.microsoft.com/dotnet/api/microsoft.extensions.hosting.hostoptions)
- [`BackgroundServiceExceptionBehavior`](https://learn.microsoft.com/dotnet/api/microsoft.extensions.hosting.backgroundserviceexceptionbehavior)
