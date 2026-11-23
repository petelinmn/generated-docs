---
title: "Promises and async/await: expert"
lang: en
track: javascript
kind: module
doc: expert
slug: 12-promises-and-async-await
part: 2
tags: [promise, microtask, job-queue, nexttick, thenable, async-stack-trace, unhandled-rejection, aggregate-error]
versions: "ECMAScript 2025+, Node.js 24.15 LTS"
status: draft
updated: 2026-11-23
---

# Promises and async/await: expert

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · **3. Expert** · [4. Quiz](quiz.en.md)

Builds on [basic](basics.en.md) and [advanced](advanced.en.md) theory. All results were obtained by running on Node.js 24.15.

## Reference

### The constructor and static methods

| Member | What it does |
|---|---|
| `new Promise(executor)` | `executor(resolve, reject)` is called synchronously; an exception in it is a rejection |
| `Promise.resolve(x)` | for a promise returns **the same** object, otherwise creates a fulfilled one (in the experiment `Promise.resolve(p) === p` is `true`) |
| `Promise.reject(e)` | a rejected promise |
| `Promise.all`, `allSettled`, `race`, `any` | combinators ([advanced theory](advanced.en.md#combinators)) |
| `Promise.withResolvers()` | `{ promise, resolve, reject }` |
| `Promise.try(fn)` | runs `fn`, turns a synchronous exception into a rejection |

### Instance methods

| Method | Notes |
|---|---|
| `then(onFulfilled, onRejected)` | returns a new promise; non-functions pass the value on (in the experiment `then(null).then(undefined)` passed `'pass'`) |
| `catch(onRejected)` | the same as `then(undefined, onRejected)` |
| `finally(fn)` | `fn` takes no arguments; the returned value is ignored, an exception in `fn` rejects the result (in the experiment `in finally`) |

### What an `async` function returns

Always a promise; `return x` fulfills with `x` (if `x` is a promise or thenable — "flattened"), `throw e` rejects. `Object.prototype.toString.call(f)` is `[object AsyncFunction]`.

### Typical Node.js outcomes related to promises

| What | What happens |
|---|---|
| Resolving with the promise itself | `TypeError: Chaining cycle detected for promise #<Promise>` (in the experiment) |
| `Promise.any([])` or all rejected | `AggregateError`: `errors` is an array of reasons, `message` is `All promises were rejected` |
| An unhandled rejection | the error is printed and the exit code is `1` unless there is an `unhandledRejection` handler |
| A pending top-level `await` | a warning and exit code `13` (in the experiment with `AbortSignal.timeout`) |

## Internals

- **The microtask queue.** The reactions of `then`/`catch`/`finally` and the continuations after `await` are put into the microtask queue. It is run to completion after the current synchronous part and **before** the next macrotask (a timer, I/O). That is why all `then`s fire before `setTimeout(…, 0)`.
- **Order in the experiment (CommonJS):** `1 sync` → `2 nextTick` → `3 micro` (promise) → `4 queueMicrotask` → `5 timeout`. In an **ES module** `process.nextTick` fires **after** the promise queue (in the experiment `2 nextTick` was printed after all `then`s, before `5 timeout`), because the module itself runs inside a microtask. Do not rely on the order of `nextTick` and promises.
- **Flattening.** If `then` returned a promise or thenable, the result is connected to it by a separate reaction: additional queue ticks are spent on this. So a chain that returns a promise from `then` may finish later than expected.
- **`await`.** `await x` converts `x` to a promise and subscribes the function's continuation to its result; the function is suspended and control returns to the caller.
- **Thenable.** For an object with a `then` method the engine calls `then` as an asynchronous task; in the experiment the object `{ then(res) { n++; res('t') } }` after `await Promise.resolve(th); await th;` got `then calls 2`: every `await`/`resolve` calls `then` again, the result is not remembered.
- **Async stack traces.** V8 restores the chain of `await`s: an error from `inner` called in `outer` shows `at inner (…)` and `at async outer (…)` (in the experiment). `async` frames are a sign of a restored trace.
- **Unhandled rejections.** After the microtask queue is drained, Node.js checks which rejected promises have no handler and fires `unhandledRejection` (in the experiment for `Promise.reject(...)` and for `late`); if a handler is added later, `rejectionHandled` is emitted (in the experiment for `late`).

> ⚠️ Verify: the exact number of queue ticks when flattening a promise in `then` and in an `async` function was not measured across engines; the `--unhandled-rejections` modes (`warn`, `strict`, `none`, etc.) were not run.

- **Resources and the process.** A promise does not keep the process alive by itself: timers and open handles do. The `AbortSignal.timeout` timer does not (in the experiment the process exited with a warning).

## Gotchas

### 1. `forEach(async …)` does not wait

`[1, 2, 3].forEach(async x => …)` returns control at once: `out.length` is `0`.

### 2. `return promise` in `try/catch`

The error is not caught: `escaped E`, while with `return await` — `caught`.

### 3. `nextTick` and promises

In CommonJS `nextTick` comes before promises, in an ES module — after.

### 4. The executor runs synchronously

The code inside `new Promise(…)` runs at once; only `then` handlers are asynchronous.

### 5. `Promise.resolve` does not always create a new object

`Promise.resolve(p) === p` is `true`, while `new Promise(r => r(p)) === p` is `false`.

### 6. `finally` can change the outcome

An exception in `finally` rejects the result (`in finally`); the returned value is ignored (`finally keeps keep`); a rejection is preserved (`finally rethrows e1`).

### 7. `then(null)` passes the value on

`Promise.resolve('pass').then(null).then(undefined)` delivers `'pass'`.

### 8. `race` and `all` cancel nothing

The losing operations keep running; to stop them you need an `AbortSignal`.

### 9. The `AbortSignal.timeout` timer does not keep the process alive

If nothing else is waiting, Node.js exits with an `unsettled top-level await` warning.

### 10. A thenable is called repeatedly

Every conversion of an object with `then` calls the method again (`then calls 2`).

## Trade-offs and alternatives

| Approach | Pros | Cons |
|---|---|---|
| Promises and chains | composition, combinators | clumsy with branching |
| `async`/`await` | linear code, `try/catch` | easy to lose parallelism and `return await` |
| Callbacks | simple, no overhead | "callback hell", manual error handling |
| Async iterators | a stream of values | sequential |
| Libraries (Bluebird, p-limit, p-retry) | a pool, retry, cancellation | a dependency; not run |
| Events (`EventEmitter`) | many values over time | no single result, errors via an event |
| Streams | backpressure | a more complex API; not run |

## Interview questions

<details><summary>1. What is a promise and what states does it have?</summary>

An object representing a future result; the states are pending, fulfilled, rejected; the transition happens once.

→ [Basic theory](basics.en.md#promise-states)

</details>

<details><summary>2. Does the executor function run synchronously or asynchronously?</summary>

Synchronously; only `then` handlers are asynchronous.

→ [Gotcha 4](#4-the-executor-runs-synchronously)

</details>

<details><summary>3. How does a <code>then</code> chain work?</summary>

Every `then` returns a new promise; the handler's value or exception determines its state; an error goes to the nearest `catch`.

→ [Basic theory](basics.en.md#then-catch-finally)

</details>

<details><summary>4. Trick: how does <code>return promise</code> differ from <code>return await promise</code> in <code>try/catch</code>?</summary>

The first does not catch the promise's error, the second does.

→ [Gotcha 2](#2-return-promise-in-trycatch)

</details>

<details><summary>5. In what order do <code>setTimeout 0</code>, <code>then</code>, <code>queueMicrotask</code>, <code>nextTick</code> run?</summary>

Synchronous code first, then `nextTick` (in CommonJS) and microtasks (`then`, `queueMicrotask`), then the timer. In an ES module `nextTick` comes after promises.

→ [Internals](#internals)

</details>

<details><summary>6. How does <code>Promise.all</code> differ from <code>allSettled</code>, <code>race</code> and <code>any</code>?</summary>

`all` — all succeed or the first error; `allSettled` — the results of all; `race` — the first to finish; `any` — the first success.

→ [Advanced theory](advanced.en.md#combinators)

</details>

<details><summary>7. Trick: why does <code>forEach(async …)</code> not work as expected?</summary>

`forEach` ignores the returned promises and does not wait for them.

→ [Gotcha 1](#1-foreachasync--does-not-wait)

</details>

<details><summary>8. How do you limit the number of simultaneous operations?</summary>

A pool of `n` "workers", each takes the next item; `await Promise.all(workers)`.

→ [Advanced theory](advanced.en.md#limiting-concurrency)

</details>

<details><summary>9. How do you cancel an asynchronous operation?</summary>

`AbortController`/`AbortSignal`: the operation must accept the signal; `Promise.race` stops only the waiting.

→ [Advanced theory](advanced.en.md#timeout-and-cancellation)

</details>

<details><summary>10. What happens to an unhandled rejection in Node.js?</summary>

By default the process terminates with code `1`; you can intercept it with `process.on('unhandledRejection')`.

→ [Advanced theory](advanced.en.md#unhandled-rejections)

</details>

<details><summary>11. Trick: does <code>Promise.resolve(p)</code> return a new promise if <code>p</code> is a promise?</summary>

No, the same object; `new Promise(r => r(p))` creates a new one.

→ [Gotcha 5](#5-promiseresolve-does-not-always-create-a-new-object)

</details>

<details><summary>12. How does <code>finally</code> affect the chain's result?</summary>

It does not change the value or the reason; but an exception inside `finally` rejects the result.

→ [Gotcha 6](#6-finally-can-change-the-outcome)

</details>

## Cheat sheet

```text
new Promise((res, rej) => …)   // the executor is synchronous; res/rej — once
p.then(f).catch(g).finally(h)  // then returns a new promise
async function f() { try { return await g() } catch (e) {…} }   // return await — inside try
await Promise.all([…])  // parallel; allSettled / race / any
Promise.withResolvers(); Promise.try(fn)
AbortSignal.timeout(ms); controller.abort(); st(ms, v, { signal })
process.on('unhandledRejection', (reason, promise) => …)
```

- `then` handlers are microtasks: after the synchronous code, before timers.
- `forEach(async …)` does not wait.
- `race` and `all` cancel nothing.

## Related materials

- Previous level: [advanced theory](advanced.en.md)
- Iterators and generators: [11 · Iterators and generators](../11-iterators-and-generators/expert.en.md)
- Functions: [04 · Functions](../04-functions/expert.en.md)
- The event loop: module 13 of the roadmap ([track](../../overview.en.md))
- The C# analogue: [10 · async/await](../../../csharp/modules/10-async-await/expert.en.md)

## Sources

- [ECMAScript: Promise Objects](https://tc39.es/ecma262/#sec-promise-objects)
- [ECMAScript: Async Function Objects](https://tc39.es/ecma262/#sec-async-function-objects)
- [MDN: Promise](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Promise)
- [MDN: Microtask guide](https://developer.mozilla.org/en-US/docs/Web/API/HTML_DOM_API/Microtask_guide)
- [Node.js: process.nextTick](https://nodejs.org/docs/latest-v24.x/api/process.html#processnexttickcallback-args)
- [V8: Fast async](https://v8.dev/blog/fast-async)
