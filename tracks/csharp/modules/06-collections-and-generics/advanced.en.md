---
title: "Collections and generics: advanced"
lang: en
track: csharp
kind: module
doc: advanced
slug: 06-collections-and-generics
part: 1
tags: [list-capacity, dictionary, modification-during-enumeration, generic-constraints, variance, covariance, immutable-collections, frozen-collections, icomparable, sorting]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-22
---

# Collections and generics: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

## List capacity

`List<T>` stores elements in an array and creates a larger one when it fills up. The `Capacity` property shows the size of the internal array, `Count` the number of elements. Verified: an empty list has capacity 0, and while adding 20 elements it changed `0 → 4 → 8 → 16 → 32` — it doubles.

```csharp
var l = new List<int>(100);      // reserve capacity: no re-allocations while adding up to 100
l.Add(1); l.Add(2); l.Add(3);
l.TrimExcess();                  // Capacity becomes 3
```

If the size is known in advance, set the capacity in the constructor (or use `EnsureCapacity`): it avoids extra allocations and copying. Appending at the end is amortized `O(1)`; inserting and removing in the middle is `O(n)` because elements are shifted.

## Modifying a collection while iterating

An enumerator remembers the collection's "version". If the collection has changed, the next `MoveNext` throws an exception. Verified:

| Operation during `foreach` | Result |
|---|---|
| `List<T>.Add` | `InvalidOperationException`: "Collection was modified; enumeration operation may not execute." |
| `List<T>.Remove` | `InvalidOperationException` |
| `Dictionary.Add` / `d[new key] = …` | `InvalidOperationException` |
| `Dictionary.Remove` | works (verified on .NET 11) |
| `Dictionary[existing key] = …` | works |

The right ways to change a collection in a loop:

```csharp
var l3 = new List<int> { 1, 2, 3, 4 };
l3.RemoveAll(x => x % 2 == 0);                // 1,3 — the best way
for (int i = l3.Count - 1; i >= 0; i--)        // from the end: indexes do not shift
    l3.RemoveAt(i);
```

Or iterate over a **copy** (`foreach (var x in list.ToList())`), or collect the changes separately and apply them after the loop.

## Dictionary in detail

```csharp
var dict = new Dictionary<string, int> { ["a"] = 1 };
dict["a"] = 5;                                      // overwrites
dict.TryAdd("a", 9);                                // false, nothing changes
int value = dict.GetValueOrDefault("q", -1);        // -1
var ci = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase) { ["Key"] = 1 };
Console.WriteLine(ci.ContainsKey("KEY"));           // True
```

- **The key comparer.** By default it is `EqualityComparer<TKey>.Default`; for strings it can be replaced, for example with a case-insensitive one.
- **Order.** In the check, with no removals the keys came out in insertion order (`3,1,2`), but after `Remove(1)` and `d[9] = …` the new key took the freed slot: `3,9,2`. The documentation does not guarantee the order, and you cannot rely on it: if you need an order, use `SortedDictionary` or a `List` of pairs.
- **Keys** must be immutable and implement `Equals`/`GetHashCode` correctly ([05 · Interfaces, structs, records](../05-interfaces-structs-records/advanced.en.md#equality-and-gethashcode)).
- **Complexity**: lookup, insertion, removal — `O(1)` on average.
- **`CollectionsMarshal.GetValueRefOrAddDefault`** returns a reference to the value and lets you update it with one lookup instead of two (`TryGetValue` + write): `ref int c = ref CollectionsMarshal.GetValueRefOrAddDefault(dict, "k", out _); c++;`.

## Sorted collections, queues, lists

- `SortedDictionary<K,V>` and `SortedSet<T>` keep keys in sort order (a balanced tree underneath): operations are `O(log n)`. `SortedSet` has `Min`, `Max` and `GetViewBetween` (verified: for `{1,3,5}` the range `2..5` gave `3,5`).
- `PriorityQueue<TElement, TPriority>` returns the element with the **smallest** priority. Do not rely on the order of elements with equal priority.
- `LinkedList<T>` gives `O(1)` insertion and removal when you have a node, but iteration is slower and memory goes to the links. In practice `List<T>` is almost always faster.
- `Queue<T>` and `Stack<T>` are array-based; `TryDequeue`/`TryPop`/`TryPeek` do not throw on an empty collection.

## Sorting and comparison

Sorting uses `IComparable<T>` (the natural order) or a supplied `IComparer<T>`/delegate:

```csharp
var words = new[] { "bb", "a", "ccc" };
Array.Sort(words, (x, y) => x.Length.CompareTo(y.Length));    // a,bb,ccc

var sorted = people.OrderBy(p => p.Age).ThenBy(p => p.Name).ToList();   // LINQ, stable
```

An important difference: **`OrderBy` (LINQ) is stable** — elements with an equal key keep their original order (verified: `("b",1),("a",1),("c",0)` → `c,b,a`), while `List<T>.Sort` and `Array.Sort` use introspective sort and **do not guarantee stability**. `BinarySearch` works only on sorted data; if the element is absent it returns the bitwise complement of the insertion position (`~index`: for `{1,3,5}` and `4` it returned `-3`, meaning insert at position `2`).

## Arrays and covariance

Arrays of reference types are **covariant**: `object[] objs = new string[] { "a" };` compiles, but writing an unsuitable type throws at run time:

```csharp
object[] objs = new string[] { "a" };
objs[0] = 1;       // ArrayTypeMismatchException
```

This is a historical quirk: safety is checked on write. `List<T>` has no such problem because generic classes are invariant (see below).

## Constraints on type parameters

| Constraint | Requires |
|---|---|
| `where T : class` / `class?` | a reference type (non-nullable / allowing `null`) |
| `where T : struct` | a non-nullable value type |
| `where T : new()` | a public parameterless constructor |
| `where T : Base` | a class derived from `Base` |
| `where T : IFoo` | implements the interface |
| `where T : U` | derived from another type parameter |
| `where T : notnull` | a non-nullable type |
| `where T : unmanaged` | a value type without references (you can take its size and a pointer) |
| `where T : allows ref struct` | (C# 13) allows substituting a `ref struct` |

Constraints combine: `where T : class, IComparable<T>, new()` (order: `class`/`struct` first, `new()` last). A constraint gives access to members: verified, `where T : new()` lets you write `new T()`, `where T : unmanaged` allows `Unsafe.SizeOf<T>()` (4 for `int`, 8 for `long`), `where T : IComparable<T>` allows calling `CompareTo`.

## Variance

**Covariance** (`out T`) lets you use a more derived type where a base type is expected; **contravariance** (`in T`) is the opposite:

```csharp
IEnumerable<object> eo = new List<string> { "a" };   // IEnumerable<out T>: covariant, OK
Func<object> fo = () => "s";                         // Func<out TResult>: covariant

Action<object> ao = o => { };
Action<string> asx = ao;                             // Action<in T>: contravariant, OK
```

- `IEnumerable<out T>`, `IReadOnlyList<out T>`, `Func<out TResult>` are covariant: they are only read from.
- `Action<in T>`, `IComparer<in T>`, `IEqualityComparer<in T>` are contravariant: they are only passed into.
- `List<T>`, `IList<T>`, `ICollection<T>`, `Dictionary<K,V>` are **invariant**: a `List<string>` cannot be converted to `List<object>` (verified: `is List<object>` is `False`), otherwise a number could be added to a list of strings.

Variance works only for **reference** types and only on interfaces and delegates; for value types (`IEnumerable<int>` → `IEnumerable<object>`) it does not work.

## Immutable and read-only

| Type | What it is |
|---|---|
| `IReadOnlyList<T>` | an **interface**: the caller cannot modify, but others can change the object (and it can be cast back: `iro as List<int>` is not `null`) |
| `ReadOnlyCollection<T>`, `list.AsReadOnly()` | a wrapper **view**: changes to the source list are visible (verified: after `src.Add(3)` the wrapper has `Count == 3`) |
| `ImmutableList<T>`, `ImmutableArray<T>`, `ImmutableDictionary` (`System.Collections.Immutable`) | truly immutable: operations return a new collection (`Add` gave 4 elements while the source has 3) |
| `FrozenDictionary<K,V>`, `FrozenSet<T>` | (.NET 8) created once and optimized for fast reads; no changes |

For a public API `IReadOnlyList<T>` is usually enough. Immutability is needed when a collection is read from several threads ([18 · Concurrency](../18-concurrency/advanced.en.md)) or stored as a record value. `Frozen*` suits lookup tables built once at startup and read very often.

## Best practices

- Set the capacity when the size is known.
- Accept `IEnumerable<T>`/`IReadOnlyList<T>`, return `IReadOnlyList<T>` or a concrete type; do not expose mutable internal collections.
- For lookup by value use `HashSet`/`Dictionary`, not `List.Contains` in a loop.
- Do not rely on element order in `Dictionary` and `HashSet`.
- Do not use mutable objects as keys.
- Constrain type parameters minimally.
- Do not take `ArrayList`/`Hashtable` in new code.

## Common mistakes

- ❌ Modifying a collection in `foreach` → ✅ `RemoveAll`, iterating a copy, a loop from the end.
- ❌ A double lookup in a dictionary (`ContainsKey` + indexer) → ✅ `TryGetValue`.
- ❌ Expecting stability from `List.Sort` → ✅ `OrderBy`.
- ❌ Keeping a reference to an internal list and changing it from outside → ✅ a defensive copy or `IReadOnlyList<T>`.
- ❌ Expecting a `List<Derived>` to be passable as `List<Base>` → ✅ `IEnumerable<Base>` or `IReadOnlyList<Base>`.
- ❌ `foreach` over a `Dictionary` while adding keys → ✅ collect the changes and apply them after the iteration.

## Comparison with other tracks

| | C# | Python |
|---|---|---|
| A dynamic array | `List<T>` | `list` |
| A dictionary | `Dictionary<K,V>` (order not guaranteed) | `dict` (insertion order guaranteed) |
| A set | `HashSet<T>` | `set` |
| A queue | `Queue<T>`, `PriorityQueue` | `collections.deque`, `heapq` |
| Element types | checked by the compiler (generics) | type hints, checked by mypy |
| Immutable | `ImmutableList<T>`, `FrozenSet<T>` | `tuple`, `frozenset` |

## Related materials

- Before: [basics](basics.en.md)
- Next: [expert](expert.en.md) — the internals of `List` and `Dictionary`, generics at run time, reference, interview questions
- Equality, `GetHashCode`, boxing: [05 · Interfaces, structs, records](../05-interfaces-structs-records/advanced.en.md)
- Querying collections: [07 · LINQ](../07-linq/overview.en.md)
- Collections for multithreaded code: [18 · Concurrency](../18-concurrency/advanced.en.md)
- The same in other tracks: [Python](../../../python/overview.en.md)

## Sources

- [Selecting a collection class](https://learn.microsoft.com/dotnet/standard/collections/selecting-a-collection-class)
- [When to use generic collections](https://learn.microsoft.com/dotnet/standard/collections/when-to-use-generic-collections)
- [Constraints on type parameters](https://learn.microsoft.com/dotnet/csharp/programming-guide/generics/constraints-on-type-parameters)
- [Covariance and contravariance](https://learn.microsoft.com/dotnet/standard/generics/covariance-and-contravariance)
- [Immutable collections](https://learn.microsoft.com/dotnet/standard/collections/thread-safe/immutable-collections)
- [Frozen collections](https://learn.microsoft.com/dotnet/api/system.collections.frozen)
- [Array.Sort: remarks](https://learn.microsoft.com/dotnet/api/system.array.sort)
