// Example: asynchronous replication, lag and read-your-writes.
// Module: system-design/06-replication-and-partitioning (basics, advanced)
// Tests: dotnet test --filter "FullyQualifiedName~ReplicationLag"
namespace Replication.Examples;

public sealed class Leader
{
    private readonly List<(string Key, string Value)> _log = [];
    private readonly Dictionary<string, string> _data = [];

    public IReadOnlyList<(string Key, string Value)> Log => _log;

    // Returns the log position of the write.
    public long Write(string key, string value)
    {
        _log.Add((key, value));
        _data[key] = value;
        return _log.Count;
    }

    public string? Read(string key) => _data.GetValueOrDefault(key);
}

public sealed class Follower(Leader leader)
{
    private readonly Dictionary<string, string> _data = [];

    public long AppliedPosition { get; private set; }

    // Called explicitly, so the lag is under the control of the caller.
    public void Replicate(int maxEntries = int.MaxValue)
    {
        while (AppliedPosition < leader.Log.Count && maxEntries-- > 0)
        {
            var (key, value) = leader.Log[(int)AppliedPosition];
            _data[key] = value;
            AppliedPosition++;
        }
    }

    public string? Read(string key) => _data.GetValueOrDefault(key);
}

public static class Routing
{
    // The client remembers the position of its last write and presents it with reads.
    public static string? ReadYourWrites(Leader leader, Follower follower, string key, long lastWritePosition) =>
        follower.AppliedPosition >= lastWritePosition ? follower.Read(key) : leader.Read(key);
}
