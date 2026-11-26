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

public class BackplaneTests
{
    [Fact]
    public void A_lost_message_is_covered_only_by_the_ttl()
    {
        var clock = new FakeTimeProvider();
        string source = "v1";
        var a = new LocalCache(clock, TimeSpan.FromMinutes(1), _ => source);
        var b = new LocalCache(clock, TimeSpan.FromMinutes(1), _ => source);
        var backplane = new Backplane();
        backplane.Subscribe(a);
        backplane.Subscribe(b);
        a.Get("k");
        b.Get("k");

        source = "v2";
        backplane.Publish("k");
        Assert.Equal(("v2", "v2"), (a.Get("k"), b.Get("k")));

        backplane.Unsubscribe(b); // a network break
        source = "v3";
        backplane.Publish("k");
        backplane.Subscribe(b);   // reconnected, the message is gone
        Assert.Equal(("v3", "v2"), (a.Get("k"), b.Get("k")));

        clock.Advance(TimeSpan.FromMinutes(1));
        Assert.Equal("v3", b.Get("k"));
    }
}

public class HashSlotTests
{
    [Fact]
    public void Crc_check_value() => Assert.Equal(0x31C3, HashSlots.Crc16("123456789"u8));

    [Fact]
    public void Hash_tags_put_related_keys_into_one_slot()
    {
        Assert.Equal(HashSlots.SlotOf("{user1000}.following"), HashSlots.SlotOf("{user1000}.followers"));
        Assert.Equal(HashSlots.SlotOf("bar"), HashSlots.SlotOf("foo{bar}{zap}"));
        Assert.Equal(HashSlots.SlotOf("{bar"), HashSlots.SlotOf("foo{{bar}}zap"));
        Assert.Equal(HashSlots.Crc16("foo{}{bar}"u8) % HashSlots.Count, HashSlots.SlotOf("foo{}{bar}"));
    }
}

public class WarmingTests
{
    [Fact]
    public async Task Warming_limits_concurrency_and_reports_failures()
    {
        int running = 0, peak = 0;
        var gate = new SemaphoreSlim(0);
        var lockObject = new Lock();

        Task<IReadOnlyCollection<string>> warming = Warming.WarmAsync(
            Enumerable.Range(0, 10).Select(i => $"{i}"),
            async (key, token) =>
            {
                lock (lockObject) peak = Math.Max(peak, ++running);
                await gate.WaitAsync(token);
                lock (lockObject) running--;
                if (key == "7") throw new InvalidOperationException();
            },
            maxConcurrency: 3,
            TestContext.Current.CancellationToken);
        gate.Release(10);

        Assert.Equal(["7"], await warming);
        Assert.InRange(peak, 1, 3);
    }
}
