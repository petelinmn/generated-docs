using Microsoft.Extensions.Time.Testing;
using Polly;
using Polly.CircuitBreaker;
using Polly.Timeout;
using Reliability.Examples;

public class PipelineTests
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Retry_repeats_only_transient_failures()
    {
        var retry = Pipelines.Retry(3, TimeSpan.Zero, TimeProvider.System);
        int calls = 0;

        await Assert.ThrowsAsync<TransientException>(
            async () => await retry.ExecuteAsync(_ => { calls++; throw new TransientException(); }, Ct));
        Assert.Equal(4, calls); // 1 call + 3 retries

        calls = 0;
        await Assert.ThrowsAsync<ArgumentException>(
            async () => await retry.ExecuteAsync(_ => { calls++; throw new ArgumentException(); }, Ct));
        Assert.Equal(1, calls);
    }

    [Fact]
    public async Task Timeout_cancels_the_token()
    {
        var clock = new FakeTimeProvider();
        var timeout = Pipelines.Timeout(TimeSpan.FromSeconds(2), clock);

        Task call = timeout.ExecuteAsync(
            async token => await Task.Delay(Timeout.InfiniteTimeSpan, token), Ct).AsTask();
        clock.Advance(TimeSpan.FromSeconds(2));

        await Assert.ThrowsAsync<TimeoutRejectedException>(() => call);
    }

    [Fact]
    public async Task Breaker_opens_then_lets_a_probe_through()
    {
        var clock = new FakeTimeProvider();
        var breaker = Pipelines.Breaker(clock);
        int calls = 0;
        ValueTask Fail() => breaker.ExecuteAsync(_ => { calls++; throw new TransientException(); }, Ct);

        for (int i = 0; i < 4; i++)
            await Assert.ThrowsAsync<TransientException>(async () => await Fail());

        await Assert.ThrowsAsync<BrokenCircuitException>(async () => await Fail());
        Assert.Equal(4, calls); // the dependency was not called

        clock.Advance(TimeSpan.FromSeconds(5));
        await breaker.ExecuteAsync(_ => ValueTask.CompletedTask, Ct); // half-open probe succeeds
        await Assert.ThrowsAsync<TransientException>(async () => await Fail()); // closed again
    }
}

public class LimitsTests
{
    [Fact]
    public void Token_bucket_allows_a_burst_then_rejects()
    {
        using var limiter = Limits.TokenBucket(burst: 3, perPeriod: 1);

        int accepted = Enumerable.Range(0, 10).Count(_ => limiter.AttemptAcquire().IsAcquired);

        Assert.Equal(3, accepted);
    }

    [Fact]
    public async Task Bulkhead_sheds_the_extra_call_and_releases_the_slot()
    {
        using var bulkhead = Limits.Bulkhead(1);
        var gate = new TaskCompletionSource<int>();

        var first = Limits.RunOrShedAsync(bulkhead, () => gate.Task);
        var second = await Limits.RunOrShedAsync(bulkhead, () => Task.FromResult(2));
        Assert.False(second.Accepted);

        gate.SetResult(1);
        Assert.Equal((true, 1), await first);
        Assert.True((await Limits.RunOrShedAsync(bulkhead, () => Task.FromResult(3))).Accepted);
    }
}

public class AmplificationTests
{
    [Theory]
    [InlineData(4, 1, 4)]
    [InlineData(4, 3, 64)]
    [InlineData(3, 5, 243)]
    public void Retries_multiply(int attempts, int layers, int expected) =>
        Assert.Equal(expected, Amplification.CallsAtTheBottom(attempts, layers));
}

public class AdaptiveTests
{
    [Fact]
    public void Grows_slowly_and_shrinks_fast()
    {
        var limit = new AimdLimit(initial: 20, min: 1, max: 100, latencyTarget: TimeSpan.FromMilliseconds(100));

        for (int i = 0; i < 21; i++) limit.Record(TimeSpan.FromMilliseconds(10), failed: false);
        Assert.Equal(21, limit.Current);

        limit.Record(TimeSpan.FromSeconds(1), failed: false);
        Assert.Equal(10, limit.Current);
        for (int i = 0; i < 10; i++) limit.Record(TimeSpan.Zero, failed: true);
        Assert.Equal(1, limit.Current);
    }
}

public class HedgingTests
{
    [Fact]
    public async Task A_second_attempt_wins_when_the_first_is_slow()
    {
        var clock = new FakeTimeProvider();
        var pipeline = Hedged.Pipeline(TimeSpan.FromSeconds(1), clock);
        int calls = 0;

        Task<string> call = pipeline.ExecuteAsync(async token =>
        {
            if (Interlocked.Increment(ref calls) > 1) return "second";
            await Task.Delay(Timeout.InfiniteTimeSpan, token);
            return "first";
        }, TestContext.Current.CancellationToken).AsTask();

        Assert.Equal(1, calls);
        clock.Advance(TimeSpan.FromSeconds(1));

        Assert.Equal("second", await call);
        Assert.Equal(2, calls); // both attempts reached the dependency
    }
}

public class ChaosTests
{
    [Theory]
    [InlineData(1.0, true, true)]
    [InlineData(1.0, false, false)]
    [InlineData(0.0, true, false)]
    public async Task Faults_are_injected_only_when_enabled(double rate, bool enabled, bool fails)
    {
        var pipeline = Chaos.Faults(rate, enabled);
        bool reached = false;
        ValueTask Call() => pipeline.ExecuteAsync(
            _ => { reached = true; return ValueTask.CompletedTask; }, TestContext.Current.CancellationToken);

        if (fails) await Assert.ThrowsAsync<TransientException>(async () => await Call());
        else await Call();

        Assert.Equal(!fails, reached);
    }
}

public class RolloutTests
{
    private static readonly string[] Users = [.. Enumerable.Range(0, 10_000).Select(i => $"u{i}")];

    [Fact]
    public void Share_is_close_and_raising_it_keeps_earlier_users()
    {
        string[] at10 = [.. Users.Where(u => Rollout.IsEnabled("checkout", u, 10))];
        string[] at50 = [.. Users.Where(u => Rollout.IsEnabled("checkout", u, 50))];

        Assert.InRange(at10.Length, 900, 1100);
        Assert.Empty(at10.Except(at50));
        Assert.NotEqual(at10, Users.Where(u => Rollout.IsEnabled("search", u, 10)));
    }
}
