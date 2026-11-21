// Example: optimistic concurrency with a concurrency token, the conflict, and two ways to resolve it.
// Module: csharp/15-ef-core (advanced)
// Tests: dotnet test --filter "FullyQualifiedName~Concurrency"
using Microsoft.EntityFrameworkCore;

namespace Examples;

public static class Concurrency
{
    // Two users load the same blog; the second save finds that Version changed and gets a conflict.
    public static Exception? LostUpdateIsDetected(TestDatabase db, int blogId)
    {
        using BloggingContext alice = db.CreateContext();
        using BloggingContext bob = db.CreateContext();
        Blog aliceBlog = alice.Blogs.Single(b => b.Id == blogId);
        Blog bobBlog = bob.Blogs.Single(b => b.Id == blogId);

        aliceBlog.Name = "Alice's name";
        aliceBlog.Version++;
        alice.SaveChanges();

        bobBlog.Name = "Bob's name";
        bobBlog.Version++;
        try
        {
            bob.SaveChanges(); // UPDATE ... WHERE Id = @id AND Version = @originalVersion → 0 rows
            return null;
        }
        catch (DbUpdateConcurrencyException conflict)
        {
            return conflict;
        }
    }

    // "Database wins": throw away local changes and take the current values.
    public static string DatabaseWins(BloggingContext context, DbUpdateConcurrencyException conflict)
    {
        var entry = conflict.Entries.Single();
        entry.Reload();
        return ((Blog)entry.Entity).Name;
    }

    // "Client wins": take the database values as the new original values and save again.
    public static string ClientWins(BloggingContext context, Blog blog)
    {
        try
        {
            context.SaveChanges();
        }
        catch (DbUpdateConcurrencyException conflict)
        {
            var entry = conflict.Entries.Single();
            var databaseValues = entry.GetDatabaseValues()!;
            entry.OriginalValues.SetValues(databaseValues);
            blog.Version = databaseValues.GetValue<int>(nameof(Blog.Version)) + 1;
            context.SaveChanges();
        }
        return blog.Name;
    }
}
