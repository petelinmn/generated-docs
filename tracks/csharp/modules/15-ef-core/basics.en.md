---
title: "EF Core: basics"
lang: en
track: csharp
kind: module
doc: basics
slug: 15-ef-core
part: 2
tags: [ef-core, orm, dbcontext, linq, change-tracking, migrations, sqlite]
versions: "EF Core 11.0, .NET 11, C# 15"
status: draft
updated: 2026-11-21
---

# EF Core: basics

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md) · [exercises](exercises.en.md) · [5–6. Code and tests](code/)

**Prerequisites:** [02 · Types and variables](../02-types-and-variables/overview.en.md); modules 05 "Interfaces, structs, records" and 07 "LINQ" (not created yet).

## Why it matters

An application works with objects, while a relational database works with tables and SQL. An **ORM** (object-relational mapper) translates between the two. Entity Framework Core is Microsoft's ORM:

- it describes tables as C# classes (the **model**);
- it turns LINQ queries into SQL and the results back into objects;
- it tracks changes to objects and generates `INSERT`, `UPDATE`, `DELETE` itself;
- it keeps the history of schema changes as **migrations**.

EF Core talks to a database through a **provider**: SQL Server, PostgreSQL (Npgsql), SQLite, MySQL and others. The examples in this module use in-memory SQLite: it is a real relational database, so you see real SQL, but the tests need no server.

## The model and DbContext

The model is plain classes. EF Core infers a lot by conventions: an `Id` property is the primary key, `BlogId` is a foreign key to `Blog`, `List<Post>` is a one-to-many navigation.

```csharp
// Navigations are virtual so that the same model also works with lazy-loading proxies.
public class Blog
{
    public int Id { get; set; }

    public required string Name { get; set; }

    public bool IsDeleted { get; set; }

    // Application-managed concurrency token: every update must increment it.
    public int Version { get; set; }

    public virtual List<Post> Posts { get; set; } = [];
}

public class Post
{
    public int Id { get; set; }

    public required string Title { get; set; }

    public int Likes { get; set; }

    public DateTimeOffset? CreatedAt { get; set; }

    public int BlogId { get; set; }

    public virtual Blog Blog { get; set; } = null!;

    public virtual List<Tag> Tags { get; set; } = [];
}
```

`DbContext` is a session with the database: it exposes `DbSet<T>` for queries and holds the tracked objects. What conventions cannot infer is configured in `OnModelCreating` with the **Fluent API**:

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Blog>(blog =>
    {
        blog.Property(b => b.Name).HasMaxLength(100);
        blog.HasIndex(b => b.Name).IsUnique();
        blog.Property(b => b.Version).IsConcurrencyToken();
        // A named query filter (EF Core 10+): can be switched off by name.
        blog.HasQueryFilter(SoftDeleteFilter, b => !b.IsDeleted);
    });
```

EF creates the many-to-many relationship `Post` ↔ `Tag` by itself: the generated schema has a `PostTag` table:

```text
CREATE TABLE "PostTag" (
    "PostsId" INTEGER NOT NULL,
    "TagsId" INTEGER NOT NULL,
    CONSTRAINT "PK_PostTag" PRIMARY KEY ("PostsId", "TagsId"),
```

The context gets its settings (provider, connection string) through `DbContextOptions`: `new DbContextOptionsBuilder<BloggingContext>().UseSqlite(connection)`. Code: [Ex01_Model.cs](code/Examples/Ex01_Model.cs).

## Queries: LINQ becomes SQL

```csharp
// Nothing is sent to the database until the query is enumerated (ToList, First, Count…).
public static List<string> PopularTitles(BloggingContext context, int minLikes) =>
    context.Posts
        .Where(p => p.Likes >= minLikes)
        .OrderByDescending(p => p.Likes)
        .Select(p => p.Title)
        .ToList();
```

`ToQueryString()` shows the SQL without running it. For this query EF generates:

```text
.param set @minLikes 25
SELECT "p"."Title"
FROM "Posts" AS "p"
INNER JOIN (
    SELECT "b"."Id", "b"."IsDeleted"
    FROM "Blogs" AS "b"
    WHERE NOT ("b"."IsDeleted")
) AS "b0" ON "p"."BlogId" = "b0"."Id"
WHERE NOT ("b0"."IsDeleted") AND "p"."Likes" >= @minLikes
ORDER BY "p"."Likes" DESC
```

- `Where`, `OrderBy`, `Select` run **in the database**: only the needed rows and columns come back.
- The variable `minLikes` became the **parameter** `@minLikes`, not part of the SQL text — this protects against SQL injection.
- The `INNER JOIN` with `Blogs` comes from the soft-delete filter in the model; it is covered in [advanced](advanced.en.md).
- The query runs only when enumerated: `ToList()`, `First()`, `Count()`, `foreach`. Until then an `IQueryable` is just a description of the query (deferred execution).

The result on the test data: `["Post 3.2", "Post 3.1", "Post 2.2"]`. Code: [Ex02_Crud.cs](code/Examples/Ex02_Crud.cs).

## Saving: SaveChanges

```csharp
public static int CreateBlog(BloggingContext context, string name, params string[] postTitles)
{
    var blog = new Blog { Name = name };
    foreach (string title in postTitles)
    {
        blog.Posts.Add(new Post { Title = title });
    }
    context.Blogs.Add(blog);       // the whole graph becomes Added
    context.SaveChanges();         // one transaction; generated keys are written back
    return blog.Id;
}

// Load, change, save: the change tracker finds the modified property and updates only it.
public static void Rename(BloggingContext context, int blogId, string newName)
{
    Blog blog = context.Blogs.Single(b => b.Id == blogId);
    blog.Name = newName;
    blog.Version++;
    context.SaveChanges();
}
```

`Add` marks the whole object graph for insertion: the blog and its posts. `SaveChanges` sends all changes to the database in one transaction and writes the generated keys back into the objects. On update EF writes **only the changed columns**:

```text
UPDATE "Blogs" SET "Name" = @p0, "Version" = @p1
WHERE "Id" = @p2 AND "Version" = @p3
RETURNING 1;
```

`Remove` deletes an entity. In the example the blog's posts are deleted with it: the relationship is required, and cascade delete is configured for it by default. `SaveChanges` returned `3`: 1 blog + 2 posts.

## Change tracking

Entities returned by a query are **tracked**: the context keeps their original values and compares them with the current ones in `SaveChanges`.

| State | Meaning | What `SaveChanges` does |
|---|---|---|
| `Detached` | the context does not know the object | nothing |
| `Added` | a new object | `INSERT` |
| `Unchanged` | loaded and not changed | nothing |
| `Modified` | properties changed | `UPDATE` |
| `Deleted` | marked for deletion | `DELETE` |

The example walks one object through every state. The actual result: `Detached → Added → Unchanged → Modified → Deleted → Detached`. Moving to `Modified` needs no EF call: assigning a property is enough, the context notices the change by itself. Code: [Ex03_ChangeTracking.cs](code/Examples/Ex03_ChangeTracking.cs).

- **`AsNoTracking()`** is a query without tracking. It is faster, but `SaveChanges` will not save changes to such objects (it returns `0`). Use it for reading: reports, read-only APIs.
- **`Find(id)`** first looks among tracked entities and goes to the database only if it finds nothing. Verified: a second `Find` of an already loaded blog ran `0` SQL commands.

## Related data: Include

By default navigations are not loaded: after a plain query `blog.Posts` is empty. Related data is requested explicitly:

```csharp
context.Blogs.Include(b => b.Posts).ToList();
```

`Include` adds a `JOIN`, and everything comes in one query. The other ways — lazy and explicit loading, projection, split queries — and the main trap, "N+1", are covered in [advanced](advanced.en.md).

## Migrations

Migrations store the history of schema changes as code. The tool is `dotnet ef`, a local .NET tool (see [01 · The .NET platform](../01-dotnet-platform/expert.en.md)). The project needs the `Microsoft.EntityFrameworkCore.Design` package.

```bash
dotnet new tool-manifest
dotnet tool install dotnet-ef --version 11.0.0
dotnet ef migrations add InitialCreate
dotnet ef database update
dotnet ef migrations script
```

The actual result:

- `migrations add` created three files: `20261121042657_InitialCreate.cs` (the migration), `20261121042657_InitialCreate.Designer.cs` (metadata) and `AppContext2ModelSnapshot.cs` (a snapshot of the current model — the next migration is computed against it).
- `database update` applied the migration and recorded it in the `__EFMigrationsHistory` table.
- `migrations script` produced a SQL script to apply by hand.

A migration is a class with `Up` (apply) and `Down` (revert) methods:

```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    migrationBuilder.CreateTable(
        name: "Blogs",
        columns: table => new
        {
            Id = table.Column<int>(type: "INTEGER", nullable: false)
                .Annotation("Sqlite:Autoincrement", true),
            Name = table.Column<string>(type: "TEXT", nullable: false)
        },
        constraints: table =>
        {
            table.PrimaryKey("PK_Blogs", x => x.Id);
        });
}
```

`Database.EnsureCreated()` creates the schema without migrations — handy for tests and prototypes (the module's examples work this way). Do not mix it with migrations: a schema created by `EnsureCreated` breaks a later `Migrate`.

## Beginner mistakes

- ❌ Reading `blog.Posts` without `Include` and wondering why it is empty → ✅ `Include` or a `Select` projection.
- ❌ Expecting the query to run when `Where` is called → ✅ SQL is sent on `ToList`, `First`, `Count`, `foreach`.
- ❌ Gluing SQL from strings → ✅ LINQ or `FromSql` with interpolation: values become parameters.
- ❌ Forgetting `SaveChanges` → ✅ without it the changes stay in memory only.
- ❌ Tracking queries for reading large volumes → ✅ `AsNoTracking()`.
- ❌ Changing the database schema by hand → ✅ migrations: `dotnet ef migrations add`.

## Related materials

- Next: [advanced](advanced.en.md) — N+1, concurrency, transactions, filters, raw SQL
- .NET tools and `dotnet tool`: [01 · The .NET platform and dotnet CLI](../01-dotnet-platform/expert.en.md)
- `DbContext` thread safety: [18 · Concurrency](../18-concurrency/overview.en.md)
- The same in other tracks: [Python](../../../python/overview.en.md) — module 20 "Databases": SQLAlchemy and Alembic

## Sources

- [EF Core overview](https://learn.microsoft.com/ef/core/)
- [Creating a model](https://learn.microsoft.com/ef/core/modeling/)
- [Querying data](https://learn.microsoft.com/ef/core/querying/)
- [Saving data](https://learn.microsoft.com/ef/core/saving/)
- [Change tracking](https://learn.microsoft.com/ef/core/change-tracking/)
- [Migrations](https://learn.microsoft.com/ef/core/managing-schemas/migrations/)
- [Applying migrations](https://learn.microsoft.com/ef/core/managing-schemas/migrations/applying)
