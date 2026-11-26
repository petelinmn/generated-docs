// Example: round robin, weighted round robin, least connections, power of two choices.
// Module: system-design/02-scalability-and-load-balancing (basics, advanced)
// Tests: dotnet test --filter "FullyQualifiedName~Balancer"
namespace Scaling.Examples;

public sealed class RoundRobin(IReadOnlyList<string> nodes)
{
    private int _next = -1;

    public string Pick()
    {
        // The uint cast keeps the index non-negative after the counter overflows.
        uint i = unchecked((uint)Interlocked.Increment(ref _next));
        return nodes[(int)(i % (uint)nodes.Count)];
    }
}

// Smooth weighted round robin: heavy nodes are spread out instead of served in bursts.
public sealed class SmoothWeightedRoundRobin(params (string Node, int Weight)[] nodes)
{
    private readonly int[] _current = new int[nodes.Length];
    private readonly int _total = nodes.Sum(n => n.Weight);
    private readonly Lock _gate = new();

    public string Pick()
    {
        lock (_gate)
        {
            int best = 0;
            for (int i = 0; i < nodes.Length; i++)
            {
                _current[i] += nodes[i].Weight;
                if (_current[i] > _current[best]) best = i;
            }
            _current[best] -= _total;
            return nodes[best].Node;
        }
    }
}

public sealed class Backend(string name)
{
    private int _active;
    public string Name { get; } = name;
    public int Active => Volatile.Read(ref _active);
    public void Start() => Interlocked.Increment(ref _active);
    public void Finish() => Interlocked.Decrement(ref _active);
}

public static class LoadAware
{
    public static Backend LeastConnections(IReadOnlyList<Backend> nodes) => nodes.MinBy(n => n.Active)!;

    // Compare two distinct random nodes: much better than one random pick, at O(1) cost.
    public static Backend PowerOfTwoChoices(IReadOnlyList<Backend> nodes, Random random)
    {
        if (nodes.Count == 1) return nodes[0];
        int a = random.Next(nodes.Count);
        int b = random.Next(nodes.Count - 1);
        if (b >= a) b++;
        return nodes[a].Active <= nodes[b].Active ? nodes[a] : nodes[b];
    }
}
