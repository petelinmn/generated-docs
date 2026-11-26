using System.Threading.Channels;
using Messaging.Examples;

public class BackpressureTests
{
    [Fact]
    public void Wait_mode_refuses_TryWrite_when_full()
    {
        var (accepted, buffered) = Backpressure.Overflow(BoundedChannelFullMode.Wait, 2, 5);
        Assert.Equal(2, accepted);
        Assert.Equal([1, 2], buffered);
    }

    [Theory]
    [InlineData(BoundedChannelFullMode.DropOldest, new[] { 4, 5 })]
    [InlineData(BoundedChannelFullMode.DropNewest, new[] { 1, 5 })]
    [InlineData(BoundedChannelFullMode.DropWrite, new[] { 1, 2 })]
    public void Drop_modes_report_success_and_lose_items(BoundedChannelFullMode mode, int[] expected)
    {
        var (accepted, buffered) = Backpressure.Overflow(mode, 2, 5);
        Assert.Equal(5, accepted);
        Assert.Equal(expected, buffered);
    }

    [Fact]
    public async Task WriteAsync_waits_until_the_consumer_reads()
    {
        var ct = TestContext.Current.CancellationToken;
        Channel<int> channel = Backpressure.Create(1, BoundedChannelFullMode.Wait);
        await channel.Writer.WriteAsync(1, ct);

        Task blocked = channel.Writer.WriteAsync(2, ct).AsTask();
        Assert.False(blocked.IsCompleted);

        await channel.Reader.ReadAsync(ct);
        await blocked;
    }
}

public class DeliveryTests
{
    private static readonly Message[] Queue = [new("a", 10), new("b", 20), new("c", 30)];

    private static int Run(Ack ack, bool idempotent)
    {
        var account = new Account();
        Delivery.Consume(Queue, ack, "b", idempotent ? account.ApplyOnce : account.Apply);
        return account.Balance;
    }

    [Fact]
    public void At_most_once_loses() => Assert.Equal(40, Run(Ack.BeforeProcessing, idempotent: false));

    [Fact]
    public void At_least_once_duplicates() => Assert.Equal(80, Run(Ack.AfterProcessing, idempotent: false));

    [Fact]
    public void Idempotent_consumer_gets_the_effect_once() =>
        Assert.Equal(60, Run(Ack.AfterProcessing, idempotent: true));
}

public class EventLogTests
{
    [Fact]
    public void Groups_are_independent_and_can_replay()
    {
        var log = new EventLog();
        log.Append("a");
        log.Append("b");

        log.Commit("billing", log.Poll("billing").Count);

        Assert.Equal(0, log.Lag("billing"));
        Assert.Equal(2, log.Lag("search"));
        log.Seek("billing", 0);
        Assert.Equal(["a", "b"], log.Poll("billing"));
    }
}

public class DeadLetterTests
{
    [Fact]
    public void Poison_message_ends_in_the_dead_letter_queue()
    {
        var queue = new WorkQueue(maxAttempts: 3);
        foreach (string body in new[] { "a", "poison", "b" }) queue.Publish(body);
        int attempts = 0;

        queue.Drain(body =>
        {
            if (body == "poison") { attempts++; throw new FormatException(); }
        });

        Assert.Equal(["a", "b"], queue.Done);
        Assert.Equal(["poison"], queue.DeadLetters);
        Assert.Equal(3, attempts);
    }
}

public class TransactionsTests
{
    [Fact]
    public void Outputs_and_offset_become_visible_together()
    {
        var log = new TransactionalLog();
        long first = log.Begin();
        log.Send(first, "a");
        log.SendOffset(first, 1);
        long second = log.Begin();
        log.Send(second, "b");

        Assert.Empty(log.ReadCommitted());
        Assert.Equal(0, log.ConsumedOffset);

        log.Commit(second);
        Assert.Empty(log.ReadCommitted()); // "a" is still open and comes first
        log.Commit(first);
        Assert.Equal(["a", "b"], log.ReadCommitted());
        Assert.Equal(1, log.ConsumedOffset);
    }

    [Fact]
    public void Aborted_records_are_hidden_from_read_committed()
    {
        var log = new TransactionalLog();
        long tx = log.Begin();
        log.Send(tx, "x");
        log.Abort(tx);

        Assert.Empty(log.ReadCommitted());
        Assert.Equal(["x"], log.ReadUncommitted());
    }
}

public class SchemaTests
{
    private static readonly SchemaField[] V1 = [new("id", false), new("note", false)];

    [Theory]
    [InlineData("add-with-default", true, true)]
    [InlineData("add-required", false, true)]
    [InlineData("remove-required", true, false)]
    public void Compatibility(string change, bool backward, bool forward)
    {
        SchemaField[] v2 = change switch
        {
            "add-with-default" => [.. V1, new("tag", true)],
            "add-required" => [.. V1, new("tag", false)],
            _ => [V1[0]],
        };

        Assert.Equal(backward, SchemaCompatibility.Backward(V1, v2));
        Assert.Equal(forward, SchemaCompatibility.Forward(V1, v2));
    }
}

public class RetryTiersTests
{
    [Fact]
    public void A_retried_message_is_overtaken_and_a_hopeless_one_is_dead_lettered()
    {
        var failures = new Dictionary<string, int> { ["a"] = 1, ["c"] = 99 };

        var (processed, dead) = RetryTiers.Run(["a", "b", "c", "d"], failures, tierDelays: [10, 100]);

        Assert.Equal(["b", "d", "a"], processed);
        Assert.Equal(["c"], dead);
    }
}

public class AssignmentTests
{
    [Fact]
    public void Sticky_moves_much_less_than_a_full_recalculation()
    {
        string[] before = Assignment.RoundRobin(12, ["a", "b", "c", "d"]);
        string[] survivors = ["a", "b", "c"];

        Assert.Equal(9, Assignment.Moved(before, Assignment.RoundRobin(12, survivors)));
        string[] sticky = Assignment.Sticky(before, survivors);
        Assert.Equal(3, Assignment.Moved(before, sticky));
        Assert.All(sticky.CountBy(c => c), g => Assert.Equal(4, g.Value));
    }
}
