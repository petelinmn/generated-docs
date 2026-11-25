---
title: "Data modeling and DDL: expert"
lang: en
track: postgresql
kind: module
doc: expert
slug: 03-data-modeling-ddl
part: 1
tags: [foreign-key, constraints, dependencies, catalog, migrations]
versions: "PostgreSQL 17+"
status: draft
updated: 2026-11-25
---

# Data modeling and DDL: expert

🌐 [Русский](expert.ru.md) · **English**

## Relationship reference

| Relationship | Implementation |
|---|---|
| One-to-one | UNIQUE foreign key on child table |
| One-to-many | Foreign key on child column |
| Many-to-many | Join table with a composite primary key |
| Entity subtype | Separate tables or subtype partitioning |

## Constraints

PostgreSQL stores constraints in the catalog and checks them during DML. Adding a NOT VALID constraint has different behavior for existing rows and needs later VALIDATE.

## Traps

- A composite foreign key must reference a UNIQUE or PRIMARY KEY with matching columns.
- `numeric(p,s)` controls precision and scale for financial data.
- `ON DELETE CASCADE` may remove more data than expected.
- Enum names can be difficult to reorder or remove.
- Exclusion constraints require `EXCLUDE` and an operator with an appropriate index.

## Interview questions

<details><summary>1. When is a composite key needed?</summary>

When a business entity is uniquely identified by a combination of columns.

</details>

<details><summary>2. How does UNIQUE differ from PRIMARY KEY?</summary>

UNIQUE rejects duplicates but ordinary nullable columns may allow NULL. PRIMARY KEY is unique and NOT NULL.

</details>

<details><summary>3. What does NOT VALID do?</summary>

It constrains new inserts or updates without immediately checking every existing row.

</details>

<details><summary>4. Why normalize?</summary>

Normalization removes repeated facts and prevents inconsistent records.

</details>

<details><summary>5. What is dangerous about CASCADE?</summary>

Deleting a parent can remove many child rows without separate SQL statements.

</details>

## Sources

- [PostgreSQL constraints](https://www.postgresql.org/docs/current/ddl-constraints.html)
- [PostgreSQL table constraints](https://www.postgresql.org/docs/current/ddl-constraints.html)
