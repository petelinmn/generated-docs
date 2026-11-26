using Consensus.Examples;
using Microsoft.Extensions.Time.Testing;

public class ElectionTests
{
    private static Node[] Cluster() => [.. Enumerable.Range(1, 5).Select(i => new Node(i))];

    [Fact]
    public void Majority_elects_a_minority_does_not()
    {
        Node[] n = Cluster();

        Assert.False(n[0].StartElection([n[1]], 5));
        Assert.True(n[2].StartElection([n[3], n[4]], 5));
    }

    [Fact]
    public void One_vote_per_term()
    {
        var voter = new Node(1);

        Assert.True(voter.RequestVote(1, candidate: 2, 0, 0));
        Assert.False(voter.RequestVote(1, candidate: 3, 0, 0));
        Assert.True(voter.RequestVote(2, candidate: 3, 0, 0));
    }

    [Fact]
    public void Candidate_with_a_stale_log_loses()
    {
        Node stale = new(1, lastLogTerm: 1, lastLogIndex: 9);
        Node[] others = [new(2, 2, 3), new(3, 2, 3)];

        Assert.False(stale.StartElection(others, 3));
    }
}

public class FencingTests
{
    [Fact]
    public void Paused_holder_is_rejected_by_the_storage()
    {
        var clock = new FakeTimeProvider();
        var leases = new LeaseService(clock);
        var storage = new FencedStorage();
        var lease = TimeSpan.FromSeconds(10);

        long a = leases.TryAcquire(lease)!.Value;
        Assert.Null(leases.TryAcquire(lease));

        clock.Advance(TimeSpan.FromSeconds(11)); // A is paused and does not notice
        long b = leases.TryAcquire(lease)!.Value;

        Assert.True(storage.TryWrite(b, "from B"));
        Assert.False(storage.TryWrite(a, "from A"));
        Assert.Equal("from B", storage.Value);
    }
}

public class LamportTests
{
    [Fact]
    public void Receive_is_later_than_send()
    {
        var a = new LamportClock();
        var b = new LamportClock();
        for (int i = 0; i < 5; i++) b.Tick();

        long sent = a.Tick();
        long received = b.Receive(sent);

        Assert.Equal((1L, 6L), (sent, received));
    }
}
