// Example: a Bloom filter, and local versus global secondary indexes over partitions.
// Module: system-design/04-data-storage (expert)
// Tests: dotnet test --filter "FullyQualifiedName~Bloom|FullyQualifiedName~SecondaryLayout"
using System.Buffers.Binary;
using System.Collections;
using System.Security.Cryptography;
using System.Text;

namespace DataStorage.Examples;

public sealed class BloomFilter(int bits, int hashes)
{
    private readonly BitArray _bits = new(bits);

    public void Add(string key)
    {
        foreach (int i in Positions(key)) _bits[i] = true;
    }

    // false: certainly absent. true: possibly present.
    public bool MightContain(string key) => Positions(key).All(i => _bits[i]);

    // Double hashing: k positions from two halves of one digest.
    private IEnumerable<int> Positions(string key)
    {
        byte[] digest = SHA256.HashData(Encoding.UTF8.GetBytes(key));
        uint a = BinaryPrimitives.ReadUInt32LittleEndian(digest);
        uint b = BinaryPrimitives.ReadUInt32LittleEndian(digest.AsSpan(4));
        for (int i = 0; i < hashes; i++) yield return (int)((a + (ulong)i * b) % (ulong)_bits.Length);
    }
}

public static class SecondaryLayout
{
    // Partitions contacted by a query on a column that is not the partition key.
    // Local index: each partition indexes its own rows, so all must be asked.
    // Global index: one index partition lists the matches, then their data partitions are visited.
    public static int PartitionsAsked(int partitions, int matchingDataPartitions, bool globalIndex) =>
        globalIndex ? 1 + matchingDataPartitions : partitions;
}
