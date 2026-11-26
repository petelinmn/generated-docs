// Example: a toy multi-version store with snapshot and serializable isolation.
// Module: system-design/07-consistency-and-cap (basics, advanced)
// Tests: dotnet test --filter "FullyQualifiedName~Isolation"
namespace Consistency.Examples;

public enum Isolation { Snapshot, Serializable }

// Not thread-safe: transactions are interleaved explicitly by the caller, which keeps anomalies repeatable.
public sealed class MvccStore
{
    private readonly Dictionary<string, List<(long CommittedAt, int Value)>> _versions = [];
    private long _clock;

    public Transaction Begin(Isolation isolation = Isolation.Snapshot) => new(this, _clock, isolation);

    public int Read(string key, long asOf = long.MaxValue) =>
        _versions.TryGetValue(key, out var list) ? list.LastOrDefault(v => v.CommittedAt <= asOf).Value : 0;

    private bool ChangedSince(string key, long snapshot) =>
        _versions.TryGetValue(key, out var list) && list[^1].CommittedAt > snapshot;

    public sealed class Transaction(MvccStore store, long snapshot, Isolation isolation)
    {
        private readonly Dictionary<string, int> _writes = [];
        private readonly HashSet<string> _reads = [];

        public int Read(string key)
        {
            _reads.Add(key);
            return _writes.TryGetValue(key, out int own) ? own : store.Read(key, snapshot);
        }

        public void Write(string key, int value) => _writes[key] = value;

        public bool TryCommit()
        {
            // First committer wins on the same key. Serializable also validates what was read.
            IEnumerable<string> mustBeUnchanged = isolation == Isolation.Serializable
                ? _writes.Keys.Union(_reads)
                : _writes.Keys;
            if (_writes.Count > 0 && mustBeUnchanged.Any(k => store.ChangedSince(k, snapshot))) return false;

            long commit = ++store._clock;
            foreach (var (key, value) in _writes)
            {
                if (!store._versions.TryGetValue(key, out var list)) store._versions[key] = list = [];
                list.Add((commit, value));
            }
            return true;
        }
    }
}
