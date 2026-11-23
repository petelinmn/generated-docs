---
title: "Exercises: functions"
lang: en
track: javascript
kind: module
doc: exercises
slug: 04-functions
part: 1
tags: [functions, this, bind, closures, default-parameters, rest-parameters]
status: draft
updated: 2026-11-23
---

# Exercises: functions

🌐 [Русский](exercises.ru.md) · **English**

> Solve it yourself; hints and solutions are under spoilers. Run the code in an ES module file.

## 1. 🟢 A sum with a remainder

**Task:** write a function `sum(...nums)` that returns the sum of all arguments, and `sumWithBase(base = 0, ...nums)` that adds an initial value.

**Expected result:** `sum(1, 2, 3)` → `6`, `sum()` → `0`, `sumWithBase(10, 1, 2)` → `13`, `sumWithBase()` → `0`.

<details><summary>Hint</summary>

`nums.reduce((a, b) => a + b, 0)`.

</details>

<details><summary>Solution</summary>

```js
const sum = (...nums) => nums.reduce((a, b) => a + b, 0);
const sumWithBase = (base = 0, ...nums) => base + sum(...nums);
console.log(sum(1, 2, 3), sum(), sumWithBase(10, 1, 2), sumWithBase());
```

```text
6 0 13 0
```

</details>

## 2. 🟢 A safe `parseInt`

**Task:** rewrite `['1','2','3'].map(parseInt)` so that the result is `[1, 2, 3]`, in two ways.

**Expected result:** `[1, 2, 3]` in both cases.

<details><summary>Hint</summary>

Either `Number`, or a wrapper with radix `10`.

</details>

<details><summary>Solution</summary>

```js
console.log(['1', '2', '3'].map(Number), ['1', '2', '3'].map(s => parseInt(s, 10)));
```

```text
[ 1, 2, 3 ] [ 1, 2, 3 ]
```

</details>

## 3. 🟡 A lost `this`

**Task:** an object `counter` with a field `n` and a method `inc()`; the method is passed as a callback (`const f = counter.inc; f()`). Show the error and fix it two ways: `bind` and an arrow wrapper.

**Expected result:** without the fix a `TypeError`; with the fix the counter increases.

<details><summary>Hint</summary>

`counter.inc.bind(counter)` and `() => counter.inc()`.

</details>

<details><summary>Solution</summary>

```js
const counter = { n: 0, inc() { return ++this.n; } };
const lost = counter.inc;
try { lost() } catch (e) { console.log(e.name + ': ' + e.message) }
const bound = counter.inc.bind(counter);
const wrapped = () => counter.inc();
console.log(bound(), wrapped());
```

```text
TypeError: Cannot read properties of undefined (reading 'n')
1 2
```

</details>

## 4. 🟡 Partial application

**Task:** using `bind`, create from `volume(h, w, d)` a function `volumeOfHeight10(w, d)` whose height is `10`. Show its `length`.

**Expected result:** `volumeOfHeight10(2, 3)` → `60`, `length` is `2`.

<details><summary>Hint</summary>

`bind` fixes arguments **from the start**; to fix the height, put it first in the parameter list.

</details>

<details><summary>Solution</summary>

```js
const volume = (h, w, d) => h * w * d;           // height is the first parameter
const volumeOfHeight10 = volume.bind(null, 10);
console.log(volumeOfHeight10(2, 3), volumeOfHeight10.length);
```

```text
60 2
```

`bind` can fix only the leading arguments, so the needed parameter is put first.

</details>

## 5. 🔴 Counters and closures

**Task:** write a factory `makeCounter(step = 1)` that returns a function that increases its own value by `step` on each call. Two counters must not affect each other. Make sure `new makeCounter()` does not work: the factory is an arrow.

**Expected result:** the values are independent; `new` on an arrow is a `TypeError`.

<details><summary>Hint</summary>

The counter variable lives inside the factory; the factory is an arrow.

</details>

<details><summary>Solution</summary>

```js
const makeCounter = (step = 1) => { let c = 0; return () => (c += step); };
const a = makeCounter(), b = makeCounter(5);
console.log(a(), a(), b(), b(), a());
try { new makeCounter() } catch (e) { console.log(e.name + ': ' + e.message) }
```

```text
1 2 5 10 3
TypeError: makeCounter is not a constructor
```

</details>
