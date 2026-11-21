// Example: mapping beyond simple columns — value converters, complex types, shadow properties.
// Module: csharp/15-ef-core (expert)
// Tests: dotnet test --filter "FullyQualifiedName~Mapping"
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Examples;

public enum OrderStatus { New, Paid, Shipped }

// A complex type: a value object without its own identity, stored in the owner's columns.
public record Address(string City, string Street);

public class Order
{
    public int Id { get; set; }

    public OrderStatus Status { get; set; }

    public required Address ShippingAddress { get; set; }
}

public class ShopContext(DbContextOptions<ShopContext> options) : DbContext(options)
{
    public DbSet<Order> Orders => Set<Order>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>(order =>
        {
            // Value converter: store the enum as readable text instead of an integer.
            order.Property(o => o.Status).HasConversion<string>().HasMaxLength(20);
            order.ComplexProperty(o => o.ShippingAddress);
            // Shadow property: exists in the model and the table, but not in the CLR class.
            order.Property<DateTime>("LastModified");
        });
    }

    public override int SaveChanges()
    {
        foreach (var entry in ChangeTracker.Entries<Order>())
        {
            if (entry.State is EntityState.Added or EntityState.Modified)
            {
                entry.Property("LastModified").CurrentValue = new DateTime(2026, 11, 21, 0, 0, 0, DateTimeKind.Utc);
            }
        }
        return base.SaveChanges();
    }
}

public static class Mapping
{
    public static string CreateScript()
    {
        using var connection = new SqliteConnection("DataSource=:memory:");
        using var context = new ShopContext(new DbContextOptionsBuilder<ShopContext>().UseSqlite(connection).Options);
        return context.Database.GenerateCreateScript();
    }

    // Reads the raw column value with plain ADO.NET to show what is actually stored.
    public static (string StoredStatus, string City, DateTime LastModified) RoundTrip()
    {
        using var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        var options = new DbContextOptionsBuilder<ShopContext>().UseSqlite(connection).Options;
        using (var context = new ShopContext(options))
        {
            context.Database.EnsureCreated();
            context.Orders.Add(new Order { Status = OrderStatus.Paid, ShippingAddress = new Address("Riga", "Brivibas 1") });
            context.SaveChanges();
        }
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT Status FROM Orders";
        string stored = (string)command.ExecuteScalar()!;

        using var read = new ShopContext(options);
        Order order = read.Orders.Single();
        DateTime lastModified = read.Entry(order).Property<DateTime>("LastModified").CurrentValue;
        return (stored, order.ShippingAddress.City, lastModified);
    }
}
