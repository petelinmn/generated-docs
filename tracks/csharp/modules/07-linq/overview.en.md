---
title: "LINQ"
lang: en
track: csharp
kind: module
doc: overview
slug: 07-linq
part: 1
levels: [1, 2, 3, 4]
tags: [linq, query-syntax, method-syntax, deferred-execution, iterators, yield, expression-trees, iqueryable, group-join, closures]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-22
---

# LINQ

🌐 [Русский](overview.ru.md) · **English**

## In a nutshell

LINQ (Language Integrated Query) is a single way to query data from any sequences: filter, transform, sort, group, join and aggregate. Queries are written as a chain of methods (`Where`, `Select`, `OrderBy`, …) or in an SQL-like syntax, and in `IQueryable` the same expressions are translated into SQL. The key idea is deferred execution: a query runs on enumeration, not on declaration.

## Key ideas

- **Operators are extension methods** of `IEnumerable<T>`; the compiler rewrites query syntax into method calls.
- **Deferred execution:** a query sees changes to the source and to captured variables; `ToList()` fixes a snapshot.
- **Streaming and buffering:** `Where`/`Select` hand out elements one by one, `OrderBy`/`GroupBy` read everything, `Join` — the whole second sequence.
- **Multiple enumeration** runs the query and its source again: materialize the result.
- **Empty data:** `First`/`Single`/`Max` throw exceptions, `FirstOrDefault` and `Max` for `int?` do not; `Sum` for `int` checks for overflow.
- **`yield` iterators** are state machines: lazy, with argument checks deferred.
- **`IEnumerable` or `IQueryable`:** an in-memory delegate or an expression tree for a provider (EF Core).

## Minimal example

```csharp
var people = new[] { new P("Ann", "IT", 30), new P("Bob", "HR", 25), new P("Cid", "IT", 35) };

var names = people
    .Where(p => p.Age > 26)
    .OrderByDescending(p => p.Age)
    .Select(p => p.Name)
    .ToList();                                  // Cid, Ann

record P(string Name, string Dept, int Age);
```

## When to use and when not

- ✅ Filtering, projecting, grouping and joining collections readably.
- ✅ Database queries through EF Core.
- ✅ Lazy sequences (`yield`).
- ❌ Side effects in `Where`/`Select`.
- ❌ Multiple enumeration of a "lazy" source without materialization.
- ❌ LINQ in the hottest code without measuring — a loop may be faster.

## Levels of this material

| Level | What's inside |
|---|---|
| [1. Basics](basics.en.md) | lambdas, the main operators, choosing an element, deferred execution, query syntax, grouping, materialization |
| [2. Advanced](advanced.en.md) | streaming and buffering, multiple enumeration, closures, empty sequences, joins, new operators, iterators, `IQueryable` |
| [3. Expert](expert.en.md) | translation of query syntax, the iterator state machine, `Enumerable` optimizations, expression trees, asynchronous LINQ, reference, gotchas, 17 interview questions |
| [4. Self-check](quiz.en.md) | 11 questions and [5 exercises](exercises.en.md) with solutions |
| 5. Runnable examples | not generated yet — `/learn csharp 07-linq level 5` |
| 6. Tests | not generated yet — `/learn csharp 07-linq level 6` |

## Related materials

- Prerequisites: [06 · Collections and generics](../06-collections-and-generics/overview.en.md), [09 · Delegates, events, lambdas](../09-delegates-events-lambdas/overview.en.md)
- LINQ to a database: [15 · EF Core](../15-ef-core/overview.en.md)
- Measuring allocations: [12 · Memory and GC](../12-memory-and-gc/overview.en.md)
- `IAsyncEnumerable<T>` and asynchronous streams: [10 · Async/await](../10-async-await/overview.en.md)
- Patterns and extension members: [11 · Modern C#](../11-modern-csharp/overview.en.md)
- The same in other tracks: [Python](../../../python/overview.en.md) — list comprehensions, `itertools`
