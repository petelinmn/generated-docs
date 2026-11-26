// Example: quorum reads and writes over N replicas.
// Module: system-design/06-replication-and-partitioning (advanced)
// Tests: dotnet test --filter "FullyQualifiedName~Quorum"
namespace Replication.Examples;

public sealed class QuorumRegister(int n)
{
    private readonly (long Version, string? Value)[] _replicas = new (long, string?)[n];

    // The caller names the replicas that answered: failures are explicit and repeatable.
    public void Write(long version, string value, params int[] reached)
    {
        foreach (int i in reached)
            if (version > _replicas[i].Version) _replicas[i] = (version, value);
    }

    public (long Version, string? Value) Read(params int[] asked) =>
        asked.Select(i => _replicas[i]).MaxBy(r => r.Version);
}

public static class Quorum
{
    // Every read set shares a replica with every write set.
    public static bool Overlaps(int n, int w, int r) => w + r > n;

    // Number of unavailable replicas that still leaves both operations possible.
    public static int ToleratedFailures(int n, int w, int r) => n - Math.Max(w, r);
}
