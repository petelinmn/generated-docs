// Solutions to the exercises of system-design/02-scalability-and-load-balancing.
// Tests: dotnet test --filter "FullyQualifiedName~Solutions"
using Scaling.Examples;

namespace Scaling.Solutions;

// Exercise 1: round robin that skips unhealthy nodes.
public sealed class HealthAwareRoundRobin(IReadOnlyList<string> nodes, Func<string, bool> isHealthy)
{
    private int _next = -1;

    public string? Pick()
    {
        // Filter first, then rotate: otherwise the neighbour of a dead node gets double load.
        string[] healthy = [.. nodes.Where(isHealthy)];
        if (healthy.Length == 0) return null;
        uint i = unchecked((uint)Interlocked.Increment(ref _next));
        return healthy[(int)(i % (uint)healthy.Length)];
    }
}

public static class Exercises
{
    // Exercise 2: share of keys that change owner when the cluster is resized.
    public static double MovedFraction(IReadOnlyList<string> keys, Func<string, string> before, Func<string, string> after) =>
        (double)keys.Count(k => before(k) != after(k)) / keys.Count;

    // Exercise 3: the node count with the highest throughput under the Universal Scalability Law.
    public static int BestNodeCount(double contention, double coherency, int max) =>
        Enumerable.Range(1, max).MaxBy(n => ScalingLaws.Universal(contention, coherency, n));
}
