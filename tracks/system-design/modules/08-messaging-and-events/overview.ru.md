---
title: "Сообщения и события"
lang: ru
track: system-design
kind: module
doc: overview
slug: 08-messaging-and-events
part: 2
levels: [1, 2, 3, 4, 5, 6]
tags: [system-design, message-broker, events, delivery-semantics, idempotency, backpressure, poison-message]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-26
---

# Сообщения и события

🌐 **Русский** · [English](overview.en.md)

## Коротко

Вместо прямого вызова отправитель кладёт сообщение в посредника, а получатель обрабатывает, когда может. Это развязывает сервисы и сглаживает пики, но приносит повторы, задержку и вопросы порядка.

## Ключевые идеи

- Команда просит что-то сделать, событие сообщает, что случилось.
- Очередь раздаёт работу, журнал хранит историю.
- Момент подтверждения определяет, потеряется сообщение или повторится.
- Идемпотентный потребитель делает повтор безвредным.
- Порядок гарантируется только в узких границах.
- Backpressure не даёт быстрому отправителю утопить получателя.
- Плохое сообщение отправляют в dead-letter, а не повторяют вечно.

## Минимальный пример

```csharp
if (_processed.Add(message.Id)) Balance += message.Amount;
```

## Когда применять, а когда нет

- ✅ Работа может выполниться позже.
- ✅ Несколько независимых получателей одного события.
- ✅ Неравномерная нагрузка.
- ❌ Клиенту нужен результат немедленно.
- ❌ Небольшое приложение без причин для распределённости.

## Уровни материала

| Уровень | Что внутри |
|---|---|
| [1. Базовая теория](basics.ru.md) | команды и события, очередь и журнал, подтверждения |
| [2. Продвинутая теория](advanced.ru.md) | семантика доставки, порядок, backpressure, dead-letter |
| [3. Экспертная теория](expert.ru.md) | справочник, подвохи Channels, вопросы |
| [4. Самопроверка](quiz.ru.md) | квиз и [упражнения](exercises.ru.md) |
| [5. Запускаемые примеры](code/Examples/) | Channels, доставка, журнал, dead-letter |
| [6. Тесты](code/Tests/) | `dotnet test` в папке `code/` |

## Связанные материалы

- Пререквизиты: [согласованность и CAP](../07-consistency-and-cap/overview.ru.md), [проектирование API](../05-api-design/overview.ru.md)
- Дальше: [паттерны надёжности](../09-reliability-patterns/overview.ru.md)
- В других треках: [RabbitMQ](../../../rabbitmq/modules/01-messaging-basics/overview.ru.md), [Kafka](../../../kafka/modules/05-delivery-semantics/overview.ru.md), [MassTransit](../../../csharp/libraries/masstransit/overview.ru.md), [фоновые задачи](../../../csharp/recipes/background-jobs/overview.ru.md)
- [Распределённые транзакции](../10-distributed-transactions/overview.ru.md)
- [Архитектурные стили](../11-architecture-styles/overview.ru.md)
- [Event sourcing и CQRS](../13-event-sourcing-and-cqrs/overview.ru.md)
- [Разбор задач](../17-design-case-studies/overview.ru.md)
