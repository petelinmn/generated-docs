---
title: "Quiz: Span and Memory"
lang: en
track: csharp
kind: module
doc: quiz
slug: 12a-span-and-memory
part: 2
tags: [span, memory, stackalloc, ref-struct, array-pool, pipelines]
status: draft
updated: 2026-11-22
---

# Quiz: Span and Memory

🌐 [Русский](quiz.ru.md) · **English**

> Answer first, then open the answer. The mark shows which level a question comes from: 🟢 [basics](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 What is `Span<T>`?

- A) a copy of a region of an array
- B) a view onto a region of memory without copying: a reference and a length
- C) a collection with automatic growth
- D) a thread-safe buffer

<details><summary>Answer</summary>

**B.** `Span<T>` neither owns memory nor copies it: it points to part of an array, a string or the stack. A describes `ToArray()`/`Substring`. C is `List<T>`. D is wrong: thread safety is not guaranteed.
→ [Basics](basics.en.md#spant-and-readonlyspant)

</details>

## 2. 🟢 What does the code print?

```csharp
int[] data = [1, 2, 3, 4, 5, 6];
Span<int> mid = data.AsSpan().Slice(1, 3);
mid[0] = 99;
Console.WriteLine(string.Join(",", data));
```

<details><summary>Answer</summary>

```text
1,99,3,4,5,6
```

`Slice` creates a new view onto the same array, so writing `mid[0]` changes `data[1]`.
→ [Basics](basics.en.md#spant-and-readonlyspant)

</details>

## 3. 🟢 What does the call `big.AsSpan(100, 5000)` do compared with `big.Substring(100, 5000)`?

- A) both operations allocate 10,000 bytes
- B) `AsSpan` allocates no heap memory, `Substring` creates a new string
- C) `AsSpan` creates a copy, `Substring` does not
- D) there is no difference

<details><summary>Answer</summary>

**B.** Verified through `GC.GetAllocatedBytesForCurrentThread`: `Substring` allocated ≥ 10,000 bytes (5000 characters of 2 bytes), `AsSpan` — 0 bytes.
→ [Basics](basics.en.md#parsing-a-string-without-copying)

</details>

## 4. 🟢 Select all correct statements

- [ ] A) a `Span<T>` can be stored in a field of an ordinary class
- [ ] B) a `Memory<T>` can be passed to an `async` method
- [ ] C) `stackalloc` allocates memory on the heap
- [ ] D) `TryCopyTo` returns `false` if the destination is too small

<details><summary>Answer</summary>

**B, D.** A is wrong: a `ref struct` in a class field is error `CS8345`. B is right — that is what `Memory<T>` exists for. C is wrong: `stackalloc` takes memory from the stack, so it is released when the method exits. D is right: `CopyTo` would throw `ArgumentException`, `TryCopyTo` returns `false` (verified).
→ [Basics](basics.en.md#useful-methods)

</details>

## 5. 🟢 Why does this code not compile?

```csharp
static async Task Run()
{
    Span<int> s = new int[2];
    await Task.Delay(1);
    Console.WriteLine(s.Length);
}
```

- A) a `Span<T>` cannot be created from an array
- B) a `Span<T>` cannot be used after `await` — error `CS4007`
- C) `Task.Delay` does not accept values
- D) `ConfigureAwait(false)` is needed

<details><summary>Answer</summary>

**B.** Locals that survive an `await` live in a field of the state machine on the heap; a `ref struct` cannot get there. The solution is `Memory<int>` or moving the `Span` work into a synchronous method.
→ [Basics](basics.en.md#span-restrictions)

</details>

## 6. 🟡 What does the code print?

```csharp
var list = new List<int>(3) { 1, 2, 3 };
var ls = CollectionsMarshal.AsSpan(list);
list.Add(4);
ls[1] = 200;
Console.WriteLine(list[1]);
```

- A) `200`
- B) `2`
- C) `4`
- D) an exception

<details><summary>Answer</summary>

**B.** The list's capacity is 3; adding a fourth element reallocates the internal array, and `ls` still points to the old one. The write of `200` went to the abandoned array (verified: `list[1] = 2, span[1] = 200`). Do not modify the list while such a `Span` is alive.
→ [Advanced theory](advanced.en.md#listt-and-collectionsmarshal)

</details>

## 7. 🟡 What is true of `ArrayPool<byte>.Shared.Rent(100)`?

- A) the array has exactly 100 elements
- B) the array has at least 100 elements and may contain old data
- C) the array is always zeroed
- D) after `Return` the array can still be used

<details><summary>Answer</summary>

**B.** The pool returns an array of a suitable size (≥ requested), and the content is not cleared. Work through `AsSpan(0, 100)`. After `Return` the array belongs to the pool — using it corrupts someone else's data.
→ [Advanced theory](advanced.en.md#arraypool-reusing-arrays)

</details>

## 8. 🟡 Select all correct statements

- [ ] A) `"héllo"u8` gives a `ReadOnlySpan<byte>` of 6 bytes
- [ ] B) `SearchValues<char>` is created once and reused
- [ ] C) `MemoryMarshal.Cast<byte, int>` copies the data
- [ ] D) `Split` on a `ReadOnlySpan<char>` (.NET 9+) returns ranges, not strings

<details><summary>Answer</summary>

**A, B, D.** A: "é" is 2 bytes in UTF-8, 6 in total (verified). B: preparing the set is expensive, searching is fast. C is wrong: the cast reinterprets the same memory. D: `csv[range]` gives a slice.
→ [Advanced theory](advanced.en.md#splitting-and-strings)

</details>

## 9. 🟡 What does `reader.AdvanceTo(buffer.Start, buffer.End)` do in a `Pipelines` consumer if an incomplete line remains in the buffer?

- A) releases the whole buffer, the incomplete line is lost
- B) releases the lines already processed (everything before `buffer.Start`), keeps the incomplete remainder in the buffer and marks it as examined — the next `ReadAsync` returns it together with new data
- C) blocks the producer forever
- D) throws an exception

<details><summary>Answer</summary>

**B.** By the time of the call, `buffer` has already been sliced past the processed lines, so `consumed = buffer.Start` releases them. `examined = buffer.End` says that the remainder was looked at, and `ReadAsync` will not return until new data arrives. This is how lines split between blocks are joined (in the verified example "hel" + "lo" → `hello`).
→ [Advanced theory](advanced.en.md#systemiopipelines)

</details>

## 10. 🟡 Open question: how do you parse the string `"42;17;8"` and sum the numbers without heap allocations? What can and cannot be stored between iterations?

<details><summary>Key points of a good answer</summary>

- `ReadOnlySpan<char> rest = line;` and a loop: `IndexOf(';')`, a slice up to the separator, `int.Parse(span)`;
- then `rest = rest[(i + 1)..]` — moving to the remainder without copying;
- alternatives: `MemoryExtensions.Split` (ranges) or `EnumerateLines()` for lines;
- slices can be kept between loop iterations (locals), but not in class fields, across `await` or in lambdas;
- the result (a number) is a value, so there are no allocations.

</details>

## 11. 🔴 What will happen?

```csharp
object[] objs = new string[1];
Span<object> s = objs;
```

- A) it compiles and works
- B) `ArrayTypeMismatchException` on assignment
- C) a compile error
- D) `InvalidCastException`

<details><summary>Answer</summary>

**B.** Arrays are covariant: a `string[]` can be assigned to `object[]`, but a `Span<object>` over such an array would allow writing a non-string into a string array. So the constructor checks the exact array type and throws `ArrayTypeMismatchException` (verified). A `ReadOnlySpan<object>` is created without an error because writes are impossible.
→ [Expert theory](expert.en.md#gotchas)

</details>

## 12. 🔴 Find the bug

```csharp
static Span<int> Make()
{
    Span<int> s = stackalloc int[4];
    return s;
}
```

<details><summary>Answer</summary>

Error `CS8352`: the returned `Span` points to the method's stack memory, which will be released on exit. Solutions: return an array (`new int[4]`) or accept the buffer as a parameter (`static void Fill(Span<int> buffer)`) so that the caller owns the memory.
→ [Expert theory](expert.en.md#why-span-has-to-be-a-ref-struct)

</details>
