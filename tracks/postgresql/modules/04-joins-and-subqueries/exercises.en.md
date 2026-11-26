---
title: "Exercises: joins and subqueries"
lang: en
track: postgresql
kind: module
doc: exercises
slug: 04-joins-and-subqueries
part: 1
tags: [join, subquery, cte, lateral, set-operations]
versions: "PostgreSQL 17+"
status: verified
updated: 2026-11-26
---

# Exercises: joins and subqueries

🌐 [Русский](exercises.ru.md) · **English**

> Solve it yourself before opening the hints. Use the data from [basics](basics.en.md).

## 1. 🟢 All customers with totals

**Task:** list every customer with the number of orders and the total amount; customers without orders show zeros.

<details><summary>Hint</summary>

Which join keeps Eve? What do `count` and `sum` return for a `NULL` row?

</details>

<details><summary>Solution</summary>

```sql
SELECT c.name, count(o.id) AS orders, coalesce(sum(o.amount), 0) AS total
FROM customers c LEFT JOIN orders o ON o.customer_id = c.id
GROUP BY c.id, c.name
ORDER BY c.id;
-- Ann 2 120.00 | Bob 1 20.00 | Eve 0 0
```

`count(o.id)` ignores the `NULL` row, `coalesce` replaces the missing sum.

</details>

## 2. 🟡 The largest order of each customer

**Task:** for every customer return the id and amount of their largest order, keeping customers without orders.

<details><summary>Hint</summary>

A subquery in `FROM` that can see the current customer.

</details>

<details><summary>Solution</summary>

```sql
-- the largest order of every customer
SELECT c.name, top.id, top.amount
FROM customers c
LEFT JOIN LATERAL (
    SELECT id, amount FROM orders o
    WHERE o.customer_id = c.id
    ORDER BY amount DESC LIMIT 1
) AS top ON true
ORDER BY c.id;
-- Ann 11 70.00 | Bob 12 20.00 | Eve NULL NULL
```

An index on `(customer_id, amount DESC)` would let each lookup stop at the first row.

</details>

## 3. 🔴 Above the average order

**Task:** find customers whose total is greater than the average amount of a single order.

<details><summary>Hint</summary>

Two scalar subqueries in `WHERE`. What happens for a customer without orders?

</details>

<details><summary>Solution</summary>

```sql
SELECT name FROM customers c
WHERE (SELECT sum(amount) FROM orders o WHERE o.customer_id = c.id)
    > (SELECT avg(amount) FROM orders);
-- Ann
```

For Eve the sum is `NULL`, the comparison is unknown, and the row is filtered out.

</details>
