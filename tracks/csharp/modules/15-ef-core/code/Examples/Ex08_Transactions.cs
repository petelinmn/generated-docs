// Example: SaveChanges is atomic; an explicit transaction spans several SaveChanges calls.
// Module: csharp/15-ef-core (advanced)
// Tests: dotnet test --filter "FullyQualifiedName~Transactions"
using Microsoft.EntityFrameworkCore;

namespace Examples;

public static class Transactions
{
    // The second blog violates the unique index on Name, so the first one is rolled back too.
    public static (Type Error, int BlogsInDatabase) SaveChangesIsAtomic(TestDatabase db)
    {
        using BloggingContext context = db.CreateContext();
        context.Blogs.Add(new Blog { Name = "Unique" });
        context.Blogs.Add(new Blog { Name = "Unique" });
        Type error = typeof(void);
        try
        {
            context.SaveChanges();
        }
        catch (DbUpdateException e)
        {
            error = e.GetType();
        }
        using BloggingContext check = db.CreateContext();
        return (error, check.Blogs.Count(b => b.Name == "Unique"));
    }

    // Two SaveChanges in one explicit transaction: nothing is kept if we roll back.
    public static int ExplicitTransactionRollback(TestDatabase db)
    {
        using (BloggingContext context = db.CreateContext())
        using (var transaction = context.Database.BeginTransaction())
        {
            context.Blogs.Add(new Blog { Name = "Tx 1" });
            context.SaveChanges();
            context.Blogs.Add(new Blog { Name = "Tx 2" });
            context.SaveChanges();
            transaction.Rollback();
        }
        using BloggingContext check = db.CreateContext();
        return check.Blogs.Count(b => b.Name.StartsWith("Tx "));
    }

    // Inside an explicit transaction, a failed SaveChanges rolls back to its savepoint only.
    public static (bool SecondFailed, int Committed) SavepointKeepsEarlierWork(TestDatabase db)
    {
        bool secondFailed = false;
        using (BloggingContext context = db.CreateContext())
        using (var transaction = context.Database.BeginTransaction())
        {
            context.Blogs.Add(new Blog { Name = "Kept" });
            context.SaveChanges();

            var duplicate = new Blog { Name = "Kept" };
            context.Blogs.Add(duplicate);
            try
            {
                context.SaveChanges();
            }
            catch (DbUpdateException)
            {
                secondFailed = true;
                context.Entry(duplicate).State = EntityState.Detached;
            }
            transaction.Commit();
        }
        using BloggingContext check = db.CreateContext();
        return (secondFailed, check.Blogs.Count(b => b.Name == "Kept"));
    }
}
