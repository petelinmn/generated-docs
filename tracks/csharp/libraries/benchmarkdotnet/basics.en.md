---
title: "BenchmarkDotNet: basics"
lang: en
track: csharp
kind: library
doc: basics
slug: benchmarkdotnet
part: 2
tags: [benchmarkdotnet, benchmarking, performance, memory-diagnoser, microbenchmarks]
versions: ".NET 11, C# 15; BenchmarkDotNet 0.16.0-preview.2"
status: draft
updated: 2026-11-22
---

# BenchmarkDotNet: basics

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [19 · Performance](../../modules/19-performance/overview.en.md), [12 · Memory and GC](../../modules/12-memory-and-gc/overview.en.md). **Time:** ~30 min.

## What it is and why

BenchmarkDotNet is the standard **microbenchmark** tool for .NET. A simple `Stopwatch` measurement is unreliable: you need JIT warm-up, many repetitions, accounting for garbage collection and variance. BenchmarkDotNet runs a benchmark in a separate process, warms it up, repeats it, computes statistics and (with `MemoryDiagnoser`) memory allocations.

```bash
dotnet add package BenchmarkDotNet
dotnet run -c Release          # always Release
```

## The first benchmark

```csharp
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

BenchmarkRunner.Run<Bench>();

[MemoryDiagnoser]
public class Bench
{
    [Params(100, 1000)] public int N;

    [Benchmark(Baseline = true)]
    public string Concat() { var s = ""; for (var i = 0; i < N; i++) s += "x"; return s; }

    [Benchmark]
    public string Builder() { var sb = new StringBuilder(); for (var i = 0; i < N; i++) sb.Append('x'); return sb.ToString(); }
}
```

Real output (an x64 processor, .NET 11, a shortened `SimpleJob(warmupCount: 2, iterationCount: 5)`):

```text
| Method  | N    | Mean        | Error       | StdDev      | Ratio | Gen0     | Gen1   | Allocated | Alloc Ratio |
|-------- |----- |------------:|------------:|------------:|------:|---------:|-------:|----------:|------------:|
| Concat  | 100  |    904.1 ns |    17.56 ns |     2.72 ns |  1.00 |   1.3361 |      - |   12576 B |        1.00 |
| Builder | 100  |    178.1 ns |     3.37 ns |     0.88 ns |  0.20 |   0.0815 |      - |     768 B |        0.06 |
| Concat  | 1000 | 48,979.0 ns | 4,950.35 ns | 1,285.59 ns |  1.00 | 109.0088 | 0.5493 | 1025976 B |       1.000 |
| Builder | 1000 |    752.2 ns |    39.30 ns |     6.08 ns |  0.02 |   0.4864 | 0.0038 |    4576 B |       0.004 |
```

## How to read the table

| Column | Meaning |
|---|---|
| `Mean` | the average time of one call |
| `Error`, `StdDev` | half of the 99.9% confidence interval; the standard deviation |
| `Ratio` | the ratio to the baseline (`Baseline = true`) |
| `Gen0`/`Gen1` | the number of garbage collections per 1000 operations |
| `Allocated` | memory allocated per call (the most deterministic number) |

The conclusion from the example: at `N = 1000` string concatenation in a loop is **~65 times slower** than `StringBuilder` and allocates ~1 MB against ~4.5 KB. **Absolute times depend on the machine** — compare ratios and allocations.

## Attributes

| Attribute | Purpose |
|---|---|
| `[Benchmark]` | a benchmark method; `Baseline = true` — the comparison base |
| `[Params(...)]` | parameterizing a field/property |
| `[MemoryDiagnoser]` | the `Gen0/Gen1/Allocated` columns |
| `[SimpleJob(...)]` | warm-up/iteration/launch parameters |
| `[GlobalSetup]` | prepare data once (outside the measurement) |

## Beginner mistakes

- ❌ Running in Debug → ✅ only `-c Release` (in Debug BenchmarkDotNet refuses to work: "Benchmark was built without optimization enabled").
- ❌ Comparing absolute numbers from another machine → ✅ compare `Ratio` and `Allocated`.
- ❌ Preparing data inside `[Benchmark]` → ✅ `[GlobalSetup]` ([advanced](advanced.en.md#setup-and-parameters)).
- ❌ Not returning the result → ✅ return a value so the JIT does not cut out the computation.
- ❌ Measuring something that depends on the network/disk → ✅ a microbenchmark is for pure CPU logic.

## Related materials

- Next: [advanced](advanced.en.md) — setup, parameters, comparing implementations, traps.
- [19 · Performance](../../modules/19-performance/overview.en.md) — the optimization strategy.
- [12 · Memory and GC](../../modules/12-memory-and-gc/overview.en.md) — allocations and generations.
- [12a · Span and memory](../../modules/12a-span-and-memory/overview.en.md) — typical optimizations.
- [Python](../../../python/overview.en.md) — `timeit` and `pytest-benchmark`.

## Sources

- [BenchmarkDotNet: Getting started](https://benchmarkdotnet.org/articles/guides/getting-started.html)
- [BenchmarkDotNet: Good practices](https://benchmarkdotnet.org/articles/guides/good-practices.html)
- [BenchmarkDotNet: Memory diagnoser](https://benchmarkdotnet.org/articles/configs/diagnosers.html)
