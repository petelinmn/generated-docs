---
title: "Распределённые транзакции: продвинутая теория"
lang: ru
track: system-design
kind: module
doc: advanced
slug: 10-distributed-transactions
part: 2
tags: [system-design, distributed-transactions, two-phase-commit, saga, outbox, inbox, idempotency]
versions: ".NET 11, C# 15; Microsoft.Data.Sqlite 11.0.0"
status: draft
updated: 2026-11-26
---

# Распределённые транзакции: продвинутая теория

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · **2. Продвинутая** · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

## Оркестрация и хореография

| | Хореография | Оркестрация |
|---|---|---|
| Кто решает | каждый реагирует на события | центральный координатор |
| Подходит | простые процессы из нескольких сервисов | сложные процессы |
| Плюс | нет лишнего компонента | процесс виден в одном месте |
| Минус | трудно проследить, возможны циклы | координатор нужно разрабатывать и защищать |

## Виды шагов

- **Компенсируемые** — можно отменить.
- **Pivot** — точка невозврата.
- **Повторяемые** — после pivot, идемпотентны и доводятся до конца.

```csharp
// After the pivot there is no way back: someone has to finish forward.
if (pastPivot) return SagaOutcome.NeedsAttention;

while (done.TryPop(out SagaStep? previous))
{
    await previous.Compensate!(); // reverse order
    journal.Add($"undo {previous.Name}");
}
```

Файл: [Ex02_Saga.cs](code/Examples/Ex02_Saga.cs). Пример хранит состояние в памяти и не повторяет шаги: настоящий оркестратор сохраняет прогресс после каждого шага.

## Аномалии

У saga нет изоляции: возможны потерянные обновления, грязные и неповторяющиеся чтения. Контрмеры: семантическая блокировка (статус «в обработке»), коммутативные обновления, перестановка шагов (рискованное — после pivot), повторное чтение перед записью, версии.

## Relay и inbox

```csharp
publish(id, payload);
if (crashBeforeMark) return;
Execute("UPDATE outbox SET sent = 1 WHERE id = $a", id);
```

Сбой между публикацией и отметкой даёт повтор, но не потерю. Получатель запоминает идентификаторы (inbox).

## Лучшие практики

- Делайте каждый шаг и компенсацию идемпотентными.
- Храните состояние саги и показывайте его оператору.
- Задавайте таймауты шагов.
- Очищайте обработанные записи outbox и inbox.

## Частые ошибки

- ❌ Помечать отправленным до публикации → ✅ после.
- ❌ Несколько relay без блокировки строк → ✅ захват пачки, иначе дубли и нарушение порядка.
- ❌ Компенсация, зависящая от исчезнувших данных → ✅ сохранять нужное в состоянии саги.
- ❌ Необратимое действие (письмо) в начале → ✅ ближе к концу.

## Сравнение с другими треками

- [Саги MassTransit](../../../csharp/libraries/masstransit/overview.ru.md).
- [Семантика доставки Kafka](../../../kafka/modules/05-delivery-semantics/overview.ru.md).

## Связанные материалы

- [Базовая теория](basics.ru.md)
- [Экспертная теория](expert.ru.md)

## Источники

- [Паттерн Saga](https://learn.microsoft.com/azure/architecture/patterns/saga)
- [Transactional outbox](https://learn.microsoft.com/azure/architecture/databases/guide/transactional-outbox-cosmos)
