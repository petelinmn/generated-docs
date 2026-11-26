// Example: chain replication: writes enter at the head, reads are served by the tail.
// Module: system-design/06-replication-and-partitioning (expert)
// Tests: dotnet test --filter "FullyQualifiedName~Chain"
namespace Replication.Examples;

public sealed class Chain(int length)
{
    private readonly List<Dictionary<string, string>> _servers =
        [.. Enumerable.Range(0, length).Select(_ => new Dictionary<string, string>())];

    // stopAfter models an update that is still travelling down the chain. true = acknowledged.
    public bool Write(string key, string value, int stopAfter = int.MaxValue)
    {
        for (int i = 0; i < _servers.Count && i < stopAfter; i++) _servers[i][key] = value;
        return stopAfter >= _servers.Count;
    }

    // Only what reached the tail is visible, so a read never returns a half-replicated value.
    public string? Read(string key) => _servers[^1].GetValueOrDefault(key);

    public void Fail(int index) => _servers.RemoveAt(index);
}
