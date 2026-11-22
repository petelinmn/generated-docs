---
title: "Performance: advanced"
lang: en
track: csharp
kind: module
doc: advanced
slug: 19-performance
part: 3
tags: [performance, benchmarkdotnet, array-pool, stackalloc, struct, profiling, memory-diagnoser]
versions: ".NET 11, C# 15; BenchmarkDotNet 0.16.0-preview.2"
status: draft
updated: 2026-11-22
---

# Performance: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [basics](basics.en.md). **Time:** ~1.5 h.

## BenchmarkDotNet

Micro-benchmarks are measured not with `Stopwatch` but with BenchmarkDotNet: it warms up the code itself, runs many iterations, estimates the spread and can count allocations. The package is `BenchmarkDotNet`; the project is a console one, run in **Release**: `dotnet run -c Release`.

```csharp
[MemoryDiagnoser]
public class Strings
{
    private readonly string[] _parts = Enumerable.Range(0, 100).Select(i => "item" + i).ToArray();

    [Benchmark(Baseline = true)]
    public string Concat() { var s = ""; foreach (var p in _parts) s += p; return s; }

    [Benchmark]
    public string StringBuilder_() { var sb = new StringBuilder(); foreach (var p in _parts) sb.Append(p); return sb.ToString(); }

    [Benchmark]
    public string Join() => string.Concat(_parts);
}

BenchmarkRunner.Run<Strings>();
```

A real run (Intel Core i5-12400, Windows 11, .NET 11.0.0, the quick `ShortRun` mode, so the error is large):

```text
| Method         | Mean       | Ratio | Gen0   | Allocated | Alloc Ratio |
| Concat         | 3,104.0 ns |  1.00 | 6.5079 |  59.81 KB |        1.00 |
| StringBuilder_ |   403.0 ns |  0.13 | 0.3996 |   3.67 KB |        0.06 |
| Join           |   275.9 ns |  0.09 | 0.1283 |   1.18 KB |        0.02 |
```

Read the columns: `Mean` — the average call time, `Ratio` — the ratio to the baseline (`Baseline = true`), `Allocated` — memory per call (the most reliable number: it is deterministic), `Gen0` — the number of generation-0 collections per 1000 operations. Time depends on the processor and version, so compare **ratios**.

| Attribute | Purpose |
|---|---|
| `[Benchmark]`, `[Benchmark(Baseline = true)]` | a measured method, the comparison base |
| `[MemoryDiagnoser]` | allocations and collections |
| `[Params(10, 1000)]` | one benchmark on several input sizes |
| `[GlobalSetup]` / `[IterationSetup]` | data preparation outside the measurement |
| `[SimpleJob(...)]`, `ShortRun` | the run profile (accuracy versus time) |

A benchmark's result must be **used** (returned from the method): otherwise the compiler may drop the "unused" computed code. Data preparation goes in `GlobalSetup`.

Environment nuances (verified): in Debug BenchmarkDotNet refuses to work ("Benchmark was built without optimization enabled"); version 0.15.8 did not recognize .NET 11 (`NotImplementedException: GetRuntimeVersion not implemented for NotRecognized`), while 0.16.0-preview.2 ran; when the project was run from a deep temporary folder, the child build could not find the `.csproj` ("Unable to find … in C:\Users\… and its subfolders") — keep the project in an ordinary directory.

## Pools and stack memory

For temporary buffers:

| Approach | Time | Allocated |
|---|---|---|
| `new byte[4096]` | 131.6 ns | 4120 B |
| `ArrayPool<byte>.Shared.Rent(4096)` + `Return` | 7.6 ns | 0 |
| `stackalloc byte[4096]` | 20.9 ns | 0 |

(The same `ShortRun` conditions; the relative order is more reliable than absolute numbers.) The pool returns an array that is **not zeroed** and possibly longer; `stackalloc` suits small sizes (4 KB is fine, megabytes are not). Details are in [12a · Span and Memory](../12a-span-and-memory/advanced.en.md#arraypool-reusing-arrays).

## LINQ versus a loop

On 1000 elements (`ShortRun`):

| Method | Time | Allocated |
|---|---|---|
| `Where(x => x % 2 == 0).Sum()` | 733.6 ns | 48 B |
| a `foreach` loop with a condition | 254.2 ns | 0 |
| a loop over a `Span` with a bitwise `&` | 254.4 ns | 0 |

The loop is about 3 times faster and does not allocate, but the absolute gain is small: it is justified in a hot path executed millions of times. A loop over a `Span` gave no gain over a loop over an array: the JIT removes the extra bounds checks anyway ([12a · Span and Memory](../12a-span-and-memory/expert.en.md#how-the-jit-works-with-span)).

## Structs and data

- An array of structs stores data contiguously — good cache locality; an array of class references holds pointers to scattered objects (verified: 824 versus 3224 bytes for 100 elements).
- Structs should be small (about 16 bytes) and immutable (`readonly struct`); passing by `in` avoids copying but can carelessly create defensive copies ([05 · Interfaces, structs, records](../05-interfaces-structs-records/advanced.en.md#mutable-structs)).
- `sealed` on classes lets the JIT devirtualize calls.
- `static` lambdas and cached delegates do not allocate memory ([09 · Delegates, events, lambdas](../09-delegates-events-lambdas/expert.en.md#a-non-capturing-lambda-and-the-cache)).

## Asynchrony and performance

Each `async` method is a state machine; on a real suspension an object is allocated. On paths that complete synchronously `ValueTask` helps ([10 · Async/await](../10-async-await/advanced.en.md#valuetask)). The main thing for server throughput is not to block threads: blocking code causes pool starvation ([10 · Async/await](../10-async-await/expert.en.md#thread-pool-and-starvation)).

## Profiling

Micro-benchmarks answer "which of two variants is faster", while a **profiler** answers "where in my application is time spent". .NET tools: `dotnet-counters` (real-time metrics: allocations, GC, the thread pool), `dotnet-trace` (tracing and CPU profile), `dotnet-dump` (dump analysis), PerfView, the Visual Studio and Rider profilers.

> ⚠️ Verify: the commands and output of `dotnet-counters`/`dotnet-trace` were not run in this material (the tools were not installed). The typical order: `dotnet tool install -g dotnet-counters`, then `dotnet-counters monitor --process-id <PID>`; a trace is taken with `dotnet-trace collect --process-id <PID>` and opened in PerfView/Speedscope. Check the parameters against the documentation before use.

## Best practices

| Rule | Why |
|---|---|
| Profile → hypothesis → change → measurement | otherwise you optimize the wrong thing |
| A release build, warm-up, many repetitions | reliable numbers |
| Look at `Allocated`, not just time | allocations affect the whole application through the GC |
| Optimize hot paths | leave the rest readable |
| Ratios matter more than absolute numbers | the figures depend on the machine |
| Capture regressions with benchmarks | performance breaks unnoticed |

## Related materials

- Back: [basics](basics.en.md). Next: [expert theory](expert.en.md) — the JIT, Native AOT, SIMD.
- [12a · Span and Memory](../12a-span-and-memory/overview.en.md) — buffers without copies.
- [10 · Async/await](../10-async-await/overview.en.md) — async and the thread pool.
- [06 · Collections and generics](../06-collections-and-generics/overview.en.md) — capacity and operation complexity.
- [22 · Observability](../22-observability/overview.en.md) — metrics and tracing in production.

## Sources

- [BenchmarkDotNet: documentation](https://benchmarkdotnet.org/articles/overview.html)
- [`ArrayPool<T>`](https://learn.microsoft.com/dotnet/api/system.buffers.arraypool-1)
- [.NET diagnostic tools](https://learn.microsoft.com/dotnet/core/diagnostics/)
- [dotnet-counters](https://learn.microsoft.com/dotnet/core/diagnostics/dotnet-counters)
