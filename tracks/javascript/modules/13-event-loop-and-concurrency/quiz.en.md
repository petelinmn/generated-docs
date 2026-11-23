---
title: "Quiz: the event loop and concurrency"
lang: en
track: javascript
kind: module
doc: quiz
slug: 13-event-loop-and-concurrency
part: 2
tags: [event-loop, microtask, worker-threads, atomics, timers]
status: draft
updated: 2026-11-23
---

# Quiz: the event loop and concurrency

🌐 [Русский](quiz.ru.md) · **English**

> Answer on your own first, then open the answer. The label shows the question level: 🟢 [basic](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 In what order is the text printed?

```js
console.log('1');
setTimeout(() => console.log('2'), 0);
Promise.resolve().then(() => console.log('3'));
console.log('4');
```

- A) `1 2 3 4`
- B) `1 4 3 2`
- C) `1 3 4 2`
- D) `1 4 2 3`

<details><summary>Answer</summary>

**B.** Synchronous code first (`1`, `4`), then the microtask (`3`), then the timer (`2`).
→ [Basic theory](basics.en.md#macrotasks-and-microtasks)

</details>

## 2. 🟢 What does `setTimeout(fn, 100)` mean?

- A) A call exactly after 100 ms
- B) A call no earlier than 100 ms later
- C) A call no later than 100 ms later
- D) A call before microtasks arrive

<details><summary>Answer</summary>

**B.** The delay is a minimum: blocking code or loop load shifts the call.
→ [Basic theory](basics.en.md#timers)

</details>

## 3. 🟢 What happens to a 10 ms timer if right after creating it the code spins in a synchronous loop for 80 ms?

- A) It fires after 10 ms, interrupting the loop
- B) It fires after the loop ends
- C) It never fires
- D) It fires twice

<details><summary>Answer</summary>

**B.** One thread: until the stack is free, nothing else runs.
→ [Basic theory](basics.en.md#blocking-the-loop)

</details>

## 4. 🟢 What is the type of the value `setTimeout` returns in Node.js?

- A) A number
- B) A string
- C) A `Timeout` object with `ref()`/`unref()`
- D) `undefined`

<details><summary>Answer</summary>

**C.** In a browser it is a number, in Node.js an object.
→ [Basic theory](basics.en.md#timers)

</details>

## 5. 🟡 What is printed first inside an `fs.readFile` callback?

```js
setTimeout(() => console.log('timeout'), 0);
setImmediate(() => console.log('immediate'));
```

- A) `timeout`
- B) `immediate`
- C) The order is random
- D) Nothing

<details><summary>Answer</summary>

**B.** Inside an I/O callback the check phase (`setImmediate`) follows poll, and timers wait for the next iteration. In the main module the order is not guaranteed.
→ [Advanced theory](advanced.en.md#the-phases-of-the-nodejs-event-loop)

</details>

## 6. 🟡 What happens to the sender's buffer after `postMessage(msg, [ab])`?

- A) It stays as it was
- B) It becomes empty: `byteLength` is `0`
- C) It is deleted from memory and reading properties throws
- D) It is copied

<details><summary>Answer</summary>

**B.** A transfer "detaches" the buffer from the sender; the receiver sees the original size.
→ [Advanced theory](advanced.en.md#messages-copying-and-transfer)

</details>

## 7. 🟡 What happens with `postMessage({ f() {} })`?

- A) The function is copied
- B) `DataCloneError`
- C) The function turns into a string
- D) The property is skipped

<details><summary>Answer</summary>

**B.** Structured clone does not copy functions.
→ [Advanced theory](advanced.en.md#messages-copying-and-transfer)

</details>

## 8. 🟡 Four threads each run `Atomics.add(a, 0, 1)` 1000 times on a shared `Int32Array`. What is `Atomics.load(a, 0)`?

- A) Less than `4000`, depends on races
- B) `4000`
- C) `1000`
- D) `0`

<details><summary>Answer</summary>

**B.** `Atomics.add` is atomic; without it updates would be lost.
→ [Advanced theory](advanced.en.md#sharedarraybuffer-and-atomics)

</details>

## 9. 🟡 Choose all true statements

- [ ] A) A worker thread has its own event loop
- [ ] B) Objects are shared between threads by reference
- [ ] C) `SharedArrayBuffer` is memory shared by threads
- [ ] D) A worker thread speeds up network I/O

<details><summary>Answer</summary>

**A, C.** B is wrong: objects are copied or transferred; D is wrong: I/O is already asynchronous, threads help with CPU load.
→ [Advanced theory](advanced.en.md#worker-threads)

</details>

## 10. 🔴 Find the mistake

> "`try { setTimeout(() => { throw new Error('x') }, 0) } catch (e) { … }` will catch the error."

<details><summary>Answer</summary>

The callback runs later on a different stack, so `try/catch` does not see it; the exception becomes `uncaughtException` and terminates the process (without a handler).
→ [Gotcha 4](expert.en.md#4-an-exception-in-a-timer-or-microtask)

</details>

## 11. 🔴 In what order is the text printed in a CommonJS file?

```js
console.log('A');
process.nextTick(() => console.log('B'));
Promise.resolve().then(() => console.log('C'));
queueMicrotask(() => console.log('D'));
setTimeout(() => console.log('E'), 0);
console.log('F');
```

- A) `A F C D B E`
- B) `A F B C D E`
- C) `A B C D E F`
- D) `A F E B C D`

<details><summary>Answer</summary>

**B.** Synchronous code (`A`, `F`), then `nextTick` (`B`), microtasks in queueing order (`C`, `D`), the timer (`E`).
→ [Gotcha 3](expert.en.md#3-nexttick-and-promises)

</details>

## 12. 🔴 What happens if a microtask endlessly queues a new microtask?

- A) Timers fire on time
- B) Timers and I/O never get control ("starvation")
- C) A `RangeError` occurs
- D) Microtasks run one per iteration

<details><summary>Answer</summary>

**B.** The microtask queue runs until empty; in the experiment a `0` timer fired only after 200001 microtasks.
→ [Internals](expert.en.md#internals)

</details>

## 13. 🔴 Open question

Explain how Node.js handles thousands of simultaneous connections with one JavaScript thread, and when worker threads are needed.

<details><summary>Key points of a good answer</summary>

- I/O is asynchronous: the OS and libuv perform operations in the background; ready results return as callbacks in the event loop's queues.
- JavaScript callbacks run one at a time, so fast handlers serve many connections; microtasks run after every macrotask.
- Blocking synchronous code delays everyone; heavy computation is moved to worker threads (messages, transfer, `SharedArrayBuffer` + `Atomics`) or split into chunks.
- Worker threads are for CPU, not I/O; it is better to create them as a pool.

</details>

→ [Expert theory](expert.en.md#internals)
