---
title: "Dapper: basics"
lang: en
track: csharp
kind: library
doc: basics
slug: dapper
part: 2
tags: [dapper, micro-orm, sql, ado-net, sqlite]
versions: ".NET 11, C# 15; Dapper 2.1.100, Microsoft.Data.Sqlite 11.0.0"
status: draft
updated: 2026-11-22
---

# Dapper: basics

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [15 · EF Core](../../modules/15-ef-core/overview.en.md) (for comparison), knowing SQL. **Time:** ~30 min.

## What it is and why

Dapper is a micro-ORM: a set of extension methods over `IDbConnection`. You write the SQL yourself, Dapper supplies the parameters and turns result rows into objects. There is no change tracking, no migrations and no LINQ provider — just fast, predictable mapping. Choose it when you need full control over SQL or maximum speed.

```bash
dotnet add package Dapper
dotnet add package Microsoft.Data.Sqlite   # any ADO.NET provider works
```

## Queries and commands

```csharp
using var cn = new SqliteConnection("Data Source=:memory:");
cn.Open();
cn.Execute("CREATE TABLE Users(Id INTEGER PRIMARY KEY, Name TEXT, Email TEXT)");

var n = cn.Execute("INSERT INTO Users(Name, Email) VALUES (@Name, @Email)",
    new[] { new { Name = "Ann", Email = "a@x" }, new { Name = "Bob", Email = "b@x" }, new { Name = "Cid", Email = (string?)null } });

var users = cn.Query<User>("SELECT Id, Name, Email FROM Users WHERE Id > @min ORDER BY Id", new { min = 1 }).ToList();
var count = cn.ExecuteScalar<long>("SELECT COUNT(*) FROM Users");
```

Verified by running: `Execute` with an array of parameters inserted 3 rows (`rows inserted: 3`); the query returned `2:Bob:b@x,3:Cid:null`; `COUNT` → `3`. The object's properties (`@Name`) become the command's parameters. A DB `NULL` becomes `null` for `string?`.

| Method | Purpose |
|---|---|
| `Query<T>` | a list of objects |
| `QueryFirst/QueryFirstOrDefault<T>` | the first row (an exception/`null` if none) |
| `QuerySingle/QuerySingleOrDefault<T>` | exactly one row |
| `Execute` | INSERT/UPDATE/DELETE, returns the number of affected rows |
| `ExecuteScalar<T>` | a single value |
| `QueryMultiple` | several result sets |

Verified: `QueryFirstOrDefault` on a missing row — `null`; `QuerySingle` on 0 rows and on several rows — `InvalidOperationException`.

## Parameters: protection from SQL injection

Always pass values as parameters, not by concatenating strings. Verified with the value `x' OR '1'='1`: with a parameter the query returned 0 rows, with concatenation — all 3 (the injection worked).

```csharp
cn.Query<User>("SELECT * FROM Users WHERE Name = @n", new { n = userInput });   // safe
```

## Beginner mistakes

- ❌ Building SQL by concatenation → ✅ parameters (`@name`).
- ❌ Expecting `created_at` to land in `CreatedAt` by itself → ✅ mapping setup is needed ([advanced](advanced.en.md#name-mapping)).
- ❌ Using `Query<T>` and forgetting `ToList()` when the connection is used afterwards → ✅ materialize the result before closing the connection.
- ❌ Expecting tracking/auto-update as in EF → ✅ you write changes as SQL commands.
- ❌ Opening a connection and not closing it → ✅ `using`.

## Related materials

- Next: [advanced](advanced.en.md) — name mapping, `IN`, joins, transactions.
- [15 · EF Core](../../modules/15-ef-core/overview.en.md) — a full ORM; when to pick which.
- [Pagination and filtering](../../recipes/pagination-filtering/overview.en.md) — queries with `LIMIT/OFFSET`.
- [Python](../../../python/overview.en.md) — working with databases in Python.

## Sources

- [Dapper on GitHub](https://github.com/DapperLib/Dapper)
- [Dapper: parameters and queries (README)](https://github.com/DapperLib/Dapper#readme)
- [Microsoft.Data.Sqlite](https://learn.microsoft.com/dotnet/standard/data/sqlite/)
