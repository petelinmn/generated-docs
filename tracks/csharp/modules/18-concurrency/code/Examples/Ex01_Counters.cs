// Example: a shared counter without synchronization, with lock (Monitor and System.Threading.Lock) and with Interlocked.
// Module: csharp/18-concurrency (basics)
// Tests: dotnet test --filter "FullyQualifiedName~Counters"
namespace Examples;

public interface ICounter
{
    void Increment();

    int Value { get; }
}

// count++ is read-modify-write: two threads can read the same value and lose an update.
public sealed class UnsafeCounter : ICounter
{
    private int _value;

    public void Increment() => _value++;

    public int Value => _value;
}

// The classic lock: any private reference-type object works as a Monitor.
public sealed class MonitorCounter : ICounter
{
    private readonly object _gate = new();
    private int _value;

    public void Increment()
    {
        lock (_gate)
        {
            _value++;
        }
    }

    public int Value
    {
        get
        {
            lock (_gate)
            {
                return _value;
            }
        }
    }
}

// .NET 9+: a dedicated Lock object; the compiler turns lock(_gate) into using (_gate.EnterScope()).
public sealed class LockCounter : ICounter
{
    private readonly Lock _gate = new();
    private int _value;

    public void Increment()
    {
        lock (_gate)
        {
            _value++;
        }
    }

    public int Value
    {
        get
        {
            lock (_gate)
            {
                return _value;
            }
        }
    }
}

// An atomic hardware operation: no lock needed for a simple counter.
public sealed class InterlockedCounter : ICounter
{
    private int _value;

    public void Increment() => Interlocked.Increment(ref _value);

    public int Value => Volatile.Read(ref _value);
}

public static class CounterRunner
{
    // Starts `threads` threads that each call Increment `iterations` times, and waits for all of them.
    public static int Run(ICounter counter, int threads, int iterations)
    {
        var workers = new Thread[threads];
        for (int t = 0; t < threads; t++)
        {
            workers[t] = new Thread(() =>
            {
                for (int i = 0; i < iterations; i++)
                {
                    counter.Increment();
                }
            });
            workers[t].Start();
        }
        foreach (Thread worker in workers)
        {
            worker.Join();
        }
        return counter.Value;
    }
}
