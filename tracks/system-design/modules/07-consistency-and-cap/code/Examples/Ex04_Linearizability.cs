// Example: checking whether a history of one register is linearizable.
// Module: system-design/07-consistency-and-cap (expert)
// Tests: dotnet test --filter "FullyQualifiedName~Linearizability"
namespace Consistency.Examples;

// An operation was called at Start and returned at End. Value: written, or observed by a read.
public sealed record Operation(bool IsWrite, int Value, int Start, int End);

public static class Linearizability
{
    // Is there an order of all operations that respects real time and register semantics?
    // Exhaustive search: exponential, only for small histories.
    public static bool Check(IReadOnlyList<Operation> history, int initial = 0)
    {
        return Search([.. history], initial);

        static bool Search(List<Operation> remaining, int value)
        {
            if (remaining.Count == 0) return true;

            // An operation may go first only if no other one finished before it started.
            int earliestEnd = remaining.Min(o => o.End);
            foreach (Operation op in remaining.Where(o => o.Start <= earliestEnd))
            {
                if (!op.IsWrite && op.Value != value) continue;

                var rest = new List<Operation>(remaining);
                rest.Remove(op);
                if (Search(rest, op.IsWrite ? op.Value : value)) return true;
            }
            return false;
        }
    }
}
