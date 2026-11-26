// Solutions to the exercises of system-design/10-distributed-transactions.
// Tests: dotnet test --filter "FullyQualifiedName~Solutions"
namespace Transactions.Solutions;

public enum Vote { Yes, No, Unknown }

public enum Decision { Commit, Abort }

public enum OrderStatus { Pending, Approved, Rejected }

public static class Exercises
{
    // Exercise 1: the coordinator commits only on a unanimous yes; silence counts as no.
    public static Decision Decide(IReadOnlyCollection<Vote> votes) =>
        votes.Count > 0 && votes.All(v => v == Vote.Yes) ? Decision.Commit : Decision.Abort;

    // Exercise 2: a compensation must eventually succeed, so it is retried.
    public static async Task<int> CompensateAsync(Func<Task> compensate, int maxAttempts)
    {
        for (int attempt = 1; ; attempt++)
        {
            try
            {
                await compensate();
                return attempt;
            }
            catch (Exception) when (attempt < maxAttempts)
            {
                // try again; the compensation has to be idempotent
            }
        }
    }

    // Exercise 3: Pending is a semantic lock; only it may be left.
    public static bool CanTransition(OrderStatus from, OrderStatus to) =>
        (from, to) is (OrderStatus.Pending, OrderStatus.Approved) or (OrderStatus.Pending, OrderStatus.Rejected);
}
