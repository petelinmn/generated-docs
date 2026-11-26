using Replication.Examples;

public class ReplicationLagTests
{
    [Fact]
    public void Follower_is_stale_until_it_catches_up()
    {
        var leader = new Leader();
        var follower = new Follower(leader);
        leader.Write("name", "old");
        follower.Replicate();
        long position = leader.Write("name", "new");

        Assert.Equal("old", follower.Read("name"));
        Assert.Equal("new", Routing.ReadYourWrites(leader, follower, "name", position));

        follower.Replicate();
        Assert.Equal("new", follower.Read("name"));
    }
}

public class QuorumTests
{
    [Fact]
    public void Overlapping_quorums_see_the_latest_write()
    {
        var register = new QuorumRegister(3);
        register.Write(1, "v1", 0, 1, 2);
        register.Write(2, "v2", 0, 1);

        Assert.True(Quorum.Overlaps(3, 2, 2));
        foreach (int[] pair in new int[][] { [0, 1], [0, 2], [1, 2] })
            Assert.Equal("v2", register.Read(pair).Value);
    }

    [Fact]
    public void Without_overlap_a_read_can_miss_the_write()
    {
        var register = new QuorumRegister(3);
        register.Write(1, "v1", 0);

        Assert.False(Quorum.Overlaps(3, 1, 1));
        Assert.Null(register.Read(2).Value);
    }

    [Theory]
    [InlineData(3, 2, 2, 1)]
    [InlineData(5, 3, 3, 2)]
    [InlineData(3, 3, 1, 0)]
    public void Tolerated_failures(int n, int w, int r, int expected) =>
        Assert.Equal(expected, Quorum.ToleratedFailures(n, w, r));
}

public class PartitioningTests
{
    [Fact]
    public void Sequential_keys_hit_one_range_partition()
    {
        int[] load = Partitioning.Load(
            Enumerable.Range(3000, 1000).Select(k => Partitioning.ByRange(k, 1000, 2000, 3000)), 4);

        Assert.Equal([0, 0, 0, 1000], load);
    }

    [Fact]
    public void Hashing_spreads_them()
    {
        int[] load = Partitioning.Load(
            Enumerable.Range(3000, 1000).Select(k => Partitioning.ByHash($"{k}", 4)), 4);

        Assert.All(load, count => Assert.InRange(count, 200, 300));
    }

    [Fact]
    public void Salting_splits_a_hot_key_into_known_buckets()
    {
        string[] written = [.. Enumerable.Range(0, 100).Select(i => Partitioning.Salted("celebrity", i, 8)).Distinct()];

        Assert.Equal(Partitioning.AllBuckets("celebrity", 8).Order(), written.Order());
    }
}

public class VersionVectorTests
{
    [Fact]
    public void Detects_concurrency()
    {
        var a = new Dictionary<string, long> { ["n1"] = 2, ["n2"] = 1 };
        var b = new Dictionary<string, long> { ["n1"] = 1, ["n2"] = 2 };
        var older = new Dictionary<string, long> { ["n1"] = 1 };

        Assert.Equal(Causality.Concurrent, VersionVector.Compare(a, b));
        Assert.Equal(Causality.Before, VersionVector.Compare(older, a));
        Assert.Equal(Causality.After, VersionVector.Compare(a, older));
        Assert.Equal(Causality.Equal, VersionVector.Compare(a, a));
    }

    [Fact]
    public void Last_write_wins_trusts_a_skewed_clock()
    {
        var noon = new DateTimeOffset(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);
        var first = new Write("first", noon.AddSeconds(5)); // clock 5 s fast
        var second = new Write("second", noon.AddSeconds(1)); // happened later

        Assert.Equal("first", LastWriteWins.Resolve(first, second).Value);
    }
}

public class ChainTests
{
    [Fact]
    public void An_unfinished_write_is_not_visible()
    {
        var chain = new Chain(3);
        Assert.True(chain.Write("k", "v1"));
        Assert.False(chain.Write("k", "v2", stopAfter: 2));
        Assert.Equal("v1", chain.Read("k"));

        chain.Fail(2); // the middle server becomes the tail and already has v2
        Assert.Equal("v2", chain.Read("k"));
    }
}

public class HandoffTests
{
    [Fact]
    public void A_stand_in_accepts_the_write_and_hands_it_back()
    {
        var store = new SloppyStore(n: 3, total: 5);
        store.Write("v1");
        store.Down(0);

        Assert.Equal(3, store.Write("v2"));
        Assert.Equal("v2", store.ValueAt(3));
        store.Up(0);
        Assert.Equal("v1", store.ValueAt(0)); // stale until the handoff

        store.Handoff();
        Assert.Equal(["v2", "v2", "v2"], store.ReadPreferred());
        Assert.Null(store.ValueAt(3));
    }
}

public class LateHandoffTests
{
    [Fact]
    public void Handoff_does_not_replace_a_newer_value()
    {
        var store = new SloppyStore(n: 3, total: 5);
        store.Down(0);
        store.Write("v2");
        store.Up(0);
        store.Write("v3"); // node 0 is back and receives the newer value directly

        store.Handoff();

        Assert.Equal(["v3", "v3", "v3"], store.ReadPreferred());
        Assert.Null(store.ValueAt(3));
    }
}

public class RebalancingTests
{
    [Fact]
    public void Only_the_needed_partitions_move_and_only_to_the_new_node()
    {
        int[] before = Rebalancing.Initial(1024, 4);
        int[] after = Rebalancing.AddNode(before, newNode: 4);

        int[] changed = [.. Enumerable.Range(0, 1024).Where(p => before[p] != after[p])];
        Assert.Equal(204, changed.Length);
        Assert.All(changed, p => Assert.Equal(4, after[p]));
        int[] counts = [.. after.CountBy(n => n).Select(g => g.Value)];
        Assert.True(counts.Max() - counts.Min() <= 1);
    }
}

public class ScatterGatherTests
{
    [Fact]
    public void A_deep_page_transfers_far_more_than_it_returns()
    {
        int[][] shards = [.. Enumerable.Range(0, 4).Select(s => Enumerable.Range(0, 1000).Select(i => i * 4 + s).ToArray())];

        var (page, transferred) = ScatterGather.Page(shards, offset: 500, size: 10);

        Assert.Equal(Enumerable.Range(500, 10), page);
        Assert.Equal(2040, transferred);
    }

    [Fact]
    public void Averages_are_combined_from_sums_and_counts() =>
        Assert.Equal(19.0, ScatterGather.Average([(10, 1), (1890, 99)]), 9);
}
