using Consistency.Examples;

public class IsolationTests
{
    private static MvccStore Seeded(params (string Key, int Value)[] rows)
    {
        var store = new MvccStore();
        var tx = store.Begin();
        foreach (var (key, value) in rows) tx.Write(key, value);
        tx.TryCommit();
        return store;
    }

    [Fact]
    public void Snapshot_does_not_see_later_commits()
    {
        MvccStore store = Seeded(("x", 1));
        var reader = store.Begin();
        var writer = store.Begin();
        writer.Write("x", 2);
        writer.TryCommit();

        Assert.Equal(1, reader.Read("x"));
        Assert.Equal(2, store.Read("x"));
    }

    [Fact]
    public void Concurrent_increments_of_one_row_conflict()
    {
        MvccStore store = Seeded(("x", 10));
        var a = store.Begin();
        var b = store.Begin();
        a.Write("x", a.Read("x") + 1);
        b.Write("x", b.Read("x") + 1);

        Assert.True(a.TryCommit());
        Assert.False(b.TryCommit());
        Assert.Equal(11, store.Read("x"));
    }

    // Rule: at least one doctor must stay on call.
    private static (bool, bool, int) GoOffCall(Isolation isolation)
    {
        MvccStore store = Seeded(("alice", 1), ("bob", 1));
        var a = store.Begin(isolation);
        var b = store.Begin(isolation);
        if (a.Read("alice") + a.Read("bob") >= 2) a.Write("alice", 0);
        if (b.Read("alice") + b.Read("bob") >= 2) b.Write("bob", 0);

        return (a.TryCommit(), b.TryCommit(), store.Read("alice") + store.Read("bob"));
    }

    [Fact]
    public void Snapshot_isolation_allows_write_skew() =>
        Assert.Equal((true, true, 0), GoOffCall(Isolation.Snapshot));

    [Fact]
    public void Serializable_rejects_write_skew() =>
        Assert.Equal((true, false, 1), GoOffCall(Isolation.Serializable));
}

public class PartitionTests
{
    [Fact]
    public void Cp_refuses_on_the_minority_side()
    {
        var cluster = new Cluster(3, Mode.Cp);
        cluster.TryWrite(0, "v1");
        cluster.Split(2);

        Assert.True(cluster.TryWrite(0, "v2"));
        Assert.False(cluster.TryWrite(2, "other"));
        Assert.False(cluster.TryRead(2, out _));
    }

    [Fact]
    public void Ap_accepts_on_both_sides_and_diverges()
    {
        var cluster = new Cluster(3, Mode.Ap);
        cluster.Split(2);
        cluster.TryWrite(0, "left");
        cluster.TryWrite(2, "right");
        cluster.Heal();

        cluster.TryRead(0, out string? a);
        cluster.TryRead(2, out string? b);
        Assert.Equal(("left", "right"), (a, b)); // healing alone does not reconcile
    }
}

public class SessionTests
{
    [Fact]
    public void Session_rejects_an_older_replica()
    {
        var session = new Session();

        Assert.Equal("new", session.Read(new Replica(2, "new")));
        Assert.Null(session.Read(new Replica(1, "old")));
    }
}
