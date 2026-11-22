---
title: "Quiz: Pagination and filtering"
lang: en
track: csharp
kind: recipe
doc: quiz
slug: pagination-filtering
part: 2
tags: [pagination, keyset-pagination, ef-core, filtering]
status: draft
updated: 2026-11-22
---

# Quiz: Pagination and filtering

🌐 [Русский](quiz.ru.md) · **English**

> Answer first, then open the answer. The mark shows which level a question comes from: 🟢 [minimal](basics.en.md) · 🟡 [production](advanced.en.md) · 🔴 [scale and edges](expert.en.md).

## 1. 🟢 What is mandatory before `Skip/Take`?

- A) `ToList()`
- B) an explicit `OrderBy`
- C) `AsNoTracking()`
- D) `Include`

<details><summary>Answer</summary>

**B.** Without an explicit sort the row order is not guaranteed, pages may overlap or lose records.
→ [Minimal](basics.en.md#offset-pagination)

</details>

## 2. 🟢 Which page does `Skip((2-1)*10).Take(10)` return on records `P01…P25` sorted by `Id`?

- A) `P01…P10`
- B) `P11…P20`
- C) `P21…P25`
- D) `P10…P19`

<details><summary>Answer</summary>

**B.** Verified: page 2 — `P11…P20`.
→ [Minimal](basics.en.md#offset-pagination)

</details>

## 3. 🟢 Where should pagination happen?

- A) in memory after `ToList()`
- B) in the SQL query (before `ToListAsync`)
- C) on the client
- D) it does not matter

<details><summary>Answer</summary>

**B.** `ToList().Skip()` loads everything from the DB; `Skip/Take` on an `IQueryable` becomes `LIMIT/OFFSET`.
→ [Minimal](basics.en.md#beginner-mistakes)

</details>

## 4. 🟢 What does the keyset condition for the page "after Id = 10" look like?

- A) `Skip(10)`
- B) `Where(p => p.Id > 10).OrderBy(p => p.Id).Take(…)`
- C) `Take(10)`
- D) `Last(10)`

<details><summary>Answer</summary>

**B.** Verified: the SQL `WHERE "p"."Id" > 10 ORDER BY "p"."Id" LIMIT @p`.
→ [Minimal](basics.en.md#keyset-pagination)

</details>

## 5. 🟡 What problem does offset have with inserts between page requests?

- A) none
- B) duplicates and gaps at page boundaries
- C) a DB crash
- D) an EF exception

<details><summary>Answer</summary>

**B.** Verified: after the insert `P11` was on both page 1 and page 2; keyset gave no duplicates.
→ [Production](advanced.en.md#why-offset-breaks)

</details>

## 6. 🟡 Why do you add `Id` to the sort and the condition in keyset by price?

- A) for beauty
- B) a tie-breaker: otherwise records with an equal price are skipped
- C) for the cache
- D) to shorten the SQL

<details><summary>Answer</summary>

**B.** Verified: `Price > 20` skipped records with price 20 (returned `P08` first instead of `P06`).
→ [Production](advanced.en.md#a-composite-sort-key)

</details>

## 7. 🟡 Select all correct statements

- [ ] A) `size + 1` lets you learn about a next page without `COUNT`
- [ ] B) a client's page size should be limited
- [ ] C) a Base64 cursor is encryption
- [ ] D) `^1` is allowed inside an EF query lambda

<details><summary>Answer</summary>

**A, B.** C is wrong: Base64 only encodes; D is wrong: expression trees forbid the from-end index (CS8790/CS8791).
→ [Production](advanced.en.md#an-api-with-a-cursor)

</details>

## 8. 🟡 Open question: when do you choose offset and when keyset?

<details><summary>Key points of a good answer</summary>

- offset — admin panels, small sets, a jump to page N is needed;
- keyset — feeds, infinite scroll, large and changing tables;
- keyset needs an index and a tie-breaker, no arbitrary jumps;
- limit the page size, settle the total question.

</details>

## 9. 🔴 Why does offset degrade on a million-row table?

- A) EF Core is slow
- B) the DB must walk through and discard all skipped rows
- C) because of Base64
- D) because of `AsNoTracking`

<details><summary>Answer</summary>

**B.** Measured: at position 900,000 offset is 15 ms, keyset 0 ms (SQLite, 1,000,000 rows).
→ [Production](advanced.en.md#why-offset-breaks)

</details>

## 10. 🔴 A client passes the page-2 cursor together with a different filter. What is wrong?

- A) everything is fine
- B) a cursor is a position in a specific sort; with another filter/sort the result is meaningless
- C) the server crashes
- D) EF resets the filter

<details><summary>Answer</summary>

**B.** Include the filters in the cursor or check they match.
→ [Scale and edges](expert.en.md#edges-and-gotchas)

</details>
