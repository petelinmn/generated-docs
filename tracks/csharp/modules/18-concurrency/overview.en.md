---
title: "Multithreading and synchronization"
lang: en
track: csharp
kind: module
doc: overview
slug: 18-concurrency
part: 3
levels: [1, 2, 3, 4, 5, 6]
tags: [concurrency, threads, thread-pool, race-condition, lock, monitor, interlocked, semaphore, reader-writer-lock, channels, deadlock, memory-model]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-21
---

# Multithreading and synchronization

🌐 [Русский](overview.ru.md) · **English**

## In a nutshell

Several threads do work in parallel on different cores. As soon as they access shared data and at least one of them writes, races appear without synchronization: the result depends on a random order of execution. .NET offers a set of synchronization tools — from the `lock` statement and `Interlocked` atomic operations to semaphores, signals and channels. The key skill is choosing the simplest tool that solves the problem.

## Key ideas

- **A data race:** `count++` from several threads loses updates — 4 threads with a million iterations each gave 1,966,527 instead of 4,000,000.
- **`lock`** lets one thread at a time into a block. Since .NET 9 there is the `System.Threading.Lock` type for it. `await` is not allowed inside `lock` (CS1996).
- **`Interlocked`** changes a single variable atomically without a lock. Several related fields need `lock`.
- **`SemaphoreSlim`** limits the number of simultaneous operations and works with `await` through `WaitAsync`.
- **`ReaderWriterLockSlim`, events, `CountdownEvent`, `Barrier`, `Channel<T>`** solve special tasks: many readers, signals, passing data.
- **A deadlock** happens when locks are taken in different orders; the cure is a single order and timeouts.
- **The memory model:** without synchronization another thread may see changes in a different order; `volatile` gives ordering but not atomicity.

## Minimal example

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

Four threads calling `Increment()` a million times each give exactly `4000000`.

## When to use and when not

- ✅ `lock` on a `System.Threading.Lock` — for shared mutable data within a process.
- ✅ `Interlocked` — for counters and flags.
- ✅ `SemaphoreSlim` and `Channel<T>` — in asynchronous code.
- ✅ Concurrent collections — when you need a shared collection.
- ❌ Shared mutable state where you can do without it: immutable data and message passing are simpler than synchronization.
- ❌ `lock (this)`, strings and `Type` as the lock object.
- ❌ Blocking waits (`.Result`, `.Wait()`) on pool threads.

## Levels of this material

| Level | What's inside |
|---|---|
| [1. Basics](basics.en.md) | threads, the pool and tasks; a data race; `lock` and `System.Threading.Lock`; `Interlocked`; concurrent collections |
| [2. Advanced](advanced.en.md) | choosing a primitive; `SemaphoreSlim` and `async`; `ReaderWriterLockSlim`; events, `CountdownEvent`, `Barrier`; `Channel<T>`; `GetOrAdd` and `Lazy`; deadlocks; `Parallel` and PLINQ |
| [3. Expert](expert.en.md) | how `lock` works; the memory model and `volatile`; `Monitor.Wait/Pulse`; lock-free and CAS; `SpinLock`; `AsyncLocal`; the thread pool; a reference of primitives, gotchas, interview questions |
| [4. Self-check](quiz.en.md) | a quiz and [exercises](exercises.en.md) with solutions |
| [5. Runnable examples](code/) | `code/Examples/` and `code/Solutions/` — the `Code.slnx` solution |
| [6. Tests](code/) | `code/Tests/` — `dotnet test` inside `code/` |

## Related materials

- Prerequisites: [03 · Control flow and methods](../03-control-flow-and-methods/overview.en.md), [02 · Types and variables](../02-types-and-variables/overview.en.md)
- In practice: an EF Core `DbContext` is not thread-safe — [15 · EF Core](../15-ef-core/advanced.en.md#dbcontext-lifetime)
- `Task`, `async`/`await`, cancellation: [10 · Async/await](../10-async-await/overview.en.md)
- `ArrayPool`, `Pipelines`: [12a · Span and Memory](../12a-span-and-memory/overview.en.md)
- The same idea in other tracks: [Python](../../../python/overview.en.md) — the GIL, `threading` and `asyncio`
