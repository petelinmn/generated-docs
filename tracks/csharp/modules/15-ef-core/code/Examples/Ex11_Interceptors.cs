// Example: a SaveChanges interceptor that fills audit fields, with an injectable clock for tests.
// Module: csharp/15-ef-core (expert)
// Tests: dotnet test --filter "FullyQualifiedName~Interceptors"
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Examples;

public sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
{
    public override DateTimeOffset GetUtcNow() => now;
}

// Runs inside SaveChanges, before EF builds the SQL, so the changed values are saved in the same batch.
public sealed class AuditInterceptor(TimeProvider clock) : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        Stamp(eventData.Context);
        return result;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        Stamp(eventData.Context);
        return ValueTask.FromResult(result);
    }

    private void Stamp(DbContext? context)
    {
        if (context is null)
        {
            return;
        }
        foreach (var entry in context.ChangeTracker.Entries<Post>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = clock.GetUtcNow();
            }
        }
    }
}
