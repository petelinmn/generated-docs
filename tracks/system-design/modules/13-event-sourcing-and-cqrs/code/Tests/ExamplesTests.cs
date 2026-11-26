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
