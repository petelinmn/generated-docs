// Example: a read model built from events, with a checkpoint.
// Module: system-design/13-event-sourcing-and-cqrs (basics, advanced)
// Tests: dotnet test --filter "FullyQualifiedName~Projection"
namespace EventSourcing.Examples;

public sealed class BalanceProjection
{
    private readonly Dictionary<string, decimal> _balances = [];

    public long Checkpoint { get; private set; }

    public decimal BalanceOf(string account) => _balances.GetValueOrDefault(account);

    // In a database the checkpoint and the read model are updated in one transaction.
    public void Handle(Stored stored)
    {
        if (stored.Position <= Checkpoint) return; // redelivery

        decimal delta = stored.Event switch
        {
            Deposited d => d.Amount,
            Withdrawn w => -w.Amount,
            _ => 0, // a projection may ignore events it does not need
        };
        _balances[stored.Stream] = BalanceOf(stored.Stream) + delta;
        Checkpoint = stored.Position;
    }

    public void CatchUp(IReadOnlyList<Stored> log)
    {
        foreach (Stored stored in log) Handle(stored);
    }
}
