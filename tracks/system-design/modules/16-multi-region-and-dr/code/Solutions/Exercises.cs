// Solutions to the exercises of system-design/16-multi-region-and-dr.
// Tests: dotnet test --filter "FullyQualifiedName~Solutions"
namespace MultiRegion.Solutions;

public sealed record Region(string Name, string Jurisdiction, int LatencyMs, bool Healthy);

public enum Strategy { BackupAndRestore, PilotLight, WarmStandby, MultiSite }

public static class Exercises
{
    // Exercise 1: the nearest healthy region that is allowed to hold the user data.
    public static Region? Route(IEnumerable<Region> regions, string jurisdiction) =>
        regions.Where(r => r.Healthy && r.Jurisdiction == jurisdiction).MinBy(r => r.LatencyMs);

    // Exercise 2: the cheapest strategy whose achievable recovery time fits the target.
    public static Strategy? Cheapest(TimeSpan rto, IReadOnlyDictionary<Strategy, TimeSpan> achievable) =>
        Enum.GetValues<Strategy>() // declared from the cheapest to the most expensive
            .Where(s => achievable.TryGetValue(s, out TimeSpan t) && t <= rto)
            .Cast<Strategy?>()
            .FirstOrDefault();

    // Exercise 3: keep the newest `daily` backups and the newest backup of each of the last `monthly` months.
    public static IReadOnlySet<DateOnly> Keep(IEnumerable<DateOnly> backups, int daily, int monthly)
    {
        DateOnly[] newestFirst = [.. backups.OrderDescending()];
        var keep = new HashSet<DateOnly>(newestFirst.Take(daily));
        keep.UnionWith(newestFirst.GroupBy(d => (d.Year, d.Month)).Take(monthly).Select(g => g.First()));
        return keep;
    }
}
