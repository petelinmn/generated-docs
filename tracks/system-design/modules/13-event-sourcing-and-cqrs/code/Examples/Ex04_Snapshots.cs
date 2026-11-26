// Example: a snapshot policy; a snapshot written by older code is ignored.
// Module: system-design/13-event-sourcing-and-cqrs (expert)
// Tests: dotnet test --filter "FullyQualifiedName~SnapshotPolicy"
namespace EventSourcing.Examples;

public sealed record Snapshot(int SchemaVersion, Account State);

public sealed class SnapshotRepository(int every, int schemaVersion)
{
    private readonly Dictionary<string, Snapshot> _snapshots = [];

    public int EventsReplayed { get; private set; }

    public Account Load(string stream, IReadOnlyList<AccountEvent> events)
    {
        // State computed by different Apply code cannot be trusted: rebuild from the events.
        Account? from = _snapshots.TryGetValue(stream, out Snapshot? s) && s.SchemaVersion == schemaVersion
            ? s.State
            : null;

        int start = from?.Version ?? 0;
        EventsReplayed = events.Count - start;
        Account account = Account.Replay(events.Skip(start), from);

        if (account.Version - start >= every) _snapshots[stream] = new Snapshot(schemaVersion, account);
        return account;
    }

    // Simulates reading the same storage with a newer version of the application.
    public SnapshotRepository Upgrade(int newSchemaVersion)
    {
        var next = new SnapshotRepository(every, newSchemaVersion);
        foreach (var (key, value) in _snapshots) next._snapshots[key] = value;
        return next;
    }
}
