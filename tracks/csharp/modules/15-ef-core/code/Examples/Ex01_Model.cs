// Example: the model used by all examples — entities, relationships, configuration and an in-memory SQLite database.
// Module: csharp/15-ef-core (basics)
// Tests: dotnet test --filter "FullyQualifiedName~Model"
using System.Data.Common;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Examples;

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

public class Tag
{
    public int Id { get; set; }

    public required string Name { get; set; }

    public virtual List<Post> Posts { get; set; } = [];
}

public class BloggingContext(DbContextOptions<BloggingContext> options) : DbContext(options)
{
    public const string SoftDeleteFilter = "SoftDelete";

    public DbSet<Blog> Blogs => Set<Blog>();

    public DbSet<Post> Posts => Set<Post>();

    public DbSet<Tag> Tags => Set<Tag>();

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

        modelBuilder.Entity<Post>(post =>
        {
            post.Property(p => p.Title).HasMaxLength(200);
            post.HasOne(p => p.Blog).WithMany(b => b.Posts).HasForeignKey(p => p.BlogId);
            // Hide posts of soft-deleted blogs too, otherwise they leak through Posts queries.
            post.HasQueryFilter(SoftDeleteFilter, p => !p.Blog.IsDeleted);
        });

        // Many-to-many by convention: EF creates the PostTag join table itself.
        modelBuilder.Entity<Post>().HasMany(p => p.Tags).WithMany(t => t.Posts);
    }
}

// Counts and records every SQL command that EF sends to the database.
public sealed class CommandLog : DbCommandInterceptor
{
    private readonly List<string> _commands = [];

    public IReadOnlyList<string> Commands => _commands;

    public int Count => _commands.Count;

    public void Clear() => _commands.Clear();

    public override InterceptionResult<DbDataReader> ReaderExecuting(
        DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result)
    {
        _commands.Add(command.CommandText);
        return result;
    }

    public override InterceptionResult<int> NonQueryExecuting(
        DbCommand command, CommandEventData eventData, InterceptionResult<int> result)
    {
        _commands.Add(command.CommandText);
        return result;
    }

    public override InterceptionResult<object> ScalarExecuting(
        DbCommand command, CommandEventData eventData, InterceptionResult<object> result)
    {
        _commands.Add(command.CommandText);
        return result;
    }
}

// An in-memory SQLite database lives as long as its connection stays open.
public sealed class TestDatabase : IDisposable
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    public TestDatabase()
    {
        _connection.Open();
        using BloggingContext context = CreateContext();
        context.Database.EnsureCreated();
    }

    public BloggingContext CreateContext(params IInterceptor[] interceptors) =>
        new(Options(lazyLoading: false, interceptors));

    public BloggingContext CreateLazyContext(params IInterceptor[] interceptors) =>
        new(Options(lazyLoading: true, interceptors));

    public DbContextOptions<BloggingContext> Options(bool lazyLoading = false, params IInterceptor[] interceptors)
    {
        var builder = new DbContextOptionsBuilder<BloggingContext>().UseSqlite(_connection);
        if (lazyLoading)
        {
            builder.UseLazyLoadingProxies();
        }
        if (interceptors.Length > 0)
        {
            builder.AddInterceptors(interceptors);
        }
        return builder.Options;
    }

    // Three blogs, two posts each, some tags.
    public void Seed()
    {
        using BloggingContext context = CreateContext();
        var csharp = new Tag { Name = "csharp" };
        var data = new Tag { Name = "data" };
        for (int b = 1; b <= 3; b++)
        {
            var blog = new Blog { Name = $"Blog {b}" };
            blog.Posts.Add(new Post { Title = $"Post {b}.1", Likes = b * 10, Tags = [csharp] });
            blog.Posts.Add(new Post { Title = $"Post {b}.2", Likes = b * 10 + 5, Tags = [csharp, data] });
            context.Blogs.Add(blog);
        }
        context.SaveChanges();
    }

    public void Dispose() => _connection.Dispose();
}
