// Example: measuring allocations with GC.GetAllocatedBytesForCurrentThread.
// Module: csharp/12-memory-and-gc (advanced)
// Tests: dotnet test --filter "FullyQualifiedName~Allocations"
using System.Collections;

namespace Examples;

public static class Allocations
{
    // Runs the action once to warm it up (JIT, static constructors), then measures the second run.
    public static long Measure(Action action)
    {
        action();
        long before = GC.GetAllocatedBytesForCurrentThread();
        action();
        return GC.GetAllocatedBytesForCurrentThread() - before;
    }

    // Each int added to a non-generic collection is boxed: a separate heap object per value.
    public static long BoxingIntoArrayList(int count)
    {
        var list = new ArrayList(count);
        return Measure(() =>
        {
            list.Clear();
            for (int i = 0; i < count; i++)
            {
                list.Add(i);
            }
        });
    }

    // List<int> stores the values inline in its array: no allocation per element.
    public static long GenericList(int count)
    {
        var list = new List<int>(count);
        return Measure(() =>
        {
            list.Clear();
            for (int i = 0; i < count; i++)
            {
                list.Add(i);
            }
        });
    }

    // Sizes of the smallest objects on a 64-bit runtime.
    public static (long Object, long EmptyArray, long String10) ObjectSizes()
    {
        object? sink = null;
        long obj = Measure(() => sink = new object());
        long array = Measure(() => sink = new byte[0]);
        long str = Measure(() => sink = new string('x', 10));
        GC.KeepAlive(sink);
        return (obj, array, str);
    }
}
