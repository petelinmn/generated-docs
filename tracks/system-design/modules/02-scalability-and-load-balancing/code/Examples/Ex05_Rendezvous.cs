// Example: rendezvous (highest random weight) hashing.
// Module: system-design/02-scalability-and-load-balancing (expert)
// Tests: dotnet test --filter "FullyQualifiedName~Rendezvous"
namespace Scaling.Examples;

public static class Rendezvous
{
    // No ring to maintain: the node with the highest hash of (node, key) wins. O(n) per lookup.
    public static string NodeFor(string key, IReadOnlyList<string> nodes) =>
        nodes.MaxBy(node => StableHash.Of($"{node}\n{key}"))!;
}
