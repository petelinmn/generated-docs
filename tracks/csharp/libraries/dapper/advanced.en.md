---
title: "Dapper: advanced"
lang: en
track: csharp
kind: library
doc: advanced
slug: dapper
part: 2
tags: [dapper, multi-mapping, transactions, type-mapping, query-multiple]
versions: ".NET 11, C# 15; Dapper 2.1.100, Microsoft.Data.Sqlite 11.0.0"
status: draft
updated: 2026-11-22
---

# Dapper: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [basics](basics.en.md). **Time:** ~45 min.

## Name mapping

Dapper maps columns to properties by name. A column `created_at` will not land in the property `CreatedAt`. Verified: without setup `CreatedAt = null`. Turn on underscore-aware matching:

```csharp
DefaultTypeMap.MatchNamesWithUnderscores = true;
```

> **A gotcha:** the setting applies to types Dapper has **not** mapped yet. Verified: for the already used type `UserCreated` it is still `null` after enabling, while for the new type `UserCreated2` it is `2026-01-01`. Enable the setting once at startup, before the first query.

Alternatives: aliases in SQL (`created_at AS CreatedAt`) or your own `SqlMapper.SetTypeMap`.

## A list in an `IN` condition

```csharp
cn.Query<User>("SELECT Id, Name FROM Users WHERE Id IN @ids", new { ids = new[] { 1, 3 } });
```

Verified: `Ann,Cid` came back. Dapper expands the collection into a list of parameters.

## Related tables (multi-mapping)

```csharp
var joined = cn.Query<Order, User, Order>(
    "SELECT o.Id, o.Total, u.Id, u.Name FROM Orders o JOIN Users u ON u.Id = o.UserId",
    (o, u) => { o.User = u; return o; },
    splitOn: "Id");
```

Verified: `1:Ann,2:Ann,3:Bob`. `splitOn` names the column where the second object starts (`Id` by default). For a one-to-many relationship the result must be grouped by hand (`GroupBy`), otherwise the parent is duplicated.

## Several results in one query

```csharp
using var multi = cn.QueryMultiple("SELECT COUNT(*) FROM Users; SELECT Name FROM Users ORDER BY Id");
var c = multi.ReadSingle<long>(); var names = multi.Read<string>().ToList();
```

Verified: `3 / Ann,Bob,Cid`. This saves round trips to the DB.

## Transactions

```csharp
using var tx = cn.BeginTransaction();
cn.Execute("INSERT INTO Users(Name) VALUES ('Tmp')", transaction: tx);
tx.Rollback();
```

Verified: after `Rollback` the row count stayed 3. The transaction must be passed to every call (`transaction: tx`) — Dapper does not pick it up by itself.

## Stored procedures and dynamic parameters

`cn.Query<T>("sp_name", new { … }, commandType: CommandType.StoredProcedure)`; for output parameters — `DynamicParameters`.

> ⚠️ Verify: stored procedures and `DynamicParameters` with output parameters need a DBMS that supports them (SQLite does not) — not run in this material.

## Asynchrony

Every method has an `…Async` variant: `QueryAsync<T>`, `ExecuteAsync`, `QueryFirstOrDefaultAsync<T>`. Pass a `CommandDefinition` with a `CancellationToken`.

> ⚠️ Verify: the `CommandDefinition` and cancellation signatures were not run in this material.

## Best practices

| Rule | Why |
|---|---|
| Only parameterized queries | security |
| A connection through `using`, short-lived | the connection pool |
| Mapping settings at startup | the type cache |
| An explicit column list instead of `SELECT *` | stability and speed |
| Pass the transaction explicitly | otherwise outside the transaction |
| Async methods in web applications | do not block threads |

## Related materials

- Back: [basics](basics.en.md). Next: [expert](expert.en.md).
- [15 · EF Core](../../modules/15-ef-core/overview.en.md) — comparison with a full ORM.
- [Testcontainers](../testcontainers/overview.en.md) — tests on a real DB.
- [19 · Performance](../../modules/19-performance/overview.en.md) — measuring.

## Sources

- [Dapper README](https://github.com/DapperLib/Dapper#readme)
- [Dapper: multi-mapping](https://github.com/DapperLib/Dapper#multi-mapping)
