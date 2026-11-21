// Example: ExecuteUpdate and ExecuteDelete — one SQL statement, no loading, no change tracker.
// Module: csharp/15-ef-core (advanced)
// Tests: dotnet test --filter "FullyQualifiedName~BulkOperations"
using Microsoft.EntityFrameworkCore;

namespace Examples;

public static class BulkOperations
{
    // UPDATE ... SET Likes = Likes + 1 WHERE ...: runs immediately, without SaveChanges.
    public static (int Affected, int Commands, string Sql) LikeAllPostsOfBlog(BloggingContext context, CommandLog log, int blogId)
    {
        log.Clear();
        int affected = context.Posts
            .Where(p => p.BlogId == blogId)
            .ExecuteUpdate(setters => setters.SetProperty(p => p.Likes, p => p.Likes + 1));
        return (affected, log.Count, log.Commands.Single());
    }

    // A tracked entity does not see the change made by ExecuteUpdate.
    public static (int TrackedLikes, int DatabaseLikes) TrackedEntityIsStale(BloggingContext context, int postId)
    {
        Post tracked = context.Posts.Single(p => p.Id == postId);
        context.Posts.Where(p => p.Id == postId)
            .ExecuteUpdate(setters => setters.SetProperty(p => p.Likes, 1000));
        int inDatabase = context.Posts.AsNoTracking().Single(p => p.Id == postId).Likes;
        return (tracked.Likes, inDatabase);
    }

    public static int DeleteUnpopular(BloggingContext context, int maxLikes) =>
        context.Posts.Where(p => p.Likes <= maxLikes).ExecuteDelete();
}
