---
title: "EF Core: advanced"
lang: en
track: csharp
kind: module
doc: advanced
slug: 15-ef-core
part: 2
tags: [ef-core, n-plus-one, eager-loading, lazy-loading, split-query, no-tracking, optimistic-concurrency, transactions, query-filters, raw-sql, sql-injection]
versions: "EF Core 11.0, .NET 11, C# 15"
status: draft
updated: 2026-11-21
---

# EF Core: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md) · [exercises](exercises.en.md) · [5–6. Code and tests](code/)

## Loading related data and the N+1 problem

Test data: 3 blogs, 2 posts each. We need to count all posts. The `CommandLog` interceptor counts SQL commands.

| Approach | Code | SQL commands |
|---|---|---|
| Lazy loading | `blog.Posts.Count` in a loop | **4** (1 + 3) |
| Eager loading | `Include(b => b.Posts)` | 1 |
| Projection | `Select(b => new { b.Name, Posts = b.Posts.Count })` | 1 |
| Explicit loading | `Entry(blog).Collection(b => b.Posts).Load()` | one per call |

**N+1** means one query for a list plus one more per element. With lazy loading it appears unnoticed:

```csharp
// N+1: one query for blogs, then one more query per blog when Posts is touched.
public static (int TotalPosts, int Commands) LazyLoading(TestDatabase db)
{
    var log = new CommandLog();
    using BloggingContext context = db.CreateLazyContext(log);
    int total = 0;
    foreach (Blog blog in context.Blogs.ToList())
    {
        total += blog.Posts.Count; // each access to Posts runs a separate SELECT
    }
    return (total, log.Count);
}
```

Lazy loading is off by default. It needs the `Microsoft.EntityFrameworkCore.Proxies` package, a `UseLazyLoadingProxies()` call and `virtual` navigations. For 3 blogs this is 4 queries; for 1000 blogs, 1001.

The cure is to request everything at once. A **projection** is usually the best option: the database returns only the needed data, and there are no entities to track:

```csharp
var rows = context.Blogs
    .OrderBy(b => b.Name)
    .Select(b => new { b.Name, Posts = b.Posts.Count })
    .AsEnumerable()
    .Select(x => (x.Name, x.Posts))
    .ToList();
```

The result `[("Blog 1", 2), ("Blog 2", 2), ("Blog 3", 2)]` takes 1 SQL command with a `COUNT(*)` subquery.

Two more tools:

- **Filtered `Include`**: `Include(b => b.Posts.Where(p => p.Likes >= 20))` loads only part of a collection. For the three blogs it gave `[0, 2, 2]` posts.
- **Explicit loading**: `context.Entry(blog).Collection(b => b.Posts).Load()` loads a navigation on demand. `IsLoaded` is `false` before the call and `true` after.

Code: [Ex04_Loading.cs](code/Examples/Ex04_Loading.cs).

### Split queries and cartesian explosion

When one query loads several collections, the database returns a cartesian product: every row of one collection is joined with every row of the other. By default EF Core uses a single query (`SingleQuery`) and warns about it:

```text
warn: RelationalEventId.MultipleCollectionIncludeWarning[20504] (Microsoft.EntityFrameworkCore.Query)
      Compiling a query which loads related collections for more than one collection navigation, either via 'Include' or through projection, but no 'QuerySplittingBehavior' has been configured. By default, Entity Framework will use 'QuerySplittingBehavior.SingleQuery', which can potentially result in slow query performance.
```

`AsSplitQuery()` splits loading into separate queries. A query with `Include(p => p.Tags)` and `Include(p => p.Blog).ThenInclude(b => b.Posts)` ran as 1 command in single mode and as 3 commands in split mode. The global setting is `UseSqlite(…, o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery))`. The price of split queries: several database round trips and no consistency guarantee between them if data changes concurrently.

## Tracking: AsNoTracking and identity resolution

| Mode | Tracking | One object per row | When |
|---|---|---|---|
| default | yes | yes | the data will be changed and saved |
| `AsNoTracking()` | no | **no** | reading, maximum speed |
| `AsNoTrackingWithIdentityResolution()` | no | yes | reading a graph with repeated objects |

Six posts with `Include(p => p.Blog)`: with `AsNoTracking` there were **6** distinct `Blog` objects (one per post), with identity resolution **3**.

**Disconnected entities.** The object came from an API and was not loaded into this context. `Update(entity)` marks **all** properties as modified, and EF updates all columns:

```text
UPDATE "Blogs" SET "IsDeleted" = @p0, "Name" = @p1, "Version" = @p2
WHERE "Id" = @p3 AND "Version" = @p4
RETURNING 1;
```

`Attach(entity)` attaches the object in the `Unchanged` state. After a property changes, only that property is marked modified — verified: `Name` has `IsModified = True`, `Version` has `False`.

## What EF can translate to SQL

EF translates expressions it knows how to translate: operators, many `string`, `Math` and `DateTime` methods, aggregates. An arbitrary C# method cannot be translated. The rule: **client evaluation is allowed only in the final `Select`**:

```csharp
// Allowed: the untranslatable call is in the top-level Select, EF runs it on the loaded rows.
public static List<string> ClientEvalInProjection(BloggingContext context) =>
    context.Posts.OrderBy(p => p.Id).Select(p => Shorten(p.Title)).ToList();

// Not allowed: the same call in Where cannot be executed by the database, EF throws.
public static List<Post> ClientEvalInWhere(BloggingContext context) =>
    context.Posts.Where(p => Shorten(p.Title).EndsWith("…")).ToList();
```

The first call returns `"Post 1…"` and other shortened titles. The second throws `InvalidOperationException`: "The LINQ expression … could not be translated… Either rewrite the query in a form that can be translated, or switch to client evaluation explicitly by inserting a call to 'AsEnumerable', 'AsAsyncEnumerable', 'ToList', or 'ToListAsync'".

### IQueryable and IEnumerable

- `IQueryable<T>` is a description of a query. `Where` becomes part of the SQL: `WHERE … "p"."Likes" > 30`.
- `IEnumerable<T>` is LINQ to Objects. After `AsEnumerable()` the filter runs in memory: in the example, to find **1** post, **all 6** were read from the database.

The mistake "called `AsEnumerable()` or `ToList()` too early" turns a light query into a dump of the whole table. Code: [Ex05_Translation.cs](code/Examples/Ex05_Translation.cs).

## ExecuteUpdate and ExecuteDelete

The classic path "load → change → `SaveChanges`" runs one `SELECT` and one `UPDATE` per entity. For bulk changes there are operations that run a single SQL statement right away:

```csharp
// UPDATE ... SET Likes = Likes + 1 WHERE ...: runs immediately, without SaveChanges.
public static (int Affected, int Commands, string Sql) LikeAllPostsOfBlog(BloggingContext context, CommandLog log, int blogId)
{
    log.Clear();
    int affected = context.Posts
        .Where(p => p.BlogId == blogId)
        .ExecuteUpdate(setters => setters.SetProperty(p => p.Likes, p => p.Likes + 1));
    return (affected, log.Count, log.Commands.Single());
}
```

The result is 2 rows in 1 command `UPDATE "Posts" AS "p" SET "Likes" = "p"."Likes" + 1 …`. Important properties:

- they **do not use the change tracker**: an already loaded object stays stale. Verified: the tracked post shows `Likes = 10`, the database has `1000`;
- they run **immediately**, without `SaveChanges`;
- they do not open a transaction themselves.

`ExecuteDelete()` deletes rows by a condition and returns their count. Code: [Ex06_BulkOperations.cs](code/Examples/Ex06_BulkOperations.cs).

## Optimistic concurrency

Two users loaded the same blog and both change it. Without protection the second silently overwrites the first one's changes — a **lost update**. Optimistic locking: mark a property as a **concurrency token**, and EF adds its original value to the `WHERE`:

```text
UPDATE "Blogs" SET "Name" = @p0, "Version" = @p1
WHERE "Id" = @p2 AND "Version" = @p3
```

If the row was already changed, the `UPDATE` affects 0 rows, and EF throws `DbUpdateConcurrencyException`: "The database operation was expected to affect 1 row(s), but actually affected 0 row(s); data may have been modified or deleted since entities were loaded".

| Option | Token | Who changes the value |
|---|---|---|
| `[ConcurrencyCheck]` / `IsConcurrencyToken()` | any property | the application (`Version++` in the example) |
| `[Timestamp]` / `IsRowVersion()` | `rowversion` | the database (SQL Server) |

Resolving the conflict:

```csharp
// "Database wins": throw away local changes and take the current values.
public static string DatabaseWins(BloggingContext context, DbUpdateConcurrencyException conflict)
{
    var entry = conflict.Entries.Single();
    entry.Reload();
    return ((Blog)entry.Entity).Name;
}
```

"Client wins": take the database values as the new original values (`entry.OriginalValues.SetValues(entry.GetDatabaseValues())`) and save again. A retry with a limited number of attempts is [exercise 3](exercises.en.md). Code: [Ex07_Concurrency.cs](code/Examples/Ex07_Concurrency.cs).

## Transactions

- **One `SaveChanges` is atomic:** if the provider supports transactions, all changes of one call are applied in a transaction. In the example the second blog violated the unique index (`SQLite Error 19: 'UNIQUE constraint failed: Blogs.Name'`), and the first blog was not saved either: `DbUpdateException`, 0 rows in the database.
- **Several `SaveChanges` calls** are combined with an explicit transaction:

```csharp
using (BloggingContext context = db.CreateContext())
using (var transaction = context.Database.BeginTransaction())
{
    context.Blogs.Add(new Blog { Name = "Tx 1" });
    context.SaveChanges();
    context.Blogs.Add(new Blog { Name = "Tx 2" });
    context.SaveChanges();
    transaction.Rollback();
}
```

After `Rollback` the database has 0 `Tx …` rows.

- **Savepoints.** If a transaction is already open, `SaveChanges` creates a savepoint by itself. A failed `SaveChanges` rolls back only to it, and earlier work is kept after `Commit`. Verified: the second `SaveChanges` failed, the first blog stayed.

Code: [Ex08_Transactions.cs](code/Examples/Ex08_Transactions.cs).

## Global query filters

A filter from `OnModelCreating` is added automatically to every query for the type. Typical uses: soft delete and multi-tenancy. Since EF Core 10 filters can be named and switched off by name:

```csharp
// Switch off one named filter for a single query (EF Core 10+).
public static int AllBlogsIncludingDeleted(BloggingContext context) =>
    context.Blogs.IgnoreQueryFilters([BloggingContext.SoftDeleteFilter]).Count();
```

After soft-deleting one blog, 2 blogs and 4 posts are visible, and with `IgnoreQueryFilters` all 3 blogs. The SQL of a blogs query: `SELECT … FROM "Blogs" AS "b" WHERE NOT ("b"."IsDeleted")`.

A subtlety: the filter on `Blog` does not hide its posts. A query on `Posts` would return the posts of a deleted blog. That is why the model has a second filter, `p => !p.Blog.IsDeleted`. Its price is a `JOIN` with `Blogs` in every posts query (see the SQL in [basics](basics.en.md)). Code: [Ex09_QueryFilters.cs](code/Examples/Ex09_QueryFilters.cs).

## Raw SQL

```csharp
// Safe: the interpolated value becomes a SQL parameter, not part of the SQL text.
public static int FindByNameSafe(BloggingContext context, string name) =>
    context.Blogs.FromSql($"SELECT * FROM Blogs WHERE Name = {name}").Count();

// Dangerous: the value is glued into the SQL text, so input can change the query itself.
#pragma warning disable EF1003 // the analyzer warns about exactly this mistake
public static int FindByNameUnsafe(BloggingContext context, string name) =>
    context.Blogs.FromSqlRaw("SELECT * FROM Blogs WHERE Name = '" + name + "'").Count();
#pragma warning restore EF1003
```

With the input `x' OR '1'='1` the safe version finds **0** blogs and the vulnerable one **all 3**: the input changed the query itself. `FromSql` with interpolation turns the value into a parameter (`WHERE Name = @p0`), and for concatenation inside `FromSqlRaw` the compiler reports analyzer warning EF1003. The result of `FromSql` can be composed further with LINQ: the global filter was added on top (`WHERE NOT ("e"."IsDeleted")`).

For scalars and types outside the model there is `Database.SqlQuery<T>`: `SqlQuery<int>($"SELECT SUM(Likes) AS Value FROM Posts").Single()` → `135`. Code: [Ex10_RawSql.cs](code/Examples/Ex10_RawSql.cs).

## DbContext lifetime

- `DbContext` is designed for **one unit of work**: create → do the operation → `SaveChanges` → dispose. In ASP.NET Core, `AddDbContext` registers it as scoped — one per HTTP request.
- `DbContext` is **not thread-safe**. A concurrent operation on one context throws `InvalidOperationException`: "A second operation was started on this context instance before a previous operation completed". Each thread or parallel task takes its own context (see [18 · Concurrency](../18-concurrency/overview.en.md)).
- A long-lived context accumulates tracked objects: memory grows and `SaveChanges` slows down.

## Best practices

- Reading: projections (`Select` into a DTO) and `AsNoTracking`.
- Load related data explicitly (`Include`, projection); enable lazy loading deliberately.
- Bulk changes: `ExecuteUpdate`/`ExecuteDelete`.
- Protect against lost updates with a concurrency token.
- Look at the generated SQL: `ToQueryString()`, `LogTo(Console.WriteLine)`.
- Async methods (`ToListAsync`, `SaveChangesAsync`) in web applications, so pool threads are not blocked.
- One context per unit of work.

## Common mistakes

- ❌ Lazy loading in a loop → ✅ N+1; `Include` or a projection.
- ❌ `ToList()`/`AsEnumerable()` before `Where` → ✅ the filter will run in memory after the whole table is loaded.
- ❌ Several collections in one `Include` on large data → ✅ `AsSplitQuery()`.
- ❌ `Update()` for a partial change → ✅ load and change, or `Attach` + change the needed properties.
- ❌ `ExecuteUpdate`, then working with loaded entities → ✅ they are stale; reload them.
- ❌ Concatenation in `FromSqlRaw` → ✅ `FromSql` with interpolation.
- ❌ A shared `DbContext` across threads or parallel tasks → ✅ a context per unit of work.

## Comparison with other tracks

| | C# — EF Core | Python — SQLAlchemy |
|---|---|---|
| Unit of work | `DbContext` | `Session` |
| Queries | LINQ | `select()` |
| Migrations | `dotnet ef migrations` | Alembic |
| Lazy loading by default | off | on (`lazy="select"`) |

More in the [Python track](../../../python/overview.en.md), module 20 "Databases".

## Related materials

- Before: [basics](basics.en.md)
- Next: [expert](expert.en.md) — tracking internals, inheritance, mapping, performance, reference, interview questions
- Threads and `DbContext`: [18 · Concurrency](../18-concurrency/advanced.en.md)
- `IDisposable` and `using`: [12 · Memory and GC](../12-memory-and-gc/basics.en.md#idisposable-and-using)

## Sources

- [Loading related data](https://learn.microsoft.com/ef/core/querying/related-data/)
- [Lazy loading](https://learn.microsoft.com/ef/core/querying/related-data/lazy)
- [Single vs. split queries](https://learn.microsoft.com/ef/core/querying/single-split-queries)
- [Client vs. server evaluation](https://learn.microsoft.com/ef/core/querying/client-eval)
- [ExecuteUpdate and ExecuteDelete](https://learn.microsoft.com/ef/core/saving/execute-insert-update-delete)
- [Handling concurrency conflicts](https://learn.microsoft.com/ef/core/saving/concurrency)
- [Transactions](https://learn.microsoft.com/ef/core/saving/transactions)
- [Global query filters](https://learn.microsoft.com/ef/core/querying/filters)
- [SQL queries](https://learn.microsoft.com/ef/core/querying/sql-queries)
- [DbContext lifetime and configuration](https://learn.microsoft.com/ef/core/dbcontext-configuration/)
