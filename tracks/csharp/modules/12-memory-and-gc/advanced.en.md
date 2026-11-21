---
title: "Memory and GC: advanced"
lang: en
track: csharp
kind: module
doc: advanced
slug: 12-memory-and-gc
part: 2
tags: [idisposable, dispose-pattern, iasyncdisposable, finalizer, safehandle, memory-leak, weak-reference, conditional-weak-table, allocations, server-gc]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-21
---

# Memory and GC: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md) · [exercises](exercises.en.md) · [5–6. Code and tests](code/)

## Implementing IDisposable

The most common case is a `sealed` class that owns other disposable objects. It needs no finalizer:

```csharp
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
```

Rules:

- `Dispose` must be **idempotent**: a repeated call does nothing and does not throw. In the test two calls went through without errors.
- After `Dispose` the object's methods throw `ObjectDisposedException`. The short form is `ObjectDisposedException.ThrowIf(condition, this)` (added in .NET 7).
- Dispose what you **own**: objects the class created itself. Whatever came from outside (through the constructor or DI) is disposed by its owner.
- `Dispose` must not throw: it is often called from `finally`, and its exception replaces the original one (see [expert](expert.en.md#an-exception-in-dispose-replaces-the-original)).

Code: [Ex04_DisposePattern.cs](code/Examples/Ex04_DisposePattern.cs).

### The Dispose(bool) pattern for a hierarchy

If a class may have derived classes, the pattern with a virtual `Dispose(bool disposing)` is used:

```csharp
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
```

The derived class overrides `Dispose(bool)`, releases its own resources and calls `base.Dispose(disposing)` at the end. The result for `DerivedResource` after two `Dispose` calls: `["derived: managed", "base: managed"]` — each part released once, the derived class before the base.

## IAsyncDisposable and await using

If releasing needs I/O — flushing a buffer to a file or the network, closing a connection gracefully — it is made asynchronous:

```csharp
// Implements both interfaces: await using calls DisposeAsync, a plain using calls Dispose.
public sealed class BufferedSink(List<string> log) : IAsyncDisposable, IDisposable
{
    private readonly List<string> _buffer = [];

    public void Write(string item) => _buffer.Add(item);

    // Flushing may need I/O, so the asynchronous path is the main one.
    public async ValueTask DisposeAsync()
    {
        await Task.Yield(); // stands in for an asynchronous flush
        log.Add($"flushed {_buffer.Count} async");
        _buffer.Clear();
    }

    public void Dispose()
    {
        log.Add($"flushed {_buffer.Count} sync");
        _buffer.Clear();
    }
}
```

`await using (var sink = …)` gave `["body", "flushed 2 async"]`, and a plain `using` gave `["flushed 1 sync"]`. The compiler checks that they match: `using` on a type with only `IAsyncDisposable` is error CS8418, and `await using` on a type with only `IDisposable` is error CS8417. So a class that is released asynchronously should implement both interfaces: synchronous callers can then dispose it too. Code: [Ex05_AsyncDispose.cs](code/Examples/Ex05_AsyncDispose.cs).

## Finalizers

A finalizer (`~ClassName()`) is a method the GC calls before freeing an object's memory. It is the last chance to release an unmanaged resource if `Dispose` was not called.

```csharp
public class FinalizableResource(FinalizerProbe probe) : IDisposable
{
    public void Dispose()
    {
        // release the resource here...
        GC.SuppressFinalize(this); // ...and tell the GC that the finalizer is no longer needed
    }

    // Runs on the finalizer thread at an unknown moment after the object becomes unreachable.
    ~FinalizableResource() => probe.Finalized = true;
}
```

What the examples showed:

| Scenario | Result |
|---|---|
| the object is abandoned without `Dispose`, then `GC.Collect()` + `GC.WaitForPendingFinalizers()` | the finalizer ran |
| `Dispose` with `GC.SuppressFinalize` was called first | the finalizer did not run |
| an object with a finalizer after the first collection | still in memory; freed only by the next collection |

Why finalizers are a last resort:

- **Unknown timing.** A finalizer runs on a separate finalizer thread, at some point after a collection. The resource may "hang" for a long time.
- **Expensive.** An object with a finalizer survives an extra collection and is promoted to an older generation — together with everything it references.
- **Order is not guaranteed.** Objects referenced by the finalizable object may already be finalized — that is why other managed objects are not touched when `disposing == false`.
- **Finalizers are not called when the process exits.** Verified: an object whose finalizer prints a line was unreachable by the end of `Main` — the line never appeared.

Code: [Ex06_Finalizers.cs](code/Examples/Ex06_Finalizers.cs).

## SafeHandle instead of a finalizer

An unmanaged resource (a file or socket handle, a pointer from a native library) is almost never wrapped with your own finalizer. That is what `SafeHandle` is for: it already has a reliable finalizer, reference counting for the duration of native calls, and a guarantee of release exactly once. All you write is `ReleaseHandle`:

```csharp
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
```

A double `Dispose` closed the handle once (`Closed = [100]`, `IsClosed = True`), and for the value `-1` ("no handle") `ReleaseHandle` was not called at all. A class that stores a `SafeHandle` implements the ordinary simple `Dispose` from the first section — without its own finalizer. Code: [Ex07_SafeHandles.cs](code/Examples/Ex07_SafeHandles.cs).

## Memory leaks in managed code

The GC does not free what is reachable. A leak in .NET is a forgotten reference. The most common example is an event on a long-lived object:

```csharp
// A short-lived object, e.g. a view model of a closed window.
public sealed class PriceView : IDisposable
{
    private readonly PriceFeed _feed;

    public PriceView(PriceFeed feed)
    {
        _feed = feed;
        _feed.PriceChanged += OnPriceChanged; // the feed now references this view
    }

    public decimal LastPrice { get; private set; }

    public void Dispose() => _feed.PriceChanged -= OnPriceChanged;

    private void OnPriceChanged(object? sender, decimal price) => LastPrice = price;
}
```

The `+=` subscription stores a delegate in the `PriceFeed` event, and the delegate references the `PriceView`. As long as the publisher lives, so does the subscriber. The result: without unsubscribing, `(ViewAlive: True, Subscribers: 1)` after `GC.Collect()`; with unsubscribing in `Dispose`, `(False, 0)`. Code: [Ex08_EventLeaks.cs](code/Examples/Ex08_EventLeaks.cs).

Typical sources of leaks:

| Source | Why it keeps objects alive | What to do |
|---|---|---|
| an event on a long-lived object | the delegate references the subscriber | unsubscribe in `Dispose`; a subscription token |
| a static collection, a cache without eviction | a static field is a root | limit size and lifetime (`MemoryCache` with policies) |
| a `Timer`, a background task | the callback references the object | dispose the timer, cancel with a `CancellationToken` |
| a lambda stored in a long-lived delegate | the closure captures variables and `this` | do not capture more than needed; unsubscribe |
| a `GCHandle` without `Free` | the handle is a root | `Free` in `finally` |

A heap snapshot helps find a leak: `dotnet-gcdump collect --process-id <PID>` saves the object graph to a `.gcdump` file, and `dotnet-gcdump report <file>` shows which types take the memory. Taking a snapshot triggers a full collection and can pause the process for a long time. To watch the heap live, use `dotnet-counters monitor --process-id <PID> --counters System.Runtime`.

## Weak references

`WeakReference<T>` references an object without keeping it alive:

```csharp
// TryGetTarget returns a strong reference while the object is alive, and false after it is collected.
public static (bool BeforeCollect, bool AfterCollect) TryGetTarget()
{
    WeakReference<byte[]> weak = CreateWeak();
    bool before = HasTarget(weak);
    GC.Collect();
    bool after = HasTarget(weak);
    return (before, after);
}
```

The result is `(True, False)`. `ConditionalWeakTable<TKey, TValue>` attaches data to objects you do not own: an entry lives as long as its key. In the example, of two entries one remained after a collection — the one whose key is still in use (`(2, 1, "kept")`). Code: [Ex09_WeakReferences.cs](code/Examples/Ex09_WeakReferences.cs).

Weak references do not replace a cache: the object disappears at the next collection of its generation, and young objects are collected very often. For caches use `MemoryCache` with a size limit and expiration.

## Measuring allocations

Fewer allocations mean fewer collections. `GC.GetAllocatedBytesForCurrentThread()` shows how many bytes the current thread has allocated:

```csharp
// Runs the action once to warm it up (JIT, static constructors), then measures the second run.
public static long Measure(Action action)
{
    action();
    long before = GC.GetAllocatedBytesForCurrentThread();
    action();
    return GC.GetAllocatedBytesForCurrentThread() - before;
}
```

Actual results on a 64-bit runtime:

| Action | Bytes allocated |
|---|---|
| add 1000 `int` values to an `ArrayList` (each one boxed) | 24,000 |
| add 1000 `int` values to a `List<int>` with preset capacity | 0 |
| `new object()` | 24 |
| `new byte[0]` | 24 |
| `new string('x', 10)` | 48 |

Even an empty object takes 24 bytes: an 8-byte header, an 8-byte type pointer and room for data — an object on a 64-bit runtime is never smaller than 24 bytes. Other counters: `GC.GetTotalAllocatedBytes()` for the whole process, `GC.CollectionCount(generation)` for the number of collections. Code: [Ex10_Allocations.cs](code/Examples/Ex10_Allocations.cs).

## Workstation and Server GC

| | Workstation GC | Server GC |
|---|---|---|
| Default | console and desktop apps | no; enabled by a setting |
| Heaps and GC threads | one heap | a heap and a GC thread per core |
| Goal | short pauses, little memory | server throughput |

Enable it in the `.csproj` with `<ServerGarbageCollection>true</ServerGarbageCollection>`, or with the environment variable `DOTNET_gcServer=1`. Verified: with this variable `GCSettings.IsServerGC` returned `True`. Both flavors use **background GC** by default: generation 2 is collected concurrently with the program. Since .NET 9, DATAS is on by default for Server GC — a mode that adapts the number of heaps to the load. The full settings table is in [expert](expert.en.md#gc-settings).

## Best practices

- Implement `IDisposable` if the class owns a disposable object or an unmanaged resource.
- Make the class `sealed` if inheritance is not needed: then a simple `Dispose` is enough.
- Unmanaged resources go through `SafeHandle`, not your own finalizer.
- Unsubscribe from events of long-lived objects; return an `IDisposable` token from `Subscribe`.
- Do not call `GC.Collect()` in production code.
- Measure allocations before optimizing.

## Common mistakes

- ❌ A finalizer "just in case" in every `IDisposable` → ✅ a finalizer is needed only by a class that holds an unmanaged resource directly, without `SafeHandle`.
- ❌ `Dispose` throws on a repeated call → ✅ an idempotent `Dispose`.
- ❌ Disposing an object received through the constructor or DI → ✅ its owner disposes it.
- ❌ Touching other managed objects in a finalizer → ✅ they may already be finalized.
- ❌ Subscribing to a singleton's event and never unsubscribing → ✅ a leak: the subscriber lives as long as the singleton.
- ❌ Only `IAsyncDisposable` on a class used synchronously → ✅ implement `IDisposable` too.

## Comparison with other tracks

| | C# / .NET | Python (CPython) |
|---|---|---|
| Freeing memory | tracing generational GC | reference counting + cycle collector (the `gc` module) |
| When an object is freed | at the collection of its generation | usually right away, when the reference count hits 0 |
| Deterministic resource release | `IDisposable` + `using` | context manager + `with` |
| Finalizer | `~ClassName()` | `__del__` |
| Weak references | `WeakReference<T>` | the `weakref` module |

More in the [Python track](../../../python/overview.en.md).

## Related materials

- Before: [basics](basics.en.md)
- Next: [expert](expert.en.md) — GC internals, object lifetime and the JIT, LOH and POH, pinning, settings and latency modes, interview questions
- Releasing synchronization primitives (`SemaphoreSlim`, `Release` in `finally`): [18 · Concurrency](../18-concurrency/advanced.en.md)
- Context pooling and `Dispose` in EF Core: [15 · EF Core](../15-ef-core/expert.en.md#performance)

## Sources

- [Implementing a Dispose method](https://learn.microsoft.com/dotnet/standard/garbage-collection/implementing-dispose)
- [Implementing a DisposeAsync method](https://learn.microsoft.com/dotnet/standard/garbage-collection/implementing-disposeasync)
- [Object.Finalize](https://learn.microsoft.com/dotnet/fundamentals/runtime-libraries/system-object-finalize)
- [SafeHandle](https://learn.microsoft.com/dotnet/api/system.runtime.interopservices.safehandle)
- [ObjectDisposedException.ThrowIf](https://learn.microsoft.com/dotnet/api/system.objectdisposedexception.throwif)
- [ConditionalWeakTable](https://learn.microsoft.com/dotnet/api/system.runtime.compilerservices.conditionalweaktable-2)
- [Workstation and server GC](https://learn.microsoft.com/dotnet/standard/garbage-collection/workstation-server-gc)
- [Background garbage collection](https://learn.microsoft.com/dotnet/standard/garbage-collection/background-gc)
- [dotnet-counters](https://learn.microsoft.com/dotnet/core/diagnostics/dotnet-counters)
- [dotnet-gcdump](https://learn.microsoft.com/dotnet/core/diagnostics/dotnet-gcdump)
