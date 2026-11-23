---
title: "Promises and async/await: advanced"
lang: en
track: javascript
kind: module
doc: advanced
slug: 12-promises-and-async-await
part: 2
tags: [promise-all, promise-allsettled, promise-race, promise-any, abort-signal, timeout, concurrency-limit, retry, unhandled-rejection]
versions: "ECMAScript 2025+, Node.js 24.15 LTS"
status: draft
updated: 2026-11-23
---

# Promises and async/await: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

Builds on [basic theory](basics.en.md). All examples were run on Node.js 24.15; the `sleep` helper is from the basic level.

## Combinators

| Method | Fulfilled when | Rejected when |
|---|---|---|
| `Promise.all` | all are fulfilled (an array of values) | any is rejected (the first error) |
| `Promise.allSettled` | always, after all have finished | never |
| `Promise.race` | the first one finished (success or error) | the first one finished with an error |
| `Promise.any` | the first one is fulfilled | all are rejected (`AggregateError`) |

```js
console.log(await Promise.all([1, sleep(5, 'a'), Promise.resolve('b')]));
try { await Promise.all([sleep(5, 'ok'), Promise.reject(new Error('fail')), sleep(50)]) } catch (e) { console.log('all rejected', e.message) }
const r = await Promise.allSettled([1, Promise.reject(new Error('no'))]);
console.log(r.map(x => x.status === 'fulfilled' ? x.value : x.reason.message));
console.log(await Promise.race([sleep(30, 'slow'), sleep(5, 'fast')]));
console.log(await Promise.any([Promise.reject(new Error('a')), sleep(5, 'any ok')]));
try { await Promise.any([Promise.reject(new Error('a')), Promise.reject(new Error('b'))]) } catch (e) { console.log(e.constructor.name, e.errors.map(x => x.message), e.message) }
```

```text
[ 1, 'a', 'b' ]
all rejected fail
[ 1, 'no' ]
fast
any ok
AggregateError [ 'a', 'b' ] All promises were rejected
```

- Elements may be non-promises: they turn into fulfilled ones.
- The elements of `allSettled` are objects `{ status: 'fulfilled', value }` or `{ status: 'rejected', reason }`.
- For empty input: `all([])` and `allSettled([])` give `[]`; `any([])` is rejected with an `AggregateError` with an empty `errors`.
- Losers are not cancelled: `race` left the slow promise running (in the experiment `loser still ran true`), and after `all` was rejected another operation still finished (`other finished 1`).

## Helper methods

```js
const r = Promise.withResolvers(); setTimeout(r.resolve, 1, 'wr'); console.log(await r.promise);
console.log(await Promise.try(() => { throw new Error('sync') }).catch(e => 'try ' + e.message));
```

```text
wr
try sync
```

- `Promise.withResolvers()` returns `{ promise, resolve, reject }` — handy when completion is triggered from elsewhere.
- `Promise.try(fn)` runs `fn` and wraps a synchronous exception into a rejected promise. Both methods are available in Node.js 24.

## return and return await

Inside `try/catch`, `return promise` will not catch the promise's error, while `return await promise` will:

```js
async function bad() { throw new Error('E') }
const r1 = await (async () => { try { return bad() } catch { return 'caught' } })().catch(e => 'escaped ' + e.message);
const r2 = await (async () => { try { return await bad() } catch { return 'caught' } })();
console.log(r1, r2);
```

```text
escaped E caught
```

Without `await` the error "slips" past the `catch` to the caller. Outside `try/catch` the result is the same.

## Sequential and parallel

Different ways to walk an array with an `async` function:

```js
const out = []; [1, 2, 3].forEach(async x => { await sleep(5); out.push(x) }); console.log('after forEach', out.length);
await sleep(20); console.log(out);
const out2 = []; for (const x of [1, 2, 3]) { await sleep(1); out2.push(x) } console.log(out2);
console.log(await Promise.all([1, 2, 3].map(async x => { await sleep(4 - x); return x * 2 })));
```

```text
after forEach 0
[ 1, 2, 3 ]
[ 1, 2, 3 ]
[ 2, 4, 6 ]
```

| Way | Behavior |
|---|---|
| `forEach(async …)` | does not wait: `out.length` is `0` at once; errors do not reach an outer `try/catch` |
| `for … of` + `await` | strictly one after another |
| `Promise.all(arr.map(async …))` | parallel, the order of results is kept regardless of speed |

Start the promises **before** `await` so the operations go together: in the experiment the functions `a` and `b` started one after another (`a start`, `b start`) and finished in order of speed (`b end`, `a end`).

## Timeout and cancellation

A **timeout** with `Promise.race`:

```js
const timeout = (p, ms) => Promise.race([p, new Promise((_, rej) => setTimeout(rej, ms, new Error('timeout')))]);
try { await timeout(sleep(50), 5) } catch (e) { console.log(e.message) }
```

```text
timeout
```

`race` does not stop the original operation. For real cancellation the operation must accept an **`AbortSignal`**:

```js
const { setTimeout: st } = await import('node:timers/promises');
console.log(await st(5, 'timers/promises'));
try { await st(50, 'v', { signal: AbortSignal.timeout(5) }) } catch (e) { console.log(e.name, e.code, e.cause?.name) }
```

```text
timers/promises
AbortError ABORT_ERR TimeoutError
```

- `node:timers/promises` gives promise-based `setTimeout`, `setImmediate`, `setInterval`.
- `AbortSignal.timeout(ms)` is a signal that fires after `ms`; the reason is a `TimeoutError` (`The operation was aborted due to timeout`).
- `new AbortController()` and `controller.abort()` are manual cancellation; the default reason is an `AbortError` (`This operation was aborted`), or any value passed to `abort('custom')`.
- `signal.throwIfAborted()` throws the reason if the signal has already fired; `AbortSignal.any([...])` combines signals.
- The `AbortSignal.timeout` timer does not keep the process alive: if only it is being awaited, Node.js exits (`Warning: Detected unsettled top-level await`). An operation that was given the signal behaves normally.

> ⚠️ Verify: support for `AbortSignal` in `fetch`, streams and file APIs of Node.js was not run in this material.

## Limiting concurrency

`Promise.all` over a thousand items starts a thousand operations at once. A pool limits the number of simultaneous ones:

```js
async function pool(items, n, fn) { const res = []; let i = 0; const run = async () => { while (i < items.length) { const k = i++; res[k] = await fn(items[k]); } }; await Promise.all(Array.from({ length: n }, run)); return res; }
let active = 0, max = 0;
console.log(await pool([1, 2, 3, 4, 5, 6], 2, async x => { active++; max = Math.max(max, active); await sleep(5); active--; return x * x }), 'max', max);
```

```text
[ 1, 4, 9, 16, 25, 36 ] max 2
```

No more than two ran at once; the order of results is preserved.

## Retry

```js
let tries = 0; const flaky = async () => { if (++tries < 3) throw new Error('fail' + tries); return 'ok after ' + tries };
async function retry(fn, n) { for (let i = 0; ; i++) { try { return await fn() } catch (e) { if (i >= n) throw e } } }
console.log(await retry(flaky, 5));
```

```text
ok after 3
```

`return await` is required here: otherwise the error would slip past the `catch`. Real code adds a pause between attempts (exponential), a limit and a check of the error type.

## Unhandled rejections

If a rejected promise has not received a handler by the end of the microtask queue, Node.js reports it:

- by default the process terminates with an error and exit code `1` (in the experiment `Promise.reject(new Error('unhandled default'))` printed the error, exit code `1`);
- the event `process.on('unhandledRejection', (reason, promise) => …)` lets you intercept it, and then the process does not crash;
- if a handler is added later, `rejectionHandled` fires (in the experiment for `late`).

```js
process.on('unhandledRejection', (reason, promise) => { console.log('unhandled:', reason.message, promise instanceof Promise); });
Promise.reject(new Error('nobody catches'));
```

```text
unhandled: nobody catches true
```

An `unhandledRejection` handler is a safety net for logging, not a replacement for `catch`.

## Best practices

- Every chain ends with `catch`; every `async` function either catches errors or passes them to the caller.
- Start independent operations with `Promise.all`; limit the number of simultaneous ones with a pool.
- You need all outcomes — `allSettled`; the first success — `any`.
- Give operations an `AbortSignal`, not only `Promise.race`.
- Inside `try/catch` write `return await`.
- Do not use `forEach` with `async`.

## Common mistakes

- ❌ `forEach(async …)` and expecting a result → ✅ `for … of` or `Promise.all(map(…))`.
- ❌ `return promise` inside `try/catch` → ✅ `return await promise`.
- ❌ `Promise.all` over a huge array → ✅ a pool.
- ❌ Thinking `race` stops the loser → ✅ cancellation via `AbortSignal`.
- ❌ Awaiting independent requests one by one → ✅ `Promise.all`.
- ❌ Relying on `unhandledRejection` instead of handling → ✅ `catch` in the chain.

## Related materials

- Next: [expert theory](expert.en.md)
- Async iterators: [11 · Iterators and generators](../11-iterators-and-generators/advanced.en.md#async-iterators-and-generators)
- Errors and debugging: module 14 of the roadmap ([track](../../overview.en.md))

## Sources

- [MDN: Promise — static methods](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Promise)
- [MDN: AbortSignal](https://developer.mozilla.org/en-US/docs/Web/API/AbortSignal)
- [Node.js: Timers Promises API](https://nodejs.org/docs/latest-v24.x/api/timers.html#timers-promises-api)
- [Node.js: Event 'unhandledRejection'](https://nodejs.org/docs/latest-v24.x/api/process.html#event-unhandledrejection)
- [Node.js: `--unhandled-rejections`](https://nodejs.org/docs/latest-v24.x/api/cli.html#--unhandled-rejectionsmode)
