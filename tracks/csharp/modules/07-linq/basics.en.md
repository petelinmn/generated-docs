---
title: "LINQ: basics"
lang: en
track: csharp
kind: module
doc: basics
slug: 07-linq
part: 1
tags: [linq, query-syntax, method-syntax, deferred-execution, where, select, orderby, groupby, lambda, ienumerable]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-22
---

# LINQ: basics

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [06 · Collections and generics](../06-collections-and-generics/overview.en.md), [03 · Control flow and methods](../03-control-flow-and-methods/overview.en.md).

## Why it matters

LINQ (Language Integrated Query) is a single way to query data: filter, transform, sort, group and aggregate **any sequences** — arrays, lists, dictionaries, XML and databases ([15 · EF Core](../15-ef-core/overview.en.md)). Instead of loops with temporary lists you describe **what** you want to get:

```csharp
var people = new[] { new P("Ann", "IT", 30), new P("Bob", "HR", 25), new P("Cid", "IT", 35) };

var names = people
    .Where(p => p.Age > 26)
    .OrderByDescending(p => p.Age)
    .Select(p => p.Name)
    .ToList();                                  // Cid, Ann

record P(string Name, string Dept, int Age);
```

LINQ works with any `IEnumerable<T>`; the methods live in the `System.Linq` namespace (it is imported by the implicit `using`s in new projects).

## Lambda expressions

Queries take **lambdas** — short anonymous functions `parameters => expression`:

```csharp
Func<int, bool> isEven = x => x % 2 == 0;
Console.WriteLine(isEven(4));    // True
```

`p => p.Age > 26` reads "for each `p` — return whether the age is greater than 26". More in [09 · Delegates, events, lambdas](../09-delegates-events-lambdas/overview.en.md).

## The main operators

| Task | Operator | Example |
|---|---|---|
| Filter | `Where` | `people.Where(p => p.Dept == "IT")` |
| Transform | `Select` | `people.Select(p => p.Name)` |
| Sort | `OrderBy`, `OrderByDescending`, `ThenBy` | `people.OrderBy(p => p.Age).ThenBy(p => p.Name)` |
| Take / skip | `Take`, `Skip`, `TakeWhile`, `SkipWhile` | `list.Skip(10).Take(5)` |
| One element | `First`, `FirstOrDefault`, `Single`, `Last` | `people.FirstOrDefault(p => p.Age > 30)` |
| Check | `Any`, `All`, `Contains` | `people.Any(p => p.Dept == "HR")` |
| Count | `Count`, `Sum`, `Min`, `Max`, `Average` | `people.Sum(p => p.Age)` |
| Remove duplicates | `Distinct`, `DistinctBy` | `people.Select(p => p.Dept).Distinct()` |
| Group | `GroupBy` | `people.GroupBy(p => p.Dept)` |
| Combine | `Concat`, `Union`, `Intersect`, `Except`, `Zip` | `a.Union(b)` |
| Join | `Join`, `GroupJoin` | see [advanced](advanced.en.md#grouping-and-joins) |
| Flatten | `SelectMany` | `orders.SelectMany(o => o.Items)` |
| Into a collection | `ToList`, `ToArray`, `ToDictionary`, `ToHashSet` | `query.ToList()` |

Operators are **chained**: the result of one is the input of the next.

### Choosing one element

| Method | If there is no element | If there is more than one |
|---|---|---|
| `First()` | `InvalidOperationException` | the first |
| `FirstOrDefault()` | the default value (`null`/`0`) | the first |
| `Single()` | `InvalidOperationException` | `InvalidOperationException` |
| `SingleOrDefault()` | the default value | `InvalidOperationException` |

Verified: `First()` and `Single()` on an empty sequence throw "Sequence contains no elements", while `Single()` on two elements throws "Sequence contains more than one element". Since .NET 6 `FirstOrDefault` has an overload with your own default value: `empty.FirstOrDefault(-1)` returned `-1`.

## Deferred execution

The main property of LINQ: **a query is a description, and it does not run until the result is needed**.

```csharp
var nums = new List<int> { 1, 2, 3 };
var q = nums.Where(x => x > 1);      // nothing has run yet
nums.Add(4);
Console.WriteLine(string.Join(",", q));   // 2,3,4 — the query sees the later Add
```

A query runs on **enumeration**: `foreach`, `ToList()`, `Count()`, `First()`, `Sum()` and so on. Every enumeration runs the query again. To fix the result, materialize it: `ToList()` or `ToArray()`:

```csharp
var snap = nums.Where(x => x > 1).ToList();   // a snapshot: 2,3,4
nums.Add(5);
Console.WriteLine(snap.Count);                // 3 — the list does not change
Console.WriteLine(q.Count());                 // 4 — the query is evaluated again
```

More on which operators are deferred and which immediate in [advanced](advanced.en.md#deferred-and-immediate-execution).

## Query syntax

Besides methods, there is an SQL-like **query syntax**. Both forms are equivalent: the compiler translates query syntax into method calls.

```csharp
var result =
    from p in people
    where p.Age > 26
    orderby p.Age descending
    select p.Name.ToUpper();          // CID, ANN
```

The main keywords: `from`, `where`, `select`, `orderby`, `group … by`, `join … on … equals`, `let`, `into`. Some operators (`Take`, `Skip`, `Distinct`, `Count`, …) have no keyword in query syntax and are called as methods: `(from p in people select p.Age).Max()`. Method syntax is usually used; query syntax is convenient for joins and multiple `from`s.

## Grouping

```csharp
var byDept = people.GroupBy(p => p.Dept);
foreach (var g in byDept)
    Console.WriteLine($"{g.Key}: {g.Count()} — {string.Join("/", g.Select(p => p.Name))}");
// IT: 2 — Ann/Cid
// HR: 1 — Bob
```

`GroupBy` returns a sequence of groups: each has a `Key` and elements. The order of groups follows the first appearance of the key (`IT`, then `HR`).

## Materializing into collections

```csharp
List<string> list = people.Select(p => p.Name).ToList();
Dictionary<string, int> ages = people.ToDictionary(p => p.Name, p => p.Age);
HashSet<string> depts = people.Select(p => p.Dept).ToHashSet();
```

`ToDictionary` throws `ArgumentException` if a key repeats (verified: "An item with the same key has already been added"). For several values per key use `ToLookup`.

## Beginner mistakes

- ❌ Thinking the query has run at the moment it is declared → ✅ it runs on enumeration; use `ToList()` for a snapshot.
- ❌ Enumerating one query several times → ✅ every pass runs the query and its side effects again: store it in a list.
- ❌ `First()` on a possibly empty sequence → ✅ `FirstOrDefault()` and a check.
- ❌ `Count() > 0` just to check for presence → ✅ `Any()`: it stops at the first element.
- ❌ `Select` with side effects (writing, calling an API) → ✅ queries only read and transform; actions go in `foreach`.
- ❌ `ToList()` too early in the chain → ✅ materialize at the end.

## Related materials

- Next: [advanced](advanced.en.md) — streaming and buffering, closures, joins, `yield` iterators, `IQueryable`
- Collections and their interfaces: [06 · Collections and generics](../06-collections-and-generics/overview.en.md)
- Lambdas and delegates: [09 · Delegates, events, lambdas](../09-delegates-events-lambdas/overview.en.md)
- LINQ to a database: [15 · EF Core](../15-ef-core/basics.en.md#queries-linq-becomes-sql)
- The same in other tracks: [Python](../../../python/overview.en.md) — list comprehensions, `itertools`, `map`/`filter`

## Sources

- [LINQ overview](https://learn.microsoft.com/dotnet/csharp/linq/)
- [Query syntax and method syntax](https://learn.microsoft.com/dotnet/csharp/linq/get-started/query-expression-basics)
- [Standard query operators](https://learn.microsoft.com/dotnet/csharp/linq/standard-query-operators/)
- [Enumerable](https://learn.microsoft.com/dotnet/api/system.linq.enumerable)
