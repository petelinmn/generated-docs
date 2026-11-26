// Example: consistent hashing with virtual nodes.
// Module: system-design/02-scalability-and-load-balancing (advanced)
// Tests: dotnet test --filter "FullyQualifiedName~Hashing"
using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;

namespace Scaling.Examples;

public static class StableHash
{
    // string.GetHashCode() is randomized per process, so it cannot be used for routing.
    public static uint Of(string value) =>
        BinaryPrimitives.ReadUInt32BigEndian(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
}

// Not thread-safe while nodes are being added: build the ring, then only read it.
public sealed class HashRing
{
    private readonly int _virtualNodes;
    private readonly SortedDictionary<uint, string> _ring = [];
    private uint[] _points = [];

    public HashRing(IEnumerable<string> nodes, int virtualNodes = 100)
    {
        _virtualNodes = virtualNodes;
        foreach (string node in nodes) Add(node);
    }

    public void Add(string node)
    {
        for (int i = 0; i < _virtualNodes; i++) _ring[StableHash.Of($"{node}#{i}")] = node;
        _points = [.. _ring.Keys];
    }

    public string NodeFor(string key)
    {
        if (_points.Length == 0) throw new InvalidOperationException("The ring is empty.");
        int i = Array.BinarySearch(_points, StableHash.Of(key));
        if (i < 0) i = ~i;                         // first point clockwise from the key
        return _ring[_points[i % _points.Length]]; // wrap around the end of the ring
    }
}

public static class ModuloSharding
{
    public static int ShardFor(string key, int shards) => (int)(StableHash.Of(key) % (uint)shards);
}
