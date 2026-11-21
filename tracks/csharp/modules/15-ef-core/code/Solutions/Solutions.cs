// Solutions to the exercises of module csharp/15-ef-core.
// Tests: dotnet test --filter "FullyQualifiedName~Tests.S0"
using Examples;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Solutions;

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
