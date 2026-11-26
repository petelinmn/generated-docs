// Example: why Raft does not switch configurations in one step, and what joint consensus requires.
// Module: system-design/12-consensus-and-coordination (expert)
// Tests: dotnet test --filter "FullyQualifiedName~Membership"
namespace Consensus.Examples;

public static class Membership
{
    public static bool HasMajority(IReadOnlySet<int> votes, IReadOnlySet<int> configuration) =>
        configuration.Count(votes.Contains) > configuration.Count / 2;

    // During the transition a decision needs separate majorities of both configurations.
    public static bool JointMajority(IReadOnlySet<int> votes, IReadOnlySet<int> oldOne, IReadOnlySet<int> newOne) =>
        HasMajority(votes, oldOne) && HasMajority(votes, newOne);
}

// Log compaction: committed entries are replaced by the state they produced.
public sealed class CompactingLog
{
    private readonly List<(string Key, string Value)> _entries = [];
    private Dictionary<string, string> _snapshot = [];

    public long SnapshotIndex { get; private set; }
    public int EntriesKept => _entries.Count;

    public void Append(string key, string value) => _entries.Add((key, value));

    public void Compact(long committedIndex)
    {
        int count = (int)(committedIndex - SnapshotIndex);
        if (count <= 0) return;

        foreach (var (key, value) in _entries.Take(count)) _snapshot[key] = value;
        _entries.RemoveRange(0, count);
        SnapshotIndex = committedIndex;
    }

    public Dictionary<string, string> State()
    {
        var state = new Dictionary<string, string>(_snapshot);
        foreach (var (key, value) in _entries) state[key] = value;
        return state;
    }
}
