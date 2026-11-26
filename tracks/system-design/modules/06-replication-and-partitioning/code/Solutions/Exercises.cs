// Solutions to the exercises of system-design/06-replication-and-partitioning.
// Tests: dotnet test --filter "FullyQualifiedName~Solutions"
namespace Replication.Solutions;

public static class Exercises
{
    // Exercise 1: the smallest read quorum that overlaps with a write quorum of w.
    public static int MinReadQuorum(int n, int w)
    {
        if (w < 1 || w > n) throw new ArgumentOutOfRangeException(nameof(w));
        return n - w + 1;
    }

    // Exercise 2: the vector that has seen everything both inputs have seen.
    public static Dictionary<string, long> MergeVectors(
        IReadOnlyDictionary<string, long> a, IReadOnlyDictionary<string, long> b) =>
        a.Keys.Union(b.Keys).ToDictionary(
            node => node,
            node => Math.Max(a.GetValueOrDefault(node), b.GetValueOrDefault(node)));

    // Exercise 3: load of the busiest partition relative to a perfectly even one.
    public static double Skew(IReadOnlyCollection<int> load)
    {
        double total = load.Sum();
        return total == 0 ? 1 : load.Max() / (total / load.Count);
    }
}
