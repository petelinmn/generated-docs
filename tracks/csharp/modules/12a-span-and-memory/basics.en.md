---
title: "Span and Memory: basics"
lang: en
track: csharp
kind: module
doc: basics
slug: 12a-span-and-memory
part: 2
tags: [span, memory, readonly-span, stackalloc, ref-struct, slicing, zero-allocation]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-22
---

# Span and Memory: basics

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [02 · Types and variables](../02-types-and-variables/overview.en.md), [06 · Collections and generics](../06-collections-and-generics/overview.en.md), [12 · Memory and GC](../12-memory-and-gc/overview.en.md). **Time:** ~1 h.

## Why it matters

To process part of an array or a string, familiar code copies it: `Substring`, `Split`, `Array.Copy`. Every copy is a heap allocation, and so work for the [garbage collector](../12-memory-and-gc/overview.en.md). `Span<T>` is a **view** onto a contiguous piece of memory: it shows part of an array, a string or the stack without copying.

## `Span<T>` and `ReadOnlySpan<T>`

`Span<T>` holds a reference to the start of a region and its length. You can read and **modify** elements through it; `ReadOnlySpan<T>` only reads (a `string` converts implicitly to `ReadOnlySpan<char>`).

```csharp
int[] data = [1, 2, 3, 4, 5, 6];
Span<int> all = data;                 // a view of the whole array
Span<int> mid = all.Slice(1, 3);      // elements 2, 3, 4 — no copy
mid[0] = 99;                          // writes through to the array
Console.WriteLine(string.Join(",", data));   // 1,99,3,4,5,6
Console.WriteLine(mid.Length);               // 3
```

A slice (`Slice` or a range `[1..4]`) creates a new view onto the same memory. A change through `mid` is visible in the original array `data`.

### Parsing a string without copying

Instead of `Split` and `Substring`, use `ReadOnlySpan<char>` slices:

```csharp
string line = "42;17;8";
ReadOnlySpan<char> rest = line;
int sum = 0;
while (true)
{
    int i = rest.IndexOf(';');
    var part = i < 0 ? rest : rest[..i];   // no allocation
    sum += int.Parse(part);                // int.Parse accepts ReadOnlySpan<char>
    if (i < 0) break;
    rest = rest[(i + 1)..];
}
Console.WriteLine(sum);   // 67
```

Verified: `Substring` on a 10,000-character string allocates ≥ 10,000 bytes, while `AsSpan` allocates 0 bytes (`GC.GetAllocatedBytesForCurrentThread` before and after).

## Useful methods

The methods work the same for arrays, lists (via `CollectionsMarshal`) and stack memory:

```csharp
Span<int> a = [3, 1, 2];
a.Sort();                                    // 1,2,3
Console.WriteLine(a.IndexOf(2));             // 1
Console.WriteLine(a.SequenceEqual([1, 2, 3]));   // True
a.Reverse();                                 // 3,2,1
Span<int> dst = new int[5];
a.CopyTo(dst);                               // 3,2,1,0,0
Console.WriteLine(a.TryCopyTo(new int[1]));  // False: too small, no exception
```

`CopyTo` into a too-small destination throws `ArgumentException`, while `TryCopyTo` returns `false`. Going out of bounds gives `IndexOutOfRangeException` (the indexer) or `ArgumentOutOfRangeException` (`Slice`): verified.

## stackalloc: memory on the stack

`stackalloc` allocates an array on the call stack — without the GC and very fast, but the memory lives only until the method ends:

```csharp
Span<byte> buf = stackalloc byte[16];
buf.Fill(7);
Console.WriteLine(buf.Length + " " + buf[15]);   // 16 7
Span<int> sp = stackalloc int[] { 1, 2, 3 };
```

The stack is limited (on the order of a megabyte per thread, depending on the OS and settings), so `stackalloc` is suitable only for small buffers (hundreds of bytes), and the size must be bounded, for example: `Span<char> buf = n <= 256 ? stackalloc char[n] : new char[n];`.

## Span restrictions

`Span<T>` is a `ref struct`: a type that lives only on the stack. The compiler forbids everything that could move it to the heap (verified, error codes):

| Not allowed | Error |
|---|---|
| a field of a regular class or struct | `CS8345` |
| capture in a lambda or local function | `CS8175` |
| conversion to `object`, boxing | `CS0029` |
| keeping it across `await` or `yield` | `CS4007` |
| a generic argument (`List<Span<int>>`) | `CS9244` |
| returning `stackalloc` memory from a method | `CS8352` |

For storing in fields and using with `async` there is `Memory<T>`.

## `Memory<T>`: when you need to store it

`Memory<T>` is an ordinary struct: it can be stored in a field and passed to `async` methods and collections. To work with the data, take `.Span`:

```csharp
static async Task<int> ReadAsync(Memory<byte> m)
{
    await Task.Delay(1);       // Memory can cross an await, Span cannot
    m.Span.Fill(1);
    return m.Length;
}

var arr = new byte[8];
Console.WriteLine(await ReadAsync(arr.AsMemory(2, 4)));   // 4
Console.WriteLine(string.Join("", arr));                  // 00111100
```

Rule: in parameters of synchronous methods use `Span<T>`/`ReadOnlySpan<T>`, in `async` methods and fields use `Memory<T>`/`ReadOnlyMemory<T>`.

## Beginner mistakes

- ❌ `Substring`/`Split` in a hot parsing loop → ✅ `ReadOnlySpan<char>` slices.
- ❌ `stackalloc` with a size taken from user input → ✅ bound the size and fall back to an array.
- ❌ Storing a `Span<T>` in a field → ✅ store `Memory<T>` and take `.Span` at the point of use.
- ❌ Awaiting while a `Span<T>` is alive in an `async` method → ✅ move the `Span` work into a synchronous method.
- ❌ Using `Span` as an "accelerator" without measuring → ✅ first check that copying is the bottleneck.

## Related materials

- Next: [advanced theory](advanced.en.md) — `Memory<T>` and `ArrayPool`, `CollectionsMarshal`, `Pipelines`.
- [12 · Memory and GC](../12-memory-and-gc/overview.en.md) — heap, stack, allocations.
- [02 · Types and variables](../02-types-and-variables/overview.en.md) — structs and strings.
- [10 · Async/await](../10-async-await/overview.en.md) — why `Span` cannot be used across `await`.
- [11 · Modern C#](../11-modern-csharp/overview.en.md) — collection expressions for `Span<T>`.

## Sources

- [`Span<T>`](https://learn.microsoft.com/dotnet/api/system.span-1)
- [Usage guidelines for `Memory<T>` and `Span<T>`](https://learn.microsoft.com/dotnet/standard/memory-and-spans/memory-t-usage-guidelines)
- [Memory and spans](https://learn.microsoft.com/dotnet/standard/memory-and-spans/)
- [stackalloc](https://learn.microsoft.com/dotnet/csharp/language-reference/operators/stackalloc)
