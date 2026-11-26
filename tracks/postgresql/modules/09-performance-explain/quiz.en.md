---
title: "Quiz: performance and EXPLAIN"
lang: en
track: postgresql
kind: module
doc: quiz
slug: 09-performance-explain
part: 2
tags: [explain, query-plan, statistics, vacuum, performance]
versions: "PostgreSQL 17+"
status: verified
updated: 2026-11-26
---

# Quiz: performance and EXPLAIN

🌐 [Русский](quiz.ru.md) · **English**

> Answer first, then open the answer. The mark shows the level: 🟢 [basics](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 In `cost=4.60..147.55`, what is 4.60?

- A) Milliseconds to the first row
- B) Estimated startup cost
- C) Rows read
- D) Memory in kB

<details><summary>Answer</summary>

**B.** Costs are planner units, not time.

→ [Basics](basics.en.md)

</details>

## 2. 🟢 What does `EXPLAIN ANALYZE DELETE ...` do?

- A) Only shows the plan
- B) Deletes the rows and shows the plan with actual data
- C) Deletes nothing in a read-only way
- D) Returns an error

<details><summary>Answer</summary>

**B.** Wrap it in `BEGIN ... ROLLBACK`.

→ [Basics](basics.en.md)

</details>

## 3. 🟢 `Buffers: shared hit=40 read=2` means

- A) 40 pages from PostgreSQL's cache, 2 requested from the OS
- B) 40 errors and 2 retries
- C) 40 rows read twice
- D) 42 kB

<details><summary>Answer</summary>

**A.** Pages are 8 kB.

→ [Basics](basics.en.md)

</details>

## 4. 🟢 Which sign points to bad statistics?

- A) High cost
- B) Estimated rows far from actual rows
- C) `Seq Scan`
- D) Many loops

<details><summary>Answer</summary>

**B.**

→ [Basics](basics.en.md)

</details>

## 5. 🟡 `actual rows=40 loops=5`. How many rows did the node return in total?

- A) 40
- B) 45
- C) 200
- D) 8

<details><summary>Answer</summary>

**C.** Values are per loop.

→ [Advanced](advanced.en.md)

</details>

## 6. 🟡 `Sort Method: external merge Disk: 11408kB`. What helps?

- A) `VACUUM FULL`
- B) More `work_mem` for this session, or an index that gives the order
- C) Lower `random_page_cost`
- D) `ANALYZE`

<details><summary>Answer</summary>

**B.**

→ [Advanced](advanced.en.md)

</details>

## 7. 🟡 Two correlated conditions, estimate 201, actual 2000. What fixes the estimate?

- A) `CREATE STATISTICS ... (dependencies)` and `ANALYZE`
- B) A larger `work_mem`
- C) An index on each column
- D) `VACUUM`

<details><summary>Answer</summary>

**A.** Indexes do not change the independence assumption.

→ [Advanced](advanced.en.md)

</details>

## 8. 🟡 Select all true statements about `VACUUM`

- [ ] A) Plain `VACUUM` lets space be reused
- [ ] B) Plain `VACUUM` usually returns space to the OS
- [ ] C) `VACUUM FULL` takes an exclusive lock
- [ ] D) Autovacuum also analyzes tables

<details><summary>Answer</summary>

**A, C, D.**

→ [Advanced](advanced.en.md)

</details>

## 9. 🟡 Why is this page slow?

```sql
SELECT * FROM orders ORDER BY id OFFSET 100000 LIMIT 10;
```

<details><summary>Answer</summary>

`OFFSET` reads and discards 100,000 rows: the plan shows `actual rows=100010`. Use keyset pagination `WHERE id > last ORDER BY id LIMIT 10`.

→ [Advanced](advanced.en.md)

</details>

## 10. 🔴 After five executions a prepared statement suddenly got slower for one value. Likely cause?

- A) A generic plan
- B) Autovacuum
- C) A deadlock
- D) The cache was cleared

<details><summary>Answer</summary>

**A.** With skewed data a generic plan fits some values badly.

→ [Expert](expert.en.md)

</details>

## 11. 🔴 Why does `WHERE code::int = 42` not use the index on `code text`?

- A) The index is invalid
- B) The index stores text values, not their casts
- C) Integers cannot be indexed
- D) Statistics are stale

<details><summary>Answer</summary>

**B.** Compare with `'42'`.

→ [Expert](expert.en.md)

</details>

## 12. 🔴 Open question: users complain that the application got slower over the last month. Your steps?

<details><summary>Key points</summary>

- `pg_stat_statements`: which queries take the most total time, and what changed.
- `EXPLAIN (ANALYZE, BUFFERS)` of those queries on production-like data.
- Estimates against actual rows, spills, filters that discard many rows.
- Table growth, dead rows, autovacuum activity, long transactions.
- One change at a time, measured.

→ [Expert](expert.en.md)

</details>
