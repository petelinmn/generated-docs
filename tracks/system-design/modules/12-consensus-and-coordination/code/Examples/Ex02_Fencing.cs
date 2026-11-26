// Example: a lease alone is not enough; the storage must check fencing tokens.
// Module: system-design/12-consensus-and-coordination (basics, advanced)
// Tests: dotnet test --filter "FullyQualifiedName~Fencing"
namespace Consensus.Examples;

// Single process, not thread-safe: it shows the protocol, not a lock service.
public sealed class LeaseService(TimeProvider clock)
{
    private DateTimeOffset _expiresAt;
    private long _token;

    // Returns a token that grows with every successful acquisition, or null if the lease is held.
    public long? TryAcquire(TimeSpan duration)
    {
        DateTimeOffset now = clock.GetUtcNow();
        if (now < _expiresAt) return null;

        _expiresAt = now + duration;
        return ++_token;
    }
}

public sealed class FencedStorage
{
    private long _highestToken;

    public string? Value { get; private set; }

    public bool TryWrite(long token, string value)
    {
        if (token < _highestToken) return false; // a holder from the past

        _highestToken = token;
        Value = value;
        return true;
    }
}
