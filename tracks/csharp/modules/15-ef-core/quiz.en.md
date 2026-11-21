---
title: "Quiz: EF Core"
lang: en
track: csharp
kind: module
doc: quiz
slug: 15-ef-core
part: 2
tags: [ef-core, change-tracking, n-plus-one, optimistic-concurrency, raw-sql, inheritance-mapping]
status: draft
updated: 2026-11-21
---

# Quiz: EF Core

🌐 [Русский](quiz.ru.md) · **English**

> Answer first, then open the answer. The mark shows which level a question comes from: 🟢 [basics](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).
>
> All questions use the same data: 3 blogs with 2 posts each, lazy loading off unless stated otherwise.

## 1. 🟢 On which line does EF send SQL to the database?

```csharp
IQueryable<Post> query = context.Posts.Where(p => p.Likes > 10);   // 1
query = query.OrderBy(p => p.Title);                                // 2
var titles = query.Select(p => p.Title);                            // 3
List<string> list = titles.ToList();                                // 4
```

- A) 1
- B) 2
- C) 3
- D) 4

<details><summary>Answer</summary>

**D.** `Where`, `OrderBy` and `Select` on `IQueryable` only build a description of the query. SQL is sent on enumeration: `ToList`, `First`, `Count`, `foreach`. All three operators end up in one SQL query.
→ [Basics](basics.en.md#queries-linq-becomes-sql)

</details>

## 2. 🟢 What does this code print?

```csharp
Blog blog = context.Blogs.Single(b => b.Id == 1);
Console.WriteLine(blog.Posts.Count);
```

- A) `2`: EF loads the posts when the navigation is accessed
- B) `0`
- C) `NullReferenceException`
- D) `InvalidOperationException`: the navigation is not loaded

<details><summary>Answer</summary>

**B.** Without `Include` and without lazy loading the navigation is not loaded, and the list is initialized empty (`= []`). There is no error, so the bug is easy to miss. You need `Include(b => b.Posts)` or a projection. Option A is right only with lazy loading enabled.
→ [Basics](basics.en.md#related-data-include)

</details>

## 3. 🟢 A blog is loaded with a regular query, then only its `Name` property is changed. What `UPDATE` does `SaveChanges` generate?

- A) it updates every column of the `Blogs` table
- B) it updates only the changed columns
- C) none: without calling `Update(blog)` changes are not saved
- D) it deletes the row and inserts a new one

<details><summary>Answer</summary>

**B.** A tracked entity keeps a snapshot of the original values, and EF writes only the changed properties: `UPDATE "Blogs" SET "Name" = @p0 …`. Calling `Update` is not needed — on the contrary, `Update` would mark every property as modified, and you would get option A.
→ [Basics](basics.en.md#saving-savechanges)

</details>

## 4. 🟢 What does `SaveChanges()` return?

```csharp
Blog blog = context.Blogs.AsNoTracking().Single(b => b.Id == 1);
blog.Name = "Changed";
int saved = context.SaveChanges();
```

- A) `1`
- B) `0`
- C) an exception: the entity is not tracked
- D) `2`

<details><summary>Answer</summary>

**B.** An entity from `AsNoTracking` is in the `Detached` state: the context does not know about it and does not see its changes. There is no error — there is simply nothing to save.
→ [Basics](basics.en.md#change-tracking)

</details>

## 5. 🟢 The blog with `Id = 1` is already loaded into this context. How many SQL commands does `context.Blogs.Find(1)` run?

- A) 0
- B) 1
- C) 2: an existence check and a load
- D) it depends on the table size

<details><summary>Answer</summary>

**A.** `Find` first looks among tracked entities and goes to the database only if the entity is not there. `Single(b => b.Id == 1)` would run SQL in any case.
→ [Basics](basics.en.md#change-tracking)

</details>

## 6. 🟢 Which statements about migrations are true? (several answers)

- A) `dotnet ef migrations add` creates a migration class with `Up` and `Down` methods and updates the model snapshot
- B) `dotnet ef migrations add` changes the database schema right away
- C) `dotnet ef database update` applies migrations and records them in `__EFMigrationsHistory`
- D) a database created by `EnsureCreated()` can later be updated with migrations

<details><summary>Answer</summary>

**A, C.** `migrations add` only generates code; the database is changed by `database update` (or `Migrate()`, a script, a bundle). `EnsureCreated` does not create the migrations history table, and a later `Migrate()` fails on the tables that already exist.
→ [Basics](basics.en.md#migrations)

</details>

## 7. 🟡 Lazy loading is enabled. How many SQL commands does the code run?

```csharp
int total = 0;
foreach (Blog blog in context.Blogs.ToList())
{
    total += blog.Posts.Count;
}
```

- A) 1
- B) 2
- C) 4
- D) 7

<details><summary>Answer</summary>

**C.** One query for the blogs and one query for the posts of each of the 3 blogs — the N+1 problem. For 1000 blogs it would be 1001 commands. The projection `Select(b => new { b.Name, Posts = b.Posts.Count })` does the same in 1 command.
→ [Advanced](advanced.en.md#loading-related-data-and-the-n1-problem)

</details>

## 8. 🟡 `Shorten` is a plain C# method. What happens?

```csharp
var a = context.Posts.Select(p => Shorten(p.Title)).ToList();
var b = context.Posts.Where(p => Shorten(p.Title).EndsWith("…")).ToList();
```

- A) both queries run in the database
- B) both throw an exception
- C) `a` runs, `b` throws `InvalidOperationException`
- D) both run: EF silently loads the table and filters in memory

<details><summary>Answer</summary>

**C.** Client evaluation is allowed only in the final `Select`: EF loads the titles and calls `Shorten` in memory. In `Where` the method would have to run in the database, and it cannot be translated to SQL — the exception "could not be translated". Option D describes the EF Core 2.x behavior, which was dropped so that tables are not loaded unnoticed.
→ [Advanced](advanced.en.md#what-ef-can-translate-to-sql)

</details>

## 9. 🟡 What does this code print? Post 1 had 10 likes.

```csharp
Post post = context.Posts.Single(p => p.Id == 1);
context.Posts.Where(p => p.Id == 1)
    .ExecuteUpdate(s => s.SetProperty(p => p.Likes, 1000));
Console.WriteLine(post.Likes);
```

- A) `1000`
- B) `10`
- C) `0`
- D) an exception: the entity is tracked

<details><summary>Answer</summary>

**B.** `ExecuteUpdate` runs SQL right away and does not touch the change tracker. The database has `1000`, while the loaded object is stale. To see the new value you need `context.Entry(post).Reload()` or a new context.
→ [Advanced](advanced.en.md#executeupdate-and-executedelete)

</details>

## 10. 🟡 Find the bug

```csharp
public int CountByName(string name) =>
    context.Blogs.FromSqlRaw("SELECT * FROM Blogs WHERE Name = '" + name + "'").Count();
```

<details><summary>Answer</summary>

SQL injection: the value is glued into the query text. The input `x' OR '1'='1` returns all 3 blogs instead of 0. The EF analyzer reports warning EF1003 on this code. The fix: `FromSql($"SELECT * FROM Blogs WHERE Name = {name}")` — the value becomes the parameter `@p0`. Simpler still is plain LINQ: `context.Blogs.Count(b => b.Name == name)`.
→ [Advanced](advanced.en.md#raw-sql)

</details>

## 11. 🟡 The `Version` property is configured with `IsConcurrencyToken()`. Two users loaded the same blog; both changed the name and incremented `Version`. The first one saved. What happens when the second one saves?

- A) the second user's changes overwrite the first user's
- B) `UPDATE … WHERE "Id" = @id AND "Version" = @old` affects 0 rows, EF throws `DbUpdateConcurrencyException`
- C) EF locks the row, and the second user waits
- D) EF merges both users' changes automatically

<details><summary>Answer</summary>

**B.** This is optimistic concurrency: the token's original value goes into the `WHERE`, and if the row was already changed the `UPDATE` finds nothing. The application must resolve the conflict: re-read the data (`Reload`) or retry the save with fresh original values. EF takes no locks (C) and merges nothing by itself (D). A is what would happen without a token, or if `Version` were not incremented.
→ [Advanced](advanced.en.md#optimistic-concurrency)

</details>

## 12. 🟡 What is wrong with this code and how do you fix it?

```csharp
await Parallel.ForEachAsync(blogIds, async (id, ct) =>
{
    Blog blog = await _context.Blogs.SingleAsync(b => b.Id == id, ct);
    blog.Name = blog.Name.Trim();
    await _context.SaveChangesAsync(ct);
});
```

<details><summary>Answer</summary>

One `DbContext` is used from several parallel tasks, and it is not thread-safe. When operations overlap, EF throws `InvalidOperationException`: "A second operation was started on this context instance before a previous operation completed", or, if the context is still initializing, "An attempt was made to use the context instance while it is being configured". In 50 runs on SQLite every run failed, with both messages. In general, though, the error depends on whether the operations overlap in time, so a run without an error proves nothing. Fixes: create a separate context per task with `IDbContextFactory<T>`; or process sequentially; or load all the blogs in one query, change them, and call `SaveChanges` once.
→ [Advanced](advanced.en.md#dbcontext-lifetime)

</details>

## 13. 🔴 Hierarchy `Payment` → `CardPayment`, `CashPayment`. Which strategy creates no table for the base type and runs a query on `Payments` with `UNION ALL`?

- A) TPH
- B) TPT
- C) TPC
- D) none: EF always creates a table for the base type

<details><summary>Answer</summary>

**C.** TPC (table per concrete type) creates a full table for every non-abstract type, and a polymorphic query combines them. TPH is one table with a discriminator; TPT is a base table plus subtype tables combined with `JOIN`. TPC keys must be unique across the whole hierarchy, which is why the example uses `Guid`.
→ [Expert](expert.en.md#inheritance-tph-tpt-tpc)

</details>

## 14. 🔴 What does this code print?

```csharp
string name = "x' OR '1'='1";
Console.WriteLine(context.Blogs.FromSqlRaw($"SELECT * FROM Blogs WHERE Name = '{name}'").Count());
```

- A) `0`: the interpolated string becomes a parameter
- B) `3`
- C) a SQL syntax exception
- D) it does not compile

<details><summary>Answer</summary>

**B.** `FromSqlRaw` takes a `string`, so C# inserts the value into the text before EF is even called — it is an injection, and the condition `OR '1'='1'` returns all blogs. Interpolation becomes a parameter only in `FromSql`, which takes a `FormattableString`. The code compiles, but the analyzer reports warning EF1002.
→ [Expert](expert.en.md#fromsqlraw-with-interpolation)

</details>

## 15. 🔴 What do the two `SaveChanges()` calls return?

```csharp
context.ChangeTracker.AutoDetectChangesEnabled = false;
Blog blog = context.Blogs.Single(b => b.Id == 3);
blog.Name = "Changed";
int first = context.SaveChanges();
context.ChangeTracker.DetectChanges();
int second = context.SaveChanges();
```

- A) `1` and `0`
- B) `0` and `1`
- C) `0` and `0`
- D) `1` and `1`

<details><summary>Answer</summary>

**B.** EF finds changes by comparing current values with the snapshot in `DetectChanges()`. Normally `SaveChanges` calls it itself, but here automatic detection is off: the first call sees no changes and returns `0`. After an explicit `DetectChanges()` the entity becomes `Modified`, and the second call saves `1` row.
→ [Expert](expert.en.md#how-change-tracking-works)

</details>
