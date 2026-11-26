// Example: data loss (RPO) and downtime (RTO) of a failover, and why replicas are not backups.
// Module: system-design/16-multi-region-and-dr (basics)
// Tests: dotnet test --filter "FullyQualifiedName~Recovery"
namespace MultiRegion.Examples;

public sealed record Write(TimeSpan At, string Key, string? Value); // a null value is a delete

public static class Recovery
{
    // Writes younger than the replication lag have not reached the other region yet.
    public static IReadOnlyList<Write> LostOnFailover(IEnumerable<Write> writes, TimeSpan failureAt, TimeSpan lag) =>
        [.. writes.Where(w => w.At <= failureAt && w.At > failureAt - lag)];

    public static TimeSpan Downtime(TimeSpan detect, TimeSpan decide, TimeSpan promote, TimeSpan redirect) =>
        detect + decide + promote + redirect;

    // Replays a log up to a moment: a replica follows deletes, point-in-time recovery stops before them.
    public static Dictionary<string, string> StateAt(IEnumerable<Write> log, TimeSpan moment)
    {
        var state = new Dictionary<string, string>();
        foreach (Write w in log.Where(w => w.At <= moment).OrderBy(w => w.At))
            if (w.Value is null) state.Remove(w.Key);
            else state[w.Key] = w.Value;
        return state;
    }
}
