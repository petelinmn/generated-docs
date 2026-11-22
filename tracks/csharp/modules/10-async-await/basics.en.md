---
title: "Async/await: basics"
lang: en
track: csharp
kind: module
doc: basics
slug: 10-async-await
part: 2
tags: [async-await, task, cancellation-token, task-whenall, asynchronous-programming]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-22
---

# Async/await: basics

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [08 · Exceptions](../08-exceptions/overview.en.md), [09 · Delegates, events, lambdas](../09-delegates-events-lambdas/overview.en.md). **Time:** ~1 h.

## Why it matters

Many operations spend most of their time **waiting**: a network response, a disk, a database. If the thread blocks meanwhile, it does nothing but stays occupied: a server with thousands of such requests runs out of threads. Asynchronous code gives the thread back while the operation waits and continues (possibly on another thread) once it completes. The `async`/`await` keywords let you write such code almost like ordinary sequential code.

Important: async/await is for **waiting on I/O**, not for speeding up computation. Heavy computation needs threads and [concurrency](../18-concurrency/overview.en.md).

## `Task` and `Task<T>`

`Task` is an object representing an operation that will finish later; `Task<T>` is such an operation with a result of type `T`. A method marked `async` returns `Task`/`Task<T>` and awaits other tasks with `await` inside:

```csharp
static async Task<int> GetNumberAsync()
{
    await Task.Delay(50);      // the thread is free while we wait
    return 42;                 // becomes the Task<int> result
}

Console.WriteLine(await GetNumberAsync());   // 42
```

Rules:

- `await task` means "wait for completion without blocking the thread, and give me the result (or the exception)";
- `await` is allowed only inside an `async` method; the entry point can also be `async Task Main`;
- by convention, names of asynchronous methods end with `Async`;
- `return 42` in an `async Task<int>` becomes the task result — no manual wrapping needed.

## What happens at await

If the task is not complete yet, the method is **suspended** and returns control to the caller (which gets an incomplete `Task`). When the wait is over, the method resumes from the `await` point. Code before the first incomplete `await` runs synchronously on the caller's thread:

```csharp
static async Task Sync()
{
    Console.WriteLine("before await");
    await Task.Yield();                 // forces the method to suspend
    Console.WriteLine("after await");
}

var t = Sync();
Console.WriteLine("returned to caller");
await t;
```

```text
before await
returned to caller
after await
```

Tasks are "hot": a task starts when the method is called, not when it is awaited:

```csharp
static async Task Hot(string n)
{
    Console.WriteLine($"{n} started");
    await Task.Delay(30);
    Console.WriteLine($"{n} finished");
}

var t1 = Hot("A");
Console.WriteLine("created A, not awaited yet");
await t1;
```

```text
A started
created A, not awaited yet
A finished
```

## Sequential and parallel

Two `await`s in a row run one after another. To run operations at the same time, start both first and then wait for them together with `Task.WhenAll`:

```csharp
static async Task<int> Work(int id) { await Task.Delay(200); return id * 10; }

var a = await Work(1);                                  // ~200 ms
var b = await Work(2);                                  // + ~200 ms = ~400 ms

int[] results = await Task.WhenAll(Work(1), Work(2));   // ~200 ms in total
Console.WriteLine(string.Join(",", results));           // 10,20
```

Verified: the sequential version takes ~400 ms, `WhenAll` ~200 ms. `Task.WhenAll` returns an array of results in the order of the arguments. `Task.WhenAny` completes when the first of the tasks completes and returns **that task** (a `Task`), not a result:

```csharp
var slow = Task.Delay(1000);
var fast = Task.Delay(50);
var winner = await Task.WhenAny(slow, fast);
Console.WriteLine(winner == fast);   // True
```

## Exceptions

An exception inside an `async` method is stored in the task and **rethrown at `await`**, like a regular one:

```csharp
static async Task Fail() { await Task.Delay(10); throw new InvalidOperationException("boom"); }

try { await Fail(); }
catch (Exception e) { Console.WriteLine($"caught {e.GetType().Name}: {e.Message}"); }
// caught InvalidOperationException: boom
```

Without `await`, the exception stays inside the task and nobody notices it — do not call an async method "into the void". More about `Wait()`/`Result` and `AggregateException` in the [advanced theory](advanced.en.md#exceptions-and-aggregateexception).

## Cancellation: CancellationToken

Asynchronous operations are canceled **cooperatively**: a `CancellationTokenSource` hands out a token, and the operation accepts it and reacts itself:

```csharp
static async Task Loop(CancellationToken ct)
{
    while (true)
    {
        ct.ThrowIfCancellationRequested();   // cooperative check
        await Task.Delay(30, ct);            // the delay itself reacts to the token
    }
}

using var cts = new CancellationTokenSource(100);        // cancels after 100 ms
try { await Loop(cts.Token); }
catch (OperationCanceledException e)
{
    Console.WriteLine(e.GetType().Name);                 // TaskCanceledException
}
```

Verified: a `TaskCanceledException` was thrown — it derives from `OperationCanceledException`, so catch the base type. A task interrupted by cancellation gets the status `Canceled`, not `Faulted`. Rule: every method that may run for a long time takes a `CancellationToken ct` as its last parameter and passes it on.

## Task.Run: computation in the background

`Task.Run` runs a delegate on a thread-pool thread — this moves heavy computation off a thread that must not be occupied (for example, the UI thread):

```csharp
Console.WriteLine(Thread.CurrentThread.IsThreadPoolThread);                         // False in Main
Console.WriteLine(await Task.Run(() => Thread.CurrentThread.IsThreadPoolThread));   // True
```

`Task.Run` does not make I/O asynchronous and is meant only for CPU work. For I/O use truly asynchronous APIs (`HttpClient.GetStringAsync`, `File.ReadAllTextAsync`).

## Beginner mistakes

- ❌ `async void` → ✅ `async Task`: you cannot await an `async void` method or catch its exception (acceptable only for event handlers).
- ❌ `.Result` / `.Wait()` in asynchronous code → ✅ `await`: blocking can cause a deadlock ([advanced theory](advanced.en.md#sync-over-async-and-deadlock)).
- ❌ Sequential `await`s for independent operations → ✅ `Task.WhenAll`.
- ❌ A forgotten `await` → ✅ always `await`: otherwise exceptions are lost and the code moves on without waiting.
- ❌ `Task.Run` around I/O "to make it asynchronous" → ✅ call the asynchronous API directly.
- ❌ Not accepting a `CancellationToken` → ✅ add it to every long-running method.

## Related materials

- Next: [advanced theory](advanced.en.md) — `ConfigureAwait`, deadlock, `ValueTask`, async streams.
- [09 · Delegates, events, lambdas](../09-delegates-events-lambdas/overview.en.md) — lambdas that can be made `async`.
- [18 · Concurrency](../18-concurrency/overview.en.md) — threads, `SemaphoreSlim`, `Channel<T>`.
- [08 · Exceptions](../08-exceptions/overview.en.md) — how `try`/`catch` works.
- [Python](../../../python/overview.en.md) — `asyncio`: a similar `async`/`await` model, but a single thread and an event loop.

## Sources

- [Asynchronous programming (C#)](https://learn.microsoft.com/dotnet/csharp/asynchronous-programming/)
- [Task-based asynchronous pattern (TAP)](https://learn.microsoft.com/dotnet/standard/asynchronous-programming-patterns/task-based-asynchronous-pattern-tap)
- [Cancellation in managed threads](https://learn.microsoft.com/dotnet/standard/threading/cancellation-in-managed-threads)
