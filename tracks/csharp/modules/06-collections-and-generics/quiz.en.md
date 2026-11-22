---
title: "Quiz: Collections and generics"
lang: en
track: csharp
kind: module
doc: quiz
slug: 06-collections-and-generics
part: 1
tags: [list, dictionary, hashset, generics, variance, collections]
status: draft
updated: 2026-11-22
---

# Quiz: Collections and generics

🌐 [Русский](quiz.ru.md) · **English**

> Answer first, then open the answer. The mark shows which level a question comes from: 🟢 [basics](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 Which collection suits quickly checking "have we seen this element" and storing only unique values?

- A) `List<T>`
- B) `HashSet<T>`
- C) `Queue<T>`
- D) an array

<details><summary>Answer</summary>

**B.** `HashSet<T>` stores unique elements, and `Add` and `Contains` are O(1) on average. In a `List<T>` the search is linear, `Queue<T>` is a FIFO queue, an array has a fixed size.
→ [Basics](basics.en.md#hashsett-queuet-stackt)

</details>

## 2. 🟢 What is safer when reading from a dictionary if the key may be missing?

- A) `dict[key]`
- B) `dict.TryGetValue(key, out var value)`
- C) `dict.Keys[key]`
- D) `(string)dict[key]`

<details><summary>Answer</summary>

**B.** The indexer throws `KeyNotFoundException` for a missing key, while `TryGetValue` returns `false` without an exception and without a second lookup.
→ [Basics](basics.en.md#dictionarytkey-tvalue)

</details>

## 3. 🟢 Select all correct statements

- [ ] A) `Queue<T>` — first in, first out (FIFO)
- [ ] B) `Stack<T>` — the last added comes out first (LIFO)
- [ ] C) an array's size can be changed after creation through `Add`
- [ ] D) `List<T>` stores elements in an internal array

<details><summary>Answer</summary>

**A, B, D.** C is wrong: an array's size is fixed, and `Add` belongs to `List<T>`, which replaces its internal array itself when it grows.
→ [Basics](basics.en.md#listt)

</details>

## 4. 🟢 What does the code print?

```csharp
var list = new List<int>();
for (int i = 0; i < 20; i++) list.Add(i);
Console.WriteLine(list.Capacity);
```

- A) `20`
- B) `32`
- C) `16`
- D) `64`

<details><summary>Answer</summary>

**B.** Capacity grows by doubling (4, 8, 16, 32): a list of 20 elements had capacity 32 (verified). If the size is known in advance — `new List<int>(20)` and there are no reallocations.
→ [Advanced theory](advanced.en.md#list-capacity)

</details>

## 5. 🟡 What happens?

```csharp
var nums = new List<int> { 1, 2, 3, 4, 5, 6 };
foreach (var n in nums)
    if (n % 2 == 0) nums.Remove(n);
```

- A) all even numbers are removed
- B) `InvalidOperationException`
- C) `IndexOutOfRangeException`
- D) an infinite loop

<details><summary>Answer</summary>

**B.** Modifying a `List<T>` during `foreach` throws `InvalidOperationException`: the enumerator tracks the collection's version (verified). Safe: `nums.RemoveAll(n => n % 2 == 0)` or iterating a copy.
→ [Advanced theory](advanced.en.md#modifying-a-collection-while-iterating)

</details>

## 6. 🟡 Which of these compiles and runs without an error?

- A) `List<object> l = new List<string>();`
- B) `IEnumerable<object> e = new List<string> { "a" };`
- C) `object[] a = new string[1]; a[0] = 1;`
- D) `List<string> l = new List<object>();`

<details><summary>Answer</summary>

**B.** `IEnumerable<out T>` is covariant: a sequence of strings is a sequence of objects (reading is safe). `List<T>` is invariant, so A and D do not compile. C compiles but at run time gives `ArrayTypeMismatchException`: arrays are covariant, a known hole.
→ [Advanced theory](advanced.en.md#variance)

</details>

## 7. 🟡 What does `BinarySearch` return?

```csharp
var a = new List<int> { 1, 3, 5 };
Console.WriteLine($"{a.BinarySearch(3)} {a.BinarySearch(4)}");
```

- A) `1 -1`
- B) `1 -3`
- C) `1 2`
- D) `0 -2`

<details><summary>Answer</summary>

**B.** For a found element it is its index (`1`). For a missing one the bitwise complement of the insertion position is returned: `~2 = -3`. The check `result >= 0` means "found"; the insertion position is `~result`.
→ [Expert theory](expert.en.md#the-binarysearch-result)

</details>

## 8. 🟡 Select all correct statements

- [ ] A) `ReadOnlyCollection<T>` is a view over the source list: additions to the source are visible through it
- [ ] B) an `ImmutableList<T>` after `ToImmutableList()` does not change when the source list changes
- [ ] C) casting an `IReadOnlyList<int>` back to `List<int>` always gives `null`
- [ ] D) the enumeration order of a `Dictionary` is guaranteed to be the insertion order

<details><summary>Answer</summary>

**A, B.** C is wrong: an interface only hides methods, `as List<int>` returned non-`null` (verified). D is wrong: the order is a side effect of the implementation; after a `Remove` and an insert it changed (`3,1,2` → `3,9,2`).
→ [Advanced theory](advanced.en.md#immutable-and-read-only)

</details>

## 9. 🟡 Open question: how do you choose a collection for "search by key often, iterate in insertion order occasionally"?

<details><summary>Key points of a good answer</summary>

- lookup by key — `Dictionary<TKey, TValue>` (O(1) on average);
- a `Dictionary`'s insertion order is not guaranteed — if you need it, you need a separate structure (`List<TKey>`, `LinkedList<T>`) or `OrderedDictionary<TKey, TValue>`;
- the key must be immutable and have correct `Equals`/`GetHashCode`;
- for frequent insertions in the middle and traversals `LinkedList<T>`/`SortedDictionary` fit, if key order is also needed.

</details>

## 10. 🔴 What does the code print?

```csharp
class Counter<T> { public static int Count; public Counter() { Count++; } }

new Counter<int>(); new Counter<int>(); new Counter<string>();
Console.WriteLine($"{Counter<int>.Count} {Counter<string>.Count}");
```

- A) `3 3`
- B) `2 1`
- C) `1 2`
- D) `2 2`

<details><summary>Answer</summary>

**B.** Each closed type (`Counter<int>`, `Counter<string>`) has its own static fields, because the runtime creates a separate type for each combination of arguments (verified).
→ [Expert theory](expert.en.md#static-fields-of-a-generic-type)

</details>

## 11. 🔴 What is true about sorting?

- A) `Enumerable.OrderBy` is stable: equal keys keep the original order
- B) `List<T>.Sort` and `Array.Sort` guarantee stability
- C) sort stability affects only speed
- D) `OrderBy` sorts the source list in place

<details><summary>Answer</summary>

**A.** `OrderBy` kept the order of equal keys (`c,b,a` for `("b",1),("a",1),("c",0)` sorted by the number). `Sort` gives no such guarantee and modifies the collection in place, while `OrderBy` returns a new sequence.
→ [Expert theory](expert.en.md#sort-stability)

</details>
