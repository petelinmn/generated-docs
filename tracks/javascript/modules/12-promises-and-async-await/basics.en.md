---
title: "Promises and async/await: basics"
lang: en
track: javascript
kind: module
doc: basics
slug: 12-promises-and-async-await
part: 2
tags: [promise, then, catch, finally, async-await, error-handling, thenable]
versions: "ECMAScript 2025+, Node.js 24.15 LTS"
status: draft
updated: 2026-11-23
---

# Promises and async/await: basics

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [04 · Functions](../04-functions/overview.en.md), [11 · Iterators and generators](../11-iterators-and-generators/overview.en.md) · **Time:** ~3 h

All examples were run on Node.js 24.15; the files sit next to a `package.json` (`{"type": "module"}`). The examples use a helper:

```js
const sleep = (ms, v) => new Promise(r => setTimeout(r, ms, v));
```

## Why it matters

Reading a file, a network request, a timer run "in the background", and the result arrives later. A promise is one way to describe such a result and combine several operations without nesting callbacks inside each other.

## Promise states

A promise is in one of three states: pending, fulfilled, rejected. The transition happens once:

```js
const p = new Promise((res, rej) => { console.log('executor sync'); res(1); res(2); rej(new Error('x')); });
console.log(p);
p.then(v => console.log('then', v));
console.log('after then registered');
```

```text
executor sync
Promise { 1 }
after then registered
then 1
```

- The executor function runs **synchronously**, right at creation.
- The repeated `res(2)` and `rej(...)` are ignored: the first value is `1`.
- The `then` handler is called **after** the synchronous code (`after then registered` is printed before `then 1`), even if the promise is already fulfilled.
- An exception in the executor turns into a rejection: `new Promise(() => { throw new Error('executor throws') }).catch(...)` catches it.

## then, catch, finally

```js
Promise.resolve(1)
  .then(v => v + 1)
  .then(v => { throw new Error('boom ' + v) })
  .then(() => console.log('skipped'))
  .catch(e => { console.log('caught', e.message); return 'rec' })
  .then(v => console.log('after catch', v))
  .finally(() => console.log('finally'));
```

```text
caught boom 2
after catch rec
finally
```

- `then` returns a **new** promise: the value the handler returns becomes its result.
- An exception in a handler rejects the next promise; the chain skips a `then` without an error handler (`skipped` is not printed) up to the nearest `catch`.
- After `catch` the chain continues: `catch` returned `'rec'`, and the next `then` received it.
- `finally` runs in any case and receives no value; it does not change the chain's result.

If a handler returns a promise, the chain waits for it (the result is "flattened"):

```js
Promise.resolve().then(() => Promise.resolve('flat')).then(v => console.log(v));
```

```text
flat
```

## async and await

An `async function` always returns a promise. Inside, `await` pauses the function until the promise settles and returns its value (or throws the error):

```js
async function f() { return 1 }
console.log(f() instanceof Promise, Object.prototype.toString.call(f));
async function bad() { throw new Error('async boom') }
async function useTry() { try { await bad() } catch (e) { return 'handled ' + e.message } finally { console.log('fin') } }
useTry().then(console.log);
```

```text
true [object AsyncFunction]
fin
handled async boom
```

- The `return` value is wrapped in a promise; a `throw` inside is a rejected promise.
- Errors are caught with an ordinary `try/catch/finally`.
- `await` can be applied to any value: a non-promise is returned as is, and a thenable (an object with a `then` method) is awaited (`await 5` → `5`).
- `await` is allowed only inside an `async` function or at the top level of an ES module ([module 09](../09-modules/basics.en.md#top-level-await)).

## Sequential and simultaneous

Two `await`s in a row run **one after another**; you can run operations simultaneously by starting both before the first `await`:

```js
const t = async (ms, tag) => { await sleep(ms); return tag };
let t0 = Date.now(); await t(30, 'a'); await t(30, 'b'); const seq = Date.now() - t0;
t0 = Date.now(); await Promise.all([t(30, 'a'), t(30, 'b')]); const par = Date.now() - t0;
console.log('seq>=55', seq >= 55, 'par<55', par < 55);
```

```text
seq>=55 true par<55 true
```

Sequential takes about 60 ms, simultaneous about 30 ms (the bounds in the output are loose; the exact time depends on the machine). More in the [advanced theory](advanced.en.md#sequential-and-parallel).

## Beginner mistakes

- ❌ Forgetting `await` and working with a promise as a value → ✅ `await` or `.then`.
- ❌ Leaving a promise without a `catch` → ✅ handle errors (otherwise the process terminates).
- ❌ Nested `then`s (a "pyramid") → ✅ a flat chain or `async`/`await`.
- ❌ Awaiting independent operations one by one → ✅ `Promise.all`.
- ❌ Thinking `finally` changes the result → ✅ it only performs an action.
- ❌ Expecting the code in `then` to run at once → ✅ it runs after the synchronous code.

## Related materials

- Next: [advanced theory](advanced.en.md) — combinators, a timeout, cancellation, a pool, retry
- Earlier: [11 · Iterators and generators](../11-iterators-and-generators/advanced.en.md#async-iterators-and-generators)

## Sources

- [MDN: Using promises](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Guide/Using_promises)
- [MDN: Promise](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Promise)
- [MDN: async function](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Statements/async_function)
- [MDN: await](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Operators/await)
