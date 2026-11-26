using CachingDesign.Examples;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;

public class CacheAsideTests
{
    private readonly FakeTimeProvider _clock = new();
    private readonly ProductStore _store = new();
    private readonly PriceService _service;

    public CacheAsideTests() => _service = new PriceService(_store, new TtlCache<int, decimal>(_clock));

    [Fact]
    public void Second_read_is_a_hit_until_the_ttl_expires()
    {
        _service.GetPrice(1);
        _service.GetPrice(1);
        Assert.Equal(1, _store.Reads);

        _clock.Advance(TimeSpan.FromMinutes(5));
        _service.GetPrice(1);
        Assert.Equal(2, _store.Reads);
    }

    [Fact]
    public void Write_invalidates()
    {
        _service.GetPrice(1);
        _service.SetPrice(1, 10m);
        Assert.Equal(10m, _service.GetPrice(1));
    }

    [Fact]
    public void Write_that_bypasses_the_service_stays_invisible()
    {
        _service.GetPrice(1);
        _store.Write(1, 10m);
        Assert.Equal(0m, _service.GetPrice(1));
    }
}

public class StampedeTests
{
    [Fact]
    public async Task Single_flight_loads_once()
    {
        var flights = new SingleFlight<string, int>();
        var gate = new TaskCompletionSource<int>();
        int loads = 0;

        Task<int>[] callers = [.. Enumerable.Range(0, 10)
            .Select(_ => flights.RunAsync("k", () => { loads++; return gate.Task; }))];
        gate.SetResult(7);

        Assert.All(await Task.WhenAll(callers), v => Assert.Equal(7, v));
        Assert.Equal(1, loads);
    }

    [Fact]
    public async Task Single_flight_does_not_cache_failures()
    {
        var flights = new SingleFlight<string, int>();
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => flights.RunAsync("k", () => Task.FromException<int>(new InvalidOperationException())));

        Assert.Equal(1, await flights.RunAsync("k", () => Task.FromResult(1)));
    }

    [Fact]
    public void Jitter_stays_within_bounds()
    {
        var random = new Random(1);
        for (int i = 0; i < 1000; i++)
            Assert.InRange(Ttl.WithJitter(TimeSpan.FromSeconds(100), 0.1, random).TotalSeconds, 90, 110);
    }
}

public class LruTests
{
    [Fact]
    public void Lru_evicts_the_least_recently_used()
    {
        var cache = new LruCache<string, int>(2);
        cache.Set("a", 1);
        cache.Set("b", 2);
        cache.TryGet("a", out _);
        cache.Set("c", 3);

        Assert.True(cache.TryGet("a", out _));
        Assert.False(cache.TryGet("b", out _));
        Assert.True(cache.TryGet("c", out _));
    }
}

public class HybridCacheTests
{
    [Fact]
    public async Task Hybrid_coalesces_misses_and_invalidates_by_tag()
    {
        var ct = TestContext.Current.CancellationToken;
        await using var provider = HybridCacheFactory.Build();
        var catalog = new CatalogService(provider.GetRequiredService<HybridCache>());
        var gate = new TaskCompletionSource();

        Task<string>[] callers = [.. Enumerable.Range(0, 10)
            .Select(_ => catalog.GetNameAsync(1, gate.Task, ct).AsTask())];
        gate.SetResult();
        await Task.WhenAll(callers);
        Assert.Equal(1, catalog.Loads);

        await catalog.GetNameAsync(1, Task.CompletedTask, ct);
        Assert.Equal(1, catalog.Loads);

        await catalog.InvalidateAllAsync(ct);
        await catalog.GetNameAsync(1, Task.CompletedTask, ct);
        Assert.Equal(2, catalog.Loads);
    }
}
