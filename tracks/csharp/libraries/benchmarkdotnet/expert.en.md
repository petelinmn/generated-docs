---
title: "BenchmarkDotNet: expert"
lang: en
track: csharp
kind: library
doc: expert
slug: benchmarkdotnet
part: 2
tags: [benchmarkdotnet, toolchains, jit, tiered-compilation, statistics, interview-questions]
versions: ".NET 11, C# 15; BenchmarkDotNet 0.16.0-preview.2"
status: draft
updated: 2026-11-22
---

# BenchmarkDotNet: expert

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · **3. Expert** · [4. Quiz](quiz.en.md)

**Prerequisites:** [basics](basics.en.md) and [advanced](advanced.en.md). **Time:** ~30 min.

## Internals

BenchmarkDotNet does not measure code in your process. It:

1. finds methods with `[Benchmark]`;
2. **generates a separate project** with wrapper code for each benchmark (that is why the child build looks for the `.csproj`);
3. builds it in Release and runs it as a **separate process** (the "default" toolchain — for a specific runtime version);
4. runs the stages: *Pilot* (choosing the number of operations per iteration), *Warmup*, *Actual* (measurements); recomputes the time per operation by `OperationsPerInvoke`;
5. computes statistics (the mean, the confidence interval, the standard deviation), marks outliers;
6. supplements with diagnoser data (`MemoryDiagnoser`: GC counters and `Allocated`).

Verified indirectly: the child process in our environment really needed access to the `.csproj` (the error "Unable to find … and its subfolders" from a deep folder); the values `IterationCount`, `WarmupCount`, `LaunchCount` were printed in the report header.

> ⚠️ Verify: the internal stages (*Pilot*, *Overhead*) and the behavior of the in-process toolchain were not investigated in this material; check the documentation — in version 0.16 the in-process toolchain API differs from 0.15.

## Edges and gotchas

**1. Release only.** Verified: a refusal in Debug.

**2. Absolute numbers are not portable.** Time depends on the processor, power, JIT version; compare `Ratio`.

**3. Outliers and noise.** Verified: the report says `1 outlier was removed`; with a small `iterationCount` the confidence interval is wide (`Error 4950 ns` at `Mean 48,979 ns` for `N=1000`).

**4. Allocations are more accurate than time.** `Allocated` is deterministic (verified: `12576 B` at `N=100`), time floats.

**5. Setup inside a benchmark.** It ends up in the measurement; move it into `[GlobalSetup]`.

**6. The BenchmarkDotNet version and the runtime version.** Verified: 0.15.8 did not recognize .NET 11, 0.16.0-preview.2 did.

**7. Tiered compilation and PGO.** Code is first compiled quickly and without optimizations, then recompiled; BenchmarkDotNet's warm-up exists for exactly this. "First run" results in an application may differ from steady-state ones.

**8. Dead code elimination.** An unused result could in theory be cut out; always return a value (on our loop there was no difference — do not rely on it).

**9. Micro vs macro.** A nanosecond win does not matter if the method is called rarely; profile the application first.

**10. Parallel runs.** Do not run benchmarks simultaneously with other load on the machine — noise.

## Trade-offs and alternatives

| Approach | Pros | Cons |
|---|---|---|
| BenchmarkDotNet | reliable microbenchmarks, statistics, diagnosers | slow, micro-level only |
| `Stopwatch` by hand | quick and simple | warm-up, noise, GC — unreliable |
| Profilers (dotnet-trace, PerfView) | find bottlenecks in a real application | do not give precise comparisons of variants |
| Load tests (k6, NBomber) | real scenarios | not about a single method |
| Built-in counters and metrics | in production | indirect |

## Interview questions

<details><summary>1. Why BenchmarkDotNet if there is `Stopwatch`?</summary>

JIT warm-up, many iterations, statistics, GC accounting, process isolation — `Stopwatch` over one run gives unreliable numbers.

</details>

<details><summary>2. Why are benchmarks run in Release?</summary>

In Debug code is not optimized; BenchmarkDotNet refuses to work (verified).

</details>

<details><summary>3. What do `Mean`, `Error`, `StdDev`, `Ratio` mean?</summary>

The mean; half of the 99.9% confidence interval; the standard deviation; the ratio to the baseline benchmark.

</details>

<details><summary>4. How do you measure memory allocations?</summary>

`[MemoryDiagnoser]`: `Gen0/Gen1/Allocated` (verified: 12,576 B against 768 B).

</details>

<details><summary>5. What is `[GlobalSetup]` for?</summary>

To prepare data once, outside the measurement.

</details>

<details><summary>6. What are `Baseline` and `Ratio`?</summary>

The comparison base and the relative time (verified: `Builder` = 0.02 of `Concat` at N=1000).

</details>

<details><summary>7. How does BenchmarkDotNet isolate benchmarks?</summary>

It generates a separate project per benchmark and runs a separate process.

</details>

<details><summary>8. Why return a value from a benchmark?</summary>

So the JIT does not remove the computation as dead code.

</details>

<details><summary>9. Trick question: an optimization gives 5% in a microbenchmark but the application did not get faster. Why?</summary>

The method is not hot; the bottleneck is elsewhere (DB, network, allocations in another part). You need a profiler.

</details>

<details><summary>10. Trick question: a benchmark at `N=1000` showed a huge difference and at `N=10` almost none. What do you do?</summary>

Parameterize with `[Params]` using realistic sizes; algorithmic complexity shows on large data (verified: 5× at 100, 65× at 1000).

</details>

<details><summary>11. Trick question: on CI the same benchmark gives other numbers. Why?</summary>

A different machine/load/power; compare `Ratio` and `Allocated` in the same environment and do not use absolute thresholds.

</details>

## Related materials

- Back: [advanced](advanced.en.md). Check yourself: [quiz](quiz.en.md) and [exercises](exercises.en.md).
- [19 · Performance](../../modules/19-performance/overview.en.md)
- [12 · Memory and GC](../../modules/12-memory-and-gc/overview.en.md)
- [22 · Observability](../../modules/22-observability/overview.en.md) — metrics in production.

## Sources

- [BenchmarkDotNet: How it works](https://benchmarkdotnet.org/articles/guides/how-it-works.html)
- [BenchmarkDotNet: Toolchains](https://benchmarkdotnet.org/articles/configs/toolchains.html)
- [.NET: Tiered compilation](https://learn.microsoft.com/dotnet/core/runtime-config/compilation)
