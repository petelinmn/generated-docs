// Example: a toy LSM tree: memtable, immutable segments, tombstones, compaction.
// Module: system-design/04-data-storage (advanced)
// Tests: dotnet test --filter "FullyQualifiedName~Lsm"
namespace DataStorage.Examples;

// Not thread-safe and keeps everything in memory: it shows the read and write paths only.
public sealed class LsmStore(int memtableLimit)
{
    private SortedDictionary<string, string?> _memtable = new(StringComparer.Ordinal);
    private readonly List<SortedDictionary<string, string?>> _segments = []; // oldest first

    public int SegmentCount => _segments.Count;
    public int StoredEntries => _memtable.Count + _segments.Sum(s => s.Count);

    public void Put(string key, string value) => Write(key, value);

    // A delete is a write of a tombstone: older segments are immutable.
    public void Delete(string key) => Write(key, null);

    public string? Get(string key) => Get(key, out _);

    public string? Get(string key, out int structuresChecked)
    {
        structuresChecked = 1;
        if (_memtable.TryGetValue(key, out string? value)) return value;

        for (int i = _segments.Count - 1; i >= 0; i--) // newest wins
        {
            structuresChecked++;
            if (_segments[i].TryGetValue(key, out value)) return value;
        }
        return null;
    }

    public void Compact()
    {
        var merged = new SortedDictionary<string, string?>(StringComparer.Ordinal);
        foreach (var segment in _segments)
            foreach (var (key, value) in segment)
                merged[key] = value;

        // All segments take part, so tombstones have nothing left to hide.
        foreach (string key in merged.Where(e => e.Value is null).Select(e => e.Key).ToList())
            merged.Remove(key);

        _segments.Clear();
        if (merged.Count > 0) _segments.Add(merged);
    }

    private void Write(string key, string? value)
    {
        _memtable[key] = value;
        if (_memtable.Count < memtableLimit) return;

        _segments.Add(_memtable); // flush
        _memtable = new(StringComparer.Ordinal);
    }
}
