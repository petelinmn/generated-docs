---
title: "LINQ: expert"
lang: en
track: csharp
kind: module
doc: expert
slug: 07-linq
part: 1
tags: [linq, query-translation, iterator-state-machine, expression-trees, iqueryable, enumerable-shortcuts, async-linq, deferred-execution, interview-questions]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-22
---

# LINQ: expert

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · **3. Expert**

## Under the hood

### Operators are extension methods

LINQ to Objects is the static class `Enumerable` with extension methods over `IEnumerable<T>`; LINQ for providers is the class `Queryable` over `IQueryable<T>`. The query `people.Where(p => p.Age > 26)` is an ordinary call of `Enumerable.Where(people, lambda)`. So any type that implements `IEnumerable<T>` gets LINQ, and your own operator is just one more extension method.

### Query syntax is "sugar"

The compiler **rewrites** query expressions into method calls before type checking:

| Query expression | Method |
|---|---|
| `from x in xs where … select …` | `xs.Where(…).Select(…)` |
| `orderby a, b descending` | `OrderBy(a).ThenByDescending(b)` |
| `group x by k` | `GroupBy(x => k)` |
| `join y in ys on a equals b` | `Join(ys, …)` |
| `join … into g` | `GroupJoin(…)` |
| several `from`s | `SelectMany(…)` |
| `let v = …` | `Select` with an intermediate anonymous type carrying both `x` and `v` |
| `… into g` (continuation) | a nested query over the result |

It follows that query syntax works with **any** type that has the needed methods (including yours), and that each keyword costs exactly as much as the method.

### An iterator is a state machine

The compiler turns a method with `yield return` into a **generated class**. Verified through reflection: for the method `Gen()` the type is named `<<<Main>$>g__Gen|0_68>d`, it implements both `IEnumerable<int>` and `IEnumerator<int>`, and its fields are `<>1__state` (the number of the current resume point), `<>2__current` (the last value) and `<>l__initialThreadId`. Local variables and parameters become fields of this class too.

- `MoveNext()` is a big `switch` on `<>1__state`: it runs code up to the next `yield return`, remembers the point and returns `true`.
- The iterator body is **not executed** when the method is called — only the object is created; so argument checks in it are deferred (see [advanced](advanced.en.md#argument-checks-immediate-and-deferred)).
- One class serves as both the enumerable and the enumerator: on the first `GetEnumerator()` on the same thread it returns itself (which is what `<>l__initialThreadId` is for), and on repeated enumeration a new instance is created.
- A `finally` block in an iterator runs on the enumerator's `Dispose()`: a `break` out of `foreach` triggers it (verified: `x1 finally`).

The same ideas underlie `async` methods (10 · Async/await).

### Optimizations in Enumerable

The LINQ implementation specializes its work by the source type. Verified on a custom collection that counts accesses:

- `Count()` — by the `Count` property, without enumeration;
- `Any()` — by `Count`, without enumeration;
- `ToList()` — copying through `CopyTo`;
- `TryGetNonEnumeratedCount` — exposes this capability: `True` for a list, `False` for an iterator.

The result of `Select(...).Count()` over a list **ran** the projections (the side effect fired twice): LINQ does not skip delegate calls because they may have observable behavior. Do not build code on the assumption that some calls will be skipped: internal optimizations are an implementation detail that has changed between versions.

### Expression trees and IQueryable

`Queryable.Where` takes an `Expression<Func<T,bool>>`, not a delegate (verified: the parameter type is `Expression`1`). The compiler builds a **data tree**: for `x => x > 2` — an `Expression.Lambda` with a `GreaterThan` node, the parameter `x` and the constant `2`; the tree's `ToString()` gives `x => (x > 2)`. A provider (EF Core, LINQ to XML…) walks the tree and translates it into its own language (SQL). `ex.Compile()` turns the tree into a delegate.

- `AsQueryable()` over an in-memory collection creates an `EnumerableQuery<T>`: its `Expression` is `System.Int32[].Where(x => (x > 1))`, and it executes the tree by compiling it back into delegates (verified).
- A lambda with a block body (CS0834) and a tuple literal (CS8143) cannot be turned into a tree, nor can a method with `ref`/`out` be called, among other constructs.
- A call that is untranslatable for the provider gives an exception or a switch to client evaluation — details in [15 · EF Core](../15-ef-core/advanced.en.md#what-ef-can-translate-to-sql).

### Asynchronous LINQ

On .NET 11 the asynchronous operators over `IAsyncEnumerable<T>` are part of the platform — the assembly `System.Linq.AsyncEnumerable` (verified: `ToAsyncEnumerable()` and `ToListAsync()` are available without third-party packages, `typeof(AsyncEnumerable).Assembly.GetName().Name` is `System.Linq.AsyncEnumerable`). They are consumed through `await foreach` and methods like `await seq.ToListAsync()`. More on `await foreach` in 10 · Async/await.

## Reference

> ⚠️ Verify: the versions in which individual operators appeared (`Chunk`, `MinBy`/`MaxBy` — .NET 6; `Order` — .NET 7; `CountBy`, `AggregateBy`, `Index` — .NET 9; `LeftJoin`/`RightJoin` — .NET 10) were checked from memory, not against the official documentation. On .NET 11 all of them work (verified by running).

### Operators by category

| Category | Operators |
|---|---|
| Filtering | `Where`, `OfType`, `Take`, `Skip`, `TakeWhile`, `SkipWhile`, `TakeLast`, `SkipLast`, `Take(Range)` |
| Projection | `Select`, `SelectMany`, `Zip`, `Index`, `Cast` |
| Sorting | `OrderBy`, `OrderByDescending`, `ThenBy`, `ThenByDescending`, `Order`, `OrderDescending`, `Reverse` |
| Grouping | `GroupBy`, `ToLookup`, `CountBy`, `AggregateBy`, `Chunk` |
| Joins | `Join`, `GroupJoin`, `LeftJoin`, `RightJoin`, `SelectMany` |
| Sets | `Distinct`, `DistinctBy`, `Union`, `UnionBy`, `Intersect`, `IntersectBy`, `Except`, `ExceptBy`, `Concat`, `Append`, `Prepend` |
| Elements | `First`, `FirstOrDefault`, `Last`, `LastOrDefault`, `Single`, `SingleOrDefault`, `ElementAt`, `ElementAtOrDefault`, `DefaultIfEmpty` |
| Quantifiers | `Any`, `All`, `Contains`, `SequenceEqual` |
| Aggregates | `Count`, `LongCount`, `Sum`, `Min`, `Max`, `MinBy`, `MaxBy`, `Average`, `Aggregate`, `TryGetNonEnumeratedCount` |
| Conversion | `ToList`, `ToArray`, `ToDictionary`, `ToHashSet`, `ToLookup`, `AsEnumerable`, `AsQueryable`, `ToAsyncEnumerable` |
| Generation | `Enumerable.Range`, `Repeat`, `Empty` |

### Manner of execution

| Group | Operators |
|---|---|
| Deferred streaming | `Where`, `Select`, `Take`, `Skip`, `SelectMany`, `Concat`, `Zip`, `Distinct`, `Union` |
| Deferred buffering | `OrderBy`, `GroupBy`, `Reverse` (the whole source); `Join`, `GroupJoin`, `Except`, `Intersect` (the second sequence) |
| Immediate | `ToList`, `ToArray`, `ToDictionary`, `ToLookup`, `Count`, `Sum`, `Min`, `Max`, `First`, `Single`, `Any`, `All`, `Aggregate`, `SequenceEqual` |

### Query keywords

| Word | Purpose |
|---|---|
| `from … in` | the source and the range variable |
| `where` | a filter |
| `select` | a projection |
| `orderby … ascending/descending` | sorting |
| `group … by …` | grouping |
| `join … in … on … equals …` | a join |
| `let` | a new variable in the query |
| `into` | a query continuation |

## Gotchas

Every gotcha was verified by running it.

### A query sees changes to the source

`q = nums.Where(x => x > 1)`, then `nums.Add(4)`: iteration gave `2,3,4`. And a `ToList()` before the addition is a snapshot that does not change.

### The variable is captured by reference

A query with `limit` changed after the query was created used the new value (`5` elements instead of `3`).

### for and foreach capture differently

`for` — one variable per loop: `3,3,3`; `foreach` — a new one per iteration: `0,1,2`.

### Double enumeration

`Count()` and `Sum()` over the same query ran the source twice: `e1 e2 e1 e2`.

### OrderBy buffers, Take does not save you

`Src("a",3).OrderBy(x => -x).Take(1)` read all three elements before the first result.

### Join reads the whole second sequence

`outer.Join(inner, …).Take(1)`: the log `o1 i1 i2 i3 got1` — one outer record, three inner ones.

### An empty sequence behaves differently

`int[]`: `Max()` and `Average()` — an exception; `int?[]`: `Max()` — `null`; `string[]`: `Max()` — `null`; `Sum()` — `0`.

### Sum checks for overflow

`new[] { int.MaxValue, 1 }.Sum()` — `OverflowException`; through `long` — `2147483648`.

### A single element

`Single()` on two elements — "Sequence contains more than one element", on an empty one — "Sequence contains no elements".

### ToDictionary and duplicates

`new[]{1,1}.ToDictionary(x => x)` — `ArgumentException`: "An item with the same key has already been added. Key: 1". `ToLookup` allows duplicates.

### An error in a lambda is deferred

`Select(x => 1 / x)` is created without an error; `DivideByZeroException` — on `ToList()`.

### Argument checks: LINQ immediately, your iterator not

`Enumerable.Where<int>(null!, …)` throws at once, while your own `yield` method — only on the first `MoveNext`.

### Select with side effects and Count

`list.Select(x => { Log(); return x; }).Count()` called the lambda for every element: the `Count` optimization does not cancel the delegate call.

### yield and try/catch

`yield return` inside a `try` with a `catch` is error CS1626; a `ref` parameter in an iterator is CS1623.

## Trade-offs and alternatives

| Task | Option | Price |
|---|---|---|
| Transforming a collection | LINQ | delegates and iterators: memory allocations |
| | a `foreach` loop | faster in hot code, but more code |
| Filter + transform | `Where` + `Select` | two iterators |
| Lookup by key in a loop | `ToDictionary`/`ToLookup` once | memory for the table |
| Minimum/maximum | `MinBy`/`MaxBy` | one pass, no sorting |
| Parallel processing | PLINQ (`AsParallel`) | overhead, order not guaranteed |
| Database queries | `IQueryable` (EF Core) | limits on translatable expressions |

LINQ is written for readability; profile before replacing it with loops. For bottlenecks measure allocations ([12 · Memory and GC](../12-memory-and-gc/advanced.en.md#measuring-allocations)).

## Interview questions

**1. What is deferred execution? Give an example of a bug caused by it.**

<details><summary>Answer</summary>

A query is a description; it runs on enumeration (`foreach`, `ToList`, `Count`…), not on declaration. A bug: a query was created, then the source or a captured variable was changed — the result differs from what was expected (`2,3,4` instead of `2,3`); or a query was iterated twice and an expensive source ran twice. The solution is materialization (`ToList`).

</details>

**2. Which operators are deferred and which immediate? What are streaming and buffering?**

<details><summary>Answer</summary>

Deferred: `Where`, `Select`, `Take`, `OrderBy`, `GroupBy`, `Join`…; immediate: `ToList`, `Count`, `Sum`, `First`, `Any`. Streaming ones (`Where`, `Select`, `Distinct`) hand out elements as the source is read; buffering ones (`OrderBy`, `GroupBy`, `Reverse`) read the whole source before the first result; `Join` reads the whole second sequence (verified by the logs).

</details>

**3. How does query syntax differ from method syntax?**

<details><summary>Answer</summary>

They are equivalent: the compiler rewrites query syntax into method calls. Query syntax is convenient for `join`, several `from`s and `let`; some operators (`Take`, `Count`, `Distinct`) have no keywords. Query syntax works with any type that has the needed methods.

</details>

**4. How does `IEnumerable<T>` differ from `IQueryable<T>`?**

<details><summary>Answer</summary>

`IEnumerable` executes in memory, its lambdas are `Func<…>` delegates. `IQueryable` holds an expression (`Expression<Func<…>>`) — a data tree that a provider translates into another language (SQL). An early `AsEnumerable()` moves the rest of the query into memory. More in [15 · EF Core](../15-ef-core/advanced.en.md#iqueryable-and-ienumerable).

</details>

**5. What is an expression tree and how does it differ from a delegate?**

<details><summary>Answer</summary>

A delegate is compiled code; a tree is a data structure describing code (`x => x > 2` → a lambda with a `GreaterThan` node, a parameter and a constant) that can be analyzed, changed, translated or compiled (`Compile()`). Not everything can be written as a tree: lambdas with blocks (CS0834) and tuple literals (CS8143) cannot.

</details>

**6. How does `yield return` work?**

<details><summary>Answer</summary>

The compiler turns the method into a generated state machine class with the fields `<>1__state`, `<>2__current` and saved local variables; `MoveNext` runs code up to the next `yield return`. The body does not run when the method is called, and `finally` runs on `Dispose` (including on `break`).

</details>

**7. Why is `Count() > 0` worse than `Any()`?**

<details><summary>Answer</summary>

For sources that are not collections, `Count()` walks the whole sequence (verified: 3 steps versus 1 for `Any()`). For `ICollection<T>` both use the `Count` property, but `Any()` states the intent and is safe for lazy sources.

</details>

**8. Trick question: what does this code print?**

```csharp
var fs = new List<Func<int>>();
for (int i = 0; i < 3; i++) fs.Add(() => i);
Console.WriteLine(string.Join(",", fs.Select(f => f())));
```

<details><summary>Answer</summary>

`3,3,3`. In a `for` loop there is one variable `i` for all iterations, and the lambdas read its final value. With `foreach` the result would be `0,1,2`. The fix is a local copy inside the loop body.

</details>

**9. What happens with `new[] { int.MaxValue, 1 }.Sum()`? How to work around it?**

<details><summary>Answer</summary>

`OverflowException`: `Sum` for `int` checks for overflow. The workaround: `Sum(x => (long)x)` (the result is `2147483648`).

</details>

**10. How do you deal with empty sequences?**

<details><summary>Answer</summary>

`First`, `Single`, `Last`, `Max`/`Min`/`Average` for `int` throw `InvalidOperationException`; `FirstOrDefault`, `SingleOrDefault` return the default value (in .NET 6+ you can set your own); `Max` for `int?` and reference types — `null`; `Sum`/`Count` — `0`; `DefaultIfEmpty(x)` substitutes a value.

</details>

**11. What is the difference between `First`, `Single` and `FirstOrDefault`?**

<details><summary>Answer</summary>

`First` — the first one, an exception if empty; `Single` — the only one, an exception when empty and when there is **more than one**; `FirstOrDefault` — the first or the default value. `Single` expresses the invariant "exactly one".

</details>

**12. How do you implement a left join?**

<details><summary>Answer</summary>

The classic way: `GroupJoin` and `SelectMany` with `DefaultIfEmpty()`; in query syntax — `join … into g from x in g.DefaultIfEmpty()`. In modern versions — the `LeftJoin` operator (on .NET 11 it returned `1x` and `2-` for the missing one).

</details>

**13. How does `GroupBy` differ from `ToLookup`?**

<details><summary>Answer</summary>

`GroupBy` is deferred: it groups on enumeration. `ToLookup` is immediate: it builds a "key → elements" table with access by key (`lk["QA"]`), and for a missing key an empty sequence (verified), not an exception.

</details>

**14. Trick question: why does your own `yield` method with `ArgumentNullException.ThrowIfNull` not throw at once while the built-in `Where` does?**

<details><summary>Answer</summary>

The iterator body does not run until the first `MoveNext`, so the check is deferred. LINQ methods are split into an outer wrapper that checks arguments immediately and an inner iterator. The same trick can be used in your own code.

</details>

**15. How is `OrderBy` related to stability and how much memory does it use?**

<details><summary>Answer</summary>

`OrderBy` is stable: equal keys keep their original order (verified on `("b",1),("a",1),("c",0)` → `c,b,a`). It buffers the whole source, so it uses `O(n)` memory, and sorts on the first `MoveNext`. `OrderBy(...).First()` is better replaced with `MinBy`.

</details>

**16. What are `AggregateBy`/`CountBy` and why do they exist?**

<details><summary>Answer</summary>

.NET 9 operators: `CountBy(key)` counts elements per key without intermediate groups (`"hello".CountBy(c => c)` → `h=1, e=1, l=2, o=1`), `AggregateBy(key, seed, func)` is a fold per key. They replace `GroupBy(...).Select(g => (g.Key, g.Count()))` and save allocations.

</details>

**17. Is there asynchronous LINQ?**

<details><summary>Answer</summary>

Yes. On .NET 11 the operators over `IAsyncEnumerable<T>` are part of the platform (the assembly `System.Linq.AsyncEnumerable`): `ToAsyncEnumerable()`, `ToListAsync()` and others (verified). Before, the `System.Linq.Async` package was used for this. It is consumed through `await foreach`.

</details>

## Cheat sheet

| Need | Solution |
|---|---|
| Fix the result | `ToList()`, `ToArray()` |
| Are there any elements | `Any()` |
| Minimum/maximum by a field | `MinBy`, `MaxBy` |
| Counting by key | `CountBy`, `GroupBy` + `Count` |
| Repeated lookup by key | `ToDictionary`, `ToLookup` |
| A left join | `LeftJoin` or `GroupJoin` + `DefaultIfEmpty` |
| Flatten nested data | `SelectMany` |
| Split into batches | `Chunk(n)` |
| Stable sort | `OrderBy` |
| Do not fail on empty | `FirstOrDefault`, `DefaultIfEmpty` |
| A sum without overflow | `Sum(x => (long)x)` |
| A lazy sequence | `yield return` |

## Related materials

- Before: [basics](basics.en.md), [advanced](advanced.en.md)
- Collections: [06 · Collections and generics](../06-collections-and-generics/expert.en.md)
- Delegates and lambdas: [09 · Delegates, events, lambdas](../09-delegates-events-lambdas/overview.en.md)
- LINQ over a database: [15 · EF Core](../15-ef-core/expert.en.md)
- `async`/`await` and `await foreach`: 10 · Async/await
- Measuring allocations: [12 · Memory and GC](../12-memory-and-gc/advanced.en.md#measuring-allocations)

## Sources

- [LINQ](https://learn.microsoft.com/dotnet/csharp/linq/)
- [Translating query expressions into method calls (C# specification)](https://learn.microsoft.com/dotnet/csharp/language-reference/language-specification/expressions#127-query-expressions)
- [yield](https://learn.microsoft.com/dotnet/csharp/language-reference/statements/yield)
- [Expression trees](https://learn.microsoft.com/dotnet/csharp/advanced-topics/expression-trees/)
- [What's new in .NET 10: libraries](https://learn.microsoft.com/dotnet/core/whats-new/dotnet-10/libraries)
- [Enumerable](https://learn.microsoft.com/dotnet/api/system.linq.enumerable)
- [Queryable](https://learn.microsoft.com/dotnet/api/system.linq.queryable)
