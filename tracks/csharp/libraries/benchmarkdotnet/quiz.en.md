---
title: "Quiz: BenchmarkDotNet"
lang: en
track: csharp
kind: library
doc: quiz
slug: benchmarkdotnet
part: 2
tags: [benchmarkdotnet, benchmarking, performance]
status: draft
updated: 2026-11-22
---

# Quiz: BenchmarkDotNet

🌐 [Русский](quiz.ru.md) · **English**

> Answer first, then open the answer. The mark: 🟢 [basics](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 In which configuration are benchmarks run?

- A) Debug
- B) Release
- C) any
- D) only in Docker

<details><summary>Answer</summary>

**B.** In Debug BenchmarkDotNet refuses to work.
→ [Basics](basics.en.md#beginner-mistakes)

</details>

## 2. 🟢 What does the `Allocated` column show?

- A) the process's occupied RAM
- B) memory allocated per call of the benchmark
- C) a file size
- D) the number of threads

<details><summary>Answer</summary>

**B.** Enabled by `[MemoryDiagnoser]`; the number is deterministic.
→ [Basics](basics.en.md#how-to-read-the-table)

</details>

## 3. 🟢 What does `Ratio = 0.02` mean?

- A) the benchmark is 50 times faster than the baseline
- B) the benchmark is 50 times slower
- C) 2% errors
- D) 2 iterations

<details><summary>Answer</summary>

**A.** Verified: `Builder` at N = 1000 had `Ratio = 0.02` relative to `Concat`.
→ [Basics](basics.en.md#the-first-benchmark)

</details>

## 4. 🟢 How do you mark the baseline benchmark?

- A) `[Benchmark(Baseline = true)]`
- B) `[Baseline]`
- C) `[First]`
- D) you cannot

<details><summary>Answer</summary>

**A.** The others are compared to it in the `Ratio` column.
→ [Basics](basics.en.md#attributes)

</details>

## 5. 🟡 Where do you put data preparation?

- A) in the body of `[Benchmark]`
- B) in `[GlobalSetup]`
- C) in a static constructor
- D) in `Main`

<details><summary>Answer</summary>

**B.** It runs once, outside the measurement.
→ [Advanced](advanced.en.md#setup-and-parameters)

</details>

## 6. 🟡 What did the measurement of `Split` against counting by `Span` show?

- A) the same
- B) `Span` is about 6 times faster and with no allocations
- C) `Split` is faster
- D) `Span` allocates more

<details><summary>Answer</summary>

**B.** Verified: 6350 ns against 1042 ns, 39,944 B against 0 B.
→ [Advanced](advanced.en.md#setup-and-parameters)

</details>

## 7. 🟡 Select all correct statements

- [ ] A) absolute times are portable between machines
- [ ] B) a benchmark's return value protects from the JIT removing code
- [ ] C) `[Params]` creates a separate result row per value
- [ ] D) the BenchmarkDotNet version must "know" the runtime version

<details><summary>Answer</summary>

**B, C, D.** A is wrong: time depends on the machine. D: verified — 0.15.8 did not recognize .NET 11.
→ [Advanced](advanced.en.md#the-environment-from-this-materials-practice)

</details>

## 8. 🟡 Open question: how do you prove an optimization is worthwhile?

<details><summary>Key points of a good answer</summary>

- profile the application and find the hot method;
- BenchmarkDotNet: `Baseline`, `MemoryDiagnoser`, realistic `[Params]`;
- compare `Ratio` and `Allocated`, not absolute numbers;
- check the effect on the application/a load test.

</details>

## 9. 🔴 How does BenchmarkDotNet isolate benchmarks?

- A) with threads
- B) it generates a separate project and runs a separate process
- C) in the same process
- D) in a container

<details><summary>Answer</summary>

**B.** That is why the child build looks for the `.csproj` and the project directory's accessibility matters.
→ [Expert](expert.en.md#internals)

</details>

## 10. 🔴 An optimization gives 5% in a microbenchmark and the application did not speed up. Why?

- A) a BenchmarkDotNet bug
- B) the method is not hot; the bottleneck is elsewhere
- C) not enough iterations
- D) a Debug build

<details><summary>Answer</summary>

**B.** Profile the application first.
→ [Expert](expert.en.md#edges-and-gotchas)

</details>
