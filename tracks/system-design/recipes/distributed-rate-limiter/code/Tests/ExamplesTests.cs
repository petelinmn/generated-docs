using System.Net;
using Microsoft.Extensions.Time.Testing;
using RateLimiting.Examples;
using StackExchange.Redis;

namespace RateLimiting.Tests;

internal static class Clocks
{
    public static FakeTimeProvider Fixed() => new(new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero));
}

// A store that fails or never answers.
internal sealed class BrokenLimiter(bool hang) : ILimiter
{
    public ValueTask<Decision> AcquireAsync(string key, int cost = 1, CancellationToken cancellationToken = default) =>
        hang
            ? new ValueTask<Decision>(new TaskCompletionSource<Decision>().Task)
            : ValueTask.FromException<Decision>(new InvalidOperationException("store down"));
}

public class TokenBucketTests
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task TokenBucket_AllowsABurstThenTheRefillRate()
    {
        var clock = Clocks.Fixed();
        var limiter = new InProcessLimiter(new Limit(Capacity: 5, RefillPerSecond: 1), clock);

        for (int i = 0; i < 5; i++) Assert.True((await limiter.AcquireAsync("ann", cancellationToken: Ct)).Allowed);
        var refused = await limiter.AcquireAsync("ann", cancellationToken: Ct);
        clock.Advance(TimeSpan.FromSeconds(2));

        Assert.False(refused.Allowed);
        Assert.Equal(TimeSpan.FromSeconds(1), refused.RetryAfter);
        Assert.Equal(1, (await limiter.AcquireAsync("ann", cancellationToken: Ct)).Remaining);
        Assert.True((await limiter.AcquireAsync("bob", cancellationToken: Ct)).Allowed);
    }

    [Fact]
    public async Task TokenBucket_SeparateInstancesMultiplyTheLimit()
    {
        var clock = Clocks.Fixed();
        ILimiter[] instances = [.. Enumerable.Range(0, 3).Select(_ => new InProcessLimiter(new Limit(5, 1), clock))];
        int allowed = 0;

        for (int i = 0; i < 30; i++)
            if ((await instances[i % 3].AcquireAsync("ann", cancellationToken: Ct)).Allowed) allowed++;

        Assert.Equal(15, allowed);
    }

    [Fact]
    public void TokenBucket_ClockGoingBackwardsAddsNothing()
    {
        var now = Clocks.Fixed().GetUtcNow();

        var (decision, _) = TokenBucket.Take(new Bucket(0, now), new Limit(5, 1), now.AddSeconds(-10), 1);

        Assert.False(decision.Allowed);
    }
}

public class ResilientTests
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Theory]
    [InlineData(OnFailure.Allow, true)]
    [InlineData(OnFailure.Deny, false)]
    public async Task Resilient_FailurePolicy(OnFailure policy, bool allowed)
    {
        var limiter = new ResilientLimiter(new BrokenLimiter(hang: false), policy, TimeSpan.FromSeconds(1), TimeProvider.System);

        Assert.Equal(allowed, (await limiter.AcquireAsync("ann", cancellationToken: Ct)).Allowed);
        Assert.Equal(1, limiter.Failures);
    }

    [Fact]
    public async Task Resilient_ABadCostIsNotTreatedAsAStoreFailure()
    {
        var limiter = new ResilientLimiter(new InProcessLimiter(new Limit(5, 1), Clocks.Fixed()),
            OnFailure.Allow, TimeSpan.FromSeconds(1), TimeProvider.System);

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await limiter.AcquireAsync("ann", -5, Ct));
        Assert.Equal(0, limiter.Failures);
    }

    [Fact]
    public async Task Resilient_TimeoutUsesTheLocalFallback()
    {
        var clock = Clocks.Fixed();
        var limiter = new ResilientLimiter(new BrokenLimiter(hang: true), OnFailure.UseFallback,
            TimeSpan.FromMilliseconds(50), clock, new InProcessLimiter(new Limit(1, 1), clock));

        var first = limiter.AcquireAsync("ann", cancellationToken: Ct).AsTask();
        clock.Advance(TimeSpan.FromMilliseconds(50));
        Assert.True((await first).Allowed);

        var second = limiter.AcquireAsync("ann", cancellationToken: Ct).AsTask();
        clock.Advance(TimeSpan.FromMilliseconds(50));
        Assert.False((await second).Allowed);
    }
}

public class LeasedTests
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Leased_SavesCallsAndStrandsTokens()
    {
        var shared = new InProcessLimiter(new Limit(20, 1), Clocks.Fixed());
        var first = new LeasedLimiter(shared, batch: 10);
        var second = new LeasedLimiter(shared, batch: 10);

        for (int i = 0; i < 10; i++) Assert.True((await first.AcquireAsync("ann", cancellationToken: Ct)).Allowed);
        Assert.True((await second.AcquireAsync("ann", cancellationToken: Ct)).Allowed); // leases the other ten

        Assert.Equal(1, first.SharedCalls);
        // Nine tokens sit unused in the second instance, so the first one is refused.
        Assert.False((await first.AcquireAsync("ann", cancellationToken: Ct)).Allowed);
    }
}

public class MiddlewareTests
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Middleware_Answers429WithRetryAfter()
    {
        await using var app = RateLimitMiddleware.Build(new InProcessLimiter(new Limit(2, 0.1), Clocks.Fixed()));
        await app.StartAsync(Ct);
        using var client = new HttpClient { BaseAddress = new Uri(app.Urls.First()) };

        var first = await client.GetAsync("/", Ct);
        await client.GetAsync("/", Ct);
        var third = await client.GetAsync("/", Ct);

        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.Equal("1", first.Headers.GetValues("X-RateLimit-Remaining").Single());
        Assert.Equal(HttpStatusCode.TooManyRequests, third.StatusCode);
        Assert.Equal(TimeSpan.FromSeconds(10), third.Headers.RetryAfter!.Delta);
    }
}

// Integration tests: skipped when there is no Redis on 127.0.0.1:6379.
[Trait("Category", "integration")]
public class RedisTests
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private static async Task<ConnectionMultiplexer> Connect()
    {
        var redis = await ConnectionMultiplexer.ConnectAsync("127.0.0.1:6379,abortConnect=false,connectTimeout=1000");
        Assert.SkipUnless(redis.IsConnected, "Redis is not available on 127.0.0.1:6379");
        return redis;
    }

    // A fresh prefix per test keeps tests independent.
    private static string Prefix() => $"test:{Guid.NewGuid():N}:";

    [Fact]
    public async Task Redis_InstancesShareOneBucket()
    {
        await using var redis = await Connect();
        string prefix = Prefix();
        var slow = new Limit(Capacity: 5, RefillPerSecond: 0.001); // practically no refill during the test
        ILimiter[] instances = [.. Enumerable.Range(0, 3).Select(_ => new RedisLimiter(redis.GetDatabase(), slow, prefix))];

        var decisions = await Task.WhenAll(Enumerable.Range(0, 30)
            .Select(i => instances[i % 3].AcquireAsync("ann", cancellationToken: Ct).AsTask()));

        Assert.Equal(5, decisions.Count(d => d.Allowed));
        Assert.All(decisions.Where(d => !d.Allowed), d => Assert.True(d.RetryAfter > TimeSpan.FromSeconds(900)));
    }

    [Fact]
    public async Task Redis_IdleBucketsExpire()
    {
        await using var redis = await Connect();
        var database = redis.GetDatabase();
        string prefix = Prefix();

        await new RedisLimiter(database, new Limit(10, 1), prefix).AcquireAsync("ann", cancellationToken: Ct);
        TimeSpan? ttl = await database.KeyTimeToLiveAsync(prefix + "ann");

        Assert.NotNull(ttl);
        Assert.InRange(ttl.Value, TimeSpan.FromSeconds(9), TimeSpan.FromSeconds(11));
    }

    [Fact]
    public async Task Redis_LuaNumbersLoseTheFraction()
    {
        await using var redis = await Connect();

        var asNumber = await redis.GetDatabase().ScriptEvaluateAsync("return 3.9");
        var asString = await redis.GetDatabase().ScriptEvaluateAsync("return tostring(3.9)");

        Assert.Equal(3, (int)asNumber);
        Assert.Equal("3.9", (string?)asString);
    }

    [Fact]
    public void Redis_ClientTimeoutDefaults()
    {
        var options = ConfigurationOptions.Parse("localhost");

        Assert.Equal((5000, 5000, 5000), (options.ConnectTimeout, options.SyncTimeout, options.AsyncTimeout));
        Assert.True(options.AbortOnConnectFail);
    }
}
