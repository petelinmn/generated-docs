---
title: "MassTransit: экспертный уровень"
lang: ru
track: csharp
kind: library
doc: expert
slug: masstransit
part: 3
tags: [masstransit, saga, state-machine, middleware, pipeline, consumer, rabbitmq, licensing]
versions: ".NET 11, C# 15; MassTransit 8.5.12"
status: draft
updated: 2026-11-24
---

# MassTransit: экспертный уровень

🌐 **Русский** · [English](expert.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Основы](basics.ru.md) · [2. Продвинутый](advanced.ru.md) · **3. Эксперт** · [4. Квиз](quiz.ru.md)

Опирается на [основы](basics.ru.md) и [продвинутый уровень](advanced.ru.md). «Проверено» означает наблюдалось запуском на .NET 11 с MassTransit 8.5.12 (шина в памяти и RabbitMQ 4.4.1).

## Справочник

### Основные понятия и API

| Элемент | Назначение | Проверено |
|---|---|---|
| `IConsumer<T>` | обработчик сообщения | да |
| `ConsumeContext<T>` | сообщение, заголовки, `RespondAsync`, `GetRetryAttempt()` | да |
| `IBusControl` | запуск/остановка шины, `Publish`, `GetSendEndpoint` | да |
| `IPublishEndpoint`, `ISendEndpointProvider` | публикация и отправка из внедряемых сервисов | не проверялись |
| `IRequestClient<TRequest>` | запрос-ответ | да |
| `AddMassTransit`, `AddConsumer<T>`, `UsingInMemory`, `UsingRabbitMq`, `ConfigureEndpoints` | регистрация и транспорты | да |
| `SetKebabCaseEndpointNameFormatter()` | имена очередей в `kebab-case` | да |
| `ReceiveEndpoint(имя, …)`, `ConfigureConsumer<T>(ctx)` | явная конечная точка | да |
| `UseMessageRetry(…)`, `Immediate`, `Interval` | повторы | да |
| `ConcurrentMessageLimit` | параллелизм конечной точки | да (1 и 4) |
| `Fault<T>` | событие отказа после исчерпания повторов | да |
| `ConsumerDefinition`, `UseMessageRedelivery`, `UseInMemoryOutbox`, Entity Framework outbox | определения, отложенный повтор, outbox | не проверялись |

### Тестовая оснастка

| Элемент | Проверено |
|---|---|
| `AddMassTransitTestHarness`, `ITestHarness`, `harness.Start()` | да |
| `harness.Published.Any<T>()`, `Consumed.Any<T>()`, `GetConsumerHarness<TConsumer>()` | да |
| `GetSagaStateMachineHarness<TMachine, TInstance>()`, `Exists(id, m => m.State)`, `NotExists(id)` | да |

### Транспорты

| Транспорт | Проверено |
|---|---|
| в памяти (`UsingInMemory`) | да |
| RabbitMQ (`MassTransit.RabbitMQ`) | да |
| Azure Service Bus, Amazon SQS, ActiveMQ, Kafka (rider), Mediator | не проверялись |

## Саги и конечные автоматы

**Сага** хранит состояние долгого процесса и реагирует на сообщения. В MassTransit сагу описывают **конечным автоматом** (`MassTransitStateMachine<TInstance>`): состояния, события и переходы. Экземпляр (`SagaStateMachineInstance`) содержит `CorrelationId` и текущее состояние.

```csharp
public class OrderState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; } = "";
    public string? Customer { get; set; }
}

public class OrderStateMachine : MassTransitStateMachine<OrderState>
{
    public State Submitted { get; private set; } = null!;
    public State Paid { get; private set; } = null!;
    public Event<OrderSubmitted> SubmittedEvent { get; private set; } = null!;
    public Event<OrderPaid> PaidEvent { get; private set; } = null!;
    public Event<OrderShipped> ShippedEvent { get; private set; } = null!;

    public OrderStateMachine()
    {
        InstanceState(x => x.CurrentState);
        Event(() => SubmittedEvent, e => e.CorrelateById(c => c.Message.OrderId));
        Event(() => PaidEvent, e => e.CorrelateById(c => c.Message.OrderId));
        Event(() => ShippedEvent, e => e.CorrelateById(c => c.Message.OrderId));

        Initially(When(SubmittedEvent).Then(c => c.Saga.Customer = c.Message.Customer).TransitionTo(Submitted));
        During(Submitted, When(PaidEvent).TransitionTo(Paid));
        During(Paid, When(ShippedEvent).Finalize());
        SetCompletedWhenFinalized();
    }
}

services.AddMassTransitTestHarness(x => x.AddSagaStateMachine<OrderStateMachine, OrderState>().InMemoryRepository());
```

Опыт в тестовой оснастке: `OrderSubmitted` → `OrderShipped` (до оплаты) → `OrderPaid` → `OrderShipped`:

```text
exists in Submitted: True
state after OrderShipped in Submitted: still Submitted
faults published: True
exists in Paid: True
NotExists after finalize returned: (null)
customer: alice
```

- Событие `OrderSubmitted` создало экземпляр в состоянии `Submitted`, `Customer` сохранён.
- **Событие вне порядка** (`OrderShipped` в состоянии `Submitted`) не продвинуло сагу и привело к ошибке `NotAcceptedStateMachineException` (внутри `UnhandledEventException`: «The ShippedEvent event is not handled during the Submitted state»); был опубликован `Fault<OrderShipped>`, состояние осталось `Submitted`.
- После `OrderPaid` — `Paid`; после второго `OrderShipped` сага финализируется и **удаляется** из репозитория (`NotExists` вернул `null`, как и для id, которого не было).

События, допустимые в нескольких состояниях, по документации описывают явно (`During(Submitted, Paid, …)`) или игнорируют (`Ignore(event)`); без этого событие вне порядка заканчивается ошибкой, как в опыте.

> ⚠️ Проверить: постоянные репозитории саг (Entity Framework, MongoDB, Redis), оптимистичная блокировка, `Schedule`/`Request` внутри саги и `Ignore` здесь не проверялись.

## Внутреннее устройство

- **Конвейер (pipeline) и фильтры.** Приём сообщения проходит цепочку фильтров (middleware): повтор, транзакция, потребитель и т. д. `UseMessageRetry` — один из фильтров.
- **Топология отражает типы сообщений.** Для каждого типа публикации создаётся обменник `Пространство:Тип`, к которому привязаны обменники конечных точек (проверено на RabbitMQ).
- **Конверт (envelope).** Сообщение передаётся в JSON-конверте с идентификаторами (`messageId`, `correlationId`, `conversationId`) и адресами (проверено: тело сообщения в очереди `fails_error` — JSON с `messageId`).
- **Ошибки.** Исключение потребителя → повторы → `Fault<T>` + перемещение в `<очередь>_error` с заголовками `MT-Fault-*` (проверено).
- **Жизненный цикл шины.** `StartAsync` запускает конечные точки, `StopAsync` останавливает; в хост-приложении этим занимается размещённая служба.

> ⚠️ Проверить: внутренности конвейера (контексты, фильтры), сериализаторы (System.Text.Json, Newtonsoft, MessagePack), заголовки `MT-*` кроме проверенных, механизм предвыборки (prefetch) и `Scheduler` здесь не проверялись.

## Версии и лицензия

- Здесь использована ветка **8.x** (8.5.12) — по сообщению разработчиков последняя открытая ветка.
- **Версия 9** (на момент написания 9.3.0 на NuGet) по объявлению разработчиков — коммерческая: для рабочего использования требуется лицензия.
- В 8.x есть анонимная **телеметрия использования** (отключается `DisableUsageTelemetry()` или `MASSTRANSIT_USAGE_TELEMETRY=false`).

> ⚠️ Проверить: точные условия лицензий v8 и v9, различия API между 8.x и 9.x и состав телеметрии здесь не проверялись. Сверяйтесь с [объявлением](https://masstransit.io/introduction/v9-announcement) и официальными страницами.

## Подвохи

### 1. Сообщения в глобальном пространстве имён

Публикация падает: «Messages types must have a valid namespace» (проверено).

### 2. `using` вместо `await using`

`InvalidOperationException` при освобождении контейнера (проверено).

### 3. Имена очередей по умолчанию не `kebab-case`

Форматтер по умолчанию оставляет `OrderSubmitted`; `kebab-case` включается явно (проверено).

### 4. Повторы идут в процессе

Сообщение не возвращается в очередь между попытками (проверено: заголовок повтора 0, 1, 2), поэтому долгие повторы удерживают поток обработки.

### 5. Событие саги вне порядка

`NotAcceptedStateMachineException` и `Fault` (проверено).

### 6. Тест без ожидания

Проверка «потреблено» сразу после `Publish` без `Any<T>()` — гонка: используйте ожидания оснастки.

### 7. Сообщения не переименовывают

Пространство имён и имя типа — часть адреса (проверено в именах обменников); переименование ломает совместимость.

### 8. Версия 9 — коммерческая

Не обновляйтесь «по умолчанию».

## Компромиссы и альтернативы

| Выбор | Плюсы | Минусы |
|---|---|---|
| MassTransit | единая модель над транспортами, повторы, саги, оснастка тестов | зависимость и лицензия (v9) |
| клиент брокера напрямую (RabbitMQ.Client) | полный контроль | всё вручную |
| `Channel<T>` и фоновые задачи | без брокера | только в процессе ([рецепт](../../recipes/background-jobs/overview.ru.md)) |
| другая библиотека (например, NServiceBus, Wolverine) | свои возможности | здесь не проверялись |

## Вопросы с собеседований

<details><summary>1. Чем `Publish` отличается от `Send`?</summary>

`Publish` — событие всем подписчикам типа; `Send` — команда в конкретную конечную точку.

→ [Основы](basics.ru.md#publish-и-send)

</details>

<details><summary>2. Подвох: сообщение в глобальном пространстве имён?</summary>

Ошибка: тип должен иметь пространство имён.

→ [Подвох 1](#1-сообщения-в-глобальном-пространстве-имён)

</details>

<details><summary>3. Что происходит, когда повторы исчерпаны?</summary>

Публикуется `Fault<T>`, сообщение перемещается в очередь `_error` с заголовками `MT-Fault-*`.

→ [Продвинутый](advanced.ru.md#ошибки-faultt-и-очередь-_error)

</details>

<details><summary>4. Как выглядит топология RabbitMQ?</summary>

Обменник типа сообщения → обменник конечной точки → очередь (оба fanout).

→ [Продвинутый](advanced.ru.md#топология-rabbitmq)

</details>

<details><summary>5. Как тестировать потребителя без брокера?</summary>

`AddMassTransitTestHarness`, шина в памяти, ожидания `Published/Consumed.Any<T>()`.

→ [Продвинутый](advanced.ru.md#тестовая-оснастка-test-harness)

</details>

<details><summary>6. Подвох: контейнер освобождён через `using`. Что будет?</summary>

`InvalidOperationException`: нужен `await using`.

→ [Подвох 2](#2-using-вместо-await-using)

</details>

<details><summary>7. Что такое сага в MassTransit?</summary>

Состояние долгого процесса, описанное конечным автоматом: состояния, события, переходы, корреляция по `CorrelationId`.

→ [Саги](#саги-и-конечные-автоматы)

</details>

<details><summary>8. Что будет, если событие пришло в неподходящем состоянии саги?</summary>

`NotAcceptedStateMachineException`, `Fault`; состояние не меняется — если событие не игнорируется явно.

→ [Подвох 5](#5-событие-саги-вне-порядка)

</details>

<details><summary>9. Как гарантировать обработку «минимум один раз» безопасно?</summary>

Идемпотентные потребители, повторы и очередь ошибок; для публикации из БД — outbox (здесь не проверялся).

→ [Продвинутый](advanced.ru.md#лучшие-практики)

</details>

<details><summary>10. Что нужно знать про версии MassTransit?</summary>

8.x — последняя открытая ветка (здесь 8.5.12); 9.x — коммерческая; в 8.x есть анонимная телеметрия использования.

→ [Версии и лицензия](#версии-и-лицензия)

</details>

<details><summary>11. Подвох: что значит «повтор идёт в процессе»?</summary>

Сообщение остаётся у потребителя между попытками; в брокере оно не возвращается в очередь.

→ [Подвох 4](#4-повторы-идут-в-процессе)

</details>

<details><summary>12. Когда MassTransit не нужен?</summary>

Когда достаточно очереди в процессе (`Channel<T>`) или прямого клиента брокера.

→ [Компромиссы](#компромиссы-и-альтернативы)

</details>

## Шпаргалка

```csharp
services.AddMassTransit(x => {
    x.SetKebabCaseEndpointNameFormatter();
    x.AddConsumer<MyConsumer>();
    x.AddRequestClient<MyRequest>();
    x.UsingRabbitMq((ctx, c) => { c.Host(...); c.ReceiveEndpoint("q", e => { e.UseMessageRetry(r => r.Interval(3, 100ms)); e.ConfigureConsumer<MyConsumer>(ctx); }); });
});
await using var sp = services.BuildServiceProvider();   // await using!
// tests: AddMassTransitTestHarness + harness.Consumed.Any<T>()
```

- События — `Publish`, команды — `Send`; тип сообщения — в пространстве имён.
- Повторы + очередь `_error`; идемпотентные потребители.
- v8 открытая, v9 коммерческая.

## Связанные материалы

- Предыдущий уровень: [продвинутый](advanced.ru.md)
- [Фоновые задачи](../../recipes/background-jobs/overview.ru.md), [Polly](../polly/overview.ru.md), [Testcontainers](../testcontainers/overview.ru.md)
- [RabbitMQ](../../../rabbitmq/overview.ru.md), [Kafka](../../../kafka/overview.ru.md)

## Источники

- [MassTransit: documentation](https://masstransit.io/documentation/concepts)
- [MassTransit: state machine sagas](https://masstransit.io/documentation/patterns/saga/state-machine)
- [MassTransit: testing](https://masstransit.io/documentation/concepts/testing)
- [Announcing MassTransit v9](https://masstransit.io/introduction/v9-announcement)
