---
title: "LINQ: advanced"
lang: en
track: csharp
kind: module
doc: advanced
slug: 07-linq
part: 1
tags: [linq, deferred-execution, streaming, buffering, closures, group-join, selectmany, iterators, yield, iqueryable, expression-trees]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-22
---

# LINQ: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

## Deferred and immediate execution

| Kind | Operators | When they run |
|---|---|---|
| **Deferred, streaming** | `Where`, `Select`, `Take`, `Skip`, `SelectMany`, `Concat`, `Zip`, `TakeWhile`, `Append`, `Distinct`, `Union` | on iteration, element by element (`Distinct` and `Union` remember values already seen) |
| **Deferred, buffering** | `OrderBy`, `GroupBy`, `Reverse` — the whole source; `Join`, `GroupJoin`, `Except`, `Intersect` — the whole **second** sequence | on the first `MoveNext`, but they read the sequence completely first |
| **Immediate** | `ToList`, `ToArray`, `ToDictionary`, `Count`, `Sum`, `Min`, `Max`, `First`, `Any`, `All`, `Aggregate` | right at the call |

The difference shows in the order of calls. A source and two streaming operators process elements one by one (verified by the log):

```csharp
var s = Src("a", 3)
    .Where(x => { log.Add($"w{x}"); return true; })
    .Select(x => { log.Add($"s{x}"); return x; });

foreach (var x in s.Take(2)) log.Add($"got{x}");
// a1 w1 s1 got1 a2 w2 s2 got2   — the third element is never read
```

Each element passes through the whole chain before the next one is read, and `Take(2)` stops reading after the second. The buffering `OrderBy`, by contrast, reads everything first:

```csharp
foreach (var x in Src("a", 3).OrderBy(x => -x).Take(1)) log.Add($"got{x}");
// a1 a2 a3 got3   — all three are read before the first result
```

Verified for other operators too: `Distinct().Take(1)` read one source element; `Join` read the first element of the outer sequence and **all** of the inner one (`o1 i1 i2 i3`); `GroupBy` and `Reverse` read the whole source; `Except` read the whole second sequence (`x1 x2`), then streams the first.

The consequences: for infinite sources and large files use streaming operators; `OrderBy(...).First()` reads the whole sequence, while `MinBy`/`MaxBy` also make one pass without sorting and are usually preferable.

## Multiple enumeration

Each iteration of a query runs it again, with its side effects and the cost of the source:

```csharp
var src3 = Src("e", 2).Select(x => x * 2);
_ = src3.Count();
_ = src3.Sum();
// log: e1 e2 e1 e2 — the source was enumerated twice
```

If you need the result several times, materialize it: `var list = query.ToList();`. For an `IEnumerable<T>` from an API that may be "lazy" (a database query, reading a file), multiple enumeration is a frequent cause of extra queries (analyzers warn about it: CA1851).

## Closures: the variable is read at execution time

A query captures the **variable**, not its value at the moment the query was created:

```csharp
int limit = 2;
var q2 = nums.Where(x => x > limit);
limit = 4;
Console.WriteLine(string.Join(",", q2));   // uses limit == 4
```

The second example is a loop. A `for` has one variable `i` for the whole loop, so closures see its final value, while a `foreach` creates a new variable on each iteration:

```csharp
var fs = new List<Func<int>>();
for (int i = 0; i < 3; i++) fs.Add(() => i);          // 3,3,3

var fs2 = new List<Func<int>>();
foreach (var i in new[] { 0, 1, 2 }) fs2.Add(() => i);  // 0,1,2
```

Verified: `for` gave `3,3,3`, `foreach` gave `0,1,2`. The fix for `for`: copy the value into a local variable inside the loop body.

## Shortcuts and emptiness checks

`Count()`, `Any()`, `ToList()` and `Last()` use knowledge about the source. Verified on a custom collection that counts accesses:

| Call | For an `ICollection<T>` |
|---|---|
| `Count()` | takes the `Count` property, **no** enumeration |
| `Any()` | takes `Count`, no enumeration |
| `ToList()` | copies through `CopyTo` rather than walking element by element |
| `Last()` | for a non-`IList<T>` enumerates the whole sequence |

For an iterator that is not a collection, `Count()` has to walk the entire sequence (verified: 3 steps), while `Any()` stops after the first (1 step). So for "are there any elements" always use `Any()`. The method `TryGetNonEnumeratedCount(out n)` returns the number only if it can be learned without iterating: `True` for a list, `False` for an iterator.

## Empty sequences and edge cases

| Call on an empty `int[]` | Result |
|---|---|
| `First()`, `Single()`, `Last()` | `InvalidOperationException`: "Sequence contains no elements" |
| `Max()`, `Min()`, `Average()` (for `int`) | `InvalidOperationException` |
| `Max()` for `int?` and for reference types (`string`) | `null` without an exception |
| `Sum()`, `Count()` | `0` |
| `FirstOrDefault()` / `FirstOrDefault(-1)` | `0` / `-1` |
| `DefaultIfEmpty(7).First()` | `7` |
| `ElementAt(5)` out of range | `ArgumentOutOfRangeException`; `ElementAtOrDefault(5)` — `0` |

`Sum` for `int` **checks for overflow**: `new[] { int.MaxValue, 1 }.Sum()` throws `OverflowException`. For large sums cast to `long`: `Sum(x => (long)x)` gave `2147483648`.

## Grouping and joins

```csharp
var lk = people.ToLookup(p => p.Dept);
Console.WriteLine(lk["QA"].Count());    // 0 — a missing key gives an empty sequence, not an exception

var joined = people.Join(depts, p => p.Dept, d => d.Code, (p, d) => $"{p.Name}-{d.Title}");
// Ann-Tech, Cid-Tech — only matching pairs (inner join)
```

- `GroupBy` is deferred, and groups come in the order of the key's first appearance; `ToLookup` is immediate, with lookup by key.
- `Join` is an inner join (matching pairs only). For a **left join**: `GroupJoin` + `SelectMany`/`DefaultIfEmpty`, and in modern versions the `LeftJoin` operator (verified on .NET 11: `new[]{1,2}.LeftJoin(…)` returned `1x` and `2-` for the missing one).

```csharp
var left = depts.GroupJoin(people, d => d.Code, p => p.Dept, (d, ps) => $"{d.Code}:{ps.Count()}");
// IT:2, QA:0 — every department, even without people
```

- `SelectMany` flattens nested sequences: `new[]{"ab","cd"}.SelectMany(s => s)` → `a,b,c,d`; with a result selector you get Cartesian pairs (`11,21,12,22`).
- `Aggregate` is a fold: `new[]{1,2,3,4}.Aggregate((a,b) => a*b)` → `24`; with a seed — `Aggregate("", (acc, s) => acc + s)`.

## Operators from recent versions

All of those listed work on .NET 11 (verified by running):

| Operator | Example and result |
|---|---|
| `Chunk(n)` | `Range(1,5).Chunk(2)` → `[1,2] [3,4] [5]` |
| `MinBy`, `MaxBy` | `MinBy(s => s.Length)` → the shortest element |
| `DistinctBy`, `UnionBy`, `ExceptBy`, `IntersectBy` | `DistinctBy(s => s.Length)` |
| `Order()`, `OrderDescending()` | sorting without a key selector |
| `CountBy(key)` | `"hello".CountBy(c => c)` → `h=1, e=1, l=2, o=1` |
| `AggregateBy(key, seed, func)` | an aggregate per group without `GroupBy` |
| `Index()` | `["a","b"].Index()` → `(0,a), (1,b)` |
| `Zip` of three sequences | tuples `(First, Second, Third)` |
| `Take(1..3)`, `Take(^2..)` | ranges; `ElementAt(^1)` is the last |
| `LeftJoin`, `RightJoin` | joins preserving the left/right side |

## Iterators and yield

A method with `yield return` is an **iterator**: it returns `IEnumerable<T>`, but the code in it does not run until iteration, and execution is suspended at each `yield return`:

```csharp
IEnumerable<int> GenFinally()
{
    try { yield return 1; yield return 2; }
    finally { log.Add("finally"); }
}

foreach (var x in GenFinally()) { log.Add($"x{x}"); break; }
// x1 finally — breaking out of the loop disposes the enumerator, and the finally block runs
```

- `yield break` ends the sequence.
- An iterator produces values lazily, so it suits infinite sequences and data streams.
- In an iterator you cannot use `ref`, `in`, `out` parameters (error CS1623) or `yield` inside a `try` with a `catch` (CS1626); inside `try`/`finally` it is allowed (verified).

### Argument checks: immediate and deferred

LINQ methods check their arguments **immediately**: `Enumerable.Where<int>(null, …)` throws `ArgumentNullException` at the moment of the call (verified). A custom iterator with `yield` runs code only on the first `MoveNext`, so the exception appears later:

```csharp
static IEnumerable<T> MyWhere<T>(IEnumerable<T> src, Func<T, bool> p)
{
    ArgumentNullException.ThrowIfNull(src);      // runs only on the first MoveNext
    foreach (var x in src) if (p(x)) yield return x;
}

var lazyBad = MyWhere<int>(null!, x => true);    // no exception yet
foreach (var _ in lazyBad) { }                   // ArgumentNullException here
```

To check immediately, split the method: the outer one (with the check) returns the result of an inner iterator. Errors in lambdas are deferred too: `Select(x => 1 / x)` is created without an error, and `DivideByZeroException` appears on `ToList()`.

## IEnumerable and IQueryable

| | `IEnumerable<T>` | `IQueryable<T>` |
|---|---|---|
| Where it runs | in memory, in your process | at the provider (a database, etc.) |
| Lambda parameter | `Func<T, bool>` (a delegate) | `Expression<Func<T, bool>>` (an expression tree) |
| What the provider does | calls the delegate | parses the tree and translates it, for example into SQL |

A lambda becomes an **expression tree** if the parameter has the type `Expression<…>`:

```csharp
Expression<Func<int, bool>> ex = x => x > 2;
Func<int, bool> fn = x => x > 2;
Console.WriteLine(ex);                         // x => (x > 2)
Console.WriteLine(ex.Body.NodeType);           // GreaterThan
Console.WriteLine(ex.Compile()(3));            // True
```

`AsQueryable()` over an in-memory collection gives an `EnumerableQuery`: it executes the tree as ordinary LINQ to Objects (verified: `qry.Provider` is `EnumerableQuery`1`). A real `IQueryable` is the `DbSet` from [15 · EF Core](../15-ef-core/advanced.en.md#what-ef-can-translate-to-sql): there an untranslatable method in `Where` leads to an exception, and an early `AsEnumerable()` moves filtering into memory. In expressions you cannot use a tuple literal (error CS8143) or lambdas with a block body (error CS0834).

## Best practices

- Write chains readably: one operator per line; move complex conditions into named methods.
- Filter (`Where`) **before** sorting and projection: less data in the buffer.
- `Any()` instead of `Count() > 0`; `FirstOrDefault` with a check instead of `First` on possibly empty data.
- Materialize (`ToList`/`ToArray`) if the result is needed more than once.
- No side effects in `Where`/`Select`.
- In hot code and with large volumes, compare with a plain loop: LINQ creates delegates and iterators.
- For membership checks use a `HashSet`, not `Contains` on a list inside a query.

## Common mistakes

- ❌ Multiple enumeration of a "lazy" source → ✅ materialization.
- ❌ `OrderBy(...).First()` for a minimum → ✅ `MinBy`.
- ❌ A closure over a `for` loop variable → ✅ a copy in a local variable.
- ❌ `Sum` for `int` on large data → ✅ `Sum(x => (long)x)`.
- ❌ `Count()` on an `IEnumerable` from a database in a loop → ✅ `Any()`, one query.
- ❌ Expecting immediate argument checks in your own iterators → ✅ an outer method with the check and an inner iterator.

## Comparison with other tracks

| | C# LINQ | Python |
|---|---|---|
| Filter and projection | `Where`, `Select` | list comprehension `[f(x) for x in xs if p(x)]`, `filter`, `map` |
| Laziness | deferred execution by default | generator expressions are lazy, list ones are not |
| Grouping | `GroupBy` | `itertools.groupby` (requires sorting) |
| Generators | `yield return` | `yield` |
| Database queries | `IQueryable`, an expression tree | SQLAlchemy `select()` |

## Related materials

- Before: [basics](basics.en.md)
- Next: [expert](expert.en.md) — translation of query syntax, the iterator state machine, an operator reference, interview questions
- Collections, `Count` and `Sort`: [06 · Collections and generics](../06-collections-and-generics/advanced.en.md)
- LINQ over a database, translation to SQL, client evaluation: [15 · EF Core](../15-ef-core/advanced.en.md)
- Closures and delegates: [09 · Delegates, events, lambdas](../09-delegates-events-lambdas/overview.en.md)
- Iterators and loops: [03 · Control flow and methods](../03-control-flow-and-methods/expert.en.md)

## Sources

- [Deferred execution and lazy evaluation](https://learn.microsoft.com/dotnet/standard/linq/deferred-execution-lazy-evaluation)
- [Standard query operators: classification by manner of execution](https://learn.microsoft.com/dotnet/csharp/linq/standard-query-operators/#classification-of-standard-query-operators-by-manner-of-execution)
- [Join operations](https://learn.microsoft.com/dotnet/csharp/linq/standard-query-operators/join-operations)
- [yield](https://learn.microsoft.com/dotnet/csharp/language-reference/statements/yield)
- [Expression trees](https://learn.microsoft.com/dotnet/csharp/advanced-topics/expression-trees/)
- [Enumerable.Sum](https://learn.microsoft.com/dotnet/api/system.linq.enumerable.sum)
- [Analyzer CA1851](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1851)
