---
title: "Quiz: joins and subqueries"
lang: en
track: postgresql
kind: module
doc: quiz
slug: 04-joins-and-subqueries
part: 1
tags: [join, subquery, cte, lateral, set-operations]
versions: "PostgreSQL 17+"
status: draft
updated: 2026-11-26
---

# Quiz: joins and subqueries

🌐 [Русский](quiz.ru.md) · **English**

> Answer first, then open the answer. The mark shows the level: 🟢 [basics](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 What stays in the result of a `LEFT JOIN`?

- A) Only matching pairs
- B) Pairs and every left row without a pair
- C) Every row of both tables
- D) Only left rows without a pair

<details><summary>Answer</summary>

**B.** A is an inner join, C is a full join, D is an anti-join.

→ [Basics](basics.en.md)

</details>

## 2. 🟢 A `CROSS JOIN` of 3 rows and 4 rows gives

- A) 4
- B) 7
- C) 12
- D) 3

<details><summary>Answer</summary>

**C.** Every row is paired with every row.

→ [Basics](basics.en.md)

</details>

## 3. 🟢 Which query lists each customer who has orders exactly once?

- A) An inner join with `orders`
- B) `WHERE EXISTS (...)`
- C) `CROSS JOIN orders`
- D) `FULL JOIN orders`

<details><summary>Answer</summary>

**B.** The join repeats a customer per order; C and D also return extra rows.

→ [Basics](basics.en.md)

</details>

## 4. 🟢 Why is a self join written with aliases?

- A) For speed
- B) To tell the two roles of the table apart
- C) It enables an index
- D) It removes duplicates

<details><summary>Answer</summary>

**B.** Without aliases column references would be ambiguous.

→ [Basics](basics.en.md)

</details>

## 5. 🟡 Using the module data, how many rows does this return?

```sql
SELECT c.name FROM customers c
LEFT JOIN orders o ON o.customer_id = c.id AND o.amount > 60;
```

<details><summary>Answer</summary>

```text
3
```

All three customers stay; `ON` only limits which orders match.

→ [Advanced](advanced.en.md)

</details>

## 6. 🟡 What does this return on the module data?

```sql
SELECT count(*) FROM customers
WHERE id NOT IN (SELECT customer_id FROM orders);
```

<details><summary>Answer</summary>

```text
0
```

Order 13 contributes a `NULL`, so the condition is never true.

→ [Advanced](advanced.en.md)

</details>

## 7. 🟡 Select all true statements

- [ ] A) `UNION` removes duplicates
- [ ] B) `UNION ALL` keeps duplicates
- [ ] C) `EXCEPT` returns rows present in both queries
- [ ] D) `INTERSECT` returns rows present in both queries

<details><summary>Answer</summary>

**A, B, D.** `EXCEPT` returns rows of the first query that are absent from the second.

→ [Advanced](advanced.en.md)

</details>

## 8. 🟡 What does `LATERAL` allow?

- A) Running the query in parallel
- B) A subquery in `FROM` referring to earlier `FROM` items
- C) Joining without a condition
- D) Skipping `NULL` values

<details><summary>Answer</summary>

**B.** Parallelism is a separate planner feature.

→ [Advanced](advanced.en.md)

</details>

## 9. 🟡 Find the bug: "customers with their number of orders"

```sql
SELECT c.name, count(*) FROM customers c
LEFT JOIN orders o ON o.customer_id = c.id
GROUP BY c.id, c.name;
```

<details><summary>Answer</summary>

`count(*)` counts the `NULL` row, so Eve gets 1 instead of 0. Use `count(o.id)`.

→ [Advanced](advanced.en.md)

</details>

## 10. 🔴 Which join algorithm requires an equality condition?

- A) Nested loop
- B) Hash join
- C) All of them
- D) None

<details><summary>Answer</summary>

**B.** A hash table can only be probed by equality; a nested loop works with any condition.

→ [Expert](expert.en.md)

</details>

## 11. 🔴 What does this print?

```sql
SELECT 5 > ALL (SELECT amount FROM orders WHERE false);
```

<details><summary>Answer</summary>

```text
t
```

`ALL` over an empty set is true.

→ [Expert](expert.en.md)

</details>

## 12. 🔴 Open question: a report sums payments and shipments of a customer in one query and the totals are too large. Explain and fix.

<details><summary>Key points</summary>

- Two one-to-many relations multiply rows.
- Aggregate each in its own subquery or CTE, then join one row per customer.
- `DISTINCT` inside `sum` is not a fix: equal amounts would be lost.

→ [Expert](expert.en.md)

</details>
