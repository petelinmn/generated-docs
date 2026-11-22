---
title: "Exercises: Background jobs"
lang: en
track: csharp
kind: recipe
doc: exercises
slug: background-jobs
part: 2
tags: [background-service, channel, periodic-timer, graceful-shutdown, host-options]
status: draft
updated: 2026-11-22
---

# Exercises: Background jobs

🌐 [Русский](exercises.ru.md) · **English**

> Solve it yourself before opening the hints. The solutions were verified by running (`Host.CreateApplicationBuilder` / Kestrel, no packages). There are no runnable versions in `code/` (level 5 does not exist).

## 1. 🟢 A periodic worker

**Task:** a worker that writes `tick` every 400 ms and writes `stop tick` on shutdown.

**Expected result:** several `tick` in ~1.5 s, after `StopAsync` — `stop tick`.

<details><summary>Hint</summary>

`BackgroundService`, `PeriodicTimer`, `catch (OperationCanceledException)`.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 1: periodic worker with clean shutdown.
class TickWorker(Log log) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(400));
        try { while (await timer.WaitForNextTickAsync(ct)) log.Lines.Enqueue("tick"); }
        catch (OperationCanceledException) { }
        log.Lines.Enqueue("stop tick");
    }
}
```

Verified: `tick` ×4 in the log, `stop tick` after shutdown.

</details>

## 2. 🟡 A job queue over HTTP

**Task:** `POST /enqueue/{x}` puts a job into a bounded channel (capacity 2) and replies at once; a worker processes them one by one (300 ms per job).

**Expected result:** three POSTs returned `queued a/b/c`, then the log has `done a`, `done b`, `done c`.

<details><summary>Hint</summary>

`Channel.CreateBounded<string>(…)` as a singleton, `ch.Writer.WriteAsync`, in the worker `await foreach (… ch.Reader.ReadAllAsync(ct))`.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 2: HTTP endpoint -> bounded channel -> worker.
builder.Services.AddSingleton(Channel.CreateBounded<string>(new BoundedChannelOptions(2) { FullMode = BoundedChannelFullMode.Wait }));
builder.Services.AddHostedService<QueueWorker>();
app.MapPost("/enqueue/{x}", async (string x, Channel<string> ch) => { await ch.Writer.WriteAsync(x); return "queued " + x; });
```

Verified: replies `queued a`, `queued b`, `queued c`; then `done a`, `done b`, `done c`.

</details>

## 3. 🟡 Do not let the worker die

**Task:** a worker that throws an exception on every second item per tick. The exception is logged and the worker keeps going. What happens without `try/catch`?

**Expected result:** with `try/catch` the worker lives; without it the host gets a stop request.

<details><summary>Hint</summary>

`catch (Exception e) when (e is not OperationCanceledException)` inside the loop; without it — `BackgroundServiceExceptionBehavior.StopHost`.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 3: isolate a failure per unit of work.
while (await timer.WaitForNextTickAsync(ct))
{
    try { await DoWorkAsync(ct); }
    catch (Exception e) when (e is not OperationCanceledException) { logger.LogError(e, "tick failed"); }
}
```

Verified on a worker throwing `InvalidOperationException` without a catch: `ApplicationStopping` fired; with `Ignore` no stop was requested.

</details>

## 4. 🔴 The shutdown timeout

**Task:** a worker ignoring the token (`Task.Delay(5000, CancellationToken.None)`) with `ShutdownTimeout = 300 ms`. What does `StopAsync` return and how long does it wait?

**Expected result:** about 300 ms, no exception.

<details><summary>Hint</summary>

`b.Services.Configure<HostOptions>(o => o.ShutdownTimeout = …)` and a `Stopwatch` around `host.StopAsync()`.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 4: ShutdownTimeout stops the waiting, not the worker.
b.Services.Configure<HostOptions>(o => o.ShutdownTimeout = TimeSpan.FromMilliseconds(300));
b.Services.AddHostedService<Stubborn>();
var host = b.Build();
await host.StartAsync();
var sw = Stopwatch.StartNew();
await host.StopAsync();                       // returns after ~300 ms
Console.WriteLine(sw.ElapsedMilliseconds);

class Stubborn : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken ct) => await Task.Delay(5000, CancellationToken.None);
}
```

Verified: `stopped without exception in 303ms`. The worker keeps running — to make it finish on time you must pass it `ct`.

</details>
