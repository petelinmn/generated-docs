---
title: "Exercises: BenchmarkDotNet"
lang: en
track: csharp
kind: library
doc: exercises
slug: benchmarkdotnet
part: 2
tags: [benchmarkdotnet, memory-diagnoser, globalsetup, params, baseline]
status: draft
updated: 2026-11-22
---

# Exercises: BenchmarkDotNet

🌐 [Русский](exercises.ru.md) · **English**

> Solve it yourself. The solutions were verified by running (BenchmarkDotNet 0.16.0-preview.2, .NET 11, a shortened `SimpleJob`; the project lay in an ordinary directory, not a temp folder). There are no runnable versions in `code/` (level 5 does not exist). Absolute times depend on the machine.

## 1. 🟢 The first benchmark

**Task:** compare string concatenation in a loop and `StringBuilder` at `N = 100` and `N = 1000` with `MemoryDiagnoser`.

**Expected result:** `StringBuilder` is faster (≈5× at 100, ≈65× at 1000) and allocates orders of magnitude less.

<details><summary>Hint</summary>

`[MemoryDiagnoser]`, `[Params(100, 1000)]`, `[Benchmark(Baseline = true)]` for the concatenation; `dotnet run -c Release`.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 1: string concatenation vs StringBuilder.
[MemoryDiagnoser]
[SimpleJob(warmupCount: 2, iterationCount: 5, launchCount: 1)]
public class Bench
{
    [Params(100, 1000)] public int N;
    [Benchmark(Baseline = true)] public string Concat() { var s = ""; for (var i = 0; i < N; i++) s += "x"; return s; }
    [Benchmark] public string Builder() { var sb = new StringBuilder(); for (var i = 0; i < N; i++) sb.Append('x'); return sb.ToString(); }
}
```

Verified: `N=100` — 904.1 ns / 12,576 B against 178.1 ns / 768 B (`Ratio 0.20`); `N=1000` — 48,979 ns / 1,025,976 B against 752.2 ns / 4,576 B (`Ratio 0.02`).

</details>

## 2. 🟡 Data preparation

**Task:** compare `Split(',').Length` and counting commas through `Span` for a string of 1000 numbers. Prepare the string once.

**Expected result:** `Span` ≈ 6 times faster and with no allocations.

<details><summary>Hint</summary>

`[GlobalSetup]` creates the string; in `SpanCount` — a loop over `_text.AsSpan()`.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 2: setup outside the measurement; Span vs Split.
private string _text = "";
[GlobalSetup] public void Setup() => _text = string.Join(",", Enumerable.Range(0, 1000));

[Benchmark(Baseline = true)] public int SplitCount() => _text.Split(',').Length;
[Benchmark] public int SpanCount() { var n = 1; foreach (var c in _text.AsSpan()) if (c == ',') n++; return n; }
```

Verified: `SplitCount` — 6350.5 ns / 39,944 B; `SpanCount` — 1042.0 ns / 0 B (`Ratio 0.16`).

</details>

## 3. 🟡 Reading the report

**Task:** from the table of exercise 1 answer: which column is the most reliable for comparing between machines? What does `Error` say for `Concat` at `N=1000`?

**Expected result:** `Allocated` (deterministic) and `Ratio`; `Error 4950 ns` at `Mean 48,979 ns` — a wide interval because of the small number of iterations.

<details><summary>Hint</summary>

`Error` is half of the 99.9% confidence interval; in the check `iterationCount = 5`.

</details>

<details><summary>Solution</summary>

Time depends on the machine and load, so to transfer between machines compare **`Ratio`** and **`Allocated`**. A wide `Error` (≈10% of `Mean`) means too few iterations: for reliable measurements do not shorten `SimpleJob`.

Verified from the report output: `Concat, N=1000: Mean 48,979.0 ns, Error 4950.35 ns, StdDev 1285.59 ns`; the report also marked outliers (`1 outlier was removed`).

</details>

## 4. 🔴 Environment traps

**Task:** what happens on a run in Debug? Which BenchmarkDotNet version should you take for .NET 11 and why does the project's location matter?

**Expected result:** Debug — a refusal "Benchmark was built without optimization enabled"; 0.15.8 did not recognize .NET 11, 0.16.0-preview.2 works; from a deep temp folder the child build did not find the `.csproj`.

<details><summary>Hint</summary>

Remember that BenchmarkDotNet generates and builds a separate project for a benchmark.

</details>

<details><summary>Solution</summary>

- Run `dotnet run -c Release`. In Debug — a refusal with the message above (verified).
- Version **0.15.8** on .NET 11 gave `NotImplementedException: GetRuntimeVersion … NotRecognized`; **0.16.0-preview.2** ran.
- The child build looks for the `.csproj` up/down from the directory; in a deep temp folder — "Unable to find … and its subfolders". Keep the project in an ordinary directory.

Verified in this environment: all three situations were reproduced.

</details>
