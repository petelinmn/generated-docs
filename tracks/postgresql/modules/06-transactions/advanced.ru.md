---
title: "Транзакции: продвинутая теория"
lang: ru
track: postgresql
kind: module
doc: advanced
slug: 06-transactions
part: 2
tags: [transaction, isolation-level, mvcc, locks, deadlock]
versions: "PostgreSQL 17+"
status: verified
updated: 2026-11-26
---

# Транзакции: продвинутая теория

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · **2. Продвинутая** · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

## Уровни изоляции

| Уровень | Грязное чтение | Неповторяющееся чтение | Фантомное чтение | Аномалия сериализации |
|---|---|---|---|---|
| Read Uncommitted | в PostgreSQL нет | возможно | возможно | возможна |
| Read Committed | нет | возможно | возможно | возможна |
| Repeatable Read | нет | нет | в PostgreSQL нет | возможна |
| Serializable | нет | нет | нет | нет |

В PostgreSQL `READ UNCOMMITTED` ведёт себя как Read Committed.

## Repeatable Read

```sql
-- A                                              -- B
BEGIN ISOLATION LEVEL REPEATABLE READ;
SELECT balance FROM accounts WHERE id = 1;        -- 100
                                                  UPDATE accounts SET balance = 150 WHERE id = 1;
                                                  INSERT INTO accounts VALUES (3, 5);
SELECT balance FROM accounts WHERE id = 1;        -- 100
SELECT count(*) FROM accounts;                    -- 2
UPDATE accounts SET balance = balance + 1 WHERE id = 1;
-- ERROR:  could not serialize access due to concurrent update
ROLLBACK;
```

Снимок неизменен, новые строки тоже не видны, а обновление строки, изменённой кем-то после снимка, завершается ошибкой.

## Потерянное обновление

```sql
-- A                                         -- B
BEGIN;                                       BEGIN;
SELECT balance FROM accounts WHERE id = 1;   SELECT balance FROM accounts WHERE id = 1;
-- 100                                       -- 100
UPDATE accounts SET balance = 100 - 10
  WHERE id = 1;
COMMIT;
                                             UPDATE accounts SET balance = 100 - 20
                                               WHERE id = 1;
                                             COMMIT;
-- final balance: 80, although 30 was taken
```

Приложение считало от устаревшего значения. Три средства:

1. Пусть считает база. На Read Committed второй `UPDATE` ждёт и затем работает с новой версией строки:

```sql
-- A                                         -- B
BEGIN;
UPDATE accounts SET balance = balance - 10
  WHERE id = 1;
                                             UPDATE accounts SET balance = balance - 20
                                               WHERE id = 1 RETURNING balance;   -- waits
COMMIT;
                                             -- 70
```

2. Блокировать строку при чтении: `SELECT ... FOR UPDATE`.
3. Использовать Repeatable Read или Serializable и повторять при ошибке.

## Блокировки строк

```sql
-- A
BEGIN;
SELECT balance FROM accounts WHERE id = 1 FOR UPDATE;

-- B
SELECT balance FROM accounts WHERE id = 1;                     -- 100, a plain read is not blocked
SELECT balance FROM accounts WHERE id = 1 FOR UPDATE NOWAIT;
-- ERROR:  could not obtain lock on row in relation "accounts"
SELECT id FROM accounts ORDER BY id FOR UPDATE SKIP LOCKED;    -- 2
SET lock_timeout = '500ms';
UPDATE accounts SET balance = 0 WHERE id = 1;
-- ERROR:  canceling statement due to lock timeout
```

`NOWAIT` отказывает сразу, `SKIP LOCKED` пропускает заблокированные строки (типичная очередь заданий), `lock_timeout` ограничивает ожидание.

## Write skew

Две транзакции читают одни и те же данные, а пишут **разные** строки, поэтому конфликта записи не возникает.

```sql
CREATE TABLE doctors (name text PRIMARY KEY, on_call boolean NOT NULL);
INSERT INTO doctors VALUES ('ann', true), ('bob', true);

-- rule: at least one doctor stays on call
-- A                                              -- B
BEGIN ISOLATION LEVEL REPEATABLE READ;            BEGIN ISOLATION LEVEL REPEATABLE READ;
SELECT count(*) FROM doctors WHERE on_call; -- 2   SELECT count(*) FROM doctors WHERE on_call; -- 2
UPDATE doctors SET on_call = false
  WHERE name = 'ann';
                                                  UPDATE doctors SET on_call = false
                                                    WHERE name = 'bob';
COMMIT;                                           COMMIT;
-- nobody is on call: count = 0
```

```text
the same steps with ISOLATION LEVEL SERIALIZABLE: A commits, B gets
ERROR:  40001: could not serialize access due to read/write dependencies among transactions
HINT:  The transaction might succeed if retried.
-- one doctor stays on call
```

## Deadlock

```sql
-- A                                         -- B
BEGIN;                                       BEGIN;
UPDATE accounts SET balance = balance - 1    UPDATE accounts SET balance = balance - 1
  WHERE id = 1;                                WHERE id = 2;
UPDATE accounts SET balance = balance + 1
  WHERE id = 2;   -- waits
                                             UPDATE accounts SET balance = balance + 1
                                               WHERE id = 1;
-- A gets:  ERROR:  deadlock detected
--                                           B continues
COMMIT;           -- rolls back              COMMIT;
-- 1 101 | 2 99   (only B's changes)
```

Через `deadlock_timeout` (по умолчанию 1 с) ожидающий процесс проверяет, нет ли цикла, и PostgreSQL прерывает одну из транзакций. Какую именно, предсказать трудно; в этом прогоне это была A.

## Лучшие практики

- Держите транзакции короткими; медленную работу делайте вне их.
- Берите блокировки строк везде в одном порядке.
- Повторяйте всю транзакцию при SQLSTATE `40001` (ошибка сериализации) и `40P01` (deadlock).
- Предпочитайте один атомарный оператор или ограничение схеме «прочитать и записать» в коде.

## Частые ошибки

- ❌ Повторять только упавший оператор → ✅ всю транзакцию с новыми чтениями.
- ❌ `SELECT FOR UPDATE` по ещё не существующим строкам против дубликатов → ✅ ограничение уникальности.
- ❌ Serializable без повторов → ✅ ошибки — нормальная часть этого уровня.

## Связанные материалы

- [Базовая теория](basics.ru.md)
- [Экспертная теория](expert.ru.md)

## Источники

- [PostgreSQL 17: изоляция транзакций](https://www.postgresql.org/docs/17/transaction-iso.html)
- [PostgreSQL 17: явные блокировки](https://www.postgresql.org/docs/17/explicit-locking.html)
