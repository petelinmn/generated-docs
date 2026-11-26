using MultiRegion.Solutions;

public class SolutionsTests
{
    [Fact]
    public void Routing_respects_residency_before_latency()
    {
        Region[] regions = [new("us", "US", 10, true), new("eu1", "EU", 80, false), new("eu2", "EU", 120, true)];

        Assert.Equal("eu2", Exercises.Route(regions, "EU")?.Name);
        Assert.Null(Exercises.Route(regions, "CH"));
    }

    [Theory]
    [InlineData(1440, Strategy.BackupAndRestore)]
    [InlineData(30, Strategy.PilotLight)]
    [InlineData(1, Strategy.MultiSite)]
    public void Cheapest_strategy(int rtoMinutes, Strategy expected)
    {
        var achievable = new Dictionary<Strategy, TimeSpan> // assumed numbers for one imaginary system
        {
            [Strategy.BackupAndRestore] = TimeSpan.FromHours(8),
            [Strategy.PilotLight] = TimeSpan.FromMinutes(30),
            [Strategy.WarmStandby] = TimeSpan.FromMinutes(5),
            [Strategy.MultiSite] = TimeSpan.FromSeconds(30),
        };

        Assert.Equal(expected, Exercises.Cheapest(TimeSpan.FromMinutes(rtoMinutes), achievable));
    }

    [Fact]
    public void Retention()
    {
        DateOnly[] backups = [new(2026, 1, 30), new(2026, 1, 31), new(2026, 2, 27), new(2026, 2, 28), new(2026, 3, 1)];

        Assert.Equal(
            [new(2026, 1, 31), new(2026, 2, 28), new(2026, 3, 1)],
            Exercises.Keep(backups, daily: 2, monthly: 3).Order());
    }
}
