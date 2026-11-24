---
title: "MassTransit: продвинутый уровень"
lang: ru
track: csharp
kind: library
doc: advanced
slug: masstransit
part: 3
tags: [masstransit, retry, fault, error-queue, request-response, rabbitmq, test-harness, concurrency]
versions: ".NET 11, C# 15; MassTransit 8.5.12"
status: draft
updated: 2026-11-24
---

# MassTransit: продвинутый уровень

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Основы](basics.ru.md) · **2. Продвинутый** · [3. Эксперт](expert.ru.md) · [4. Квиз](quiz.ru.md)

Опирается на [основы](basics.ru.md). Код запускался на .NET 11 с MassTransit 8.5.12; RabbitMQ — 4.4.1 в Docker. Помощник `Build(...)` создаёт `ServiceCollection` с `AddMassTransit` и шиной «в памяти».

## Повторы

Временный сбой потребителя лечится повтором: `UseMessageRetry` на конечной точке.

```csharp
cfg.ReceiveEndpoint("flaky", e =>
{
    e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromMilliseconds(100)));
    e.ConfigureConsumer<FlakyConsumer>(ctx);
});
```

Потребитель падает на первых двух попытках:

```text
[FlakyConsumer] attempt 1 (retry attempt header: 0)
[FlakyConsumer] attempt 2 (retry attempt header: 1)
[FlakyConsumer] attempt 3 (retry attempt header: 2)
[FlakyConsumer] success
```

Один и тот же `Consume` вызван трижды, номер повтора доступен как `context.GetRetryAttempt()`. Повторы выполняются **в процессе**, в том же сообщении, без возврата в очередь. Потому обработчик должен быть идемпотентным: частично выполненная работа повторится.

Политики: `Immediate(n)`, `Interval(n, пауза)`, `Exponential(...)` и другие; `Handle<T>`/`Ignore<T>` выбирают исключения для повтора. Сравните с [Polly](../polly/overview.ru.md): это такая же идея на уровне конечной точки.

## Ошибки, `Fault<T>` и очередь `_error`

Если повторы исчерпаны, сообщение **не теряется**: MassTransit публикует событие `Fault<T>` и перекладывает сообщение в очередь ошибок.

```csharp
e.UseMessageRetry(r => r.Immediate(2));      // always failing consumer
```

```text
[FaultObserver] Fault<OrderSubmitted>: System.InvalidOperationException : boom
attempts: 3
```

Три вызова (первый и два повтора), затем `Fault<OrderSubmitted>` получил подписчик `IConsumer<Fault<OrderSubmitted>>`. В RabbitMQ сообщение оказалось в очереди **`fails_error`** (для конечной точки `fails`) с заголовками диагностики:

```text
MT-Reason: fault
MT-Fault-ConsumerType: Shop.AlwaysFailsConsumer
MT-Fault-ExceptionType: System.InvalidOperationException
MT-Fault-Message: boom
MT-Fault-MessageType: Shop.OrderSubmitted
MT-Fault-RetryCount: 2
MT-Fault-InputAddress: rabbitmq://localhost/fails
MT-Fault-StackTrace: ...
MT-Host-MassTransitVersion: 8.5.12.0   (и другие MT-Host-*)
```

Очередь ошибок — это аналог dead-letter-очереди ([RabbitMQ: DLX](../../../rabbitmq/modules/06-dead-lettering-and-retries/overview.ru.md)): по ней видно, что сломалось, и сообщение можно вернуть в обработку вручную. Вместе с обменником `fails_error` в брокере появился и обменник события отказа `MassTransit:Fault--Shop:OrderSubmitted--`.

> ⚠️ Проверить: поведение `UseMessageRedelivery` (отложенный повтор), `_skipped`-очереди для сообщений без подходящего потребителя и `UseInMemoryOutbox` здесь не проверялись.

## Запрос и ответ

`IRequestClient<TRequest>` отправляет запрос и ждёт типизированный ответ:

```csharp
public class CheckOrderConsumer : IConsumer<CheckOrder>
{
    public Task Consume(ConsumeContext<CheckOrder> context) =>
        context.RespondAsync(new OrderStatus(context.Message.OrderId, "Accepted"));
}

x.AddRequestClient<CheckOrder>();
var resp = await client.GetResponse<OrderStatus>(new CheckOrder(id));
```

```text
response: Accepted
no consumer -> RequestTimeoutException: Timeout waiting for response, RequestId: c0200000-...
```

Без потребителя клиент через заданный таймаут (в опыте `RequestTimeout.After(s: 2)`) получает `RequestTimeoutException`. Запрос-ответ связывает сервисы по времени, как и RPC ([RabbitMQ: шаблоны](../../../rabbitmq/modules/04-messaging-patterns/overview.ru.md)): ставьте таймауты и не злоупотребляйте.

> ⚠️ Проверить: значение таймаута запроса по умолчанию (по документации 30 секунд) здесь не проверялось.

## Параллелизм

`ConcurrentMessageLimit` ограничивает число одновременно обрабатываемых сообщений конечной точки. 8 сообщений по 200 мс:

```text
ConcurrentMessageLimit=1: max parallel = 1, total 1051 ms
ConcurrentMessageLimit=4: max parallel = 4, total 744 ms
```

При лимите 1 сообщения обрабатываются последовательно, при 4 — до четырёх одновременно. Значение по умолчанию здесь не измерялось: проверены только лимиты 1 и 4. Раз обработка может идти параллельно, у состояния, разделяемого между вызовами, нужна синхронизация.

## Топология RabbitMQ

Для транспорта RabbitMQ:

```csharp
x.SetKebabCaseEndpointNameFormatter();
x.UsingRabbitMq((ctx, c) =>
{
    c.Host("localhost", "/", h => { h.Username("guest"); h.Password("guest"); });
    c.ConfigureEndpoints(ctx);
});
```

После публикации `OrderSubmitted` для двух потребителей в брокере:

```text
exchanges Shop:OrderSubmitted fanout
exchanges audit fanout
exchanges order-submitted fanout
queues audit classic (x-queue-type: classic)
queues order-submitted classic (x-queue-type: classic)
binding Shop:OrderSubmitted -> audit exchange
binding Shop:OrderSubmitted -> order-submitted exchange
binding audit -> audit queue
binding order-submitted -> order-submitted queue
```

Цепочка такая: обменник типа сообщения `Shop:OrderSubmitted` (fanout) → обменник конечной точки (fanout) → очередь. Издатель знает только тип сообщения, а каждый потребитель имеет свою очередь ([RabbitMQ: обменники](../../../rabbitmq/modules/02-exchanges-and-routing/overview.ru.md)). Очереди — `classic`.

> ⚠️ Проверить: кворумные очереди (`SetQuorumQueue`) и другие настройки транспорта RabbitMQ не проверялись.

## Тестовая оснастка (test harness)

Для тестов есть `AddMassTransitTestHarness` — шина в памяти с наблюдением за сообщениями:

```csharp
services.AddMassTransitTestHarness(x => x.AddConsumer<OrderSubmittedConsumer>());
await using var sp = services.BuildServiceProvider(true);
var harness = sp.GetRequiredService<ITestHarness>();
await harness.Start();
await harness.Bus.Publish(order);
Console.WriteLine(await harness.Published.Any<OrderSubmitted>());
Console.WriteLine(await harness.Consumed.Any<OrderSubmitted>());
var ch = harness.GetConsumerHarness<OrderSubmittedConsumer>();
Console.WriteLine(await ch.Consumed.Any<OrderSubmitted>());
```

```text
published: True
consumed : True
consumer harness consumed: True
```

`Published`, `Consumed`, `Sent` и оснастка отдельного потребителя (`GetConsumerHarness`) ожидают события с таймаутом: тест не нужно «усыплять» `Task.Delay`. Подробности и саги — в [экспертной теории](expert.ru.md).

## Телеметрия использования

В 8.x есть анонимная **телеметрия использования**: по документации данные отправляются один раз через пять минут после запуска шины; отключается `DisableUsageTelemetry()` в конфигурации или переменной окружения `MASSTRANSIT_USAGE_TELEMETRY=false`. Опыты длились меньше пяти минут, и отправка не проверялась.

> ⚠️ Проверить: состав отправляемых данных здесь не проверялся; отключите телеметрию, если политика организации этого требует.

## Лучшие практики

- Потребители идемпотентны: повторы и доставка «минимум один раз».
- Задавайте политику повторов и следите за очередью `_error`.
- Для тестов — test harness, а не `Task.Delay`.
- Команды — `Send`, события — `Publish`; типы сообщений — в стабильных пространствах имён (они часть адреса).
- Не помещайте в сообщения лишние данные и изменяемые ссылки.
- `kebab-case` для имён очередей (или единый форматтер в команде).

## Частые ошибки

- ❌ Не настраивать повторы: сообщение сразу уходит в `_error` → ✅ `UseMessageRetry`.
- ❌ Переименовать пространство имён сообщения → ✅ изменится адрес обменника; старые и новые версии перестанут видеть друг друга.
- ❌ Синхронный `GetResponse` без таймаута → ✅ `RequestTimeout`.
- ❌ Тесты на `Task.Delay` → ✅ `harness.Consumed.Any<T>()`.

## Связанные материалы

- Раньше: [основы](basics.ru.md)
- Дальше: [экспертный уровень](expert.ru.md) — саги, внутреннее устройство, подвохи
- [Polly](../polly/overview.ru.md) — повторы вне брокера; [Testcontainers](../testcontainers/overview.ru.md) — настоящий брокер в тестах
- [RabbitMQ: надёжность](../../../rabbitmq/modules/05-reliability/overview.ru.md)

## Источники

- [MassTransit: exceptions and retry](https://masstransit.io/documentation/concepts/exceptions)
- [MassTransit: request-response](https://masstransit.io/documentation/concepts/requests)
- [MassTransit: RabbitMQ transport](https://masstransit.io/documentation/transports/rabbitmq)
- [MassTransit: testing](https://masstransit.io/documentation/concepts/testing)
- [MassTransit: usage telemetry](https://masstransit.io/documentation/configuration/usage-telemetry)
