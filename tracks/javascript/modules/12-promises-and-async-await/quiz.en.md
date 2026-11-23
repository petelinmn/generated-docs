---
title: "Quiz: promises and async/await"
lang: en
track: javascript
kind: module
doc: quiz
slug: 12-promises-and-async-await
part: 2
tags: [promise, async-await, promise-all, abort-signal, microtask]
status: draft
updated: 2026-11-23
---

# Quiz: promises and async/await

🌐 [Русский](quiz.ru.md) · **English**

> Answer on your own first, then open the answer. The label shows the question level: 🟢 [basic](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 What does the code print?

```js
const p = new Promise((res, rej) => { res(1); res(2); rej(new Error('x')); });
p.then(v => console.log(v));
```

- A) `1`
- B) `2`
- C) `Error: x`
- D) Nothing

<details><summary>Answer</summary>

**A.** A promise changes state once; the repeated `res` and `rej` are ignored.
→ [Basic theory](basics.en.md#promise-states)

</details>

## 2. 🟢 In what order are the letters printed?

```js
console.log('A');
setTimeout(() => console.log('D'), 0);
Promise.resolve().then(() => console.log('C'));
console.log('B');
```

- A) `A B C D`
- B) `A C B D`
- C) `A D C B`
- D) `A B D C`

<details><summary>Answer</summary>

**A.** Synchronous code first (`A`, `B`), then the `then` microtask (`C`), then the timer (`D`).
→ [Internals](expert.en.md#internals)

</details>

## 3. 🟢 What does the code print?

```js
Promise.resolve(1)
  .then(v => v + 1)
  .then(v => { throw new Error('boom ' + v) })
  .then(() => console.log('skipped'))
  .catch(e => { console.log('caught', e.message); return 'rec' })
  .then(v => console.log('after', v));
```

- A) `caught boom 2`, `after rec`
- B) `skipped`, `caught boom 2`
- C) `caught boom 2`, `after undefined`
- D) Only `caught boom 2`

<details><summary>Answer</summary>

**A.** The error skips `then` up to `catch`; `catch` returned `'rec'`, and the chain continues.
→ [Basic theory](basics.en.md#then-catch-finally)

</details>

## 4. 🟢 What does an `async` function return if it has `return 1` inside?

- A) `1`
- B) A promise fulfilled with `1`
- C) `undefined`
- D) Depends on whether there is an `await`

<details><summary>Answer</summary>

**B.** An `async` function always returns a promise.
→ [Basic theory](basics.en.md#async-and-await)

</details>

## 5. 🟡 What does the code print?

```js
async function bad() { throw new Error('E') }
const r = await (async () => { try { return bad() } catch { return 'caught' } })().catch(e => 'escaped ' + e.message);
console.log(r);
```

- A) `caught`
- B) `escaped E`
- C) `undefined`
- D) `TypeError`

<details><summary>Answer</summary>

**B.** `return bad()` without `await` returns the rejected promise past the `catch`. With `return await bad()` it would be `caught`.
→ [Advanced theory](advanced.en.md#return-and-return-await)

</details>

## 6. 🟡 What does `Promise.all` return if one of the promises is rejected?

- A) An array with the results of the others
- B) A rejected promise with the first error
- C) An array of `{status, ...}`
- D) It waits for all and then throws all the errors

<details><summary>Answer</summary>

**B.** `all` is rejected on the first error (but the other operations keep running). `allSettled` gives the results of all.
→ [Advanced theory](advanced.en.md#combinators)

</details>

## 7. 🟡 What does the code print?

```js
const out = [];
[1, 2, 3].forEach(async x => { await sleep(5); out.push(x) });
console.log(out.length);
```

- A) `3`
- B) `0`
- C) `1`
- D) `undefined`

<details><summary>Answer</summary>

**B.** `forEach` does not wait for the returned promises. To wait, use `for … of` or `Promise.all(arr.map(…))`.
→ [Advanced theory](advanced.en.md#sequential-and-parallel)

</details>

## 8. 🟡 What does `Promise.any([Promise.reject(new Error('a')), Promise.reject(new Error('b'))])` return?

- A) A rejection `Error: a`
- B) A rejection with an `AggregateError` whose `errors` hold both reasons
- C) A fulfillment with `undefined`
- D) A fulfillment with an array of errors

<details><summary>Answer</summary>

**B.** If all are rejected, `any` is rejected with an `AggregateError` (`All promises were rejected`).
→ [Advanced theory](advanced.en.md#combinators)

</details>

## 9. 🟡 Choose all true statements

- [ ] A) `Promise.race` stops the losing operations
- [ ] B) `AbortSignal.timeout(ms)` creates a signal that fires after `ms`
- [ ] C) `Promise.allSettled` is never rejected
- [ ] D) `finally` receives the promise's result as an argument

<details><summary>Answer</summary>

**B, C.** A is wrong: `race` cancels nothing; D is wrong: `finally` receives no arguments.
→ [Advanced theory](advanced.en.md#timeout-and-cancellation)

</details>

## 10. 🔴 Find the mistake

> "`Promise.resolve(p)` always creates a new promise."

<details><summary>Answer</summary>

If `p` is a promise, the same object is returned: `Promise.resolve(p) === p`. And `new Promise(r => r(p))` creates a new one.
→ [Gotcha 5](expert.en.md#5-promiseresolve-does-not-always-create-a-new-object)

</details>

## 11. 🔴 In what order is the text printed in a CommonJS file?

```js
console.log('1 sync');
setTimeout(() => console.log('5 timeout'), 0);
Promise.resolve().then(() => console.log('3 micro'));
process.nextTick(() => console.log('2 nextTick'));
```

- A) `1`, `3`, `2`, `5`
- B) `1`, `2`, `3`, `5`
- C) `1`, `5`, `2`, `3`
- D) `1`, `2`, `5`, `3`

<details><summary>Answer</summary>

**B.** In CommonJS `nextTick` runs before the promise queue, and the timer after the microtasks. In an ES module `nextTick` would come after the promises.
→ [Gotcha 3](expert.en.md#3-nexttick-and-promises)

</details>

## 12. 🔴 What happens on an unhandled rejection in Node.js 24 without an `unhandledRejection` handler?

- A) Nothing, the error is ignored
- B) A warning and the process keeps working
- C) The error is printed and the process terminates with code `1`
- D) The promise is retried automatically

<details><summary>Answer</summary>

**C.** In the experiment the process terminated with an error and code `1`.
→ [Advanced theory](advanced.en.md#unhandled-rejections)

</details>

## 13. 🔴 Open question

Explain how `async`/`await` relates to promises and the microtask queue, and why `await` in a loop is sequential while `Promise.all` is parallel.

<details><summary>Key points of a good answer</summary>

- An `async` function returns a promise; `await` converts a value to a promise and subscribes the function's continuation to its result (a microtask).
- The function is suspended and control returns to the caller; after the synchronous code the microtask queue runs before timers.
- `await` in a loop starts the next operation only after the previous one finishes; `Promise.all` starts all operations at once and waits for them together.
- Limiting concurrency needs a pool, cancellation needs an `AbortSignal`; `race`/`all` cancel nothing.

</details>

→ [Expert theory](expert.en.md#internals)
