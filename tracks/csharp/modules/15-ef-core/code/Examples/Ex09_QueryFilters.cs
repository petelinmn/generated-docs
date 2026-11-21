// Example: soft delete with a named global query filter, and how to switch it off.
// Module: csharp/15-ef-core (advanced)
// Tests: dotnet test --filter "FullyQualifiedName~QueryFilters"
using Microsoft.EntityFrameworkCore;

namespace Examples;

public static class QueryFilters
{
    public static void SoftDelete(BloggingContext context, int blogId)
    {
        Blog blog = context.Blogs.Single(b => b.Id == blogId);
        blog.IsDeleted = true;
        blog.Version++;
        context.SaveChanges();
    }

    // The filter is added to every query on Blog and on Post (see OnModelCreating).
    public static (int VisibleBlogs, int VisiblePosts) Visible(BloggingContext context) =>
        (context.Blogs.Count(), context.Posts.Count());

    // Switch off one named filter for a single query (EF Core 10+).
    public static int AllBlogsIncludingDeleted(BloggingContext context) =>
        context.Blogs.IgnoreQueryFilters([BloggingContext.SoftDeleteFilter]).Count();

    public static string FilteredSql(BloggingContext context) => context.Blogs.ToQueryString();
}
