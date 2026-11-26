// Example: conflicts in active-active: last-write-wins loses an item, an observed-remove set does not.
// Module: system-design/16-multi-region-and-dr (advanced, expert)
// Tests: dotnet test --filter "FullyQualifiedName~ActiveActive"
namespace MultiRegion.Examples;

public static class LastWriteWins
{
    public static (long Stamp, string[] Items) Merge((long Stamp, string[] Items) a, (long Stamp, string[] Items) b) =>
        b.Stamp > a.Stamp ? b : a;
}

// A removal deletes only the additions it has seen, so a concurrent add survives.
public sealed class ObservedRemoveSet
{
    private readonly HashSet<(string Item, Guid Tag)> _added = [];
    private readonly HashSet<Guid> _removed = []; // grows forever without garbage collection

    public IEnumerable<string> Items => _added.Where(a => !_removed.Contains(a.Tag)).Select(a => a.Item).Distinct().Order();

    public void Add(string item) => _added.Add((item, Guid.NewGuid()));

    public void Remove(string item)
    {
        foreach (var (_, tag) in _added.Where(a => a.Item == item)) _removed.Add(tag);
    }

    public void Merge(ObservedRemoveSet other)
    {
        _added.UnionWith(other._added);
        _removed.UnionWith(other._removed);
    }
}
