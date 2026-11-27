using Microsoft.Extensions.Time.Testing;
using RateLimiting.Solutions;

namespace RateLimiting.Tests;

public class SolutionsTests
{
    [Fact]
    public void Solutions_SlidingWindowHasNoDoubleBurstAtTheBoundary()
    {
        var clock = new FakeTimeProvider(new DateTimeOffset(2026, 1, 1, 0, 0, 59, TimeSpan.Zero));
        var window = new SlidingWindow(10, TimeSpan.FromMinutes(1), clock);

        Assert.Equal(10, Enumerable.Range(0, 20).Count(_ => window.TryAcquire()));
        clock.Advance(TimeSpan.FromSeconds(2));
        Assert.Equal(0, Enumerable.Range(0, 20).Count(_ => window.TryAcquire()));
        clock.Advance(TimeSpan.FromSeconds(29)); // half of the new window has passed
        Assert.Equal(5, Enumerable.Range(0, 20).Count(_ => window.TryAcquire()));
    }

    [Fact]
    public void Solutions_RefusalConsumesNothing()
    {
        var limits = new AllOrNothing(10, 3);

        Assert.True(limits.TryAcquire(2));
        Assert.False(limits.TryAcquire(2));
        Assert.Equal([8, 1], limits.Left);
    }

    [Fact]
    public void Solutions_ShardsAddUp()
    {
        int[] parts = Shards.Split(100, 8);

        Assert.Equal(100, parts.Sum());
        Assert.Equal((12, 13), (parts.Min(), parts.Max()));
    }
}
