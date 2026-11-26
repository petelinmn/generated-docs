---
title: "Quiz: transactions"
lang: en
track: postgresql
kind: module
doc: quiz
slug: 06-transactions
part: 2
tags: [transaction, isolation-level, mvcc, locks, deadlock]
versions: "PostgreSQL 17+"
status: draft
updated: 2026-11-26
---

# Quiz: transactions

🌐 [Русский](quiz.ru.md) · **English**

> Answer first, then open the answer. The mark shows the level: 🟢 [basics](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 Which property means "all or nothing"?

- A) Atomicity
- B) Consistency
- C) Isolation
- D) Durability

<details><summary>Answer</summary>

**A.**

→ [Basics](basics.en.md)

</details>

## 2. 🟢 A statement failed inside `BEGIN`. What can you do next?

- A) Continue with other statements
- B) Only roll back, fully or to a savepoint
- C) `COMMIT` the successful part
- D) Nothing, the connection is closed

<details><summary>Answer</summary>

**B.** `COMMIT` of an aborted transaction rolls it back.

→ [Basics](basics.en.md)

</details>

## 3. 🟢 The default isolation level of PostgreSQL is

- A) Read Uncommitted
- B) Read Committed
- C) Repeatable Read
- D) Serializable

<details><summary>Answer</summary>

**B.**

→ [Basics](basics.en.md)

</details>

## 4. 🟢 Under Read Committed, can two identical `SELECT`s in one transaction differ?

- A) No, never
- B) Yes, if another transaction committed between them
- C) Only with uncommitted data
- D) Only after `SAVEPOINT`

<details><summary>Answer</summary>

**B.** Each statement takes a new snapshot.

→ [Basics](basics.en.md)

</details>

## 5. 🟡 Balance is 100. Two sessions read it, then write `100 - 10` and `100 - 20`. Result?

- A) 70
- B) 80 or 90, depending on who writes last
- C) 100
- D) An error

<details><summary>Answer</summary>

**B.** A lost update; in the order shown in the module the result was 80.

→ [Advanced](advanced.en.md)

</details>

## 6. 🟡 What is the final balance?

```sql
-- balance = 100; session A and then session B run, A commits after B started waiting
UPDATE accounts SET balance = balance - 10 WHERE id = 1;   -- A, inside a transaction
UPDATE accounts SET balance = balance - 20 WHERE id = 1;   -- B
```

<details><summary>Answer</summary>

```text
70
```

B waits for A and applies its change to the new row version.

→ [Advanced](advanced.en.md)

</details>

## 7. 🟡 Select all true statements about `FOR UPDATE`

- [ ] A) A plain `SELECT` of the row still works
- [ ] B) `NOWAIT` raises an error instead of waiting
- [ ] C) `SKIP LOCKED` waits for locked rows
- [ ] D) Another `UPDATE` of the row waits

<details><summary>Answer</summary>

**A, B, D.** `SKIP LOCKED` passes over them.

→ [Advanced](advanced.en.md)

</details>

## 8. 🟡 Two on-call doctors each remove themselves after checking that two are on call. Which level prevents zero doctors?

- A) Read Committed
- B) Repeatable Read
- C) Serializable
- D) Any of them

<details><summary>Answer</summary>

**C.** This is write skew: different rows are written, so Repeatable Read allows it.

→ [Advanced](advanced.en.md)

</details>

## 9. 🟡 What do you do after `ERROR: deadlock detected`?

- A) Repeat the last statement
- B) Roll back and retry the whole transaction
- C) Restart the server
- D) Ignore it

<details><summary>Answer</summary>

**B.** The transaction is aborted.

→ [Advanced](advanced.en.md)

</details>

## 10. 🔴 What does this print for the new row?

```sql
CREATE TABLE t (id int GENERATED ALWAYS AS IDENTITY, v int);
BEGIN; INSERT INTO t(v) VALUES (1); ROLLBACK;
INSERT INTO t(v) VALUES (2) RETURNING id;
```

<details><summary>Answer</summary>

```text
2
```

Sequence values are not returned on rollback.

→ [Expert](expert.en.md)

</details>

## 11. 🔴 Which lock blocks a plain `SELECT`?

- A) `FOR UPDATE`
- B) `ROW EXCLUSIVE`
- C) `ACCESS EXCLUSIVE`
- D) `SHARE`

<details><summary>Answer</summary>

**C.** Only `ACCESS EXCLUSIVE`.

→ [Expert](expert.en.md)

</details>

## 12. 🔴 Open question: how would you prevent selling the last item twice?

<details><summary>Key points</summary>

- A conditional atomic update `... SET stock = stock - 1 WHERE id = ? AND stock > 0` and a check of the affected row count.
- A `CHECK (stock >= 0)` constraint as a safety net.
- `FOR UPDATE` or Serializable when the decision needs several rows; retries.

→ [Expert](expert.en.md)

</details>
