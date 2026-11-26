---
title: "Exercises: transactions"
lang: en
track: postgresql
kind: module
doc: exercises
slug: 06-transactions
part: 2
tags: [transaction, isolation-level, mvcc, locks, deadlock]
versions: "PostgreSQL 17+"
status: verified
updated: 2026-11-26
---

# Exercises: transactions

🌐 [Русский](exercises.ru.md) · **English**

> Solve it yourself before opening the hints. Use the table from [basics](basics.en.md) and two `psql` sessions.

## 1. 🟢 A safe transfer

**Task:** move 30 from account 1 to account 2 so that money is never lost and the balance never goes negative.

<details><summary>Hint</summary>

One transaction, and a condition in the first `UPDATE`.

</details>

<details><summary>Solution</summary>

```sql
BEGIN;
UPDATE accounts SET balance = balance - 30 WHERE id = 1 AND balance >= 30;
-- check that one row was updated, otherwise ROLLBACK
UPDATE accounts SET balance = balance + 30 WHERE id = 2;
COMMIT;
```

The `CHECK` constraint protects the data even if the application forgets the condition.

</details>

## 2. 🟡 A job queue

**Task:** several workers take unfinished jobs; no job may be taken by two of them and none should wait for another.

<details><summary>Hint</summary>

A locking clause that passes over rows already taken.

</details>

<details><summary>Solution</summary>

```sql
CREATE TABLE jobs (id int PRIMARY KEY, done boolean NOT NULL DEFAULT false);
INSERT INTO jobs (id) VALUES (1), (2), (3);

BEGIN;
SELECT id FROM jobs WHERE NOT done ORDER BY id LIMIT 1 FOR UPDATE SKIP LOCKED;
-- process the job, then mark it done
UPDATE jobs SET done = true WHERE id = 1;
COMMIT;
```

If a worker dies, its transaction is rolled back and the job becomes available again.

</details>

## 3. 🔴 No deadlocks in transfers

**Task:** transfers run in both directions between two accounts at the same time. Make a deadlock between them impossible.

<details><summary>Hint</summary>

Who locks which row first?

</details>

<details><summary>Solution</summary>

```sql
BEGIN;
SELECT id FROM accounts WHERE id IN (1, 2) ORDER BY id FOR UPDATE;
-- both rows are locked in one fixed order, now move the money in either direction
COMMIT;
```

Every transaction takes the locks in the order of `id`, so a cycle of waiting cannot form.

</details>
