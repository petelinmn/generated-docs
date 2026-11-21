---
title: "Multithreading and synchronization: expert"
lang: en
track: csharp
kind: module
doc: expert
slug: 18-concurrency
part: 3
tags: [concurrency, memory-model, volatile, monitor, lock-free, compare-and-swap, spinlock, async-local, thread-pool]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-21
---

# Multithreading and synchronization: expert

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · **3. Expert** · [4. Quiz](quiz.en.md) · [exercises](exercises.en.md) · [5–6. Code and tests](code/)

## How lock works

For an object that is not a `System.Threading.Lock`, the statement `lock (x) { … }` compiles to `Monitor.Enter`/`Monitor.Exit` inside `try/finally`. The same code by hand:

```csharp
object gate = new();
int shared = 0;
bool lockTaken = false;
try
{
    Monitor.Enter(gate, ref lockTaken);
    shared++;
}
finally
{
    if (lockTaken)
    {
        Monitor.Exit(gate);
    }
}
```

If the compiler knows that `x` is of type `System.Threading.Lock` (.NET 9+), `lock` becomes `using (x.EnterScope()) { … }`. `EnterScope()` returns a `ref struct` with a `Dispose()` method. Properties of both mechanisms:

- both are **reentrant**: a thread that already owns the lock can enter it again — a nested `lock (newLock) { lock (newLock) { … } }` works;
- both are tied to the **owner thread**: `Monitor.Exit` from a thread that does not own the lock throws `SynchronizationLockException` ("The calling thread does not hold the lock");
- `Lock` works this way only if the expression type is exactly `Lock`. If it is converted to `object` and locked, a plain `Monitor` is used, and the compiler produces warning CS9216.

## The memory model: volatile, Volatile and barriers

Synchronization solves two problems: **atomicity** (nobody sees an operation half-done) and **visibility/ordering** (one thread's changes are visible to another in the right order). The .NET memory model lets the compiler and the processor reorder ordinary reads and writes as long as that preserves consistency **within a single thread**. For other threads the order is not guaranteed.

| Tool | Guarantee |
|---|---|
| `lock`, `Monitor`, `Lock` | mutual exclusion plus full visibility of the changes made under the lock |
| `Interlocked.*` | atomicity and a **full fence** |
| `Volatile.Read` | acquire: later reads and writes do not execute before it |
| `Volatile.Write` | release: earlier reads and writes do not execute after it |
| a `volatile` field | every read is a `Volatile.Read`, every write is a `Volatile.Write` |

Limitations of `volatile`:

- it gives no atomicity for anything except assignment: `volatile int x; x++` is still a race;
- it applies only to fields of reference types, pointers, `sbyte`, `byte`, `short`, `ushort`, `int`, `uint`, `char`, `float`, `bool`, enums based on them, and `IntPtr`/`UIntPtr`. `volatile long` and `volatile double` are error CS0677;
- on a multiprocessor system a volatile read does not guarantee that you get the latest value written.

64-bit reads are atomic in a 64-bit process. In a 32-bit process a `long` read can be "torn", and `Interlocked.Read` exists for that.

**Publishing an object.** Assigning a reference to a location accessible to other threads is a release operation with respect to the object's fields. So if a thread created an object and stored the reference in a shared field, another thread that reads the reference sees a fully constructed object. Safe lazy creation with a double check relies on this ([exercise 2](exercises.en.md)): the fast path reads the field with `Volatile.Read`, the slow path creates the object under `lock`.

## Monitor.Wait and Monitor.Pulse

`Monitor` can not only exclude but also wait for a condition. `Monitor.Wait(gate)` atomically **releases** the lock and puts the thread to sleep, and re-acquires the lock before returning. `Pulse` wakes one waiter, `PulseAll` wakes all of them. Waiting queues are built on this:

```csharp
public void Add(T item)
{
    lock (_gate)
    {
        // Always wait in a loop: after waking up, the condition may already be false again.
        while (_items.Count == capacity)
        {
            Monitor.Wait(_gate); // releases the lock while waiting, re-acquires it before returning
        }
        _items.Enqueue(item);
        Monitor.PulseAll(_gate);
    }
}
```

The condition is checked **in a `while` loop**, not in an `if`: the woken thread competes for the lock again, and by the time it gets it, another thread may have already taken the space or the item. A buffer with capacity 4 passes 1000 numbers from a producer to a consumer without losses; the sum is `500500`. Application code uses `Channel<T>` instead. Code: [Ex09_BlockingBuffer.cs](code/Examples/Ex09_BlockingBuffer.cs).

## Lock-free: CompareExchange

`Interlocked.CompareExchange(ref location, value, comparand)` atomically writes `value` only if `location` still holds `comparand`, and returns the previous value. The "read → compute → try to write → retry on failure" loop (compare-and-swap, CAS) lets you build data structures without locks:

```csharp
public void Push(T value)
{
    var node = new Node(value, null);
    while (true)
    {
        Node? head = Volatile.Read(ref _head);
        node.Next = head;
        // Publish the node only if nobody changed the head since we read it; otherwise retry.
        if (Interlocked.CompareExchange(ref _head, node, head) == head)
        {
            return;
        }
    }
}
```

10,000 parallel `Push` calls give exactly 10,000 items. Lock-free code does not block threads, but it is hard to get right: under heavy contention threads repeat the loop many times for nothing. The classic ABA problem (a value changes to another and back) does not arise in this stack on .NET: while a thread holds a reference to a node, the garbage collector does not free it and reuse its memory for a new node. Code: [Ex10_LockFreeStack.cs](code/Examples/Ex10_LockFreeStack.cs).

## SpinLock and SpinWait

`SpinLock` does not put the thread to sleep; it spins in a loop until the lock is free. This pays off only for very short sections, and the documentation advises using it only if measurements show a gain. The main trap is that `SpinLock` is a **struct**:

```csharp
// Wrong: readonly makes the compiler pass a defensive copy to Enter and to Exit.
#pragma warning disable IDE0044
private readonly SpinLock _spin = new(enableThreadOwnerTracking: true);
#pragma warning restore IDE0044

public void EnterAndExit()
{
    bool taken = false;
    _spin.Enter(ref taken); // locks a temporary copy
    _spin.Exit();           // unlocks another copy, which was never locked
}
```

The call throws `SynchronizationLockException`: `Exit` is called on a copy that was never locked. This is the same defensive copy as with a mutable struct in an `in` parameter (see [03 · Control flow and methods](../03-control-flow-and-methods/expert.en.md)). A `SpinLock` must not be copied and must not be stored in a `readonly` field. Besides, it is **not reentrant**: a second `Enter` by the same thread with owner tracking enabled throws `LockRecursionException`. `SpinWait` is a helper for your own wait loops: it decides by itself when to spin and when to yield the processor. Code: [Ex11_SpinLockPitfall.cs](code/Examples/Ex11_SpinLockPitfall.cs).

## Per-thread data: AsyncLocal, ThreadLocal, ThreadStatic

| Type | The value is tied to | After `await` / in a new thread |
|---|---|---|
| a `[ThreadStatic] static` field | an OS thread | each thread has its own value |
| `ThreadLocal<T>` | an OS thread, with an initial-value factory | each thread has its own value |
| `AsyncLocal<T>` | the logical flow of execution (ExecutionContext) | the value "moves" together with asynchronous code |

```csharp
var asyncLocal = new AsyncLocal<string>();
using var threadLocal = new ThreadLocal<string>();
asyncLocal.Value = "request-42";
threadLocal.Value = "request-42";

(string?, string?) seen = default;
// Thread.Start captures the current ExecutionContext, and AsyncLocal values live in it.
var thread = new Thread(() => seen = (asyncLocal.Value, threadLocal.Value));
thread.Start();
thread.Join();
```

The new thread sees `("request-42", null)`: the `AsyncLocal` value arrived with the execution context, while `ThreadLocal` is empty in the new thread. That is why request data (an identifier, the user) in asynchronous code is kept in `AsyncLocal`, not in `ThreadLocal`: after `await`, the code may continue on another thread. Code: [Ex12_ContextFlow.cs](code/Examples/Ex12_ContextFlow.cs).

## The thread pool from the inside

- The pool creates worker threads on demand until their number reaches the minimum (`ThreadPool.GetMinThreads`). After that it may add threads or wait until busy ones free up. On the author's machine the minimum equals the number of logical processors: `12` with `Environment.ProcessorCount = 12`.
- The number of pool threads is limited. If many pool threads are blocked (`.Result`, `.Wait()`, `Thread.Sleep`, synchronous I/O), new tasks cannot start — this is **thread pool starvation**. The cure is asynchronous code without blocking waits.
- `TaskCreationOptions.LongRunning` is a hint to the scheduler. The default scheduler runs such a task not on a pool thread (`IsThreadPoolThread = False`), so long blocking work does not take a thread away from the pool.

## Reference

### Synchronization primitives

| Primitive | Kind | Reentrancy | Owner | Async | Across processes |
|---|---|---|---|---|---|
| `lock` / `Monitor` | lightweight | yes | thread | no | no |
| `System.Threading.Lock` (.NET 9+) | lightweight | yes | thread | no | no |
| `SpinLock` | lightweight, struct | no (`LockRecursionException`) | thread (if tracking is on) | no | no |
| `Interlocked` | atomic operations | — | — | — | no |
| `SemaphoreSlim` | lightweight | no: a second `Wait` by the same thread waits like any other | no | `WaitAsync` | no |
| `Semaphore` | `WaitHandle` | no | no | no | named — Windows only |
| `Mutex` | `WaitHandle` | yes | thread: `ReleaseMutex` from another thread throws `ApplicationException` | no | yes; on Unix scoped to the user by default |
| `ReaderWriterLockSlim` | lightweight | no by default (`NoRecursion`) | thread | no | no |
| `ManualResetEventSlim` | lightweight | — | — | no | no |
| `AutoResetEvent`, `ManualResetEvent`, `EventWaitHandle` | `WaitHandle` | — | — | no | a named `EventWaitHandle` — Windows only |
| `CountdownEvent`, `Barrier` | lightweight | — | — | no | no |
| `Channel<T>` | asynchronous queue | — | — | yes | no |

Lightweight types do not use OS kernel objects and are usually faster, but they work only within a process.

### Waiting with a timeout and cancellation

| Primitive | Waiting with a timeout | Cancellation |
|---|---|---|
| `Monitor` | `Monitor.TryEnter(obj, timeout)` | no |
| `SemaphoreSlim` | `Wait(timeout)`, `WaitAsync(timeout)` | `CancellationToken` |
| `ManualResetEventSlim`, `CountdownEvent`, `Barrier` | `Wait(timeout)`, `SignalAndWait(timeout)` | `CancellationToken` |
| `WaitHandle` (`Mutex`, `Semaphore`, events) | `WaitOne(timeout)`, `WaitHandle.WaitAll/WaitAny` | no |
| `Channel<T>` | — | `CancellationToken` in `ReadAsync`/`WriteAsync` |

### Thread-safe collections

| Collection | Order | Feature |
|---|---|---|
| `ConcurrentDictionary<TKey, TValue>` | none | atomic `TryAdd`, `TryRemove`, `AddOrUpdate`, `GetOrAdd`; delegates run outside the lock |
| `ConcurrentQueue<T>` | FIFO | `TryDequeue` |
| `ConcurrentStack<T>` | LIFO | `TryPop`, `PushRange` |
| `ConcurrentBag<T>` | none | fastest when a thread consumes what it added itself |
| `BlockingCollection<T>` | depends on the storage | blocking `Add`/`Take`, bounded capacity; new code uses `Channel<T>` |
| `ImmutableDictionary`, `ImmutableList` | — | immutable: a "change" creates a new collection, reads need no locks |

## Gotchas

### Lock converted to object

```csharp
Lock l = new();
object o = l;
lock (o) { }   // warning CS9216: Monitor-based locking is used
```

The new fast mechanism works only if the static type of the expression is `Lock`. Through `object`, you get a plain `Monitor`.

### await inside lock

`lock (gate) { await Task.Delay(1); }` is compile error CS1996. After `await` the code may continue on another thread, while `Monitor` is tied to the owner thread. The fix is `SemaphoreSlim.WaitAsync`.

### lock on a value type

`int v = 1; lock (v) { }` is error CS0185: only a reference type can be locked. Boxing on every call would create a new object (see [02 · Types and variables](../02-types-and-variables/advanced.en.md)), and the lock would protect nothing.

### volatile does not make ++ atomic

`volatile` guarantees ordering and visibility, but `x++` still consists of a read and a write. Counters use `Interlocked.Increment`. `volatile long` does not compile at all (CS0677).

### Interlocked and a property

`Interlocked.Increment(ref c.P)` on a property is error CS0206: `ref` needs a variable or a field, and a property is a pair of methods. An atomically updated value is stored in a field.

### SemaphoreSlim is not reentrant

```csharp
var sem = new SemaphoreSlim(1, 1);
sem.Wait();
sem.Wait(100);   // False: the same thread waits like any other
```

A semaphore has no owner, only a count. A recursive call of a method that waits on the same `SemaphoreSlim(1, 1)` again hangs forever without a timeout.

### ReaderWriterLockSlim and recursion

With the default `NoRecursion` policy, a second `EnterWriteLock` by the same thread throws `LockRecursionException` ("Recursive write lock acquisitions not allowed in this mode"), as does an attempt to take a write lock while holding a read lock.

### Mutex remembers its owner

A `Mutex` can be acquired again on the same thread (`WaitOne` twice), but released only on that thread: `ReleaseMutex` from another thread throws `ApplicationException` ("Object synchronization method was called from an unsynchronized block of code"). If a thread exits without releasing the mutex, the next thread that acquires it gets `AbandonedMutexException`. Because of the thread affinity, `Mutex` cannot be used around `await`.

### Enumerating a dictionary while it changes

A regular `Dictionary` throws `InvalidOperationException` ("Collection was modified") if an item is added during `foreach`. `ConcurrentDictionary` throws no exception in the same situation, but the enumeration may or may not see the new items.

### Random from several threads

`Random` is not thread-safe. The documentation warns that without synchronization its methods start returning 0. A shared generator is available as `Random.Shared`.

## Trade-offs and alternatives

| Approach | Pros | Cons |
|---|---|---|
| `lock` / `Lock` | simple, clear, reentrant | blocks threads; cannot be used with `await` |
| `Interlocked` | lock-free, fast | only one variable |
| Lock-free structures | no locks and no deadlocks | hard to write and to verify; retries under contention |
| Concurrent collections | ready-made and tested | only individual operations are atomic, not sequences of them |
| Immutable data | reads need no synchronization | every change creates a new object |
| Message passing (`Channel<T>`) | no shared state | a different architectural style; queuing delays |

The order of choice: first remove shared mutable state; if that fails — `Interlocked` for one variable, `lock` for several, specialized primitives only for a specific task.

## Interview questions

**1. What is a data race, and why is `count++` not thread-safe?**

<details><summary>Answer</summary>

A race is a situation where the result depends on the order in which threads execute. `count++` consists of a read, an addition and a write. Two threads may read the same value, and one increment gets lost: 4 threads with a million iterations each gave 1,966,527 instead of 4,000,000.

</details>

**2. What does `lock` compile to?**

<details><summary>Answer</summary>

For a regular object — `Monitor.Enter(obj, ref taken)` inside `try` and `Monitor.Exit` in `finally`. For an expression of type `System.Threading.Lock` (.NET 9+) — `using (x.EnterScope())`.

</details>

**3. Why is `System.Threading.Lock` better than `lock` on an `object`?**

<details><summary>Answer</summary>

It is a separate type created only for mutual exclusion, and the documentation recommends it for best performance. The gotcha: if a `Lock` is converted to `object`, a plain `Monitor` is used and the compiler warns (CS9216).

</details>

**4. Why can't you `await` inside `lock`? What do you use instead?**

<details><summary>Answer</summary>

It is compile error CS1996. `Monitor` is tied to a thread, and after `await` execution may continue on another thread. Instead of `lock` — `SemaphoreSlim(1, 1)` with `await WaitAsync()` and `Release()` in `finally`.

</details>

**5. How does `SemaphoreSlim` differ from `Semaphore`, and `Mutex` from `lock`?**

<details><summary>Answer</summary>

`SemaphoreSlim` is lightweight, in-process, and has `WaitAsync`. `Semaphore` is a kernel object and can be named to synchronize processes (Windows only). `Mutex` is also a kernel object: it works across processes, but it is slower than `lock` and, like `lock`, tied to the owner thread.

</details>

**6. What is a deadlock, and how do you avoid it?**

<details><summary>Answer</summary>

Threads wait for each other in a cycle. Ways out: a single lock order, holding one lock at a time, timeouts (`TryEnter`, `Wait(timeout)`), not calling foreign code under a lock.

</details>

**7. What does `volatile` guarantee and what doesn't it?**

<details><summary>Answer</summary>

It guarantees ordering: a read is acquire, a write is release. It does not guarantee atomicity of compound operations (`x++`) and does not guarantee that a read sees the very latest value on a multiprocessor system. It cannot be applied to `long` and `double`.

</details>

**8. Why does lazy initialization need a double check, and why `Volatile.Read` there?**

<details><summary>Answer</summary>

The first check without a lock makes the common path fast; the second, under `lock`, prevents two threads from creating the object twice. `Volatile.Read`/`Volatile.Write` order the read and the publication of the reference. In application code it is simpler to use `Lazy<T>` or `LazyInitializer`.

</details>

**9. 🪤 How many times does the factory run in `ConcurrentDictionary.GetOrAdd(key, factory)` when called from 8 threads at once?**

<details><summary>Answer</summary>

From 1 to 8: the factory runs outside the dictionary's lock. Verified — 8 times when all threads were inside the factory at the same time. One value ends up in the dictionary. If the factory is expensive — `GetOrAdd(key, _ => new Lazy<T>(factory)).Value`.

</details>

**10. 🪤 Why does a `SpinLock` in a `readonly` field throw `SynchronizationLockException`?**

<details><summary>Answer</summary>

`SpinLock` is a struct. For a `readonly` field the compiler passes a defensive copy to every call: `Enter` locks one copy, and `Exit` is called on another, unlocked one.

</details>

**11. 🪤 A thread holds a `SemaphoreSlim(1, 1)` and calls `Wait()` again. What happens?**

<details><summary>Answer</summary>

The thread hangs: a semaphore has no owner, and a second wait by the same thread is no different from anyone else's. `lock`, `Lock` and `Mutex` are reentrant in this situation.

</details>

**12. 🪤 Can a `Mutex` be released from another thread?**

<details><summary>Answer</summary>

No: `ReleaseMutex` from a thread that does not own the mutex throws `ApplicationException`. That is why `Mutex` is not used around `await`.

</details>

**13. What is thread pool starvation?**

<details><summary>Answer</summary>

When pool threads are blocked by waits (`.Result`, `.Wait()`, `Thread.Sleep`) and the pool is limited, new tasks cannot start and the app "freezes". The cure is asynchronous code without blocking waits; for long blocking work — `TaskCreationOptions.LongRunning`.

</details>

**14. How does `AsyncLocal<T>` differ from `ThreadLocal<T>`?**

<details><summary>Answer</summary>

`ThreadLocal` stores a value per OS thread. `AsyncLocal` stores it in the execution context, which flows into continuations after `await` and into new threads. Request data in asynchronous code needs `AsyncLocal`.

</details>

**15. Why is the condition for `Monitor.Wait` checked in a `while` loop?**

<details><summary>Answer</summary>

After waking up, the thread competes for the lock again. While it was getting it, another thread may have changed the state, and the condition may be false again. `while` re-checks it after every wake-up.

</details>

## Cheat sheet

| Task | How |
|---|---|
| Protect data | `private readonly Lock _gate = new();` + `lock (_gate) { … }` |
| A counter | `Interlocked.Increment(ref _count)` |
| A lock with `await` | `await _sem.WaitAsync(); try { … } finally { _sem.Release(); }` |
| Limit parallelism | `new SemaphoreSlim(N, N)` or `Parallel.ForEachAsync` with `MaxDegreeOfParallelism` |
| Many readers | `ReaderWriterLockSlim` |
| A producer–consumer queue | `Channel.CreateBounded<T>(capacity)` |
| An expensive factory in a dictionary | `ConcurrentDictionary<K, Lazy<V>>` |
| Wait with a timeout | `Monitor.TryEnter(obj, timeout)`, `Wait(timeout)` |
| Request data in async code | `AsyncLocal<T>` |
| Random numbers from many threads | `Random.Shared` |

## Related materials

- Earlier: [advanced theory](advanced.en.md)
- Defensive copies of structs: [03 · Control flow and methods](../03-control-flow-and-methods/expert.en.md)
- Boxing value types: [02 · Types and variables](../02-types-and-variables/advanced.en.md)
- The garbage collector and object lifetime: [12 · Memory and GC](../12-memory-and-gc/expert.en.md)
- How EF Core detects concurrent use of a context: [15 · EF Core](../15-ef-core/expert.en.md#the-concurrent-use-detector)
- The same idea in other tracks: [Python](../../../python/overview.en.md) — the GIL and concurrency models

## Sources

- [The lock statement](https://learn.microsoft.com/dotnet/csharp/language-reference/statements/lock)
- [`System.Threading.Lock`](https://learn.microsoft.com/dotnet/api/system.threading.lock)
- [The volatile keyword](https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/volatile)
- [The .NET memory model](https://github.com/dotnet/runtime/blob/main/docs/design/specs/Memory-model.md)
- [`Interlocked.Read`](https://learn.microsoft.com/dotnet/api/system.threading.interlocked.read)
- [`SpinLock`](https://learn.microsoft.com/dotnet/api/system.threading.spinlock)
- [`Mutex`](https://learn.microsoft.com/dotnet/api/system.threading.mutex)
- [`AsyncLocal<T>`](https://learn.microsoft.com/dotnet/api/system.threading.asynclocal-1)
- [Overview of synchronization primitives](https://learn.microsoft.com/dotnet/standard/threading/overview-of-synchronization-primitives)
- [The managed thread pool](https://learn.microsoft.com/dotnet/standard/threading/the-managed-thread-pool)
