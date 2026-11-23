---
title: "The event loop and concurrency: expert"
lang: en
track: javascript
kind: module
doc: expert
slug: 13-event-loop-and-concurrency
part: 2
tags: [event-loop, libuv, microtask-starvation, timers-clamping, atomics, worker-threads, structured-clone, performance]
versions: "ECMAScript 2025+, Node.js 24.15 LTS"
status: draft
updated: 2026-11-23
---

# The event loop and concurrency: expert

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · **3. Expert** · [4. Quiz](quiz.en.md)

Builds on [basic](basics.en.md) and [advanced](advanced.en.md) theory. All results were obtained by running on Node.js 24.15.

## Reference

### Queues and APIs

| Queue | API | When it runs |
|---|---|---|
| `nextTick` | `process.nextTick(fn, ...args)` | after the current operation, before microtasks (CommonJS) |
| Microtasks | `Promise.then`, `queueMicrotask(fn)`, `await` | after the `nextTick` queue and after every callback |
| Timers | `setTimeout`, `setInterval` | the timers phase |
| I/O | `fs`, network callbacks | the poll phase |
| Check | `setImmediate(fn, ...args)` | after poll |
| Close | `close` callbacks | the close phase |

### Timers and limits

| Behavior | Result in the experiment |
|---|---|
| Delay `0` and `1` | equivalent: `setTimeout(…, 1)` and `setTimeout(…, 0)` fire in creation order (`[ 't1', 't0' ]`) |
| A delay above `2**31 - 1` | `TimeoutOverflowWarning`, the delay is replaced by `1` |
| The timer object | a `Timeout` with `ref()`, `unref()`, `hasRef()`, `refresh()`; `setImmediate` returns an `Immediate` |
| Cancellation | `clearTimeout`, `clearInterval`, `clearImmediate` |
| An exception in a callback | an uncaught process exception: caught by `process.on('uncaughtException')` |

### Atomics (in the experiment)

| Operation | Result |
|---|---|
| `Atomics.add(ia, i, v)` / `sub` / `and` / `or` / `xor` | return the **old** value (`add` → `0`, then `load` → `5`) |
| `Atomics.compareExchange(ia, i, expected, replacement)` | writes if the value equals `expected`; returns the old one |
| `Atomics.exchange`, `load`, `store` | exchange, read, write |
| `Atomics.wait(ia, i, expected, timeout)` | `'timed-out'` on timeout, `'not-equal'` if the value differs |
| `Atomics.notify(ia, i, count)` | the number of woken threads (`0` if nobody was waiting) |
| `Atomics.isLockFree(4)` | `true` |

`Atomics.wait` blocks the thread: in the Node.js main thread it is allowed, but it stops the loop.

## Internals

- **libuv.** The Node.js event loop is implemented in the libuv library; V8 runs JavaScript and the microtask queue. File operations and part of the work (DNS, crypto, zlib) libuv performs in a **thread pool** (4 by default); network I/O goes through OS event-waiting mechanisms.
- **One stack.** All JavaScript callbacks run on one thread: a worker thread is a separate V8 instance with its own loop and heap, so objects are not shared between threads, only copied or transferred.
- **Microtask starvation.** The microtask queue runs until it is empty: self-reproducing microtasks keep a timer from firing. In the experiment a `0` timer fired only after 200001 microtasks (`timer after starvation, microtasks: 200001`).
- **Transfer.** `postMessage(value, [transfer])` and `structuredClone(value, { transfer })` "detach" a buffer: the sender's `byteLength` becomes `0`, the `detached` property is `true` (in the experiment `0 4 true`).
- **Errors outside chains.** An exception in `queueMicrotask`/a timer does not land in any `try/catch`: it becomes an uncaught process exception (in the experiment `uncaught: in microtask`, `uncaught: in timer` with an `uncaughtException` handler; without it the process terminates).
- **Measurements.** `performance.now()` is monotonic; `Date.now()` can jump when the system time changes.

> ⚠️ Verify: the default size of the libuv thread pool (4), the list of operations that run in it and the variable `UV_THREADPOOL_SIZE` come from the documentation and were not run.

> ⚠️ Verify: the behavior of `Atomics.wait` in browsers (forbidden in the main thread) and the behavior of threads under `resourceLimits` were not checked.

## Gotchas

### 1. `setTimeout(…, 0)` will not fire before microtasks

Any `then` or `queueMicrotask` runs earlier; in the experiment the timer waited for 200001 microtasks.

### 2. The order of `setTimeout` and `setImmediate`

In the main module it is not guaranteed; inside I/O `setImmediate` is first (in the experiment `io: immediate`, then `io: timeout`).

### 3. `nextTick` and promises

In CommonJS `nextTick` is before promises (`A F B C D E`), in an ES module after ([module 12](../12-promises-and-async-await/expert.en.md#internals)).

### 4. An exception in a timer or microtask

It does not land in a `try/catch` around `setTimeout`: the process gets `uncaughtException`.

### 5. A timer keeps the process alive

A live `setInterval` keeps the process from exiting; `unref()` removes that (in the experiment the process exited after `end`).

### 6. A shared counter without `Atomics`

Updates are lost; use `Atomics.add` (in the experiment the total is `1500`).

### 7. A transfer zeroes the buffer

After `transfer` the sender's `byteLength` is `0`.

### 8. Delays are not exact

`ms` is a minimum; a timer set for 10 ms fired later after an 80 ms block (in the experiment `timer fired late: true`).

### 9. Threads are not for I/O

I/O is already asynchronous; a thread costs more and will not speed up waiting for the network.

## Trade-offs and alternatives

| Approach | Pros | Cons |
|---|---|---|
| Chunks with `setImmediate` | simple, one thread | splitting the code, longer total time |
| Worker threads | real CPU parallelism | data copying, starting is expensive |
| Child processes | isolation, any language | even more expensive, exchange through channels |
| `SharedArrayBuffer` + `Atomics` | no copying | complex, easy to get wrong |
| Cluster (`cluster`) | many processes on one port | separate memory; not run |
| An external task queue | scaling across machines | infrastructure; not run |

## Interview questions

<details><summary>1. Why is JavaScript single-threaded while Node.js handles many requests?</summary>

I/O is asynchronous: operations run in the background (the OS, the libuv thread pool), and JavaScript callbacks run on one thread in turn.

→ [Basic theory](basics.en.md#one-thread)

</details>

<details><summary>2. How do macrotasks differ from microtasks?</summary>

Microtasks (`then`, `queueMicrotask`) run to completion after every macrotask (a timer, I/O); macrotasks go one per iteration.

→ [Basic theory](basics.en.md#macrotasks-and-microtasks)

</details>

<details><summary>3. Trick: in what order do <code>setTimeout 0</code> and <code>setImmediate</code> fire?</summary>

In the main module it is not guaranteed; inside an I/O callback `setImmediate` is first.

→ [Gotcha 2](#2-the-order-of-settimeout-and-setimmediate)

</details>

<details><summary>4. What is <code>process.nextTick</code> and why is a recursive one dangerous?</summary>

A separate Node.js queue, run before promise microtasks; recursion lets neither I/O nor timers in.

→ [Basic theory](basics.en.md#setimmediate-and-processnexttick)

</details>

<details><summary>5. How do you avoid blocking the loop with a heavy computation?</summary>

Chunks with `setImmediate` or a worker thread.

→ [Basic theory](basics.en.md#blocking-the-loop)

</details>

<details><summary>6. Trick: does <code>setTimeout(fn, 100)</code> guarantee a call exactly after 100 ms?</summary>

No, it is a minimum delay; blocking code or loop load shifts the call.

→ [Gotcha 8](#8-delays-are-not-exact)

</details>

<details><summary>7. How does a worker thread differ from a process?</summary>

A thread lives in one process, with a separate V8 and loop, exchanging messages or shared memory; a process is fully isolated, exchanging through channels.

→ [Advanced theory](advanced.en.md#worker-threads)

</details>

<details><summary>8. How do you pass data between threads without copying?</summary>

A transfer (`postMessage(v, [buf])`) or a `SharedArrayBuffer`.

→ [Advanced theory](advanced.en.md#messages-copying-and-transfer)

</details>

<details><summary>9. What is <code>Atomics</code> for?</summary>

Atomic operations on shared memory: without them parallel `a[0]++` lose updates.

→ [Advanced theory](advanced.en.md#sharedarraybuffer-and-atomics)

</details>

<details><summary>10. Trick: an exception inside <code>setTimeout</code> — will a surrounding <code>try/catch</code> catch it?</summary>

No: the callback runs later, on a different stack. The exception becomes `uncaughtException`.

→ [Gotcha 4](#4-an-exception-in-a-timer-or-microtask)

</details>

<details><summary>11. What is microtask starvation?</summary>

Self-reproducing microtasks keep timers and I/O from running.

→ [Internals](#internals)

</details>

<details><summary>12. How do you measure loop blocking?</summary>

`monitorEventLoopDelay()` from `perf_hooks`, or measuring `performance.now()` around a timer.

→ [Advanced theory](advanced.en.md#measuring-time-and-loop-delay)

</details>

## Cheat sheet

```text
sync → nextTick (CJS) → microtasks → timers → poll (I/O) → setImmediate → close
setTimeout(fn, ms, ...args); t.unref(); clearTimeout(t)
setImmediate(fn) — after I/O; inside I/O before setTimeout 0
new Worker(code, { eval: true, workerData }); parentPort.postMessage / on('message')
postMessage(v, [buf]) — transfer; SharedArrayBuffer + Atomics.add/load
performance.now(); monitorEventLoopDelay()
```

- Microtasks run to completion before the next macrotask.
- Threads are for the CPU, not for I/O.
- Shared memory only through `Atomics`.

## Related materials

- Previous level: [advanced theory](advanced.en.md)
- Promises: [12 · Promises and async/await](../12-promises-and-async-await/expert.en.md)
- Concurrency in C#: [18 · Concurrency](../../../csharp/modules/18-concurrency/expert.en.md)
- Processes and threads in Linux: [Linux](../../../linux/overview.en.md)

## Sources

- [Node.js: Event Loop, Timers, and process.nextTick()](https://nodejs.org/en/learn/asynchronous-work/event-loop-timers-and-nexttick)
- [Node.js: Worker threads](https://nodejs.org/docs/latest-v24.x/api/worker_threads.html)
- [Node.js: Timers](https://nodejs.org/docs/latest-v24.x/api/timers.html)
- [HTML: Event loops](https://html.spec.whatwg.org/multipage/webappapis.html#event-loops)
- [MDN: Atomics](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Atomics)
- [libuv: Design overview](https://docs.libuv.org/en/v1.x/design.html)
