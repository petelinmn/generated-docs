// Example: queries across shards: pagination and averages need care.
// Module: system-design/06-replication-and-partitioning (expert)
// Tests: dotnet test --filter "FullyQualifiedName~ScatterGather"
namespace Replication.Examples;

public static class ScatterGather
{
    // Every shard must return offset + size rows: the requested page may come entirely from one of them.
    public static (int[] Page, int RowsTransferred) Page(IReadOnlyList<int[]> shards, int offset, int size)
    {
        int[][] parts = [.. shards.Select(s => s.Order().Take(offset + size).ToArray())];
        return ([.. parts.SelectMany(p => p).Order().Skip(offset).Take(size)], parts.Sum(p => p.Length));
    }

    // Combine sums and counts, never the averages themselves.
    public static double Average(IEnumerable<(double Sum, long Count)> partials)
    {
        var list = partials.ToList();
        return list.Sum(p => p.Sum) / list.Sum(p => p.Count);
    }
}
