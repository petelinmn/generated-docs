// Solutions to the exercises of module csharp/18-concurrency.
// Tests: dotnet test --filter "FullyQualifiedName~Tests.S0"
using System.Collections.Concurrent;

namespace Solutions;

// Exercise 1: several fields must change together, so one lock protects all of them.
public sealed class SafeStatistics
{
    private readonly Lock _gate = new();
    private long _count;
    private long _sum;
    private long _max = long.MinValue;

    public void Add(long value)
    {
        lock (_gate)
        {
            _count++;
            _sum += value;
            if (value > _max)
            {
                _max = value;
            }
        }
    }

    // A consistent snapshot: all three values are read under the same lock.
    public (long Count, long Sum, long Max) Snapshot()
    {
        lock (_gate)
        {
            return (_count, _sum, _max);
        }
    }
}

// Exercise 2: thread-safe lazy initialization with double-checked locking.
public sealed class OnceInitializer<T>(Func<T> factory)
    where T : class
{
    private readonly Lock _gate = new();
    private T? _value;

    public T Value
    {
        get
        {
            // Fast path without a lock; Volatile.Read pairs with Volatile.Write below.
            T? value = Volatile.Read(ref _value);
            if (value is not null)
            {
                return value;
            }
            lock (_gate)
            {
                value = _value;
                if (value is null)
                {
                    value = factory();
                    Volatile.Write(ref _value, value);
                }
                return value;
            }
        }
    }
}

// Exercise 3: one async-friendly lock per key; different keys do not block each other.
public sealed class KeyedLock
{
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();

    public async Task<T> RunAsync<T>(string key, Func<Task<T>> action)
    {
        SemaphoreSlim gate = _locks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
        await gate.WaitAsync();
        try
        {
            return await action();
        }
        finally
        {
            gate.Release();
        }
    }
}

// Exercise 4: a lock-free "store the maximum" built on a compare-and-swap loop.
public static class AtomicMath
{
    public static int InterlockedMax(ref int location, int value)
    {
        int current = Volatile.Read(ref location);
        while (value > current)
        {
            int seen = Interlocked.CompareExchange(ref location, value, current);
            if (seen == current)
            {
                return value; // our value was stored
            }
            current = seen; // someone else changed it: re-check against the new value
        }
        return current;
    }
}
