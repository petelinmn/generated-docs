// Example: monotonic reads: a client never goes back in time.
// Module: system-design/07-consistency-and-cap (advanced)
// Tests: dotnet test --filter "FullyQualifiedName~Session"
namespace Consistency.Examples;

public sealed record Replica(long Version, string Value);

public sealed class Session
{
    private long _seen;

    // Returns null when the replica is older than what this client has already observed.
    public string? Read(Replica replica)
    {
        if (replica.Version < _seen) return null;
        _seen = replica.Version;
        return replica.Value;
    }
}
