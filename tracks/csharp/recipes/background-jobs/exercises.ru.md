---
title: "Упражнения: Фоновые задачи"
lang: ru
track: csharp
kind: recipe
doc: exercises
slug: background-jobs
part: 2
tags: [background-service, channel, periodic-timer, graceful-shutdown, host-options]
status: draft
updated: 2026-11-22
---

# Упражнения: Фоновые задачи

🌐 **Русский** · [English](exercises.en.md)

> Решайте сами, потом открывайте подсказки. Решения проверены запуском (хост `Host.CreateApplicationBuilder` / Kestrel, без пакетов). Запускаемых версий в `code/` нет (уровня 5 нет).

## 1. 🟢 Периодический воркер

**Задача:** воркер, который каждые 400 мс пишет `tick` и при остановке пишет `stop tick`.

**Ожидаемый результат:** за ~1,5 с несколько `tick`, после `StopAsync` — `stop tick`.

<details><summary>Подсказка</summary>

`BackgroundService`, `PeriodicTimer`, `catch (OperationCanceledException)`.

</details>

<details><summary>Решение</summary>

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

Проверено: в журнале `tick` ×4, после остановки — `stop tick`.

</details>

## 2. 🟡 Очередь заданий через HTTP

**Задача:** `POST /enqueue/{x}` кладёт задание в ограниченный канал (ёмкость 2) и сразу отвечает; воркер обрабатывает по одному (300 мс на задание).

**Ожидаемый результат:** три POST вернули `queued a/b/c`, затем в журнале `done a`, `done b`, `done c`.

<details><summary>Подсказка</summary>

`Channel.CreateBounded<string>(…)` как singleton, `ch.Writer.WriteAsync`, в воркере `await foreach (… ch.Reader.ReadAllAsync(ct))`.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 2: HTTP endpoint -> bounded channel -> worker.
builder.Services.AddSingleton(Channel.CreateBounded<string>(new BoundedChannelOptions(2) { FullMode = BoundedChannelFullMode.Wait }));
builder.Services.AddHostedService<QueueWorker>();
app.MapPost("/enqueue/{x}", async (string x, Channel<string> ch) => { await ch.Writer.WriteAsync(x); return "queued " + x; });
```

Проверено: ответы `queued a`, `queued b`, `queued c`; затем `done a`, `done b`, `done c`.

</details>

## 3. 🟡 Не дать воркеру умереть

**Задача:** воркер, который раз в тик бросает исключение на каждом втором элементе. Исключение логируется, воркер продолжает работу. Что произойдёт без `try/catch`?

**Ожидаемый результат:** с `try/catch` воркер живёт; без него — хост получает запрос остановки.

<details><summary>Подсказка</summary>

`catch (Exception e) when (e is not OperationCanceledException)` внутри цикла; без него — `BackgroundServiceExceptionBehavior.StopHost`.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 3: isolate a failure per unit of work.
while (await timer.WaitForNextTickAsync(ct))
{
    try { await DoWorkAsync(ct); }
    catch (Exception e) when (e is not OperationCanceledException) { logger.LogError(e, "tick failed"); }
}
```

Проверено на воркере, бросающем `InvalidOperationException` без перехвата: сработал `ApplicationStopping`; при `Ignore` остановка не запрашивалась.

</details>

## 4. 🔴 Таймаут остановки

**Задача:** воркер, игнорирующий токен (`Task.Delay(5000, CancellationToken.None)`), при `ShutdownTimeout = 300 мс`. Что вернёт `StopAsync` и сколько будет ждать?

**Ожидаемый результат:** около 300 мс, без исключения.

<details><summary>Подсказка</summary>

`b.Services.Configure<HostOptions>(o => o.ShutdownTimeout = …)` и замер `Stopwatch` вокруг `host.StopAsync()`.

</details>

<details><summary>Решение</summary>

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

Проверено: `stopped without exception in 303ms`. Воркер продолжает работать — чтобы он завершался вовремя, нужно передавать ему `ct`.

</details>
