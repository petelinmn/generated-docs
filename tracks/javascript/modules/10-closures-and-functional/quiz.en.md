---
title: "Quiz: closures and functional style"
lang: en
track: javascript
kind: module
doc: quiz
slug: 10-closures-and-functional
part: 2
tags: [closure, currying, memoization, immutability]
status: draft
updated: 2026-11-23
---

# Quiz: closures and functional style

🌐 [Русский](quiz.ru.md) · **English**

> Answer on your own first, then open the answer. The label shows the question level: 🟢 [basic](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 What does the code print?

```js
function makeCounter() { let n = 0; return { inc: () => ++n, get: () => n }; }
const c1 = makeCounter(), c2 = makeCounter();
c1.inc(); c1.inc(); c2.inc();
console.log(c1.get(), c2.get());
```

- A) `3 3`
- B) `2 1`
- C) `1 1`
- D) `2 2`

<details><summary>Answer</summary>

**B.** Every call of `makeCounter` creates its own environment with its own `n`.
→ [Basic theory](basics.en.md#what-a-closure-is)

</details>

## 2. 🟢 What does the code print?

```js
let x = 1; const getX = () => x; x = 2; console.log(getX());
```

- A) `1`
- B) `2`
- C) `undefined`
- D) `ReferenceError`

<details><summary>Answer</summary>

**B.** A closure keeps the variable, not the value at the moment of creation.
→ [Basic theory](basics.en.md#a-closure-keeps-the-variable-not-the-value)

</details>

## 3. 🟢 What does the code print?

```js
const fs = [];
for (var i = 0; i < 3; i++) fs.push(() => i);
console.log(fs.map(f => f()));
```

- A) `[ 0, 1, 2 ]`
- B) `[ 3, 3, 3 ]`
- C) `[ 2, 2, 2 ]`
- D) `[]`

<details><summary>Answer</summary>

**B.** `var` has one variable for the whole loop; by the time of the calls `i` is `3`. With `let` it would be `[ 0, 1, 2 ]`.
→ [Basic theory](basics.en.md#closures-in-a-loop)

</details>

## 4. 🟢 What does `bank.balance` return?

```js
const bank = (() => { let balance = 100; return { deposit(x) { balance += x; return balance; } }; })();
```

- A) `100`
- B) `undefined`
- C) `ReferenceError`
- D) `0`

<details><summary>Answer</summary>

**B.** `balance` is a local variable of the closure; the object has no such property.
→ [Basic theory](basics.en.md#factories-and-private-state)

</details>

## 5. 🟡 What does `['1', '2', '3'].map(parseInt)` return?

- A) `[ 1, 2, 3 ]`
- B) `[ 1, NaN, NaN ]`
- C) `[ '1', '2', '3' ]`
- D) `TypeError`

<details><summary>Answer</summary>

**B.** `map` passes the index as the second argument, and `parseInt` treats it as the radix.
→ [Advanced theory](advanced.en.md#higher-order-functions)

</details>

## 6. 🟡 What does the code print?

```js
const pipe = (...fns) => x => fns.reduce((v, f) => f(v), x);
const inc = x => x + 1, sq = x => x * x;
console.log(pipe(inc, sq)(3));
```

- A) `10`
- B) `16`
- C) `7`
- D) `12`

<details><summary>Answer</summary>

**B.** `pipe` goes left to right: `sq(inc(3))` = `16`. `compose(inc, sq)(3)` would give `10`.
→ [Advanced theory](advanced.en.md#composition)

</details>

## 7. 🟡 What happens with `[].reduce((a, b) => a + b)`?

- A) `0`
- B) `undefined`
- C) `TypeError: Reduce of empty array with no initial value`
- D) `NaN`

<details><summary>Answer</summary>

**C.** Without an initial value an empty array is an error. Pass an initial value: `reduce(f, 0)`.
→ [Advanced theory](advanced.en.md#higher-order-functions)

</details>

## 8. 🟡 How many times does the function run?

```js
let calls = 0; const slowSq = memo(n => { calls++; return n * n; });
slowSq(4); slowSq(4); slowSq(5); console.log(calls);
```

(`memo` is a `Map` cache.)

- A) `3`
- B) `2`
- C) `1`
- D) `0`

<details><summary>Answer</summary>

**B.** For `4` the result came from the cache; `4` and `5` were computed.
→ [Advanced theory](advanced.en.md#memoization)

</details>

## 9. 🟡 Choose all true statements

- [ ] A) `Object.freeze` freezes nested objects
- [ ] B) `{ ...o }` makes a shallow copy
- [ ] C) `structuredClone` can copy functions
- [ ] D) `toSorted()` does not change the original array

<details><summary>Answer</summary>

**B, D.** `freeze` is shallow (A is wrong), `structuredClone` throws `DataCloneError` on functions (C is wrong).
→ [Advanced theory](advanced.en.md#immutability)

</details>

## 10. 🔴 Find the mistake

> "`curry((a, b = 2) => a + b)(1)` returns a function waiting for the second argument."

<details><summary>Answer</summary>

`fn.length` is `1` (parameters before the first default value are counted), so the function is called at once and returns `3`.
→ [Gotcha 5](expert.en.md#5-currying-and-default-values)

</details>

## 11. 🔴 What does the code print in sloppy mode (a CommonJS file)?

```js
const o = Object.freeze({ a: 1 }); o.a = 2; o.z = 1; console.log(o.a, o.z);
```

- A) `2 1`
- B) `1 undefined`
- C) `TypeError`
- D) `1 1`

<details><summary>Answer</summary>

**B.** In sloppy mode the write is silently ignored; in strict mode and in an ES module it would be a `TypeError`.
→ [Gotcha 3](expert.en.md#3-freezing-and-modes)

</details>

## 12. 🔴 What does `structuredClone(new K())` give for an instance of class `K`?

- A) An instance of `K` with methods
- B) A plain object without the prototype of `K`: `instanceof K` is `false`
- C) `DataCloneError` for any class
- D) `null`

<details><summary>Answer</summary>

**B.** The data is copied, the prototype is lost (`k.constructor === Object`).
→ [Ways to copy](expert.en.md#ways-to-copy)

</details>

## 13. 🔴 Open question

Explain what a closure is at the level of environments and why counters from one factory are independent.

<details><summary>Key points of a good answer</summary>

- Every function call creates an environment record with local variables and a reference to the outer environment.
- A function remembers the environment when it is created (`[[Environment]]`); names are looked up along the chain.
- A closure refers to a binding, not a value, so it sees changes.
- Two calls of a factory create two environments: hence the independence; two functions from one call share one environment.

</details>

→ [Expert theory](expert.en.md#internals)
