---
title: "The event loop and concurrency: basics"
lang: en
track: javascript
kind: module
doc: basics
slug: 13-event-loop-and-concurrency
part: 2
tags: [event-loop, call-stack, macrotask, microtask, set-timeout, set-immediate, next-tick, blocking]
versions: "ECMAScript 2025+, Node.js 24.15 LTS"
status: draft
updated: 2026-11-23
---

# The event loop and concurrency: basics

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [12 · Promises and async/await](../12-promises-and-async-await/overview.en.md) · **Time:** ~2.5 h

All examples were run on Node.js 24.15; the files sit next to a `package.json` (`{"type": "module"}`) unless stated otherwise. The delays in the output are loose: the exact time depends on the machine.

## Why it matters

To understand **when** code runs, why a server "hangs", and in what order timers and promises fire. Without this, asynchronous bugs look random.

## One thread

JavaScript runs code in one thread: there is a **call stack**, and until it is empty no other task starts. The runtime handles I/O, timers and network in the background and returns the result through **queues**, from which the **event loop** takes tasks when the stack is free.

## Macrotasks and microtasks

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

- **Macrotasks:** timers, I/O, `setImmediate`. The loop takes them one at a time.
- **Microtasks:** `then`/`catch`/`finally` handlers, continuations after `await`, `queueMicrotask`. After every macrotask the microtask queue runs **to completion** before the next macrotask starts.
- So a promise always fires before a `0` timer.

More about promises is in [module 12](../12-promises-and-async-await/expert.en.md#internals).

## Timers

```js
setTimeout((a, b) => console.log('args', a, b), 100, 'x', 'y');
let n = 0; const iv = setInterval(() => { if (++n === 3) { clearInterval(iv); console.log('interval x3'); } }, 3);
const t = setTimeout(() => {}, 1000);
console.log(typeof t, typeof t.ref, t.hasRef());
t.unref(); console.log(t.hasRef()); clearTimeout(t);
```

```text
object function true
false
interval x3
args x y
```

- `setTimeout(fn, ms, ...args)` calls `fn` no earlier than `ms` milliseconds later; extra arguments are passed to `fn`.
- `setInterval` repeats the call; it is stopped with `clearInterval`.
- In Node.js a timer is an **object** (`typeof t` is `object`, not a number as in browsers) with the methods `ref()`, `unref()`, `hasRef()`. `unref()` allows the process to exit even if the timer has not fired.
- `ms` is a **minimum** delay: the actual firing depends on the load of the loop.

## Blocking the loop

Long synchronous code delays everything:

```js
const t0 = Date.now();
setTimeout(() => console.log('timer fired late:', Date.now() - t0 >= 80), 10);
while (Date.now() - t0 < 80);
console.log('blocked');
```

```text
blocked
timer fired late: true
```

The 10 ms timer fired only after the 80 ms blocking loop. On a server this means one heavy request slows all the others.

**How not to block:** split the work into chunks and give control back to the loop with `setImmediate`:

```js
function heavy(n) { let s = 0; for (let i = 0; i < n; i++) s += i; return s; }
let ticks = 0; const iv = setInterval(() => ticks++, 1);
let done = 0;
await new Promise(res => { function step() { heavy(2e6); if (++done < 20) setImmediate(step); else res(); } step(); });
clearInterval(iv); console.log('chunked: timer ran during work', ticks > 0);
```

```text
chunked: timer ran during work true
```

The same amount of work in one block would not let the timer fire even once (in the experiment — `blocking: timer ran during work false`). For heavy computations a [worker thread](advanced.en.md#worker-threads) is better.

## setImmediate and process.nextTick

- `setImmediate(fn)` — call `fn` on the next iteration of the loop, **after** the current I/O.
- `process.nextTick(fn)` — call `fn` **right after the current operation**, before promise microtasks (in CommonJS).

```js
// CommonJS (.cjs)
Promise.resolve().then(() => { console.log('p1'); process.nextTick(() => console.log('tick from p1')); }).then(() => console.log('p2'));
process.nextTick(() => { console.log('n1'); Promise.resolve().then(() => console.log('promise from n1')); });
console.log('sync end');
```

```text
sync end
n1
p1
promise from n1
p2
tick from p1
```

First the `nextTick` queue (`n1`), then the whole promise queue (`p1`, `promise from n1`, `p2`), and only then the `nextTick` queued from a promise (`tick from p1`). A recursive `nextTick` can "freeze" the loop, letting neither I/O nor timers in. In an ES module the order differs ([module 12](../12-promises-and-async-await/expert.en.md#internals)).

## Beginner mistakes

- ❌ Expecting `setTimeout(fn, 0)` to run at once → ✅ after synchronous code and microtasks.
- ❌ A long synchronous loop in a request handler → ✅ chunks or a worker thread.
- ❌ Relying on the exact time of `setTimeout` → ✅ treat the delay as a minimum.
- ❌ A recursive `process.nextTick` → ✅ `setImmediate`.
- ❌ Using a numeric timer id as in browsers → ✅ in Node.js it is an object.
- ❌ Forgetting `clearInterval` → ✅ the process will not exit.

## Related materials

- Next: [advanced theory](advanced.en.md) — Node.js loop phases, worker threads, shared memory
- Earlier: [12 · Promises and async/await](../12-promises-and-async-await/basics.en.md)

## Sources

- [Node.js: The Node.js Event Loop](https://nodejs.org/en/learn/asynchronous-work/event-loop-timers-and-nexttick)
- [Node.js: Timers](https://nodejs.org/docs/latest-v24.x/api/timers.html)
- [MDN: The event loop](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Event_loop)
- [MDN: Using microtasks](https://developer.mozilla.org/en-US/docs/Web/API/HTML_DOM_API/Microtask_guide)
