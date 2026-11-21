---
title: "Exercises: EF Core"
lang: en
track: csharp
kind: module
doc: exercises
slug: 15-ef-core
part: 2
tags: [ef-core, projection, n-plus-one, optimistic-concurrency, interceptors, soft-delete]
status: draft
updated: 2026-11-21
---

# Exercises: EF Core

🌐 [Русский](exercises.ru.md) · **English**

> Solve on your own before opening the hints. Solutions are under spoilers; runnable versions are in [code/Solutions/Solutions.cs](code/Solutions/Solutions.cs), and their tests in [code/Tests/SolutionsTests.cs](code/Tests/SolutionsTests.cs).
>
> The model and the test database come from [Ex01_Model.cs](code/Examples/Ex01_Model.cs): `TestDatabase` creates in-memory SQLite, `Seed()` adds 3 blogs ("Blog 1"…"Blog 3"); blog `b` has posts "Post b.1" with `b * 10` likes and "Post b.2" with `b * 10 + 5`.

## 1. 🟢 The most popular posts

**Task:** write `PostQueries.TopTitles(context, minLikes, take)`: the titles of posts with at least `minLikes` likes, by likes descending (ties broken by title), at most `take` of them. Filtering, sorting and limiting must run in the database. The query only reads data.

**Expected result:**

```text
TopTitles(context, minLikes: 20, take: 3) → ["Post 3.2", "Post 3.1", "Post 2.2"]
```

<details><summary>Hint</summary>

All operators work on `IQueryable`, with `ToList()` at the very end. `AsNoTracking()` suits reading, and `Select` returns only the titles.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 1: a read-only query — filter, sort, limit and project in the database.
public static class PostQueries
{
    public static List<string> TopTitles(BloggingContext context, int minLikes, int take) =>
        context.Posts
            .AsNoTracking()
            .Where(p => p.Likes >= minLikes)
            .OrderByDescending(p => p.Likes)
            .ThenBy(p => p.Title)
            .Take(take)
            .Select(p => p.Title)
            .ToList();
}
```

`ThenBy` makes the order deterministic when the like counts are equal. Strictly speaking, `AsNoTracking` is not required here: a projection to `string` returns no entities, so there is nothing to track. But it states the "read-only" intent explicitly.

</details>

## 2. 🟡 A report without N+1

**Task:** the code below builds a per-blog report, but with lazy loading it runs 1 + N queries. Write `BlogReports.Summaries(context)` that returns the same result — `(Blog, Posts, TotalLikes)` for each blog, sorted by name — with **one** SQL query.

```csharp
foreach (Blog blog in context.Blogs.OrderBy(b => b.Name).ToList())
{
    rows.Add((blog.Name, blog.Posts.Count, blog.Posts.Sum(p => p.Likes)));
}
```

**Expected result:**

```text
[("Blog 1", 2, 25), ("Blog 2", 2, 45), ("Blog 3", 2, 65)], SQL commands: 1
```

<details><summary>Hint</summary>

Do not load the posts at all: ask the database to compute `Count` and `Sum` in a projection. A tuple literal inside an `IQueryable` query does not compile: error CS8143 "An expression tree may not contain a tuple literal". Project into an anonymous type and convert to a tuple after `AsEnumerable()`.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 2: replace an N+1 loop with one query that projects the aggregates.
public static class BlogReports
{
    public static List<(string Blog, int Posts, int TotalLikes)> Summaries(BloggingContext context) =>
        context.Blogs
            .OrderBy(b => b.Name)
            .Select(b => new { b.Name, Posts = b.Posts.Count, TotalLikes = b.Posts.Sum(p => p.Likes) })
            .AsEnumerable()
            .Select(x => (x.Name, x.Posts, x.TotalLikes))
            .ToList();
}
```

`Count` and `Sum` become subqueries in one `SELECT`, and the database returns three short rows. `AsEnumerable()` comes after the projection, so ready-made aggregates arrive in memory. The test checks the command count with the `CommandLog` interceptor.

</details>

## 3. 🟡 Rename with a retry on conflict

**Task:** write `BlogService.RenameWithRetry(context, blogId, newName, maxAttempts = 3)`. The method renames the blog and increments `Version`. If someone changed the blog between loading and saving, `SaveChanges` throws `DbUpdateConcurrencyException` — then take fresh original values from the database and retry ("client wins"). Return the number of the successful attempt. After `maxAttempts` failures the exception must propagate.

**Expected result:** before the first save the test renames the blog through another context.

```text
RenameWithRetry(context, blogId: 1, "Mine") → 2, name in the database: "Mine"
```

<details><summary>Hint</summary>

`conflict.Entries.Single()` is the tracker entry for the blog. `GetDatabaseValues()` returns the current values of the row (or `null` if it was deleted). Copy them into `OriginalValues` — then the next `UPDATE` compares against the current `Version`. A `catch … when (attempt < maxAttempts)` filter is a convenient way to limit attempts.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 3: optimistic concurrency with a retry — "client wins" on the name.
public static class BlogService
{
    public static int RenameWithRetry(BloggingContext context, int blogId, string newName, int maxAttempts = 3)
    {
        Blog blog = context.Blogs.Single(b => b.Id == blogId);
        for (int attempt = 1; ; attempt++)
        {
            blog.Name = newName;
            blog.Version++;
            try
            {
                context.SaveChanges();
                return attempt;
            }
            catch (DbUpdateConcurrencyException conflict) when (attempt < maxAttempts)
            {
                // Refresh original values (and the token) from the database, then try again.
                var entry = conflict.Entries.Single();
                var databaseValues = entry.GetDatabaseValues()
                    ?? throw new InvalidOperationException("The blog was deleted by someone else.");
                entry.OriginalValues.SetValues(databaseValues);
                blog.Version = databaseValues.GetValue<int>(nameof(Blog.Version));
            }
        }
    }
}
```

After `SetValues` the original value of `Version` matches the database, and the current value is incremented by 1 again before the next attempt. When the attempts run out, the `when` filter does not match and the exception propagates. If the row was deleted, retrying is pointless — the method says so explicitly.

</details>

## 4. 🔴 Soft delete through an interceptor

**Task:** write `SoftDeleteInterceptor : SaveChangesInterceptor`. When code calls `context.Blogs.Remove(blog)`, an `UPDATE` must run instead of a `DELETE`: `IsDeleted = true`. The blog's posts stay in the database, and the global filter hides the blog from regular queries.

**Expected result:**

```text
Remove(blog 1) + SaveChanges
check.Blogs.Count() → 2
check.Blogs.IgnoreQueryFilters().Single(b => b.Id == 1).IsDeleted → true
posts of blog 1 in the database (IgnoreQueryFilters) → 2
```

<details><summary>Hint</summary>

Override `SavingChanges`: it is called before EF builds the SQL. Find the entries in the `Deleted` state in `ChangeTracker.Entries<Blog>()` and move them to `Modified`. Do not forget the concurrency token.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 4: turn Remove() into a soft delete for blogs.
public sealed class SoftDeleteInterceptor : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        if (eventData.Context is not null)
        {
            foreach (var entry in eventData.Context.ChangeTracker.Entries<Blog>())
            {
                if (entry.State == EntityState.Deleted)
                {
                    entry.State = EntityState.Modified; // UPDATE instead of DELETE
                    entry.Entity.IsDeleted = true;
                    entry.Entity.Version++;
                }
            }
        }
        return result;
    }
}
```

The posts are not deleted: the blog was loaded without its posts, so the tracker's cascade did not touch them, and the database received an `UPDATE`, not a `DELETE`, so the database cascade did not fire either. Had the posts been loaded, `Remove` would have moved them to `Deleted` — then the interceptor would have to handle them too. For `SaveChangesAsync` you also need to override `SavingChangesAsync`.

</details>
