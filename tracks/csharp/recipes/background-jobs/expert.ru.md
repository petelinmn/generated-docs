---
title: "Фоновые задачи: масштаб и грани"
lang: ru
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

# Фоновые задачи: масштаб и грани

🌐 **Русский** · [English](expert.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Минимум](basics.ru.md) · [2. Production](advanced.ru.md) · **3. Масштаб и грани** · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [минимум](basics.ru.md) и [production](advanced.ru.md). **Время:** ~30 мин.

## Масштаб: когда `Channel<T>` недостаточно

Очередь в памяти процесса **не переживает перезапуск**: при падении или деплое все неотработанные задания пропадают. И каждый экземпляр приложения имеет свою очередь.

| Потребность | Решение |
|---|---|
| задания нельзя терять | durable-очередь: таблица в БД (outbox), брокер (RabbitMQ, Azure Service Bus, Kafka) |
| расписание (cron), повторы, дашборд | библиотеки планировщиков (Hangfire, Quartz.NET) |
| несколько экземпляров, задание — один раз | блокировка/lease в БД или общий брокер с competing consumers |
| долгие и ресурсоёмкие задания | отдельный процесс-воркер (Worker Service, контейнер, Kubernetes Job/CronJob) |
| только «выстрелил и забыл», потеря допустима | `Channel<T>` + `BackgroundService` |

> ⚠️ Проверить: возможности Hangfire/Quartz.NET (хранилища, API) в этом материале не запускались — выбирайте по их документации.

## Грани и подвохи

**1. Периодический таймер и несколько экземпляров.** Периодическая задача запустится в каждом экземпляре; без блокировки уборка/рассылка выполнится N раз.

**2. `PeriodicTimer` не копит тики.** Если обработка длиннее периода, пропущенные тики не накапливаются — следующий `WaitForNextTickAsync` завершится при ближайшем тике; шторма тиков нет.

**3. Исключение убивает хост по умолчанию.** Проверено: `StopHost` останавливает приложение; `Ignore` оставляет воркер мёртвым без шума ([production](advanced.ru.md#исключения-в-воркере)).

**4. `ShutdownTimeout` не убивает воркер.** Проверено: `StopAsync` вернулся за ~300 мс без исключения, а воркер, игнорирующий токен, остался выполняться.

**5. Потеря элемента при остановке.** После `Writer.Complete()` читатель дочитывает остаток (проверено: `2,3`), но при аварийном падении очередь в памяти теряется.

**6. Неограниченная очередь.** Медленный потребитель — рост памяти до `OutOfMemory`; ограничивайте ёмкость и выбирайте `FullMode` осознанно (`Wait` — давление на издателя, `DropOldest` — потеря старых, проверено: остались `3,4`).

**7. `async void` и «забытые» задачи.** Задача, запущенная без `await`, при исключении теряется; хостируемый сервис даёт владельца жизненному циклу.

**8. Scoped в singleton.** Внедрение `DbContext` в воркер напрямую — ошибка времени жизни; используйте `IServiceScopeFactory`.

**9. Долгая синхронная часть `ExecuteAsync`.** Проверено на .NET 11: `StartAsync` не блокируется; на других версиях поведение другое ([production](advanced.ru.md#запуск-воркера)).

## Компромиссы и альтернативы

| Подход | Плюсы | Минусы |
|---|---|---|
| `BackgroundService` + `Channel<T>` | просто, без зависимостей | не durable, не распределён |
| Outbox + воркер | атомарно с транзакцией БД, не теряется | нужна таблица и опрос |
| Брокер сообщений | масштаб, повторы, разделение сервисов | инфраструктура |
| Hangfire/Quartz | расписание, ретраи, дашборд | зависимость и хранилище |
| Kubernetes CronJob | изоляция, масштабирование отдельно | операционная сложность |

## Вопросы с собеседований

<details><summary>1. Как выполнить работу в фоне в ASP.NET Core?</summary>

Хостируемый сервис (`IHostedService`/`BackgroundService`), регистрируется через `AddHostedService`; очередь — `Channel<T>`; периодичность — `PeriodicTimer`.

</details>

<details><summary>2. Чем `BackgroundService` отличается от `IHostedService`?</summary>

`BackgroundService` — готовая база: реализует `StartAsync/StopAsync`, вам остаётся `ExecuteAsync(CancellationToken)`; `IHostedService` — низкоуровневый интерфейс с `StartAsync`/`StopAsync`.

</details>

<details><summary>3. Как получить `DbContext` в воркере?</summary>

Через `IServiceScopeFactory.CreateScope()` на каждую единицу работы (проверено: scoped-сервис получен из scope).

</details>

<details><summary>4. Что произойдёт, если `ExecuteAsync` бросит исключение?</summary>

По умолчанию хост остановится (`StopHost`, проверено: `ApplicationStopping`); при `Ignore` воркер молча умрёт.

</details>

<details><summary>5. Как корректно остановить воркер?</summary>

Отмена `ct`, выход по `OperationCanceledException`, дописывание текущей работы, для очереди — `Complete()`; ожидание ограничено `HostOptions.ShutdownTimeout`.

</details>

<details><summary>6. Чем `PeriodicTimer` лучше `Timer`/`Task.Delay` в цикле?</summary>

Асинхронное ожидание, поддержка отмены, не накапливает тики, нет наложения вызовов (следующий тик ждёт завершения обработки).

</details>

<details><summary>7. Что делает ограниченный `Channel<T>` при заполнении?</summary>

Зависит от `FullMode`: `Wait` — `WriteAsync` ждёт (`TryWrite` возвращает `false`, проверено), `DropOldest/DropNewest/DropWrite` — отбрасывает.

</details>

<details><summary>8. Почему очереди в памяти недостаточно для критичных заданий?</summary>

Она теряется при перезапуске/падении и принадлежит одному экземпляру; нужна durable-очередь (outbox, брокер).

</details>

<details><summary>9. Подвох: приложение в 3 экземплярах, `BackgroundService` рассылает письма раз в час. Сколько писем получит клиент?</summary>

Три: таймер работает в каждом экземпляре. Нужна блокировка/lease, единственный воркер-процесс или планировщик с координацией.

</details>

<details><summary>10. Подвох: `ShutdownTimeout` истёк — воркер убит?</summary>

Нет. Хост перестаёт ждать (проверено: `StopAsync` вернулся без исключения за ~300 мс), но игнорирующий токен воркер продолжает выполняться до завершения процесса.

</details>

## Связанные материалы

- Назад: [production](advanced.ru.md). Проверьте себя: [квиз](quiz.ru.md) и [упражнения](exercises.ru.md).
- [21 · Архитектура приложения](../../modules/21-architecture/overview.ru.md) — outbox и границы сервисов.
- [18 · Конкурентность](../../modules/18-concurrency/overview.ru.md) — `Channel<T>`.
- [Kubernetes](../../../kubernetes/overview.ru.md) — Job и CronJob для отдельных воркеров.

## Источники

- [Hosted services](https://learn.microsoft.com/aspnet/core/fundamentals/host/hosted-services)
- [Channels](https://learn.microsoft.com/dotnet/core/extensions/channels)
- [Worker Services](https://learn.microsoft.com/dotnet/core/extensions/workers)
