---
title: "Promises and async/await"
lang: en
track: javascript
kind: module
doc: overview
slug: 12-promises-and-async-await
part: 2
levels: [1, 2, 3, 4]
tags: [promise, async-await, promise-all, promise-allsettled, promise-any, microtask, unhandled-rejection, abort-signal]
versions: "ECMAScript 2025+, Node.js 24.15 LTS"
status: draft
updated: 2026-11-26
---

# Promises and async/await

🌐 [Русский](overview.ru.md) · **English**

## In short

A promise is an object representing the result of an asynchronous operation that will appear later: success (fulfilled), an error (rejected) or waiting (pending). Promises are assembled into chains with `then`/`catch`/`finally`, and `async`/`await` lets you write the same thing as linear code with an ordinary `try/catch`. The combinators `Promise.all`, `allSettled`, `race`, `any` join several operations.

## Key ideas

- **A promise changes state once:** pending → fulfilled or rejected; repeated `resolve`/`reject` are ignored.
- **`then` handlers are always asynchronous:** they run as microtasks after the current synchronous code.
- **`then` returns a new promise,** so chains are built from promises; an error travels down the chain to the nearest `catch`.
- **An `async` function always returns a promise;** `await` pauses it until the result.
- **`return promise` and `return await promise` differ** inside `try/catch`.
- **`Promise.all` is parallel and "all or nothing",** `allSettled` — the results of all, `race` — the first one, `any` — the first success.
- **`await` in a loop is sequential;** `forEach(async …)` does not wait.
- **An unhandled rejection terminates** a Node.js process: every promise needs an error handler.
- **Cancellation and a timeout** are done with `AbortSignal` and `Promise.race`.

## Minimal example

```js
const sleep = (ms, v) => new Promise(r => setTimeout(r, ms, v));
async function main() {
  const [a, b] = await Promise.all([sleep(5, 'a'), sleep(5, 'b')]);
  console.log(a, b);
}
main();
```

```text
a b
```

Two operations run simultaneously, and `await` waits for both.

## When to use and when not

| Task | Choice |
|---|---|
| Sequential dependent steps | `await` one after another |
| Independent operations | `Promise.all` |
| You need the results of all, even failed | `Promise.allSettled` |
| The first successful of several sources | `Promise.any` |
| A timeout, cancellation | `Promise.race` + `AbortSignal` |
| A stream of values over time | async iterators ([module 11](../11-iterators-and-generators/overview.en.md)) |

- ❌ Do not use `forEach` with an `async` function if you need to wait for it.
- ❌ Do not leave promises without an error handler.

## Material levels

| Level | What is inside |
|---|---|
| [1. Basic theory](basics.en.md) | states, `then`/`catch`/`finally`, chains, `async`/`await`, errors |
| [2. Advanced theory](advanced.en.md) | combinators, parallel and sequential, timeout and cancellation, a pool, retry, unhandled rejections |
| [3. Expert theory](expert.en.md) | a reference, microtask order, internals, pitfalls, interview questions |
| [4. Self-check](quiz.en.md) | quiz and [exercises](exercises.en.md) |
| 5. Runnable examples | not created yet — `/learn javascript 12-promises-and-async-await level 5` |
| 6. Tests | not created yet — `/learn javascript 12-promises-and-async-await level 6` |

## Related materials

- Prerequisites: [04 · Functions](../04-functions/overview.en.md), [11 · Iterators and generators](../11-iterators-and-generators/overview.en.md)
- Next: [13 · The event loop and concurrency](../13-event-loop-and-concurrency/overview.en.md)
- Analogues in other tracks: [C#: async/await](../../../csharp/modules/10-async-await/overview.en.md), [Python](../../../python/overview.en.md) (asyncio)
- [Effects and cancellation in React](../../../react/modules/06-effects/overview.en.md)
- [Suspense and transitions in React](../../../react/modules/12-concurrent-features/overview.en.md)
