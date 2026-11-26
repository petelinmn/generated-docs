// Example: an append-only log: reading does not remove, each group has its own offset.
// Module: system-design/08-messaging-and-events (basics, advanced)
// Tests: dotnet test --filter "FullyQualifiedName~EventLog"
namespace Messaging.Examples;

public sealed class EventLog
{
    private readonly List<string> _events = [];
    private readonly Dictionary<string, int> _offsets = [];

    public void Append(string e) => _events.Add(e);

    public IReadOnlyList<string> Poll(string group, int max = 100)
    {
        int offset = _offsets.GetValueOrDefault(group);
        return _events.GetRange(offset, Math.Min(max, _events.Count - offset));
    }

    public void Commit(string group, int count) => _offsets[group] = _offsets.GetValueOrDefault(group) + count;

    // Replay: move the offset back and read history again.
    public void Seek(string group, int offset) => _offsets[group] = offset;

    public int Lag(string group) => _events.Count - _offsets.GetValueOrDefault(group);
}
