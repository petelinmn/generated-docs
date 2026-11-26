// Example: sloppy quorum with hinted handoff, as described in the Dynamo paper.
// Module: system-design/06-replication-and-partitioning (expert)
// Tests: dotnet test --filter "FullyQualifiedName~Handoff"
namespace Replication.Examples;

// Nodes 0..n-1 are the preferred replicas of one key, the rest are stand-ins.
public sealed class SloppyStore(int n, int total)
{
    private readonly string?[] _data = new string?[total];
    private readonly Dictionary<int, int> _hints = []; // stand-in -> intended node
    private readonly HashSet<int> _down = [];

    public void Down(int node) => _down.Add(node);
    public void Up(int node) => _down.Remove(node);
    public string? ValueAt(int node) => _data[node];

    // Writes to the first n healthy nodes. Returns how many accepted.
    public int Write(string value)
    {
        var missed = new Queue<int>(Enumerable.Range(0, n).Where(_down.Contains));
        int written = 0;
        for (int node = 0; node < total && written < n; node++)
        {
            if (_down.Contains(node)) continue;
            _data[node] = value;
            if (node >= n) _hints[node] = missed.Dequeue();
            written++;
        }
        return written;
    }

    public string?[] ReadPreferred() => _data[..n];

    public void Handoff()
    {
        foreach (var (standIn, intended) in _hints.Where(h => !_down.Contains(h.Value)).ToList())
        {
            _data[intended] = _data[standIn];
            _data[standIn] = null;
            _hints.Remove(standIn);
        }
    }
}
