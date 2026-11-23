---
title: "Exercises: types and values"
lang: en
track: javascript
kind: module
doc: exercises
slug: 02-types-and-values
part: 1
tags: [types, typeof, coercion, nan, nullish-coalescing, json]
status: draft
updated: 2026-11-23
---

# Exercises: types and values

🌐 [Русский](exercises.ru.md) · **English**

> Solve it yourself; hints and solutions are under spoilers. Run the code in a `.js` file of a project with `"type": "module"` (see [module 01](../01-environment/basics.en.md)).

## 1. 🟢 Determine the type

**Task:** write a function `kind(x)` that returns `'null'`, `'array'`, `'nan'` or the result of `typeof` for other values.

**Expected result:** `kind(null)` → `'null'`, `kind([])` → `'array'`, `kind(NaN)` → `'nan'`, `kind(10n)` → `'bigint'`, `kind(() => {})` → `'function'`.

<details><summary>Hint</summary>

The order of checks matters: `typeof null` is `'object'`, and `typeof NaN` is `'number'`.

</details>

<details><summary>Solution</summary>

```js
function kind(x) {
  if (x === null) return 'null';
  if (Array.isArray(x)) return 'array';
  if (Number.isNaN(x)) return 'nan';
  return typeof x;
}
console.log(kind(null), kind([]), kind(NaN), kind(10n), kind(() => {}));
```

```text
null array nan bigint function
```

</details>

## 2. 🟢 Default values

**Task:** the function `port(cfg)` returns `cfg.port`, or `3000` if it is `null` or `undefined`; `0` must be preserved.

**Expected result:** `port({port: 0})` → `0`, `port({})` → `3000`, `port({port: null})` → `3000`.

<details><summary>Hint</summary>

Not `||`.

</details>

<details><summary>Solution</summary>

```js
const port = (cfg) => cfg.port ?? 3000;
console.log(port({ port: 0 }), port({}), port({ port: null }));
```

```text
0 3000 3000
```

</details>

## 3. 🟡 Safe comparison of fractions

**Task:** the function `almostEqual(a, b)` returns `true` if the numbers differ by less than `Number.EPSILON`. Check it on `0.1 + 0.2` and `0.3`, then on `1` and `1.1`.

**Expected result:** `true`, `false`.

<details><summary>Hint</summary>

`Math.abs(a - b) < Number.EPSILON`.

</details>

<details><summary>Solution</summary>

```js
const almostEqual = (a, b) => Math.abs(a - b) < Number.EPSILON;
console.log(almostEqual(0.1 + 0.2, 0.3), almostEqual(1, 1.1));
```

```text
true false
```

For large numbers a fixed tolerance `Number.EPSILON` does not work: it fits values on the order of one.

</details>

## 4. 🟡 What does the code print?

**Task:** predict and then check the result: `[10, 9, 1].sort()`, `[] + {}`, `'5' - 3`, `null >= 0`, `JSON.stringify({ a: undefined, b: NaN })`.

**Expected result:** `[ 1, 10, 9 ]`, `[object Object]`, `2`, `true`, `{"b":null}`.

<details><summary>Hint</summary>

`sort` without a comparison function sorts strings; `>=` converts `null` to a number.

</details>

<details><summary>Solution</summary>

```js
console.log([10, 9, 1].sort(), [] + {}, '5' - 3, null >= 0, JSON.stringify({ a: undefined, b: NaN }));
```

```text
[ 1, 10, 9 ] [object Object] 2 true {"b":null}
```

</details>

## 5. 🔴 Values and references

**Task:** the function `bump(o)` must increase `o.n` by 1 and **not** change the passed object: it returns a new object. Show how this differs from a variant that mutates the argument. Then explain why `{ n: 1 } === { n: 1 }` is false.

**Expected result:** the original object keeps `n: 1`, the result has `n: 2`; the comparison of literals is false.

<details><summary>Hint</summary>

Create a new object with the literal `{ ...o, n: o.n + 1 }` (object spread is studied in module 05 of the roadmap).

</details>

<details><summary>Solution</summary>

```js
const bump = (o) => ({ ...o, n: o.n + 1 });
const a = { n: 1 };
const b = bump(a);
console.log(a.n, b.n, a === b, { n: 1 } === { n: 1 });
```

```text
1 2 false false
```

Two literals create two different objects; `===` on objects compares references.

</details>
