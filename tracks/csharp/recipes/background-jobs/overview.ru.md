---
title: "Фоновые задачи"
lang: ru
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

# Фоновые задачи

🌐 **Русский** · [English](overview.en.md)

## Коротко

Рецепт: выполнять работу вне HTTP-запроса — периодически (`BackgroundService` + `PeriodicTimer`) и по очереди (`Channel<T>`). Воркер живёт вместе с приложением, получает токен остановки, для scoped-зависимостей создаёт scope. Очередь в памяти проста, но не переживает перезапуск и не распределена: для критичных заданий нужен outbox или брокер.

## Ключевые идеи

- `AddHostedService<T>()` + `BackgroundService.ExecuteAsync(ct)`.
- `PeriodicTimer` — периодичность без накопления тиков.
- `Channel<T>` ограниченной ёмкости — очередь с обратным давлением.
- Scope на единицу работы через `IServiceScopeFactory`.
- Необработанное исключение по умолчанию останавливает хост.
- Graceful shutdown: `ct`, дописать работу, `Complete()`; `ShutdownTimeout` не убивает воркер.
- Несколько экземпляров = несколько таймеров и очередей.

## Минимальный пример

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

## Когда применять, а когда нет

- ✅ Периодическая уборка, синхронизация, прогрев кэша.
- ✅ «Ответить сразу, обработать позже» для некритичных заданий.
- ✅ Обработка очереди с ограничением параллелизма.
- ❌ Задания, которые нельзя терять (нужен outbox/брокер).
- ❌ Задание должно выполниться ровно один раз на нескольких экземплярах — без блокировки.
- ❌ Тяжёлая работа, мешающая обслуживать запросы — вынесите в отдельный процесс.

## Уровни материала

| Уровень | Что внутри |
|---|---|
| [1. Минимум](basics.ru.md) | `BackgroundService`, `PeriodicTimer`, `Channel<T>`, обратное давление |
| [2. Production](advanced.ru.md) | scoped-зависимости, исключения, graceful shutdown, параллелизм |
| [3. Масштаб и грани](expert.ru.md) | durable-очереди, несколько экземпляров, подвохи, 10 вопросов с собеседований |
| [4. Самопроверка](quiz.ru.md) | 10 вопросов и [4 упражнения](exercises.ru.md) с решениями |
| 5. Запускаемые примеры | не сгенерирован — `/learn csharp background-jobs level 5` |
| 6. Тесты | не сгенерирован — `/learn csharp background-jobs level 6` |

## Связанные материалы

- Пререквизиты: [10 · async/await](../../modules/10-async-await/overview.ru.md), [13 · DI, конфигурация, логирование](../../modules/13-di-config-logging/overview.ru.md)
- Рядом: [18 · Конкурентность](../../modules/18-concurrency/overview.ru.md), [Rate limiting](../rate-limiting/overview.ru.md)
- В других треках: [Python](../../../python/overview.ru.md) — фоновые задачи в asyncio; [Kubernetes · Pod](../../../kubernetes/modules/04-pods/overview.ru.md) — `SIGTERM` и остановка пода, на которых держится graceful shutdown воркера; [Kubernetes · Другие workload-ы](../../../kubernetes/modules/12-workloads/overview.ru.md) — Job и CronJob как внешняя альтернатива
- Рецепты и библиотеки: [Serilog](../../libraries/serilog/overview.ru.md), [Кэширование](../caching/overview.ru.md), [Загрузка файлов](../file-upload/overview.ru.md), [Устойчивый HttpClient](../http-client-resilience/overview.ru.md), [Real-time через SignalR](../signalr-realtime/overview.ru.md), [MassTransit](../../libraries/masstransit/overview.ru.md) — когда очередь нужна между сервисами
- [System design: сообщения и события](../../../system-design/modules/08-messaging-and-events/overview.ru.md)
