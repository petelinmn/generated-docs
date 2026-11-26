using CachingDesign.Solutions;

public class SolutionsTests
{
    [Fact]
    public void Origin_load() => Assert.Equal(100, Exercises.OriginQps(10_000, 0.99), 6);

    [Theory]
    [InlineData(9, Freshness.Fresh)]
    [InlineData(10, Freshness.Stale)]
    [InlineData(15, Freshness.Missing)]
    public void Classify(int ageSeconds, Freshness expected) =>
        Assert.Equal(expected, Exercises.Classify(
            TimeSpan.FromSeconds(ageSeconds), TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(5)));

    [Fact]
    public void Older_version_does_not_overwrite()
    {
        var cache = new VersionedCache<int, string>();
        cache.Set(1, 2, "new");
        cache.Set(1, 1, "old");
        Assert.Equal("new", cache.Get(1));
    }
}
