---
title: "Multithreading and synchronization: advanced"
lang: en
track: csharp
kind: module
doc: advanced
slug: 18-concurrency
part: 3
tags: [concurrency, semaphore, reader-writer-lock, events, barrier, channels, producer-consumer, deadlock, parallel]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-21
---

# Multithreading and synchronization: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md) · [exercises](exercises.en.md) · [5–6. Code and tests](code/)

## Which primitive to choose

| Task | Tool |
|---|---|
| Protect shared data within a process | `lock` on a `System.Threading.Lock` |
| Change a single variable atomically | `Interlocked` |
| Protect data in asynchronous code (with `await` inside) | `SemaphoreSlim(1, 1)` + `WaitAsync` |
| Limit the number of simultaneous operations | `SemaphoreSlim(N, N)` |
| Many readers, rare writes | `ReaderWriterLockSlim` |
| Wait for an event or open a "gate" | `ManualResetEventSlim`, `AutoResetEvent` |
| Wait for N participants to finish | `CountdownEvent` |
| Synchronize the phases of an algorithm | `Barrier` |
| Pass data from producers to consumers | `Channel<T>` |
| Synchronization between processes | a named `Mutex` |

Names with the `Slim` suffix are lightweight versions: they do not use OS kernel objects and are faster, but they work only within a single process.

## SemaphoreSlim and asynchronous code

`lock` cannot be used around `await`: `await` in the body of a `lock` is compile error CS1996. Asynchronous code uses `SemaphoreSlim`: its `WaitAsync` method waits without blocking a thread. A semaphore with a count of N lets at most N participants through at the same time:

```csharp
// Runs all jobs, at most `maxConcurrency` at a time; returns the peak concurrency actually observed.
public static async Task<int> RunAsync(int jobs, int maxConcurrency, TimeSpan jobDuration)
{
    using var semaphore = new SemaphoreSlim(maxConcurrency, maxConcurrency);
    int running = 0;
    int peak = 0;

    async Task RunJobAsync()
    {
        await semaphore.WaitAsync(); // unlike lock, waiting does not block a thread
        try
        {
            int now = Interlocked.Increment(ref running);
            InterlockedMax(ref peak, now);
            await Task.Delay(jobDuration);
        }
        finally
        {
            Interlocked.Decrement(ref running);
            semaphore.Release();
        }
    }

    await Task.WhenAll(Enumerable.Range(0, jobs).Select(_ => RunJobAsync()));
    return peak;
}
```

12 jobs with a limit of 3 give a peak of exactly `3`. `Release()` always goes into `finally`: otherwise an exception "eats" a semaphore permit, and the others wait forever. `SemaphoreSlim(1, 1)` is the asynchronous counterpart of `lock`. Code: [Ex04_Throttling.cs](code/Examples/Ex04_Throttling.cs).

## ReaderWriterLockSlim

If data is read often and changed rarely, a regular `lock` makes readers wait for each other for no reason. `ReaderWriterLockSlim` lets many readers in at once and one writer — alone:

```csharp
// Upgradeable read: only one thread at a time may hold it, so check-then-add is safe.
public TValue GetOrAdd(TKey key, Func<TKey, TValue> factory)
{
    _lock.EnterUpgradeableReadLock();
    try
    {
        if (_items.TryGetValue(key, out TValue? existing))
        {
            return existing;
        }
        _lock.EnterWriteLock();
        try
        {
            TValue created = factory(key);
            _items[key] = created;
            return created;
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }
    finally
    {
        _lock.ExitUpgradeableReadLock();
    }
}
```

| Mode | How many threads at once | Compatible with |
|---|---|---|
| `EnterReadLock` | many | other reads and one upgradeable |
| `EnterUpgradeableReadLock` | one | reads; can move to write |
| `EnterWriteLock` | one | nothing |

100 parallel `GetOrAdd` calls for one key run the factory exactly once. Important: a regular read lock cannot be "upgraded" to a write lock. By default (`LockRecursionPolicy.NoRecursion`) this throws `LockRecursionException` with the message "Write lock may not be acquired with read lock held". The "read and maybe write" scenario has the upgradeable mode. Code: [Ex05_ReaderWriterCache.cs](code/Examples/Ex05_ReaderWriterCache.cs).

## Signals between threads

Sometimes the task is not to protect data but to **tell** another thread that something happened.

| Type | Behavior |
|---|---|
| `ManualResetEventSlim` | after `Set()` stays open and lets everyone through until `Reset()` is called |
| `AutoResetEvent` | after `Set()` lets **one** waiter through and closes itself |
| `CountdownEvent` | opens when the counter reaches zero (`Signal()` decrements it) |
| `Barrier` | all N participants wait for each other at the end of every phase |

A start "gate" and a "finish line":

```csharp
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
```

While the gate is closed, `Passed` is `0` whatever the thread interleaving. After `Open()` (that is, `_gate.Set()`) all 5 workers pass, and `WaitAll` returns `true`. For comparison: with `AutoResetEvent`, one `Set()` releases exactly one of three waiters. Code: [Ex06_Signaling.cs](code/Examples/Ex06_Signaling.cs).

## Producer and consumer: Channel\<T\>

`System.Threading.Channels` is an asynchronous queue for passing data between parts of a program. A bounded channel slows the producer down by itself when consumers cannot keep up:

```csharp
Channel<int> channel = Channel.CreateBounded<int>(capacity);

Task producer = Task.Run(async () =>
{
    for (int i = 1; i <= items; i++)
    {
        await channel.Writer.WriteAsync(i); // waits while the channel is full
    }
    channel.Writer.Complete(); // lets ReadAllAsync finish
});

Task<long>[] readers = Enumerable.Range(0, consumers).Select(_ => Task.Run(async () =>
{
    long sum = 0;
    await foreach (int item in channel.Reader.ReadAllAsync())
    {
        sum += item;
    }
    return sum;
})).ToArray();
```

10,000 items through a channel with capacity 16 and 4 consumers give a sum of exactly `50005000`: every item is read once. The default full mode is `BoundedChannelFullMode.Wait`: `WriteAsync` waits for space, and `TryWrite` into a full channel returns `false` (`[true, true, false]` for capacity 2). `Complete()` is required, otherwise `ReadAllAsync` waits for new items forever. Code: [Ex07_ProducerConsumer.cs](code/Examples/Ex07_ProducerConsumer.cs).

## ConcurrentDictionary: GetOrAdd and Lazy

`ConcurrentDictionary` methods are atomic **except for the delegates** passed to them: the factory in `GetOrAdd` runs outside the internal lock. If several threads fail to find the key at the same time, each of them calls the factory, and one result ends up in the dictionary:

```csharp
// The value factory runs outside the dictionary's lock: concurrent callers can all run it.
// The barrier forces `callers` threads to be inside the factory at the same time.
public static int FactoryCallsWithGetOrAdd(int callers)
{
    var cache = new ConcurrentDictionary<string, int>();
    int calls = 0;
    using var barrier = new Barrier(callers);
    RunConcurrently(callers, () => cache.GetOrAdd("key", _ =>
    {
        Interlocked.Increment(ref calls);
        barrier.SignalAndWait();
        return 42;
    }));
    return calls;
}
```

8 threads — 8 factory calls. If the factory is expensive or has side effects, the value is wrapped in `Lazy<T>`: all threads get the same `Lazy` object, and its factory runs exactly **1** time. Code: [Ex03_ConcurrentCollections.cs](code/Examples/Ex03_ConcurrentCollections.cs).

## Deadlocks

A deadlock happens when threads wait for each other in a cycle: the first holds `A` and waits for `B`, the second holds `B` and waits for `A`. The example below creates this situation every time and detects it with `Monitor.TryEnter` with a timeout:

```csharp
var t1 = new Thread(() =>
{
    lock (a)
    {
        bothHoldOne.SignalAndWait();
        first = Monitor.TryEnter(b, timeout);
        if (first) Monitor.Exit(b);
        bothTried.SignalAndWait();
    }
});
var t2 = new Thread(() =>
{
    lock (b)
    {
        bothHoldOne.SignalAndWait();
        second = Monitor.TryEnter(a, timeout);
        if (second) Monitor.Exit(a);
        bothTried.SignalAndWait();
    }
});
```

The result is `(False, False)`: neither thread got the second lock. With a plain `lock` instead of `TryEnter` the program would hang forever. The cure is a **single lock order**: everyone takes the locks in the same order.

```csharp
// Both accounts are always locked in the same order (by Id), so no cycle of waits can form.
public static void Transfer(Account from, Account to, decimal amount)
{
    Account first = from.Id < to.Id ? from : to;
    Account second = from.Id < to.Id ? to : from;
    lock (first.Gate)
    {
        lock (second.Gate)
        {
            from.Balance -= amount;
            to.Balance += amount;
        }
    }
}
```

10,000 opposite transfers between two accounts finish without hanging, and the total stays `2000`. Other ways: hold only one lock at a time, use timeouts (`TryEnter`, `Wait(timeout)`), and do not call foreign code (events, callbacks) under a lock. Code: [Ex08_Deadlock.cs](code/Examples/Ex08_Deadlock.cs).

## Parallel and PLINQ

Processing collections in parallel does not require creating threads by hand:

| API | What for |
|---|---|
| `Parallel.For` / `Parallel.ForEach` | CPU work over a collection; `MaxDegreeOfParallelism` limits parallelism |
| `Parallel.ForEachAsync` | asynchronous processing with limited parallelism (.NET 6+) |
| PLINQ: `.AsParallel()` | a parallel LINQ query |

Verified examples: `Parallel.ForEachAsync` over 20 items with `MaxDegreeOfParallelism = 4` never runs more than 4 tasks at once. `Enumerable.Range(1, 1_000_000).AsParallel().Select(x => (long)x).Sum()` gives `500000500000`. The `Parallel.For` overload with local state accumulates the sum in each thread separately and combines the results with one `Interlocked.Add` per thread — faster than `Interlocked` on every iteration.

## Best practices

- First avoid shared mutable state: immutable data, per-thread results, message passing through `Channel<T>`.
- If synchronization is needed, take the simplest suitable tool: `Interlocked` → `lock` → specialized primitives.
- Hold a lock as briefly as possible and do not call foreign code under it.
- Acquire several locks in a single order.
- `Release`/`Exit`/`Dispose` — only in `finally`.
- In asynchronous code — `SemaphoreSlim.WaitAsync` and `Channel<T>`, not blocking `Wait()`.

## Common mistakes

- ❌ `await` inside `lock` → ✅ error CS1996; use `SemaphoreSlim(1, 1)` and `WaitAsync`.
- ❌ `semaphore.Release()` not in `finally` → ✅ an exception permanently reduces the number of permits.
- ❌ Upgrading a read lock to write → ✅ `LockRecursionException`; use the upgradeable mode.
- ❌ An expensive factory in `ConcurrentDictionary.GetOrAdd` → ✅ `Lazy<T>`: the factory may run several times.
- ❌ Forgetting `Writer.Complete()` → ✅ channel consumers wait forever.
- ❌ A shared `new Random()` from several threads → ✅ `Random` is not thread-safe, and under a race its methods start returning 0; use `Random.Shared`.
- ❌ Blocking pool threads with waits (`.Result`, `.Wait()`, `Thread.Sleep`) → ✅ the pool is limited, and tasks stop starting; see the [expert theory](expert.en.md).

## Comparison with other tracks

| | C# | Python |
|---|---|---|
| Mutual exclusion | `lock`, `Lock` | `threading.Lock` |
| Asynchronous lock | `SemaphoreSlim.WaitAsync` | `asyncio.Lock` |
| Producer–consumer queue | `Channel<T>` | `queue.Queue`, `asyncio.Queue` |
| Parallel CPU work | threads run in parallel on all cores | in CPython threads are limited by the GIL; processes are used for CPU work |

Python in detail — in the [Python track](../../../python/overview.en.md), modules 14 "Asyncio" and 15 "Threads and processes".

## Related materials

- Earlier: [basics](basics.en.md)
- Next: [expert theory](expert.en.md) — the memory model, `Monitor.Wait/Pulse`, lock-free, a reference of primitives
- A real-world case: one `DbContext` from several threads — [15 · EF Core](../15-ef-core/advanced.en.md#dbcontext-lifetime)
- The same idea in other tracks: [Python](../../../python/overview.en.md)

## Sources

- [SemaphoreSlim and Semaphore](https://learn.microsoft.com/dotnet/standard/threading/semaphore-and-semaphoreslim)
- [`ReaderWriterLockSlim`](https://learn.microsoft.com/dotnet/api/system.threading.readerwriterlockslim)
- [Overview of synchronization primitives](https://learn.microsoft.com/dotnet/standard/threading/overview-of-synchronization-primitives)
- [System.Threading.Channels](https://learn.microsoft.com/dotnet/core/extensions/channels)
- [`ConcurrentDictionary<TKey, TValue>`](https://learn.microsoft.com/dotnet/api/system.collections.concurrent.concurrentdictionary-2)
- [The lock statement](https://learn.microsoft.com/dotnet/csharp/language-reference/statements/lock)
- [`System.Random`: thread safety](https://learn.microsoft.com/dotnet/api/system.random)
