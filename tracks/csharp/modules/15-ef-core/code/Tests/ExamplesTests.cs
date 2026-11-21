using Examples;
using Microsoft.EntityFrameworkCore;

namespace Tests;

// Every test gets its own in-memory SQLite database: tests are isolated and deterministic.
public abstract class DatabaseTest : IDisposable
{
    protected readonly TestDatabase Db = new();

    protected DatabaseTest(bool seed = true)
    {
        if (seed)
        {
            Db.Seed();
        }
    }

    public void Dispose() => Db.Dispose();
}

public class Ex01_ModelTests : DatabaseTest
{
    [Fact]
    public void Seed_CreatesGraphWithManyToMany()
    {
        using BloggingContext context = Db.CreateContext();

        Assert.Equal(3, context.Blogs.Count());
        Assert.Equal(6, context.Posts.Count());
        Assert.Equal(2, context.Tags.Count());
        Assert.Equal(6, context.Tags.Include(t => t.Posts).Single(t => t.Name == "csharp").Posts.Count);
    }
}

public class Ex02_CrudTests : DatabaseTest
{
    [Fact]
    public void Crud_RoundTrip()
    {
        using BloggingContext context = Db.CreateContext();

        int id = Crud.CreateBlog(context, "New", "A", "B");
        Assert.True(id > 0);

        Crud.Rename(context, id, "Renamed");
        Assert.Equal("Renamed", Db.CreateContext().Blogs.Single(b => b.Id == id).Name);

        Assert.Equal(3, Crud.Delete(context, id)); // 1 blog + 2 posts
        Assert.Equal(0, Db.CreateContext().Blogs.Count(b => b.Id == id));
    }

    [Fact]
    public void LinqQuery_BecomesSql()
    {
        using BloggingContext context = Db.CreateContext();

        Assert.Equal(["Post 3.2", "Post 3.1", "Post 2.2"], Crud.PopularTitles(context, 25));
        string sql = Crud.PopularTitlesSql(context, 25);
        Assert.Contains("WHERE", sql);
        Assert.Contains("ORDER BY", sql);
    }
}

public class Ex03_ChangeTrackingTests : DatabaseTest
{
    [Fact]
    public void Lifecycle_GoesThroughAllStates()
    {
        using BloggingContext context = Db.CreateContext();

        Assert.Equal(
            [EntityState.Detached, EntityState.Added, EntityState.Unchanged,
             EntityState.Modified, EntityState.Deleted, EntityState.Detached],
            ChangeTracking.Lifecycle(context));
    }

    [Fact]
    public void Find_UsesTrackedEntity_WithoutSql()
    {
        var log = new CommandLog();
        using BloggingContext context = Db.CreateContext(log);

        Assert.Equal((true, 0), ChangeTracking.FindUsesTracker(context, log, blogId: 1));
    }

    [Fact]
    public void NoTracking_ChangesAreNotSaved()
    {
        using BloggingContext context = Db.CreateContext();

        Assert.Equal((EntityState.Detached, 0), ChangeTracking.NoTrackingChangesAreIgnored(context, 1));
    }

    [Fact]
    public void IdentityResolution_DeduplicatesInstances()
    {
        using BloggingContext context = Db.CreateContext();

        Assert.Equal((6, 3), ChangeTracking.BlogInstancesFromPosts(context));
    }

    [Fact]
    public void Update_MarksAllPropertiesModified()
    {
        var log = new CommandLog();
        using BloggingContext context = Db.CreateContext(log);
        var detached = new Blog { Id = 1, Name = "From API", Version = 0 };

        string sql = ChangeTracking.UpdateDisconnected(context, log, detached);

        Assert.Contains("\"Name\"", sql);
        Assert.Contains("\"IsDeleted\"", sql);
        Assert.Contains("\"Version\"", sql);
    }
}

public class Ex04_LoadingTests : DatabaseTest
{
    [Fact]
    public void LazyLoading_RunsNPlusOneQueries() =>
        Assert.Equal((6, 1 + 3), Loading.LazyLoading(Db));

    [Fact]
    public void EagerLoading_RunsOneQuery() =>
        Assert.Equal((6, 1), Loading.EagerLoading(Db));

    [Fact]
    public void Projection_RunsOneQuery()
    {
        var (rows, commands) = Loading.Projection(Db);

        Assert.Equal([("Blog 1", 2), ("Blog 2", 2), ("Blog 3", 2)], rows);
        Assert.Equal(1, commands);
    }

    [Fact]
    public void SplitQuery_RunsOneQueryPerCollection()
    {
        var (single, split) = Loading.SplitQuery(Db);

        Assert.Equal(1, single);
        Assert.Equal(3, split);
    }

    [Fact]
    public void FilteredInclude_LoadsPartOfCollection()
    {
        using BloggingContext context = Db.CreateContext();

        Assert.Equal([0, 2, 2], Loading.FilteredInclude(context));
    }

    [Fact]
    public void ExplicitLoading_LoadsOnDemand()
    {
        using BloggingContext context = Db.CreateContext();

        Assert.Equal((false, 2, true), Loading.ExplicitLoading(context));
    }
}

public class Ex05_TranslationTests : DatabaseTest
{
    [Fact]
    public void ClientEvaluation_IsAllowedInFinalProjection()
    {
        using BloggingContext context = Db.CreateContext();

        Assert.Equal("Post 1…", Translation.ClientEvalInProjection(context)[0]);
    }

    [Fact]
    public void ClientEvaluation_InWhere_Throws()
    {
        using BloggingContext context = Db.CreateContext();

        var error = Assert.Throws<InvalidOperationException>(() => Translation.ClientEvalInWhere(context));
        Assert.Contains("could not be translated", error.Message);
    }

    [Fact]
    public void IQueryableFiltersInDatabase_IEnumerableInMemory()
    {
        using BloggingContext context = Db.CreateContext();

        var (count, sql) = Translation.FilterInDatabase(context);
        Assert.Equal(1, count);
        Assert.Contains("WHERE", sql);

        Assert.Equal((1, 6), Translation.FilterInMemory(context));
    }
}

public class Ex06_BulkOperationsTests : DatabaseTest
{
    [Fact]
    public void ExecuteUpdate_IsOneStatement()
    {
        var log = new CommandLog();
        using BloggingContext context = Db.CreateContext(log);

        var (affected, commands, sql) = BulkOperations.LikeAllPostsOfBlog(context, log, blogId: 1);

        Assert.Equal(2, affected);
        Assert.Equal(1, commands);
        Assert.StartsWith("UPDATE", sql.TrimStart());
    }

    [Fact]
    public void ExecuteUpdate_BypassesChangeTracker()
    {
        using BloggingContext context = Db.CreateContext();

        Assert.Equal((10, 1000), BulkOperations.TrackedEntityIsStale(context, postId: 1));
    }

    [Fact]
    public void ExecuteDelete_ReturnsAffectedRows()
    {
        using BloggingContext context = Db.CreateContext();

        Assert.Equal(2, BulkOperations.DeleteUnpopular(context, maxLikes: 15));
    }
}

public class Ex07_ConcurrencyTests : DatabaseTest
{
    [Fact]
    public void SecondSave_GetsConcurrencyException() =>
        Assert.IsType<DbUpdateConcurrencyException>(Concurrency.LostUpdateIsDetected(Db, blogId: 1));

    [Fact]
    public void Conflict_CanBeResolvedEitherWay()
    {
        using BloggingContext bob = Db.CreateContext();
        Blog bobBlog = bob.Blogs.Single(b => b.Id == 2);
        using (BloggingContext alice = Db.CreateContext())
        {
            Crud.Rename(alice, 2, "Alice wins?");
        }

        bobBlog.Name = "Bob";
        bobBlog.Version++;

        Assert.Equal("Bob", Concurrency.ClientWins(bob, bobBlog));
        Assert.Equal("Bob", Db.CreateContext().Blogs.Single(b => b.Id == 2).Name);
    }

    [Fact]
    public void DatabaseWins_ReloadsCurrentValues()
    {
        using BloggingContext bob = Db.CreateContext();
        Blog bobBlog = bob.Blogs.Single(b => b.Id == 3);
        using (BloggingContext alice = Db.CreateContext())
        {
            Crud.Rename(alice, 3, "Alice");
        }
        bobBlog.Name = "Bob";
        bobBlog.Version++;

        var conflict = Assert.Throws<DbUpdateConcurrencyException>(() => bob.SaveChanges());

        Assert.Equal("Alice", Concurrency.DatabaseWins(bob, conflict));
    }
}

public class Ex08_TransactionsTests : DatabaseTest
{
    [Fact]
    public void SaveChanges_IsAllOrNothing() =>
        Assert.Equal((typeof(DbUpdateException), 0), Transactions.SaveChangesIsAtomic(Db));

    [Fact]
    public void ExplicitTransaction_RollsBackSeveralSaves() =>
        Assert.Equal(0, Transactions.ExplicitTransactionRollback(Db));

    [Fact]
    public void Savepoint_KeepsWorkBeforeTheFailure() =>
        Assert.Equal((true, 1), Transactions.SavepointKeepsEarlierWork(Db));
}

public class Ex09_QueryFiltersTests : DatabaseTest
{
    [Fact]
    public void SoftDeletedBlog_AndItsPosts_AreHidden()
    {
        using BloggingContext context = Db.CreateContext();

        QueryFilters.SoftDelete(context, blogId: 1);

        Assert.Equal((2, 4), QueryFilters.Visible(context));
        Assert.Equal(3, QueryFilters.AllBlogsIncludingDeleted(context));
        Assert.Contains("IsDeleted", QueryFilters.FilteredSql(context));
    }
}

public class Ex10_RawSqlTests : DatabaseTest
{
    private const string Injection = "x' OR '1'='1";

    [Fact]
    public void FromSql_IsParameterized_FromSqlRawConcatenationIsInjectable()
    {
        using BloggingContext context = Db.CreateContext();

        Assert.Equal(0, RawSql.FindByNameSafe(context, Injection));
        Assert.Equal(3, RawSql.FindByNameUnsafe(context, Injection));
        Assert.Contains("@", RawSql.SafeSql(context, Injection));
    }

    [Fact]
    public void SqlQuery_ReturnsScalar()
    {
        using BloggingContext context = Db.CreateContext();

        Assert.Equal(10 + 15 + 20 + 25 + 30 + 35, RawSql.TotalLikes(context));
    }
}

public class Ex11_InterceptorsTests : DatabaseTest
{
    [Fact]
    public void AuditInterceptor_StampsNewPosts()
    {
        var now = new DateTimeOffset(2026, 11, 21, 12, 0, 0, TimeSpan.Zero);
        using BloggingContext context = Db.CreateContext(new AuditInterceptor(new FixedTimeProvider(now)));

        int id = Crud.CreateBlog(context, "Audited", "First");

        Assert.Equal(now, Db.CreateContext().Posts.Single(p => p.BlogId == id).CreatedAt);
    }
}

public class Ex12_InheritanceTests
{
    [Fact]
    public void Tph_UsesOneTableWithDiscriminator()
    {
        string script = Inheritance.CreateScript(o => new TphContext(o));

        Assert.Contains("\"Discriminator\"", script);
        Assert.Single(System.Text.RegularExpressions.Regex.Matches(script, "CREATE TABLE"));
    }

    [Fact]
    public void Tpt_AndTpc_UseSeveralTables()
    {
        Assert.Equal(3, System.Text.RegularExpressions.Regex.Matches(Inheritance.CreateScript(o => new TptContext(o)), "CREATE TABLE").Count);
        Assert.Equal(2, System.Text.RegularExpressions.Regex.Matches(Inheritance.CreateScript(o => new TpcContext(o)), "CREATE TABLE").Count);
    }

    [Fact]
    public void PolymorphicQuery_ReturnsDerivedTypes()
    {
        Assert.Equal((1, 1), Inheritance.RoundTrip(o => new TphContext(o)));
        Assert.Equal((1, 1), Inheritance.RoundTrip(o => new TptContext(o)));
        Assert.Equal((1, 1), Inheritance.RoundTrip(o => new TpcContext(o)));
    }
}

public class Ex13_PerformanceTests : DatabaseTest
{
    [Fact]
    public void CompiledQuery_Works()
    {
        using BloggingContext context = Db.CreateContext();

        Assert.Equal(["Post 3.1", "Post 3.2"], Performance.CompiledQuery(context, 30));
    }

    [Fact]
    public void PooledFactory_ReusesInstances() =>
        Assert.Equal((true, 3), Performance.PooledFactory(Db.Options()));

    [Fact]
    public async Task ConcurrentUse_Throws()
    {
        Exception? error = await Performance.ConcurrentUseAsync(Db);

        Assert.IsType<InvalidOperationException>(error);
        Assert.Contains("second operation", error!.Message);
    }
}

public class Ex14_MappingTests
{
    [Fact]
    public void Converters_ComplexTypes_ShadowProperties()
    {
        Assert.Equal(("Paid", "Riga", new DateTime(2026, 11, 21, 0, 0, 0, DateTimeKind.Utc)), Mapping.RoundTrip());

        string script = Mapping.CreateScript();
        Assert.Contains("\"ShippingAddress_City\"", script);
        Assert.Contains("\"LastModified\"", script);
    }
}
