---
title: "Async/await"
lang: en
track: csharp
kind: module
doc: overview
slug: 10-async-await
part: 2
levels: [1, 2, 3, 4]
tags: [async-await, task, valuetask, cancellation-token, configureawait, deadlock, async-streams]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-22
---

# Async/await

🌐 [Русский](overview.ru.md) · **English**

## In a nutshell

`async`/`await` is the asynchronous programming model of C#: a method suspends while waiting for an operation (network, disk, database), freeing the thread, and continues after the operation completes. The result of the wait is represented by a `Task` or `Task<T>`. The code is written almost like sequential code, and the compiler turns it into a state machine. The approach is for scalable servers and responsive applications: it saves threads but does not speed up computation.

## Key ideas

- **`await` does not block the thread:** the method is suspended, and the continuation runs after the task completes.
- **Tasks are "hot":** a `Task` starts when the method is called; independent operations are started together and awaited with `Task.WhenAll`.
- **Exceptions** are stored in the task and thrown at `await`; `.Result`/`.Wait()` wrap them in an `AggregateException`.
- **Cancellation is cooperative:** pass a `CancellationToken` to every long method; build timeouts with `CancelAfter` and linked tokens.
- **Sync-over-async** (`.Result`, `.Wait()`) causes deadlocks in a synchronization context and thread-pool starvation; the rule is "async all the way".
- **`ConfigureAwait(false)`** goes into libraries; ASP.NET Core has no synchronization context.
- **`ValueTask<T>`** saves allocations on synchronous completion but allows exactly one `await`.
- **Asynchronous streams:** `IAsyncEnumerable<T>` with `await foreach` and `IAsyncDisposable` with `await using`.

## Minimal example

```csharp
static async Task<int> Work(int id) { await Task.Delay(200); return id * 10; }

// Start both operations first, then wait for both: ~200 ms instead of ~400 ms.
int[] results = await Task.WhenAll(Work(1), Work(2));
Console.WriteLine(string.Join(",", results));   // 10,20
```

## When to use and when not

- ✅ I/O: HTTP, files, databases, queues.
- ✅ Server code with many simultaneous requests.
- ✅ UI: do not block the main thread.
- ❌ Pure computation: `async` does not speed it up, you need `Task.Run`/`Parallel`.
- ❌ Blocking calls (`.Result`, `.Wait()`) inside asynchronous code.
- ❌ `async void` anywhere except event handlers.

## Levels of this material

| Level | What's inside |
|---|---|
| [1. Basics](basics.en.md) | why async, `Task`/`Task<T>`, what happens at `await`, `WhenAll`/`WhenAny`, exceptions, `CancellationToken`, `Task.Run` |
| [2. Advanced](advanced.en.md) | `ConfigureAwait` and the synchronization context, deadlock, exceptions and `AggregateException`, timeouts, `WhenEach`, `ValueTask`, async streams, `TaskCompletionSource`, `PeriodicTimer` |
| [3. Expert](expert.en.md) | the state machine and the awaitable pattern, task states, `Run` vs `StartNew`, `ExecutionContext`/`AsyncLocal`, synchronous continuations, pool starvation, reference, gotchas, 17 interview questions |
| [4. Self-check](quiz.en.md) | 12 questions and [5 exercises](exercises.en.md) with solutions |
| 5. Runnable examples | not generated yet — `/learn csharp 10-async-await level 5` |
| 6. Tests | not generated yet — `/learn csharp 10-async-await level 6` |

## Related materials

- Prerequisites: [08 · Exceptions](../08-exceptions/overview.en.md), [09 · Delegates, events, lambdas](../09-delegates-events-lambdas/overview.en.md)
- Next: [18 · Concurrency](../18-concurrency/overview.en.md), [15 · EF Core](../15-ef-core/overview.en.md) (asynchronous queries)
- Resources: [12 · Memory and GC](../12-memory-and-gc/overview.en.md) — `IAsyncDisposable`
- The same idea in other tracks: [Python](../../../python/overview.en.md) — `asyncio`, `gather` ↔ `Task.WhenAll`
