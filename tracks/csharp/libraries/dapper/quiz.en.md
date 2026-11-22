---
title: "Quiz: Dapper"
lang: en
track: csharp
kind: library
doc: quiz
slug: dapper
part: 2
tags: [dapper, micro-orm, sql]
status: draft
updated: 2026-11-22
---

# Quiz: Dapper

🌐 [Русский](quiz.ru.md) · **English**

> Answer first, then open the answer. The mark: 🟢 [basics](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 What is Dapper?

- A) a full ORM with tracking
- B) a micro-ORM: extension methods over `IDbConnection`, SQL is written by hand
- C) a SQL generator from LINQ
- D) a DB driver

<details><summary>Answer</summary>

**B.** Dapper supplies parameters and maps rows to objects; no tracking or migrations.
→ [Basics](basics.en.md#what-it-is-and-why)

</details>

## 2. 🟢 What does `Execute` return for an INSERT of an array of 3 objects?

- A) `0`
- B) `3`
- C) `1`
- D) identifiers

<details><summary>Answer</summary>

**B.** Verified: `rows inserted: 3`.
→ [Basics](basics.en.md#queries-and-commands)

</details>

## 3. 🟢 What does `QueryFirstOrDefault` return when the row is missing?

- A) an exception
- B) `null`
- C) an empty object
- D) `0`

<details><summary>Answer</summary>

**B.** Verified; `QuerySingle` throws `InvalidOperationException`.
→ [Basics](basics.en.md#queries-and-commands)

</details>

## 4. 🟢 How do you safely put user input into a query?

- A) by concatenation
- B) as a parameter (`@n` + `new { n = … }`)
- C) through `string.Format`
- D) escaping quotes by hand

<details><summary>Answer</summary>

**B.** Verified: a parameter — 0 rows for `x' OR '1'='1`, concatenation — all 3.
→ [Basics](basics.en.md#parameters-protection-from-sql-injection)

</details>

## 5. 🟡 Why did `created_at` not land in `CreatedAt`?

- A) a bug
- B) Dapper maps by name; a setting (`MatchNamesWithUnderscores`) or an alias is needed
- C) `Include` is needed
- D) the `string` type is not supported

<details><summary>Answer</summary>

**B.** Verified: without setup `null`.
→ [Advanced](advanced.en.md#name-mapping)

</details>

## 6. 🟡 You enabled `MatchNamesWithUnderscores` after the first query — the result for the already used type?

- A) it starts working
- B) stays `null`: the setting applies to types not yet mapped
- C) an exception
- D) works after a DB restart

<details><summary>Answer</summary>

**B.** Verified; for a new type the value was picked up.
→ [Advanced](advanced.en.md#name-mapping)

</details>

## 7. 🟡 Select all correct statements

- [ ] A) the transaction must be passed to every call via `transaction:`
- [ ] B) `splitOn` says where the second object starts in multi-mapping
- [ ] C) Dapper tracks object changes itself
- [ ] D) `IN @ids` accepts a collection

<details><summary>Answer</summary>

**A, B, D.** C is wrong: there is no tracking.
→ [Advanced](advanced.en.md#transactions)

</details>

## 8. 🟡 Open question: load orders with their user in one query. What do you consider?

<details><summary>Key points of a good answer</summary>

- `Query<Order, User, Order>` with `splitOn`;
- for one-to-many — grouping by the parent;
- an explicit column list; parameters;
- indexes on the related columns.

</details>

## 9. 🔴 Why is Dapper fast?

- A) it uses a Redis cache
- B) it generates and caches the `IDataReader` parsing code (IL emit), with no tracking layers
- C) it does not hit the DB
- D) it uses reflection on every call

<details><summary>Answer</summary>

**B.** The deserializer is cached by the query's identity.
→ [Expert](expert.en.md#internals)

</details>

## 10. 🔴 The parent repeats in a multi-mapping result. What to do?

- A) `Distinct()` on the whole object
- B) group by the parent's key and collect the children
- C) remove `splitOn`
- D) use `QuerySingle`

<details><summary>Answer</summary>

**B.** A JOIN gives a row per child.
→ [Expert](expert.en.md#edges-and-gotchas)

</details>
