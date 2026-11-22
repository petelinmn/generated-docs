---
title: "Collections and generics: basics"
lang: en
track: csharp
kind: module
doc: basics
slug: 06-collections-and-generics
part: 1
tags: [collections, generics, list, dictionary, hashset, queue, stack, array, ienumerable, collection-expressions, type-parameters]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-22
---

# Collections and generics: basics

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [02 · Types and variables](../02-types-and-variables/overview.en.md), [03 · Control flow and methods](../03-control-flow-and-methods/overview.en.md).

## Why it matters

Almost every program stores sets of data: a list of orders, a dictionary of settings, a queue of tasks. .NET has **collections** for this — ready-made data structures. They are **generic**: the element type is given as a parameter (`List<int>`, `Dictionary<string, Order>`), so the compiler checks types and values are not boxed (see [05 · Interfaces, structs, records](../05-interfaces-structs-records/advanced.en.md#mutable-structs)).

## Arrays

An array is a fixed number of elements of one type. The size is set at creation and does not change:

```csharp
int[] numbers = [1, 2, 3];          // collection expression
int[] zeros = new int[5];           // five zeros
Console.WriteLine(numbers[0]);      // 1
Console.WriteLine(numbers.Length);  // 3
```

- Indexes start at zero; going out of bounds throws `IndexOutOfRangeException`.
- An array is a **reference type**: `int[] b = a;` does not copy the elements (`b[0] = 9` changes `a[0]` too); a copy is made by `Clone()` (shallow) or `[..a]`.
- There are multidimensional arrays (`int[2, 3]`, whose `Length` is the total number of elements: 6) and "jagged" ones (`int[][]`, an array of arrays, whose inner arrays are initially `null`).

## `List<T>`

`List<T>` is a dynamic array: elements can be added and removed.

```csharp
var names = new List<string> { "Ann", "Bob" };
names.Add("Cid");
names.Remove("Bob");
names.Insert(0, "Zed");
Console.WriteLine(names.Count);             // 3
Console.WriteLine(string.Join(",", names)); // Zed,Ann,Cid
Console.WriteLine(names.Contains("Ann"));   // True
```

The essentials: `Add`, `AddRange`, `Insert`, `Remove`, `RemoveAt`, `RemoveAll(predicate)`, `Contains`, `IndexOf`, `Sort`, `Reverse`, the indexer `list[i]`, `Count`. Access by index is fast, search by value (`Contains`, `IndexOf`) is linear.

## `Dictionary<TKey, TValue>`

A dictionary stores "key — value" pairs and finds a value by key almost instantly:

```csharp
var ages = new Dictionary<string, int> { ["Ann"] = 30 };
ages["Bob"] = 25;                        // add or overwrite
Console.WriteLine(ages["Ann"]);          // 30

if (ages.TryGetValue("Cid", out int age))
    Console.WriteLine(age);
else
    Console.WriteLine("no such key");    // no such key

foreach (var (name, value) in ages)
    Console.WriteLine($"{name}: {value}");
```

The rules:

- keys are unique: `Add` with an existing key throws `ArgumentException`, while assignment through the indexer overwrites the value;
- accessing a missing key with `ages["zzz"]` throws `KeyNotFoundException`; `TryGetValue`, `ContainsKey` or `GetValueOrDefault` are safer;
- a key cannot be `null` (`ArgumentNullException`);
- do not consider the enumeration order guaranteed (more in [advanced](advanced.en.md#dictionary-in-detail)).

## `HashSet<T>`, `Queue<T>`, `Stack<T>`

| Collection | Purpose | Main operations |
|---|---|---|
| `HashSet<T>` | a set of unique values | `Add` (returns `false` for a duplicate), `Contains`, `Remove`, `UnionWith`, `IntersectWith` |
| `Queue<T>` | a queue: first in, first out (FIFO) | `Enqueue`, `Dequeue`, `Peek`, `TryDequeue` |
| `Stack<T>` | a stack: last in, first out (LIFO) | `Push`, `Pop`, `Peek` |
| `SortedSet<T>`, `SortedDictionary<K,V>` | the same, but ordered by key | like `HashSet`/`Dictionary` |
| `PriorityQueue<TElement, TPriority>` | a priority queue | `Enqueue(element, priority)`, `Dequeue` |
| `LinkedList<T>` | a doubly linked list | `AddFirst`, `AddLast`, `Remove` a node |

Examples: `hs.Add(2)` for an already existing element returned `False`; a `PriorityQueue` with priorities 5, 1, 3 yields elements in priority order `1, 3, 5` (a smaller value comes earlier); `Peek` on an empty queue throws `InvalidOperationException`, while `TryDequeue` returns `false`.

## Collection interfaces

Collections implement common interfaces, so code can be written against an interface:

| Interface | What it gives |
|---|---|
| `IEnumerable<T>` | iteration only (`foreach`) |
| `IReadOnlyCollection<T>`, `IReadOnlyList<T>` | + `Count` and an index, no modification |
| `ICollection<T>` | + `Add`, `Remove`, `Count` |
| `IList<T>` | + access by index, `Insert` |
| `IDictionary<K,V>`, `IReadOnlyDictionary<K,V>` | a dictionary |
| `ISet<T>` | set operations |

The rule: **accept the narrowest interface** you need (`IEnumerable<T>` for reading, `IReadOnlyList<T>` if you need an index), and **return** a suitable concrete type or a read-only interface.

```csharp
static int Total(IEnumerable<int> values) => values.Sum();   // works with arrays, lists, sets…
```

## Collection expressions

C# 12 added a unified syntax for creating collections — square brackets, and the `..` operator inserts (spreads) another sequence:

```csharp
List<int> a = [1, 2, 3];
int[] b = [..a, 4];               // 1,2,3,4
ReadOnlySpan<int> c = [1, 2];     // no allocation on the heap
Dictionary<string, int> d = new() { ["a"] = 1 };
```

The result type is determined by the target type on the left: an array, `List<T>`, `Span<T>`, an interface (`IEnumerable<T>`, `IReadOnlyList<T>`, `ICollection<T>`).

## Generics

A generic type or method is described with a **type parameter** `T`, and a concrete type is substituted on use. Without generics you would have to write a separate class for each type or store `object` with casts and boxing.

```csharp
public class Box<T>(T value)
{
    public T Value { get; } = value;
}

var a = new Box<int>(1);
var b = new Box<string>("s");           // a different closed type
```

A generic method:

```csharp
static T Max<T>(T a, T b) where T : IComparable<T> => a.CompareTo(b) >= 0 ? a : b;

Console.WriteLine(Max(3, 7));           // 7 — T is inferred as int
Console.WriteLine(Max("a", "b"));       // b — T is inferred as string
```

`T` is usually inferred from the arguments. The **constraint** `where T : IComparable<T>` tells the compiler that `T` has a `CompareTo` method; without it you cannot call it. The main constraints: `class`, `struct`, `new()`, a specific base class or interface, `notnull` and `unmanaged` — the full list is in [advanced](advanced.en.md#constraints-on-type-parameters).

`default(T)` is the default value of a type parameter: `0` for `int`, `null` for `string`, the zero date for `DateTime`.

## Which collection to choose

| Need | Choice |
|---|---|
| A sequence, appending at the end, access by index | `List<T>` |
| A fixed set of elements, maximum speed | an array |
| Looking up a value by key | `Dictionary<K,V>` |
| Membership checks, unique values | `HashSet<T>` |
| A queue (FIFO) / a stack (LIFO) | `Queue<T>` / `Stack<T>` |
| Always sorted | `SortedSet<T>` / `SortedDictionary<K,V>` |
| Most important first | `PriorityQueue<T,P>` |
| Read only | `IReadOnlyList<T>`, `ImmutableList<T>` |

## Beginner mistakes

- ❌ Modifying a list inside a `foreach` over it → ✅ `InvalidOperationException`; use `RemoveAll` or a `for` loop from the end.
- ❌ Searching a `List<T>` by value in a loop → ✅ `HashSet<T>` or `Dictionary`.
- ❌ Reading `dict[key]` without checking → ✅ `TryGetValue`.
- ❌ Copying an array by assignment → ✅ `Clone()` or `[..a]`.
- ❌ Using the non-generic `ArrayList`, `Hashtable` → ✅ the generic counterparts: in `ArrayList` every number is boxed.
- ❌ Returning an internal `List<T>` from a method → ✅ `IReadOnlyList<T>`, so the caller cannot change the state.

## Related materials

- Next: [advanced](advanced.en.md) — capacity, rules for modification during iteration, constraints, variance, immutable collections
- Structs, records, equality (important for keys): [05 · Interfaces, structs, records](../05-interfaces-structs-records/overview.en.md)
- Querying collections: [07 · LINQ](../07-linq/overview.en.md)
- Classes and inheritance: [04 · OOP: classes and objects](../04-oop-basics/overview.en.md)
- The same in other tracks: [Python](../../../python/overview.en.md) — `list`, `dict`, `set`, `deque`, `heapq`, type hints

## Sources

- [Collections](https://learn.microsoft.com/dotnet/standard/collections/)
- [Selecting a collection class](https://learn.microsoft.com/dotnet/standard/collections/selecting-a-collection-class)
- [Generics](https://learn.microsoft.com/dotnet/csharp/fundamentals/types/generics)
- [Collection expressions](https://learn.microsoft.com/dotnet/csharp/language-reference/operators/collection-expressions)
- [`List<T>`](https://learn.microsoft.com/dotnet/api/system.collections.generic.list-1)
- [`Dictionary<TKey,TValue>`](https://learn.microsoft.com/dotnet/api/system.collections.generic.dictionary-2)
