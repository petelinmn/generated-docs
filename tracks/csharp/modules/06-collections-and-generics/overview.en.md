---
title: "Collections and generics"
lang: en
track: csharp
kind: module
doc: overview
slug: 06-collections-and-generics
part: 1
levels: [1, 2, 3]
tags: [collections, generics, list, dictionary, hashset, queue, stack, array, ienumerable, collection-expressions, type-parameters]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-22
---

# Collections and generics

🌐 [Русский](overview.ru.md) · **English**

## In a nutshell

Collections are the ready-made data structures of .NET: an array, `List<T>`, `Dictionary<K,V>`, `HashSet<T>`, queues, stacks, sorted and immutable variants. All of them are generic: the element type is given as a parameter, the compiler checks types, and values are not boxed. Choosing a collection and understanding how it works determine both the speed and the correctness of a program.

## Key ideas

- **An array** has a fixed size; **`List<T>`** is a dynamic array whose capacity doubles; **`Dictionary`** and **`HashSet`** are hash tables with `O(1)` average lookup.
- **Modifying a collection during `foreach`** gives `InvalidOperationException`: use `RemoveAll`, iterate a copy, or loop from the end.
- **The order in `Dictionary` is not guaranteed**: after a removal a new key can take the freed slot.
- **Interfaces** (`IEnumerable<T>`, `IReadOnlyList<T>`, …): accept the narrowest one, do not expose mutable internal collections.
- **Generics are reified:** `List<int>` and `List<string>` are different types with their own static fields; value types are not boxed; `where T : …` constraints give access to members.
- **Variance:** `IEnumerable<out T>` is covariant, `Action<in T>` is contravariant, `List<T>` is invariant; arrays are unsafely covariant.
- **Read-only ≠ immutable:** `IReadOnlyList` and `ReadOnlyCollection` are views, `Immutable*` and `Frozen*` are real protection.

## Minimal example

```csharp
var ages = new Dictionary<string, int> { ["Ann"] = 30 };
ages["Bob"] = 25;

if (ages.TryGetValue("Cid", out int age))
    Console.WriteLine(age);
else
    Console.WriteLine("no such key");    // no such key

static T Max<T>(T a, T b) where T : IComparable<T> => a.CompareTo(b) >= 0 ? a : b;
Console.WriteLine(Max(3, 7));            // 7
```

## When to use and when not

- ✅ `List<T>` by default for sequences, `Dictionary` for lookup by key, `HashSet` for uniqueness.
- ✅ `IReadOnlyList<T>` in public API signatures.
- ✅ `Immutable*`/`Frozen*` for data shared between threads and for lookup tables.
- ❌ `ArrayList`, `Hashtable` — boxing and no typing.
- ❌ `List.Contains` in a loop over large data — use a `HashSet`.
- ❌ Mutable keys in dictionaries and sets.

## Levels of this material

| Level | What's inside |
|---|---|
| [1. Basics](basics.en.md) | arrays, `List`, `Dictionary`, `HashSet`, queues and stacks, collection interfaces, collection expressions, generics basics, choosing a collection |
| [2. Advanced](advanced.en.md) | capacity, modification during iteration, the dictionary in detail, sorting, type parameter constraints, variance, immutable and frozen collections |
| [3. Expert](expert.en.md) | the internals of `List` and `Dictionary`, generics at run time, collection expressions, complexity and variance references, gotchas, 17 interview questions |
| 4. Self-check | not generated yet — `/learn csharp 06-collections-and-generics level 4` |
| 5. Runnable examples | not generated yet — `/learn csharp 06-collections-and-generics level 5` |
| 6. Tests | not generated yet — `/learn csharp 06-collections-and-generics level 6` |

## Related materials

- Prerequisites: [05 · Interfaces, structs, records](../05-interfaces-structs-records/overview.en.md), [04 · OOP: classes and objects](../04-oop-basics/overview.en.md)
- Next: [07 · LINQ](../07-linq/overview.en.md)
- Measuring allocations: [12 · Memory and GC](../12-memory-and-gc/overview.en.md)
- Thread-safe collections: [18 · Concurrency](../18-concurrency/overview.en.md)
- The same in other tracks: [Python](../../../python/overview.en.md) — `list`, `dict`, `set`, `heapq`
