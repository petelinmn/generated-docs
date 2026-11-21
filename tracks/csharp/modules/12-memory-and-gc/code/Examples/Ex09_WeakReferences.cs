// Example: WeakReference<T> and ConditionalWeakTable — referencing objects without keeping them alive.
// Module: csharp/12-memory-and-gc (advanced)
// Tests: dotnet test --filter "FullyQualifiedName~WeakReferences"
using System.Runtime.CompilerServices;

namespace Examples;

public static class WeakReferences
{
    // TryGetTarget returns a strong reference while the object is alive, and false after it is collected.
    public static (bool BeforeCollect, bool AfterCollect) TryGetTarget()
    {
        WeakReference<byte[]> weak = CreateWeak();
        bool before = HasTarget(weak);
        GC.Collect();
        bool after = HasTarget(weak);
        return (before, after);
    }

    // A separate method: the strong reference returned by TryGetTarget dies with its frame.
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static bool HasTarget(WeakReference<byte[]> weak) => weak.TryGetTarget(out _);

    // ConditionalWeakTable attaches data to objects you do not own; an entry disappears with its key.
    public static (int Before, int After, string? Value) AttachedData()
    {
        var table = new ConditionalWeakTable<object, string>();
        var kept = new object();
        table.Add(kept, "kept");
        AddTemporary(table);

        int before = table.Count();
        GC.Collect();
        int after = table.Count();
        table.TryGetValue(kept, out string? value);
        GC.KeepAlive(kept);
        return (before, after, value);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference<byte[]> CreateWeak() => new(new byte[1024]);

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void AddTemporary(ConditionalWeakTable<object, string> table) =>
        table.Add(new object(), "temporary");
}
