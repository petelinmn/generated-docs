using EventSourcing.Examples;
using EventSourcing.Solutions;

public class SolutionsTests
{
    private static readonly AccountEvent[] History = [new Deposited(100), new Withdrawn(30), new Deposited(5)];

    [Fact]
    public void Temporal_query() => Assert.Equal(70, Exercises.BalanceAt(History, 2));

    [Fact]
    public void Snapshot_gives_the_same_state_as_a_full_replay()
    {
        Account snapshot = Account.Replay(History.Take(2));

        Assert.Equal(Account.Replay(History), Exercises.Load(snapshot, History));
        Assert.Equal(Account.Replay(History), Exercises.Load(null, History));
    }

    [Fact]
    public void Conflict_is_resolved_by_deciding_again()
    {
        var store = new EventStore();
        store.TryAppend("a", 0, new Deposited(100));
        bool interfered = false;

        bool ok = Exercises.TryWithdraw(store, "a", 60, maxAttempts: 3, beforeAppend: () =>
        {
            if (interfered) return;
            interfered = true;
            store.TryAppend("a", 1, new Withdrawn(50));
        });

        Assert.False(ok); // after the reload only 50 is left
        Assert.Equal(50, Account.Replay(store.Read("a")).Balance);
    }
}
