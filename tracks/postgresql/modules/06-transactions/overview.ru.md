---
title: "Транзакции"
lang: ru
track: postgresql
kind: module
doc: overview
slug: 06-transactions
part: 2
levels: [1, 2, 3, 4]
tags: [transaction, isolation-level, mvcc, locks, deadlock]
versions: "PostgreSQL 17+"
status: draft
updated: 2026-11-26
---

# Транзакции

🌐 **Русский** · [English](overview.en.md)

## Коротко

Транзакция — это группа операторов, которая применяется целиком или не применяется вовсе. PostgreSQL выполняет много транзакций одновременно и разделяет их с помощью MVCC (multiversion concurrency control): писатели создают новые версии строк, читатели видят согласованный снимок. Насколько одна транзакция замечает другую, задаёт уровень изоляции, а конфликты писателей решают блокировки.

## Ключевые идеи

- ACID: атомарность, согласованность, изоляция, долговечность.
- После ошибки транзакция принимает только откат или откат к точке сохранения.
- Уровень по умолчанию — Read Committed: каждый оператор видит то, что было зафиксировано к его началу.
- Repeatable Read держит один снимок на всю транзакцию.
- Serializable отвергает сочетания, которых не дал бы ни один последовательный порядок; приложение повторяет попытку.
- Обычные чтения не блокируются строковыми блокировками; писатели одной строки ждут.
- «Прочитал, посчитал в приложении, записал» теряет обновления, если это не защищено.

## Минимальный пример

```sql
CREATE TABLE accounts (id int PRIMARY KEY, balance int NOT NULL CHECK (balance >= 0));
INSERT INTO accounts VALUES (1, 100), (2, 100);
```

```sql
BEGIN;
UPDATE accounts SET balance = balance - 30 WHERE id = 1;
UPDATE accounts SET balance = balance - 500 WHERE id = 2;
-- ERROR:  new row for relation "accounts" violates check constraint "accounts_balance_check"
SELECT 1;
-- ERROR:  current transaction is aborted, commands ignored until end of transaction block
COMMIT;   -- rolls back

SELECT id, balance FROM accounts ORDER BY id;   -- 1 100 | 2 100
```

## Когда применять, а когда нет

- ✅ Несколько изменений, которые должны примениться вместе.
- ✅ Решение на основе только что прочитанных данных.
- ❌ Держать транзакцию открытой в ожидании пользователя или удалённого сервиса.
- ❌ Считать, что уровень по умолчанию защищает любое бизнес-правило.

## Уровни материала

| Уровень | Что внутри |
|---|---|
| [1. Базовая теория](basics.ru.md) | ACID, `BEGIN`/`COMMIT`, точки сохранения, Read Committed |
| [2. Продвинутая теория](advanced.ru.md) | уровни изоляции, потерянные обновления, блокировки строк, write skew, deadlock |
| [3. Экспертная теория](expert.ru.md) | устройство MVCC, справочник блокировок и настроек, подвохи, вопросы с собеседований |
| [4. Самопроверка](quiz.ru.md) | квиз и [упражнения](exercises.ru.md) |
| 5. Запускаемые примеры | ещё не создан — `/learn postgresql 06-transactions level 5` |
| 6. Тесты | ещё не создан — `/learn postgresql 06-transactions level 6` |

## Связанные материалы

- Пререквизиты: [02 · SQL-запросы](../02-sql-queries/overview.ru.md), [05 · Индексы](../05-indexes/overview.ru.md)
- Дальше: следующий модуль по [дорожной карте PostgreSQL](../../overview.ru.md)
- Взгляд проектировщика: [согласованность и CAP](../../../system-design/modules/07-consistency-and-cap/overview.ru.md), [распределённые транзакции](../../../system-design/modules/10-distributed-transactions/overview.ru.md)
- Из .NET: [EF Core](../../../csharp/modules/15-ef-core/overview.ru.md)
- [07 · Представления и функции](../07-views-functions/overview.ru.md)
- [09 · Производительность и EXPLAIN](../09-performance-explain/overview.ru.md)
