---
title: "Сообщения и события: продвинутая теория"
lang: ru
track: system-design
kind: module
doc: advanced
slug: 08-messaging-and-events
part: 2
tags: [system-design, message-broker, events, delivery-semantics, idempotency, backpressure, poison-message]
versions: ".NET 11, C# 15"
status: verified
updated: 2026-11-27
---

# Сообщения и события: продвинутая теория

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · **2. Продвинутая** · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

## Семантика доставки

| Семантика | Когда ack | При сбое |
|---|---|---|
| at-most-once | до обработки | сообщение теряется |
| at-least-once | после обработки | повторяется |
| «эффект один раз» | после, плюс дедупликация | повтор безвреден |

```csharp
// In a real system the id and the effect are saved in one transaction.
public void ApplyOnce(Message message)
{
    if (_processed.Add(message.Id)) Balance += message.Amount;
}
```

Три сообщения на 10, 20 и 30 со сбоем на втором дают: ack до обработки — 40, после без дедупликации — 80, с дедупликацией — 60. Файл: [Ex02_Delivery.cs](code/Examples/Ex02_Delivery.cs).

## Порядок

Порядок обычно сохраняется только внутри одной партиции или очереди с одним потребителем; повторы и параллельные потребители его нарушают. Сообщения одной сущности направляют по ключу в одну партицию.

## Backpressure

```csharp
public static Channel<int> Create(int capacity, BoundedChannelFullMode mode) =>
    Channel.CreateBounded<int>(new BoundedChannelOptions(capacity) { FullMode = mode });
```

| `FullMode` | Что происходит при заполнении | Остаётся (ёмкость 2, пишем 1–5) |
|---|---|---|
| `Wait` (по умолчанию) | `WriteAsync` ждёт, `TryWrite` возвращает `false` | 1, 2 |
| `DropOldest` | выбрасывается самый старый | 4, 5 |
| `DropNewest` | выбрасывается самый новый в буфере | 1, 5 |
| `DropWrite` | выбрасывается записываемый | 1, 2 |

Файл: [Ex01_Backpressure.cs](code/Examples/Ex01_Backpressure.cs).

## Dead-letter

```csharp
// Requeue at the tail so a failing message does not block the others.
if (item.Attempt < maxAttempts) _queue.Enqueue((item.Body, item.Attempt + 1));
else DeadLetters.Add(item.Body);
```

Сообщение, которое не обработать никогда (poison message), после лимита уходит в отдельную очередь для разбора. Файл: [Ex04_DeadLetter.cs](code/Examples/Ex04_DeadLetter.cs).

## Лучшие практики

- Давайте сообщению идентификатор, тип и версию схемы.
- Мониторьте глубину очереди, лаг и возраст старейшего сообщения.
- Изменяйте схему совместимо.
- Имейте процедуру разбора и повторной отправки из dead-letter.

## Частые ошибки

- ❌ Записать в базу и отдельно опубликовать → ✅ outbox: событие в той же транзакции, публикация отдельным процессом.
- ❌ Неограниченная очередь в памяти → ✅ ограниченная и явная политика.
- ❌ Повторять бесконечно → ✅ лимит и задержка.
- ❌ Дедупликация только в памяти → ✅ в долговечном хранилище.

## Сравнение с другими треками

- [Dead-letter и повторы в RabbitMQ](../../../rabbitmq/modules/06-dead-lettering-and-retries/overview.ru.md).
- [Семантика доставки в Kafka](../../../kafka/modules/05-delivery-semantics/overview.ru.md).
- [MassTransit](../../../csharp/libraries/masstransit/overview.ru.md).

## Связанные материалы

- [Базовая теория](basics.ru.md)
- [Экспертная теория](expert.ru.md)

## Источники

- [Channels в .NET](https://learn.microsoft.com/dotnet/core/extensions/channels)
- [RabbitMQ: подтверждения](https://www.rabbitmq.com/docs/confirms)
