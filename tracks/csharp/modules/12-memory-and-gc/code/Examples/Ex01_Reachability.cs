// Example: the GC frees objects that are unreachable from roots — including reference cycles.
// Module: csharp/12-memory-and-gc (basics)
// Tests: dotnet test --filter "FullyQualifiedName~Reachability"
using System.Runtime.CompilerServices;

namespace Examples;

public sealed class Holder
{
    public object? Item { get; set; }
}

public sealed class Node
{
    public Node? Next { get; set; }
}

public static class Reachability
{
    // The object is referenced only from holder.Item: no local variable of this method points to it,
    // so the result does not depend on how long the JIT keeps locals alive (Debug vs Release).
    public static (bool WhileReferenced, bool AfterRelease) Demo()
    {
        var holder = new Holder();
        WeakReference weak = PutNewObject(holder);

        GC.Collect();
        bool whileReferenced = weak.IsAlive;   // reachable: local holder → holder.Item

        holder.Item = null;                    // drop the only strong reference
        GC.Collect();
        return (whileReferenced, weak.IsAlive);
    }

    // Two objects referencing each other, but nothing else references them: both are garbage.
    public static bool CycleIsCollected()
    {
        WeakReference weak = MakeCycle();
        GC.Collect();
        return !weak.IsAlive;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference PutNewObject(Holder holder)
    {
        var item = new object();
        holder.Item = item;
        return new WeakReference(item);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference MakeCycle()
    {
        var a = new Node();
        var b = new Node { Next = a };
        a.Next = b;
        return new WeakReference(a);
    }
}
