// Example: finalizers — when they run, how Dispose suppresses them, and why such objects live longer.
// Module: csharp/12-memory-and-gc (advanced)
// Tests: dotnet test --filter "FullyQualifiedName~Finalizers"
using System.Runtime.CompilerServices;

namespace Examples;

// Records what happened to it in a probe object that outlives it.
public sealed class FinalizerProbe
{
    public volatile bool Finalized;
}

public class FinalizableResource(FinalizerProbe probe) : IDisposable
{
    public void Dispose()
    {
        // release the resource here...
        GC.SuppressFinalize(this); // ...and tell the GC that the finalizer is no longer needed
    }

    // Runs on the finalizer thread at an unknown moment after the object becomes unreachable.
    ~FinalizableResource() => probe.Finalized = true;
}

public static class Finalizers
{
    public static bool RunsWhenNotDisposed()
    {
        var probe = new FinalizerProbe();
        Abandon(probe, dispose: false);
        GC.Collect();
        GC.WaitForPendingFinalizers();
        return probe.Finalized;
    }

    public static bool SuppressedByDispose()
    {
        var probe = new FinalizerProbe();
        Abandon(probe, dispose: true);
        GC.Collect();
        GC.WaitForPendingFinalizers();
        return probe.Finalized;
    }

    // The first collection only queues the finalizer: the object itself is still in memory.
    // trackResurrection: true keeps the weak reference valid until the memory is really freed.
    public static (bool AliveAfterFirstGc, bool AliveAfterSecondGc) SurvivesOneMoreCollection()
    {
        WeakReference weak = CreateTracked();
        GC.Collect();
        bool afterFirst = weak.IsAlive;
        GC.WaitForPendingFinalizers();
        GC.Collect();
        return (afterFirst, weak.IsAlive);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void Abandon(FinalizerProbe probe, bool dispose)
    {
        var resource = new FinalizableResource(probe);
        if (dispose)
        {
            resource.Dispose();
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference CreateTracked() =>
        new(new FinalizableResource(new FinalizerProbe()), trackResurrection: true);
}
