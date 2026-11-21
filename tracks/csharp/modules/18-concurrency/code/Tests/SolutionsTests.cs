using Solutions;

namespace Tests;

public class S01_SafeStatisticsTests
{
    [Fact]
    public void ParallelAdds_GiveAConsistentSnapshot()
    {
        var stats = new SafeStatistics();

        Parallel.For(1, 10_001, i => stats.Add(i));

        Assert.Equal((10_000L, 10_000L * 10_001 / 2, 10_000L), stats.Snapshot());
    }
}

public class S02_OnceInitializerTests
{
    [Fact]
    public void Factory_RunsOnce_EvenUnderContention()
    {
        int calls = 0;
        var once = new OnceInitializer<object>(() =>
        {
            Interlocked.Increment(ref calls);
            return new object();
        });
        using var start = new Barrier(8);
        var seen = new object?[8];

        var threads = Enumerable.Range(0, 8).Select(i => new Thread(() =>
        {
            start.SignalAndWait(); // all threads ask for the value at the same moment
            seen[i] = once.Value;
        })).ToList();
        threads.ForEach(t => t.Start());
        threads.ForEach(t => t.Join());

        Assert.Equal(1, calls);
        Assert.All(seen, value => Assert.Same(seen[0], value));
    }
}

public class S03_KeyedLockTests
{
    [Fact]
    public async Task SameKey_RunsOneAtATime()
    {
        var keyed = new KeyedLock();
        int running = 0, peak = 0;

        async Task<int> Work()
        {
            int now = Interlocked.Increment(ref running);
            AtomicMath.InterlockedMax(ref peak, now);
            await Task.Delay(5);
            Interlocked.Decrement(ref running);
            return now;
        }

        await Task.WhenAll(Enumerable.Range(0, 20).Select(_ => keyed.RunAsync("same", Work)));

        Assert.Equal(1, peak);
    }
}

public class S04_AtomicMathTests
{
    [Fact]
    public void InterlockedMax_KeepsTheLargestValue()
    {
        int max = int.MinValue;

        Parallel.For(-5_000, 5_001, i => AtomicMath.InterlockedMax(ref max, i));

        Assert.Equal(5_000, max);
    }

    [Fact]
    public void InterlockedMax_ReturnsTheStoredMaximum()
    {
        int location = 10;

        Assert.Equal(10, AtomicMath.InterlockedMax(ref location, 3));
        Assert.Equal(20, AtomicMath.InterlockedMax(ref location, 20));
        Assert.Equal(20, location);
    }
}
