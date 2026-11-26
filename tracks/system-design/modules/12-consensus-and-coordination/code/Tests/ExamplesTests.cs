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

public class PaxosTests
{
    [Fact]
    public void A_chosen_value_survives_a_later_proposer()
    {
        Acceptor[] a = [new(), new(), new(), new(), new()];

        Assert.Equal("x", Proposer.Propose(1, "x", [a[0], a[1], a[2]], 5));
        Assert.Equal("x", Proposer.Propose(2, "y", [a[2], a[3], a[4]], 5));
    }

    [Fact]
    public void No_majority_or_a_stale_number_chooses_nothing()
    {
        Acceptor[] a = [new(), new(), new()];

        Assert.Null(Proposer.Propose(1, "x", [a[0]], 3));
        Proposer.Propose(5, "y", a, 3);
        Assert.Null(Proposer.Propose(3, "z", a, 3));
    }
}

public class HybridClockTests
{
    [Fact]
    public void Causality_holds_although_the_receiver_clock_is_behind()
    {
        long fast = 100, slow = 10;
        var a = new HybridClock(() => fast);
        var b = new HybridClock(() => slow);

        HlcStamp sent = a.Tick();
        HlcStamp received = b.Receive(sent);
        HlcStamp next = b.Tick();

        Assert.Equal(new HlcStamp(100, 0), sent);
        Assert.Equal(new HlcStamp(100, 1), received);
        Assert.True(next.CompareTo(received) > 0);

        slow = 200; // physical time caught up and passed
        Assert.Equal(new HlcStamp(200, 0), b.Tick());
    }
}

public class MembershipTests
{
    [Fact]
    public void Switching_at_once_allows_two_disjoint_majorities()
    {
        HashSet<int> oldOne = [1, 2, 3], newOne = [1, 2, 3, 4, 5];
        HashSet<int> sideA = [1, 2], sideB = [3, 4, 5];

        Assert.True(Membership.HasMajority(sideA, oldOne));
        Assert.True(Membership.HasMajority(sideB, newOne));
        Assert.False(Membership.JointMajority(sideA, oldOne, newOne));
        Assert.False(Membership.JointMajority(sideB, oldOne, newOne));
        Assert.True(Membership.JointMajority(new HashSet<int> { 1, 2, 4 }, oldOne, newOne));
    }

    [Fact]
    public void Compaction_keeps_the_state()
    {
        var log = new CompactingLog();
        log.Append("a", "1");
        log.Append("a", "2");
        log.Append("b", "3");

        log.Compact(2);

        Assert.Equal((1, 2L), (log.EntriesKept, log.SnapshotIndex));
        Assert.Equal(("2", "3"), (log.State()["a"], log.State()["b"]));
    }
}
