// Example: how many partitions change owner on a rebalance, with two assignment strategies.
// Module: system-design/08-messaging-and-events (expert)
// Tests: dotnet test --filter "FullyQualifiedName~Assignment"
namespace Messaging.Examples;

public static class Assignment
{
    // result[partition] = consumer. Recomputed from scratch.
    public static string[] RoundRobin(int partitions, IReadOnlyList<string> consumers) =>
        [.. Enumerable.Range(0, partitions).Select(p => consumers[p % consumers.Count])];

    // Keeps what it can: only orphaned partitions and the surplus of overloaded consumers move.
    public static string[] Sticky(IReadOnlyList<string> previous, IReadOnlyList<string> consumers)
    {
        var load = consumers.ToDictionary(c => c, _ => 0);
        var result = new string?[previous.Count];
        int cap = (previous.Count + consumers.Count - 1) / consumers.Count;

        for (int p = 0; p < previous.Count; p++)
            if (load.TryGetValue(previous[p], out int count) && count < cap)
            {
                result[p] = previous[p];
                load[previous[p]]++;
            }

        for (int p = 0; p < previous.Count; p++)
            if (result[p] is null)
            {
                string target = load.MinBy(l => l.Value).Key;
                result[p] = target;
                load[target]++;
            }
        return result!;
    }

    public static int Moved(IReadOnlyList<string> before, IReadOnlyList<string> after) =>
        before.Zip(after).Count(p => p.First != p.Second);
}
