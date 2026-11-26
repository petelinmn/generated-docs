---
title: "Сообщения и события: базовая теория"
lang: ru
track: system-design
kind: module
doc: basics
slug: 08-messaging-and-events
part: 2
tags: [system-design, message-broker, events, delivery-semantics, idempotency, backpressure, poison-message]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-26
---

# Сообщения и события: базовая теория

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [обзор](overview.ru.md) · **1. Базовая** · [2. Продвинутая](advanced.ru.md) · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** async/await · **Время:** ~2 ч

## Зачем это нужно

Если оформление заказа синхронно вызывает оплату, склад и почту, то сбой почты ломает заказ, а задержки складываются. Сообщения позволяют принять заказ сразу и сделать остальное позже.

## Команды и события

| | Команда | Событие |
|---|---|---|
| Смысл | «сделай» | «произошло» |
| Имя | `ChargeCard` | `OrderPaid` |
| Получатель | один владелец | сколько угодно подписчиков |

## Очередь и журнал

| | Очередь | Журнал |
|---|---|---|
| После обработки | сообщение удаляется | остаётся до истечения хранения |
| Позиция | ведёт брокер | offset группы потребителей |
| Повторное чтение | обычно нет | да |
| Пример | [RabbitMQ](../../../rabbitmq/modules/01-messaging-basics/overview.ru.md) | [Kafka](../../../kafka/modules/01-kafka-basics/overview.ru.md) |

```csharp
public IReadOnlyList<string> Poll(string group, int max = 100)
{
    int offset = _offsets.GetValueOrDefault(group);
    return _events.GetRange(offset, Math.Min(max, _events.Count - offset));
}
```

Чтение не удаляет события: группа `billing` дочитала, а `search` отстаёт на два, и любая может перемотать назад. Файл: [Ex03_Log.cs](code/Examples/Ex03_Log.cs).

## Подтверждения

Потребитель подтверждает (ack) обработку. Пока подтверждения нет, брокер считает сообщение незавершённым и может доставить снова. В RabbitMQ автоматическое подтверждение — это «отправил и забыл», оно небезопасно для важных данных.

## Ошибки новичка

- ❌ Подтверждать до обработки → ✅ после.
- ❌ Считать, что сообщение придёт ровно один раз → ✅ быть готовым к повтору.
- ❌ Класть в сообщение объект на десятки мегабайт → ✅ ссылку.
- ❌ Событие с именем-приказом → ✅ прошедшее время.

## Связанные материалы

- [Продвинутая теория](advanced.ru.md)
- [Фоновые задачи в .NET](../../../csharp/recipes/background-jobs/overview.ru.md)

## Источники

- [RabbitMQ: подтверждения](https://www.rabbitmq.com/docs/confirms)
- [Паттерн Competing Consumers](https://learn.microsoft.com/azure/architecture/patterns/competing-consumers)
