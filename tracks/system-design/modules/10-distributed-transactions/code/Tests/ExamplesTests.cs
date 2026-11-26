using Transactions.Examples;

public class TwoPhaseTests
{
    [Fact]
    public void One_no_vote_aborts_everyone()
    {
        Participant[] all = [new(), new(canCommit: false), new()];
        TwoPhaseCommit.Run(all);
        Assert.All(all, p => Assert.Equal(State.Aborted, p.State));
    }

    [Fact]
    public void Coordinator_crash_leaves_participants_in_doubt()
    {
        Participant[] all = [new(), new()];
        TwoPhaseCommit.Run(all, crashAfterPrepare: true);
        Assert.All(all, p => Assert.Equal(State.Prepared, p.State));
    }
}

public class SagaTests
{
    private static SagaStep Ok(string name, bool compensable = true) =>
        new(name, () => Task.CompletedTask, compensable ? () => Task.CompletedTask : null);

    private static SagaStep Fails(string name) =>
        new(name, () => Task.FromException(new InvalidOperationException()), () => Task.CompletedTask);

    [Fact]
    public async Task Failure_compensates_in_reverse_order()
    {
        var journal = new List<string>();
        SagaOutcome outcome = await Saga.RunAsync([Ok("order"), Ok("stock"), Fails("payment")], journal);

        Assert.Equal(SagaOutcome.Compensated, outcome);
        Assert.Equal(["order", "stock", "undo stock", "undo order"], journal);
    }

    [Fact]
    public async Task Failure_after_the_pivot_cannot_be_compensated()
    {
        var journal = new List<string>();
        SagaOutcome outcome = await Saga.RunAsync(
            [Ok("stock"), Ok("charge", compensable: false), Fails("ship")], journal);

        Assert.Equal(SagaOutcome.NeedsAttention, outcome);
        Assert.Equal(["stock", "charge"], journal);
    }

    [Fact]
    public async Task A_failing_compensation_stops_the_rollback()
    {
        var journal = new List<string>();
        var broken = new SagaStep("stock", () => Task.CompletedTask,
            () => Task.FromException(new TimeoutException()));

        await Assert.ThrowsAsync<TimeoutException>(
            () => Saga.RunAsync([Ok("order"), broken, Fails("payment")], journal));
        Assert.DoesNotContain("undo order", journal);
    }
}

public class OutboxTests
{
    [Fact]
    public void Order_and_event_are_atomic()
    {
        using var db = new OrderDatabase();
        Assert.Throws<InvalidOperationException>(() => db.PlaceOrder(1, failBeforeCommit: true));

        Assert.Equal((0L, 0L), (db.Count("orders"), db.Count("outbox")));
    }

    [Fact]
    public void Crash_in_the_relay_duplicates_and_the_inbox_absorbs_it()
    {
        using var db = new OrderDatabase();
        var inbox = new Inbox();
        int deliveries = 0;
        void Publish(long id, string payload) { deliveries++; inbox.Receive(id, payload); }
        db.PlaceOrder(7);

        db.Relay(Publish, crashBeforeMark: true);
        db.Relay(Publish);
        db.Relay(Publish);

        Assert.Equal(2, deliveries);
        Assert.Equal(["OrderPlaced:7"], inbox.Handled);
    }
}
