---
title: "Span and Memory"
lang: en
track: csharp
kind: module
doc: overview
slug: 12a-span-and-memory
part: 2
levels: [1, 2, 3, 4]
tags: [span, memory, stackalloc, ref-struct, array-pool, pipelines, zero-allocation]
versions: ".NET 11, C# 15; System.IO.Pipelines 11.0.0"
status: draft
updated: 2026-11-22
---

# Span and Memory

🌐 [Русский](overview.ru.md) · **English**

## In a nutshell

`Span<T>` and `ReadOnlySpan<T>` are views onto a contiguous region of memory (an array, a string, the stack, native memory): a reference plus a length, with no copying. They let you parse strings and process buffers without heap allocations and without GC load. The limitation is `ref struct`: a `Span` lives only on the stack; for fields and `async` there is `Memory<T>`. `ArrayPool<T>`, `System.IO.Pipelines` and fast parsing inside the .NET library itself are built on these types.

## Key ideas

- **Span does not copy:** `Slice` and ranges create a new view onto the same memory; a write is visible in the original array.
- **`ReadOnlySpan<char>`** replaces `Substring` and `Split` when parsing: `IndexOf`, slices, `int.Parse(span)`, `Split` by ranges.
- **`stackalloc`** gives a stack buffer without the GC; the size must be bounded.
- **`Span<T>` is a `ref struct`:** not in a class field, a lambda, across `await`, or as a generic argument (without `allows ref struct`).
- **`Memory<T>`** is stored in fields and crosses `async`; `.Span` gives fast access.
- **`ArrayPool<T>`** reuses large arrays: `Rent` returns an array ≥ requested, `Return` goes in `finally`.
- **`System.IO.Pipelines`** joins data split across blocks and manages buffers itself.
- **Careful:** a `Span` over a `List<T>` goes stale after reallocation; `==` compares the reference and length.

## Minimal example

```csharp
string line = "42;17;8";
ReadOnlySpan<char> rest = line;
int sum = 0;
while (true)
{
    int i = rest.IndexOf(';');
    var part = i < 0 ? rest : rest[..i];   // a slice, no allocation
    sum += int.Parse(part);
    if (i < 0) break;
    rest = rest[(i + 1)..];
}
Console.WriteLine(sum);   // 67
```

## When to use and when not

- ✅ Hot parsing and formatting paths, network protocols, buffers.
- ✅ Methods that accept "part of an array or string": `ReadOnlySpan<T>` instead of copies.
- ✅ Small temporary buffers — `stackalloc`; large ones — `ArrayPool<T>`.
- ❌ Premature optimization: measure first, then use `Span`.
- ❌ Storing a `Span<T>` between calls or across `await` — use `Memory<T>`.
- ❌ `stackalloc` with a size from external input.

## Levels of this material

| Level | What's inside |
|---|---|
| [1. Basics](basics.en.md) | why Span, `Span<T>`/`ReadOnlySpan<T>`, slices, string parsing, `stackalloc`, `ref struct` restrictions, `Memory<T>` |
| [2. Advanced](advanced.en.md) | choosing a type, `Split`/`EnumerateLines`/`SearchValues`/UTF-8, `ArrayPool`, `CollectionsMarshal`, `MemoryMarshal`, `System.IO.Pipelines` |
| [3. Expert](expert.en.md) | internals and interior pointers, `ref struct` rules, `scoped` and `allows ref struct`, equality, gotchas, 14 interview questions |
| [4. Self-check](quiz.en.md) | 12 questions and [5 exercises](exercises.en.md) with solutions |
| 5. Runnable examples | not generated yet — `/learn csharp 12a-span-and-memory level 5` |
| 6. Tests | not generated yet — `/learn csharp 12a-span-and-memory level 6` |

## Related materials

- Prerequisites: [12 · Memory and GC](../12-memory-and-gc/overview.en.md), [06 · Collections and generics](../06-collections-and-generics/overview.en.md)
- Nearby: [10 · Async/await](../10-async-await/overview.en.md) (`Memory<T>` and `await`), [11 · Modern C#](../11-modern-csharp/overview.en.md) (collection expressions)
- Next: [18 · Concurrency](../18-concurrency/overview.en.md)
- The same idea in other tracks: [Python](../../../python/overview.en.md) — `memoryview` and `bytes`/`bytearray`
