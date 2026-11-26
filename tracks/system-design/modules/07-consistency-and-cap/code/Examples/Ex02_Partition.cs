// Example: what a cluster does during a network partition: refuse (CP) or diverge (AP).
// Module: system-design/07-consistency-and-cap (basics, advanced)
// Tests: dotnet test --filter "FullyQualifiedName~Partition"
namespace Consistency.Examples;

public enum Mode { Cp, Ap }

public sealed class Cluster(int size, Mode mode)
{
    private readonly string?[] _values = new string?[size];
    private int[] _group = new int[size]; // nodes with the same group number can talk

    public void Split(params int[] isolated)
    {
        _group = new int[size];
        foreach (int node in isolated) _group[node] = 1;
    }

    public void Heal() => _group = new int[size];

    private int[] Reachable(int node) => [.. Enumerable.Range(0, size).Where(i => _group[i] == _group[node])];

    private bool HasMajority(int node) => Reachable(node).Length > size / 2;

    public bool TryWrite(int node, string value)
    {
        if (mode == Mode.Cp && !HasMajority(node)) return false;

        foreach (int i in Reachable(node)) _values[i] = value;
        return true;
    }

    // A CP node without a majority cannot know whether its copy is current.
    public bool TryRead(int node, out string? value)
    {
        value = null;
        if (mode == Mode.Cp && !HasMajority(node)) return false;

        value = _values[node];
        return true;
    }
}
