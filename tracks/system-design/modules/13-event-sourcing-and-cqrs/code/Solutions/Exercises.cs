// Solutions to the exercises of system-design/13-event-sourcing-and-cqrs.
// Tests: dotnet test --filter "FullyQualifiedName~Solutions"
using EventSourcing.Examples;

namespace EventSourcing.Solutions;

public static class Exercises
{
    // Exercise 1: the balance as it was after the first `version` events.
    public static decimal BalanceAt(IReadOnlyList<AccountEvent> events, int version) =>
        Account.Replay(events.Take(version)).Balance;

    // Exercise 2: start from a snapshot and replay only what came after it.
    public static Account Load(Account? snapshot, IReadOnlyList<AccountEvent> events) =>
        Account.Replay(events.Skip(snapshot?.Version ?? 0), snapshot);

    // Exercise 3: reload and decide again after a concurrency conflict.
    public static bool TryWithdraw(EventStore store, string stream, decimal amount, int maxAttempts, Action? beforeAppend = null)
    {
        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            Account account = Account.Replay(store.Read(stream));
            if (amount > account.Balance) return false; // a business refusal is not retried

            AccountEvent decision = account.Withdraw(amount);
            beforeAppend?.Invoke(); // lets a test slip in a concurrent write
            if (store.TryAppend(stream, account.Version, decision)) return true;
        }
        return false;
    }
}
