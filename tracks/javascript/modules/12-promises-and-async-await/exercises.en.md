---
title: "Exercises: promises and async/await"
lang: en
track: javascript
kind: module
doc: exercises
slug: 12-promises-and-async-await
part: 2
tags: [promise, async-await, promisify, promise-allsettled, abort-signal, concurrency-limit, retry]
status: draft
updated: 2026-11-23
---

# Exercises: promises and async/await

🌐 [Русский](exercises.ru.md) · **English**

> Solve on your own; hints and solutions are under spoilers. Run the code in a file next to a `package.json` (`{"type": "module"}`); all exercises use `const sleep = (ms, v) => new Promise(r => setTimeout(r, ms, v));`.

## 1. 🟢 Output order

**Task:** predict the output order, then check by running.

```js
console.log('A');
setTimeout(() => console.log('D'), 0);
Promise.resolve().then(() => console.log('C'));
(async () => { console.log('B'); await null; console.log('C2'); })();
console.log('A2');
```

**Expected result:** `A`, `B`, `A2`, `C`, `C2`, `D`.

<details><summary>Hint</summary>

The body of an `async` function runs synchronously up to the first `await`; everything after `await` is a microtask, and the timer comes after the microtasks.

</details>

<details><summary>Solution</summary>

```text
A
B
A2
C
C2
D
```

`A`, `B` and `A2` are synchronous code (`B` is printed inside the `async` function before `await`). Then the microtask queue: `C` (queued earlier), `C2` (the continuation after `await null`). The timer fires last.

</details>

## 2. 🟢 A wrapper over a callback

**Task:** write `promisify(fn)` that turns a function in the style `fn(...args, cb)` (callback `(err, value)`) into a function returning a promise. Check success and error.

**Expected result:** `data`, `cb failed`.

<details><summary>Hint</summary>

`new Promise((res, rej) => fn(...a, (err, v) => err ? rej(err) : res(v)))`.

</details>

<details><summary>Solution</summary>

```js
function readLater(cb) { setTimeout(() => cb(null, 'data'), 1); }
function failLater(cb) { setTimeout(() => cb(new Error('cb failed')), 1); }
const promisify = fn => (...a) => new Promise((res, rej) => fn(...a, (err, v) => err ? rej(err) : res(v)));
console.log(await promisify(readLater)());
try { await promisify(failLater)() } catch (e) { console.log(e.message) }
```

```text
data
cb failed
```

Node.js has a ready-made `util.promisify` for this.

</details>

## 3. 🟡 A summary from `allSettled`

**Task:** start three operations, one of which is rejected. Print an array of the successful values and an array of error messages.

**Expected result:** `[ 'a', 'c' ] [ 'bad' ]`.

<details><summary>Hint</summary>

`Promise.allSettled` and `filter` by `status`.

</details>

<details><summary>Solution</summary>

```js
const jobs = [sleep(2, 'a'), Promise.reject(new Error('bad')), sleep(1, 'c')];
const res = await Promise.allSettled(jobs);
console.log(res.filter(r => r.status === 'fulfilled').map(r => r.value), res.filter(r => r.status === 'rejected').map(r => r.reason.message));
```

```text
[ 'a', 'c' ] [ 'bad' ]
```

</details>

## 4. 🟡 A timeout with cancellation

**Task:** write `withTimeout(fn, ms)` that calls `fn(signal)` and cancels the operation through an `AbortController` after `ms`. The operation `work(ms)(signal)` listens to the signal. Check a fast and a slow operation.

**Expected result:** `done`, `timeout`.

<details><summary>Hint</summary>

`ac.abort(new Error('timeout'))`; in the operation — `signal.addEventListener('abort', …)`; in the end `clearTimeout`.

</details>

<details><summary>Solution</summary>

```js
function withTimeout(fn, ms) { const ac = new AbortController(); const t = setTimeout(() => ac.abort(new Error('timeout')), ms); return fn(ac.signal).finally(() => clearTimeout(t)); }
const work = (ms) => (signal) => new Promise((res, rej) => { const t = setTimeout(res, ms, 'done'); signal.addEventListener('abort', () => { clearTimeout(t); rej(signal.reason); }); });
console.log(await withTimeout(work(5), 50));
try { await withTimeout(work(50), 5) } catch (e) { console.log(e.message) }
```

```text
done
timeout
```

</details>

## 5. 🔴 A pool with retry

**Task:** write `pool(items, n, fn)` (no more than `n` simultaneous operations, the order of results preserved) and `retry(fn, times)`. Process `[1, 2, 3, 4]`: even numbers must fail on the first attempt and pass on the second. Show the number of attempts.

**Expected result:** `[ 10, 20, 30, 40 ] { '1': 1, '2': 2, '3': 1, '4': 2 }`.

<details><summary>Hint</summary>

A pool is `n` "workers" with a shared index; in `retry` — `return await` inside `try`.

</details>

<details><summary>Solution</summary>

```js
async function pool(items, n, fn) { const out = []; let i = 0; await Promise.all(Array.from({ length: n }, async () => { while (i < items.length) { const k = i++; out[k] = await fn(items[k]); } })); return out; }
const attempts = {};
const flaky = async x => { attempts[x] = (attempts[x] ?? 0) + 1; if (x % 2 === 0 && attempts[x] < 2) throw new Error('retry ' + x); return x * 10; };
async function retry(fn, times) { for (let i = 0; ; i++) { try { return await fn() } catch (e) { if (i >= times) throw e } } }
console.log(await pool([1, 2, 3, 4], 2, x => retry(() => flaky(x), 2)), attempts);
```

```text
[ 10, 20, 30, 40 ] { '1': 1, '2': 2, '3': 1, '4': 2 }
```

</details>
