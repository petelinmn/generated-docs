---
title: "Multithreading and synchronization: basics"
lang: en
track: csharp
kind: module
doc: basics
slug: 18-concurrency
part: 3
tags: [concurrency, threads, thread-pool, race-condition, lock, monitor, interlocked, concurrent-collections]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-21
---

# Multithreading and synchronization: basics

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md) · [exercises](exercises.en.md) · [5–6. Code and tests](code/)

**Prerequisites:** [03 · Control flow and methods](../03-control-flow-and-methods/overview.en.md) — lambdas and closures; [02 · Types and variables](../02-types-and-variables/overview.en.md) — value and reference types.

## Why it matters

A modern processor has many cores, and a program that does everything on one thread uses only one of them. Several threads let you do work in parallel and avoid blocking the UI or request handling while a long operation runs.

The price is **shared state**. While one thread reads and changes the data, everything is simple. As soon as several threads access the same variable and at least one of them writes, **synchronization** is needed. Without it the result depends on the order in which the operating system switched threads, and such bugs reproduce at random.

## Threads, the thread pool and tasks

| Way | What it is | When to use |
|---|---|---|
| `new Thread(...)` | a separate OS thread, foreground by default | rarely: long background work with special requirements |
| `ThreadPool` / `Task.Run` | work is queued to a pool of reusable threads | short computations, most background work |
| `Task.Factory.StartNew(…, TaskCreationOptions.LongRunning)` | a task for which the scheduler provides a separate thread | long blocking work, so it does not occupy a pool thread |

```csharp
// A new Thread is a foreground thread by default: the process waits for it before exiting.
public static ThreadKind DedicatedThread()
{
    ThreadKind result = default;
    var thread = new Thread(() => result = Current());
    thread.Start();
    thread.Join();
    return result;
}

// Task.Run queues work to the thread pool; pool threads are background threads.
public static ThreadKind PoolTask() => Task.Run(Current).GetAwaiter().GetResult();

// LongRunning tells the default scheduler not to occupy a pool thread with a long blocking job.
public static ThreadKind LongRunningTask() =>
    Task.Factory.StartNew(Current, TaskCreationOptions.LongRunning).GetAwaiter().GetResult();
```

| Call | `IsThreadPoolThread` | `IsBackground` |
|---|---|---|
| `DedicatedThread()` | `False` | `False` |
| `PoolTask()` | `True` | `True` |
| `LongRunningTask()` | `False` | `True` |

- `Start()` starts the thread, `Join()` waits for it to finish.
- The process does not exit while at least one **foreground** thread is running. Background threads (including all pool threads) end together with the process.
- Everyday code uses `Task.Run` and `await`. Asynchrony is covered in detail in module 10; here the topic is thread synchronization.

Code: [Ex02_ThreadKinds.cs](code/Examples/Ex02_ThreadKinds.cs).

## A data race

A counter incremented by four threads a million times each:

```csharp
// count++ is read-modify-write: two threads can read the same value and lose an update.
public sealed class UnsafeCounter : ICounter
{
    private int _value;

    public void Increment() => _value++;

    public int Value => _value;
}
```

```text
CounterRunner.Run(new UnsafeCounter(), threads: 4, iterations: 1_000_000)
1966527
```

4,000,000 was expected, less than half came out, and the number differs on every run. `_value++` is three steps: read the value, add one, write it back. If two threads read the same value, both write the same result, and one increment is lost. This is a **data race** (race condition).

## lock: mutual exclusion

The `lock` statement lets only one thread at a time into a block of code. The others wait until it leaves:

```csharp
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
```

Starting with .NET 9 and C# 13, a dedicated object of type `System.Threading.Lock` is recommended for locking — it is faster:

```csharp
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
```

Both counters give exactly `4000000`. Rules:

- Lock a **private** object that exists only for this purpose. Do not lock `this`, `Type` objects or strings: other code may lock them too, and string literals are also interned (see [02 · Types and variables](../02-types-and-variables/expert.en.md)).
- Reading the protected data also happens under the same `lock`, otherwise you may see an intermediate state.
- Do as little work as possible inside `lock`: the other threads are waiting meanwhile.
- `lock` is released even on an exception: the compiler wraps the block in `try/finally`.

Code: [Ex01_Counters.cs](code/Examples/Ex01_Counters.cs).

## Interlocked: atomic operations

Simple operations on a single variable need no lock. The `Interlocked` class performs them atomically — as an indivisible processor operation:

```csharp
// An atomic hardware operation: no lock needed for a simple counter.
public sealed class InterlockedCounter : ICounter
{
    private int _value;

    public void Increment() => Interlocked.Increment(ref _value);

    public int Value => Volatile.Read(ref _value);
}
```

| Method | What it does | Returns |
|---|---|---|
| `Increment(ref x)` / `Decrement(ref x)` | `x + 1` / `x - 1` | the new value |
| `Add(ref x, n)` | `x + n` | the new value |
| `Exchange(ref x, v)` | writes `v` | the **old** value |
| `CompareExchange(ref x, v, expected)` | writes `v` only if `x == expected` | the **old** value |

Example: with `x = 5`, the call `Exchange(ref x, 7)` returns `5`. Then `CompareExchange(ref x, 9, 7)` returns `7` and writes 9, while `CompareExchange(ref x, 1, 0)` returns `9` and changes nothing, because `x` is not 0.

`Interlocked` suits a single variable. If several fields must change consistently, you need `lock` ([exercise 1](exercises.en.md)).

## Thread-safe collections

The regular `List<T>` and `Dictionary<TKey, TValue>` are not designed for simultaneous writes from several threads. `System.Collections.Concurrent` has collections that synchronize themselves: `ConcurrentDictionary`, `ConcurrentQueue`, `ConcurrentStack`, `ConcurrentBag`.

```csharp
// AddOrUpdate retries the update delegate until it wins, so no increment is lost.
public static IReadOnlyDictionary<string, int> CountWords(IEnumerable<string> words, int threads)
{
    var counts = new ConcurrentDictionary<string, int>();
    Parallel.ForEach(words, new ParallelOptions { MaxDegreeOfParallelism = threads }, word =>
        counts.AddOrUpdate(word, 1, (_, current) => current + 1));
    return counts;
}
```

30,000 words (`a`, `b`, `a` × 10,000) on 8 threads give exactly `a = 20000`, `b = 10000`. Code: [Ex03_ConcurrentCollections.cs](code/Examples/Ex03_ConcurrentCollections.cs).

## Beginner mistakes

- ❌ "It works on my machine" without synchronization → ✅ a race shows up at random; shared mutable data is always protected.
- ❌ `lock (this)` or `lock ("key")` → ✅ `private readonly Lock _gate = new();`.
- ❌ Protecting the write but reading without `lock` → ✅ reads and writes under the same lock.
- ❌ Long work (network, files) inside `lock` → ✅ inside — only the change of shared data.
- ❌ `Dictionary` from several threads → ✅ `ConcurrentDictionary` or `lock`.
- ❌ `new Thread` for every small task → ✅ `Task.Run`: pool threads are reused.

## Related materials

- Next: [advanced theory](advanced.en.md) — semaphores, `ReaderWriterLockSlim`, signals, channels, deadlocks
- Prerequisites: [03 · Control flow and methods](../03-control-flow-and-methods/overview.en.md), [02 · Types and variables](../02-types-and-variables/overview.en.md)
- The same idea in other tracks: [Python](../../../python/overview.en.md) — module 15 "Threads and processes", the GIL and `threading.Lock`

## Sources

- [The lock statement](https://learn.microsoft.com/dotnet/csharp/language-reference/statements/lock)
- [`System.Threading.Lock`](https://learn.microsoft.com/dotnet/api/system.threading.lock)
- [Overview of synchronization primitives](https://learn.microsoft.com/dotnet/standard/threading/overview-of-synchronization-primitives)
- [The managed thread pool](https://learn.microsoft.com/dotnet/standard/threading/the-managed-thread-pool)
- [`ConcurrentDictionary<TKey, TValue>`](https://learn.microsoft.com/dotnet/api/system.collections.concurrent.concurrentdictionary-2)
