---
title: "Фоновые задачи: production"
lang: ru
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

# Фоновые задачи: production

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Минимум](basics.ru.md) · **2. Production** · [3. Масштаб и грани](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [минимальное решение](basics.ru.md). **Время:** ~45 мин.

## Scoped-зависимости

Хостируемый сервис — singleton, поэтому scoped-сервис (например `DbContext`) в конструктор не внедряется. Для каждой единицы работы создают scope:

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

Проверено: scoped `Db` из scope получен. Внедрение scoped-сервиса напрямую в singleton — «захваченная зависимость» ([13](../../modules/13-di-config-logging/advanced.ru.md#проблема-captive-dependency)); при включённой валидации scopes это ошибка при запуске.

## Исключения в воркере

Необработанное исключение из `ExecuteAsync` по умолчанию **останавливает весь хост** (`BackgroundServiceExceptionBehavior.StopHost`). Проверено на воркере, бросающем `InvalidOperationException`:

| Настройка | Что произошло |
|---|---|
| по умолчанию (`StopHost`) | сработал `ApplicationStopping`, остановка запрошена (`True`) |
| `o.BackgroundServiceExceptionBehavior = Ignore` | остановка не запрошена (`False`), воркер просто умер |

```csharp
builder.Services.Configure<HostOptions>(o =>
    o.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore);
```

`Ignore` опасен: воркер молча перестаёт работать. Правильнее ловить исключения **внутри цикла** на каждую единицу работы, логировать и продолжать:

```csharp
while (await timer.WaitForNextTickAsync(ct))
{
    try { await DoWorkAsync(ct); }
    catch (Exception e) when (e is not OperationCanceledException) { logger.LogError(e, "tick failed"); }
}
```

## Graceful shutdown

При остановке хост отменяет `ct` и ждёт воркеры до `HostOptions.ShutdownTimeout`. Воркер должен:

1. передавать `ct` во все ожидания и корректно выходить по `OperationCanceledException`;
2. дописывать текущую единицу работы (в примере очереди — `Task.Delay(…, CancellationToken.None)` дорабатывает начатый элемент);
3. для очереди — вызвать `Writer.Complete()` и дочитать остаток, если нельзя терять задания.

Проверено: воркер, игнорирующий токен (`Task.Delay(5000, CancellationToken.None)`), при `ShutdownTimeout = 300 мс` — `StopAsync` вернулся примерно за 303 мс **без исключения**: хост перестал ждать, а сам воркер остаётся работать, пока процесс не завершится. Таймаут по умолчанию — не бесконечность, поэтому долгие задания надо делать прерываемыми.

## Несколько потребителей и параллелизм

Один `QueueWorker` обрабатывает элементы последовательно. Для параллелизма запускают N читателей одного канала (элемент достаётся ровно одному) или `Parallel.ForEachAsync` по `ReadAllAsync`:

```csharp
await Parallel.ForEachAsync(ch.Reader.ReadAllAsync(ct),
    new ParallelOptions { MaxDegreeOfParallelism = 4, CancellationToken = ct },
    async (item, token) => await HandleAsync(item, token));
```

Пул ограничен `MaxDegreeOfParallelism`; вместе с ограниченным каналом это даёт обратное давление: издатель ждёт, пока потребители разгребут очередь.

## Запуск воркера

Проверено на .NET 11: `StartAsync` хоста завершился за ~6 мс, хотя в `ExecuteAsync` синхронная часть занимала 700 мс (`Thread.Sleep`) — она выполнилась уже после возврата `StartAsync`.

> ⚠️ Проверить: в документации по более ранним версиям сказано, что синхронная часть `ExecuteAsync` блокирует запуск остальных сервисов. Поведение .NET 11 проверено, для других версий — нет; всё равно начинайте воркер с первого `await`.

## Best practices

| Правило | Почему |
|---|---|
| `try/catch` внутри цикла на единицу работы | один сбой не должен убивать воркер |
| Передавать `ct` везде | быстрая остановка |
| Ограниченный канал | обратное давление вместо роста памяти |
| Scope на единицу работы | корректное время жизни `DbContext` |
| Логировать начало, конец, сбои | наблюдаемость ([22](../../modules/22-observability/overview.ru.md)) |
| Идемпотентные задания | повторы после перезапуска не вредят |
| Не `Task.Run` из запроса | потеря работы при остановке |

## Связанные материалы

- Назад: [минимум](basics.ru.md). Дальше: [масштаб и грани](expert.ru.md).
- [13 · DI, конфигурация, логирование](../../modules/13-di-config-logging/overview.ru.md) — scopes и options.
- [18 · Конкурентность](../../modules/18-concurrency/overview.ru.md) — `Channel<T>`, `Parallel.ForEachAsync`.
- [Rate limiting](../rate-limiting/overview.ru.md) — защита входа, пока очередь разгребается.

## Источники

- [Hosted services](https://learn.microsoft.com/aspnet/core/fundamentals/host/hosted-services)
- [`HostOptions`](https://learn.microsoft.com/dotnet/api/microsoft.extensions.hosting.hostoptions)
- [`BackgroundServiceExceptionBehavior`](https://learn.microsoft.com/dotnet/api/microsoft.extensions.hosting.backgroundserviceexceptionbehavior)
