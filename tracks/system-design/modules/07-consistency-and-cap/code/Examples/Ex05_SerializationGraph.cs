// Example: an execution is serializable if its dependency graph has no cycle.
// Module: system-design/07-consistency-and-cap (expert)
// Tests: dotnet test --filter "FullyQualifiedName~SerializationGraph"
namespace Consistency.Examples;

// An edge From -> To means: in any equivalent serial order, From must come first.
public sealed record Dependency(string From, string To);

public static class SerializationGraph
{
    public static bool HasCycle(IReadOnlyCollection<Dependency> edges)
    {
        var next = edges.ToLookup(e => e.From, e => e.To);
        var done = new HashSet<string>();
        var path = new HashSet<string>();

        bool Visit(string node)
        {
            if (done.Contains(node)) return false;
            if (!path.Add(node)) return true;
            if (next[node].Any(Visit)) return true;
            path.Remove(node);
            done.Add(node);
            return false;
        }

        return edges.Select(e => e.From).Any(Visit);
    }
}
