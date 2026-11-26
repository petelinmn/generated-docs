---
title: "MassTransit"
lang: ru
track: csharp
kind: library
doc: overview
slug: masstransit
part: 3
levels: [1, 2, 3, 4, 5, 6]
tags: [masstransit, messaging, message-bus, consumer, saga, rabbitmq, test-harness, retry]
versions: ".NET 11, C# 15; MassTransit 8.5.12"
status: draft
updated: 2026-11-25
---

# MassTransit

🌐 **Русский** · [English](overview.en.md)

## Коротко

MassTransit — библиотека обмена сообщениями для .NET. Она прячет транспорт (RabbitMQ, Azure Service Bus, Amazon SQS, Kafka, а для тестов — «в памяти») за единой моделью: **сообщение**, **шина**, **конечная точка**, **потребитель**. Из коробки — повторы, очередь ошибок, запрос-ответ, саги на конечных автоматах и тестовая оснастка. Всё проверено запуском на ветке **8.x (8.5.12)**: версия 9 — коммерческая, для её использования в рабочей системе нужна лицензия.

## Ключевые идеи

- **Контракт — тип в пространстве имён** (`record Shop.OrderSubmitted`); без пространства имён публикация падает.
- **`Publish` — событие всем подписчикам, `Send` — команда одной конечной точке.**
- **Потребитель — `IConsumer<T>`;** имена очередей по умолчанию `OrderSubmitted`, `kebab-case` включается явно.
- **Контейнер освобождают через `await using`.**
- **Повторы** (`UseMessageRetry`) идут в процессе; после исчерпания — `Fault<T>` и очередь `<имя>_error` с заголовками `MT-Fault-*`.
- **RabbitMQ:** обменник типа сообщения → обменник конечной точки → очередь.
- **Test harness:** тесты без `Task.Delay`; саги на конечных автоматах (`MassTransitStateMachine`).
- **В 8.x есть анонимная телеметрия использования** (отключается).

## Минимальный пример

```csharp
await using var sp = new ServiceCollection()
    .AddMassTransit(x => { x.AddConsumer<OrderSubmittedConsumer>(); x.UsingInMemory((ctx, c) => c.ConfigureEndpoints(ctx)); })
    .BuildServiceProvider();
var bus = sp.GetRequiredService<IBusControl>();
await bus.StartAsync();
await bus.Publish(new OrderSubmitted(Guid.NewGuid(), "alice", 19.99m));
```

Всё запускалось на .NET 11 и MassTransit 8.5.12, RabbitMQ — 4.4.1 в Docker (см. [основы](basics.ru.md)).

## Когда применять и когда нет

- ✅ Несколько сервисов обмениваются событиями и командами через брокер.
- ✅ Нужны повторы, очередь ошибок, саги и удобные тесты.
- ✅ Хотите менять транспорт без переписывания кода обработчиков.
- ❌ Достаточно очереди в процессе (`Channel<T>`) — см. [фоновые задачи](../../recipes/background-jobs/overview.ru.md).
- ❌ Нужен полный контроль над протоколом брокера.
- ❌ Лицензия v9 вам не подходит (тогда v8 или другая библиотека).

## Запуск кода

```bash
cd tracks/csharp/libraries/masstransit/code
dotnet test
```

11 тестов; 10 не требуют ничего, кроме .NET 11 SDK, а интеграционный тест с RabbitMQ пропускается, если брокера нет. Чтобы он выполнился, поднимите одноразовый брокер: `docker run -d --name rabbit -p 5672:5672 rabbitmq:4-management`.

## Уровни материала

| Уровень | Что внутри |
|---|---|
| [1. Основы](basics.ru.md) | сообщение, потребитель, шина в памяти, Publish и Send, имена очередей |
| [2. Продвинутый](advanced.ru.md) | повторы, `Fault<T>` и `_error`, запрос-ответ, параллелизм, топология RabbitMQ, test harness |
| [3. Эксперт](expert.ru.md) | справочник, саги, внутреннее устройство, версии и лицензия, 12 вопросов с собеседований |
| [4. Самопроверка](quiz.ru.md) | 10 вопросов и [4 упражнения](exercises.ru.md) с решениями |
| 5. [Запускаемые примеры](code/Examples/) | 6 примеров и [решения упражнений](code/Solutions/Exercises.cs) |
| 6. [Тесты](code/Tests/) | xUnit v3 на тестовой оснастке и один интеграционный с RabbitMQ |

## Связанные материалы

- Пререквизиты: [13 · DI, конфигурация, логирование](../../modules/13-di-config-logging/overview.ru.md), [10 · async/await](../../modules/10-async-await/overview.ru.md)
- Рядом: [Фоновые задачи](../../recipes/background-jobs/overview.ru.md), [Polly](../polly/overview.ru.md), [Testcontainers](../testcontainers/overview.ru.md)
- В других треках: [RabbitMQ](../../../rabbitmq/overview.ru.md) — брокер под транспортом; [Kafka](../../../kafka/overview.ru.md) — журнал событий
- [System design: сообщения и события](../../../system-design/modules/08-messaging-and-events/overview.ru.md)
- [System design: распределённые транзакции](../../../system-design/modules/10-distributed-transactions/overview.ru.md)
