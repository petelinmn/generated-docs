---
title: "Transactions: expert"
lang: en
track: postgresql
kind: module
doc: expert
slug: 06-transactions
part: 2
tags: [transaction, isolation-level, mvcc, locks, deadlock]
versions: "PostgreSQL 17+"
status: verified
updated: 2026-11-26
---

# Transactions: expert

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · **3. Expert** · [4. Quiz](quiz.en.md)

## Under the hood

### Row versions

Every row version records `xmin`, the transaction that created it, and `xmax`, which marks deletion or locking. `ctid` is the physical position. An `UPDATE` writes a new version elsewhere:

```sql
-- transaction numbers differ on every server; compare them with each other
SELECT xmin, xmax, ctid, balance FROM accounts WHERE id = 1;   -- 790 | 0 | (0,1) | 100
UPDATE accounts SET balance = 101 WHERE id = 1;
SELECT xmin, xmax, ctid, balance FROM accounts WHERE id = 1;   -- 791 | 0 | (0,3) | 101
```

A snapshot decides which versions are visible. Old versions are removed later by `VACUUM`. A nonzero `xmax` does not necessarily mean the row is deleted: the deleting transaction may be unfinished or rolled back. `ctid` changes on updates and on `VACUUM FULL`, so it is not an identifier.

### Transaction ID wraparound

Transaction identifiers are 32-bit and wrap around. `VACUUM` freezes old rows so that they stay "in the past". If freezing falls far behind, PostgreSQL warns and, with fewer than three million identifiers left, refuses to assign new ones.

### Serializable

Serializable runs on snapshots as Repeatable Read does and additionally tracks read/write dependencies. When a dangerous pattern appears, one transaction is cancelled with `40001`. It does not block as strict locking would, but it requires retries. The documentation advises: declare `READ ONLY` where possible, limit active connections with a pool, avoid "idle in transaction", and drop explicit locks that are no longer needed.

## Reference

| Row lock | Taken by | Conflicts with |
|---|---|---|
| `FOR UPDATE` | `DELETE`, `UPDATE` of key columns, explicit | all four modes |
| `FOR NO KEY UPDATE` | other `UPDATE`s, explicit | `FOR SHARE`, `FOR NO KEY UPDATE`, `FOR UPDATE` |
| `FOR SHARE` | explicit | `FOR NO KEY UPDATE`, `FOR UPDATE` |
| `FOR KEY SHARE` | foreign key checks, explicit | `FOR UPDATE` |

There are eight table-level lock modes, from `ACCESS SHARE` (taken by `SELECT`) to `ACCESS EXCLUSIVE` (for example `DROP TABLE`, `TRUNCATE`, `VACUUM FULL`). Only `ACCESS EXCLUSIVE` blocks a plain `SELECT`.

| Statement | Effect |
|---|---|
| `BEGIN ISOLATION LEVEL ...` | level of this transaction |
| `SET TRANSACTION ...` | the same, before the first query |
| `BEGIN READ ONLY` | forbid writes |
| `SAVEPOINT`, `ROLLBACK TO`, `RELEASE` | partial rollback |
| `LOCK TABLE ... IN ... MODE` | explicit table lock |
| `pg_advisory_lock`, `pg_advisory_xact_lock` | application locks for a session or a transaction |
| `PREPARE TRANSACTION 'id'`, `COMMIT PREPARED`, `ROLLBACK PREPARED` | two-phase commit; off by default (`max_prepared_transactions = 0`): `ERROR:  prepared transactions are disabled` |

| Setting | Default | Limits |
|---|---|---|
| `default_transaction_isolation` | `read committed` | level of new transactions |
| `deadlock_timeout` | `1s` | wait before checking for a deadlock |
| `lock_timeout` | `0` (off) | waiting for a lock |
| `statement_timeout` | `0` (off) | duration of a statement |
| `idle_in_transaction_session_timeout` | `0` (off) | idle time inside a transaction |
| `transaction_timeout` | `0` (off) | duration of a transaction |

Where to look when something waits: `pg_stat_activity` (`state`, `wait_event_type`, `xact_start`) and `pg_locks`; `pg_blocking_pids(pid)` returns who blocks a process.

## Gotchas

### The Repeatable Read snapshot starts at the first query

```sql
-- A                                          -- B
BEGIN ISOLATION LEVEL REPEATABLE READ;
                                              UPDATE accounts SET balance = 150 WHERE id = 1;
SELECT balance FROM accounts WHERE id = 1;    -- 150
COMMIT;
```

### Sequences ignore rollback

```sql
CREATE TABLE t (id int GENERATED ALWAYS AS IDENTITY, v int);
BEGIN;
INSERT INTO t(v) VALUES (1);
ROLLBACK;
INSERT INTO t(v) VALUES (2) RETURNING id;   -- 2, not 1
```

### now() is frozen inside a transaction

```sql
BEGIN;
SELECT now() AS t1 \gset
SELECT pg_sleep(0.2);
SELECT now() = :'t1'::timestamptz            AS now_same,      -- t
       clock_timestamp() > :'t1'::timestamptz AS clock_moved;  -- t
COMMIT;
```

`now()` returns the start time of the transaction; for elapsed time use `clock_timestamp()`.

### DDL is transactional

```sql
BEGIN;
CREATE TABLE temp_x (a int);
ROLLBACK;
SELECT to_regclass('temp_x') IS NULL AS gone;   -- t
```

Unlike in some other databases, a migration can be rolled back as a whole. Commands such as `CREATE INDEX CONCURRENTLY` cannot run inside a transaction.

## Trade-offs and alternatives

| Approach | Pro | Con |
|---|---|---|
| Atomic `UPDATE ... SET x = x - 1` | simple, no retries | only for logic expressible in SQL |
| `SELECT FOR UPDATE` | predictable | waiting, deadlocks |
| Version column (optimistic) | no locks held between read and write | retries, application code |
| Repeatable Read | stable reports | serialization errors, write skew remains |
| Serializable | all rules hold if each transaction is correct alone | retries, tracking overhead |
| Constraints (`UNIQUE`, `CHECK`, `EXCLUDE`) | enforced at any level | only what they can express |

## Interview questions

**1. What is the default isolation level?**

<details><summary>Answer</summary>

Read Committed.

</details>

**2. Can PostgreSQL perform dirty reads?**

<details><summary>Answer</summary>

No. Read Uncommitted behaves as Read Committed.

</details>

**3. How does Repeatable Read differ from the SQL standard minimum?**

<details><summary>Answer</summary>

In PostgreSQL it also prevents phantom reads, but still allows serialization anomalies.

</details>

**4. What happens to an `UPDATE` under Read Committed when the row is being changed?**

<details><summary>Answer</summary>

It waits, then re-evaluates the `WHERE` condition on the new version and applies the change to it.

</details>

**5. What is write skew?**

<details><summary>Answer</summary>

Transactions read overlapping data and write different rows, breaking a rule neither breaks alone. Serializable or explicit locking prevents it.

</details>

**6. How do you handle SQLSTATE 40001?**

<details><summary>Answer</summary>

Retry the whole transaction, with a limit and backoff.

</details>

**7. Does `SELECT ... FOR UPDATE` block readers?**

<details><summary>Answer</summary>

Plain `SELECT` is not blocked. Other writers and lockers of the same row are.

</details>

**8. How do you avoid deadlocks?**

<details><summary>Answer</summary>

Lock objects in a consistent order, keep transactions short, and still be ready to retry.

</details>

**9. Why is a long transaction harmful?**

<details><summary>Answer</summary>

It keeps old row versions from being removed by `VACUUM` and holds its locks.

</details>

**10. Trick question: the transaction was rolled back. Is the identity value reused?**

<details><summary>Answer</summary>

No. Sequence values are not reclaimed, so gaps are normal.

</details>

**11. Trick question: when is the snapshot of a Repeatable Read transaction taken?**

<details><summary>Answer</summary>

At its first query, not at `BEGIN`.

</details>

**12. What is an advisory lock?**

<details><summary>Answer</summary>

A lock on an application-chosen number; the database attaches no meaning to it. It is held for the session or for the transaction.

</details>

## Related materials

- [Advanced theory](advanced.en.md)
- [Quiz](quiz.en.md) and [exercises](exercises.en.md)

## Sources

- [PostgreSQL 17: transaction isolation](https://www.postgresql.org/docs/17/transaction-iso.html)
- [PostgreSQL 17: explicit locking](https://www.postgresql.org/docs/17/explicit-locking.html)
- [PostgreSQL 17: system columns](https://www.postgresql.org/docs/17/ddl-system-columns.html)
- [PostgreSQL 17: routine vacuuming](https://www.postgresql.org/docs/17/routine-vacuuming.html)
- [PostgreSQL 17: client connection defaults](https://www.postgresql.org/docs/17/runtime-config-client.html)
- [PostgreSQL 17: lock management settings](https://www.postgresql.org/docs/17/runtime-config-locks.html)
- [PostgreSQL 17: sequence functions](https://www.postgresql.org/docs/17/functions-sequence.html)
