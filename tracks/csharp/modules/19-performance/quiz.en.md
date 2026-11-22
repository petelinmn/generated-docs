---
title: "Quiz: Performance"
lang: en
track: csharp
kind: module
doc: quiz
slug: 19-performance
part: 3
tags: [performance, allocations, benchmarkdotnet, native-aot, gc-pressure]
status: draft
updated: 2026-11-22
---

# Quiz: Performance

🌐 [Русский](quiz.ru.md) · **English**

> Answer first, then open the answer. The mark shows which level a question comes from: 🟢 [basics](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 Where do you start optimizing?

- A) rewriting with `Span` and `stackalloc`
- B) measuring: find the bottleneck, then optimize and measure again
- C) giving up LINQ everywhere
- D) enabling Server GC

<details><summary>Answer</summary>

**B.** Optimization without measurement complicates code without a guaranteed benefit; the "bottleneck" guessed by eye is almost always wrong.
→ [Basics](basics.en.md#why-it-matters)

</details>

## 2. 🟢 In which configuration is performance measured?

- A) Debug
- B) Release
- C) any: no difference
- D) only Native AOT

<details><summary>Answer</summary>

**B.** In Debug the code is not optimized; BenchmarkDotNet even refuses to work ("Benchmark was built without optimization enabled", verified).
→ [Basics](basics.en.md#what-to-measure)

</details>

## 3. 🟢 How many bytes does `object o = n;` allocate for an `int n`?

- A) 0
- B) 24
- C) 4
- D) 80

<details><summary>Answer</summary>

**B.** Boxing a value type allocates a heap object: 24 bytes on a 64-bit platform (verified through `GC.GetAllocatedBytesForCurrentThread`).
→ [Basics](basics.en.md#allocations-and-gc-pressure)

</details>

## 4. 🟢 What does `Alloc` return for a `static` lambda and for a lambda that captures a local variable?

- A) 0 and 80 bytes
- B) 80 and 0 bytes
- C) 0 and 0
- D) 24 and 24

<details><summary>Answer</summary>

**A.** A `static` lambda captures nothing and is cached (0 bytes); capturing creates a closure object (80 bytes, verified).
→ [Basics](basics.en.md#allocations-and-gc-pressure)

</details>

## 5. 🟡 Which way of joining 50 strings in a loop allocates the least?

- A) `s += p`
- B) `StringBuilder`
- C) `string.Concat(parts)` over an array
- D) all the same

<details><summary>Answer</summary>

**C.** Verified: `+=` — 7968 bytes, `StringBuilder` — 1176, `string.Concat(array)` — 304. If the pieces are already in an array, `Concat`/`Join` is the cheapest.
→ [Basics](basics.en.md#simple-techniques)

</details>

## 6. 🟡 What does the `Allocated` column in BenchmarkDotNet show and why is it more reliable than time?

- A) the average call time
- B) memory allocated per operation; the number is deterministic and does not depend on machine noise
- C) the number of threads
- D) the assembly size

<details><summary>Answer</summary>

**B.** Time depends on the processor, load and version, while allocations are reproducible. Compare ratios and `Allocated`.
→ [Advanced theory](advanced.en.md#benchmarkdotnet)

</details>

## 7. 🟡 Select all correct statements

- [ ] A) `ArrayPool<byte>.Shared.Rent(4096)` returned an array that may be longer and is not zeroed
- [ ] B) `stackalloc` suits megabyte buffers
- [ ] C) a `foreach` loop over a `Span<int>` was not faster than over an `int[]` in the verified measurement
- [ ] D) `[MemoryDiagnoser]` adds allocations and collections to the report

<details><summary>Answer</summary>

**A, C, D.** B is wrong: the stack is limited, `stackalloc` is for small buffers. C: both loops gave 254 ns (the JIT removes bounds checks).
→ [Advanced theory](advanced.en.md#pools-and-stack-memory)

</details>

## 8. 🟡 `foreach` over a `List<int>` through a variable of type `IEnumerable<int>` allocates memory, but through `List<int>` it does not. Why?

- A) the compiler is wrong
- B) through the interface a boxed enumerator is used (40 bytes), while `List<T>` has a struct one
- C) `IEnumerable<int>` always copies the list
- D) it depends on the C# version

<details><summary>Answer</summary>

**B.** The struct `List<T>.Enumerator` is boxed when cast to an interface (verified: 40 versus 0 bytes). Accept a concrete type or `IReadOnlyList<T>` where it matters, or use `Span`.
→ [Expert theory](expert.en.md#gotchas)

</details>

## 9. 🟡 Open question: how will you prove that a new implementation is really better than the old one?

<details><summary>Key points of a good answer</summary>

- a benchmark of both versions in Release (BenchmarkDotNet): `Baseline`, `MemoryDiagnoser`, realistic input data (`[Params]`);
- comparing ratios and allocations, not absolute numbers;
- an application profile shows that the spot really is hot;
- verification on the real workload/metrics after the change and regression protection (a benchmark in CI);
- accounting for the cost: readability and risk.

</details>

## 10. 🔴 What does Native AOT give and what does it limit?

- A) only speeds up the JIT
- B) ahead-of-time compilation without a JIT: fast start, small size, no installed runtime; it limits dynamic code generation and reflection
- C) works only on Linux
- D) replaces the garbage collector

<details><summary>Answer</summary>

**B.** Verified: "hello" was built into a self-contained `aot.exe` of about 900 KB, `RuntimeFeature.IsDynamicCodeSupported` is `False`. Reflection and dynamic code need generators and annotations.
→ [Expert theory](expert.en.md#native-aot)

</details>

## 11. 🔴 A benchmark showed a hundredfold speedup. Which two causes should you check first?

- A) hardware and the weather
- B) the result is unused (the JIT dropped the code) and the input is constant (computed at compile time)
- C) the Windows version
- D) the console font size

<details><summary>Answer</summary>

**B.** Dead-code elimination and constant folding are classic pitfalls: return the result from the method and take data from fields/`GlobalSetup`.
→ [Expert theory](expert.en.md#micro-benchmark-pitfalls)

</details>
