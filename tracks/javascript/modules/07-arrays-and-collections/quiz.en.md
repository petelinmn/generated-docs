---
title: "Quiz: arrays and collections"
lang: en
track: javascript
kind: module
doc: quiz
slug: 07-arrays-and-collections
part: 1
tags: [array, sort, set, map, weakmap, typed-array]
status: draft
updated: 2026-11-23
---

# Quiz: arrays and collections

🌐 [Русский](quiz.ru.md) · **English**

> Answer first, then open the answer. The mark shows which level a question comes from: 🟢 [basics](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 What does `console.log(Array(3), Array.of(3))` print?

- A) `[ 3 ] [ 3 ]`
- B) `[ <3 empty items> ] [ 3 ]`
- C) `[ 0, 0, 0 ] [ 3 ]`
- D) `[ undefined, undefined, undefined ] [ 3 ]`

<details><summary>Answer</summary>

**B.** `Array(3)` creates three empty slots, `Array.of(3)` an array of one element.
→ [Basics](basics.en.md#creation)

</details>

## 2. 🟢 What happens to the array `[1, 2, 3]` after `a.length = 1`?

- A) Nothing
- B) `[1]`
- C) `[1, <2 empty items>]`
- D) An error

<details><summary>Answer</summary>

**B.** Decreasing `length` truncates the array.
→ [Basics](basics.en.md#length-and-holes)

</details>

## 3. 🟢 What does `[10, 9, 1].sort()` return?

- A) `[1, 9, 10]`
- B) `[1, 10, 9]`
- C) `[10, 9, 1]`
- D) `[9, 10, 1]`

<details><summary>Answer</summary>

**B.** By default the sort is string-based. A numeric one: `sort((a, b) => a - b)`.
→ [Basics](basics.en.md#sorting)

</details>

## 4. 🟢 Does `toSorted()` change the original array?

- A) Yes
- B) No, it returns a sorted copy
- C) Only for numbers
- D) Only for strings

<details><summary>Answer</summary>

**B.** Unlike `sort()`, which changes the array and returns the same one.
→ [Basics](basics.en.md#mutating-and-copying-methods)

</details>

## 5. 🟡 What does `[].reduce((a, b) => a + b)` return?

- A) `0`
- B) `undefined`
- C) `TypeError: Reduce of empty array with no initial value`
- D) `NaN`

<details><summary>Answer</summary>

**C.** With an initial value `[].reduce(f, 0)` would return `0`.
→ [Basics](basics.en.md#map-filter-reduce-flat)

</details>

## 6. 🟡 What are `[NaN].indexOf(NaN)` and `[NaN].includes(NaN)`?

- A) `0` and `true`
- B) `-1` and `true`
- C) `-1` and `false`
- D) `0` and `false`

<details><summary>Answer</summary>

**B.** `indexOf` uses strict equality, `includes` uses SameValueZero.
→ [Basics](basics.en.md#searching)

</details>

## 7. 🟡 How many elements are in `new Set([1, 2, 2, 3, NaN, NaN, 0, -0])`?

- A) `8`
- B) `7`
- C) `5`
- D) `4`

<details><summary>Answer</summary>

**C.** `1, 2, 3, NaN, 0` remain: `NaN` equals `NaN`, and `0` and `-0` are the same value.
→ [Advanced](advanced.en.md#set)

</details>

## 8. 🟡 What does `JSON.stringify(new Map([[1, 2]]))` return?

- A) `[[1,2]]`
- B) `{"1":2}`
- C) `{}`
- D) `TypeError`

<details><summary>Answer</summary>

**C.** A `Map` is not serialized directly: use `JSON.stringify([...map])`.
→ [Advanced](advanced.en.md#map)

</details>

## 9. 🟡 What does `new Uint8Array([250, 255, 256, -1, 1.9])` give?

- A) `[250, 255, 256, -1, 1.9]`
- B) `[250, 255, 0, 255, 1]`
- C) `[250, 255, 255, 0, 2]`
- D) `RangeError`

<details><summary>Answer</summary>

**B.** Values wrap modulo 256, the fractional part is dropped. `Uint8ClampedArray` clamps the range.
→ [Advanced](advanced.en.md#typed-arrays)

</details>

## 10. 🟡 Select all correct statements

- [ ] A) `Map` accepts objects as keys
- [ ] B) A `WeakMap` can be iterated with `for...of`
- [ ] C) `Set` keeps insertion order
- [ ] D) `WeakMap` accepts a string as a key

<details><summary>Answer</summary>

**A, C.** B is wrong (no iteration), D is wrong: `TypeError: Invalid value used as weak map key`.
→ [Advanced](advanced.en.md#weakmap-and-weakset)

</details>

## 11. 🔴 Find the flaw

> "`Array(2).map(() => 1)` returns `[1, 1]`."

<details><summary>Answer</summary>

`Array(2)` is two holes, and `map` skips holes: the result is `[ <2 empty items> ]`. You need `Array.from({ length: 2 }, () => 1)` or `Array(2).fill(1)`.
→ [Gotcha 3](expert.en.md#3-holes-versus-undefined)

</details>

## 12. 🔴 What does the code print?

```js
const order = new Map(); order.set('b', 1).set('a', 2).set('b', 3);
console.log([...order]);
```

- A) `[['a', 2], ['b', 3]]`
- B) `[['b', 3], ['a', 2]]`
- C) `[['b', 1], ['a', 2], ['b', 3]]`
- D) `[['b', 1], ['a', 2]]`

<details><summary>Answer</summary>

**B.** A repeated `set` updates the value but not the key's position.
→ [Internals](expert.en.md#internals)

</details>

## 13. 🔴 Open question

When do you choose a `Map` and when a plain object?

<details><summary>Key points of a good answer</summary>

- `Map` — arbitrary keys (objects, `NaN`), insertion order, `size`, frequent additions and removals, no collisions with the prototype.
- An object — a fixed set of string fields, JSON, destructuring.
- An object as a dictionary for external keys is dangerous (`__proto__`); alternatives — `Object.create(null)` or `Map`.

</details>

→ [Advanced](advanced.en.md#map-or-a-plain-object)
