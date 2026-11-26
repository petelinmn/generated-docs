---
title: "Quiz: views and functions"
lang: en
track: postgresql
kind: module
doc: quiz
slug: 07-views-functions
part: 2
tags: [view, materialized-view, plpgsql, stored-procedure, trigger]
versions: "PostgreSQL 17+"
status: draft
updated: 2026-11-26
---

# Quiz: views and functions

🌐 [Русский](quiz.ru.md) · **English**

> Answer first, then open the answer. The mark shows the level: 🟢 [basics](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 What does a plain view store?

- A) A copy of the rows
- B) The query
- C) An index
- D) A cached result for one minute

<details><summary>Answer</summary>

**B.** Rows are read from the tables on every use.

→ [Basics](basics.en.md)

</details>

## 2. 🟢 How is a table function used?

- A) In `FROM`
- B) Only with `CALL`
- C) Only in triggers
- D) In `CREATE INDEX`

<details><summary>Answer</summary>

**A.**

→ [Basics](basics.en.md)

</details>

## 3. 🟢 What does this print?

```sql
SELECT price_with_tax(10, rate => 0.1);
```

<details><summary>Answer</summary>

```text
11.00
```

The named argument replaces the default.

→ [Basics](basics.en.md)

</details>

## 4. 🟢 Why use `$$` around a function body?

- A) It makes it faster
- B) To write quotes inside without escaping
- C) It marks it immutable
- D) It is required for views

<details><summary>Answer</summary>

**B.**

→ [Basics](basics.en.md)

</details>

## 5. 🟡 A view filters `WHERE active` and has no check option. What happens on inserting an inactive row through it?

- A) Error
- B) The row is inserted but not visible in the view
- C) Nothing is inserted
- D) The row becomes active

<details><summary>Answer</summary>

**B.** `WITH CHECK OPTION` would make it an error.

→ [Advanced](advanced.en.md)

</details>

## 6. 🟡 What does `REFRESH MATERIALIZED VIEW CONCURRENTLY` require?

- A) A trigger
- B) A unique index on plain columns covering all rows
- C) A primary key on the base table
- D) Superuser

<details><summary>Answer</summary>

**B.**

→ [Advanced](advanced.en.md)

</details>

## 7. 🟡 What does a `BEFORE INSERT` row trigger returning `NULL` do?

- A) Inserts a row of nulls
- B) Silently skips the row
- C) Raises an error
- D) Nothing special

<details><summary>Answer</summary>

**B.**

→ [Advanced](advanced.en.md)

</details>

## 8. 🟡 Select all true statements

- [ ] A) A statement trigger fires even if no rows changed
- [ ] B) The return value of an `AFTER` row trigger is ignored
- [ ] C) A function can `COMMIT` in its body
- [ ] D) `INSTEAD OF` triggers are for views

<details><summary>Answer</summary>

**A, B, D.** Transaction control is for procedures, not functions.

→ [Advanced](advanced.en.md)

</details>

## 9. 🟡 Find the vulnerability

```sql
EXECUTE 'SELECT count(*) FROM ' || tbl;
```

<details><summary>Answer</summary>

SQL injection through `tbl`. Use `format('... %I', tbl)`.

→ [Advanced](advanced.en.md)

</details>

## 10. 🔴 Rights are checked against whom for a default view?

- A) The querying user
- B) The view owner
- C) The superuser
- D) Nobody

<details><summary>Answer</summary>

**B.** `security_invoker = true` switches to the caller.

→ [Expert](expert.en.md)

</details>

## 11. 🔴 Which volatility is the default?

- A) `IMMUTABLE`
- B) `STABLE`
- C) `VOLATILE`
- D) None

<details><summary>Answer</summary>

**C.**

→ [Expert](expert.en.md)

</details>

## 12. 🔴 Open question: when would you refuse to put a rule into a trigger?

<details><summary>Key points</summary>

- It calls external services or must happen after commit.
- A constraint expresses it.
- It makes bulk changes too slow or behaviour surprising.
- The team cannot test and version it.

→ [Expert](expert.en.md)

</details>
