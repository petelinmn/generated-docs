---
title: "Упражнения: MassTransit"
lang: ru
track: csharp
kind: library
doc: exercises
slug: masstransit
part: 3
tags: [masstransit, consumer, retry, request-response, test-harness]
status: draft
updated: 2026-11-24
---

# Упражнения: MassTransit

🌐 **Русский** · [English](exercises.en.md)

> Решайте сами; подсказки и решения — под спойлерами. Консольный проект `net11.0` с пакетом `MassTransit` 8.5.12; сообщения — в пространстве имён `Shop` (см. [основы](basics.ru.md#как-проверялся-код)). Решения запускались на .NET 11.

## 1. 🟢 Первое сообщение

**Задача:** создайте шину «в памяти» с потребителем `OrderSubmittedConsumer`, опубликуйте `OrderSubmitted` и выведите покупателя и сумму. Не забудьте корректно освободить контейнер.

**Ожидаемый результат:** строка вида `alice 19,99` (формат числа зависит от культуры).

<details><summary>Подсказка</summary>

`AddMassTransit` → `AddConsumer` → `UsingInMemory(... ConfigureEndpoints)`, шина `IBusControl`; `await using`.

</details>

<details><summary>Решение</summary>

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

См. [основы](basics.ru.md#шина-в-памяти).

</details>

## 2. 🟢 Событие и команда

**Задача:** добавьте второй потребитель `AuditConsumer` на тот же тип. Опубликуйте сообщение, затем отправьте его командой в очередь `audit`. Сколько потребителей получило каждое?

**Ожидаемый результат:** `Publish` — оба потребителя, `Send` — только `AuditConsumer`.

<details><summary>Подсказка</summary>

`bus.GetSendEndpoint(new Uri("queue:audit"))`.

</details>

<details><summary>Решение</summary>

См. [основы](basics.ru.md#publish-и-send).

</details>

## 3. 🟡 Повторы

**Задача:** потребитель `FlakyConsumer` падает на первых двух попытках. Настройте конечную точку на три повтора с интервалом 100 мс и выведите номера попыток.

**Ожидаемый результат:** попытки 1, 2, 3, на третьей успех (заголовок повтора 0, 1, 2).

<details><summary>Подсказка</summary>

Явная конечная точка: `ReceiveEndpoint("flaky", e => { e.UseMessageRetry(…); e.ConfigureConsumer<FlakyConsumer>(ctx); })`.

</details>

<details><summary>Решение</summary>

См. [продвинутый](advanced.ru.md#повторы).

</details>

## 4. 🔴 Тест на оснастке и сага

**Задача:** (а) напишите проверку потребителя на тестовой оснастке без `Task.Delay`; (б) опишите сагу «отправлен → оплачен → отгружен» и проверьте, что отгрузка до оплаты оставляет сагу в состоянии `Submitted`.

**Ожидаемый результат:** (а) `published: True`, `consumed: True`; (б) `state after OrderShipped in Submitted: still Submitted`.

<details><summary>Подсказка</summary>

`AddMassTransitTestHarness`, `harness.Consumed.Any<T>()`, `GetSagaStateMachineHarness<TMachine, TInstance>()` и `Exists(id, m => m.Submitted)`.

</details>

<details><summary>Решение</summary>

См. [продвинутый](advanced.ru.md#тестовая-оснастка-test-harness) и [экспертный](expert.ru.md#саги-и-конечные-автоматы): полный код автомата и проверки.

</details>
