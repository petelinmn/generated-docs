---
title: "Exercises: Multithreading and synchronization"
lang: en
track: csharp
kind: module
doc: exercises
slug: 18-concurrency
part: 3
tags: [lock, double-checked-locking, semaphore, compare-and-swap]
status: draft
updated: 2026-11-21
---

# Exercises: Multithreading and synchronization

🌐 [Русский](exercises.ru.md) · **English**

> Solve it yourself before opening the hints. Solutions are under spoilers, runnable versions are in [code/Solutions/Solutions.cs](code/Solutions/Solutions.cs), their tests are in [code/Tests/SolutionsTests.cs](code/Tests/SolutionsTests.cs).

## 1. 🟢 Thread-safe statistics

**Task:** write a `SafeStatistics` class with `Add(long value)` and `Snapshot()` methods; the latter returns `(Count, Sum, Max)`. The methods are called from many threads at once, and the snapshot must be consistent: all three values at the same moment.

**Expected result:**

```text
Parallel.For(1, 10_001, i => stats.Add(i));
stats.Snapshot() → (10000, 50005000, 10000)
```

<details><summary>Hint</summary>

`Interlocked` on each field separately will not do: between the updates another thread sees an inconsistent set. You need one lock for the whole state.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 1: several fields must change together, so one lock protects all of them.
public sealed class SafeStatistics
{
    private readonly Lock _gate = new();
    private long _count;
    private long _sum;
    private long _max = long.MinValue;

    public void Add(long value)
    {
        lock (_gate)
        {
            _count++;
            _sum += value;
            if (value > _max)
            {
                _max = value;
            }
        }
    }

    // A consistent snapshot: all three values are read under the same lock.
    public (long Count, long Sum, long Max) Snapshot()
    {
        lock (_gate)
        {
            return (_count, _sum, _max);
        }
    }
}
```

The `SafeStatistics` class in [Solutions.cs](code/Solutions/Solutions.cs). Reading the snapshot also happens under the lock — otherwise you may get `Count` from before an addition and `Sum` from after it.

</details>

## 2. 🟡 Lazy initialization with a double check

**Task:** write `OnceInitializer<T>` with a `Value` property that calls the factory on first access and returns the same object afterwards. The factory must run exactly once, even if 8 threads access `Value` at the same time. The common path (the value already exists) must not take a lock.

**Expected result:**

```text
8 threads read Value at once → the factory is called once, all get the same object
```

<details><summary>Hint</summary>

A check without a lock → `lock` → a second check under the lock → creation. Read and publish the field with `Volatile.Read` and `Volatile.Write`.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 2: thread-safe lazy initialization with double-checked locking.
public sealed class OnceInitializer<T>(Func<T> factory)
    where T : class
{
    private readonly Lock _gate = new();
    private T? _value;

    public T Value
    {
        get
        {
            // Fast path without a lock; Volatile.Read pairs with Volatile.Write below.
            T? value = Volatile.Read(ref _value);
            if (value is not null)
            {
                return value;
            }
            lock (_gate)
            {
                value = _value;
                if (value is null)
                {
                    value = factory();
                    Volatile.Write(ref _value, value);
                }
                return value;
            }
        }
    }
}
```

The `OnceInitializer<T>` class in [Solutions.cs](code/Solutions/Solutions.cs). The second check under the lock is required: while a thread was waiting for the lock, another thread may have already created the object. In real code `Lazy<T>` does the same.

</details>

## 3. 🟡 A lock per key

**Task:** write `KeyedLock` with a `Task<T> RunAsync<T>(string key, Func<Task<T>> action)` method. Actions with the same key run strictly one at a time; actions with different keys do not interfere. `action` contains `await`.

**Expected result:**

```text
20 calls of RunAsync("same", …) → no more than 1 runs at a time
```

<details><summary>Hint</summary>

`lock` will not do because of `await`. Keep one `SemaphoreSlim(1, 1)` per key in a `ConcurrentDictionary`.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 3: one async-friendly lock per key; different keys do not block each other.
public sealed class KeyedLock
{
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();

    public async Task<T> RunAsync<T>(string key, Func<Task<T>> action)
    {
        SemaphoreSlim gate = _locks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
        await gate.WaitAsync();
        try
        {
            return await action();
        }
        finally
        {
            gate.Release();
        }
    }
}
```

The `KeyedLock` class in [Solutions.cs](code/Solutions/Solutions.cs). `GetOrAdd` may create an extra `SemaphoreSlim`, but only one ends up in the dictionary and gets used. The semaphores are never removed from the dictionary here — for an unbounded set of keys that is a memory leak, and real code needs cleanup.

</details>

## 4. 🔴 An atomic maximum

**Task:** write `AtomicMath.InterlockedMax(ref int location, int value)`. The method atomically writes `value` into `location` if it is greater than the current value, and returns the resulting value. No `lock`.

**Expected result:**

```text
Parallel.For(-5_000, 5_001, i => AtomicMath.InterlockedMax(ref max, i)) → max == 5000
InterlockedMax(ref location /* 10 */, 3)  → 10
InterlockedMax(ref location /* 10 */, 20) → 20
```

<details><summary>Hint</summary>

A compare-and-swap loop: read the current value; if the new one is not greater, leave; otherwise `CompareExchange`. If it returned something other than what you read, someone changed the value in between — retry with the new value.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 4: a lock-free "store the maximum" built on a compare-and-swap loop.
public static class AtomicMath
{
    public static int InterlockedMax(ref int location, int value)
    {
        int current = Volatile.Read(ref location);
        while (value > current)
        {
            int seen = Interlocked.CompareExchange(ref location, value, current);
            if (seen == current)
            {
                return value; // our value was stored
            }
            current = seen; // someone else changed it: re-check against the new value
        }
        return current;
    }
}
```

The `AtomicMath` class in [Solutions.cs](code/Solutions/Solutions.cs). The same technique is used in [Ex04_Throttling.cs](code/Examples/Ex04_Throttling.cs) to track peak load and in the lock-free stack from the [expert theory](expert.en.md).

</details>
