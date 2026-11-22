---
title: "Performance: expert"
lang: en
track: csharp
kind: module
doc: expert
slug: 19-performance
part: 3
tags: [performance, jit, tiered-compilation, native-aot, simd, gc-modes, microbenchmark-pitfalls, interview-questions]
versions: ".NET 11, C# 15; BenchmarkDotNet 0.16.0-preview.2"
status: draft
updated: 2026-11-22
---

# Performance: expert

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · **3. Expert** · [4. Quiz](quiz.en.md)

**Prerequisites:** [basic](basics.en.md) and [advanced](advanced.en.md) theory. **Time:** ~1.5 h.

## The JIT and tiered compilation

.NET code is compiled to IL, and the JIT turns it into machine code on the first call. So that an application starts quickly yet still works well on hot spots, **tiered compilation** is used: a method is first compiled quickly with no complex optimizations (tier 0), and when it turns out to be "hot", it is recompiled with optimization (tier 1). Hot loops in long-running methods are replaced on the fly (OSR, on-stack replacement). Dynamic PGO (profile-guided optimization) gathers execution statistics at the early tiers and uses them for optimization: devirtualizing calls, code layout.

Consequences for measurement: the first calls of a method are slower than the steady state; so benchmarks warm the code up, and a `Stopwatch` around one call proves nothing. **ReadyToRun** (precompiled code, faster start, then recompilation of hot methods) and **Native AOT** (a fully precompiled executable) remove part of the startup cost.

> ⚠️ Verify: the specific thresholds for moving between tiers and the settings (`TieredCompilation`, `TieredPGO`, `TC_QuickJitForLoops`) depend on the runtime version; check the "Configuring tiered compilation" documentation for .NET 11 before relying on numbers.

## Native AOT

`PublishAot=true` compiles the application ahead of time to machine code with no JIT. Verified: a console "hello" application was published with `dotnet publish -c Release` (the C++ toolchain component for the platform is required), producing a self-contained `aot.exe` of about 900 KB (923,136 bytes) with no runtime installed; there `RuntimeFeature.IsDynamicCodeSupported` is `False` (under the JIT — `True`, and `IsDynamicCodeCompiled` — `True`). Pros: fast start, small size, no JIT. Limitations: no dynamic code generation (`Reflection.Emit`, some expressions), reflection is limited by trimming, source generators are needed (JSON, regular expressions, configuration binding), and publishing is for a specific platform. It suits CLIs, serverless and containers with fast start; it is inconvenient for applications that use reflection heavily ([20 · Reflection and source generators](../20-reflection-and-source-generators/overview.en.md)).

## SIMD and vectorization

Modern processors handle several values with one instruction (SIMD). In .NET: `Vector<T>` (the size depends on the machine), `Vector128/256/512<T>`, and vectorized BCL methods (`Span.IndexOf`, `SequenceEqual`, `Sum` for some types, `SearchValues`). Verified: on the test machine `Vector.IsHardwareAccelerated == True`, `Vector<int>.Count == 8` (256 bits); the sum of 1000 numbers through `Vector<int>` matched `Sum()` (`499500`):

```csharp
static int SumVec(int[] a)
{
    var acc = Vector<int>.Zero; int i = 0;
    for (; i <= a.Length - Vector<int>.Count; i += Vector<int>.Count) acc += new Vector<int>(a, i);
    int s = Vector.Sum(acc);
    for (; i < a.Length; i++) s += a[i];            // the tail that did not fit a whole vector
    return s;
}
```

Writing SIMD by hand is worth it only for the hottest kernels after measuring; the ready BCL methods are usually enough.

## GC modes

| Mode | Characteristic |
|---|---|
| Workstation, concurrent (the default in the verified environment: `ServerGC=False`, `LatencyMode=Interactive`) | short pauses for interactive applications |
| Server | separate heaps and GC threads per core; higher throughput, more memory |
| Non-concurrent | no background GC; less overhead |
| `GCSettings.LatencyMode` / `GC.TryStartNoGCRegion` | temporary limits on collections in critical sections |

The mode is set in `runtimeconfig.json` (`System.GC.Server`, `System.GC.Concurrent`) or by environment variables. More on generations and modes in [12 · Memory and GC](../12-memory-and-gc/expert.en.md).

## Micro-benchmark pitfalls

- **Dead-code elimination.** If a computation's result is unused, the JIT may drop it: return a value from the benchmark.
- **Constant folding.** A constant input lets the result be computed at compile time: take data from fields/`GlobalSetup`.
- **Warm-up and JIT tiers.** Measure after stabilization; BenchmarkDotNet does this.
- **Machine noise.** Background processes, power modes, thermal limits: close extras, compare ratios.
- **Unrepresentative data.** The size and shape of the input affect the result (`[Params]`).
- **Spread.** `ShortRun` gives a large error (in the verified runs `Error` was tens of percent): conclusions need the full mode.
- **Measuring the wrong thing.** A micro-benchmark does not show the effect on the GC of the whole application — look at allocations.

## Reference

| What | Tool / API |
|---|---|
| Micro-benchmarks | BenchmarkDotNet (`[Benchmark]`, `[MemoryDiagnoser]`, `[Params]`) |
| Allocations | `GC.GetAllocatedBytesForCurrentThread`, `GC.GetTotalAllocatedBytes`, `MemoryDiagnoser` |
| Collections | `GC.CollectionCount(n)` (verified: 22 gen0 collections for 200k allocations of 1 KB) |
| Buffers | `ArrayPool<T>`, `stackalloc`, `Span<T>`, `MemoryPool<T>` |
| Vectorization | `Vector<T>`, `Vector128/256/512`, `SearchValues` |
| Startup and size | ReadyToRun, trimming, Native AOT |
| Diagnostics | `dotnet-counters`, `dotnet-trace`, `dotnet-dump`, PerfView |
| GC settings | `System.GC.Server`, `System.GC.Concurrent`, `GCSettings` |

## Gotchas

**1. Optimization without measurement.** The most frequent source of complicated code with no benefit.

**2. Debug and Release.** Debug can be an order of magnitude slower; BenchmarkDotNet results in Debug are forbidden.

**3. Absolute numbers from someone else's machine.** Compare ratios and allocations.

**4. LINQ "is always slow".** LINQ's allocations on 1000 elements (48 B) are small; in most places it is appropriate. A loop is about 3 times faster, but whether it matters in your hot path is decided by the profile.

**5. A `static` lambda versus a capturing lambda.** 0 versus 80 bytes per call (verified): avoid capturing in hot code.

**6. A concrete collection seen as `IEnumerable<T>`.** `foreach` over a `List<int>` through the interface allocates 40 bytes (a boxed enumerator), through the `List<int>` type — 0 (verified).

**7. `params object[]`.** Each call with arguments is an array allocation and boxing (120 bytes for three numbers); `params ReadOnlySpan<T>` — 0 (verified).

**8. A pool without returning.** `ArrayPool` is fast as long as you return buffers; a forgotten `Return` turns it into an ordinary allocation.

## Trade-offs and alternatives

| Approach | Pros | Cons |
|---|---|---|
| Readable code with LINQ | easier to maintain | allocations in a hot path |
| Manual loops, pools, `Span` | fewer allocations | harder and more brittle |
| Native AOT | startup and size | limits on reflection and dynamic code |
| ReadyToRun | faster start | larger assemblies |
| Server GC | throughput | more memory |
| Caching results | removes work | invalidation and memory |

## Interview questions

<details><summary>1. How do you approach performance optimization?</summary>

I measure: a profiler shows the bottleneck, a benchmark compares variants, metrics (allocations, GC collections) show side effects. Then I change one place and measure again. You cannot optimize without measurement.

</details>

<details><summary>2. Why can't you measure with a single `Stopwatch` run?</summary>

The first call includes JIT compilation and warm-up, the spread is large, JIT optimizations change by tier. You need warm-up, many iterations and statistics — BenchmarkDotNet.

</details>

<details><summary>3. What is tiered compilation?</summary>

A method is first compiled quickly (tier 0), a hot one is recompiled with optimization (tier 1); loops are replaced on the fly (OSR); dynamic PGO uses the execution profile.

</details>

<details><summary>4. What is GC pressure and how do you reduce it?</summary>

A large number of allocations causes frequent collections (verified: 22 gen0 collections for 200k allocations of 1 KB). Reduce it with pools, `Span`/`stackalloc`, structs, and avoiding boxing and extra strings/LINQ in a hot path.

</details>

<details><summary>5. When `StringBuilder`, and when `+`?</summary>

`+` for a few fixed pieces (the compiler optimizes it); in a loop — `StringBuilder` or `string.Concat/Join` (verified: 50 concatenations — 7968 versus 1176 and 304 bytes).

</details>

<details><summary>6. What is Native AOT and what are its limitations?</summary>

Ahead-of-time compilation to machine code without a JIT: fast start and small size, no runtime installed. Limitations: no dynamic code generation, reflection and trimming need generators and annotations.

</details>

<details><summary>7. What is `ArrayPool<T>` for and what are its risks?</summary>

Reusing large arrays instead of allocating. Risks: not returning (`Return`), using after return, not zeroed, a length greater than requested.

</details>

<details><summary>8. What does `[MemoryDiagnoser]` do?</summary>

It adds to the report the number of collections by generation and the memory allocated per operation — the most stable benchmark numbers.

</details>

<details><summary>9. What does SIMD give?</summary>

Processing several values with one instruction: `Vector<T>`, `Vector128/256/512`, vectorized BCL methods. A gain on long data; by hand — only for the hottest kernels.

</details>

<details><summary>10. Trick question: is a loop over a `Span` faster than over an array?</summary>

No: in the verified measurement a loop over `int[]` and over `Span<int>` gave the same time (254 ns) — the JIT removes bounds checks in both cases. The gain of `Span` is the absence of copies.

</details>

<details><summary>11. Trick question: a benchmark showed a huge speedup, but nothing changed in the application. Why?</summary>

Perhaps the JIT dropped an unused result or constants folded; or the spot is not hot under the real workload. You need a returned value, realistic input data and a profile of the application.

</details>

<details><summary>12. Trick question: why does BenchmarkDotNet refuse to work in Debug?</summary>

A Debug build is not optimized, the results are meaningless; the program reports "built without optimization enabled" (verified).

</details>

## Related materials

- Back: [advanced theory](advanced.en.md). Check yourself: [quiz](quiz.en.md) and [exercises](exercises.en.md).
- [12 · Memory and GC](../12-memory-and-gc/overview.en.md) — the GC and generations.
- [12a · Span and Memory](../12a-span-and-memory/overview.en.md) — buffers without copies.
- [20 · Reflection and source generators](../20-reflection-and-source-generators/overview.en.md) — generators for AOT.
- [22 · Observability](../22-observability/overview.en.md) — performance metrics in production.

## Sources

- [Tiered compilation](https://learn.microsoft.com/dotnet/core/runtime-config/compilation)
- [Native AOT](https://learn.microsoft.com/dotnet/core/deploying/native-aot/)
- [GC configuration](https://learn.microsoft.com/dotnet/core/runtime-config/garbage-collector)
- [Vectorization: `System.Numerics.Vector`](https://learn.microsoft.com/dotnet/standard/simd)
