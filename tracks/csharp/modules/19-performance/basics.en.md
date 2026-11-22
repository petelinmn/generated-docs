---
title: "Performance: basics"
lang: en
track: csharp
kind: module
doc: basics
slug: 19-performance
part: 3
tags: [performance, allocations, gc-pressure, boxing, stringbuilder, benchmarking]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-22
---

# Performance: basics

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [12 · Memory and GC](../12-memory-and-gc/overview.en.md), [12a · Span and Memory](../12a-span-and-memory/overview.en.md), [07 · LINQ](../07-linq/overview.en.md). **Time:** ~1 h.

## Why it matters

Performance is response time (latency), throughput, memory consumption and load on the garbage collector. For most applications "ordinary" code is enough; but in hot paths (loops, request handling, parsing) extra memory allocations and unfortunate techniques noticeably slow a program down. The main rule: **measure first, optimize second**. Guessing the "bottleneck" is almost always wrong, and optimization without measurement complicates code for no benefit.

## What to measure

| Metric | What to measure with |
|---|---|
| execution time | BenchmarkDotNet (micro), a profiler (in an application) |
| memory allocations | `GC.GetAllocatedBytesForCurrentThread()`, `MemoryDiagnoser` |
| GC load | the number of collections (`GC.CollectionCount`), `dotnet-counters` counters |
| latency and throughput | load tests (k6, bombardier) |

Always measure a **Release build**: in Debug the code is not optimized and the numbers are meaningless (BenchmarkDotNet refuses to work in Debug — verified: "Benchmark was built without optimization enabled").

## Allocations and GC pressure

Every heap allocation is work for the [garbage collector](../12-memory-and-gc/overview.en.md): the more garbage, the more frequent the collections. In the verified loop of 200,000 allocations of 1 KB arrays, 22 generation-0 collections occurred. So in hot code it matters to know **what allocates memory**. Measuring this is easy:

```csharp
static long Alloc(Action a)
{
    a(); a();                                   // warm up (JIT, caches)
    long before = GC.GetAllocatedBytesForCurrentThread();
    a();
    return GC.GetAllocatedBytesForCurrentThread() - before;
}
```

Verified on .NET 11 (Release), allocations per call:

| Operation | Bytes |
|---|---|
| `object o = n;` (boxing an `int`) | 24 |
| `$"v={n}"` (interpolation with a number) | 32 |
| `"a" + s` (concatenating two strings) | 32 |
| a lambda capturing a local variable | 80 |
| a `static` lambda (no capture) | 0 |
| `foreach` over a `List<int>` (a variable of type `List<int>`) | 0 |
| `foreach` over the same list through `IEnumerable<int>` | 40 |
| `arr.Where(...).Sum()` on an array of 8 | 48 |
| the same `foreach` loop with an `if` over the array | 0 |
| `new int[256]` | 1048 |
| `ArrayPool<int>.Shared.Rent(256)` + `Return` | 0 |
| `stackalloc int[256]` | 0 |
| `Substring` (5 characters) | 32 |
| an `AsSpan` slice | 0 |
| a `params object[]` call with 3 numbers | 120 |
| a `params ReadOnlySpan<int>` call with 3 numbers | 0 |

The numbers are for 64-bit .NET 11; they illustrate orders of magnitude, not a guarantee.

## Simple techniques

**Strings.** `s += x` in a loop creates a new string each time: 50 concatenations allocated 7968 bytes, `StringBuilder` — 1176, `string.Concat(array)` — 304 (verified). For many pieces use `StringBuilder` or `string.Concat/Join`.

**LINQ in a hot path.** A `Where(...).Sum()` chain allocates iterator objects and a delegate (48 bytes on a small array); the equivalent loop — 0. Outside hot paths LINQ is fine and readable ([07 · LINQ](../07-linq/overview.en.md)).

**Boxing.** Converting a value type to `object` (including through `params object[]` or untyped collections) allocates memory — use generics and typed methods.

**Collections.** Set the capacity if the size is known (`new List<int>(1000)`): otherwise the list grows by doubling with copying ([06 · Collections and generics](../06-collections-and-generics/overview.en.md)).

**Slices.** `Substring`, `Split` are copies; `AsSpan`/`ReadOnlySpan<char>` — no allocations ([12a · Span and Memory](../12a-span-and-memory/overview.en.md)).

**Structs instead of small classes.** An array of 100 instances of a small class is 3224 bytes and 100 heap objects; an array of 100 structs with the same fields is 824 bytes (verified): fewer objects, better data locality. But large and mutable structs are costly to copy ([05 · Interfaces, structs, records](../05-interfaces-structs-records/overview.en.md)).

## Beginner mistakes

- ❌ Optimizing "by eye" → ✅ measure, find the bottleneck, optimize, measure again.
- ❌ Measuring with a single `Stopwatch` run in Debug → ✅ Release, warm-up, many repetitions, BenchmarkDotNet.
- ❌ Removing LINQ everywhere → ✅ only in hot paths confirmed by measurement.
- ❌ Concatenating strings in a loop with `+=` → ✅ `StringBuilder`.
- ❌ Ignoring allocations and GC → ✅ look at `MemoryDiagnoser`.
- ❌ Sacrificing readability for a micro-gain → ✅ an optimization must pay off on a measured workload.

## Related materials

- Next: [advanced theory](advanced.en.md) — BenchmarkDotNet, pools, structs, profiling.
- [12 · Memory and GC](../12-memory-and-gc/overview.en.md) — generations and collections.
- [12a · Span and Memory](../12a-span-and-memory/overview.en.md) — allocations without copies.
- [07 · LINQ](../07-linq/overview.en.md) — where LINQ is costly.
- [Python](../../../python/overview.en.md) — profiling and optimization in Python.

## Sources

- [.NET performance guidance](https://learn.microsoft.com/dotnet/core/performance/)
- [`GC.GetAllocatedBytesForCurrentThread`](https://learn.microsoft.com/dotnet/api/system.gc.getallocatedbytesforcurrentthread)
- [Garbage collection](https://learn.microsoft.com/dotnet/standard/garbage-collection/fundamentals)
- [BenchmarkDotNet](https://benchmarkdotnet.org/)
