// Example: implementing IDisposable — a simple sealed class and the Dispose(bool) pattern for a hierarchy.
// Module: csharp/12-memory-and-gc (advanced)
// Tests: dotnet test --filter "FullyQualifiedName~DisposePattern"
namespace Examples;

// The common case: a sealed class that owns other disposable objects. No finalizer needed.
public sealed class ReportWriter : IDisposable
{
    private readonly StringWriter _writer = new();
    private bool _disposed;

    public int DisposeCalls { get; private set; }

    public void WriteLine(string line)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        _writer.WriteLine(line);
    }

    // Idempotent: a second call does nothing and does not throw.
    public void Dispose()
    {
        DisposeCalls++;
        if (_disposed)
        {
            return;
        }
        _writer.Dispose();
        _disposed = true;
    }
}

// The pattern for a class that may have derived classes: a virtual Dispose(bool).
public class ResourceBase : IDisposable
{
    private bool _disposed;

    public List<string> Log { get; } = [];

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this); // a derived class may add a finalizer
    }

    // disposing == true: called from Dispose, managed objects may be touched.
    // disposing == false: called from a finalizer, other managed objects may already be finalized.
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }
        if (disposing)
        {
            Log.Add("base: managed");
        }
        _disposed = true;
    }
}

public sealed class DerivedResource : ResourceBase
{
    private bool _disposed;

    protected override void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }
        if (disposing)
        {
            Log.Add("derived: managed");
        }
        _disposed = true;
        base.Dispose(disposing); // the base class releases its resources last
    }
}
