// Example: rebuilding a read model without downtime: build the new one beside, switch when caught up.
// Module: system-design/13-event-sourcing-and-cqrs (expert)
// Tests: dotnet test --filter "FullyQualifiedName~BlueGreen"
namespace EventSourcing.Examples;

public sealed class ProjectionSwitch(BalanceProjection initial)
{
    private BalanceProjection? _candidate;

    public BalanceProjection Live { get; private set; } = initial;

    public void StartRebuild() => _candidate = new BalanceProjection();

    // Both consume the stream; only the live one serves queries.
    public void Process(IReadOnlyList<Stored> log, int batch)
    {
        Live.CatchUp(log);
        foreach (Stored stored in log.Where(s => s.Position > (_candidate?.Checkpoint ?? long.MaxValue)).Take(batch))
            _candidate!.Handle(stored);
    }

    public bool TrySwitch(long head)
    {
        if (_candidate is null || _candidate.Checkpoint < head) return false;

        (Live, _candidate) = (_candidate, null);
        return true;
    }
}
