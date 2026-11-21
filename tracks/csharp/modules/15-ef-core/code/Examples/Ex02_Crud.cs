// Example: create, read, update, delete — and the SQL that a LINQ query becomes.
// Module: csharp/15-ef-core (basics)
// Tests: dotnet test --filter "FullyQualifiedName~Crud"
using Microsoft.EntityFrameworkCore;

namespace Examples;

public static class Crud
{
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

    // Nothing is sent to the database until the query is enumerated (ToList, First, Count…).
    public static List<string> PopularTitles(BloggingContext context, int minLikes) =>
        context.Posts
            .Where(p => p.Likes >= minLikes)
            .OrderByDescending(p => p.Likes)
            .Select(p => p.Title)
            .ToList();

    public static string PopularTitlesSql(BloggingContext context, int minLikes) =>
        context.Posts
            .Where(p => p.Likes >= minLikes)
            .OrderByDescending(p => p.Likes)
            .Select(p => p.Title)
            .ToQueryString();

    // Load, change, save: the change tracker finds the modified property and updates only it.
    public static void Rename(BloggingContext context, int blogId, string newName)
    {
        Blog blog = context.Blogs.Single(b => b.Id == blogId);
        blog.Name = newName;
        blog.Version++;
        context.SaveChanges();
    }

    public static int Delete(BloggingContext context, int blogId)
    {
        Blog blog = context.Blogs.Include(b => b.Posts).Single(b => b.Id == blogId);
        context.Blogs.Remove(blog);    // posts are deleted too: the relationship is required (cascade)
        return context.SaveChanges();  // number of rows affected
    }
}
