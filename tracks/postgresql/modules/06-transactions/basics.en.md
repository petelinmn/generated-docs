---
title: "Transactions: basics"
lang: en
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

# Transactions: basics

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** `INSERT`, `UPDATE`, `SELECT` · **Time:** ~2 h

## Data for the examples

All results were produced on PostgreSQL 17.11. Each example starts from this state.

```sql
CREATE TABLE accounts (id int PRIMARY KEY, balance int NOT NULL CHECK (balance >= 0));
INSERT INTO accounts VALUES (1, 100), (2, 100);
```

## ACID

| Property | Meaning |
|---|---|
| Atomicity | all changes of a transaction or none |
| Consistency | constraints hold before and after |
| Isolation | concurrent transactions do not see each other's unfinished work |
| Durability | committed changes survive a crash |

## BEGIN, COMMIT, ROLLBACK

Without `BEGIN` every statement is its own transaction (autocommit). `BEGIN` opens a block that `COMMIT` makes permanent and `ROLLBACK` discards.

## An error aborts the whole block

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

The first update is undone too, although it succeeded.

## Savepoints

A savepoint lets you undo only a part.

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

The default level. Every statement takes a new snapshot of committed data, so two identical queries in one transaction may differ:

```sql
-- A                                         -- B
BEGIN;
SELECT balance FROM accounts WHERE id = 1;   -- 100
                                             UPDATE accounts SET balance = 150 WHERE id = 1;
SELECT balance FROM accounts WHERE id = 1;   -- 150
COMMIT;
```

Uncommitted changes of others are never visible.

## Beginner mistakes

- ❌ Changing money in two separate autocommit statements → ✅ one transaction.
- ❌ Ignoring the error and continuing → ✅ `ROLLBACK`, or use a savepoint.
- ❌ Leaving a session "idle in transaction" → ✅ commit or roll back at once.
- ❌ Expecting a repeated query to return the same → ✅ not under Read Committed.

## Related materials

- [Advanced theory](advanced.en.md)

## Sources

- [PostgreSQL 17: transaction isolation](https://www.postgresql.org/docs/17/transaction-iso.html)
- [PostgreSQL 17: explicit locking](https://www.postgresql.org/docs/17/explicit-locking.html)
