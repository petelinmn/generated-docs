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

public class QueueingTests
{
    [Theory]
    [InlineData(0.5, 20)]
    [InlineData(0.8, 50)]
    [InlineData(0.9, 100)]
    [InlineData(0.99, 1000)]
    public void Response_time_explodes_near_full_utilization(double utilization, double expected) =>
        Assert.Equal(expected, Queueing.MeanResponse(10, utilization), 6);

    [Fact]
    public void A_shared_queue_over_many_servers_waits_less()
    {
        Assert.Equal(0.8, Queueing.ProbabilityOfWaiting(1, 0.8), 9);
        Assert.Equal(40, Queueing.MeanWait(1, 0.8, 10), 6);
        Assert.Equal(0.409, Queueing.ProbabilityOfWaiting(10, 8), 3);
        Assert.Equal(2.05, Queueing.MeanWait(10, 8, 10), 2);
    }

    [Fact]
    public void Faster_repair_raises_availability()
    {
        var month = TimeSpan.FromDays(30);

        Assert.Equal(0.99861, Queueing.Availability(month, TimeSpan.FromHours(1)), 5);
        Assert.Equal(0.99988, Queueing.Availability(month, TimeSpan.FromMinutes(5)), 5);
    }
}

public class LoadModelTests
{
    [Fact]
    public void Closed_model_hides_the_stall()
    {
        // 1000 requests every 10 ms, 1 ms of work, a 1 s pause in the middle.
        var closed = LoadModels.Closed(1000, 10, 1, stallAt: 5000, stall: 1000);
        var open = LoadModels.Open(1000, 10, 1, stallAt: 5000, stall: 1000);

        Assert.Equal(1, closed.Count(l => l > 100));
        Assert.Equal(1, Latency.Percentile(closed, 99));
        Assert.Equal(101, open.Count(l => l > 100));
        Assert.Equal(911, Latency.Percentile(open, 99));
    }
}
