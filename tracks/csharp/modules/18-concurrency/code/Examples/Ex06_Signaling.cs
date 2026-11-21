// Example: signaling between threads — a start gate (ManualResetEventSlim) and a finish line (CountdownEvent).
// Module: csharp/18-concurrency (advanced)
// Tests: dotnet test --filter "FullyQualifiedName~Signaling"
namespace Examples;

public sealed class StartGate : IDisposable
{
    private readonly ManualResetEventSlim _gate = new(initialState: false);
    private readonly CountdownEvent _finished;
    private int _passed;

    public StartGate(int workers)
    {
        _finished = new CountdownEvent(workers);
        for (int i = 0; i < workers; i++)
        {
            new Thread(() =>
            {
                _gate.Wait(); // every worker blocks here until Open is called
                Interlocked.Increment(ref _passed);
                _finished.Signal();
            }).Start();
        }
    }

    public int Passed => Volatile.Read(ref _passed);

    public int Remaining => _finished.CurrentCount;

    // A manual-reset event stays signaled, so it releases all waiting threads at once.
    public void Open() => _gate.Set();

    public bool WaitAll(TimeSpan timeout) => _finished.Wait(timeout);

    public void Dispose()
    {
        _gate.Dispose();
        _finished.Dispose();
    }
}
