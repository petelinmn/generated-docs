using Scaling.Examples;

public class BalancerTests
{
    [Fact]
    public void Round_robin_cycles()
    {
        var rr = new RoundRobin(["a", "b", "c"]);
        Assert.Equal("abcab", string.Concat(Enumerable.Range(0, 5).Select(_ => rr.Pick())));
    }

    [Fact]
    public void Smooth_weighted_spreads_the_heavy_node()
    {
        var wrr = new SmoothWeightedRoundRobin(("a", 5), ("b", 1), ("c", 1));
        Assert.Equal("aabacaa", string.Concat(Enumerable.Range(0, 7).Select(_ => wrr.Pick())));
    }

    [Fact]
    public void Least_connections_picks_the_idle_node()
    {
        Backend a = new("a"), b = new("b");
        a.Start();
        Assert.Same(b, LoadAware.LeastConnections([a, b]));
    }

    [Fact]
    public void Two_choices_never_picks_the_most_loaded_node()
    {
        Backend a = new("a"), b = new("b"), c = new("c");
        b.Start(); c.Start(); c.Start();
        var random = new Random(42);

        for (int i = 0; i < 1000; i++)
            Assert.NotSame(c, LoadAware.PowerOfTwoChoices([a, b, c], random));
    }
}

public class HashingTests
{
    private static readonly string[] Keys = [.. Enumerable.Range(0, 10_000).Select(i => $"user:{i}")];

    [Fact]
    public void Adding_a_node_moves_keys_only_to_the_new_node()
    {
        var before = new HashRing(["n1", "n2", "n3", "n4"]);
        var after = new HashRing(["n1", "n2", "n3", "n4", "n5"]);

        string[] moved = [.. Keys.Where(k => before.NodeFor(k) != after.NodeFor(k))];

        Assert.All(moved, k => Assert.Equal("n5", after.NodeFor(k)));
        Assert.InRange(moved.Length / 10_000.0, 0.15, 0.25);
    }

    [Fact]
    public void Modulo_moves_most_keys()
    {
        int moved = Keys.Count(k => ModuloSharding.ShardFor(k, 4) != ModuloSharding.ShardFor(k, 5));
        Assert.InRange(moved / 10_000.0, 0.75, 0.85);
    }
}

public class ScalingLawsTests
{
    [Fact]
    public void Amdahl_has_a_ceiling()
    {
        Assert.Equal(6.9, ScalingLaws.Amdahl(0.05, 10), 1);
        Assert.Equal(16.8, ScalingLaws.Amdahl(0.05, 100), 1);
    }

    [Fact]
    public void Coherency_makes_throughput_go_down() =>
        Assert.True(ScalingLaws.Universal(0.05, 0.001, 100) < ScalingLaws.Universal(0.05, 0.001, 31));
}

public class YarpGatewayTests
{
    [Fact]
    public async Task Yarp_alternates_between_backends()
    {
        await using var one = await YarpGateway.StartBackendAsync("one");
        await using var two = await YarpGateway.StartBackendAsync("two");
        await using var gateway = await YarpGateway.StartGatewayAsync(
            YarpGateway.AddressOf(one), YarpGateway.AddressOf(two));
        using var client = new HttpClient { BaseAddress = new Uri(YarpGateway.AddressOf(gateway)) };

        var answers = new List<string>();
        for (int i = 0; i < 4; i++)
            answers.Add(await client.GetStringAsync("/", TestContext.Current.CancellationToken));

        Assert.Equal(2, answers.Count(a => a == "one"));
        Assert.Equal(2, answers.Count(a => a == "two"));
        Assert.NotEqual(answers[0], answers[1]);
    }
}

public class RendezvousTests
{
    [Fact]
    public void Removing_a_node_moves_only_its_keys()
    {
        string[] all = ["n1", "n2", "n3", "n4", "n5"];
        string[] without = ["n1", "n2", "n4", "n5"];
        string[] keys = [.. Enumerable.Range(0, 10_000).Select(i => $"user:{i}")];

        string[] moved = [.. keys.Where(k => Rendezvous.NodeFor(k, all) != Rendezvous.NodeFor(k, without))];

        Assert.All(moved, k => Assert.Equal("n3", Rendezvous.NodeFor(k, all)));
        Assert.InRange(moved.Length / 10_000.0, 0.17, 0.23);
    }
}

public class MaglevTests
{
    private const int Size = 65_537;
    private static readonly string[] Nodes = [.. Enumerable.Range(1, 10).Select(i => $"n{i}")];

    [Fact]
    public void Slots_are_shared_almost_equally()
    {
        int[] counts = [.. new MaglevTable(Nodes, Size).Slots.CountBy(s => s).Select(p => p.Value)];

        Assert.Equal(10, counts.Length);
        Assert.True(counts.Max() - counts.Min() <= 1);
    }

    [Fact]
    public void A_composite_size_is_rejected() =>
        Assert.Throws<ArgumentException>(() => new MaglevTable(Nodes, 65_536));

    [Fact]
    public void Removing_a_node_disturbs_few_other_slots()
    {
        var before = new MaglevTable(Nodes, Size).Slots;
        var after = new MaglevTable([.. Nodes.Where(n => n != "n3")], Size).Slots;

        int others = before.Zip(after).Count(p => p.First != "n3" && p.First != p.Second);

        Assert.DoesNotContain("n3", after);
        Assert.InRange(others / (double)Size, 0.0001, 0.05); // small, but not zero
    }
}

public class DrainingTests
{
    [Fact]
    public async Task A_request_in_flight_finishes_during_shutdown()
    {
        var ct = TestContext.Current.CancellationToken;
        var server = await SlowServer.StartAsync(TimeSpan.FromSeconds(30));
        await using var app = server.App;
        using var client = new HttpClient();

        Task<string> request = client.GetStringAsync($"{YarpGateway.AddressOf(app)}/slow", ct);
        await server.RequestEntered;
        Task stopping = app.StopAsync(ct);
        Assert.False(stopping.IsCompleted);
        server.Release();

        Assert.Equal("done", await request);
        await stopping;
    }

    [Fact]
    public async Task A_request_longer_than_the_timeout_is_aborted()
    {
        var ct = TestContext.Current.CancellationToken;
        var server = await SlowServer.StartAsync(TimeSpan.FromMilliseconds(200));
        await using var app = server.App;
        using var client = new HttpClient();

        Task<string> request = client.GetStringAsync($"{YarpGateway.AddressOf(app)}/slow", ct);
        await server.RequestEntered;
        await app.StopAsync(ct);

        await Assert.ThrowsAsync<HttpRequestException>(() => request);
    }
}
