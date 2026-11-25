---
title: "MassTransit: основы"
lang: ru
track: csharp
kind: library
doc: basics
slug: masstransit
part: 3
tags: [masstransit, messaging, message-bus, consumer, publish-subscribe, in-memory-transport, dependency-injection]
versions: ".NET 11, C# 15; MassTransit 8.5.12"
status: draft
updated: 2026-11-25
---

# MassTransit: основы

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [обзор](overview.ru.md) · **1. Основы** · [2. Продвинутый](advanced.ru.md) · [3. Эксперт](expert.ru.md) · [4. Квиз](quiz.ru.md) · [5–6. Код и тесты](code/)

**Пререквизиты:** [13 · DI, конфигурация, логирование](../../modules/13-di-config-logging/overview.ru.md), [10 · async/await](../../modules/10-async-await/overview.ru.md); полезно знакомство с брокерами сообщений ([RabbitMQ](../../../rabbitmq/overview.ru.md)). **Время:** ~45 мин.

## Как проверялся код

Весь код запускался на .NET 11 SDK 11.0.100 (`net11.0`) с MassTransit 8.5.12; выводы скопированы из запусков. Транспорт «в памяти» (in-memory) не требует брокера; примеры с RabbitMQ запускались на настоящем RabbitMQ 4.4.1 в Docker (`rabbitmq:4-management`). Все сообщения лежат в пространстве имён `Shop`:

```csharp
namespace Shop;

public record OrderSubmitted(Guid OrderId, string Customer, decimal Amount);
```

> **Версия и лицензия.** Здесь описана ветка **8.x** (8.5.12) — последняя открытая. По объявлению разработчиков версия **9** стала коммерческой: для рабочего использования нужна лицензия. Условия проверяйте на официальных страницах MassTransit; в этом материале версия 9 не запускалась.

## Что такое MassTransit

**MassTransit** — библиотека для обмена сообщениями в .NET: она скрывает транспорт (RabbitMQ, Azure Service Bus, Amazon SQS, Kafka и др., а также «в памяти» для тестов) за единой моделью: **сообщение** (message), **шина** (bus), **конечная точка** (endpoint, принимающая очередь), **потребитель** (consumer). Поверх неё есть повторы, обработка ошибок, запрос-ответ и саги.

```bash
dotnet add package MassTransit --version 8.5.12
dotnet add package MassTransit.RabbitMQ --version 8.5.12     # only for the RabbitMQ transport
```

## Контракт сообщения

Сообщение — обычный тип (удобно `record`). Тип **должен находиться в пространстве имён**: тип в глобальном пространстве имён при публикации даёт ошибку.

```text
System.ArgumentException: Messages types must have a valid namespace: OrderSubmitted (Parameter 'T')
```

Имя типа вместе с пространством имён становится частью адреса («тема» сообщения): `Shop:OrderSubmitted`.

## Потребитель

Потребитель реализует `IConsumer<TMessage>`:

```csharp
public class OrderSubmittedConsumer : IConsumer<OrderSubmitted>
{
    public Task Consume(ConsumeContext<OrderSubmitted> context)
    {
        Console.WriteLine($"{context.Message.Customer} {context.Message.Amount} | MessageId set: {context.MessageId.HasValue}");
        return Task.CompletedTask;
    }
}
```

`ConsumeContext<T>` содержит само сообщение (`Message`), служебные данные (`MessageId`, `SourceAddress`, заголовки) и методы для ответа и публикации.

## Шина «в памяти»

```csharp
await using var sp = new ServiceCollection()
    .AddMassTransit(x =>
    {
        x.AddConsumer<OrderSubmittedConsumer>();
        x.UsingInMemory((ctx, cfg) => cfg.ConfigureEndpoints(ctx));
    })
    .BuildServiceProvider();

var bus = sp.GetRequiredService<IBusControl>();
await bus.StartAsync();
await bus.Publish(new OrderSubmitted(Guid.NewGuid(), "alice", 19.99m));
await Task.Delay(500);
await bus.StopAsync();
```

```text
bus address: loopback://localhost/<host>_Mt_bus_<id>
[OrderSubmittedConsumer] alice 19,99 | MessageId set: True | Source: /<host>_Mt_bus_<id>
```

- `AddMassTransit` регистрирует шину и потребителей в контейнере внедрения зависимостей.
- `ConfigureEndpoints` создаёт принимающую конечную точку для каждого потребителя.
- Шина запускается `StartAsync` (в приложении с `Host` это делает размещённая служба) и останавливается `StopAsync`.
- Сообщение дошло, `MessageId` присвоен автоматически.

> **Подвох:** контейнер нужно освобождать через **`await using`**. С обычным `using var sp` получаем `InvalidOperationException: 'MassTransit.UsageTracking.UsageTracker' type only implements IAsyncDisposable. Use DisposeAsync to dispose the container.` — некоторые сервисы MassTransit поддерживают только асинхронное освобождение.

Код: [Messages.cs](code/Examples/Messages.cs), [Ex01_PublishSend.cs](code/Examples/Ex01_PublishSend.cs).

## Publish и Send

| Метод | Адресат | Когда |
|---|---|---|
| `Publish` | все подписчики типа сообщения | **событие**: «произошло» |
| `Send` | одна конкретная конечная точка | **команда**: «сделай» |

Два потребителя (`OrderSubmittedConsumer`, `AuditConsumer`) на один тип сообщения:

```csharp
await bus.Publish(order);                                         // to every subscriber
var ep = await bus.GetSendEndpoint(new Uri("queue:audit"));
await ep.Send(order);                                             // to one endpoint
```

```text
-- Publish (all subscribers):
[AuditConsumer] saw 4f54732d
[OrderSubmittedConsumer] alice 19,99 | MessageId set: True | Source: /<host>_Mt_bus_<id>
-- Send to queue:audit:
[AuditConsumer] saw 4f54732d
```

`Publish` дошёл до обоих потребителей, `Send` на `queue:audit` — только до одного. Порядок вывода между потребителями не гарантирован.

Код потребителей: [Ex01_PublishSend.cs](code/Examples/Ex01_PublishSend.cs); проверка того, что `Publish` получают оба, а `Send` один: [ExamplesTests.cs](code/Tests/ExamplesTests.cs).

## Имена конечных точек

Имя очереди для потребителя строится из имени класса без суффикса `Consumer`. Форматтер по умолчанию оставляет `OrderSubmitted` (в журнале тестовой оснастки: `Endpoint Ready: loopback://localhost/OrderSubmitted`). Для привычного `kebab-case` включите его явно:

```csharp
x.SetKebabCaseEndpointNameFormatter();      // OrderSubmittedConsumer -> order-submitted
```

В RabbitMQ с этим форматтером получились очереди `order-submitted` и `audit` ([продвинутая теория](advanced.ru.md#топология-rabbitmq)).

## Типичные ошибки новичков

- ❌ Сообщение в глобальном пространстве имён → ✅ любое пространство имён.
- ❌ `using var sp` вместо `await using` → ✅ `await using`.
- ❌ `Publish`, когда нужна команда одному получателю → ✅ `Send`.
- ❌ Ждать порядок между разными потребителями → ✅ порядок не гарантирован.
- ❌ Брать v9 для рабочей системы, не проверив лицензию → ✅ v8 или лицензия v9.

## Связанные материалы

- Дальше: [продвинутый уровень](advanced.ru.md) — повторы, ошибки, запрос-ответ, RabbitMQ
- [Фоновые задачи](../../recipes/background-jobs/overview.ru.md) — очередь в процессе против брокера
- [RabbitMQ](../../../rabbitmq/overview.ru.md) — брокер под MassTransit; [Kafka](../../../kafka/overview.ru.md) — журнал событий

## Источники

- [MassTransit: documentation](https://masstransit.io/documentation/concepts)
- [MassTransit: getting started](https://masstransit.io/quick-starts/in-memory)
- [Announcing MassTransit v9](https://masstransit.io/introduction/v9-announcement)
