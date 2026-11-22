---
title: "Performance"
lang: en
track: csharp
kind: module
doc: overview
slug: 19-performance
part: 3
levels: [1, 2, 3, 4]
tags: [performance, allocations, benchmarkdotnet, array-pool, native-aot, gc-pressure, simd]
versions: ".NET 11, C# 15; BenchmarkDotNet 0.16.0-preview.2"
status: draft
updated: 2026-11-22
---

# Performance

🌐 [Русский](overview.ru.md) · **English**

## In a nutshell

The performance of .NET code is time, throughput, memory and the load on the garbage collector. The main rule: measure first, optimize second. Micro-benchmarks are done with BenchmarkDotNet (in Release, with `MemoryDiagnoser`), bottlenecks in an application are found by a profiler. Most often the gain comes from reducing memory allocations: `StringBuilder`/`Concat`, avoiding boxing, `Span`, `ArrayPool`, structs instead of small classes, a loop instead of LINQ in a hot path; for startup and size — ReadyToRun and Native AOT.

## Key ideas

- **Measure:** a Release build, warm-up, many repetitions; compare ratios and allocations, not absolute numbers.
- **Allocations matter more than microseconds:** each allocation is pressure on the GC; checked with `GC.GetAllocatedBytesForCurrentThread` and `MemoryDiagnoser`.
- **Typical costs:** boxing (24 B), a capturing lambda (80 B), `params object[]` (120 B for three numbers), an interface `foreach` over `List<T>` (40 B), `s += x` in a loop.
- **Zero allocations:** a `static` lambda, `Span`/`AsSpan`, `stackalloc`, `ArrayPool`, `params ReadOnlySpan<T>`.
- **BenchmarkDotNet:** `[Benchmark]`, `Baseline`, `[MemoryDiagnoser]`, `[Params]`; use the result; it refuses to work in Debug.
- **The JIT:** tiered compilation and dynamic PGO; **Native AOT** gives fast start and small size at the cost of dynamic code.
- **SIMD and GC modes:** `Vector<T>`, vectorized BCL methods; Workstation/Server GC.

## Minimal example

```csharp
static long Alloc(Action a)
{
    a(); a();                                   // warm up
    long before = GC.GetAllocatedBytesForCurrentThread();
    a();
    return GC.GetAllocatedBytesForCurrentThread() - before;
}

string[] parts = Enumerable.Range(0, 50).Select(i => "p" + i).ToArray();
Console.WriteLine(Alloc(() => { var s = ""; foreach (var p in parts) s += p; }));   // 7968
Console.WriteLine(Alloc(() => { _ = string.Concat(parts); }));                      // 304
```

## When to use and when not

- ✅ Hot paths: a loop over millions of elements, parsing, request handling.
- ✅ High-load services where GC pauses are noticeable.
- ✅ Console utilities and serverless — fast start (Native AOT).
- ❌ Optimization without measurement and a profile.
- ❌ Worsening readability for a micro-gain outside hot paths.
- ❌ Conclusions from a benchmark in Debug or with a single run.

## Levels of this material

| Level | What's inside |
|---|---|
| [1. Basics](basics.en.md) | what to measure and with what, allocations and GC pressure, a cost table, simple techniques |
| [2. Advanced](advanced.en.md) | BenchmarkDotNet, pools and stack memory, LINQ versus a loop, structs, async, profiling |
| [3. Expert](expert.en.md) | the JIT and tiers, Native AOT, SIMD, GC modes, micro-benchmark pitfalls, gotchas, 12 interview questions |
| [4. Self-check](quiz.en.md) | 11 questions and [5 exercises](exercises.en.md) with solutions |
| 5. Runnable examples | not generated yet — `/learn csharp 19-performance level 5` |
| 6. Tests | not generated yet — `/learn csharp 19-performance level 6` |

## Related materials

- Prerequisites: [12 · Memory and GC](../12-memory-and-gc/overview.en.md), [12a · Span and Memory](../12a-span-and-memory/overview.en.md)
- Nearby: [10 · Async/await](../10-async-await/overview.en.md), [07 · LINQ](../07-linq/overview.en.md)
- Next: [22 · Observability](../22-observability/overview.en.md)
- Performance regressions: [16 · Unit tests](../16-testing-unit/overview.en.md)
- The same idea in other tracks: [Python](../../../python/overview.en.md) — profiling
- Recipes and libraries: [BenchmarkDotNet](../../libraries/benchmarkdotnet/overview.en.md), [Dapper](../../libraries/dapper/overview.en.md), [Mapperly](../../libraries/mapperly/overview.en.md), [Serilog](../../libraries/serilog/overview.en.md), [Spectre.Console](../../libraries/spectre-console/overview.en.md), [Caching](../../recipes/caching/overview.en.md), [File upload](../../recipes/file-upload/overview.en.md), [Pagination and filtering](../../recipes/pagination-filtering/overview.en.md)
