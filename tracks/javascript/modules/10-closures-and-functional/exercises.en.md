---
title: "Exercises: closures and functional style"
lang: en
track: javascript
kind: module
doc: exercises
slug: 10-closures-and-functional
part: 2
tags: [closure, pipe, memoization, immutability]
status: draft
updated: 2026-11-23
---

# Exercises: closures and functional style

🌐 [Русский](exercises.ru.md) · **English**

> Solve on your own; hints and solutions are under spoilers. Run the code in a file next to a `package.json` (`{"type": "module"}`).

## 1. 🟢 A counter with a step

**Task:** write `makeCounter(step = 1)` with the methods `inc()` (adds `step` and returns the value), `reset()` and `get()`. Check that two counters are independent.

**Expected result:** for `makeCounter(5)` after two `inc()` calls — `10`, after `reset()` — `0`; a new `makeCounter()` after one `inc()` — `1`, the old one — `0`.

<details><summary>Hint</summary>

The state `n` is a local variable of the factory; the methods are closures over it.

</details>

<details><summary>Solution</summary>

```js
function makeCounter(step = 1) { let n = 0; return { inc: () => (n += step), reset: () => { n = 0; }, get: () => n }; }
const c = makeCounter(5); c.inc(); c.inc(); console.log(c.get()); c.reset(); console.log(c.get());
const other = makeCounter(); other.inc(); console.log(other.get(), c.get());
```

```text
10
0
1 0
```

</details>

## 2. 🟢 A slug pipeline

**Task:** use `pipe` to build a function that trims spaces, lowercases the string and replaces runs of whitespace with `-`.

**Expected result:** `slug('  Hello Big World ')` → `hello-big-world`.

<details><summary>Hint</summary>

`const pipe = (...fns) => x => fns.reduce((v, f) => f(v), x);`

</details>

<details><summary>Solution</summary>

```js
const pipe = (...fns) => x => fns.reduce((v, f) => f(v), x);
const slug = pipe(s => s.trim(), s => s.toLowerCase(), s => s.replace(/\s+/g, '-'));
console.log(slug('  Hello Big World '));
```

```text
hello-big-world
```

</details>

## 3. 🟡 Fix `map` and `reduce`

**Task:** `['10', '10', '10'].map(parseInt)` gives the wrong thing. Fix it to get `[ 10, 10, 10 ]`; then compute the sum of `['1', '2', '3']` with `reduce` and the sum of an empty array without an error.

**Expected result:** `[ 10, 10, 10 ] 6 0`.

<details><summary>Hint</summary>

Pass the radix explicitly and set the initial value of `reduce`.

</details>

<details><summary>Solution</summary>

```js
console.log(['10', '10', '10'].map(s => parseInt(s, 10)), ['1', '2', '3'].map(Number).reduce((a, b) => a + b, 0), [].reduce((a, b) => a + b, 0));
```

```text
[ 10, 10, 10 ] 6 0
```

</details>

## 4. 🟡 Memoization with several arguments

**Task:** write `memoize(fn)` that works for functions with several arguments. Show that the function `add` is called once for `(1, 2)`, while `(2, 1)` is a separate call.

**Expected result:** `3 3 3 2` (three results and the number of computations).

<details><summary>Hint</summary>

The cache key is `JSON.stringify(args)`.

</details>

<details><summary>Solution</summary>

```js
function memoize(fn) { const cache = new Map(); return (...args) => { const k = JSON.stringify(args); if (!cache.has(k)) cache.set(k, fn(...args)); return cache.get(k); }; }
let calls = 0; const add = memoize((a, b) => { calls++; return a + b; });
console.log(add(1, 2), add(1, 2), add(2, 1), calls);
```

```text
3 3 3 2
```

The approach fits only arguments that serialize to JSON without loss.

</details>

## 5. 🔴 An immutable update

**Task:** write `setIn(obj, path, value)` that returns a **new** object with a changed nested value and does not change the original. Unchanged branches must stay the same references (structural sharing).

**Expected result:** `B A true true false` — the new name, the old name, the `theme` branch is not copied, the `tags` array is not copied, the objects differ.

<details><summary>Hint</summary>

Recursion: `{ ...obj, [k]: rest.length ? setIn(obj[k] ?? {}, rest, value) : value }`.

</details>

<details><summary>Solution</summary>

```js
const setIn = (obj, [k, ...rest], v) => ({ ...obj, [k]: rest.length ? setIn(obj[k] ?? {}, rest, v) : v });
const state = { user: { name: 'A', tags: ['x'] }, theme: { dark: false } };
const next = setIn(state, ['user', 'name'], 'B');
console.log(next.user.name, state.user.name, next.theme === state.theme, next.user.tags === state.user.tags, next === state);
```

```text
B A true true false
```

Only the objects on the path to the changed value are copied.

</details>
