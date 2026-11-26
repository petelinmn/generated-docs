---
title: "Exercises: indexes"
lang: en
track: postgresql
kind: module
doc: exercises
slug: 05-indexes
part: 2
tags: [index, b-tree, gin, brin, query-plan]
versions: "PostgreSQL 17+"
status: draft
updated: 2026-11-26
---

# Exercises: indexes

🌐 [Русский](exercises.ru.md) · **English**

> Solve it yourself before opening the hints. Use the table from [basics](basics.en.md) and check every answer with `EXPLAIN`.

## 1. 🟢 The latest events of a user

**Task:** make "the ten latest events of one user" run without a sort step.

<details><summary>Hint</summary>

Which column is compared for equality, and which one orders the result?

</details>

<details><summary>Solution</summary>

```sql
CREATE INDEX events_user_created ON events (user_id, created_at DESC);
SELECT * FROM events WHERE user_id = 42 ORDER BY created_at DESC LIMIT 10;
```

An ascending index works too, read backwards.

</details>

## 2. 🟡 A queue of failures

**Task:** failed events are 1% of the table and are processed oldest first. Make the index as small as possible.

<details><summary>Hint</summary>

The index does not have to contain every row.

</details>

<details><summary>Solution</summary>

```sql
CREATE INDEX events_failed ON events (created_at) WHERE status = 'failed';
```

The query must repeat `status = 'failed'`.

</details>

## 3. 🔴 Case-insensitive login

**Task:** find an event by email regardless of letter case, using an index.

<details><summary>Hint</summary>

What exactly is compared in `WHERE`?

</details>

<details><summary>Solution</summary>

```sql
CREATE INDEX events_lower_email ON events (lower(email));
ANALYZE events;
EXPLAIN (COSTS OFF) SELECT id FROM events WHERE lower(email) = 'user7@example.com';
```

The expression in the query must match the expression in the index. A `UNIQUE` index on the same expression would also forbid addresses differing only in case.

</details>
