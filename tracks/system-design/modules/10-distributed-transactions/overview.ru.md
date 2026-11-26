---
title: "Распределённые транзакции"
lang: ru
track: system-design
kind: module
doc: overview
slug: 10-distributed-transactions
part: 2
levels: [1, 2, 3, 4, 5, 6]
tags: [system-design, distributed-transactions, two-phase-commit, saga, outbox, inbox, idempotency]
versions: ".NET 11, C# 15; Microsoft.Data.Sqlite 11.0.0"
status: draft
updated: 2026-11-26
---

# Распределённые транзакции

🌐 **Русский** · [English](overview.en.md)

## Коротко

Одна операция затрагивает несколько сервисов и баз, а общей транзакции у них обычно нет. Здесь — способы не оставить систему в половинчатом состоянии: двухфазная фиксация, saga, outbox и inbox.

## Ключевые идеи

- Локальная транзакция атомарна, цепочка вызовов — нет.
- 2PC даёт атомарность ценой блокировки при сбое координатора.
- Saga — последовательность локальных транзакций с компенсациями.
- Компенсация отменяет смысл действия, а не «откатывает время».
- После точки невозврата (pivot) идти можно только вперёд.
- Outbox сохраняет событие вместе с данными.
- Inbox делает повторную доставку безвредной.

## Минимальный пример

```csharp
Execute("INSERT INTO orders (id) VALUES ($a)", orderId);
Execute("INSERT INTO outbox (payload) VALUES ($a)", $"OrderPlaced:{orderId}");
```

## Когда применять, а когда нет

- ✅ Бизнес-процесс проходит через несколько владельцев данных.
- ✅ Нужно надёжно публиковать события.
- ❌ Всё можно сделать одной локальной транзакцией: сделайте так.
- ❌ Saga там, где недопустимо видеть промежуточное состояние.

## Уровни материала

| Уровень | Что внутри |
|---|---|
| [1. Базовая теория](basics.ru.md) | проблема двойной записи, 2PC, saga, outbox |
| [2. Продвинутая теория](advanced.ru.md) | оркестрация и хореография, pivot, аномалии, inbox |
| [3. Экспертная теория](expert.ru.md) | справочник, подвохи, вопросы |
| [4. Самопроверка](quiz.ru.md) | квиз и [упражнения](exercises.ru.md) |
| [5. Запускаемые примеры](code/Examples/) | 2PC, saga, outbox на SQLite |
| [6. Тесты](code/Tests/) | `dotnet test` в папке `code/` |

## Связанные материалы

- Пререквизиты: [согласованность и CAP](../07-consistency-and-cap/overview.ru.md), [сообщения и события](../08-messaging-and-events/overview.ru.md)
- Дальше: [архитектурные стили](../11-architecture-styles/overview.ru.md)
- В других треках: [саги в MassTransit](../../../csharp/libraries/masstransit/overview.ru.md), [транзакции в EF Core](../../../csharp/modules/15-ef-core/overview.ru.md)
