// Example: the lookup table of Maglev hashing (Eisenbud et al., NSDI 2016).
// Module: system-design/02-scalability-and-load-balancing (expert)
// Tests: dotnet test --filter "FullyQualifiedName~Maglev"
namespace Scaling.Examples;

public sealed class MaglevTable
{
    private readonly string[] _table;

    // size must be a prime larger than the number of nodes: then every skip visits every slot.
    public MaglevTable(IReadOnlyList<string> nodes, int size)
    {
        // With a composite size the search for a free slot could loop forever.
        if (size <= nodes.Count || !IsPrime(size)) throw new ArgumentException("A prime larger than the node count is required.", nameof(size));

        var offset = new int[nodes.Count];
        var skip = new int[nodes.Count];
        for (int i = 0; i < nodes.Count; i++)
        {
            offset[i] = (int)(StableHash.Of($"offset:{nodes[i]}") % (uint)size);
            skip[i] = (int)(StableHash.Of($"skip:{nodes[i]}") % (uint)(size - 1)) + 1;
        }

        _table = new string[size];
        var next = new int[nodes.Count];
        // Nodes take turns, each claiming the first free slot of its own preference order.
        for (int filled = 0, i = 0; filled < size; i = (i + 1) % nodes.Count)
        {
            int slot;
            do slot = (int)((offset[i] + (long)next[i]++ * skip[i]) % size);
            while (_table[slot] is not null);

            _table[slot] = nodes[i];
            filled++;
        }
    }

    private static bool IsPrime(int n) =>
        n > 1 && Enumerable.Range(2, (int)Math.Sqrt(n) - 1).All(d => n % d != 0);

    public IReadOnlyList<string> Slots => _table;

    public string NodeFor(string key) => _table[(int)(StableHash.Of(key) % (uint)_table.Length)];
}
