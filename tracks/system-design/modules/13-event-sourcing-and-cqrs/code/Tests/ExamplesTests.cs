using EventSourcing.Examples;

public class EventStoreTests
{
    [Fact]
    public void State_is_the_fold_of_events()
    {
        Account account = Account.Replay([new Deposited(100), new Withdrawn(30)]);

        Assert.Equal((70m, 2), (account.Balance, account.Version));
        Assert.Throws<InvalidOperationException>(() => account.Withdraw(71));
    }

    [Fact]
    public void Stale_version_is_rejected()
    {
        var store = new EventStore();
        store.TryAppend("a", 0, new Deposited(100));

        Assert.True(store.TryAppend("a", 1, new Withdrawn(80)));
        Assert.False(store.TryAppend("a", 1, new Withdrawn(80)));
        Assert.Equal(20, Account.Replay(store.Read("a")).Balance);
    }
}

public class ProjectionTests
{
    [Fact]
    public void Lags_catches_up_ignores_duplicates_and_can_be_rebuilt()
    {
        var store = new EventStore();
        var view = new BalanceProjection();
        store.TryAppend("a", 0, new Deposited(100));
        view.CatchUp(store.Log);
        store.TryAppend("a", 1, new Withdrawn(30));

        Assert.Equal(100, view.BalanceOf("a")); // not caught up yet
        view.CatchUp(store.Log);
        view.CatchUp(store.Log);
        Assert.Equal(70, view.BalanceOf("a"));

        var rebuilt = new BalanceProjection();
        rebuilt.CatchUp(store.Log);
        Assert.Equal(70, rebuilt.BalanceOf("a"));
    }
}

public class UpcastingTests
{
    [Fact]
    public void Old_and_new_versions_read_the_same_way()
    {
        Assert.Equal(new DepositedV2(5, "USD"), Upcaster.Read(1, """{"amount":5}"""));
        Assert.Equal(new DepositedV2(5, "EUR"), Upcaster.Read(2, """{"amount":5,"currency":"EUR"}"""));
    }
}

public class SnapshotPolicyTests
{
    private static readonly AccountEvent[] History = [.. Enumerable.Range(0, 250).Select(_ => (AccountEvent)new Deposited(1))];

    [Fact]
    public void Replays_only_the_tail_until_the_schema_changes()
    {
        var repository = new SnapshotRepository(every: 100, schemaVersion: 1);

        repository.Load("a", History[..230]);
        Assert.Equal(230, repository.EventsReplayed);

        Assert.Equal(250, repository.Load("a", History).Balance);
        Assert.Equal(20, repository.EventsReplayed);

        var upgraded = repository.Upgrade(2);
        upgraded.Load("a", History);
        Assert.Equal(250, upgraded.EventsReplayed);
    }
}

public class BlueGreenTests
{
    [Fact]
    public void Switches_only_after_catching_up()
    {
        var store = new EventStore();
        for (int i = 0; i < 5; i++) store.TryAppend("a", i, new Deposited(10));
        var projections = new ProjectionSwitch(new BalanceProjection());
        projections.StartRebuild();

        projections.Process(store.Log, batch: 2);
        Assert.False(projections.TrySwitch(head: 5));
        Assert.Equal(50, projections.Live.BalanceOf("a")); // still served by the old one

        projections.Process(store.Log, batch: 10);
        var old = projections.Live;
        Assert.True(projections.TrySwitch(head: 5));
        Assert.NotSame(old, projections.Live);
        Assert.Equal(50, projections.Live.BalanceOf("a"));
    }
}

public class ShreddingTests
{
    [Fact]
    public void Deleting_the_key_makes_the_stored_data_unreadable()
    {
        var vault = new ShreddingVault();
        byte[] first = vault.Protect("u1", "ann@example.com");
        byte[] second = vault.Protect("u1", "ann@example.com");

        Assert.NotEqual(first, second);
        Assert.Equal("ann@example.com", vault.Reveal("u1", first));

        vault.Protect("u2", "bob@example.com");
        Assert.ThrowsAny<System.Security.Cryptography.CryptographicException>(() => vault.Reveal("u2", first));

        vault.Forget("u1");
        Assert.Null(vault.Reveal("u1", first));
    }
}
