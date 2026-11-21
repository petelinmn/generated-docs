// Example: loading related data — lazy loading and the N+1 problem, eager loading, projection, split queries, explicit loading.
// Module: csharp/15-ef-core (basics and advanced)
// Tests: dotnet test --filter "FullyQualifiedName~Loading"
using Microsoft.EntityFrameworkCore;

namespace Examples;

public static class Loading
{
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

    // Eager loading: Include brings the posts in the same query (a JOIN).
    public static (int TotalPosts, int Commands) EagerLoading(TestDatabase db)
    {
        var log = new CommandLog();
        using BloggingContext context = db.CreateContext(log);
        int total = context.Blogs.Include(b => b.Posts).ToList().Sum(b => b.Posts.Count);
        return (total, log.Count);
    }

    // Projection: ask only for what you need; EF builds one query with a subquery or GROUP BY.
    public static (List<(string Name, int Posts)> Rows, int Commands) Projection(TestDatabase db)
    {
        var log = new CommandLog();
        using BloggingContext context = db.CreateContext(log);
        var rows = context.Blogs
            .OrderBy(b => b.Name)
            .Select(b => new { b.Name, Posts = b.Posts.Count })
            .AsEnumerable()
            .Select(x => (x.Name, x.Posts))
            .ToList();
        return (rows, log.Count);
    }

    // Two collections in one query multiply rows (cartesian explosion); a split query runs one SELECT per collection.
    public static (int SingleQueryCommands, int SplitQueryCommands) SplitQuery(TestDatabase db)
    {
        var log = new CommandLog();
        using BloggingContext context = db.CreateContext(log);
        _ = context.Posts.Include(p => p.Tags).Include(p => p.Blog).ThenInclude(b => b.Posts).AsSingleQuery().ToList();
        int single = log.Count;

        log.Clear();
        context.ChangeTracker.Clear();
        _ = context.Posts.Include(p => p.Tags).Include(p => p.Blog).ThenInclude(b => b.Posts).AsSplitQuery().ToList();
        return (single, log.Count);
    }

    // Filtered include: load only part of a collection.
    public static List<int> FilteredInclude(BloggingContext context) =>
        context.Blogs
            .Include(b => b.Posts.Where(p => p.Likes >= 20))
            .OrderBy(b => b.Name)
            .ToList()
            .Select(b => b.Posts.Count)
            .ToList();

    // Explicit loading: load a navigation later, on demand, for one entity.
    public static (bool LoadedBefore, int Posts, bool LoadedAfter) ExplicitLoading(BloggingContext context)
    {
        Blog blog = context.Blogs.OrderBy(b => b.Name).First();
        var posts = context.Entry(blog).Collection(b => b.Posts);
        bool before = posts.IsLoaded;
        posts.Load();
        return (before, blog.Posts.Count, posts.IsLoaded);
    }
}
