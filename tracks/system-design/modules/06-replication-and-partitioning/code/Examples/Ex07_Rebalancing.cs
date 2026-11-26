// Example: many logical partitions, few nodes: adding a node moves whole partitions and no more than needed.
// Module: system-design/06-replication-and-partitioning (expert)
// Tests: dotnet test --filter "FullyQualifiedName~Rebalancing"
namespace Replication.Examples;

public static class Rebalancing
{
    // assignment[p] = node of logical partition p.
    public static int[] Initial(int partitions, int nodes) => [.. Enumerable.Range(0, partitions).Select(p => p % nodes)];

    public static int[] AddNode(IReadOnlyList<int> assignment, int newNode)
    {
        int[] result = [.. assignment];
        int target = result.Length / (newNode + 1);

        for (int moved = 0; moved < target; moved++)
        {
            // Take from whoever has most, so the old nodes end up even.
            int donor = result.Where(n => n != newNode).CountBy(n => n).MaxBy(g => g.Value).Key;
            result[Array.LastIndexOf(result, donor)] = newNode;
        }
        return result;
    }
}
