// Example: entity states, automatic change detection, Find, no-tracking queries and disconnected updates.
// Module: csharp/15-ef-core (basics and advanced)
// Tests: dotnet test --filter "FullyQualifiedName~ChangeTracking"
using Microsoft.EntityFrameworkCore;

namespace Examples;

public static class ChangeTracking
{
    // Walks one entity through every state and records what the change tracker reports.
    public static List<EntityState> Lifecycle(BloggingContext context)
    {
        var states = new List<EntityState>();
        var blog = new Blog { Name = "Lifecycle" };
        states.Add(context.Entry(blog).State);   // Detached: the context does not know it yet

        context.Blogs.Add(blog);
        states.Add(context.Entry(blog).State);   // Added

        context.SaveChanges();
        states.Add(context.Entry(blog).State);   // Unchanged

        blog.Name = "Lifecycle 2";               // a plain property change, no EF call
        states.Add(context.Entry(blog).State);   // Modified: Entry() detects the change

        context.SaveChanges();
        context.Blogs.Remove(blog);
        states.Add(context.Entry(blog).State);   // Deleted

        context.SaveChanges();
        states.Add(context.Entry(blog).State);   // Detached again
        return states;
    }

    // Find returns an already tracked entity without going to the database.
    public static (bool SameInstance, int Commands) FindUsesTracker(BloggingContext context, CommandLog log, int blogId)
    {
        Blog loaded = context.Blogs.Single(b => b.Id == blogId);
        log.Clear();
        Blog? found = context.Blogs.Find(blogId);
        return (ReferenceEquals(loaded, found), log.Count);
    }

    // No-tracking queries are cheaper for read-only work, but changes to their results are not saved.
    public static (EntityState State, int Saved) NoTrackingChangesAreIgnored(BloggingContext context, int blogId)
    {
        Blog blog = context.Blogs.AsNoTracking().Single(b => b.Id == blogId);
        blog.Name = "Not saved";
        return (context.Entry(blog).State, context.SaveChanges());
    }

    // Without identity resolution, the same row produces a separate object for every occurrence.
    public static (int Distinct, int WithIdentityResolution) BlogInstancesFromPosts(BloggingContext context)
    {
        int plain = context.Posts.AsNoTracking().Include(p => p.Blog)
            .ToList().Select(p => p.Blog).Distinct().Count();
        int resolved = context.Posts.AsNoTrackingWithIdentityResolution().Include(p => p.Blog)
            .ToList().Select(p => p.Blog).Distinct().Count();
        return (plain, resolved);
    }

    // A disconnected entity (e.g. from an API request): Update marks every property as modified.
    public static string UpdateDisconnected(BloggingContext context, CommandLog log, Blog detached)
    {
        context.Blogs.Update(detached);
        log.Clear();
        context.SaveChanges();
        return log.Commands.Single();
    }
}
