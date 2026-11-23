---
title: "Exercises: the event loop and concurrency"
lang: en
track: javascript
kind: module
doc: exercises
slug: 13-event-loop-and-concurrency
part: 2
tags: [event-loop, set-immediate, worker-threads, atomics, timers]
status: draft
updated: 2026-11-23
---

# Exercises: the event loop and concurrency

🌐 [Русский](exercises.ru.md) · **English**

> Solve on your own; hints and solutions are under spoilers. Run the code in a file next to a `package.json` (`{"type": "module"}`) unless stated otherwise.

## 1. 🟢 Output order

**Task:** predict the output order in a CommonJS (`.cjs`) file, then check by running.

```js
console.log('A');
process.nextTick(() => console.log('B'));
Promise.resolve().then(() => console.log('C'));
queueMicrotask(() => console.log('D'));
setTimeout(() => console.log('E'), 0);
console.log('F');
```

**Expected result:** `A`, `F`, `B`, `C`, `D`, `E`.

<details><summary>Hint</summary>

Synchronous code first, then the `nextTick` queue, then microtasks in order, then the timer.

</details>

<details><summary>Solution</summary>

```text
A
F
B
C
D
E
```

In an ES module `B` would come after `C` and `D`.

</details>

## 2. 🟢 A timer that does not keep the process alive

**Task:** create a `setInterval` that must not prevent the process from exiting, and make sure the program ends, having printed only `end`.

**Expected result:** `end`.

<details><summary>Hint</summary>

`setInterval(…).unref()`.

</details>

<details><summary>Solution</summary>

```js
setInterval(() => console.log('never printed'), 1000).unref();
console.log('end');
```

```text
end
```

</details>

## 3. 🟡 A sum in chunks

**Task:** write `sumChunked(n, chunk)` that sums the numbers from `0` to `n - 1` in chunks, giving control back to the loop via `setImmediate`. Check that a parallel `setInterval(…, 1)` timer managed to fire during the computation.

**Expected result:** `12499997500000 true` for `n = 5e6`.

<details><summary>Hint</summary>

Inside a promise — a function `step` that processes `chunk` numbers and calls `setImmediate(step)` until it reaches `n`.

</details>

<details><summary>Solution</summary>

```js
const sumChunked = (n, chunk = 1e5) => new Promise(res => { let i = 0, s = 0; (function step() { const end = Math.min(i + chunk, n); for (; i < end; i++) s += i; if (i < n) setImmediate(step); else res(s); })(); });
let ticks = 0; const iv = setInterval(() => ticks++, 1);
const total = await sumChunked(5e6); clearInterval(iv);
console.log(total, ticks > 0);
```

```text
12499997500000 true
```

The value of `ticks` depends on the machine, so only the fact of firing is checked.

</details>

## 4. 🟡 A computation in a worker thread

**Task:** write `runWorker(n)` that starts a worker thread computing the sum `0 … n - 1` and returns a promise with the result. Run three computations in parallel.

**Expected result:** `499999500000` for `n = 1e6`, and `45,4950` for `10` and `100`.

<details><summary>Hint</summary>

`new Worker(code, { eval: true, workerData: n })`; the result goes through `parentPort.postMessage`; in the parent `w.once('message', res)` and `w.once('error', rej)`.

</details>

<details><summary>Solution</summary>

```js
import { Worker } from 'node:worker_threads';
const runWorker = (n) => new Promise((res, rej) => {
  const w = new Worker(`import { parentPort, workerData } from 'node:worker_threads'; let s = 0; for (let i = 0; i < workerData; i++) s += i; parentPort.postMessage(s);`, { eval: true, workerData: n });
  w.once('message', res); w.once('error', rej);
});
console.log(await runWorker(1e6), (await Promise.all([runWorker(10), runWorker(100)])).join(','));
```

```text
499999500000 45,4950
```

</details>

## 5. 🔴 A shared counter with `Atomics`

**Task:** three worker threads increment one number in a `SharedArrayBuffer` 500 times each. Get a total without losses.

**Expected result:** `1500`.

<details><summary>Hint</summary>

`Atomics.add(a, 0, 1)` inside the thread; the shared buffer goes through `workerData`; the total is `Atomics.load`.

</details>

<details><summary>Solution</summary>

```js
import { Worker } from 'node:worker_threads';
const sab = new SharedArrayBuffer(4); const ia = new Int32Array(sab);
const code = `import { workerData } from 'node:worker_threads'; const a = new Int32Array(workerData); for (let i = 0; i < 500; i++) Atomics.add(a, 0, 1);`;
await Promise.all([1, 2, 3].map(() => new Promise(r => new Worker(code, { eval: true, workerData: sab }).on('exit', r))));
console.log(Atomics.load(ia, 0));
```

```text
1500
```

</details>
