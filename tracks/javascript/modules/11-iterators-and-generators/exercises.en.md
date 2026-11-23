---
title: "Exercises: iterators and generators"
lang: en
track: javascript
kind: module
doc: exercises
slug: 11-iterators-and-generators
part: 2
tags: [iterator, generator, async-generator, iterator-helpers]
status: draft
updated: 2026-11-23
---

# Exercises: iterators and generators

🌐 [Русский](exercises.ru.md) · **English**

> Solve on your own; hints and solutions are under spoilers. Run the code in a file next to a `package.json` (`{"type": "module"}`).

## 1. 🟢 A countdown

**Task:** create a class `Countdown(n)` that can be walked with `for...of` and spread: it hands out `n`, `n-1`, …, `1`. Find the maximum of `Countdown(4)` via `Math.max(...)`.

**Expected result:** `[ 3, 2, 1 ] 4`.

<details><summary>Hint</summary>

The method `*[Symbol.iterator]() { … }` is a generator.

</details>

<details><summary>Solution</summary>

```js
class Countdown { constructor(n) { this.n = n } *[Symbol.iterator]() { for (let i = this.n; i > 0; i--) yield i; } }
console.log([...new Countdown(3)], Math.max(...new Countdown(4)));
```

```text
[ 3, 2, 1 ] 4
```

</details>

## 2. 🟢 Fibonacci and `take`

**Task:** write an infinite generator `fib()` and a function `take(n, it)`. Print the first 8 numbers.

**Expected result:** `0 1 1 2 3 5 8 13`.

<details><summary>Hint</summary>

The state is a pair `[a, b]`; `take` leaves the loop after `n` values.

</details>

<details><summary>Solution</summary>

```js
function* fib() { let [a, b] = [0, 1]; while (true) { yield a; [a, b] = [b, a + b]; } }
function* take(n, it) { if (n <= 0) return; let i = 0; for (const v of it) { yield v; if (++i >= n) return; } }
console.log([...take(8, fib())].join(' '));
```

```text
0 1 1 2 3 5 8 13
```

</details>

## 3. 🟡 A running average

**Task:** write a generator `avg()` that receives numbers through `next(x)` and returns the current average. Feed it `10`, `20`, `60`.

**Expected result:** `10 15 30`.

<details><summary>Hint</summary>

The first `next()` only starts the generator; keep the sum and the counter in local variables.

</details>

<details><summary>Solution</summary>

```js
function* avg() { let sum = 0, n = 0; while (true) { const x = yield n ? sum / n : undefined; sum += x; n++; } }
const a = avg(); a.next(); console.log(a.next(10).value, a.next(20).value, a.next(60).value);
```

```text
10 15 30
```

</details>

## 4. 🟡 Chunks and helpers

**Task:** (a) write a generator `chunk(it, size)` that hands out arrays of `size` elements (the last may be shorter). (b) Use iterator helpers to get the first 5 even Fibonacci numbers.

**Expected result:** `[ [ 1, 2 ], [ 3, 4 ], [ 5 ] ]` and `[ 0, 2, 8, 34, 144 ]`.

<details><summary>Hint</summary>

For (b) use `fib().filter(...).take(5).toArray()`.

</details>

<details><summary>Solution</summary>

```js
function* chunk(it, size) { let buf = []; for (const v of it) { buf.push(v); if (buf.length === size) { yield buf; buf = []; } } if (buf.length) yield buf; }
console.log([...chunk([1, 2, 3, 4, 5], 2)]);
console.log(fib().filter(x => x % 2 === 0).take(5).toArray());
```

```text
[ [ 1, 2 ], [ 3, 4 ], [ 5 ] ]
[ 0, 2, 8, 34, 144 ]
```

`fib` is the generator from exercise 2.

</details>

## 5. 🔴 Paginated loading and releasing a resource

**Task:** (a) write an async generator `pages(n)` that hands out `{ page, items }` with a small delay, and collect all `items`. (b) Write an async generator with `try/finally` that prints `released`, and make sure an early `break` from `for await` releases the resource.

**Expected result:** `[ 10, 11, 20, 21, 30, 31 ]` and `released`.

<details><summary>Hint</summary>

`for await (const { items } of pages(3)) …`; `break` calls the generator's `return()`, and it runs `finally`.

</details>

<details><summary>Solution</summary>

```js
const sleep = ms => new Promise(r => setTimeout(r, ms));
async function* pages(n) { for (let p = 1; p <= n; p++) { await sleep(1); yield { page: p, items: [p * 10, p * 10 + 1] }; } }
const all = []; for await (const { items } of pages(3)) all.push(...items); console.log(all);
async function* resource() { try { yield 1; yield 2; yield 3 } finally { console.log('released') } }
for await (const v of resource()) { if (v === 2) break; }
```

```text
[ 10, 11, 20, 21, 30, 31 ]
released
```

</details>
