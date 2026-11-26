// Example: an append-only event store with optimistic concurrency and an aggregate.
// Module: system-design/13-event-sourcing-and-cqrs (basics)
// Tests: dotnet test --filter "FullyQualifiedName~EventStore"
namespace EventSourcing.Examples;

public abstract record AccountEvent;
public sealed record Deposited(decimal Amount) : AccountEvent;
public sealed record Withdrawn(decimal Amount) : AccountEvent;

public sealed record Stored(long Position, string Stream, AccountEvent Event);

// Not thread-safe: a real store makes the version check and the append one atomic operation.
public sealed class EventStore
{
    private readonly List<Stored> _log = [];

    public IReadOnlyList<Stored> Log => _log;

    public IReadOnlyList<AccountEvent> Read(string stream) =>
        [.. _log.Where(s => s.Stream == stream).Select(s => s.Event)];

    public bool TryAppend(string stream, int expectedVersion, params AccountEvent[] events)
    {
        if (Read(stream).Count != expectedVersion) return false; // someone else wrote first

        foreach (AccountEvent e in events) _log.Add(new Stored(_log.Count + 1, stream, e));
        return true;
    }
}

public sealed record Account(decimal Balance, int Version)
{
    public static readonly Account Empty = new(0, 0);

    // Apply states facts: it never rejects an event that already happened.
    public Account Apply(AccountEvent e) => e switch
    {
        Deposited d => new(Balance + d.Amount, Version + 1),
        Withdrawn w => new(Balance - w.Amount, Version + 1),
        _ => throw new InvalidOperationException($"Unknown event {e.GetType().Name}"),
    };

    public static Account Replay(IEnumerable<AccountEvent> events, Account? from = null) =>
        events.Aggregate(from ?? Empty, (state, e) => state.Apply(e));

    // Decide checks the rules and produces a new event.
    public AccountEvent Withdraw(decimal amount) =>
        amount <= Balance ? new Withdrawn(amount) : throw new InvalidOperationException("Insufficient funds.");
}
