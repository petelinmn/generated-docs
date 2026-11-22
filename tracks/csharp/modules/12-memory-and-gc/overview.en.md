---
title: "Memory and GC"
lang: en
track: csharp
kind: module
doc: overview
slug: 12-memory-and-gc
part: 2
levels: [1, 2, 3, 4, 5, 6]
tags: [gc, garbage-collection, managed-heap, generations, large-object-heap, idisposable, using, finalizer, safehandle, memory-leak, weak-reference, pinning, server-gc]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-21
---

# Memory and GC

🌐 [Русский](overview.ru.md) · **English**

## In a nutshell

In .NET, `new` allocates memory for objects, and the garbage collector (GC) frees it: it finds objects unreachable from the program's roots and frees their memory. To keep collections fast, the heap is divided into generations: young objects are checked often, old ones rarely. But the GC manages memory only: files, sockets and connections are released deterministically — through `IDisposable` and `using`. And .NET does have memory leaks too: they are references someone forgot about.

## Key ideas

- **Reachability, not reference counting:** the GC walks the graph from the roots (thread stacks, static fields, GC handles); unreachable cycles are collected.
- **Generations 0, 1, 2:** a survivor is promoted to the next generation. Objects of 85,000 bytes and up go straight to the LOH, which is collected together with generation 2.
- **`IDisposable` and `using`** release resources at a known moment, even on an exception; `using var` at the end of the block, in reverse order.
- **A finalizer is a last resort:** it runs at an unknown time, extends the object's life by a collection, and is not called when the process exits. For OS handles, `SafeHandle`.
- **Leaks in managed code** come from events of long-lived objects, static collections, timers, unreleased `GCHandle`s.
- **Lifetime depends on the JIT:** in optimized code an object is collectible right after its last use; in Debug it lives until the end of the method.
- **The GC is configurable:** Workstation or Server, background GC, latency modes, heap limits in a container.

## Minimal example

```csharp
// using declarations: disposed at the end of the enclosing block, in reverse order.
public static List<string> Declarations()
{
    var log = new List<string>();
    {
        using var a = new Logged("a", log);
        using var b = new Logged("b", log);
        log.Add("body end");
    }
    log.Add("after block");
    return log;
}
```

The result is `["body end", "dispose b", "dispose a", "after block"]` — the resources were released at the end of the block, in reverse order, without the GC.

## When to use and when not

- ✅ `using` — for everything that implements `IDisposable`: streams, connections, HTTP responses, `CancellationTokenSource`.
- ✅ Your own `IDisposable` — if the class owns a disposable object or an unmanaged resource.
- ✅ `SafeHandle` — for OS handles and native libraries.
- ✅ Measure allocations (`GC.GetAllocatedBytesForCurrentThread`, `dotnet-counters`) before optimizing.
- ❌ `GC.Collect()` in production code.
- ❌ Your own finalizer "just in case".
- ❌ Nulling local variables "for the GC".

## Levels of this material

| Level | What's inside |
|---|---|
| [1. Basics](basics.en.md) | the stack and the managed heap; roots and reachability; when a collection runs; generations and the LOH; `IDisposable`, `using`, `using var` |
| [2. Advanced](advanced.en.md) | implementing `IDisposable` and the `Dispose(bool)` pattern; `IAsyncDisposable`; finalizers; `SafeHandle`; memory leaks; weak references and `ConditionalWeakTable`; measuring allocations; Workstation and Server GC |
| [3. Expert](expert.en.md) | how a collection works, the write barrier and regions; object lifetime and the JIT; finalization and resurrection; LOH and POH; pinning; GC and latency modes; settings; reference, gotchas, 18 interview questions |
| [4. Self-check](quiz.en.md) | quiz and [exercises](exercises.en.md) with solutions |
| [5. Runnable examples](code/) | `code/Examples/` and `code/Solutions/` — solution `Code.slnx` |
| [6. Tests](code/) | `code/Tests/` — `dotnet test` in the `code/` folder |

## Related materials

- Prerequisites: [02 · Types and variables](../02-types-and-variables/overview.en.md) — value and reference types, boxing
- The runtime and its settings: [01 · The .NET platform and dotnet CLI](../01-dotnet-platform/overview.en.md)
- `Dispose` in practice: [15 · EF Core](../15-ef-core/overview.en.md) — the `DbContext` lifetime
- Next: [12a · Span and Memory](../12a-span-and-memory/overview.en.md), [19 · Performance](../19-performance/overview.en.md)
- `IAsyncDisposable` and state machine allocations: [10 · Async/await](../10-async-await/overview.en.md)
- The same in other tracks: [Python](../../../python/overview.en.md) — CPython reference counting and cycle collector, `with`
