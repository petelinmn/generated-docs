---
title: "The event loop and concurrency"
lang: en
track: javascript
kind: module
doc: overview
slug: 13-event-loop-and-concurrency
part: 2
levels: [1, 2, 3, 4]
tags: [event-loop, macrotask, microtask, timers, set-immediate, next-tick, worker-threads, atomics, shared-array-buffer]
versions: "ECMAScript 2025+, Node.js 24.15 LTS"
status: draft
updated: 2026-11-26
---

# The event loop and concurrency

🌐 [Русский](overview.ru.md) · **English**

## In short

JavaScript runs code in a single thread. Concurrency is created by the runtime: the event loop takes ready tasks from queues and runs them one at a time to completion. Long operations (I/O, timers) go "to the background" and come back as callbacks. For true parallel computation, worker threads are used, exchanging messages and sharing memory.

## Key ideas

- **One thread, one task at a time:** while synchronous code runs, nothing else happens.
- **Macrotasks** (timers, I/O, `setImmediate`) and **microtasks** (`then`, `queueMicrotask`) are different queues; microtasks run to completion after every macrotask.
- **`process.nextTick`** is a separate Node.js queue served before promise microtasks.
- **`setTimeout(fn, 0)` is not "at once":** it is at least one iteration of the loop.
- **Blocking code delays everything:** timers, responses, request handling.
- **Inside an I/O callback `setImmediate` always comes before `setTimeout(…, 0)`.**
- **Worker threads** run code in parallel; data is copied (structured clone), transferred or shared (`SharedArrayBuffer`).
- **`Atomics`** make operations on shared memory safe.
- **I/O does not need worker threads:** it is already asynchronous; they are for CPU load.

## Minimal example

```js
console.log('1 sync');
setTimeout(() => console.log('4 timeout'), 0);
Promise.resolve().then(() => console.log('3 microtask'));
console.log('2 sync');
```

```text
1 sync
2 sync
3 microtask
4 timeout
```

First all the synchronous code, then microtasks, then the timer.

## When to use and when not

| Task | Choice |
|---|---|
| I/O, network, timers | ordinary asynchrony, no threads needed |
| Heavy computation (hash, compression, parsing) | a worker thread |
| Splitting long work without blocking the loop | chunks with `setImmediate` |
| A shared counter between threads | `SharedArrayBuffer` + `Atomics` |
| Failure isolation | a separate process (`child_process`, [module 16](../../overview.en.md)) |

- ❌ Do not run long synchronous loops in a request handler.
- ❌ Do not rely on the exact time a timer fires.

## Material levels

| Level | What is inside |
|---|---|
| [1. Basic theory](basics.en.md) | single thread, queues, timers, blocking the loop, `setImmediate` and `nextTick` |
| [2. Advanced theory](advanced.en.md) | Node.js loop phases, queue order, worker threads, messages, shared memory, measuring delays |
| [3. Expert theory](expert.en.md) | a reference, internals, pitfalls, interview questions |
| [4. Self-check](quiz.en.md) | quiz and [exercises](exercises.en.md) |
| 5. Runnable examples | not created yet — `/learn javascript 13-event-loop-and-concurrency level 5` |
| 6. Tests | not created yet — `/learn javascript 13-event-loop-and-concurrency level 6` |

## Related materials

- Prerequisites: [12 · Promises and async/await](../12-promises-and-async-await/overview.en.md)
- Next: [14 · Errors and debugging](../14-errors-and-debugging/overview.en.md)
- Analogues in other tracks: [C#: concurrency](../../../csharp/modules/18-concurrency/overview.en.md), [Python](../../../python/overview.en.md) (GIL, asyncio), [Linux: processes](../../../linux/overview.en.md)
- [State updates and timers in React](../../../react/modules/03-state-and-events/overview.en.md)
- [Rendering performance in React](../../../react/modules/11-performance/overview.en.md)
