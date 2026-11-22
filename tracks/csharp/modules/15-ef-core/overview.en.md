---
title: "EF Core"
lang: en
track: csharp
kind: module
doc: overview
slug: 15-ef-core
part: 2
levels: [1, 2, 3, 4, 5, 6]
tags: [ef-core, orm, dbcontext, linq, change-tracking, migrations, n-plus-one, optimistic-concurrency, transactions, query-filters, raw-sql, inheritance-mapping, interceptors]
versions: "EF Core 11.0, .NET 11, C# 15"
status: draft
updated: 2026-11-21
---

# EF Core

🌐 [Русский](overview.ru.md) · **English**

## In a nutshell

Entity Framework Core is Microsoft's ORM: C# classes describe tables, LINQ queries become SQL, and changes to objects are saved with one `SaveChanges` call. EF Core works with SQL Server, PostgreSQL, SQLite and other databases through providers and keeps the schema history as migrations. It removes routine work but does not make SQL go away: for an application to be fast and correct, you need to understand what SQL is generated, what is tracked, and where updates get lost. That is exactly what interviews ask about most.

## Key ideas

- **`DbContext` is a unit of work:** it runs queries, tracks loaded entities and saves changes in a transaction. A context is not thread-safe; in a web application there is one per request.
- **LINQ → SQL:** operators on `IQueryable` run in the database, and SQL is sent only on enumeration. An early `AsEnumerable()` or `ToList()` loads the whole table.
- **Change tracking:** EF compares entities with a snapshot and updates only the changed columns. For reading, `AsNoTracking()`.
- **Related data is loaded explicitly:** `Include` or a projection. Lazy loading in a loop causes the N+1 problem: for 3 blogs, 4 queries instead of 1.
- **Optimistic concurrency:** a token in the `WHERE` turns a lost update into a `DbUpdateConcurrencyException`.
- **Bulk operations** `ExecuteUpdate`/`ExecuteDelete` run a single SQL statement but do not update the tracker.
- **Raw SQL is safe only with parameters:** `FromSql($"…")`; concatenation in `FromSqlRaw` is SQL injection.

## Minimal example

```csharp
// Nothing is sent to the database until the query is enumerated (ToList, First, Count…).
public static List<string> PopularTitles(BloggingContext context, int minLikes) =>
    context.Posts
        .Where(p => p.Likes >= minLikes)
        .OrderByDescending(p => p.Likes)
        .Select(p => p.Title)
        .ToList();
```

EF generates `SELECT "p"."Title" FROM "Posts" … WHERE … "p"."Likes" >= @minLikes ORDER BY "p"."Likes" DESC`: filtering and sorting run in the database, and the value is passed as a parameter. On the test data the result is `["Post 3.2", "Post 3.1", "Post 2.2"]`.

## When to use and when not

- ✅ Business applications: CRUD, object graphs, transactional logic.
- ✅ When the schema evolves along with the code — migrations.
- ✅ Read APIs — with projections, `AsNoTracking` and compiled queries.
- ❌ Bulk loading millions of rows — `SqlBulkCopy`, `COPY`.
- ❌ Complex reports that are easier to write in SQL — `FromSql`/`SqlQuery` or Dapper.
- ❌ Lazy loading in a web API — it hides N+1.

## Levels of this material

| Level | What's inside |
|---|---|
| [1. Basics](basics.en.md) | the model and `DbContext`; LINQ → SQL; `SaveChanges`; entity states; `Include`; migrations |
| [2. Advanced](advanced.en.md) | N+1 and loading strategies; split queries; `AsNoTracking` and identity resolution; client evaluation; `ExecuteUpdate`/`ExecuteDelete`; optimistic concurrency; transactions; global filters; raw SQL; context lifetime |
| [3. Expert](expert.en.md) | the query pipeline and parameters; snapshots and `DetectChanges`; interceptors; lazy loading internals; TPH/TPT/TPC; converters, complex types, shadow properties; compiled queries and pooling; reference, gotchas, 21 interview questions |
| [4. Self-check](quiz.en.md) | quiz and [exercises](exercises.en.md) with solutions |
| [5. Runnable examples](code/) | `code/Examples/` and `code/Solutions/` — solution `Code.slnx`, in-memory SQLite |
| [6. Tests](code/) | `code/Tests/` — `dotnet test` in the `code/` folder |

## Related materials

- Prerequisites: [02 · Types and variables](../02-types-and-variables/overview.en.md); [07 · LINQ](../07-linq/overview.en.md); [10 · Async/await](../10-async-await/overview.en.md)
- `DbContext` and threads: [18 · Concurrency](../18-concurrency/overview.en.md)
- The `dotnet ef` tool: [01 · The .NET platform and dotnet CLI](../01-dotnet-platform/expert.en.md)
- `required`/`init` in entities, primary constructors: [11 · Modern C#](../11-modern-csharp/overview.en.md)
- An API on top of the data: [14 · ASP.NET Core: Minimal API](../14-aspnet-core-minimal-api/overview.en.md)
- Tests with a database: [17 · Testing: integration tests](../17-testing-integration/overview.en.md)
- Repository and aggregates: [21 · Application architecture](../21-architecture/overview.en.md)
- The same in other tracks: [Python](../../../python/overview.en.md) — module 20 "Databases": SQLAlchemy and Alembic
- Recipes and libraries: [Dapper](../../libraries/dapper/overview.en.md), [Mapperly](../../libraries/mapperly/overview.en.md), [Testcontainers](../../libraries/testcontainers/overview.en.md), [Caching](../../recipes/caching/overview.en.md), [Pagination and filtering](../../recipes/pagination-filtering/overview.en.md)
