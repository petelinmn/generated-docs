using System.Diagnostics;
using System.Diagnostics.Metrics;
using Observability.Examples;

public class InstrumentationTests
{
    [Fact]
    public void Metrics_are_observed_by_a_listener()
    {
        const string name = "Test.Metrics";
        using var service = new CheckoutService(name);
        var seen = new List<(string Instrument, object? Outcome)>();
        using var listener = new MeterListener();
        listener.InstrumentPublished = (instrument, l) =>
        {
            if (instrument.Meter.Name == name) l.EnableMeasurementEvents(instrument);
        };
        listener.SetMeasurementEventCallback<long>((i, _, tags, _) => seen.Add((i.Name, tags[0].Value)));
        listener.SetMeasurementEventCallback<double>((i, _, tags, _) => seen.Add((i.Name, tags[0].Value)));
        listener.Start();

        service.Place(succeed: false, seconds: 0.2);

        Assert.Equal([("shop.checkout.orders", "error"), ("shop.checkout.duration", "error")], seen);
    }

    [Fact]
    public void Activity_is_null_without_a_listener_and_recorded_with_one()
    {
        const string name = "Test.Traces";
        using var service = new CheckoutService(name);
        Assert.Null(service.Source.StartActivity("PlaceOrder"));

        var stopped = new List<Activity>();
        using var listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == name,
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData,
            ActivityStopped = stopped.Add,
        };
        ActivitySource.AddActivityListener(listener);
        service.Place(succeed: false, seconds: 0.1);

        Assert.Equal(ActivityStatusCode.Error, Assert.Single(stopped).Status);
    }
}

public class HistogramTests
{
    [Fact]
    public void Merged_buckets_give_the_combined_quantile()
    {
        var fast = new Histogram(0.1, 0.5, 2);
        var slow = new Histogram(0.1, 0.5, 2);
        for (int i = 0; i < 990; i++) fast.Record(0.05);
        for (int i = 0; i < 10; i++) slow.Record(1.5);

        Assert.Equal(2, slow.QuantileUpperBound(0.99));
        fast.Merge(slow);
        Assert.Equal(0.1, fast.QuantileUpperBound(0.99));
        Assert.Equal(2, fast.QuantileUpperBound(0.999));
    }
}

public class ExemplarTests
{
    [Fact]
    public void The_slow_bucket_points_to_its_trace()
    {
        var histogram = new ExemplarHistogram(0.1, 1);
        histogram.Record(0.05); // no activity: no exemplar

        using var activity = new Activity("slow").Start();
        histogram.Record(3);

        Assert.Equal((3.0, activity.TraceId.ToHexString()), histogram.ExemplarOfSlowestBucket());
    }
}

public class SyntheticTests
{
    [Fact]
    public void Alerts_only_when_most_locations_fail()
    {
        Assert.False(SyntheticCheck.ShouldAlert([true, false, true]));
        Assert.True(SyntheticCheck.ShouldAlert([false, false, true]));
    }
}

public class LogSamplingTests
{
    [Theory]
    [InlineData(0.0, 1)]
    [InlineData(1.0, 2)]
    public void Warnings_are_kept_whatever_the_information_rate(double probability, int expected)
    {
        var written = LogSampling.Run(probability);

        Assert.Equal(expected, written.Count);
        Assert.Contains("payment is slow", written);
    }
}
