---
title: "Dapper"
lang: en
track: csharp
kind: library
doc: overview
slug: dapper
part: 2
levels: [1, 2, 3, 4]
tags: [dapper, micro-orm, sql, ado-net]
versions: ".NET 11, C# 15; Dapper 2.1.100, Microsoft.Data.Sqlite 11.0.0"
status: draft
updated: 2026-11-22
---

# Dapper

🌐 [Русский](overview.ru.md) · **English**

## In a nutshell

Dapper is a micro-ORM: extension methods over `IDbConnection`. You write the SQL, Dapper supplies parameters and maps rows to objects. Fast (it caches the generated `IDataReader` parsing), predictable, with no tracking or migrations. It is often used together with EF Core: the model and writes in EF, heavy reports in Dapper.

## Key ideas

- `Query<T>`, `QueryFirst/Single`, `Execute`, `ExecuteScalar`, `QueryMultiple`.
- Parameters as an anonymous object; SQL concatenation opens an injection (verified).
- Mapping by name; `created_at` needs a setting that applies before a type's first use.
- Multi-mapping with `splitOn`; one-to-many is grouped by hand.
- The transaction is passed explicitly.
- No tracking: you write `UPDATE` yourself.

## Minimal example

```csharp
using var cn = new SqliteConnection("Data Source=app.db");
var users = cn.Query<User>("SELECT Id, Name FROM Users WHERE Id > @min", new { min = 1 }).ToList();
```

## When to use and when not

- ✅ Reports, hot paths, complex SQL.
- ✅ An existing DB without an EF model.
- ✅ You need speed and control.
- ❌ A lot of CRUD over a rich model with changes (EF Core is more convenient).
- ❌ You need migrations and tracking out of the box.
- ❌ The team is not ready to maintain SQL by hand.

## Levels of this material

| Level | What's inside |
|---|---|
| [1. Basics](basics.en.md) | queries, parameters, methods, injection |
| [2. Advanced](advanced.en.md) | name mapping, `IN`, multi-mapping, transactions |
| [3. Expert](expert.en.md) | internals, comparison with EF, gotchas, 11 interview questions |
| [4. Self-check](quiz.en.md) | 10 questions and [4 exercises](exercises.en.md) with solutions |
| 5. Runnable examples | not generated yet — `/learn csharp dapper level 5` |
| 6. Tests | not generated yet — `/learn csharp dapper level 6` |

## Related materials

- Prerequisites: [15 · EF Core](../../modules/15-ef-core/overview.en.md)
- Nearby: [Pagination and filtering](../../recipes/pagination-filtering/overview.en.md), [Testcontainers](../testcontainers/overview.en.md), [BenchmarkDotNet](../benchmarkdotnet/overview.en.md)
- In other tracks: [Python](../../../python/overview.en.md) — working with databases in Python
- Recipes and libraries: [Mapperly](../mapperly/overview.en.md)
- [System design: choosing a store](../../../system-design/modules/04-data-storage/overview.en.md)
