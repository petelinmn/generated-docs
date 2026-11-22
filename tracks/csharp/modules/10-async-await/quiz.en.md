---
title: "Quiz: Async/await"
lang: en
track: csharp
kind: module
doc: quiz
slug: 10-async-await
part: 2
tags: [async-await, task, configureawait, deadlock, valuetask, cancellation-token]
status: draft
updated: 2026-11-22
---

# Quiz: Async/await

🌐 [Русский](quiz.ru.md) · **English**

> Answer first, then open the answer. The mark shows which level a question comes from: 🟢 [basics](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 What happens to the thread when a method reaches an `await` on an incomplete task?

- A) the thread blocks until the task completes
- B) the method is suspended, control returns to the caller, the thread is free
- C) a new thread is created that waits for the task
- D) the method keeps running and the result is substituted later

<details><summary>Answer</summary>

**B.** `await` does not block: the method becomes a state machine, returns an incomplete task to the caller, and the continuation runs after the wait is over. A is `.Wait()`/`.Result`. C is wrong: waiting for I/O occupies no thread at all. D describes fire-and-forget, not `await`.
→ [Basics](basics.en.md#what-happens-at-await)

</details>

## 2. 🟢 What does the code print?

```csharp
static async Task Sync()
{
    Console.WriteLine("before await");
    await Task.Yield();
    Console.WriteLine("after await");
}

var t = Sync();
Console.WriteLine("returned to caller");
await t;
```

<details><summary>Answer</summary>

```text
before await
returned to caller
after await
```

Code before the first incomplete `await` runs synchronously on the caller's thread, so "before await" is printed first. `Task.Yield()` forces the method to suspend — the caller gets control back and prints the second line.
→ [Basics](basics.en.md#what-happens-at-await)

</details>

## 3. 🟢 Two independent operations of 200 ms each. How do you run them in ~200 ms?

- A) `await a(); await b();`
- B) `var ta = a(); var tb = b(); await Task.WhenAll(ta, tb);`
- C) `await Task.WhenAny(a(), b());`
- D) `Task.Run(() => a().Wait()); await b();`

<details><summary>Answer</summary>

**B.** Both operations start immediately ("hot" tasks), and `WhenAll` waits for both. A runs them sequentially, ~400 ms (verified). C returns after the first one completes — the second is left unawaited and its exceptions are lost. D blocks a pool thread while waiting.
→ [Basics](basics.en.md#sequential-and-parallel)

</details>

## 4. 🟢 Select all correct statements

- [ ] A) `async void` is appropriate for event handlers
- [ ] B) an exception from an asynchronous method whose task is never awaited is thrown to the caller immediately
- [ ] C) `Task.Run` is needed to make I/O asynchronous
- [ ] D) a method that may run for a long time should accept a `CancellationToken`

<details><summary>Answer</summary>

**A, D.** A: event handlers have a `void` signature; this is the only justified use. B is wrong: the exception is stored in the task and thrown only at `await` (or lost if nobody awaits). C is wrong: `Task.Run` occupies a pool thread and does not make I/O asynchronous — you need a truly asynchronous API. D is right: a token is the standard way of cooperative cancellation.
→ [Basics](basics.en.md#beginner-mistakes)

</details>

## 5. 🟢 What exception type will be in `catch`?

```csharp
using var cts = new CancellationTokenSource(100);
try { await Task.Delay(1000, cts.Token); }
catch (OperationCanceledException e) { Console.WriteLine(e.GetType().Name); }
```

- A) `OperationCanceledException`
- B) `TaskCanceledException`
- C) `TimeoutException`
- D) `AggregateException`

<details><summary>Answer</summary>

**B.** `Task.Delay` with a canceled token throws `TaskCanceledException` — a subclass of `OperationCanceledException` (which is why a `catch` of the base type catches it). `TimeoutException` is thrown by `Task.WaitAsync`, not by a token. `AggregateException` appears only with `Wait()`/`Result`.
→ [Basics](basics.en.md#cancellation-cancellationtoken)

</details>

## 6. 🟡 What is wrong with this code (a UI handler with a synchronization context)?

```csharp
void Button_Click(object sender, EventArgs e)
{
    var data = LoadAsync().Result;
    label.Text = data;
}

async Task<string> LoadAsync()
{
    await Task.Delay(100);
    return "ok";
}
```

<details><summary>Answer</summary>

A deadlock. The UI thread blocks on `.Result`, while the continuation of `LoadAsync` after `await` wants to return to exactly that UI context, which is busy. Verified on a single-threaded context: `Wait(500)` returned `False`. The fix: make the handler `async void` and use `await LoadAsync()`. For library code the alternative is `ConfigureAwait(false)` in `LoadAsync` (the continuation then does not need the context), but blocking the thread remains bad practice.
→ [Advanced theory](advanced.en.md#sync-over-async-and-deadlock)

</details>

## 7. 🟡 What does the code print?

```csharp
static async Task F(string m) { await Task.Delay(10); throw new Exception(m); }

var all = Task.WhenAll(F("one"), F("two"));
try { await all; }
catch (Exception e) { Console.WriteLine($"{e.Message}; {all.Exception!.InnerExceptions.Count}"); }
```

- A) `one; 1`
- B) `one; 2`
- C) `two; 2`
- D) `One or more errors occurred. (…)`, then `2`

<details><summary>Answer</summary>

**B.** `await` unwraps the `AggregateException` and throws the **first** exception (`one`); all exceptions remain in `Task.Exception` — there are two. D would happen with `all.Wait()`: there you get an `AggregateException` whose message starts with "One or more errors occurred." (verified: the order of the inner messages is not guaranteed).
→ [Advanced theory](advanced.en.md#exceptions-and-aggregateexception)

</details>

## 8. 🟡 Select all correct statements about `ValueTask<T>`

- [ ] A) it avoids allocating memory when the operation completes synchronously
- [ ] B) a single instance can be safely awaited from several places in parallel
- [ ] C) `AsTask()` gives a regular task if the result must be stored
- [ ] D) it is always faster than `Task`

<details><summary>Answer</summary>

**A, C.** A is the main purpose of the type. B is wrong: the contract is at most one `await`. C is right. D is wrong: the struct is copied, and in the asynchronous case there is no gain; apply it after measuring.
→ [Advanced theory](advanced.en.md#valuetask)

</details>

## 9. 🟡 Where is `ConfigureAwait(false)` needed?

- A) in UI button handlers that change form controls after `await`
- B) in library code that does not know the caller's environment
- C) in every ASP.NET Core controller method, otherwise there will be a deadlock
- D) nowhere: it is obsolete

<details><summary>Answer</summary>

**B.** A library must not impose a return to the caller's context. A is wrong: after `await` the UI thread is needed there, and `ConfigureAwait(false)` would break the code. C is wrong: ASP.NET Core has no synchronization context, so a deadlock is impossible and `ConfigureAwait(false)` changes nothing. D is wrong: in libraries it remains the recommended practice.
→ [Advanced theory](advanced.en.md#synchronization-context-and-configureawait)

</details>

## 10. 🟡 Open question: how do you add a timeout to an asynchronous operation that accepts a `CancellationToken`, and distinguish it from cancellation by the caller?

<details><summary>Key points of a good answer</summary>

- create a linked source: `CancellationTokenSource.CreateLinkedTokenSource(outerToken)`;
- call `cts.CancelAfter(timeout)` and pass `cts.Token` to the operation;
- catch `OperationCanceledException` with the filter `when (!outerToken.IsCancellationRequested)` — that means it was the timeout — and throw `TimeoutException`;
- do not confuse it with `Task.WaitAsync(timeout)`: it stops waiting but does not cancel the operation itself;
- dispose the source with `using`.

</details>

## 11. 🔴 What does the code print?

```csharp
var al = new AsyncLocal<string>();
al.Value = "outer";

static async Task SetIt(AsyncLocal<string> a) { a.Value = "set in async method"; await Task.Yield(); }
static void SetSync(AsyncLocal<string> a) { a.Value = "set in sync method"; }

await SetIt(al);
Console.WriteLine(al.Value);
SetSync(al);
Console.WriteLine(al.Value);
```

- A) `set in async method` / `set in sync method`
- B) `outer` / `set in sync method`
- C) `outer` / `outer`
- D) `set in async method` / `outer`

<details><summary>Answer</summary>

**B.** `ExecutionContext` flows down the chain, but an `async` method works on its own copy: after it returns, the change is not visible to the caller. A synchronous method changes the caller's context directly. Verified by running.
→ [Expert theory](expert.en.md#execution-context-executioncontext-and-asynclocalt)

</details>

## 12. 🔴 Find the bug

```csharp
await Task.Factory.StartNew(async () =>
{
    await Task.Delay(500);
    Console.WriteLine("work done");
});
Console.WriteLine("continue");
```

<details><summary>Answer</summary>

`StartNew` with an `async` lambda returns `Task<Task>`: `await` waits only for the outer task, which completes when the lambda reaches its first `await`. So "continue" is printed before "work done" (verified: the `await` returned in under 400 ms). The fix is `await Task.Run(async () => …)`, which unwraps the nested task, or `await (await Task.Factory.StartNew(…))`.
→ [Expert theory](expert.en.md#taskrun-and-taskfactorystartnew)

</details>
