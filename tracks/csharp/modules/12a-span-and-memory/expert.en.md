---
title: "Span and Memory: expert"
lang: en
track: csharp
kind: module
doc: expert
slug: 12a-span-and-memory
part: 2
tags: [span, memory, ref-struct, interior-pointer, scoped, allows-ref-struct, memory-marshal, interview-questions]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-22
---

# Span and Memory: expert

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · **3. Expert** · [4. Quiz](quiz.en.md)

**Prerequisites:** [basic](basics.en.md) and [advanced](advanced.en.md) theory. **Time:** ~1.5 h.

## How Span is built

`Span<T>` is a struct of two fields: a **managed reference** (`ref T`) to the first element and a length (`int`). Verified: `Unsafe.SizeOf<Span<int>>()` and `Unsafe.SizeOf<ReadOnlySpan<char>>()` are 16 bytes each (8 + 4 + padding). That is why a `Span` is cheap to pass by value.

The reference inside a `Span<T>` is an **interior pointer**: it can point not only to the start of an array but to an element in the middle of an object, a class field or stack memory. The garbage collector can track such references and updates them when heap objects move, and the object a `Span` points into stays alive. Verified: `MemoryMarshal.CreateSpan(ref box.X, 1)` gives a `Span<int>` over an object's field, and writing to it changes `box.X`.

`Memory<T>` is an ordinary struct: a reference to an owner object (an array, a `string` or a `MemoryManager<T>`), an offset and a length (also 16 bytes). An ordinary reference plus an offset allows storage on the heap, but to read elements a `Memory<T>` is turned into a `Span<T>` each time.

### Why Span has to be a ref struct

An interior pointer cannot be stored on the heap: the GC cannot track such references from heap objects, and a struct holding a reference to stack memory would outlive its frame. Hence the `ref struct` restrictions:

| Not allowed | Error (verified) |
|---|---|
| being a field of a class / a regular struct | `CS8345` |
| getting into a closure | `CS8175` |
| boxing into `object` | `CS0029` |
| living across `await`/`yield` | `CS4007` |
| a generic argument without `allows ref struct` | `CS9244` |
| returning a `Span` over `stackalloc` memory from a method | `CS8352` |
| calling `Equals(object)` | does not compile (`CS1503`): use `SequenceEqual` |

What was relaxed in newer C# versions:

- **`scoped`** (C# 11) — a parameter or local does not "escape" the method: `static int Sum(scoped Span<int> s)` accepts even `stackalloc` without lifetime analysis (verified: `Sum(stackalloc int[] {1, 2, 3})` = 6);
- **`allows ref struct`** (C# 13) — a generic allows a `ref struct`: `static void Print<T>(T t) where T : IHasN, allows ref struct`; a ref struct can also implement interfaces (verified on `ref struct Counter : IHasN`);
- in **iterators** and `async` methods a `Span` may be used as long as it does not cross an `await`/`yield`: error `CS4007` appears exactly when it crosses.

A `readonly ref struct` guarantees immutable fields: the compiler makes no defensive copies when passing it as `in`. `Span<T>` and `ReadOnlySpan<T>` are declared this way.

## Equality and `ToString`

- The `==` operator of `Span<T>` compares the **reference and length**, not the contents: two `Span`s over the same array are equal, over different arrays with the same data — not (verified: `True` and `False`). To compare contents use `SequenceEqual`.
- `ToString()` of `Span<char>`/`ReadOnlySpan<char>` returns a string with the contents (`"abc"`), for others — a type description: `System.Span<Int32>[3]` (verified).
- An empty `default` and `Span<T>.Empty` are equal; `Span<int> s = (int[]?)null` gives an empty span (verified).

## How the JIT works with Span

Access `span[i]` contains a bounds check. The JIT removes it where it can prove the index is valid (for example, in the loop `for (int i = 0; i < span.Length; i++)`), so a loop over a `Span` is comparable to a loop over an array. Methods such as `IndexOf`, `SequenceEqual`, `Contains` and `SearchValues` use SIMD instructions (`Vector128/256/512`) — a gain on long data. Specific numbers depend on the processor and version — measure with BenchmarkDotNet (module "19 · Performance" is not created yet).

`stackalloc` zeroes memory by default; the `[SkipLocalsInit]` attribute and a compiler option turn zeroing off for speed — then you must not read what you did not write.

## Memory without copying: what else exists

| Type / API | Purpose |
|---|---|
| `ReadOnlySequence<T>` | a sequence of several segments; `Slice`, `PositionOf`, `GetPosition` |
| `MemoryPool<T>`, `IMemoryOwner<T>` | a pool with an owner (verified: `Rent(10).Memory.Length >= 10`) |
| `MemoryManager<T>` | a base class for custom memory owners (native, pinned) |
| `Memory<T>.Pin()` | pin memory to pass to native code |
| `MemoryMarshal.Cast`, `CreateSpan`, `GetReference` | low-level casting and access by reference |
| `Unsafe` | `As`, `Add`, `SizeOf`, `ReadUnaligned` — without checks |
| `string.Create` | create a string by filling its buffer through a `Span<char>` — one allocation |
| `Convert.ToHexStringLower` (.NET 9+) | a ready replacement for a hand-written hex string (verified: `000fff`) |

## Gotchas

**1. A `Span` over a `List<T>` goes stale.** After an `Add` that reallocates, the old `Span` writes to an abandoned array (verified in the [advanced theory](advanced.en.md#listt-and-collectionsmarshal)).

**2. Array covariance.** Creating a `Span<object>` over a `string[]` throws `ArrayTypeMismatchException`: `Span<T>` does not allow array covariance so that a write cannot break type safety. For reading, a `ReadOnlySpan<object>` is enough — it is created without an error (verified).

**3. `Rent` returns a longer array.** Work with the requested length via `AsSpan(0, n)`, not with `rented.Length`.

**4. `Span` and `async`.** Any local `Span` variable in an `async` method that is used after an `await` is error `CS4007`. In a top-level-statements program this shows up unexpectedly: a `Span` declared before an `await` cannot be used after it.

**5. `stackalloc` in a loop.** Each iteration adds stack memory, which is released only on leaving the method. Allocate the buffer before the loop.

**6. `MemoryMarshal.Cast` and alignment.** Casting a `Span<byte>` to `Span<int>` requires the length to be divisible by the type size (extra bytes are dropped) and depends on the platform's byte order.

**7. Micro-optimization without measurement.** Only one thing is verified: `AsSpan` does not allocate while `Substring` does; the time gain must be measured.

## Trade-offs and alternatives

| Approach | Pros | Cons |
|---|---|---|
| `Substring`/`Split`/copying | simple, safe | allocations, GC load |
| `ReadOnlySpan<char>` and slices | no allocations | `ref struct` restrictions, not in `async`/fields |
| `Memory<T>` | storable in fields, works with `async` | `.Span` needed for access |
| `ArrayPool<T>` | reuse of large buffers | must be returned, leaks/races possible |
| `System.IO.Pipelines` | networking, stream parsing, buffer management | more complex than `Stream`, not for everyone |
| `unsafe` pointers | maximum control | no safety checks |

## Interview questions

<details><summary>1. What is `Span<T>` and what problem does it solve?</summary>

A view onto a contiguous region of memory (an array, a string, the stack, native memory) without copying. It allows slicing and parsing without heap allocations.

</details>

<details><summary>2. Why is `Span<T>` a `ref struct`?</summary>

It contains an interior managed pointer, which cannot live on the heap: it could outlive the stack memory it points to. So it exists only on the stack — hence the ban on class fields, closures, boxing and `await`.

</details>

<details><summary>3. How does `Memory<T>` differ from `Span<T>`?</summary>

`Memory<T>` is an ordinary struct: it can be stored in a field, used in `async` and collections; data access is through `.Span`. `Span<T>` is faster and safer on the stack but has restrictions.

</details>

<details><summary>4. Why can't you use a `Span<T>` after `await`?</summary>

The method becomes a state machine, and locals that survive an `await` are stored in its fields (on the heap). A `ref struct` cannot be stored on the heap — error `CS4007`.

</details>

<details><summary>5. What does `stackalloc` do and why is it dangerous?</summary>

It allocates memory on the call stack (no GC, until the method exits). Dangers: stack overflow with large sizes (fatal for the process) and accumulation in loops. The size must be bounded with a fallback to an array.

</details>

<details><summary>6. How do you parse a string without `Substring`/`Split`?</summary>

Through `ReadOnlySpan<char>`: `IndexOf` + slices, `int.Parse(ReadOnlySpan<char>)`, `MemoryExtensions.Split` (ranges), `EnumerateLines()`.

</details>

<details><summary>7. What does `==` do for `Span<T>`?</summary>

It compares the reference to the start and the length, not the contents. For contents use `SequenceEqual`.

</details>

<details><summary>8. What is `ArrayPool<T>` and what are its pitfalls?</summary>

A pool of reusable arrays: `Rent`/`Return`. The returned array may be longer than requested and is not zeroed; you must not use the array after `Return` or return it twice; return it in `finally`.

</details>

<details><summary>9. Why is `CollectionsMarshal.AsSpan(list)` dangerous?</summary>

It hands out the list's internal array. If the list reallocates the array (`Add` beyond capacity), the `Span` keeps pointing to the old array, and writes to it are lost (verified).

</details>

<details><summary>10. What is `System.IO.Pipelines` and what is the point of `AdvanceTo(consumed, examined)`?</summary>

A `Pipe` connects a producer and a consumer through a shared managed buffer. `consumed` is what was processed and can be released, `examined` is what was looked at; unconsumed data stays in the buffer and the consumer receives it together with new data. This is how messages split between blocks are handled.

</details>

<details><summary>11. What do `scoped` and `allows ref struct` give?</summary>

`scoped` tells the compiler that a reference does not leave the method (so `stackalloc` data can be passed). `allows ref struct` permits a `ref struct` as a generic argument.

</details>

<details><summary>12. Trick question: what happens with `Span<object> s = new string[1];`?</summary>

`ArrayTypeMismatchException`: `Span<T>` does not allow array covariance (verified). With `ReadOnlySpan<object>` the same assignment is allowed.

</details>

<details><summary>13. Trick question: what does `ToString()` return for a `Span<int>` with the elements `1, 2, 3`?</summary>

Not the numbers: `ToString()` returns the type description `System.Span<Int32>[3]`, and only for `Span<char>`/`ReadOnlySpan<char>` the contents as a string (verified).

</details>

<details><summary>14. Trick question: is `Span` always faster than an array?</summary>

No. A loop over a `Span` is comparable to a loop over an array: the gain comes from the absence of copying and allocation. If copying is not the bottleneck there will be no noticeable difference; the decision is made by measurement.

</details>

## Related materials

- Back: [advanced theory](advanced.en.md). Check yourself: [quiz](quiz.en.md) and [exercises](exercises.en.md).
- [12 · Memory and GC](../12-memory-and-gc/overview.en.md) — the GC and pinning memory.
- [10 · Async/await](../10-async-await/overview.en.md) — the state machine and `ref struct` restrictions.
- [11 · Modern C#](../11-modern-csharp/overview.en.md) — collection expressions, `params ReadOnlySpan<T>`, `allows ref struct`.
- [06 · Collections and generics](../06-collections-and-generics/overview.en.md) — generic constraints.

## Sources

- [Usage guidelines for `Memory<T>` and `Span<T>`](https://learn.microsoft.com/dotnet/standard/memory-and-spans/memory-t-usage-guidelines)
- [`ref struct`](https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/ref-struct)
- [`allows ref struct`](https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/where-generic-type-constraint)
- [`MemoryMarshal`](https://learn.microsoft.com/dotnet/api/system.runtime.interopservices.memorymarshal)
- [`ReadOnlySequence<T>`](https://learn.microsoft.com/dotnet/api/system.buffers.readonlysequence-1)
