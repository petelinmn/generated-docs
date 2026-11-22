---
title: "Фоновые задачи: минимальное решение"
lang: ru
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

# Фоновые задачи: минимальное решение

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [обзор](overview.ru.md) · **1. Минимум** · [2. Production](advanced.ru.md) · [3. Масштаб и грани](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [10 · async/await](../../modules/10-async-await/overview.ru.md), [13 · DI, конфигурация, логирование](../../modules/13-di-config-logging/overview.ru.md). **Время:** ~30 мин.

## Задача

Выполнять работу вне HTTP-запроса: периодически (очистка, синхронизация) и по очереди (отправить письмо, обработать файл), не блокируя ответ клиенту. В .NET для этого есть хостируемые сервисы (hosted services): они запускаются и останавливаются вместе с приложением.

## Периодическая работа: `BackgroundService` + `PeriodicTimer`

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

`ExecuteAsync` живёт, пока работает приложение; токен `ct` отменяется при остановке. `PeriodicTimer` ждёт следующий тик асинхронно и не накапливает пропущенные тики, если обработка длиннее периода.

## Очередь: `Channel<T>`

Контроллер кладёт задание в очередь и сразу отвечает, воркер забирает по одному:

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

Проверено запуском (Kestrel + `HttpClient`, три POST подряд): ответы `queued a`, `queued b`, `queued c`; в журнале воркеров идут `tick` и `done a`, `done b`, `done c` по одному — запросы вернулись сразу, работа выполнена позже. После `StopAsync` оба воркера корректно завершились (`stop tick` записан).

## Ограниченная очередь и обратное давление

`CreateBounded(2)` — очередь на 2 элемента; при заполнении поведение задаёт `FullMode`. Проверено:

| Операция | Результат |
|---|---|
| `TryWrite` ×3 в канал ёмкости 2 | `True True False` |
| `await WriteAsync(3)` при полной очереди | остаётся в ожидании, пока читатель не заберёт элемент (после `ReadAsync` — завершился) |
| `FullMode = DropOldest`, записали 1..4 | остались `3,4` |
| `Writer.Complete()` и затем `ReadAllAsync` | читатель дочитывает оставшееся (`2,3`) и завершается |

Неограниченная очередь (`CreateUnbounded`) при медленном воркере растёт без предела — это утечка памяти.

## Частые ошибки новичка

- ❌ `Task.Run(() => …)` в обработчике запроса «на потом» → ✅ работа теряется при остановке и нет контроля; нужен хостируемый сервис.
- ❌ Исключение в воркере без `try/catch` → ✅ по умолчанию остановит весь хост ([production](advanced.ru.md#исключения-в-воркере)).
- ❌ Scoped-сервис (например `DbContext`) в конструкторе воркера → ✅ создавать scope через `IServiceScopeFactory` ([production](advanced.ru.md#scoped-зависимости)).
- ❌ Игнорировать `CancellationToken` → ✅ передавать его во все ожидания.
- ❌ Неограниченная очередь → ✅ `CreateBounded`.

## Связанные материалы

- Дальше: [production](advanced.ru.md) — scoped-зависимости, исключения, graceful shutdown, несколько потребителей.
- [10 · async/await](../../modules/10-async-await/overview.ru.md) — отмена и `await`.
- [13 · DI, конфигурация, логирование](../../modules/13-di-config-logging/overview.ru.md) — время жизни сервисов и хостинг.
- [18 · Конкурентность](../../modules/18-concurrency/overview.ru.md) — `Channel<T>` и примитивы синхронизации.
- [Python](../../../python/overview.ru.md) — фоновые задачи в asyncio.

## Источники

- [Background tasks with hosted services](https://learn.microsoft.com/aspnet/core/fundamentals/host/hosted-services)
- [`BackgroundService`](https://learn.microsoft.com/dotnet/api/microsoft.extensions.hosting.backgroundservice)
- [Channels](https://learn.microsoft.com/dotnet/core/extensions/channels)
- [`PeriodicTimer`](https://learn.microsoft.com/dotnet/api/system.threading.periodictimer)
