// Example: GC settings and controls — flavor, latency mode, forced compaction, no-GC regions.
// Module: csharp/12-memory-and-gc (expert)
// Tests: dotnet test --filter "FullyQualifiedName~GcControl"
using System.Runtime;

namespace Examples;

public static class GcControl
{
    public static (bool IsServer, GCLatencyMode Latency, int MaxGeneration) Settings() =>
        (GCSettings.IsServerGC, GCSettings.LatencyMode, GC.MaxGeneration);

    // A plain GC.Collect() compacts only if it is worth it; the overload can force compaction.
    public static bool ForcedCompaction()
    {
        GC.Collect(2, GCCollectionMode.Forced, blocking: true, compacting: true);
        return GC.GetGCMemoryInfo().Compacted;
    }

    // CompactOnce applies to the next full blocking GC and then resets to Default.
    public static (bool Compacted, GCLargeObjectHeapCompactionMode ModeAfter) CompactLargeObjectHeapOnce()
    {
        GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
        GC.Collect();
        return (GC.GetGCMemoryInfo().Compacted, GCSettings.LargeObjectHeapCompactionMode);
    }

    // Inside a no-GC region allocations up to the requested size do not trigger a collection.
    public static (bool Started, GCLatencyMode Inside, int Gen0Collections) NoGcRegion()
    {
        bool started = GC.TryStartNoGCRegion(1_000_000);
        GCLatencyMode inside = GCSettings.LatencyMode;
        int before = GC.CollectionCount(0);
        for (int i = 0; i < 1_000; i++)
        {
            _ = new byte[100];
        }
        int collections = GC.CollectionCount(0) - before;
        if (started)
        {
            GC.EndNoGCRegion();
        }
        return (started, inside, collections);
    }
}
