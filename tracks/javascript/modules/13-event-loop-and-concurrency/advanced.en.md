---
title: "The event loop and concurrency: advanced"
lang: en
track: javascript
kind: module
doc: advanced
slug: 13-event-loop-and-concurrency
part: 2
tags: [event-loop-phases, poll-phase, worker-threads, message-channel, transferable, shared-array-buffer, atomics, perf-hooks]
versions: "ECMAScript 2025+, Node.js 24.15 LTS"
status: draft
updated: 2026-11-23
---

# The event loop and concurrency: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

Builds on [basic theory](basics.en.md). All examples were run on Node.js 24.15.

## The phases of the Node.js event loop

One iteration of the loop in Node.js passes through phases (per the Node.js documentation):

| Phase | What runs |
|---|---|
| timers | `setTimeout` and `setInterval` callbacks whose time has come |
| pending callbacks | deferred system callbacks |
| idle, prepare | internal needs |
| poll | I/O: getting new events, running I/O callbacks; the loop may wait here |
| check | `setImmediate` callbacks |
| close callbacks | close callbacks (for example `socket.on('close')`) |

Between the callbacks of each phase the `process.nextTick` queue and promise microtasks run. A practical consequence: **inside an I/O callback** `setImmediate` always fires before `setTimeout(…, 0)`:

```js
// CommonJS (.cjs)
const fs = require('node:fs');
fs.readFile(__filename, () => {
  setTimeout(() => console.log('io: timeout'), 0);
  setImmediate(() => console.log('io: immediate'));
  process.nextTick(() => console.log('io: nextTick'));
  Promise.resolve().then(() => console.log('io: promise'));
});
```

```text
io: nextTick
io: promise
io: immediate
io: timeout
```

After the callback come `nextTick` and promises, then the check phase (`setImmediate`), and only then the next iteration and the timers phase. But **in the main module** the order of `setTimeout(…, 0)` and `setImmediate` is not guaranteed: it depends on how busy the process is.

> ⚠️ Verify: the nondeterminism of the order of `setTimeout 0` and `setImmediate` in the main module comes from the Node.js documentation and was not run again.

## Worker threads

The module `node:worker_threads` runs JavaScript in a separate thread with its own event loop and its own V8 environment. Communication is by messages:

```js
import { Worker, isMainThread } from 'node:worker_threads';
const code = `
import { parentPort, workerData, threadId } from 'node:worker_threads';
let s = 0; for (let i = 0; i < workerData.n; i++) s += i;
parentPort.postMessage({ s, threadId });
parentPort.on('message', m => { if (m === 'ping') parentPort.postMessage('pong'); else if (m === 'bye') process.exit(0); });
`;
const w = new Worker(code, { eval: true, workerData: { n: 1e6 } });
const msgs = [];
w.on('message', m => { msgs.push(m); if (m.s !== undefined) w.postMessage('ping'); else if (m === 'pong') w.postMessage('bye'); });
await new Promise(r => w.on('exit', r));
console.log(msgs, isMainThread);
```

```text
[ { s: 499999500000, threadId: 1 }, 'pong' ] true
```

- `new Worker(file or code, { eval: true, workerData })` creates a thread; `workerData` is copied into it.
- Inside the thread `parentPort.postMessage(...)` sends a message to the parent, `parentPort.on('message', …)` receives.
- `isMainThread` is `true` in the main thread; the worker's `threadId` is `1`.
- A thread ends when it has nothing left to wait for, or on `process.exit()` inside it (the `exit` event on the parent).
- The parent receives an error from inside the thread as the `error` event (in the experiment `worker boom`):

```js
const bad = new Worker(`throw new Error('worker boom')`, { eval: true });
await new Promise(r => bad.on('error', e => { console.log('worker error:', e.message); r(); }));
```

```text
worker error: worker boom
```

It makes sense to tie the number of threads to the number of cores: `os.availableParallelism()`.

## Messages: copying and transfer

Data in `postMessage` is copied with the **structured clone** algorithm (`Date`, `Map`, `Set`, `ArrayBuffer`, circular references are supported; functions are not). Large buffers can be **transferred** without copying: the sender's buffer is "zeroed":

```js
import { MessageChannel } from 'node:worker_threads';
const ab = new ArrayBuffer(8); const { port1, port2 } = new MessageChannel();
const got = new Promise(r => port2.once('message', r));
port1.postMessage({ ab, d: new Date(0), m: new Map([[1, 2]]) }, [ab]);
const m = await got; console.log(ab.byteLength, m.ab.byteLength, m.d instanceof Date, m.m);
port1.close();
try { new MessageChannel().port1.postMessage({ f() {} }) } catch (e) { console.log(e.name) }
```

```text
0 8 true Map(1) { 1 => 2 }
DataCloneError
```

- After the transfer the sender's `ab.byteLength` is `0`, the receiver's is `8`.
- `MessageChannel` creates a pair of ports: a message sent into `port1` arrives at `port2`.
- Functions are not copied: `DataCloneError`.

## SharedArrayBuffer and Atomics

`SharedArrayBuffer` is memory **shared** between threads. Ordinary operations on it race; `Atomics` provides atomic operations:

```js
import { Worker } from 'node:worker_threads';
const sab = new SharedArrayBuffer(4); const ia = new Int32Array(sab);
const wcode = `import { workerData } from 'node:worker_threads'; const a = new Int32Array(workerData); for (let i = 0; i < 1000; i++) Atomics.add(a, 0, 1);`;
const ws = [0, 1, 2, 3].map(() => new Worker(wcode, { eval: true, workerData: sab }));
await Promise.all(ws.map(w => new Promise(r => w.on('exit', r))));
console.log('atomic total', Atomics.load(ia, 0));
```

```text
atomic total 4000
```

Four threads incremented one counter 1000 times each: the total is `4000` thanks to `Atomics.add`. With an ordinary `a[0]++` the result would be smaller and unpredictable. There are also `Atomics.wait`/`notify` for waiting and waking threads.

> ⚠️ Verify: lost updates with an ordinary `a[0]++` on shared memory were not run in this material; the claim follows from the memory model.

## Measuring time and loop delay

```js
import { performance, monitorEventLoopDelay } from 'node:perf_hooks';
const a = performance.now(); await new Promise(r => setTimeout(r, 20)); console.log('elapsed>=19', performance.now() - a >= 19);
console.log(typeof monitorEventLoopDelay);
```

```text
elapsed>=19 true
function
```

- `performance.now()` is monotonic high-resolution time (independent of system clock changes), unlike `Date.now()`.
- `monitorEventLoopDelay()` returns a histogram of loop delays: growing values are a sign that the loop is being blocked.

## Best practices

- Do not block the loop: put heavy work in a worker thread or process it in chunks.
- Create worker threads in advance (a pool): starting a thread costs time and memory.
- Transfer large buffers instead of copying.
- Protect shared memory with `Atomics`; prefer message exchange where possible.
- Measure with `performance.now()`, not `Date.now()`.
- Watch the loop delay in production.

## Common mistakes

- ❌ A worker thread for I/O → ✅ asynchronous APIs are enough.
- ❌ Starting a thread per request → ✅ a pool.
- ❌ Expecting an order of `setTimeout 0` and `setImmediate` in the main module → ✅ rely only on the order inside I/O.
- ❌ A shared counter without `Atomics` → ✅ `Atomics.add`.
- ❌ Sending functions through `postMessage` → ✅ data.
- ❌ Reading a transferred buffer on the sender's side → ✅ after transfer it is empty.

## Related materials

- Next: [expert theory](expert.en.md)
- Promises and microtasks: [12 · Promises and async/await](../12-promises-and-async-await/advanced.en.md)
- Concurrency in C#: [18 · Concurrency](../../../csharp/modules/18-concurrency/overview.en.md)

## Sources

- [Node.js: Event Loop, Timers, and process.nextTick()](https://nodejs.org/en/learn/asynchronous-work/event-loop-timers-and-nexttick)
- [Node.js: Worker threads](https://nodejs.org/docs/latest-v24.x/api/worker_threads.html)
- [MDN: Atomics](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Atomics)
- [MDN: SharedArrayBuffer](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/SharedArrayBuffer)
- [Node.js: perf_hooks](https://nodejs.org/docs/latest-v24.x/api/perf_hooks.html)
