using Messaging.Solutions;

public class SolutionsTests
{
    [Fact]
    public void Same_key_same_partition()
    {
        Assert.Equal(Exercises.PartitionFor("order-1", 8), Exercises.PartitionFor("order-1", 8));
        Assert.All(Enumerable.Range(0, 100), i => Assert.InRange(Exercises.PartitionFor($"{i}", 8), 0, 7));
    }

    [Fact]
    public void Deduplicator_forgets_old_ids()
    {
        var dedup = new Deduplicator(window: 2);

        Assert.True(dedup.IsNew("a"));
        Assert.False(dedup.IsNew("a"));
        dedup.IsNew("b");
        dedup.IsNew("c");
        Assert.True(dedup.IsNew("a")); // fell out of the window
    }

    [Fact]
    public void Resequencer_waits_for_the_gap()
    {
        var r = new Resequencer<string>();

        Assert.Empty(r.Add(2, "b"));
        Assert.Equal(["a", "b"], r.Add(1, "a"));
        Assert.Empty(r.Add(1, "a"));
        Assert.Equal(["c"], r.Add(3, "c"));
    }
}
