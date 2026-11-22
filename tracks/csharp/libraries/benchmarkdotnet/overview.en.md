---
title: "BenchmarkDotNet"
lang: en
track: csharp
kind: library
doc: overview
slug: benchmarkdotnet
part: 2
levels: [1, 2, 3, 4]
tags: [benchmarkdotnet, benchmarking, performance, memory-diagnoser]
versions: ".NET 11, C# 15; BenchmarkDotNet 0.16.0-preview.2"
status: draft
updated: 2026-11-22
---

# BenchmarkDotNet

🌐 [Русский](overview.ru.md) · **English**

## In a nutshell

BenchmarkDotNet is the standard microbenchmark library for .NET: it warms code up itself, runs benchmarks in a separate process, computes statistics and (with `MemoryDiagnoser`) memory allocations. Compare relative values (`Ratio`, `Allocated`), not absolute times. Run it only in Release.

## Key ideas

- `[Benchmark]`, `[Params]`, `[GlobalSetup]`, `[MemoryDiagnoser]`, `Baseline = true`.
- `Ratio` and `Allocated` are portable between machines; `Mean` is not.
- Verified: concatenation in a loop at N=1000 is ≈65 times slower than `StringBuilder` and allocates ≈1 MB; `Span` is ≈6 times faster than `Split` with no allocations.
- Release only; the library version must know the runtime version.
- Each benchmark is a separate process and a generated project.
- A microbenchmark does not replace profiling the application.

## Minimal example

```csharp
BenchmarkRunner.Run<Bench>();

[MemoryDiagnoser]
public class Bench
{
    [Benchmark] public int Sum() { var x = 0; for (var i = 0; i < 1000; i++) x += i; return x; }
}
```

## When to use and when not

- ✅ Comparing two implementations of a hot method.
- ✅ Checking that an optimization really reduces allocations.
- ✅ Choosing between data structures at realistic sizes.
- ❌ Code that depends on the network/disk/DB.
- ❌ Optimization without profiling.
- ❌ Absolute performance thresholds in CI.

## Levels of this material

| Level | What's inside |
|---|---|
| [1. Basics](basics.en.md) | the first benchmark, reading the report, attributes |
| [2. Advanced](advanced.en.md) | `GlobalSetup`, parameters, DCE, the environment |
| [3. Expert](expert.en.md) | how a run works, gotchas, alternatives, 11 interview questions |
| [4. Self-check](quiz.en.md) | 10 questions and [4 exercises](exercises.en.md) with solutions |
| 5. Runnable examples | not generated yet — `/learn csharp benchmarkdotnet level 5` |
| 6. Tests | not generated yet — `/learn csharp benchmarkdotnet level 6` |

## Related materials

- Prerequisites: [19 · Performance](../../modules/19-performance/overview.en.md), [12 · Memory and GC](../../modules/12-memory-and-gc/overview.en.md)
- Nearby: [12a · Span and memory](../../modules/12a-span-and-memory/overview.en.md), [Mapperly](../mapperly/overview.en.md), [Dapper](../dapper/overview.en.md)
- In other tracks: [Python](../../../python/overview.en.md) — `timeit`, `pytest-benchmark`
