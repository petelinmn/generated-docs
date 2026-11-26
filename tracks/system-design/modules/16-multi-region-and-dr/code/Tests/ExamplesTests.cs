using MultiRegion.Examples;

public class RecoveryTests
{
    private static TimeSpan S(int seconds) => TimeSpan.FromSeconds(seconds);

    private static readonly Write[] Log =
    [
        new(S(10), "a", "1"),
        new(S(55), "b", "2"),
        new(S(58), "a", null), // an accidental delete
    ];

    [Fact]
    public void Writes_inside_the_lag_are_lost()
    {
        Assert.Equal(["b", "a"], Recovery.LostOnFailover(Log, S(60), S(6)).Select(w => w.Key));
        // A write that is exactly as old as the lag has already arrived.
        Assert.Equal(["a"], Recovery.LostOnFailover(Log, S(60), S(5)).Select(w => w.Key));
    }

    [Fact]
    public void Downtime_is_the_sum_of_the_steps() =>
        Assert.Equal(S(900), Recovery.Downtime(S(60), S(600), S(180), S(60)));

    [Fact]
    public void A_replica_repeats_the_delete_and_point_in_time_recovery_does_not()
    {
        Assert.False(Recovery.StateAt(Log, S(60)).ContainsKey("a"));
        Assert.True(Recovery.StateAt(Log, S(57)).ContainsKey("a"));
    }
}

public class FailoverTests
{
    [Fact]
    public void Switches_after_a_streak_and_returns_after_a_longer_one()
    {
        var c = new FailoverController("eu", "us", failAfter: 3, recoverAfter: 5);
        foreach (bool ok in new[] { false, false, true, false, false }) c.Probe(ok);
        Assert.Equal("eu", c.Active);

        c.Probe(false);
        Assert.Equal("us", c.Active);

        for (int i = 0; i < 4; i++) c.Probe(true);
        Assert.Equal("us", c.Active);
        c.Probe(true);
        Assert.Equal("eu", c.Active);
    }
}

public class ActiveActiveTests
{
    [Fact]
    public void Last_write_wins_drops_a_concurrent_item()
    {
        var merged = LastWriteWins.Merge((1, ["book", "pen"]), (2, ["book", "mug"]));

        Assert.DoesNotContain("pen", merged.Items);
    }

    [Fact]
    public void Observed_remove_set_keeps_a_concurrent_add()
    {
        var eu = new ObservedRemoveSet();
        var us = new ObservedRemoveSet();
        eu.Add("book");
        us.Merge(eu);

        eu.Remove("book");
        us.Add("book"); // concurrent with the removal
        us.Add("mug");
        eu.Merge(us);
        us.Merge(eu);

        Assert.Equal(["book", "mug"], eu.Items);
        Assert.Equal(eu.Items, us.Items);
    }
}
