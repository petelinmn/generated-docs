---
title: "Memory and GC: basics"
lang: en
track: csharp
kind: module
doc: basics
slug: 12-memory-and-gc
part: 2
tags: [gc, garbage-collection, managed-heap, generations, large-object-heap, idisposable, using]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-21
---

# Memory and GC: basics

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md) · [exercises](exercises.en.md) · [5–6. Code and tests](code/)

**Prerequisites:** [02 · Types and variables](../02-types-and-variables/overview.en.md) — value and reference types, boxing.

## Why it matters

In C#, `new` allocates memory for objects, and the **garbage collector** (GC) — part of the runtime (see [01 · The .NET platform](../01-dotnet-platform/basics.en.md)) — frees it. There is no `free` or `delete` to call, so there are no dangling pointers and no double frees. But automatic memory has a price and limits:

- a collection stops or slows the program, and frequent allocations make the GC run more often;
- an object lives as long as something references it — a forgotten reference becomes a memory leak;
- the GC manages memory only. It does not close files, sockets or database connections in time — that is what `IDisposable` and `using` are for.

## The stack and the managed heap

- The **managed heap** is the memory the GC manages. It holds instances of reference types: class instances, arrays, strings, delegates, boxed values.
- A thread's **stack** holds local variables and method parameters. It is freed automatically when a method returns; the GC does not manage it.

A value of a value type lives where its variable is declared: a local `int` on the stack, an `int` field inside an object on the heap. More in [02 · Types and variables](../02-types-and-variables/expert.en.md), and how boxing creates heap objects in [module 02 advanced](../02-types-and-variables/advanced.en.md).

## How the GC decides an object is no longer needed

The GC does not count references. It starts from the **roots** and walks every object reachable through references. Roots are static fields, local variables on thread stacks, CPU registers, GC handles and the finalization queue. Everything the walk did not reach is garbage.

```csharp
// The object is referenced only from holder.Item: no local variable of this method points to it,
// so the result does not depend on how long the JIT keeps locals alive (Debug vs Release).
public static (bool WhileReferenced, bool AfterRelease) Demo()
{
    var holder = new Holder();
    WeakReference weak = PutNewObject(holder);

    GC.Collect();
    bool whileReferenced = weak.IsAlive;   // reachable: local holder → holder.Item

    holder.Item = null;                    // drop the only strong reference
    GC.Collect();
    return (whileReferenced, weak.IsAlive);
}
```

A `WeakReference` lets you observe an object without keeping it alive. The result is `(True, False)`: while the object is reachable through `holder.Item`, the GC leaves it alone; once the reference is cleared, it is collected.

Since the GC looks for reachable objects instead of counting references, **cycles are no problem**: two objects that reference each other but are unreachable from roots get collected (`CycleIsCollected()` → `True`). Code: [Ex01_Reachability.cs](code/Examples/Ex01_Reachability.cs).

> `GC.Collect()` is used in the examples to show the behavior. Ordinary code does not call it: the GC decides when to collect.

## When a collection happens

Garbage collection starts when:

- the memory used by objects on the managed heap exceeds a threshold the GC tunes itself;
- the system is low on physical memory;
- `GC.Collect()` is called.

Most often it is the first: the program allocates memory, and at some point another `new` triggers a collection.

## Generations

Most objects live very briefly: a temporary string, an iterator, a request DTO. So the heap is divided into **generations**, and young objects are checked more often than old ones:

| Generation | What is there | How often it is collected |
|---|---|---|
| 0 | new objects | most often, fast |
| 1 | survived one collection — a buffer between young and old | less often |
| 2 | long-lived: caches, singletons, static data | rarely; a full collection checks all generations |
| LOH | large objects, 85,000 bytes and up | together with generation 2 |

An object that survives a collection is **promoted** to the next generation:

```csharp
// A new object starts in gen 0; every collection it survives promotes it, up to gen 2.
public static List<int> Promotion()
{
    var survivor = new object();
    var generations = new List<int> { GC.GetGeneration(survivor) };
    for (int i = 0; i < 3; i++)
    {
        GC.Collect();
        generations.Add(GC.GetGeneration(survivor));
    }
    return generations;
}
```

The result is `[0, 1, 2, 2]` — there is nothing above generation 2. A generation 0 collection walks only young objects, so it is fast. A `byte[1_000]` array is created in generation 0, while `byte[84_976]` (exactly 85,000 bytes including the array header) goes straight to the **large object heap** (LOH), and `GC.GetGeneration` returns `2` for it. Code: [Ex02_Generations.cs](code/Examples/Ex02_Generations.cs).

## IDisposable and using

The GC frees memory, but it does not know that an object holds a file, a socket or a database connection. Such a resource must be released **deterministically** — as soon as it is no longer needed. For that, a class implements `IDisposable` with its single method `Dispose()`, and code calls it through `using`:

```csharp
// using statement: Dispose runs when the block ends.
public static List<string> Statement()
{
    var log = new List<string>();
    using (var file = new Logged("file", log))
    {
        log.Add("body");
    }
    log.Add("after");
    return log;
}
```

The result is `["body", "dispose file", "after"]`. The compiler turns `using` into `try/finally`, so `Dispose` runs even on an exception: the `WithException` example gave `["dispose resource", "caught"]` — the resource was released before the exception was caught.

A **using declaration** needs no block: the resource is released at the end of the enclosing block, and several resources in reverse order:

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

The result is `["body end", "dispose b", "dispose a", "after block"]`. Reverse order makes sense: `b` was created later and may depend on `a`.

For resources released asynchronously (for example, flushing a buffer to the network) there are `IAsyncDisposable` and `await using` — covered in [advanced](advanced.en.md). Code: [Ex03_Using.cs](code/Examples/Ex03_Using.cs).

What implements `IDisposable` in the standard library: `FileStream`, `StreamReader`/`StreamWriter`, `HttpResponseMessage`, `SqlConnection` and other database connections, `DbContext` from [EF Core](../15-ef-core/advanced.en.md#dbcontext-lifetime), `CancellationTokenSource`, `Timer`, `SemaphoreSlim`.

## Beginner mistakes

- ❌ Calling `GC.Collect()` to "clean up memory" → ✅ the GC picks the moment itself; `GC.Collect()` is a full blocking collection of all generations, usually just an extra pause.
- ❌ Thinking `Dispose` frees the object's memory → ✅ it releases a resource (a file, a socket); the GC frees the memory later.
- ❌ Creating a `FileStream`, an HTTP response or a connection without `using` → ✅ `using` or `using var`.
- ❌ Believing `obj = null` frees memory immediately → ✅ the object becomes a candidate for collection and is freed at the next collection of its generation.
- ❌ Fearing circular references → ✅ the GC collects unreachable cycles.
- ❌ Keeping what is no longer needed in static collections → ✅ a static field is a root: everything it references lives until the program ends.

## Related materials

- Next: [advanced](advanced.en.md) — the Dispose pattern, finalizers, `SafeHandle`, leaks through events, weak references, GC flavors
- Value and reference types, boxing: [02 · Types and variables](../02-types-and-variables/expert.en.md)
- Disposing a database context: [15 · EF Core](../15-ef-core/advanced.en.md#dbcontext-lifetime)
- The same in other tracks: [Python](../../../python/overview.en.md) — CPython reference counting and cycle collector, `with` instead of `using`

## Sources

- [Fundamentals of garbage collection](https://learn.microsoft.com/dotnet/standard/garbage-collection/fundamentals)
- [The large object heap](https://learn.microsoft.com/dotnet/standard/garbage-collection/large-object-heap)
- [Cleaning up unmanaged resources](https://learn.microsoft.com/dotnet/standard/garbage-collection/unmanaged)
- [The using statement](https://learn.microsoft.com/dotnet/csharp/language-reference/statements/using)
- [IDisposable](https://learn.microsoft.com/dotnet/api/system.idisposable)
