// Solutions to the exercises of system-design/12-consensus-and-coordination.
// Tests: dotnet test --filter "FullyQualifiedName~Solutions"
namespace Consensus.Solutions;

public static class Exercises
{
    // Exercise 1: failed servers a majority-based cluster survives.
    public static int ToleratedFailures(int clusterSize) => (clusterSize - 1) / 2;

    // Exercise 2: a randomized election timeout from [min, max].
    public static TimeSpan ElectionTimeout(TimeSpan min, TimeSpan max, Random random) =>
        min + (max - min) * random.NextDouble();

    // Exercise 3: the highest index a leader may mark committed.
    // matchIndex includes the leader itself; logTerms[i] is the term of entry i + 1.
    public static int CommitIndex(IReadOnlyList<int> matchIndex, IReadOnlyList<long> logTerms, long currentTerm, int committed)
    {
        for (int n = logTerms.Count; n > committed; n--)
        {
            bool majority = matchIndex.Count(m => m >= n) > matchIndex.Count / 2;
            // Entries of earlier terms are never committed by counting replicas alone.
            if (majority && logTerms[n - 1] == currentTerm) return n;
        }
        return committed;
    }
}
