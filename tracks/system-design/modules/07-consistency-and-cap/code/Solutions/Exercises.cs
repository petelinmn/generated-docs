// Solutions to the exercises of system-design/07-consistency-and-cap.
// Tests: dotnet test --filter "FullyQualifiedName~Solutions"
namespace Consistency.Solutions;

public static class Exercises
{
    // Exercise 1: can a group of reachable nodes make progress in a cluster of n?
    public static bool HasQuorum(int reachable, int n) => reachable > n / 2;

    // Exercise 2: repeat an optimistic transaction until it commits.
    public static int RunWithRetry(Func<bool> attempt, int maxAttempts)
    {
        for (int i = 1; i <= maxAttempts; i++)
            if (attempt()) return i;
        throw new InvalidOperationException($"No success after {maxAttempts} attempts.");
    }
}

// Exercise 3: a grow-only counter that converges whatever the order of merges.
public sealed class GCounter(string node)
{
    private readonly Dictionary<string, long> _counts = [];

    public long Value => _counts.Values.Sum();

    public void Increment() => _counts[node] = _counts.GetValueOrDefault(node) + 1;

    public void Merge(GCounter other)
    {
        // Maximum, not sum: merging the same state twice must change nothing.
        foreach (var (key, count) in other._counts)
            _counts[key] = Math.Max(_counts.GetValueOrDefault(key), count);
    }
}
