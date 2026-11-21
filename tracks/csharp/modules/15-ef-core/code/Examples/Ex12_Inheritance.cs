// Example: three ways to map an inheritance hierarchy — TPH, TPT, TPC — and the schema each one creates.
// Module: csharp/15-ef-core (expert)
// Tests: dotnet test --filter "FullyQualifiedName~Inheritance"
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Examples;

public abstract class Payment
{
    public Guid Id { get; set; }   // Guid keys: SQLite cannot generate integer keys for TPC

    public decimal Amount { get; set; }
}

public class CardPayment : Payment
{
    public required string CardLast4 { get; set; }
}

public class CashPayment : Payment
{
    public decimal Change { get; set; }
}

// One context type per strategy: EF caches the model per context type.
public abstract class PaymentsContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<Payment> Payments => Set<Payment>();

    protected abstract void MapHierarchy(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<Payment> payment);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        MapHierarchy(modelBuilder.Entity<Payment>());
        modelBuilder.Entity<CardPayment>();
        modelBuilder.Entity<CashPayment>();
    }
}

// Table per hierarchy (the default): one table, a discriminator column, nullable columns for subtype data.
public sealed class TphContext(DbContextOptions options) : PaymentsContext(options)
{
    protected override void MapHierarchy(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<Payment> payment) =>
        payment.UseTphMappingStrategy();
}

// Table per type: a table for the base type plus a table per subtype, joined by key.
public sealed class TptContext(DbContextOptions options) : PaymentsContext(options)
{
    protected override void MapHierarchy(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<Payment> payment) =>
        payment.UseTptMappingStrategy();
}

// Table per concrete type: a full table per non-abstract type, no table for the abstract base.
public sealed class TpcContext(DbContextOptions options) : PaymentsContext(options)
{
    protected override void MapHierarchy(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<Payment> payment) =>
        payment.UseTpcMappingStrategy();
}

public static class Inheritance
{
    public static string CreateScript<TContext>(Func<DbContextOptions, TContext> create)
        where TContext : PaymentsContext
    {
        using var connection = new SqliteConnection("DataSource=:memory:");
        using TContext context = create(Options(connection));
        return context.Database.GenerateCreateScript();
    }

    // Polymorphic query: ask for the base type, get instances of the derived types.
    public static (int Cards, int Cash) RoundTrip<TContext>(Func<DbContextOptions, TContext> create)
        where TContext : PaymentsContext
    {
        using var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        using (TContext context = create(Options(connection)))
        {
            context.Database.EnsureCreated();
            context.Payments.Add(new CardPayment { Id = Guid.NewGuid(), Amount = 10, CardLast4 = "4242" });
            context.Payments.Add(new CashPayment { Id = Guid.NewGuid(), Amount = 5, Change = 1 });
            context.SaveChanges();
        }
        using (TContext context = create(Options(connection)))
        {
            List<Payment> all = context.Payments.ToList();
            return (all.OfType<CardPayment>().Count(), all.OfType<CashPayment>().Count());
        }
    }

    private static DbContextOptions Options(SqliteConnection connection) =>
        new DbContextOptionsBuilder().UseSqlite(connection).Options;
}
