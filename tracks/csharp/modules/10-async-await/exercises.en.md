---
title: "Exercises: Async/await"
lang: en
track: csharp
kind: module
doc: exercises
slug: 10-async-await
part: 2
tags: [async-await, task-whenall, cancellation-token, retry, semaphore-slim, timeout]
status: draft
updated: 2026-11-22
---

# Exercises: Async/await

🌐 [Русский](exercises.ru.md) · **English**

> Solve it yourself before opening the hints. Solutions are under spoilers. The code was verified by running it; there are no runnable versions in `code/` yet (level 5 does not exist).

## 1. 🟢 Parallel sum

**Task:** write `static Task<int> SumAsync(IEnumerable<Func<Task<int>>> sources)`: it starts all sources at once and returns the sum of their results.

**Expected result:**

```text
SumAsync([ delay50 → 1, delay50 → 2, FromResult(3) ])   → 6   (~50 ms, not ~100 ms)
```

<details><summary>Hint</summary>

Start everything first (`Select(s => s())` + `ToList()`), then a single `await Task.WhenAll`. A lazy `Select` without `ToList` would start the operations twice if enumerated again.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 1: sum results of several async sources in parallel.
static async Task<int> SumAsync(IEnumerable<Func<Task<int>>> sources)
{
    var tasks = sources.Select(s => s()).ToList(); // start all, then await together
    int[] results = await Task.WhenAll(tasks);
    return results.Sum();
}
```

Verified: three sources give `6`. The key idea is to start everything before the first `await`.

</details>

## 2. 🟡 Timeout that cancels the operation

**Task:** write `WithTimeoutAsync<T>(Func<CancellationToken, Task<T>> op, TimeSpan timeout, CancellationToken ct = default)`. If the operation does not finish within `timeout`, it must be **canceled**, and the method throws `TimeoutException`. If the caller canceled (`ct`), an ordinary cancellation propagates.

**Expected result:**

```text
operation 1000 ms, timeout 50 ms     → TimeoutException "Operation timed out after 00:00:00.0500000."
operation 10 ms,   timeout 1 s       → result "ok"
ct already canceled                  → TaskCanceledException
```

<details><summary>Hint</summary>

You need a linked token source: `CreateLinkedTokenSource(ct)` + `CancelAfter(timeout)`. A timeout differs from a cancellation by the caller in the state of the outer `ct` — use a `catch … when` filter.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 2: timeout that also cancels the underlying operation.
static async Task<T> WithTimeoutAsync<T>(Func<CancellationToken, Task<T>> op, TimeSpan timeout, CancellationToken ct = default)
{
    using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
    cts.CancelAfter(timeout);
    try
    {
        return await op(cts.Token);
    }
    catch (OperationCanceledException) when (!ct.IsCancellationRequested)
    {
        throw new TimeoutException($"Operation timed out after {timeout}.");
    }
}
```

`Task.WaitAsync(timeout)` would not fit: it stops waiting, but the operation keeps running. A linked token really interrupts the operation.

</details>

## 3. 🟡 Limiting parallelism

**Task:** write `ForEachLimitedAsync<T>(IEnumerable<T> items, int maxConcurrency, Func<T, CancellationToken, Task> body, CancellationToken ct = default)`: it runs `body` for each item, but no more than `maxConcurrency` at a time. Without `Parallel.ForEachAsync`.

**Expected result:** with 12 items and `maxConcurrency = 3`, the maximum number running at once is `3`.

<details><summary>Hint</summary>

`SemaphoreSlim(maxConcurrency)`: before starting — `await gate.WaitAsync(ct)`, after completion (in `finally`) — `Release()`. At the end — `Task.WhenAll`.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 3: bounded parallelism.
static async Task ForEachLimitedAsync<T>(IEnumerable<T> items, int maxConcurrency, Func<T, CancellationToken, Task> body, CancellationToken ct = default)
{
    using var gate = new SemaphoreSlim(maxConcurrency);
    var tasks = new List<Task>();
    foreach (var item in items)
    {
        await gate.WaitAsync(ct);
        tasks.Add(Run(item));
    }
    await Task.WhenAll(tasks);

    async Task Run(T item)
    {
        try { await body(item, ct); }
        finally { gate.Release(); }
    }
}
```

Verified: a counter of concurrent calls (via `Interlocked`) showed a maximum of `3`. `Release` in `finally` is mandatory, otherwise the slot is not returned on an exception.

</details>

## 4. 🟡 Retry with a delay

**Task:** write `RetryAsync<T>(Func<int, CancellationToken, Task<T>> op, int maxAttempts, TimeSpan delay, CancellationToken ct = default)`: it calls `op(attemptNumber, ct)`; on an exception it waits `delay` and retries, up to `maxAttempts` attempts. The last exception propagates. After `ct` is canceled there are no retries.

**Expected result:**

```text
op fails on attempts 1 and 2, succeeds on the 3rd, maxAttempts = 5 → "ok on attempt 3"
op always fails, maxAttempts = 3                                   → InvalidOperationException "fail 3"
```

<details><summary>Hint</summary>

`for (int attempt = 1; ; attempt++)` + `try/catch` with the filter `when (attempt < maxAttempts && !ct.IsCancellationRequested)`: on the last attempt the filter is false — the exception leaves the method.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 4: retry with fixed delay.
static async Task<T> RetryAsync<T>(Func<int, CancellationToken, Task<T>> op, int maxAttempts, TimeSpan delay, CancellationToken ct = default)
{
    for (int attempt = 1; ; attempt++)
    {
        try
        {
            return await op(attempt, ct);
        }
        catch (Exception) when (attempt < maxAttempts && !ct.IsCancellationRequested)
        {
            await Task.Delay(delay, ct);
        }
    }
}
```

The `when` filter does not catch the exception of the last attempt, so the call stack is preserved. In real projects this logic is provided by the Polly library (not created yet).

</details>

## 5. 🔴 First successful result

**Task:** write `FirstSuccessfulAsync<T>(IReadOnlyList<Func<CancellationToken, Task<T>>> ops, CancellationToken ct = default)`: it starts all operations, returns the result of the **first successful one** and cancels the rest. If all fail — an `AggregateException` with all errors.

**Expected result:**

```text
a: fails after 10 ms; b: "b" after 40 ms; c: "c" after 500 ms  → "b", operation c is canceled
all fail                                                         → AggregateException with 2 inner exceptions
```

<details><summary>Hint</summary>

A `Task.WhenAny(pending)` loop; remove the completed task from the list. For the winner call `await cts.CancelAsync()`. Give the operations the one shared token of a linked source.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 5: first successful result, cancel the rest.
static async Task<T> FirstSuccessfulAsync<T>(IReadOnlyList<Func<CancellationToken, Task<T>>> ops, CancellationToken ct = default)
{
    using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
    var pending = ops.Select(op => op(cts.Token)).ToList();
    var errors = new List<Exception>();
    while (pending.Count > 0)
    {
        var done = await Task.WhenAny(pending);
        pending.Remove(done);
        if (done.IsCompletedSuccessfully)
        {
            await cts.CancelAsync();   // stop the losers
            return done.Result;
        }
        errors.Add(done.Exception!.GetBaseException());
    }
    throw new AggregateException("All operations failed.", errors);
}
```

Verified: on the example from the task `b` was returned and operation `c` received the cancellation; with two failing operations an `AggregateException` with 2 errors is thrown. A limitation of the solution: a delegate that throws **synchronously** (not `async`) breaks the method at `Select`; in production code wrap the call in `Task.Run` or `try/catch`.

</details>
