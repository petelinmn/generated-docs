// Example: version vectors detect concurrent writes that last-write-wins would drop.
// Module: system-design/06-replication-and-partitioning (expert)
// Tests: dotnet test --filter "FullyQualifiedName~VersionVector"
namespace Replication.Examples;

public enum Causality { Equal, Before, After, Concurrent }

public static class VersionVector
{
    public static Causality Compare(IReadOnlyDictionary<string, long> a, IReadOnlyDictionary<string, long> b)
    {
        bool less = false, greater = false;
        foreach (string node in a.Keys.Union(b.Keys))
        {
            long x = a.GetValueOrDefault(node), y = b.GetValueOrDefault(node);
            less |= x < y;
            greater |= x > y;
        }

        return (less, greater) switch
        {
            (false, false) => Causality.Equal,
            (true, false) => Causality.Before,
            (false, true) => Causality.After,
            _ => Causality.Concurrent,
        };
    }
}

public sealed record Write(string Value, DateTimeOffset Timestamp);

public static class LastWriteWins
{
    // Picks by wall clock: a node with a fast clock wins regardless of what really happened later.
    public static Write Resolve(Write a, Write b) => b.Timestamp > a.Timestamp ? b : a;
}
