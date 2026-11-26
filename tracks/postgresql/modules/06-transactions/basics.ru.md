---
title: "Транзакции: базовая теория"
lang: ru
track: postgresql
kind: module
doc: basics
slug: 06-transactions
part: 2
tags: [transaction, isolation-level, mvcc, locks, deadlock]
versions: "PostgreSQL 17+"
status: draft
updated: 2026-11-26
---

# Транзакции: базовая теория

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [обзор](overview.ru.md) · **1. Базовая** · [2. Продвинутая](advanced.ru.md) · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** `INSERT`, `UPDATE`, `SELECT` · **Время:** ~2 ч

## Данные для примеров

Все результаты получены на PostgreSQL 17.11. Каждый пример начинается с этого состояния.

```sql
CREATE TABLE accounts (id int PRIMARY KEY, balance int NOT NULL CHECK (balance >= 0));
INSERT INTO accounts VALUES (1, 100), (2, 100);
```

## ACID

| Свойство | Смысл |
|---|---|
| Атомарность | все изменения транзакции или ни одного |
| Согласованность | ограничения соблюдаются до и после |
| Изоляция | параллельные транзакции не видят незавершённую работу друг друга |
| Долговечность | зафиксированные изменения переживают сбой |

## BEGIN, COMMIT, ROLLBACK

Без `BEGIN` каждый оператор — отдельная транзакция (autocommit). `BEGIN` открывает блок, который `COMMIT` делает постоянным, а `ROLLBACK` отбрасывает.

## Ошибка прерывает весь блок

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

Первое обновление тоже отменяется, хотя прошло успешно.

## Точки сохранения

Точка сохранения позволяет отменить только часть.

```sql
BEGIN;
UPDATE accounts SET balance = 90 WHERE id = 1;
SAVEPOINT s;
UPDATE accounts SET balance = -1 WHERE id = 2;   -- ERROR: check constraint
ROLLBACK TO SAVEPOINT s;                         -- the transaction is usable again
COMMIT;
-- 1 90 | 2 100
```

## Read Committed

Уровень по умолчанию. Каждый оператор берёт новый снимок зафиксированных данных, поэтому два одинаковых запроса в одной транзакции могут различаться:

```sql
-- A                                         -- B
BEGIN;
SELECT balance FROM accounts WHERE id = 1;   -- 100
                                             UPDATE accounts SET balance = 150 WHERE id = 1;
SELECT balance FROM accounts WHERE id = 1;   -- 150
COMMIT;
```

Незафиксированные изменения других не видны никогда.

## Ошибки новичка

- ❌ Менять деньги двумя отдельными autocommit-операторами → ✅ одна транзакция.
- ❌ Игнорировать ошибку и продолжать → ✅ `ROLLBACK` или точка сохранения.
- ❌ Оставлять сессию «idle in transaction» → ✅ сразу фиксировать или откатывать.
- ❌ Ждать от повторного запроса того же результата → ✅ на Read Committed это не так.

## Связанные материалы

- [Продвинутая теория](advanced.ru.md)

## Источники

- [PostgreSQL 17: изоляция транзакций](https://www.postgresql.org/docs/17/transaction-iso.html)
- [PostgreSQL 17: явные блокировки](https://www.postgresql.org/docs/17/explicit-locking.html)
