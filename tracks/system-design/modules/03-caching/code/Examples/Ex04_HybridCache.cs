// Example: HybridCache coalesces concurrent misses and invalidates by tag.
// Module: system-design/03-caching (basics, advanced)
// Tests: dotnet test --filter "FullyQualifiedName~Hybrid"
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;

namespace CachingDesign.Examples;

public static class HybridCacheFactory
{
    public static ServiceProvider Build()
    {
        var services = new ServiceCollection();
        services.AddHybridCache(o => o.DefaultEntryOptions = new HybridCacheEntryOptions
        {
            Expiration = TimeSpan.FromMinutes(5),           // overall (L2) lifetime
            LocalCacheExpiration = TimeSpan.FromMinutes(1), // in-process (L1) lifetime
        });
        return services.BuildServiceProvider();
    }
}

public sealed class CatalogService(HybridCache cache)
{
    private int _loads;
    public int Loads => _loads;

    public ValueTask<string> GetNameAsync(int id, Task gate, CancellationToken ct = default) =>
        cache.GetOrCreateAsync(
            $"product:{id}",
            async _ =>
            {
                Interlocked.Increment(ref _loads);
                await gate; // stands in for a slow database query
                return $"Product {id}";
            },
            tags: ["products"],
            cancellationToken: ct);

    public ValueTask InvalidateAllAsync(CancellationToken ct = default) => cache.RemoveByTagAsync("products", ct);
}
