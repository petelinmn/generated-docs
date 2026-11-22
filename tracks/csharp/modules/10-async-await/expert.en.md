---
title: "Async/await: expert"
lang: en
track: csharp
kind: module
doc: expert
slug: 10-async-await
part: 2
tags: [async-await, state-machine, awaitable, execution-context, async-local, valuetask, task-scheduler, thread-pool-starvation, interview-questions]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-22
---

# Async/await: expert

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · **3. Expert** · [4. Quiz](quiz.en.md)

**Prerequisites:** [basic](basics.en.md) and [advanced](advanced.en.md) theory. **Time:** ~2 h.

## How the compiler turns an async method into code

A method marked `async` is compiled into a **state machine** — a hidden type that implements `IAsyncStateMachine`. Locals and parameters that live across an `await` become its fields; the wrapper method creates the machine, calls `builder.Start(ref machine)` and returns `builder.Task`. Verified via reflection on a method `RunAsync(int x)` with two `await`s:

```csharp
class Demo
{
    public async Task<int> RunAsync(int x) { await Task.Delay(1); var y = x + 1; await Task.Delay(1); return y; }
}

foreach (var t in typeof(Demo).GetNestedTypes(BindingFlags.NonPublic))
    Console.WriteLine($"{t.Name}  IsValueType={t.IsValueType}  interfaces={string.Join(",", t.GetInterfaces().Select(i => i.Name))}");
```

```text
Debug:   <RunAsync>d__0  IsValueType=False  interfaces=IAsyncStateMachine
Release: <RunAsync>d__0  IsValueType=True   interfaces=IAsyncStateMachine
```

Machine fields: `<>1__state` (the continuation point number), `<>t__builder`, parameters and variables that survive an `await` (in Release the local `y` is stored as `<y>5__2`), and `<>u__N` fields for awaiters. In Release the machine is a **struct**: while the method completes synchronously it lives on the stack. At the first real suspension the machine is boxed onto the heap — one allocation for the lifetime of the call; in Debug it is a class, and the allocation always happens.

Each `await expr` expands roughly like this:

1. `awaiter = expr.GetAwaiter()`;
2. if `awaiter.IsCompleted` — the result is taken immediately, without suspension (the fast path);
3. otherwise the state number is stored in `<>1__state`, the awaiter is saved to a field, `builder.AwaitUnsafeOnCompleted(ref awaiter, ref machine)` is called and the method returns;
4. when the awaiter completes, it calls `MoveNext()`, the machine jumps to the right point and `awaiter.GetResult()` is called (it also throws the exception).

### The awaitable pattern

`await` works with more than `Task`: any type with a `GetAwaiter()` method returning a type with `IsCompleted`, `OnCompleted` (via `INotifyCompletion`) and `GetResult()` is enough. Verified with a minimal implementation:

```csharp
struct MyAwaitable { public MyAwaiter GetAwaiter() => new(); }

struct MyAwaiter : INotifyCompletion
{
    public bool IsCompleted => false;
    public void OnCompleted(Action continuation) { Console.WriteLine("OnCompleted called"); continuation(); }
    public void GetResult() { Console.WriteLine("GetResult called"); }
}

await new MyAwaitable();
// OnCompleted called
// GetResult called
```

`GetAwaiter` can also be an extension method — this is how `await` works for `TimeSpan`, `Process` and the like in third-party libraries.

### Builders and return types

The builder defines the type of an `async` method's result: `Task`/`Task<T>` use `AsyncTaskMethodBuilder`, `ValueTask`/`ValueTask<T>` use `AsyncValueTaskMethodBuilder`, `void` uses `AsyncVoidMethodBuilder`. Since C# 7 **any** "task-like" type marked with `[AsyncMethodBuilder(typeof(...))]` is allowed. For `ValueTask` you can enable machine pooling with the attribute `[AsyncMethodBuilder(typeof(PoolingAsyncValueTaskMethodBuilder))]` on a method (measure the gain first).

## Tasks: states and scheduling

| `Status` property | When |
|---|---|
| `Created` | the task was created by a constructor and not started (rare) |
| `WaitingForActivation` | a "promise" task — the result of an async method or a `TaskCompletionSource` — waiting to complete |
| `WaitingToRun` / `Running` | scheduled / the delegate is executing |
| `WaitingForChildrenToComplete` | waiting for nested tasks created with `AttachedToParent` |
| `RanToCompletion` | succeeded |
| `Canceled` | canceled (an `OperationCanceledException` with the task's token was thrown) |
| `Faulted` | completed with an exception |

There are two kinds of tasks: **delegate** tasks (`Task.Run`, `Task.Factory.StartNew`) run code on a scheduler, and **promise** tasks (the result of an `async` method, `TaskCompletionSource`, `Task.Delay`) occupy no thread: they are "completed" from outside.

### `Task.Run` and `Task.Factory.StartNew`

`Task.Run(async () => ...)` unwraps the nested task, while `StartNew` does not and returns `Task<Task>`:

```csharp
Task<Task> tt = Task.Factory.StartNew(async () => { await Task.Delay(50); });
Console.WriteLine(tt.IsCompleted);   // False now, but it completes as soon as the lambda reaches its first await
await tt.Unwrap();                   // must unwrap to wait for the inner work

var viaRun = Task.Run(async () => { await Task.Delay(50); });   // already unwrapped
await viaRun;
```

The outer `StartNew` task completes when the lambda reaches its first `await`, not when it finishes. In addition, `StartNew` uses `TaskScheduler.Current` rather than the pool, so behavior changes inside a custom scheduler. Use `Task.Run`.

### Execution context: `ExecutionContext` and `AsyncLocal<T>`

`ExecutionContext` is a container of implicit data (`AsyncLocal<T>`, parts of security information) that **flows** through `await` and `Task.Run`: the continuation sees the values that existed when the wait began. There is no reverse flow — changes made inside an `async` method are not visible to the caller after it returns:

```csharp
var al = new AsyncLocal<string>();
al.Value = "outer";
await Task.Run(async () => { Console.WriteLine(al.Value); al.Value = "inner"; await Task.Delay(1); });   // outer
Console.WriteLine(al.Value);                                                                              // outer

static async Task SetIt(AsyncLocal<string> a) { a.Value = "set in async method"; await Task.Yield(); }
static void SetSync(AsyncLocal<string> a) { a.Value = "set in sync method"; }

await SetIt(al);  Console.WriteLine(al.Value);   // outer
SetSync(al);      Console.WriteLine(al.Value);   // set in sync method
```

The builder saves and restores the `ExecutionContext` around an `async` method, so "setting a value in a called async method" does not work. Practical consequence: `AsyncLocal` suits correlation identifiers flowing down the call chain, but not passing results up. `AsyncLocal` details are in [18 · Concurrency](../18-concurrency/expert.en.md).

### `TaskCompletionSource` and synchronous continuations

By default `SetResult` runs subscribers' continuations **synchronously, on the calling thread**. If a subscriber works for a long time, it blocks whoever called `SetResult`, and while holding a lock this can deadlock. Verified: a continuation with `Thread.Sleep(300)`:

```csharp
var tcs = new TaskCompletionSource<int>();                                           // default
var w = Task.Run(async () => { await tcs.Task; Thread.Sleep(300); });
await Task.Delay(50);
tcs.SetResult(1);                                                                    // blocked ~300 ms

var tcs2 = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
// the same waiter: SetResult returns in ~0 ms, the continuation runs on the pool
```

```text
default: SetResult blocked ~300 ms (continuation ran inline)
RunContinuationsAsynchronously: SetResult blocked ~0 ms
```

If a `TaskCompletionSource` is created in library code, you almost always need `RunContinuationsAsynchronously`.

## Reference

| API | Purpose | Since |
|---|---|---|
| `Task.Run(Func<T>)` | run CPU work on the pool | 4.5 |
| `Task.WhenAll` / `WhenAny` | wait for all / the first | 4.5 |
| `Task.WhenEach` | an asynchronous stream of tasks as they complete | .NET 9 |
| `Task.Delay(ms, ct)` | asynchronous pause | 4.5 |
| `Task.Yield()` | force a suspension | 4.5 |
| `Task.FromResult` / `CompletedTask` / `FromCanceled` / `FromException` | ready-made tasks | 4.5+ |
| `Task.WaitAsync(timeout, ct)` | stop waiting on timeout/token | .NET 6 |
| `ConfigureAwait(bool)` / `ConfigureAwait(ConfigureAwaitOptions)` | control the context / suppress exceptions | 4.5 / .NET 8 |
| `CancellationTokenSource.CancelAsync()`, `CancelAfter` | cancellation | .NET 8 / 4.5 |
| `Parallel.ForEachAsync` | parallel asynchronous loop | .NET 6 |
| `PeriodicTimer` | asynchronous timer | .NET 6 |
| `IAsyncEnumerable<T>`, `await foreach` | asynchronous streams | C# 8 |
| `IAsyncDisposable`, `await using` | asynchronous cleanup | C# 8 |
| `ValueTask`, `ValueTask<T>` | a task without allocation on synchronous completion | .NET Core 2.1 |
| `TaskCompletionSource<T>` | turn a callback into a `Task` | 4.0 |

`ConfigureAwaitOptions.SuppressThrowing` lets you wait for a task and **not receive** its exception:

```csharp
await Fail().ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);   // no exception thrown
```

Tasks that failed but were never awaited keep their error in `Task.Exception`; `Task.Run(() => throw ...)` has `IsFaulted == true` after a pause (verified).

## Thread pool and starvation

Asynchronous code wins while pool threads are free. Blocking (`Task.Wait`, `Thread.Sleep`, synchronous I/O) occupies a thread for nothing; the pool adds threads with a delay (beyond the minimum, via a hill-climbing algorithm), so under load **starvation** sets in: tasks sit in the queue while the threads are busy waiting for other tasks. Diagnostics: a growing pool queue (`ThreadPool.PendingWorkItemCount`), latency with low CPU usage. The cure is removing sync-over-async, not inflating `SetMinThreads`.

## Gotchas

**1. An `async` method without `await` completes synchronously.**

```csharp
static async Task<int> Quick() => 5;
Console.WriteLine(Quick().IsCompleted);   // True
```

An exception from such a method still goes into the task rather than being thrown at the call.

**2. `Select(async ...)` starts the operations on every enumeration.** The query is lazy; every pass over it creates the tasks again:

```csharp
int started = 0;
var query = new[] { 1, 2, 3 }.Select(async i => { Interlocked.Increment(ref started); await Task.Delay(10); return i * i; });
Console.WriteLine($"before enumeration: started = {started}");
var list = query.ToList();
Console.WriteLine($"after ToList: started = {started}");
var results = await Task.WhenAll(query);
Console.WriteLine($"after WhenAll(query): started = {started}; results = {string.Join(",", results)}");
```

```text
before enumeration: started = 0
after ToList: started = 3
after WhenAll(query): started = 6; results = 1,4,9
```

Materialize the query once (`ToList()`) and pass the list to `WhenAll`.

**3. After `await` in a console app you are already on a pool thread.**

```csharp
Console.WriteLine(Thread.CurrentThread.IsThreadPoolThread);   // False
await Task.Delay(10);
Console.WriteLine(Thread.CurrentThread.IsThreadPoolThread);   // True
```

This matters for thread-dependent code (`[ThreadStatic]`, COM): after `await` the thread is different.

**4. `async void` sends the exception to the context.** Verified with a context that intercepts it: the message "from async void" arrived in `Post`. Without a context the exception terminates the process as unhandled.

**5. A canceled task is not `Faulted`.** `Task.Delay(…, ct)` yields the status `Canceled` on cancellation; `catch (Exception)` catches it, but `task.IsFaulted` is `False`.

**6. Awaiting a `ValueTask` twice.** If a `ValueTask` is built on top of a regular `Task`, the second `await` happens to work (verified: both printed `1`), but the contract forbids it, and for tasks backed by pooled sources (`IValueTaskSource`) the result is unpredictable.

**7. Identical ready-made tasks.** `Task.FromResult(true)` returns a cached instance: `ReferenceEquals(Task.FromResult(true), Task.FromResult(true))` is `True` (verified). Do not compare tasks by reference.

**8. `lock` and `await`.** `lock (o) { await ... }` is a compile error `CS1996`: after `await` the thread may differ, and `Monitor` is bound to a thread. Use `SemaphoreSlim.WaitAsync`.

## Trade-offs and alternatives

| Task | Approach |
|---|---|
| Waiting on I/O | `async`/`await` |
| CPU work | `Task.Run`, `Parallel`, [threads](../18-concurrency/overview.en.md) |
| A stream of events over time | `IAsyncEnumerable<T>`, `Channel<T>` |
| Callback → task | `TaskCompletionSource` |
| A pipeline with a limit | `Parallel.ForEachAsync`, `SemaphoreSlim`, `Channel<T>` |
| Reactive streams | Rx.NET (an external library) |

## Interview questions

<details><summary>1. How does `async`/`await` differ from creating a thread?</summary>

`async` creates no threads. The method is suspended at `await`, and the thread goes back to the caller. I/O operations complete via the OS, and the continuation is scheduled on a pool thread. A thread is needed only for the time the code runs, not for the time of waiting.

</details>

<details><summary>2. What does the compiler do with an `async` method?</summary>

It turns it into a state machine (`IAsyncStateMachine`) whose fields hold the locals that survive an `await`. The builder (`AsyncTaskMethodBuilder`) creates the returned task and schedules the continuation.

</details>

<details><summary>3. When does `await` not suspend the method?</summary>

When `awaiter.IsCompleted == true`: the task has already completed (a ready `Task.FromResult`, a cached result). Execution then continues synchronously without allocating a continuation.

</details>

<details><summary>4. How do `await task` and `task.Result` differ?</summary>

`await` does not block the thread and unwraps the exception; `.Result` blocks the thread and wraps the exception in an `AggregateException`. Blocking can lead to a deadlock in a synchronization context and to pool starvation.

</details>

<details><summary>5. Explain the deadlock with `.Result` in a UI.</summary>

A method calls `.Result`, blocking the UI thread. Inside, `await` wants to return the continuation to the UI `SynchronizationContext`, but the thread is busy waiting for the task. Each waits for the other. Cure: `await` along the whole chain, or `ConfigureAwait(false)` in library code.

</details>

<details><summary>6. What does `ConfigureAwait(false)` do, and what about ASP.NET Core?</summary>

It declines to return to the captured context. ASP.NET Core has no synchronization context, so there is practically no difference; libraries still use it so the code works in any environment.

</details>

<details><summary>7. Why is `async void` dangerous?</summary>

There is no returned task: you cannot await it, and the caller cannot catch the exception. The exception goes to the `SynchronizationContext` or terminates the process. It is acceptable for event handlers.

</details>

<details><summary>8. What happens with `Task.WhenAll` if two tasks fail?</summary>

`await` throws the first exception; the rest are available in `Task.Exception.InnerExceptions`. So after `WhenAll`, inspect the task itself in `catch` if all errors matter.

</details>

<details><summary>9. When should you use `ValueTask`?</summary>

When a method is called very often and usually completes synchronously (a cache, a buffer). Rules: `await` once, do not use concurrently, do not read `.Result` before completion; to keep it, use `AsTask()`.

</details>

<details><summary>10. How do you cancel an asynchronous operation?</summary>

Pass a `CancellationToken`; the operation checks it (`ThrowIfCancellationRequested`) or passes it to an API that supports tokens. Cancellation is cooperative: code that does not check the token will not be interrupted.

</details>

<details><summary>11. How does `Task.Run` differ from `Task.Factory.StartNew`?</summary>

`Task.Run` always uses the pool and unwraps nested tasks (`Task<Task>` → `Task`); `StartNew` uses `TaskScheduler.Current` and returns `Task<Task>` for `async` lambdas. `Task.Run` is the right choice.

</details>

<details><summary>12. How do you turn a callback API into a `Task`?</summary>

With `TaskCompletionSource<T>`: return `tcs.Task` and call `SetResult`/`SetException` in the callback. Create it with `RunContinuationsAsynchronously`, otherwise continuations run synchronously inside `SetResult`.

</details>

<details><summary>13. What is "async all the way"?</summary>

The principle: if you call an asynchronous method, the caller is asynchronous too and awaits all the way to the entry point. Mixing synchronous and asynchronous code creates deadlocks and pool starvation.

</details>

<details><summary>14. Why can't you use `lock` with `await`?</summary>

`Monitor` is bound to a thread: after `await` execution may continue on another thread, which cannot release the lock. The compiler forbids it (`CS1996`). Use `SemaphoreSlim.WaitAsync`.

</details>

<details><summary>15. Trick question: does an `AsyncLocal` set inside an `async` method change for the caller?</summary>

No. `ExecutionContext` flows down and forward along the `await` chain but is restored on return from an async method. Verified: after `await SetIt(al)` the value stayed `outer`; a synchronous method kept its change.

</details>

<details><summary>16. Trick question: how long does `await a(); await b();` take if each takes 1 s, and how to speed it up?</summary>

2 seconds: the operations run sequentially. First start both (`var ta = a(); var tb = b();`), then `await Task.WhenAll(ta, tb)` — ~1 s.

</details>

<details><summary>17. What is thread-pool starvation and how do you recognize it?</summary>

All pool threads are blocked (most often by sync-over-async), new tasks wait in the queue, and the pool adds threads with a delay. Signs: growing latency with low CPU usage and a growing pool queue. The cure is removing blocking calls.

</details>

## Related materials

- Back: [advanced theory](advanced.en.md). Check yourself: [quiz](quiz.en.md) and [exercises](exercises.en.md).
- [18 · Concurrency](../18-concurrency/overview.en.md) — `AsyncLocal`, the thread pool, `SemaphoreSlim`, `Channel<T>`.
- [08 · Exceptions](../08-exceptions/overview.en.md), [09 · Delegates, events, lambdas](../09-delegates-events-lambdas/overview.en.md) — lambdas and exceptions in async code.
- [12 · Memory and GC](../12-memory-and-gc/overview.en.md) — state machine allocations, `IAsyncDisposable`.
- [Python](../../../python/overview.en.md) — `asyncio`: coroutines, the event loop, `Task`.

## Sources

- [Task-based asynchronous pattern (TAP)](https://learn.microsoft.com/dotnet/standard/asynchronous-programming-patterns/task-based-asynchronous-pattern-tap)
- [Async in depth](https://learn.microsoft.com/dotnet/standard/async-in-depth)
- [`ConfigureAwaitOptions`](https://learn.microsoft.com/dotnet/api/system.threading.tasks.configureawaitoptions)
- [`TaskCreationOptions.RunContinuationsAsynchronously`](https://learn.microsoft.com/dotnet/api/system.threading.tasks.taskcreationoptions)
- [`AsyncLocal<T>`](https://learn.microsoft.com/dotnet/api/system.threading.asynclocal-1)
- [`IAsyncStateMachine`](https://learn.microsoft.com/dotnet/api/system.runtime.compilerservices.iasyncstatemachine)
