// Example: compiled queries, a pooled context factory, and the concurrent-use check of DbContext.
// Module: csharp/15-ef-core (expert)
// Tests: dotnet test --filter "FullyQualifiedName~Performance"
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Examples;

public static class Performance
{
    // Translation LINQ → SQL happens once; the delegate is thread-safe and reusable across contexts.
    private static readonly Func<BloggingContext, int, IEnumerable<Post>> PostsWithMinLikes =
        EF.CompileQuery<BloggingContext, int, Post>((context, minLikes) =>
            context.Posts.AsNoTracking().Where(p => p.Likes >= minLikes).OrderBy(p => p.Id));

    public static List<string> CompiledQuery(BloggingContext context, int minLikes) =>
        PostsWithMinLikes(context, minLikes).Select(p => p.Title).ToList();

    // A pooled factory resets and reuses context instances instead of creating new ones.
    public static (bool SameInstanceReused, int Blogs) PooledFactory(DbContextOptions<BloggingContext> options)
    {
        var factory = new PooledDbContextFactory<BloggingContext>(options);
        BloggingContext first = factory.CreateDbContext();
        first.Dispose(); // returns the instance to the pool
        using BloggingContext second = factory.CreateDbContext();
        return (ReferenceEquals(first, second), second.Blogs.Count());
    }

    // DbContext is not thread-safe: starting a second operation while the first is running throws.
    // EF checks this only while an operation is inside a call (executing a command, MoveNext),
    // so the interceptor holds the first command until the second operation has been tried.
    public static async Task<Exception?> ConcurrentUseAsync(TestDatabase db)
    {
        var blocker = new BlockFirstCommand();
        using BloggingContext context = db.CreateContext(blocker);
        Task first = Task.Run(() => context.Blogs.ToList());
        await blocker.Started;
        try
        {
            _ = context.Posts.Count();
            return null;
        }
        catch (InvalidOperationException e)
        {
            return e;
        }
        finally
        {
            blocker.Release();
            await first;
        }
    }

    private sealed class BlockFirstCommand : Microsoft.EntityFrameworkCore.Diagnostics.DbCommandInterceptor
    {
        private readonly TaskCompletionSource _started = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly ManualResetEventSlim _release = new();
        private int _calls;

        public Task Started => _started.Task;

        public void Release() => _release.Set();

        public override Microsoft.EntityFrameworkCore.Diagnostics.InterceptionResult<System.Data.Common.DbDataReader> ReaderExecuting(
            System.Data.Common.DbCommand command,
            Microsoft.EntityFrameworkCore.Diagnostics.CommandEventData eventData,
            Microsoft.EntityFrameworkCore.Diagnostics.InterceptionResult<System.Data.Common.DbDataReader> result)
        {
            if (Interlocked.Increment(ref _calls) == 1)
            {
                _started.SetResult();
                _release.Wait();
            }
            return result;
        }
    }
}
