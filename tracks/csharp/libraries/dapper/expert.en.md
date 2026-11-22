---
title: "Dapper: expert"
lang: en
track: csharp
kind: library
doc: expert
slug: dapper
part: 2
tags: [dapper, ef-core-comparison, il-emit, caching, interview-questions]
versions: ".NET 11, C# 15; Dapper 2.1.100"
status: draft
updated: 2026-11-22
---

# Dapper: expert

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · **3. Expert** · [4. Quiz](quiz.en.md)

**Prerequisites:** [basics](basics.en.md) and [advanced](advanced.en.md). **Time:** ~30 min.

## Internals

On the first execution of a query Dapper generates, through IL emit (`System.Reflection.Emit`), a method that parses an `IDataReader` into an object and caches it by the query's "identity" (the SQL, the parameter type, the result type, the connection string). Repeated calls with the same SQL use the ready deserializer — so Dapper is close in speed to hand-written code over `DbDataReader`.

A consequence that was verified: name-mapping settings affect only types that are not yet cached ([advanced](advanced.en.md#name-mapping)).

> ⚠️ Verify: the cache details (size, eviction) and IL-emit behavior under Native AOT were not investigated in this material — for AOT see the Dapper documentation (`Dapper.AOT`).

## Dapper or EF Core

| | Dapper | EF Core |
|---|---|---|
| SQL | by hand | generated from LINQ |
| Tracking and `SaveChanges` | no | yes |
| Migrations | no | yes |
| Speed on simple queries | close to ADO.NET | slightly lower, depends on settings |
| Complex reports, specific SQL | convenient | possible through raw SQL |
| Changing the DBMS | rewrite SQL | mostly the provider |

They go well together: writes and the model in EF Core, heavy reports in Dapper on the same connection.

## Edges and gotchas

**1. No tracking.** You changed an object — nothing went to the DB until you run `UPDATE` yourself.

**2. The name-mapping cache.** Verified: `MatchNamesWithUnderscores` does not affect a type after its first use.

**3. `QuerySingle` is strict.** Verified: 0 rows or more than one — `InvalidOperationException`; `QueryFirstOrDefault` on 0 rows gives `null`.

**4. SQL concatenation.** Verified: the injection `x' OR '1'='1` returned all rows; a parameter — 0.

**5. Multi-mapping and `splitOn`.** A wrong `splitOn` gives empty nested objects without an error.

**6. One-to-many.** Without grouping the parent repeats for each child row.

**7. The transaction is not implicit.** If you do not pass `transaction:`, the command runs outside it (an exception for some providers).

**8. Buffering.** `Query<T>` buffers the result by default; for big selections — `buffered: false` and stream processing with the connection open.

**9. `NULL` values and value types.** `NULL` into an `int` (not `int?`) causes a cast error — declare nullable.

## Trade-offs and alternatives

| Approach | Pros | Cons |
|---|---|---|
| Dapper | speed, control, simplicity | SQL by hand, no migrations |
| EF Core | productivity, migrations, tracking | the abstraction leaks, overhead |
| Plain ADO.NET | maximum control | a lot of boilerplate |
| RepoDB, linq2db | other micro-ORMs/providers | smaller community |
| Source-generated mapping (Dapper.AOT) | AOT compatibility | separate setup |

## Interview questions

<details><summary>1. What is Dapper and how does it differ from EF Core?</summary>

A micro-ORM: extension methods over `IDbConnection`, the SQL is written by hand, mapping into objects is automatic; no tracking, migrations or LINQ.

</details>

<details><summary>2. Why is Dapper fast?</summary>

It generates and caches the code that parses an `IDataReader` into an object (IL emit) and adds no tracking layers.

</details>

<details><summary>3. How do you protect against SQL injection in Dapper?</summary>

Parameters (`@name` and an anonymous object). Verified: a parameter — 0 rows, concatenation — all rows.

</details>

<details><summary>4. How do you map `created_at` to the property `CreatedAt`?</summary>

`DefaultTypeMap.MatchNamesWithUnderscores = true` before the type is first used, an alias in SQL, or your own type map.

</details>

<details><summary>5. How do you load related entities?</summary>

Multi-mapping: `Query<A, B, A>(sql, map, splitOn: …)`; for one-to-many — grouping.

</details>

<details><summary>6. What is the difference between `QueryFirst` and `QuerySingle`?</summary>

`First` takes the first row, `Single` requires exactly one (verified: otherwise `InvalidOperationException`).

</details>

<details><summary>7. How do you work with transactions?</summary>

`BeginTransaction()` and passing `transaction:` to every call; `Commit/Rollback` (verified: after `Rollback` the row count did not grow).

</details>

<details><summary>8. How do you pass a list into `IN`?</summary>

A collection parameter: `WHERE Id IN @ids` (verified).

</details>

<details><summary>9. Trick question: you enabled `MatchNamesWithUnderscores` but `CreatedAt` is still `null`. Why?</summary>

The type had already been mapped and cached before the setting was enabled (verified); enable it at application startup.

</details>

<details><summary>10. Trick question: a query returned the parent several times in multi-mapping. Why?</summary>

A one-to-many relationship in a JOIN gives a row per child; you must group by the parent's key.

</details>

<details><summary>11. When do you choose Dapper and when EF Core?</summary>

Dapper — reports, hot paths, complex SQL, an existing DB; EF Core — CRUD over a model, migrations, tracking. Often — together.

</details>

## Related materials

- Back: [advanced](advanced.en.md). Check yourself: [quiz](quiz.en.md) and [exercises](exercises.en.md).
- [15 · EF Core](../../modules/15-ef-core/overview.en.md)
- [20 · Reflection and source generators](../../modules/20-reflection-and-source-generators/overview.en.md) — IL emit and generators.
- [BenchmarkDotNet](../benchmarkdotnet/overview.en.md) — how to measure.

## Sources

- [Dapper README](https://github.com/DapperLib/Dapper#readme)
- [Dapper.AOT](https://github.com/DapperLib/DapperAOT)
