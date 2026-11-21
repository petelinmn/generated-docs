// Example: generations, promotion of survivors, the large object heap and the pinned object heap.
// Module: csharp/12-memory-and-gc (basics and expert)
// Tests: dotnet test --filter "FullyQualifiedName~Generations"
namespace Examples;

public static class Generations
{
    // A new object starts in gen 0; every collection it survives promotes it, up to gen 2.
    public static List<int> Promotion()
    {
        var survivor = new object();
        var generations = new List<int> { GC.GetGeneration(survivor) };
        for (int i = 0; i < 3; i++)
        {
            GC.Collect();
            generations.Add(GC.GetGeneration(survivor));
        }
        return generations;
    }

    // Objects of 85,000 bytes or more go to the LOH, which is collected together with gen 2.
    // On a 64-bit runtime an array has a 24-byte header, so byte[84_976] is exactly 85,000 bytes.
    public static (int Small, int JustBelow, int Large) LargeObjects() =>
        (GC.GetGeneration(new byte[1_000]),
         GC.GetGeneration(new byte[84_975]),
         GC.GetGeneration(new byte[84_976]));

    // A pinned array from the pinned object heap (POH) is never moved; GetGeneration reports 2 for it.
    public static int PinnedArrayGeneration() =>
        GC.GetGeneration(GC.AllocateArray<byte>(16, pinned: true));

    // A gen 2 collection also collects gen 0 and gen 1, so their counters grow too.
    public static (int Gen0, int Gen1, int Gen2) CountersAfterFullCollection()
    {
        int gen0 = GC.CollectionCount(0), gen1 = GC.CollectionCount(1), gen2 = GC.CollectionCount(2);
        GC.Collect(2);
        return (GC.CollectionCount(0) - gen0, GC.CollectionCount(1) - gen1, GC.CollectionCount(2) - gen2);
    }
}
