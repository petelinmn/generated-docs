---
title: "Exercises: Collections and generics"
lang: en
track: csharp
kind: module
doc: exercises
slug: 06-collections-and-generics
part: 1
tags: [dictionary, hashset, list, generics, constraints, lru-cache]
status: draft
updated: 2026-11-22
---

# Exercises: Collections and generics

🌐 [Русский](exercises.ru.md) · **English**

> Solve it yourself before opening the hints. Solutions are under spoilers. The code was verified by running it; there are no runnable versions in `code/` yet (level 5 does not exist).

## 1. 🟢 Word frequency

**Task:** write `Dictionary<string, int> WordFrequency(string text)`: it counts words case-insensitively; the separators are a space and the characters `, . ! ?`.

**Expected result:**

```text
"The cat and the hat. The end!" → The=3, and=1, cat=1, end=1, hat=1
```

(the key keeps the spelling of the first occurrence — `The`.)

<details><summary>Hint</summary>

`new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)` and `freq[w] = freq.GetValueOrDefault(w) + 1`.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 1: counting with a dictionary and a case-insensitive comparer.
static Dictionary<string, int> WordFrequency(string text)
{
    var freq = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
    foreach (var w in text.Split([' ', ',', '.', '!', '?'], StringSplitOptions.RemoveEmptyEntries))
        freq[w] = freq.GetValueOrDefault(w) + 1;
    return freq;
}
```

Verified: after sorting by descending frequency and then by key, the output matches the table.

</details>

## 2. 🟢 Unique elements preserving order

**Task:** write a generic `List<T> Distinct<T>(IEnumerable<T> src)` that removes duplicates and keeps the order of first occurrence. Without LINQ.

**Expected result:**

```text
Distinct([3, 1, 3, 2, 1]) → 3,1,2
```

<details><summary>Hint</summary>

`HashSet<T>.Add` returns `true` only if the element was not there yet.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 2: de-duplicate while preserving order.
static List<T> Distinct<T>(IEnumerable<T> src) where T : notnull
{
    var seen = new HashSet<T>();
    var result = new List<T>();
    foreach (var x in src)
        if (seen.Add(x)) result.Add(x);
    return result;
}
```

Verified: `3,1,2`. The set gives an O(1) check, the list gives the order.

</details>

## 3. 🟡 Safe removal while iterating

**Task:** in the list `1..6` the even numbers must be removed. Show that `foreach` + `Remove` throws an exception, and do the removal correctly.

**Expected result:**

```text
foreach + Remove → InvalidOperationException
RemoveAll        → removed 3, left 1,3,5
```

<details><summary>Hint</summary>

`List<T>` has `RemoveAll(Predicate<T>)`, which returns the number of removed elements.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 3: never remove from a list inside foreach; use RemoveAll.
var nums = new List<int> { 1, 2, 3, 4, 5, 6 };
try { foreach (var n in nums) if (n % 2 == 0) nums.Remove(n); }
catch (InvalidOperationException) { Console.WriteLine("InvalidOperationException"); }

nums = [1, 2, 3, 4, 5, 6];
int removed = nums.RemoveAll(n => n % 2 == 0);
Console.WriteLine($"{removed} {string.Join(",", nums)}");   // 3 1,3,5
```

Verified: the exception and then `3 1,3,5`.

</details>

## 4. 🟡 A generic maximum

**Task:** write `T Max<T>(IEnumerable<T> items) where T : IComparable<T>`. For an empty sequence throw `InvalidOperationException("Sequence is empty.")`.

**Expected result:**

```text
Max([3, 9, 2])                  → 9
Max(["pear", "apple", "zebra"]) → zebra
Max([])                         → InvalidOperationException
```

<details><summary>Hint</summary>

The `IComparable<T>` constraint provides `CompareTo`. Take the enumerator manually to check emptiness.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 4: a generic method with a constraint.
static T Max<T>(IEnumerable<T> items) where T : IComparable<T>
{
    using var e = items.GetEnumerator();
    if (!e.MoveNext()) throw new InvalidOperationException("Sequence is empty.");
    var max = e.Current;
    while (e.MoveNext())
        if (e.Current.CompareTo(max) > 0) max = e.Current;
    return max;
}
```

Verified: `9`, `zebra`, and an exception with the message `Sequence is empty.`.

</details>

## 5. 🔴 An LRU cache

**Task:** write `LruCache<TKey, TValue>(int capacity)` with the methods `Put` and `TryGet`. On overflow the least recently used element is evicted; `TryGet` and a repeated `Put` count as use. All operations are O(1).

**Expected result:**

```text
capacity = 2: Put(a), Put(b), TryGet(a) → True 1
Put(c) evicts b → TryGet(b) False, TryGet(a) True, TryGet(c) True, Count = 2
```

<details><summary>Hint</summary>

`Dictionary<TKey, LinkedListNode<…>>` for lookup and a `LinkedList<…>` for usage order: the head is the most recent, the tail is the eviction candidate.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 5: LRU cache — a dictionary for O(1) lookup plus a linked list for recency order.
class LruCache<TKey, TValue> where TKey : notnull
{
    private readonly int _capacity;
    private readonly Dictionary<TKey, LinkedListNode<(TKey Key, TValue Value)>> _map = new();
    private readonly LinkedList<(TKey Key, TValue Value)> _order = new();   // front = most recent

    public LruCache(int capacity)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(capacity);
        _capacity = capacity;
    }

    public int Count => _map.Count;

    public bool TryGet(TKey key, out TValue value)
    {
        if (_map.TryGetValue(key, out var node))
        {
            _order.Remove(node);
            _order.AddFirst(node);
            value = node.Value.Value;
            return true;
        }
        value = default!;
        return false;
    }

    public void Put(TKey key, TValue value)
    {
        if (_map.TryGetValue(key, out var existing)) { _order.Remove(existing); _map.Remove(key); }
        else if (_map.Count == _capacity)
        {
            var last = _order.Last!;
            _order.RemoveLast();
            _map.Remove(last.Value.Key);
        }
        _map[key] = _order.AddFirst((key, value));
    }
}
```

Verified: the output matches the table. The implementation is not thread-safe; multithreaded code needs synchronization ([18 · Concurrency](../18-concurrency/overview.en.md)).

</details>
