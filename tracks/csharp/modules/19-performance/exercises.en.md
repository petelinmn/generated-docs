---
title: "Exercises: Performance"
lang: en
track: csharp
kind: module
doc: exercises
slug: 19-performance
part: 3
tags: [performance, allocations, span, array-pool, linq, boxing, gc]
status: draft
updated: 2026-11-22
---

# Exercises: Performance

🌐 [Русский](exercises.ru.md) · **English**

> Solve it yourself before opening the hints. All exercises measure allocations with the `Alloc` helper from the [basics](basics.en.md#allocations-and-gc-pressure); run in **Release** (`dotnet run -c Release`). The numbers were verified on .NET 11. There are no runnable versions in `code/` yet (level 5 does not exist).

## 1. 🟢 Measure and compare string joining

**Task:** join 50 strings in three ways (`+=`, `StringBuilder`, `string.Concat`) and print the bytes allocated by each.

**Expected result (orders of magnitude):**

```text
string +=        7968 B
StringBuilder    1176 B
string.Concat     304 B
```

<details><summary>Hint</summary>

`Alloc(() => { var s = ""; foreach (var p in parts) s += p; })` and likewise for the others; `Alloc` calls the action twice to warm up.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 1: measure allocations of three ways to join 50 strings.
string[] parts = Enumerable.Range(0, 50).Select(i => "p" + i).ToArray();

Console.WriteLine(Alloc(() => { var s = ""; foreach (var p in parts) s += p; }));
Console.WriteLine(Alloc(() => { var sb = new StringBuilder(); foreach (var p in parts) sb.Append(p); _ = sb.ToString(); }));
Console.WriteLine(Alloc(() => { _ = string.Concat(parts); }));
```

Verified: `7968`, `1176`, `304`. Concatenation in a loop with `+=` creates 50 intermediate strings.

</details>

## 2. 🟢 Remove LINQ from a hot path

**Task:** replace `arr.Where(v => v > 2).Sum()` with a loop without allocations and confirm it by measurement.

**Expected result:**

```text
LINQ  → 48 B
loop  → 0 B
```

<details><summary>Hint</summary>

`int s = 0; foreach (var v in arr) if (v > 2) s += v;`.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 2: replace a LINQ chain with a loop in a hot path.
int[] arr = [1, 2, 3, 4, 5, 6, 7, 8];
Console.WriteLine(Alloc(() => { _ = arr.Where(v => v > 2).Sum(); }));                    // 48
Console.WriteLine(Alloc(() => { int s = 0; foreach (var v in arr) if (v > 2) s += v; })); // 0
```

Verified: `48` and `0`. Do not do this everywhere: the gain pays off only in hot spots.

</details>

## 3. 🟡 Get rid of boxing through `params`

**Task:** the method `static int P(params object[] xs) => xs.Length;` is called with three numbers. Show the allocation and rewrite it with `params ReadOnlySpan<int>`.

**Expected result:**

```text
P(1, 2, 3) with object[]           → 120 B
Q(1, 2, 3) with ReadOnlySpan<int>  → 0 B
```

<details><summary>Hint</summary>

C# 13 allows `params` for `ReadOnlySpan<T>`: `static int Q(params ReadOnlySpan<int> xs) => xs.Length;`.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 3: params ReadOnlySpan<T> avoids both the array and the boxing.
static int P(params object[] xs) => xs.Length;
static int Q(params ReadOnlySpan<int> xs) => xs.Length;

Console.WriteLine(Alloc(() => { P(1, 2, 3); }));   // 120
Console.WriteLine(Alloc(() => { Q(1, 2, 3); }));   // 0
```

Verified: `120` and `0`. The gain is double: no array and no boxing of three numbers.

</details>

## 4. 🟡 An enumerator without boxing

**Task:** measure `foreach` over a `List<int>` through a variable of type `List<int>` and through `IEnumerable<int>`; explain the difference.

**Expected result:**

```text
through List<int>        → 0 B
through IEnumerable<int> → 40 B
```

<details><summary>Hint</summary>

`List<T>.GetEnumerator()` returns a struct; when cast to the `IEnumerator<T>` interface it is boxed.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 4: the struct enumerator is boxed when the list is seen as IEnumerable<T>.
var list = new List<int> { 1, 2, 3 };
Console.WriteLine(Alloc(() => { int s = 0; foreach (var i in list) s += i; }));                       // 0
Console.WriteLine(Alloc(() => { int s = 0; foreach (var i in (IEnumerable<int>)list) s += i; }));     // 40
```

Verified: `0` and `40`. If a method must accept any collection, this is the price of generality; for hot paths accept `List<T>`/`T[]`/`ReadOnlySpan<T>`.

</details>

## 5. 🔴 A buffer benchmark

**Task:** write a BenchmarkDotNet class with three methods for getting a 4096-byte buffer: `new byte[4096]` (the baseline), `ArrayPool<byte>.Shared.Rent`/`Return`, `stackalloc`. Enable `[MemoryDiagnoser]`, run it in Release and compare.

**Expected result (the order, `ShortRun`, i5-12400):**

```text
NewArray    ~131 ns  4120 B
Pooled      ~7.6 ns     0 B
StackAlloc  ~21 ns      0 B
```

<details><summary>Hint</summary>

The method must return a value (for example `b.Length`) so that the JIT does not drop the code. For .NET 11 you need BenchmarkDotNet 0.16.0-preview.2 or newer. Keep the project in an ordinary directory.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 5: a BenchmarkDotNet comparison of three buffer strategies.
[MemoryDiagnoser]
public class Buffers
{
    [Benchmark(Baseline = true)]
    public int NewArray() { var b = new byte[4096]; b[0] = 1; return b.Length; }

    [Benchmark]
    public int Pooled()
    {
        var b = ArrayPool<byte>.Shared.Rent(4096);
        try { b[0] = 1; return b.Length; }
        finally { ArrayPool<byte>.Shared.Return(b); }
    }

    [Benchmark]
    public int StackAlloc() { Span<byte> b = stackalloc byte[4096]; b[0] = 1; return b.Length; }
}

BenchmarkRunner.Run<Buffers>();   // dotnet run -c Release
```

Verified (BenchmarkDotNet 0.16.0-preview.2): `NewArray` 131.6 ns / 4120 B, `Pooled` 7.6 ns / 0, `StackAlloc` 20.9 ns / 0. Time depends on the machine and mode, so the order matters: neither the pool nor the stack allocates on the heap; `stackalloc` zeroes 4 KB by default, while a pooled buffer is not zeroed.

</details>
