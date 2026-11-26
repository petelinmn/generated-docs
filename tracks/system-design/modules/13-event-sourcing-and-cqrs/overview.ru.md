---
title: "Event sourcing и CQRS"
lang: ru
track: system-design
kind: module
doc: overview
slug: 13-event-sourcing-and-cqrs
part: 3
levels: [1, 2, 3, 4, 5, 6]
tags: [system-design, event-sourcing, cqrs, events, projection, snapshot, upcasting]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-26
---

# Event sourcing и CQRS

🌐 **Русский** · [English](overview.en.md)

## Коротко

Event sourcing хранит не текущее состояние, а все изменения в виде событий. CQRS разделяет модели записи и чтения. Их часто применяют вместе, но это независимые решения, и оба добавляют сложность.

## Ключевые идеи

- Состояние — это свёртка потока событий.
- События неизменяемы; исправление — новое событие.
- Конкурентные записи отсекаются проверкой версии потока.
- Проекции строят модели чтения и отстают от записи.
- Снимок ускоряет загрузку, но не заменяет события.
- Старые события читают через upcasting.
- Паттерн дорого внедрить и дорого отменить.

## Минимальный пример

```csharp
Account account = Account.Replay([new Deposited(100), new Withdrawn(30)]); // balance 70
```

## Когда применять, а когда нет

- ✅ Нужна история и объяснение, почему состояние такое.
- ✅ Модели чтения сильно отличаются от модели записи.
- ❌ Простой CRUD.
- ❌ Команда не готова к eventual consistency и версионированию событий.

## Уровни материала

| Уровень | Что внутри |
|---|---|
| [1. Базовая теория](basics.ru.md) | события, агрегат, event store, CQRS |
| [2. Продвинутая теория](advanced.ru.md) | проекции, снимки, версии событий, персональные данные |
| [3. Экспертная теория](expert.ru.md) | справочник, подвохи, вопросы |
| [4. Самопроверка](quiz.ru.md) | квиз и [упражнения](exercises.ru.md) |
| [5. Запускаемые примеры](code/Examples/) | event store, проекция, upcasting |
| [6. Тесты](code/Tests/) | `dotnet test` в папке `code/` |

## Связанные материалы

- Пререквизиты: [согласованность](../07-consistency-and-cap/overview.ru.md), [сообщения и события](../08-messaging-and-events/overview.ru.md), [распределённые транзакции](../10-distributed-transactions/overview.ru.md)
- В других треках: [C#: архитектура и CQRS](../../../csharp/modules/21-architecture/overview.ru.md), [основы Kafka](../../../kafka/modules/01-kafka-basics/overview.ru.md)
