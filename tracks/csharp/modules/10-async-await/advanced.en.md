---
title: "Async/await: advanced"
lang: en
track: csharp
kind: module
doc: advanced
slug: 10-async-await
part: 2
tags: [async-await, configureawait, synchronization-context, deadlock, valuetask, async-streams, cancellation-token, task-whenall]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-22
---

# Async/await: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [basics](basics.en.md). **Time:** ~1.5 h.

## Synchronization context and ConfigureAwait

After `await`, a method continues where it "should": if the thread has a `SynchronizationContext` (UI threads of WPF, WinForms and similar), the continuation goes back into it; otherwise onto a thread-pool thread (`TaskScheduler.Current`). A console application and ASP.NET Core have no synchronization context. `ConfigureAwait(false)` says "I don't care where I continue" and skips the return to the context.

Rule of thumb:

- **libraries** put `ConfigureAwait(false)` on every `await`: they do not know the environment they are called from;
- **application code** (ASP.NET Core controllers, UI handlers that need the UI thread) is written without it.

## Sync-over-async and deadlock

Blocking on an asynchronous method (`.Result`, `.Wait()`, `GetAwaiter().GetResult()`) is called sync-over-async. If the calling thread is busy blocking while the method's continuation must return to exactly that thread, you get a **deadlock**: the thread waits for the task, the task waits for the thread. A reproduction with a single-threaded context (like a UI thread):

```csharp
using System.Collections.Concurrent;

// A context with one worker thread, like a UI thread.
class SingleThreadContext : SynchronizationContext
{
    private readonly BlockingCollection<(SendOrPostCallback, object?)> _queue = new();

    public SingleThreadContext()
    {
        new Thread(() =>
        {
            SetSynchronizationContext(this);
            foreach (var (callback, state) in _queue.GetConsumingEnumerable()) callback(state);
        }) { IsBackground = true }.Start();
    }

    public override void Post(SendOrPostCallback d, object? state) => _queue.Add((d, state));
}

static async Task<int> Blocking() { await Task.Delay(50); return 1; }                          // continuation returns to the context
static async Task<int> BlockingCfg() { await Task.Delay(50).ConfigureAwait(false); return 1; } // continuation runs on the pool

var ctx = new SingleThreadContext();
ctx.Post(_ => Console.WriteLine($"Wait finished within 500 ms: {Blocking().Wait(500)}"), null);
// the lambda blocks the only context thread, so the continuation can never run
await Task.Delay(1000);
ctx.Post(_ => Console.WriteLine($"with ConfigureAwait(false): {BlockingCfg().Wait(500)}"), null);
await Task.Delay(300);
```

```text
Wait finished within 500 ms: False
with ConfigureAwait(false): True
```

The cure: do not block, use `await` all the way up the call chain ("async all the way"). ASP.NET Core has no synchronization context, so the classic deadlock does not occur there, but sync-over-async occupies thread-pool threads and causes **thread-pool starvation** under load.

## Exceptions and AggregateException

`await` unwraps the exception, while `Wait()`/`Result` wrap it in an `AggregateException`:

```csharp
static async Task Fail() { await Task.Yield(); throw new InvalidOperationException("boom"); }
static async Task<int> Fail2() { await Task.Yield(); throw new InvalidOperationException("boom"); }

try { Fail().GetAwaiter().GetResult(); } catch (Exception e) { Console.WriteLine(e.GetType().Name); }  // InvalidOperationException
try { _ = Fail2().Result; }             catch (Exception e) { Console.WriteLine(e.GetType().Name); }  // AggregateException
```

If several tasks of `Task.WhenAll` fail, `await` throws **only the first** exception, and all of them are in `Task.Exception`:

```csharp
static async Task F(string m) { await Task.Delay(10); throw new Exception(m); }

var all = Task.WhenAll(F("one"), F("two"));
try { await all; }
catch (Exception e)
{
    Console.WriteLine($"{e.Message}; all.Exception count = {all.Exception!.InnerExceptions.Count}");
}
// one; all.Exception count = 2
```

If you never `await` a faulted task, its exception stays in `Task.Exception`. Unhandled task exceptions do not terminate the process (since .NET 4.5), but losing errors is still a bug.

`await` is allowed in `catch` and `finally`; inside `lock` it is not (`CS1996`), and async methods cannot have `ref`, `in`, `out` parameters (`CS1988`): to wait inside a critical section, use `SemaphoreSlim` ([18 · Concurrency](../18-concurrency/advanced.en.md)).

## Cancellation

### Timeout and linked tokens

```csharp
using var cts = new CancellationTokenSource();
cts.CancelAfter(50);                                 // cancel in 50 ms
try { await Task.Delay(1000, cts.Token); }
catch (TaskCanceledException) { Console.WriteLine("canceled by timeout"); }

using var a = new CancellationTokenSource();
using var b = new CancellationTokenSource();
using var linked = CancellationTokenSource.CreateLinkedTokenSource(a.Token, b.Token);
b.Cancel();
Console.WriteLine(linked.IsCancellationRequested);   // True: any parent cancels the linked source
```

A linked source is handy for adding a timeout to a token that came from outside. For a ready-made bounded wait there is `Task.WaitAsync` — it does **not cancel** the operation itself, it only stops waiting for it:

```csharp
try { await Task.Delay(1000).WaitAsync(TimeSpan.FromMilliseconds(50)); }
catch (TimeoutException e) { Console.WriteLine(e.Message); }   // The operation has timed out.
```

### Telling a timeout from a cancellation

A timeout via a linked token and a cancellation by the caller produce the same `OperationCanceledException`. They are told apart by the outer token: if it is not canceled, the timeout fired. The full pattern is in [exercise 2](exercises.en.md).

## WhenAll, WhenAny, WhenEach

`Task.WhenEach` (since .NET 9) yields tasks **as they complete**, not in the order they were passed:

```csharp
var tasks = new[] { Delay(300, "c"), Delay(100, "a"), Delay(200, "b") };
await foreach (var done in Task.WhenEach(tasks))
    Console.WriteLine($"done: {await done}");
// done: a, done: b, done: c

static async Task<string> Delay(int ms, string s) { await Task.Delay(ms); return s; }
```

`Task.WhenAny` in a loop works similarly but costs O(n²) — for large sets use `WhenEach`.

## Limiting parallelism

`Task.WhenAll` over a thousand tasks starts a thousand operations at once. To limit them, use `Parallel.ForEachAsync` (.NET 6+):

```csharp
await Parallel.ForEachAsync(Enumerable.Range(1, 10),
    new ParallelOptions { MaxDegreeOfParallelism = 3 },
    async (i, ct) => await Task.Delay(30, ct));
```

Verified: at most 3 iterations ran at the same time. The `SemaphoreSlim` variant is in [exercise 3](exercises.en.md).

## ValueTask

`Task` is a class: every allocation costs memory. If a method **often** completes synchronously (a cache, a buffer), return `ValueTask<T>` — a struct that does not allocate in the synchronous case:

```csharp
static ValueTask<int> Cached(Dictionary<int, int> cache, int key) =>
    cache.TryGetValue(key, out var v)
        ? new ValueTask<int>(v)                  // no allocation
        : new ValueTask<int>(SlowAsync(cache, key));

static async Task<int> SlowAsync(Dictionary<int, int> cache, int key)
{
    await Task.Delay(10);
    return cache[key] = key * 2;
}

var cache = new Dictionary<int, int>();
var v1 = Cached(cache, 5); Console.WriteLine(v1.IsCompletedSuccessfully);   // False
await v1;
var v2 = Cached(cache, 5); Console.WriteLine(v2.IsCompletedSuccessfully);   // True
```

`ValueTask` restrictions: **one** `await` per instance, no concurrent `await` from several places, no `.Result` before completion. If you need to hold the result, call `AsTask()`. Return `Task` by default; use `ValueTask` only after measuring ([19 · Performance](../19-performance/overview.en.md)).

## Async streams and resources

`IAsyncEnumerable<T>` and `await foreach` are a sequence whose items appear asynchronously:

```csharp
static async IAsyncEnumerable<int> Numbers([EnumeratorCancellation] CancellationToken ct = default)
{
    for (int i = 1; i <= 3; i++)
    {
        await Task.Delay(10, ct);
        yield return i;
    }
}

await foreach (var n in Numbers()) Console.Write($"{n} ");   // 1 2 3
```

The `[EnumeratorCancellation]` attribute binds the token passed via `.WithCancellation(ct)` to the method parameter. A resource with asynchronous cleanup implements `IAsyncDisposable` and is closed with `await using` (details of `Dispose` are in [12 · Memory and GC](../12-memory-and-gc/overview.en.md)):

```csharp
await using (var r = new Res()) { Console.WriteLine("using body"); }   // using body, then DisposeAsync

class Res : IAsyncDisposable
{
    public async ValueTask DisposeAsync() { await Task.Delay(1); Console.WriteLine("DisposeAsync"); }
}
```

## TaskCompletionSource and PeriodicTimer

`TaskCompletionSource<T>` turns an event or a callback into a `Task`:

```csharp
var tcs = new TaskCompletionSource<string>();
_ = Task.Run(async () => { await Task.Delay(30); tcs.SetResult("from callback"); });
Console.WriteLine(await tcs.Task);   // from callback
```

`PeriodicTimer` is an asynchronous replacement for a `while + Task.Delay` loop: ticks do not pile up and do not drift if processing is fast:

```csharp
using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(30));
int ticks = 0;
while (await timer.WaitForNextTickAsync())
    if (++ticks == 3) break;
Console.WriteLine(ticks);   // 3
```

## Fire-and-forget and async void

Starting without awaiting is acceptable only if errors are handled inside:

```csharp
static async Task DoWorkAsync() { await Task.Delay(10); throw new InvalidOperationException("boom"); }

_ = Task.Run(async () =>
{
    try { await DoWorkAsync(); }
    catch (Exception ex) { Console.WriteLine($"background work failed: {ex.Message}"); }   // otherwise the error is lost
});
await Task.Delay(100);   // background work failed: boom
```

`async void` throws the exception straight into the `SynchronizationContext` (or onto the pool, where it terminates the process). Verified with a context that catches exceptions: the exception from `async void` arrived in `Post` — it is not reachable from `await`. For long-lived background work in applications use `BackgroundService` (the "Background jobs" recipe is not created yet).

## Best practices

| Rule | Why |
|---|---|
| `async` all the way | otherwise deadlock or pool starvation |
| `CancellationToken` as the last parameter, pass it on | otherwise cancellation does not reach the operation |
| `Task.WhenAll` for independent operations | the sum of times becomes the maximum |
| `ConfigureAwait(false)` in libraries | independence from the context |
| `async Task`, not `async void` | it can be awaited and its errors caught |
| Do not wrap I/O in `Task.Run` | an extra thread for no benefit |
| `await` instead of `.Result`/`.Wait()` | see above |
| Do not remove `async`/`await` "for optimization" where there is a `using` or `try` | returning a `Task` without `await` disposes the resource or leaves the `try` before the operation completes |

## Related materials

- Back: [basics](basics.en.md). Next: [expert theory](expert.en.md) — the state machine, `ExecutionContext`, `ValueTask` internals.
- [18 · Concurrency](../18-concurrency/overview.en.md) — `SemaphoreSlim`, `Channel<T>`, `Parallel`.
- [12 · Memory and GC](../12-memory-and-gc/overview.en.md) — `IDisposable`, `IAsyncDisposable`.
- [08 · Exceptions](../08-exceptions/overview.en.md) — `AggregateException`, `when` filters.
- [15 · EF Core](../15-ef-core/overview.en.md) — asynchronous queries (`ToListAsync`).
- [Python](../../../python/overview.en.md) — `asyncio`: `gather` ↔ `Task.WhenAll`, `TaskGroup`.

## Sources

- [ConfigureAwait FAQ](https://devblogs.microsoft.com/dotnet/configureawait-faq/)
- [Async streams (`IAsyncEnumerable<T>`)](https://learn.microsoft.com/dotnet/csharp/asynchronous-programming/generate-consume-asynchronous-stream)
- [`ValueTask<T>`](https://learn.microsoft.com/dotnet/api/system.threading.tasks.valuetask-1)
- [`Task.WhenEach`](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task.wheneach)
- [`Parallel.ForEachAsync`](https://learn.microsoft.com/dotnet/api/system.threading.tasks.parallel.foreachasync)
- [`PeriodicTimer`](https://learn.microsoft.com/dotnet/api/system.threading.periodictimer)
