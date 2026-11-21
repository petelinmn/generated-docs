// Example: pinning — keeping the GC from moving an object while native code or a pointer uses it.
// Module: csharp/12-memory-and-gc (expert)
// Tests: dotnet test --filter "FullyQualifiedName~Pinning"
using System.Runtime.InteropServices;

namespace Examples;

public static class Pinning
{
    // fixed pins the array only for the duration of the block.
    public static unsafe int SumWithPointer(byte[] data)
    {
        int sum = 0;
        fixed (byte* start = data)
        {
            for (byte* p = start; p < start + data.Length; p++)
            {
                sum += *p;
            }
        }
        return sum;
    }

    // A pinned GCHandle keeps the object in place until Free — even across collections.
    public static bool PinnedHandleAddressIsStable()
    {
        var buffer = new byte[100];
        GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
        try
        {
            nint before = handle.AddrOfPinnedObject();
            GC.Collect(2, GCCollectionMode.Forced, blocking: true, compacting: true);
            return before == handle.AddrOfPinnedObject();
        }
        finally
        {
            handle.Free(); // a forgotten Free pins the object forever and fragments the heap
        }
    }
}
