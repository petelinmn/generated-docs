---
title: "SQL queries: expert"
lang: en
track: postgresql
kind: module
doc: expert
slug: 02-sql-queries
part: 1
tags: [query-lifecycle, planner, logical-order, null, window-functions]
versions: "PostgreSQL 17+"
status: draft
updated: 2026-11-25
---

# SQL queries: expert

🌐 [Русский](expert.ru.md) · **English**

## Logical order

Conceptually, the query is read in `FROM`, `WHERE`, `GROUP BY`, `HAVING`, `SELECT`, `DISTINCT`, `ORDER BY`, `LIMIT` order. This does not guarantee physical execution order.

## Planner

PostgreSQL analyzes SQL, chooses a strategy, and builds a plan. Physical execution can differ from source text because of indexes, statistics, and join algorithms.

## Traps

- `COUNT(*)` counts rows; `COUNT(column)` ignores NULL.
- Ordinary predicates do not return true for `NULL`.
- `DISTINCT` includes NULL as one value for set comparison.
- `LIMIT` without sorting does not define a stable page.
- A window function executes after logical WHERE and GROUP BY stages.

## Interview questions

<details><summary>1. How does WHERE differ from HAVING?</summary>

WHERE filters rows before grouping; HAVING filters aggregated groups.

</details>

<details><summary>2. Why use DISTINCT ON?</summary>

PostgreSQL removes every row but the first for a key set, according to ordering. Without deterministic ORDER BY, the result is not defined.

</details>

<details><summary>3. Why does COUNT(column) ignore NULL?</summary>

The count aggregate ignores NULL values of the input expression.

</details>

<details><summary>4. When is OFFSET a bad choice?</summary>

For deep pages and frequently changing data, because skipped-row work grows and keys shift.

</details>

<details><summary>5. How does UNION ALL differ from UNION?</summary>

UNION removes duplicates; UNION ALL keeps every row.

</details>

## Sources

- [PostgreSQL SELECT](https://www.postgresql.org/docs/current/sql-select.html)
- [PostgreSQL EXPLAIN](https://www.postgresql.org/docs/current/using-explain.html)
