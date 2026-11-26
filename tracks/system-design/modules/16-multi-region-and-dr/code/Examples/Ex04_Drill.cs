// Example: the worst case of DNS failover, a write-once backup store, and a drill report.
// Module: system-design/16-multi-region-and-dr (expert)
// Tests: dotnet test --filter "FullyQualifiedName~Drill|FullyQualifiedName~Immutable"
namespace MultiRegion.Examples;

public static class DnsFailover
{
    // Detection needs several failed probes; then resolvers may keep the old address for the whole TTL.
    public static TimeSpan WorstCase(TimeSpan probeInterval, int failuresNeeded, TimeSpan recordUpdate, TimeSpan ttl) =>
        probeInterval * failuresNeeded + recordUpdate + ttl;
}

// A model of write-once retention: nothing can be removed or replaced before its date.
public sealed class ImmutableStore(TimeProvider clock)
{
    private readonly Dictionary<string, (byte[] Data, DateTimeOffset RetainUntil)> _objects = [];

    public bool TryPut(string name, byte[] data, TimeSpan retention)
    {
        if (_objects.TryGetValue(name, out var o) && clock.GetUtcNow() < o.RetainUntil) return false;
        _objects[name] = ([.. data], clock.GetUtcNow() + retention);
        return true;
    }

    public bool TryDelete(string name) =>
        _objects.TryGetValue(name, out var o) && clock.GetUtcNow() >= o.RetainUntil && _objects.Remove(name);
}

public sealed record DrillResult(TimeSpan Recovery, TimeSpan DataLoss, bool MeetsRto, bool MeetsRpo);

public static class Drill
{
    public static DrillResult Evaluate(
        DateTimeOffset failure, DateTimeOffset serviceRestored, DateTimeOffset newestRecoveredWrite,
        TimeSpan rto, TimeSpan rpo)
    {
        TimeSpan recovery = serviceRestored - failure, loss = failure - newestRecoveredWrite;
        return new(recovery, loss, recovery <= rto, loss <= rpo);
    }
}
