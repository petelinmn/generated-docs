---
title: "Quiz: LINQ"
lang: en
track: csharp
kind: module
doc: quiz
slug: 07-linq
part: 1
tags: [linq, deferred-execution, closures, iterator, iqueryable, yield]
status: draft
updated: 2026-11-22
---

# Quiz: LINQ

🌐 [Русский](quiz.ru.md) · **English**

> Answer first, then open the answer. The mark shows which level a question comes from: 🟢 [basics](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 What does the code print?

```csharp
var nums = new List<int> { 1, 2, 3 };
var q = nums.Where(x => x > 1);
nums.Add(4);
Console.WriteLine(string.Join(",", q));
```

- A) `2,3`
- B) `2,3,4`
- C) `1,2,3,4`
- D) a run-time error

<details><summary>Answer</summary>

**B.** `Where` does not run when the query is created — it runs on enumeration (deferred execution), so the query sees the added element (verified). A `ToList()` before the addition would give a snapshot `2,3`.
→ [Basics](basics.en.md#deferred-execution)

</details>

## 2. 🟢 Which calls execute the query immediately?

- [ ] A) `Where`
- [ ] B) `ToList`
- [ ] C) `Count`
- [ ] D) `Select`

<details><summary>Answer</summary>

**B, C.** `ToList`, `ToArray`, `Count`, `Sum`, `First` and the like start enumeration at once. `Where` and `Select` only describe the query and run later.
→ [Basics](basics.en.md#materializing-into-collections)

</details>

## 3. 🟢 What does `Single()` throw on a sequence of two elements?

- A) returns the first element
- B) `InvalidOperationException` ("Sequence contains more than one element")
- C) `null`
- D) `ArgumentException`

<details><summary>Answer</summary>

**B.** `Single` requires exactly one element: with two — "more than one element", on an empty one — "no elements" (verified). `First` takes the first, `FirstOrDefault` returns the default value on an empty sequence.
→ [Basics](basics.en.md#choosing-one-element)

</details>

## 4. 🟢 What does `GroupBy(w => w[0])` do over a list of words?

- A) sorts the words by first letter
- B) returns a sequence of groups: the key is the first letter, the elements are the words starting with it
- C) removes duplicates
- D) joins the words into one string

<details><summary>Answer</summary>

**B.** Each group is an `IGrouping<char, string>` with a `Key`. For sorting there is `OrderBy`, for duplicates — `Distinct`.
→ [Basics](basics.en.md#grouping)

</details>

## 5. 🟡 How many times does the source run?

```csharp
int evaluations = 0;
IEnumerable<int> Source() { foreach (var i in new[] { 1, 2, 3 }) { evaluations++; yield return i; } }

var lazy = Source();
var c = lazy.Count();
var s = lazy.Sum();
Console.WriteLine($"{c} {s} evaluations={evaluations}");
```

- A) `evaluations=3`
- B) `evaluations=6`
- C) `evaluations=1`
- D) `evaluations=0`

<details><summary>Answer</summary>

**B.** Each terminal operator enumerates the deferred query anew: `Count()` and `Sum()` gave 3 + 3 (verified). If you need the result several times, materialize once: `var snap = Source().ToList();` (then `evaluations=3`).
→ [Advanced theory](advanced.en.md#multiple-enumeration)

</details>

## 6. 🟡 What does the code print?

```csharp
int limit = 3;
var q = Enumerable.Range(1, 10).Where(x => x <= limit);
limit = 5;
Console.WriteLine(q.Count());
```

- A) `3`
- B) `5`
- C) `10`
- D) `0`

<details><summary>Answer</summary>

**B.** The lambda captures the **variable** `limit`, not its value, and reads it when the query runs, by which time `limit` is `5` (verified).
→ [Advanced theory](advanced.en.md#closures-the-variable-is-read-at-execution-time)

</details>

## 7. 🟡 What does an empty sequence return?

- [ ] A) `new int[0].Max()` — an exception
- [ ] B) `new int?[0].Max()` — `null`
- [ ] C) `new int[0].Sum()` — `0`
- [ ] D) `new int[0].Average()` — `0`

<details><summary>Answer</summary>

**A, B, C.** `Max` and `Average` for `int` on an empty sequence throw `InvalidOperationException`, for `int?` (and reference types) `Max` returns `null`, `Sum` returns `0` (verified). D is wrong: `Average` throws.
→ [Advanced theory](advanced.en.md#empty-sequences-and-edge-cases)

</details>

## 8. 🟡 What do the two loops print?

```csharp
var a = new List<Func<int>>();
for (int i = 0; i < 3; i++) a.Add(() => i);

var b = new List<Func<int>>();
foreach (var j in Enumerable.Range(0, 3)) b.Add(() => j);

Console.WriteLine(string.Join(",", a.Select(f => f())));
Console.WriteLine(string.Join(",", b.Select(f => f())));
```

<details><summary>Answer</summary>

```text
3,3,3
0,1,2
```

In `for` there is one variable `i` for the whole loop — all closures see the final value `3`. In `foreach` the variable is created anew on each iteration (verified).
→ [Expert theory](expert.en.md#for-and-foreach-capture-differently)

</details>

## 9. 🟡 Open question: how does `IEnumerable<T>` differ from `IQueryable<T>` and what is dangerous about it?

<details><summary>Key points of a good answer</summary>

- `IEnumerable<T>` runs lambdas in memory as delegates; `IQueryable<T>` keeps an expression tree (`Expression`) that a provider (for example EF Core) translates to SQL;
- in `IQueryable` a `Where` can use only what the provider can translate; ordinary C# methods inside do not work;
- calling `AsEnumerable()`/`ToList()` too early moves filtering into memory and loads extra data;
- the combination: filters and projections on the database side, final processing in memory ([15 · EF Core](../15-ef-core/overview.en.md)).

</details>

## 10. 🔴 How many elements does the source read before the first result?

```csharp
Src("a", 3).OrderBy(x => -x).Take(1);   // Src yields 3 elements
```

- A) 1
- B) 3
- C) 2
- D) 0

<details><summary>Answer</summary>

**B.** `OrderBy` buffers the whole sequence before yielding the first element — `Take(1)` does not help (verified). Likewise `Join` reads the whole second sequence (log `o1 i1 i2 i3 got1`). The lazy "streaming" operators are `Where`, `Select`, `Take`.
→ [Expert theory](expert.en.md#orderby-buffers-take-does-not-save-you)

</details>

## 11. 🔴 Find the bug

```csharp
static IEnumerable<int> Read(IEnumerable<int> src)
{
    ArgumentNullException.ThrowIfNull(src);
    try { foreach (var x in src) yield return x; }
    catch (Exception) { yield break; }
}
```

<details><summary>Answer</summary>

Two problems. First, `yield return` inside a `try` with a `catch` is compile error `CS1626`. Second, even without it the argument check in an iterator is deferred until the first `MoveNext()` rather than running at the call (unlike LINQ operators, which check at once). The solution: split the method into a wrapper that checks the arguments and returns a local iterator.
→ [Expert theory](expert.en.md#yield-and-trycatch)

</details>
