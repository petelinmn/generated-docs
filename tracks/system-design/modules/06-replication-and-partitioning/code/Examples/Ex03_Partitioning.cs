// Example: range and hash partitioning, hot partitions and key salting.
// Module: system-design/06-replication-and-partitioning (basics, advanced)
// Tests: dotnet test --filter "FullyQualifiedName~Partitioning"
using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;

namespace Replication.Examples;

public static class Partitioning
{
    // upperBounds are exclusive and sorted; keys beyond the last bound go to the last partition.
    public static int ByRange(long key, params long[] upperBounds)
    {
        for (int i = 0; i < upperBounds.Length; i++)
            if (key < upperBounds[i]) return i;
        return upperBounds.Length;
    }

    // A stable hash: every process must route a key the same way.
    public static int ByHash(string key, int partitions)
    {
        uint hash = BinaryPrimitives.ReadUInt32BigEndian(SHA256.HashData(Encoding.UTF8.GetBytes(key)));
        return (int)(hash % (uint)partitions);
    }

    // Splits one hot key into several; readers must then query every bucket.
    public static string Salted(string key, long sequence, int buckets) => $"{key}#{sequence % buckets}";

    public static IEnumerable<string> AllBuckets(string key, int buckets) =>
        Enumerable.Range(0, buckets).Select(i => $"{key}#{i}");

    public static int[] Load(IEnumerable<int> assignments, int partitions)
    {
        int[] load = new int[partitions];
        foreach (int p in assignments) load[p]++;
        return load;
    }
}
