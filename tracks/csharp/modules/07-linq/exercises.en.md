---
title: "Exercises: LINQ"
lang: en
track: csharp
kind: module
doc: exercises
slug: 07-linq
part: 1
tags: [linq, groupby, groupjoin, deferred-execution, iterator, yield]
status: draft
updated: 2026-11-22
---

# Exercises: LINQ

🌐 [Русский](exercises.ru.md) · **English**

> Solve it yourself before opening the hints. Solutions are under spoilers. The code was verified by running it; there are no runnable versions in `code/` yet (level 5 does not exist).

## 1. 🟢 The longest words

**Task:** from an array of words take the three longest among words longer than 3 characters; for equal length — alphabetically.

**Expected result:**

```text
["apple", "fig", "banana", "kiwi", "cherry", "date"] → banana,cherry,apple
```

<details><summary>Hint</summary>

`Where` → `OrderByDescending(length)` → `ThenBy(word)` → `Take(3)`.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 1: filter, order by several keys, take.
string[] words = ["apple", "fig", "banana", "kiwi", "cherry", "date"];
var top = words.Where(w => w.Length > 3)
               .OrderByDescending(w => w.Length)
               .ThenBy(w => w)
               .Take(3);
Console.WriteLine(string.Join(",", top));   // banana,cherry,apple
```

Verified: `banana,cherry,apple`. `banana` and `cherry` have 6 letters each; on a tie the alphabetical order wins.

</details>

## 2. 🟢 Grouping by first letter

**Task:** group words by first letter and print `letter:count` in letter order.

**Expected result:**

```text
["apple", "avocado", "banana", "blueberry", "cherry"] → a:2 b:2 c:1
```

<details><summary>Hint</summary>

`GroupBy(w => w[0])`, then `OrderBy(g => g.Key)` and `g.Count()`.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 2: grouping and counting.
string[] more = ["apple", "avocado", "banana", "blueberry", "cherry"];
var groups = more.GroupBy(w => w[0])
                 .OrderBy(g => g.Key)
                 .Select(g => $"{g.Key}:{g.Count()}");
Console.WriteLine(string.Join(" ", groups));   // a:2 b:2 c:1
```

Verified: `a:2 b:2 c:1`.

</details>

## 3. 🟡 Double enumeration

**Task:** the source is an iterator with an `evaluations` counter yielding `1, 2, 3`. Call `Count()` and `Sum()` over it, show the number of executions, then fix the code so that the source runs once.

**Expected result:**

```text
Without materialization: 3 6 evaluations=6
With ToList():           3 6 evaluations=3
```

<details><summary>Hint</summary>

Each terminal operator starts the deferred query anew. Keep the result: `ToList()`.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 3: deferred queries run once per terminal operator — materialize once.
int evaluations = 0;
IEnumerable<int> Source() { foreach (var i in new[] { 1, 2, 3 }) { evaluations++; yield return i; } }

var lazy = Source();
var c = lazy.Count(); var s = lazy.Sum();
Console.WriteLine($"{c} {s} evaluations={evaluations}");     // 3 6 evaluations=6

evaluations = 0;
var snap = Source().ToList();
var c2 = snap.Count; var s2 = snap.Sum();
Console.WriteLine($"{c2} {s2} evaluations={evaluations}");   // 3 6 evaluations=3
```

Verified: `evaluations=6` and `evaluations=3`. For a database query, double enumeration means two queries.

</details>

## 4. 🟡 Totals by customer

**Task:** given customers `(Id, Name)` and orders `(CustomerId, Amount)`. Print each customer's order total, **including customers with no orders** (total 0), by descending total, then by name.

**Expected result:**

```text
customers Ann(1), Bob(2), Cid(3); orders: 1→10, 1→5, 2→7
→ Ann=15, Bob=7, Cid=0
```

<details><summary>Hint</summary>

`Join` would lose customers without orders — you need `GroupJoin`: for each customer it yields their orders (possibly an empty group).

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 4: GroupJoin keeps customers that have no orders.
record Customer(int Id, string Name);
record Order(int CustomerId, decimal Amount);

var customers = new[] { new Customer(1, "Ann"), new Customer(2, "Bob"), new Customer(3, "Cid") };
var orders = new[] { new Order(1, 10m), new Order(1, 5m), new Order(2, 7m) };

var totals = customers
    .GroupJoin(orders, c => c.Id, o => o.CustomerId,
               (c, os) => new { c.Name, Total = os.Sum(o => o.Amount) })
    .OrderByDescending(x => x.Total)
    .ThenBy(x => x.Name);

foreach (var t in totals) Console.WriteLine($"{t.Name}={t.Total}");
```

Verified: `Ann=15`, `Bob=7`, `Cid=0`. `Sum` over an empty group gives `0`.

</details>

## 5. 🔴 Your own Batch operator

**Task:** write `IEnumerable<IReadOnlyList<T>> Batch<T>(IEnumerable<T> source, int size)` that cuts a sequence into batches of `size` (the last may be shorter). Argument validation must fire **immediately at the call**, not during enumeration.

**Expected result:**

```text
Batch(1..7, 3)     → 1,2,3 | 4,5,6 | 7
Batch(1..3, 0)     → ArgumentOutOfRangeException immediately (ParamName = "size")
```

<details><summary>Hint</summary>

A method with `yield` defers even the checks. Split it: an outer method validates the arguments and returns a call to a local iterator.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 5: a custom LINQ operator with eager argument validation.
static IEnumerable<IReadOnlyList<T>> Batch<T>(IEnumerable<T> source, int size)
{
    ArgumentNullException.ThrowIfNull(source);
    ArgumentOutOfRangeException.ThrowIfLessThan(size, 1);
    return Iterate(source, size);

    static IEnumerable<IReadOnlyList<T>> Iterate(IEnumerable<T> src, int size)
    {
        var buf = new List<T>(size);
        foreach (var x in src)
        {
            buf.Add(x);
            if (buf.Count == size) { yield return buf; buf = new List<T>(size); }
        }
        if (buf.Count > 0) yield return buf;
    }
}
```

Verified: `1,2,3 | 4,5,6 | 7` and an immediate exception for `size = 0`. .NET already has `Enumerable.Chunk`; the exercise shows the "wrapper + iterator" technique.

</details>
