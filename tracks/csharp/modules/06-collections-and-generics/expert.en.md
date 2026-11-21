---
title: "Collections and generics: expert"
lang: en
track: csharp
kind: module
doc: expert
slug: 06-collections-and-generics
part: 1
tags: [list-internals, dictionary-internals, hash-table, generics-runtime, reification, variance, collection-expressions, complexity, comparers, interview-questions]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-22
---

# Collections and generics: expert

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · **3. Expert**

## Under the hood

### List<T>

Inside, `List<T>` has three private fields (verified through reflection): the array `_items`, the counter `_size` and the version number `_version`. `Count` is `_size`, `Capacity` is the length of `_items`. When an item is added to a full list, a new array twice as large is allocated (`0 → 4 → 8 → 16 → 32`) and the elements are copied. Every modification increments `_version`; the `List<T>.Enumerator` enumerator is a struct that remembers the version at creation and compares it on every `MoveNext`: that is why a change during `foreach` gives `InvalidOperationException`. The struct enumerator allocates no heap object if `foreach` runs over a variable of type `List<T>`, and if you cast it to `IEnumerable<T>` the enumerator is boxed ([03 · Control flow and methods](../03-control-flow-and-methods/expert.en.md)).

### Dictionary<TKey, TValue>

The `Dictionary` fields (verified): `_buckets`, `_entries`, `_fastModMultiplier`, `_count`, `_freeList`, `_freeCount`, `_version`, `_comparer`. It is a **hash table with chaining on arrays**:

1. The key's hash (`comparer.GetHashCode(key)`) is reduced to a bucket number in `_buckets`.
2. The bucket stores the index of the first entry in the `_entries` array; each entry holds the key, the value, the hash and the index of the **next entry** in the same bucket. This is how collisions are handled: along the chain the hash and the key (`Equals`) are compared.
3. A removed entry goes to the free list (`_freeList`), and the next insertion takes its place. That is why after `Remove(1)` and adding key `9` the enumeration order was `3,9,2`, not `3,2,9` (verified).
4. The number of buckets is a **prime**: after adding 1 element there are 3 buckets, after 20 — 37, and `EnsureCapacity(100)` returned 107 (verified). When full, the table is rebuilt larger.

This implies a requirement for keys: `GetHashCode` must be fast and uniform, and equal keys must give equal hashes. For `string` the runtime uses a **randomized** hash: the value of `"abc".GetHashCode()` differed between two runs (1199515494 and 373536423), which protects against attacks that pick colliding keys. So never persist a string's hash between runs.

`HashSet<T>` is built the same way (the same fields without values), `SortedDictionary` and `SortedSet` are a balanced tree, `Stack<T>` is an array `_array`, `_size`, `_version`.

### Collection expressions

The compiler chooses the creation method by the target type. Verified by runtime types: for `ICollection<int>` a `List<int>` is created, and for `IEnumerable<int>` and `IReadOnlyList<int>` — special compiler-generated read-only types (`<>z__ReadOnlyArray`1`, `<>z__ReadOnlySingleElementList`1`). For `ReadOnlySpan<int>` no data is allocated on the heap.

### Generics at run time

Unlike Java, .NET **reifies** generics: the information about type parameters is preserved at run time.

- `typeof(List<>)` is an open generic type, `typeof(List<int>)` is a closed constructed one; `typeof(Dictionary<,>).GetGenericArguments().Length` is 2 (verified).
- Every closed type is a **separate type** with its own static fields: `Counter<int>.Next()` gave `1, 2`, while `Counter<string>.Next()` started from `1` again (verified).
- For value types the JIT creates a separate code specialization, so `List<int>` stores values right in the array: adding 1000 `int` values did not allocate a byte (measured in [12 · Memory and GC](../12-memory-and-gc/advanced.en.md#measuring-allocations)), unlike `ArrayList` (24,000 bytes).
- Calling an interface method through a constrained type parameter does not box a struct (0 bytes versus 24 for an interface parameter — [05 · Interfaces, structs, records](../05-interfaces-structs-records/expert.en.md#boxing-and-constrained-calls)).
- Without constraints, only the members of `object` are available on `T`: you cannot call operators or type methods; static abstract interface members allow `T.Add(a, b)` ([05](../05-interfaces-structs-records/advanced.en.md#static-interface-members)).

## Reference

### Operation complexity

| Collection | Access by index/key | Search by value | Insert | Remove |
|---|---|---|---|---|
| `T[]` | `O(1)` | `O(n)` | — (fixed size) | — |
| `List<T>` | `O(1)` | `O(n)` | `O(1)` amortized at the end, `O(n)` in the middle | `O(n)` |
| `Dictionary<K,V>` | `O(1)` average | `O(n)` | `O(1)` average | `O(1)` average |
| `HashSet<T>` | — | `O(1)` average | `O(1)` average | `O(1)` average |
| `SortedDictionary<K,V>`, `SortedSet<T>` | `O(log n)` | `O(log n)` | `O(log n)` | `O(log n)` |
| `Queue<T>`, `Stack<T>` | — | `O(n)` | `O(1)` | `O(1)` |
| `PriorityQueue<E,P>` | — | — | `O(log n)` | `O(log n)` |
| `LinkedList<T>` | — | `O(n)` | `O(1)` with a node | `O(1)` with a node |

### Interfaces and their members

| Interface | Key members | Implemented by |
|---|---|---|
| `IEnumerable<T>` | `GetEnumerator()` | all collections, `string` (`IEnumerable<char>`) |
| `ICollection<T>` | `Count`, `Add`, `Remove`, `Contains`, `Clear`, `CopyTo` | `List`, `HashSet`, `Dictionary` |
| `IList<T>` | indexer, `Insert`, `RemoveAt`, `IndexOf` | `List`, arrays |
| `IReadOnlyCollection<T>`, `IReadOnlyList<T>`, `IReadOnlyDictionary<K,V>` | `Count`, indexer | almost all |
| `ISet<T>`, `IReadOnlySet<T>` | `UnionWith`, `IntersectWith`, `IsSubsetOf`, … | `HashSet`, `SortedSet` |
| `IComparer<T>`, `IEqualityComparer<T>` | `Compare`, `Equals`/`GetHashCode` | comparers for `Sort` and dictionaries |

### Type variance

| Type | Variance |
|---|---|
| `IEnumerable<out T>`, `IEnumerator<out T>`, `IReadOnlyList<out T>`, `IReadOnlyCollection<out T>` | covariant |
| `Func<…, out TResult>` | covariant in the result, contravariant in the parameters (`in T`) |
| `Action<in T>`, `IComparer<in T>`, `IEqualityComparer<in T>`, `IComparable<in T>` | contravariant |
| `List<T>`, `IList<T>`, `ICollection<T>`, `Dictionary<K,V>`, `HashSet<T>` | invariant |
| arrays of reference types | covariant with a check on write (`ArrayTypeMismatchException`) |

The rules: `out` — the parameter only in result positions; `in` — only in argument positions; variance works only between **reference** types and only on interfaces and delegates.

### Classes and interfaces for comparison

| Need | Implement |
|---|---|
| the natural order | `IComparable<T>` |
| an order "from outside" | `IComparer<T>` / a `Comparison<T>` delegate |
| equality in collections | `IEquatable<T>` + `GetHashCode` |
| non-standard equality for one dictionary | an `IEqualityComparer<T>` in the constructor |
| case-insensitive strings | `StringComparer.OrdinalIgnoreCase` |

## Gotchas

Every gotcha was verified by running it.

### Capacity grows in jumps

A list of 20 elements had capacity 32, not 20: doubling leaves headroom. After `TrimExcess` a list of 3 elements got capacity 3. If you create `new List<int>(100)`, the capacity is 100 right away — with no re-allocations.

### Modification during iteration is not always an error

`List.Add`, `List.Remove`, `Dictionary.Add` during `foreach` throw `InvalidOperationException`. But `Dictionary.Remove` and assignment to an **existing** key do not (on .NET 11). Do not build code on this: the rules for `List` and `HashSet` differ.

### Dictionary enumeration order

The keys came out in insertion order (`3,1,2`) as long as nothing was removed. After `Remove(1)` and inserting `9`: `3,9,2`. The order is a side effect of the implementation, not a guarantee.

### Array covariance

`object[] objs = new string[]{"a"}; objs[0] = 1;` → `ArrayTypeMismatchException` at run time, not a compile error.

### List<string> is not a List<object>

`new List<string>() is List<object>` → `False` (invariance). But `IEnumerable<object> eo = new List<string>{ "a" };` works.

### ReadOnlyCollection is a view, not a copy

After `src.Add(3)` the wrapper `new ReadOnlyCollection<int>(src)` shows `Count == 3`. A real copy is `ImmutableList` (a `ToImmutableList()` snapshot: after adding to the source list the immutable one kept its original 3 elements).

### IReadOnlyList can be cast back

`iro as List<int>` returned non-`null`: the interface only hides methods, it does not protect. For real protection use `AsReadOnly()` or `ImmutableArray`.

### The BinarySearch result

For `{1,3,5}`: the found `3` → `1`; the missing `4` → `-3`, which is `~2`, the insertion position `2` in bitwise complement. The check `result >= 0` means "found".

### Static fields of a generic type

Each closed type has its own static field: `Counter<int>` and `Counter<string>` count independently (`1, 2` and `1`).

### A string's hash changes between runs

Two runs gave `1199515494` and `373536423` for `"abc"`. Never write a string's `GetHashCode` to a file or a database.

### A null key

`dict[null!] = 1` → `ArgumentNullException`. `null` is allowed in a `HashSet<T>` (verified), but not as a `Dictionary` key.

### Sort stability

`OrderBy` preserved the order of equal keys (`c,b,a` for `("b",1),("a",1),("c",0)`). `List.Sort` and `Array.Sort` do not guarantee this.

## Trade-offs and alternatives

| Task | Option | Price |
|---|---|---|
| Fast lookup by key | `Dictionary` | memory for the table; order not guaranteed |
| Sorted iteration | `SortedDictionary` | `O(log n)` per operation |
| | `Dictionary` + sorting when needed | sorting on every iteration |
| A lookup table built once | `FrozenDictionary` | slow to create, fast to read |
| Immutability, thread safety | `Immutable*` | every "change" creates a new collection |
| Read only in an API | `IReadOnlyList<T>` | does not protect against casting back |
| A queue for threads | `ConcurrentQueue`, `Channel<T>` | see [18 · Concurrency](../18-concurrency/advanced.en.md) |
| Minimum allocations | `Span<T>`, `ArrayPool<T>`, `CollectionsMarshal` | more complex code and lifetimes |

## Interview questions

**1. How is `List<T>` built? How does the capacity grow?**

<details><summary>Answer</summary>

An array `_items`, a counter `_size` and a version `_version`. On overflow an array twice as large is allocated (`0 → 4 → 8 → 16 → 32`, verified) and the elements are copied, so `Add` is amortized `O(1)`. Insertion in the middle is `O(n)` (a shift). A known size is better set in the constructor.

</details>

**2. How is `Dictionary<K,V>` built?**

<details><summary>Answer</summary>

A hash table with chaining on arrays: `_buckets` (the index of the first entry) and `_entries` (key, value, hash, the index of the next entry in the chain). The key's hash gives the bucket, and along the chain the hash and `Equals` are compared. The number of buckets is prime (3, 37, 107 in the checks). Removed entries go to the free list and are reused, so the enumeration order is not guaranteed.

</details>

**3. What is the complexity of `Dictionary` operations? When does it degrade?**

<details><summary>Answer</summary>

Lookup, insertion and removal are `O(1)` on average. It degrades to `O(n)` if `GetHashCode` produces many collisions (a bad hash, for example a constant). String hashes are randomized by the runtime, which protects against deliberate collision picking.

</details>

**4. Why can't you modify a collection in `foreach`? What to do?**

<details><summary>Answer</summary>

The enumerator remembers the collection's version and checks it on every `MoveNext`; a change gives `InvalidOperationException` ("Collection was modified…"). Solutions: `RemoveAll`, a `for` loop from the end, iterating a copy (`ToList()`), collecting changes and applying them after the loop. `Dictionary.Remove` and writing to an existing key are allowed in .NET 11, but you should not rely on that.

</details>

**5. How does `List<T>` differ from an array? When is an array better?**

<details><summary>Answer</summary>

An array has a fixed size, faster access (no version check and no extra level), and supports multidimensionality and covariance. `List<T>` grows and is more convenient. An array is better when the size is known and speed matters (including `Span<T>` over it).

</details>

**6. What are covariance and contravariance? Give examples.**

<details><summary>Answer</summary>

Covariance (`out T`) — `IEnumerable<string>` can be used as `IEnumerable<object>` (read only). Contravariance (`in T`) — `Action<object>` can be assigned to `Action<string>` (pass in only). `List<T>` is invariant: `List<string>` is not a `List<object>`, otherwise an `int` could be put into it. It works only for reference types, interfaces and delegates.

</details>

**7. Trick question: why are arrays covariant and what is dangerous about it?**

<details><summary>Answer</summary>

A historical decision of .NET 1.0 (before generics existed). `object[] a = new string[1]; a[0] = 1;` compiles but throws `ArrayTypeMismatchException` at run time: every write into an array of reference types checks the type (verified).

</details>

**8. How are generics implemented in .NET? How do they differ from Java?**

<details><summary>Answer</summary>

They are reified: type parameters are preserved at run time, `typeof(List<int>)` and `typeof(List<string>)` are different types with their own static fields (verified), and for value types the JIT creates separate code without boxing. In Java generics are erased at compile time (type erasure) and primitive values are boxed.

</details>

**9. What type parameter constraints are there and why are they needed?**

<details><summary>Answer</summary>

`class`, `struct`, `new()`, a base class, an interface, `notnull`, `unmanaged`, `allows ref struct`, another type parameter. A constraint gives access to members: without `where T : IComparable<T>` you cannot call `CompareTo`, without `new()` — `new T()`, without `unmanaged` — `Unsafe.SizeOf<T>()`.

</details>

**10. How do `IEnumerable<T>`, `ICollection<T>`, `IList<T>`, `IReadOnlyList<T>` differ? What to accept and what to return?**

<details><summary>Answer</summary>

`IEnumerable<T>` — iteration only; `IReadOnlyList<T>` — + `Count` and an index without modification; `ICollection<T>` — + modification and `Count`; `IList<T>` — + an index and `Insert`. Accept the narrowest interface that is enough; return `IReadOnlyList<T>` or a concrete type (but not a mutable internal collection).

</details>

**11. How do `ReadOnlyCollection<T>`, `IReadOnlyList<T>` and `ImmutableList<T>` differ?**

<details><summary>Answer</summary>

`IReadOnlyList<T>` is only an interface: the object stays mutable and can be cast back to `List<T>`. `ReadOnlyCollection<T>` is a wrapper view: it does not change itself but shows changes to the source list (after `src.Add(3)`, `Count == 3`). `ImmutableList<T>` is true immutability: operations return a new collection.

</details>

**12. When to use `HashSet<T>` and when `List<T>`?**

<details><summary>Answer</summary>

`HashSet<T>` — for uniqueness and a fast "is the element there" check (`O(1)` on average), order does not matter. `List<T>` — when order and indexes are needed and duplicates are allowed. `Contains` on a list is `O(n)`, so in a loop it makes the algorithm quadratic.

</details>

**13. What is the difference between `OrderBy` and `List.Sort`?**

<details><summary>Answer</summary>

`OrderBy` (LINQ) returns a new sequence and is **stable** (equal keys keep their order). `List.Sort` sorts in place with introspective sort and **does not guarantee stability**. Verified on `("b",1),("a",1),("c",0)`: `OrderBy` gave `c,b,a`.

</details>

**14. What does `BinarySearch` return if the element is absent?**

<details><summary>Answer</summary>

A negative number — the bitwise complement of the position where the element should be inserted (`~index`). For `{1,3,5}` and `4` the result is `-3` = `~2`. The list must be sorted, otherwise the result is unpredictable.

</details>

**15. Trick question: why did the key order in a `Dictionary` change after `Remove` and `Add`?**

<details><summary>Answer</summary>

The freed entry goes to the free list (`_freeList`), and the next insertion takes its place in the `_entries` array. The enumeration order is the order of entries in the array, so `3,1,2` became `3,9,2` after removing `1` and adding `9`. The documentation does not guarantee the order.

</details>

**16. What does `where T : unmanaged` do?**

<details><summary>Answer</summary>

It allows only value types that contain no references (numbers, `bool`, `char`, `enum`, structs made of such fields). It lets you take the size (`Unsafe.SizeOf<T>()` — 4 for `int`, 8 for `long`) and a pointer to the value, which low-level and interop code needs.

</details>

**17. How do collection expressions work? What type is created?**

<details><summary>Answer</summary>

The compiler chooses the method by the target type: an array, `List<T>`, `Span<T>`/`ReadOnlySpan<T>` without a heap allocation, interfaces (for `ICollection<T>` — `List<T>`, for `IEnumerable<T>`/`IReadOnlyList<T>` — a compiler-generated read-only type). `..` inserts the elements of another sequence.

</details>

## Cheat sheet

| Need | Solution |
|---|---|
| A list with a known size | `new List<T>(n)` |
| Remove by condition | `RemoveAll(predicate)` |
| Safely read from a dictionary | `TryGetValue`, `GetValueOrDefault` |
| Update a value with one lookup | `CollectionsMarshal.GetValueRefOrAddDefault` |
| Uniqueness, `Contains` | `HashSet<T>` |
| Always sorted | `SortedSet<T>`, `SortedDictionary<K,V>` |
| The most important first | `PriorityQueue<E,P>` |
| An immutable lookup table | `FrozenDictionary` |
| Copy an array | `[..a]`, `Clone()` |
| A covariant parameter | `IEnumerable<T>`, `IReadOnlyList<T>` |
| A type parameter constraint | `where T : …` |
| Stable sort | `OrderBy` |

## Related materials

- Before: [basics](basics.en.md), [advanced](advanced.en.md)
- Equality, `IEquatable<T>`, boxing: [05 · Interfaces, structs, records](../05-interfaces-structs-records/expert.en.md)
- Querying collections: [07 · LINQ](../07-linq/overview.en.md)
- Measuring allocations: [12 · Memory and GC](../12-memory-and-gc/advanced.en.md#measuring-allocations)
- Thread-safe collections: [18 · Concurrency](../18-concurrency/advanced.en.md)

## Sources

- [Collections and data structures](https://learn.microsoft.com/dotnet/standard/collections/)
- [Generics in .NET](https://learn.microsoft.com/dotnet/standard/generics/)
- [Generics in the run time](https://learn.microsoft.com/dotnet/csharp/programming-guide/generics/generics-in-the-run-time)
- [Covariance and contravariance in generics](https://learn.microsoft.com/dotnet/standard/generics/covariance-and-contravariance)
- [Collection expressions: specification](https://learn.microsoft.com/dotnet/csharp/language-reference/proposals/csharp-12.0/collection-expressions)
- [Frozen collections](https://learn.microsoft.com/dotnet/api/system.collections.frozen)
- [CollectionsMarshal](https://learn.microsoft.com/dotnet/api/system.runtime.interopservices.collectionsmarshal)
- [List<T>.Sort: remarks](https://learn.microsoft.com/dotnet/api/system.collections.generic.list-1.sort)
