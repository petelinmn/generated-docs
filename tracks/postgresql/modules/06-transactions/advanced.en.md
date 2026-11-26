---
title: "Transactions: advanced"
lang: en
track: postgresql
kind: module
doc: advanced
slug: 06-transactions
part: 2
tags: [transaction, isolation-level, mvcc, locks, deadlock]
versions: "PostgreSQL 17+"
status: draft
updated: 2026-11-26
---

# Transactions: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

## Isolation levels

| Level | Dirty read | Non-repeatable read | Phantom read | Serialization anomaly |
|---|---|---|---|---|
| Read Uncommitted | not in PostgreSQL | possible | possible | possible |
| Read Committed | no | possible | possible | possible |
| Repeatable Read | no | no | not in PostgreSQL | possible |
| Serializable | no | no | no | no |

In PostgreSQL `READ UNCOMMITTED` behaves as Read Committed.

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

The snapshot is stable, new rows are invisible too, and updating a row that somebody else changed since the snapshot fails.

## Lost update

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

The application computed from a stale value. Three remedies:

1. Let the database compute. Under Read Committed the second `UPDATE` waits and then works on the new row version:

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

2. Lock the row when reading: `SELECT ... FOR UPDATE`.
3. Use Repeatable Read or Serializable and retry on failure.

## Row locks

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

`NOWAIT` fails at once, `SKIP LOCKED` passes over locked rows (a typical job queue), `lock_timeout` limits waiting.

## Write skew

Two transactions read the same data and write **different** rows, so no write conflict arises.

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

## Deadlocks

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

After `deadlock_timeout` (1 s by default) a waiting process checks for a cycle and PostgreSQL aborts one of the transactions. Which one is hard to predict; in this run it was A.

## Best practices

- Keep transactions short; do slow work outside them.
- Take locks on rows in one fixed order everywhere.
- Retry the whole transaction on SQLSTATE `40001` (serialization failure) and `40P01` (deadlock).
- Prefer a single atomic statement or a constraint to a read-then-write in code.

## Common mistakes

- ❌ Retrying only the failed statement → ✅ the whole transaction, with fresh reads.
- ❌ `SELECT FOR UPDATE` on rows that do not exist yet to prevent duplicates → ✅ a unique constraint.
- ❌ Serializable without retry logic → ✅ failures are a normal part of it.

## Related materials

- [Basics](basics.en.md)
- [Expert theory](expert.en.md)

## Sources

- [PostgreSQL 17: transaction isolation](https://www.postgresql.org/docs/17/transaction-iso.html)
- [PostgreSQL 17: explicit locking](https://www.postgresql.org/docs/17/explicit-locking.html)
