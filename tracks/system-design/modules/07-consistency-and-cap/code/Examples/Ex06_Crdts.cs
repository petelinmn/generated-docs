// Example: two more CRDTs: a counter that can decrease and a last-writer-wins register.
// Module: system-design/07-consistency-and-cap (expert)
// Tests: dotnet test --filter "FullyQualifiedName~Crdt"
namespace Consistency.Examples;

// Two grow-only maps: increments and decrements.
public sealed class PnCounter(string node)
{
    private readonly Dictionary<string, long> _plus = [], _minus = [];

    public long Value => _plus.Values.Sum() - _minus.Values.Sum();

    public void Add(long delta)
    {
        var map = delta >= 0 ? _plus : _minus;
        map[node] = map.GetValueOrDefault(node) + Math.Abs(delta);
    }

    public void Merge(PnCounter other)
    {
        MergeInto(_plus, other._plus);
        MergeInto(_minus, other._minus);
    }

    private static void MergeInto(Dictionary<string, long> target, Dictionary<string, long> source)
    {
        foreach (var (key, count) in source) target[key] = Math.Max(target.GetValueOrDefault(key), count);
    }
}

// The node id breaks ties, so replicas that see equal timestamps still agree.
public readonly record struct LwwRegister(string Value, long Timestamp, string Node)
{
    public static LwwRegister Merge(LwwRegister a, LwwRegister b) =>
        (a.Timestamp, string.CompareOrdinal(a.Node, b.Node)).CompareTo((b.Timestamp, 0)) >= 0 ? a : b;
}
