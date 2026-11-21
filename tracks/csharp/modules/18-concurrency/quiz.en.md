---
title: "Quiz: Multithreading and synchronization"
lang: en
track: csharp
kind: module
doc: quiz
slug: 18-concurrency
part: 3
tags: [concurrency, lock, interlocked, semaphore, deadlock, memory-model]
status: draft
updated: 2026-11-21
---

# Quiz: Multithreading and synchronization

🌐 [Русский](quiz.ru.md) · **English**

> Answer first, then open the answer. The mark shows which level a question comes from: 🟢 [basics](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 Four threads run `_value++` on a shared field a million times each, without synchronization. What happens?

- A) Exactly 4,000,000
- B) An `InvalidOperationException`
- C) A number no greater than 4,000,000, different from run to run
- D) Exactly 1,000,000: the threads overwrite each other completely

<details><summary>Answer</summary>

**C.** `_value++` is a read, an addition and a write. Two threads may read the same value, and one increment is lost. A real run gave 1,966,527. There is no exception: a race corrupts data silently.
→ [Basics](basics.en.md)

</details>

## 2. 🟢 Which object is best to use in `lock`?

- A) `this`
- B) the string literal `"orders"`
- C) `typeof(OrderService)`
- D) `private readonly Lock _gate = new();`

<details><summary>Answer</summary>

**D.** A private object that exists only for locking. `this` and a `Type` can be locked by other code, and string literals are interned and shared by the whole program. Since .NET 9 there is the `System.Threading.Lock` type for this.
→ [Basics](basics.en.md)

</details>

## 3. 🟢 What does `Interlocked.Exchange(ref x, 7)` return if `x == 5`?

- A) `7`
- B) `5`
- C) `true`
- D) `12`

<details><summary>Answer</summary>

**B.** `Exchange` writes the new value and returns the **previous** one. After the call `x == 7`. `CompareExchange` also returns the previous value.
→ [Basics](basics.en.md)

</details>

## 4. 🟢 Which threads keep the process from exiting while they run?

- A) All threads without exception
- B) Threads created with `new Thread` with default settings
- C) Pool threads that run `Task.Run`
- D) None: the process exits together with `Main`

<details><summary>Answer</summary>

**B.** `new Thread` is foreground by default (`IsBackground = False`), and the process waits for it to finish. Pool threads are background threads and end together with the process.
→ [Basics](basics.en.md)

</details>

## 5. 🟢 Select all correct statements

- [ ] A) `Interlocked.Increment` is atomic and needs no `lock`
- [ ] B) If two fields must change consistently, `Interlocked` on each of them is enough
- [ ] C) `lock` is released even if an exception is thrown inside the block
- [ ] D) A regular `Dictionary` is safe for simultaneous writes from several threads

<details><summary>Answer</summary>

**A, C.** Separate atomic operations on two fields do not make the pair atomic — between them another thread sees an intermediate state (B is wrong). `lock` expands into `try/finally` (C is right). `Dictionary` is not designed for simultaneous writes (D is wrong): you need `ConcurrentDictionary` or `lock`.
→ [Basics](basics.en.md)

</details>

## 6. 🟡 What happens to this code?

```csharp
object g = new();
lock (g) { await Task.Delay(1); }
```

- A) It works, but slowly
- B) Compile error CS1996
- C) A `SynchronizationLockException` at run time
- D) A deadlock

<details><summary>Answer</summary>

**B.** `await` inside `lock` is forbidden: after `await` the code may continue on another thread, while `Monitor` is tied to the owner thread. The fix is `SemaphoreSlim(1, 1)` with `await WaitAsync()` and `Release()` in `finally`.
→ [Advanced](advanced.en.md)

</details>

## 7. 🟡 12 asynchronous jobs run through `SemaphoreSlim(3, 3)`: each does `await WaitAsync()`, works for 20 ms and calls `Release()`. How many jobs run at the same time?

- A) 12
- B) No more than 3
- C) Exactly 1
- D) It depends on the number of processor cores

<details><summary>Answer</summary>

**B.** A semaphore with a count of 3 lets no more than three participants through. In a real run the peak was exactly 3. Waiting in `WaitAsync` does not block threads, so the number of cores does not matter here.
→ [Advanced](advanced.en.md)

</details>

## 8. 🟡 A thread called `rw.EnterReadLock()` and then, without releasing it, `rw.EnterWriteLock()` on a `ReaderWriterLockSlim` with default settings. What happens?

- A) The lock is upgraded to write
- B) The thread hangs forever
- C) `LockRecursionException`
- D) The write runs without a lock

<details><summary>Answer</summary>

**C.** With the default `NoRecursion` policy this is an exception: "Write lock may not be acquired with read lock held". The "read and maybe write" scenario uses `EnterUpgradeableReadLock`.
→ [Advanced](advanced.en.md)

</details>

## 9. 🟡 8 threads call `dict.GetOrAdd("key", factory)` on an empty `ConcurrentDictionary` at the same time. How many times does `factory` run?

- A) Exactly 1 — `ConcurrentDictionary` is thread-safe
- B) From 1 to 8
- C) Exactly 8
- D) 0 — the value is created lazily on read

<details><summary>Answer</summary>

**B.** The factory runs outside the dictionary's internal lock. When all 8 threads were inside the factory at the same time, it ran 8 times, and one value ended up in the dictionary. To make the factory run once, the value is wrapped in `Lazy<T>`.
→ [Advanced](advanced.en.md)

</details>

## 10. 🟡 Find the bug

```csharp
void Transfer(Account from, Account to, decimal amount)
{
    lock (from.Gate)
    {
        lock (to.Gate)
        {
            from.Balance -= amount;
            to.Balance += amount;
        }
    }
}
```

<details><summary>Answer</summary>

Opposite transfers `Transfer(a, b)` and `Transfer(b, a)` acquire the locks in opposite order: the first holds `a` and waits for `b`, the second holds `b` and waits for `a`. That is a deadlock. The fix is a single lock order, e.g. by `Id`: first the lock of the account with the smaller `Id`, then the larger one.
→ [Advanced](advanced.en.md)

</details>

## 11. 🔴 Why does this code throw `SynchronizationLockException`?

```csharp
private readonly SpinLock _spin = new(enableThreadOwnerTracking: true);

public void EnterAndExit()
{
    bool taken = false;
    _spin.Enter(ref taken);
    _spin.Exit();
}
```

- A) `SpinLock` cannot be used in instance methods
- B) `SpinLock` is a struct; for a `readonly` field `Enter` and `Exit` work on different defensive copies
- C) Owner tracking forbids calling `Exit` right after `Enter`
- D) `Enter` did not take the lock because `taken` was `false`

<details><summary>Answer</summary>

**B.** To call a method on a `readonly` struct field, the compiler passes a copy. `Enter` locks one copy, and `Exit` is called on another — unlocked — one. The documentation explicitly forbids storing a `SpinLock` in `readonly` fields.
→ [Expert](expert.en.md)

</details>

## 12. 🔴 Select all correct statements about the memory model

- [ ] A) `volatile int x; x++` is atomic
- [ ] B) A `volatile long` field does not compile
- [ ] C) `Interlocked` methods are full memory barriers
- [ ] D) Ordinary reads and writes may be reordered if that does not change single-thread behavior

<details><summary>Answer</summary>

**B, C, D.** `volatile` gives ordering (acquire/release) but not atomicity of compound operations (A is wrong). `volatile long` and `volatile double` are error CS0677. `Interlocked` is a full fence in the .NET memory model. Reordering ordinary operations is allowed as long as single-thread consistency is preserved.
→ [Expert](expert.en.md)

</details>

## 13. 🔴 Open question

A service keeps the request identifier in a `ThreadLocal<string>`, and the request handler is asynchronous. The logs sometimes show an empty or someone else's identifier. Why, and how do you fix it?

<details><summary>Key points of a good answer</summary>

- `ThreadLocal` is tied to an OS thread, and after `await` the code may continue on another pool thread — with its own value or none.
- That pool thread may have served another request before — hence "someone else's" identifier.
- `AsyncLocal<T>` stores the value in the execution context, which flows into continuations after `await` and into new threads: in the verified example a new thread sees `("request-42", null)`.
- The fix is `AsyncLocal<string>`.

</details>
