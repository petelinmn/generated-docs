// Example: wrapping an OS handle in a SafeHandle instead of writing a finalizer.
// Module: csharp/12-memory-and-gc (advanced)
// Tests: dotnet test --filter "FullyQualifiedName~SafeHandles"
using Microsoft.Win32.SafeHandles;

namespace Examples;

// A fake "operating system": hands out numeric handles and records which ones were closed.
public sealed class FakeOs
{
    private int _next = 100;

    public List<nint> Closed { get; } = [];

    public nint Open() => _next++;

    public void Close(nint handle) => Closed.Add(handle);
}

// SafeHandle already has a critical finalizer, reference counting and "release exactly once".
// All we write is how to release the handle.
public sealed class FakeFileHandle : SafeHandleZeroOrMinusOneIsInvalid
{
    private readonly FakeOs _os;

    public FakeFileHandle(FakeOs os, nint handle) : base(ownsHandle: true)
    {
        _os = os;
        SetHandle(handle);
    }

    protected override bool ReleaseHandle()
    {
        _os.Close(handle);
        return true;
    }
}

public static class SafeHandles
{
    public static (List<nint> Closed, bool IsClosed) DisposeTwice()
    {
        var os = new FakeOs();
        var file = new FakeFileHandle(os, os.Open());
        file.Dispose();
        file.Dispose(); // safe: the handle is released only once
        return (os.Closed, file.IsClosed);
    }

    // 0 and -1 mean "no handle": SafeHandleZeroOrMinusOneIsInvalid does not call ReleaseHandle for them.
    public static int InvalidHandleIsNotReleased()
    {
        var os = new FakeOs();
        using (var file = new FakeFileHandle(os, -1))
        {
        }
        return os.Closed.Count;
    }
}
