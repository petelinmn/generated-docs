using DesignProcess.Examples;

public class CapacityEstimateTests
{
    private static readonly Workload Chat = new(
        DailyActiveUsers: 10_000_000, WritesPerUserPerDay: 20, ReadsPerWrite: 5,
        BytesPerWrite: 500, PeakFactor: 3, RetentionDays: 365, ReplicationFactor: 3);

    [Fact]
    public void Estimates_traffic_and_storage()
    {
        Capacity c = CapacityEstimator.Estimate(Chat);

        Assert.Equal(2314.8, c.AverageWriteQps, 1);
        Assert.Equal(6944.4, c.PeakWriteQps, 1);
        Assert.Equal(34722.2, c.PeakReadQps, 1);
        Assert.Equal(109_500_000_000_000, c.StorageBytes);
        Assert.Equal(17_361_111, c.PeakEgressBytesPerSecond, 0);
    }

    [Fact]
    public void Littles_law() =>
        Assert.Equal(400, CapacityEstimator.InFlight(2000, TimeSpan.FromMilliseconds(200)), 6);
}

public class AvailabilityTests
{
    [Fact]
    public void Serial_dependencies_lower_availability() =>
        Assert.Equal(0.997002999, Availability.Serial(0.999, 0.999, 0.999), 9);

    [Fact]
    public void Redundancy_raises_availability() =>
        Assert.Equal(0.9999, Availability.Parallel(0.99, 0.99), 9);

    [Fact]
    public void Downtime_per_30_days() =>
        Assert.Equal(43.2, Availability.AllowedDowntime(0.999, TimeSpan.FromDays(30)).TotalMinutes, 3);

    [Fact]
    public void Error_budget() =>
        Assert.Equal(0.5, Availability.BudgetSpent(0.999, 1_000_000, 500), 9);
}

public class PercentilesTests
{
    // 98 fast requests and 2 very slow ones.
    private static readonly double[] Samples = [.. Enumerable.Repeat(10.0, 98), 2000, 2000];

    [Fact]
    public void Average_hides_the_tail()
    {
        Assert.Equal(49.8, Samples.Average(), 6);
        Assert.Equal(10, Latency.Percentile(Samples, 50));
        Assert.Equal(2000, Latency.Percentile(Samples, 99));
    }

    [Fact]
    public void Fan_out_amplifies_the_tail() =>
        Assert.Equal(0.634, Latency.ChanceOfSlowRequest(0.01, 100), 3);
}
