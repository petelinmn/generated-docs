---
title: "Распределённые транзакции: базовая теория"
lang: ru
track: system-design
kind: module
doc: basics
slug: 10-distributed-transactions
part: 2
tags: [system-design, distributed-transactions, two-phase-commit, saga, outbox, inbox, idempotency]
versions: ".NET 11, C# 15; Microsoft.Data.Sqlite 11.0.0"
status: draft
updated: 2026-11-26
---

# Распределённые транзакции: базовая теория

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [обзор](overview.ru.md) · **1. Базовая** · [2. Продвинутая](advanced.ru.md) · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** транзакции, сообщения · **Время:** ~2 ч

## Зачем это нужно

Сервис записал заказ в свою базу и должен списать деньги в другом. Между этими действиями может случиться что угодно. Результат — заказ без оплаты или оплата без заказа.

## Двойная запись

«Сначала в базу, потом в брокер» — это две независимые операции. Сбой между ними теряет событие; обратный порядок публикует событие о том, чего не произошло.

## Двухфазная фиксация

1. **Prepare:** координатор спрашивает всех, могут ли они зафиксировать.
2. **Commit или abort:** фиксация только при единогласном «да».

```csharp
bool all = true;
foreach (Participant p in participants) all &= p.Prepare();
if (crashAfterPrepare) return;
```

Ответив «да», участник не вправе решать сам. Если координатор исчез, участники остаются в состоянии Prepared и удерживают блокировки. Файл: [Ex01_TwoPhaseCommit.cs](code/Examples/Ex01_TwoPhaseCommit.cs). Это модель без журнала и восстановления.

## Saga

Вместо одной большой транзакции — цепочка локальных. Если шаг не удался, выполненные отменяются компенсирующими действиями в обратном порядке: «отменить резерв», «вернуть деньги».

## Outbox

```csharp
using SqliteTransaction tx = _connection.BeginTransaction();
Execute("INSERT INTO orders (id) VALUES ($a)", orderId);
Execute("INSERT INTO outbox (payload) VALUES ($a)", $"OrderPlaced:{orderId}");
if (failBeforeCommit) throw new InvalidOperationException("Crash"); // dispose rolls back
tx.Commit();
```

Заказ и событие фиксируются вместе. Отдельный процесс (relay) читает таблицу outbox и публикует. Файл: [Ex03_Outbox.cs](code/Examples/Ex03_Outbox.cs).

## Ошибки новичка

- ❌ Вызывать другой сервис внутри открытой транзакции базы → ✅ транзакции короткие и локальные.
- ❌ Считать компенсацию откатом → ✅ это новое действие, видимое всем.
- ❌ Публиковать после `Commit` без outbox → ✅ сбой потеряет событие.
- ❌ Не продумывать отказ компенсации → ✅ повторы и эскалация.

## Связанные материалы

- [Продвинутая теория](advanced.ru.md)
- [MassTransit](../../../csharp/libraries/masstransit/overview.ru.md)

## Источники

- [Паттерн Saga](https://learn.microsoft.com/azure/architecture/patterns/saga)
- [PostgreSQL: PREPARE TRANSACTION](https://www.postgresql.org/docs/current/sql-prepare-transaction.html)
