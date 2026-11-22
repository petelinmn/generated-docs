---
title: "BenchmarkDotNet: advanced"
lang: en
track: csharp
kind: library
doc: advanced
slug: benchmarkdotnet
part: 2
tags: [benchmarkdotnet, globalsetup, params, baseline, dead-code-elimination, jobs]
versions: ".NET 11, C# 15; BenchmarkDotNet 0.16.0-preview.2"
status: draft
updated: 2026-11-22
---

# BenchmarkDotNet: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [basics](basics.en.md). **Time:** ~45 min.

## Setup and parameters

Data for the measurement is prepared in `[GlobalSetup]` — it runs once and is **not part** of the measured time:

```csharp
[MemoryDiagnoser]
public class Bench
{
    private string _text = "";
    [GlobalSetup] public void Setup() => _text = string.Join(",", Enumerable.Range(0, 1000));

    [Benchmark(Baseline = true)] public int SplitCount() => _text.Split(',').Length;
    [Benchmark] public int SpanCount() { var n = 1; foreach (var c in _text.AsSpan()) if (c == ',') n++; return n; }
}
```

Real output (a string of 1000 numbers separated by commas):

| Method | Mean | Ratio | Allocated |
|---|---|---|---|
| `SplitCount` | 6,350.5 ns | 1.00 | 39,944 B |
| `SpanCount` | 1,042.0 ns | 0.16 | **0 B** |

Counting commas through a `Span` is ~6 times faster and allocates no memory: `Split` creates an array of strings. This is a typical win from the optimizations of [12a · Span and memory](../../modules/12a-span-and-memory/overview.en.md). `[Params]` iterates the parameter values (see [basics](basics.en.md#the-first-benchmark)), giving a separate row for each.

## Return the result

BenchmarkDotNet "consumes" a method's return value, so the JIT cannot throw the computation out as dead code (dead code elimination, DCE). A check: a summing loop `x += i` without a return and with one gave close values (`239.2 ns` and `279.4 ns`) — in this case the JIT did not drop the result, but you cannot rely on it: return a value or use a `Consumer`.

> ⚠️ Verify: scenarios in which the JIT really removes an unused computation, and the `Consumer`/`Blackhole` class, were not run in this material.

## Run configuration

`[SimpleJob(warmupCount: 2, iterationCount: 5, launchCount: 1)]` shortens the run: this is how the examples in this material were verified (36 seconds for 4 benchmarks). For reliable measurements leave the defaults — BenchmarkDotNet itself picks the number of iterations until the result is stable.

| Parameter | What it does |
|---|---|
| `warmupCount` | the number of warm-up iterations |
| `iterationCount` | the number of measured iterations |
| `launchCount` | the number of process launches |
| `[Params]` | a parameter matrix |

In the "Hints" section of the output BenchmarkDotNet reports outliers (`Outliers … 1 outlier was removed`) — measurement noise.

## When a benchmark lies

- **A Debug build** — the run is refused (verified).
- **A method that is too short** — timer noise exceeds the method's time; BenchmarkDotNet automatically increases the number of operations per iteration.
- **Different code with different JIT optimization** — measure on the target .NET (tiered compilation, PGO).
- **The CPU cache and data** — small sets sit in the cache and do not show the real cost of memory access.
- **Dependence on the outside world** — network/disk give variance unrelated to your code.

## The environment (from this material's practice)

- BenchmarkDotNet **0.15.8** did not recognize .NET 11 (`NotImplementedException: GetRuntimeVersion … NotRecognized`); 0.16.0-preview.2 ran.
- Running a project from a deep temp folder led to a build error of the child project ("Unable to find … in C:\Users\… and its subfolders") — keep the project in an ordinary directory.
- In Debug the run is rejected; for a quick check of the examples `DefaultConfig.Instance.WithOptions(ConfigOptions.DisableOptimizationsValidator)` was used — **not** for real measurements.

## Best practices

| Rule | Why |
|---|---|
| Release and the target .NET version | real conditions |
| `Baseline = true` and `Ratio` | comparability between machines |
| `MemoryDiagnoser` always | allocations are the most stable number |
| Setup in `[GlobalSetup]` | measure only what is needed |
| Return the result | protection from DCE |
| Do not trust a single run | look at `StdDev`, outliers |

## Related materials

- Back: [basics](basics.en.md). Next: [expert](expert.en.md).
- [19 · Performance](../../modules/19-performance/advanced.en.md) — strategy and pools.
- [12a · Span and memory](../../modules/12a-span-and-memory/overview.en.md)
- [Mapperly](../mapperly/overview.en.md), [Dapper](../dapper/overview.en.md) — what is worth comparing with benchmarks.

## Sources

- [BenchmarkDotNet: Setup and cleanup](https://benchmarkdotnet.org/articles/features/setup-and-cleanup.html)
- [BenchmarkDotNet: Baseline](https://benchmarkdotnet.org/articles/features/baselines.html)
- [BenchmarkDotNet: Good practices](https://benchmarkdotnet.org/articles/guides/good-practices.html)
