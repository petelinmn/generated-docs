using Examples;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Solutions;

namespace Tests;

public class S01_PostQueriesTests : DatabaseTest
{
    [Fact]
    public void TopTitles_FiltersSortsAndLimits()
    {
        using BloggingContext context = Db.CreateContext();

        Assert.Equal(["Post 3.2", "Post 3.1", "Post 2.2"], PostQueries.TopTitles(context, minLikes: 20, take: 3));
    }
}

public class S02_BlogReportsTests : DatabaseTest
{
    [Fact]
    public void Summaries_AreComputedInOneQuery()
    {
        var log = new CommandLog();
        using BloggingContext context = Db.CreateContext(log);

        var rows = BlogReports.Summaries(context);

        Assert.Equal([("Blog 1", 2, 25), ("Blog 2", 2, 45), ("Blog 3", 2, 65)], rows);
        Assert.Equal(1, log.Count);
    }
}

public class S03_BlogServiceTests : DatabaseTest
{
    // Changes the blog through another context right before the first save, to force a conflict.
    private sealed class ConcurrentEdit(TestDatabase db) : SaveChangesInterceptor
    {
        private bool _done;

        public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
        {
            if (!_done)
            {
                _done = true;
                using BloggingContext other = db.CreateContext();
                Crud.Rename(other, 1, "Someone else");
            }
            return result;
        }
    }

    [Fact]
    public void RenameWithRetry_SucceedsOnSecondAttempt()
    {
        using BloggingContext context = Db.CreateContext(new ConcurrentEdit(Db));

        int attempts = BlogService.RenameWithRetry(context, blogId: 1, "Mine");

        Assert.Equal(2, attempts);
        Assert.Equal("Mine", Db.CreateContext().Blogs.Single(b => b.Id == 1).Name);
    }
}

public class S04_SoftDeleteInterceptorTests : DatabaseTest
{
    [Fact]
    public void Remove_BecomesSoftDelete()
    {
        using (BloggingContext context = Db.CreateContext(new SoftDeleteInterceptor()))
        {
            context.Blogs.Remove(context.Blogs.Single(b => b.Id == 1));
            context.SaveChanges();
        }

        using BloggingContext check = Db.CreateContext();
        Assert.Equal(2, check.Blogs.Count());
        Blog deleted = check.Blogs.IgnoreQueryFilters().Single(b => b.Id == 1);
        Assert.True(deleted.IsDeleted);
        Assert.Equal(2, check.Posts.IgnoreQueryFilters().Count(p => p.BlogId == 1));
    }
}
