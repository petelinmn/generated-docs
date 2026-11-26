---
title: "Transactions"
lang: en
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

# Transactions

🌐 [Русский](overview.ru.md) · **English**

## In a nutshell

A transaction is a group of statements that takes effect as a whole or not at all. PostgreSQL runs many transactions at once and keeps them apart with MVCC (multiversion concurrency control): writers create new row versions, readers see a consistent snapshot. How much one transaction notices another is set by the isolation level, and conflicts between writers are settled by locks.

## Key ideas

- ACID: atomicity, consistency, isolation, durability.
- After an error a transaction accepts nothing but a rollback, or a rollback to a savepoint.
- The default level is Read Committed: every statement sees what had been committed when it started.
- Repeatable Read keeps one snapshot for the whole transaction.
- Serializable refuses combinations that no serial order could produce; the application retries.
- Plain reads are not blocked by row locks; writers of the same row wait.
- "Read, compute in the application, write" loses updates unless protected.

## Minimal example

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

## When to use and when not

- ✅ Several changes that must be applied together.
- ✅ A decision based on data you have just read.
- ❌ Keeping a transaction open while waiting for a user or a remote service.
- ❌ Assuming the default level protects every business rule.

## Levels of this material

| Level | What's inside |
|---|---|
| [1. Basics](basics.en.md) | ACID, `BEGIN`/`COMMIT`, savepoints, Read Committed |
| [2. Advanced](advanced.en.md) | isolation levels, lost updates, row locks, write skew, deadlocks |
| [3. Expert](expert.en.md) | MVCC inside, reference of locks and settings, gotchas, interview questions |
| [4. Self-check](quiz.en.md) | a quiz and [exercises](exercises.en.md) |
| 5. Runnable examples | not created yet — `/learn postgresql 06-transactions level 5` |
| 6. Tests | not created yet — `/learn postgresql 06-transactions level 6` |

## Related materials

- Prerequisites: [02 · SQL queries](../02-sql-queries/overview.en.md), [05 · Indexes](../05-indexes/overview.en.md)
- Next: the following module of the [PostgreSQL roadmap](../../overview.en.md)
- The design view: [consistency and CAP](../../../system-design/modules/07-consistency-and-cap/overview.en.md), [distributed transactions](../../../system-design/modules/10-distributed-transactions/overview.en.md)
- From .NET: [EF Core](../../../csharp/modules/15-ef-core/overview.en.md)
- [07 · Views and functions](../07-views-functions/overview.en.md)
