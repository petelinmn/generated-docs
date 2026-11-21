// Example: a deadlock from opposite lock order, detected with Monitor.TryEnter, and the fix: a global lock order.
// Module: csharp/18-concurrency (advanced)
// Tests: dotnet test --filter "FullyQualifiedName~Deadlock"
namespace Examples;

public static class DeadlockDemo
{
    // Thread 1 holds A and wants B, thread 2 holds B and wants A.
    // The barriers make the bad interleaving happen every time; TryEnter with a timeout reports it.
    public static (bool FirstGotBoth, bool SecondGotBoth) OppositeOrder(TimeSpan timeout)
    {
        object a = new(), b = new();
        bool first = false, second = false;
        using var bothHoldOne = new Barrier(2);
        using var bothTried = new Barrier(2);

        var t1 = new Thread(() =>
        {
            lock (a)
            {
                bothHoldOne.SignalAndWait();
                first = Monitor.TryEnter(b, timeout);
                if (first) Monitor.Exit(b);
                bothTried.SignalAndWait();
            }
        });
        var t2 = new Thread(() =>
        {
            lock (b)
            {
                bothHoldOne.SignalAndWait();
                second = Monitor.TryEnter(a, timeout);
                if (second) Monitor.Exit(a);
                bothTried.SignalAndWait();
            }
        });
        t1.Start(); t2.Start();
        t1.Join(); t2.Join();
        return (first, second);
    }
}

public sealed class Account(int id, decimal balance)
{
    public int Id { get; } = id;

    public decimal Balance { get; private set; } = balance;

    internal readonly Lock Gate = new();

    // Both accounts are always locked in the same order (by Id), so no cycle of waits can form.
    public static void Transfer(Account from, Account to, decimal amount)
    {
        Account first = from.Id < to.Id ? from : to;
        Account second = from.Id < to.Id ? to : from;
        lock (first.Gate)
        {
            lock (second.Gate)
            {
                from.Balance -= amount;
                to.Balance += amount;
            }
        }
    }
}
