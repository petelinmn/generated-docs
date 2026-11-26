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
