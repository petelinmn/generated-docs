---
title: "Quiz: Background jobs"
lang: en
track: csharp
kind: recipe
doc: quiz
slug: background-jobs
part: 2
tags: [background-service, hosted-service, channel, graceful-shutdown]
status: draft
updated: 2026-11-22
---

# Quiz: Background jobs

🌐 [Русский](quiz.ru.md) · **English**

> Answer first, then open the answer. The mark shows which level a question comes from: 🟢 [minimal](basics.en.md) · 🟡 [production](advanced.en.md) · 🔴 [scale and edges](expert.en.md).

## 1. 🟢 Which base class suits long-running background work?

- A) `Thread`
- B) `BackgroundService`
- C) `Task.Run` in a controller
- D) a `Timer` in a static field

<details><summary>Answer</summary>

**B.** A hosted service starts and stops with the application and gets a shutdown `CancellationToken`. `Task.Run` from a request loses the work on shutdown.
→ [Minimal](basics.en.md#the-task)

</details>

## 2. 🟢 How should periodic work be done in a worker?

- A) `while (true) Thread.Sleep(…)`
- B) `PeriodicTimer` and `await timer.WaitForNextTickAsync(ct)`
- C) `async void`
- D) a recursive `Task.Run`

<details><summary>Answer</summary>

**B.** The wait is asynchronous, supports cancellation and does not accumulate missed ticks.
→ [Minimal](basics.en.md#periodic-work-backgroundservice--periodictimer)

</details>

## 3. 🟢 What does `TryWrite` of a third item into a bounded channel of capacity 2 return?

- A) `True`
- B) `False`
- C) throws
- D) overwrites the first item

<details><summary>Answer</summary>

**B.** Verified: `True True False`. An `await WriteAsync` in this case waits until the reader frees a slot.
→ [Minimal](basics.en.md#a-bounded-queue-and-backpressure)

</details>

## 4. 🟢 What happens with `FullMode = DropOldest` if you write 1..4 into a channel of capacity 2?

- A) `1,2` remain
- B) `3,4` remain
- C) an exception
- D) the channel blocks

<details><summary>Answer</summary>

**B.** Verified: `3,4` remained — old items are pushed out by new ones.
→ [Minimal](basics.en.md#a-bounded-queue-and-backpressure)

</details>

## 5. 🟡 How do you get a scoped service (for example `DbContext`) in a worker?

- A) inject it into the constructor
- B) create a scope through `IServiceScopeFactory` per unit of work
- C) make it static
- D) register it as a singleton

<details><summary>Answer</summary>

**B.** A worker is a singleton; constructor injection of a scoped service is a captive dependency. Verified: the service was obtained from `CreateScope()`.
→ [Production](advanced.en.md#scoped-dependencies)

</details>

## 6. 🟡 What happens by default if `ExecuteAsync` throws an unhandled exception?

- A) nothing
- B) the host stops (`StopHost`)
- C) the worker restarts itself
- D) the exception is swallowed

<details><summary>Answer</summary>

**B.** Verified: `ApplicationStopping` fired. With `Ignore` there is no stop, the worker simply dies.
→ [Production](advanced.en.md#exceptions-in-a-worker)

</details>

## 7. 🟡 Select all correct statements

- [ ] A) the token `ct` is cancelled when the host stops
- [ ] B) `try/catch` inside the loop per unit of work protects the worker from one failing job
- [ ] C) `ShutdownTimeout` forcibly kills the worker
- [ ] D) an unbounded channel grows in memory with a slow consumer

<details><summary>Answer</summary>

**A, B, D.** C is wrong: verified — `StopAsync` returned without an exception in ~300 ms, and the worker ignoring the token kept running.
→ [Production](advanced.en.md#graceful-shutdown)

</details>

## 8. 🟡 Open question: how would you process a queue with four threads and backpressure?

<details><summary>Key points of a good answer</summary>

- a bounded `Channel<T>` (`Wait`) — the producer waits;
- `Parallel.ForEachAsync` over `ReadAllAsync` with `MaxDegreeOfParallelism = 4`, or 4 readers;
- a scope per item, `ct` in every wait;
- `try/catch` per item, logging.

</details>

## 9. 🔴 The app runs in 3 instances, a `BackgroundService` sends emails hourly. How many emails does a customer get?

- A) one
- B) three — the timer runs in every instance
- C) zero
- D) depends on `PeriodicTimer`

<details><summary>Answer</summary>

**B.** You need a lock/lease, a single worker process or a scheduler with coordination.
→ [Scale and edges](expert.en.md#edges-and-gotchas)

</details>

## 10. 🔴 Why is `Channel<T>` unsuitable for jobs that must not be lost?

- A) it is slow
- B) the in-memory queue disappears on a crash/restart and belongs to one instance
- C) it does not support async
- D) it is not thread-safe

<details><summary>Answer</summary>

**B.** For critical jobs you need a durable queue: an outbox in the DB, a message broker.
→ [Scale and edges](expert.en.md#scale-when-channelt-is-not-enough)

</details>
