---
title: "Quiz: types and values"
lang: en
track: javascript
kind: module
doc: quiz
slug: 02-types-and-values
part: 1
tags: [types, typeof, coercion, equality, nan, nullish-coalescing]
status: draft
updated: 2026-11-23
---

# Quiz: types and values

🌐 [Русский](quiz.ru.md) · **English**

> Answer first, then open the answer. The mark shows which level a question comes from: 🟢 [basics](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 What does `typeof null` print?

- A) `'null'`
- B) `'undefined'`
- C) `'object'`
- D) `'number'`

<details><summary>Answer</summary>

**C.** A historical quirk: `null` is a primitive, but `typeof` returns `'object'`.
→ [Basics](basics.en.md#types-and-typeof)

</details>

## 2. 🟢 What is `0.1 + 0.2 === 0.3`?

- A) `true`
- B) `false`
- C) `NaN`
- D) An error

<details><summary>Answer</summary>

**B.** `0.1 + 0.2` is `0.30000000000000004`: binary fractions are approximate. Compare with a tolerance (`Number.EPSILON`).
→ [Basics](basics.en.md#numbers)

</details>

## 3. 🟢 How do you correctly check that `x` is `NaN`?

- A) `x === NaN`
- B) `x == NaN`
- C) `Number.isNaN(x)`
- D) `typeof x === 'NaN'`

<details><summary>Answer</summary>

**C.** `NaN` is not equal even to itself, so A and B are always false.
→ [Basics](basics.en.md#nan)

</details>

## 4. 🟢 What do `'5' + 3` and `'5' - 3` return?

- A) `8` and `2`
- B) `'53'` and `2`
- C) `'53'` and `'53'`
- D) `8` and `'2'`

<details><summary>Answer</summary>

**B.** `+` concatenates when a string is present, `-` converts the operands to numbers.
→ [Basics](basics.en.md#coercion-and-comparison)

</details>

## 5. 🟢 What value does `cfg.port ?? 3000` give if `cfg.port` is `0`?

- A) `3000`
- B) `0`
- C) `undefined`
- D) An error

<details><summary>Answer</summary>

**B.** `??` reacts only to `null` and `undefined`. `cfg.port || 3000` would give `3000`.
→ [Basics](basics.en.md#--and-)

</details>

## 6. 🟡 What do `1n === 1` and `1n == 1` return?

- A) `true` and `true`
- B) `false` and `true`
- C) `true` and `false`
- D) A `TypeError`

<details><summary>Answer</summary>

**B.** The types differ (`bigint` and `number`), so strict comparison is false; loose comparison compares by value.
→ [Advanced](advanced.en.md#bigint)

</details>

## 7. 🟡 What does `Boolean(new Boolean(false))` return?

- A) `false`
- B) `true`
- C) `undefined`
- D) An error

<details><summary>Answer</summary>

**B.** `new Boolean(false)` is an object, and any object is truthy.
→ [Advanced](advanced.en.md#wrapper-objects)

</details>

## 8. 🟡 What does `JSON.stringify({ a: undefined, c: NaN, e: [undefined] })` do?

- A) `{"a":null,"c":null,"e":[null]}`
- B) `{"c":null,"e":[null]}`
- C) `{"a":"undefined","c":"NaN","e":["undefined"]}`
- D) An error

<details><summary>Answer</summary>

**B.** A property with `undefined` is skipped, `NaN` becomes `null`, and `undefined` in an array becomes `null`.
→ [Advanced](advanced.en.md#json-and-types)

</details>

## 9. 🟡 Select all falsy values

- [ ] A) `0n`
- [ ] B) `'0'`
- [ ] C) `[]`
- [ ] D) `NaN`

<details><summary>Answer</summary>

**A, D.** `'0'`, `[]` and `{}` are truthy. There are exactly eight falsy values.
→ [Basics](basics.en.md#truthiness-truthy-and-falsy)

</details>

## 10. 🟡 The function `mutate(obj) { obj.n = 99; obj = { n: 0 }; }` is called with `o1 = { n: 1 }`. What is `o1.n`?

- A) `1`
- B) `0`
- C) `99`
- D) `undefined`

<details><summary>Answer</summary>

**C.** The parameter received a copy of the reference: a field change is visible outside, a reassignment of the parameter is not.
→ [Basics](basics.en.md#values-and-references)

</details>

## 11. 🔴 Find the flaw

> "To check that a value is an object, `typeof x === 'object'` is enough."

<details><summary>Answer</summary>

The condition is also true for `null`, arrays and dates. You need `x !== null && typeof x === 'object'`, and `Array.isArray(x)` for arrays.
→ [Gotcha 1](expert.en.md#1-typeof-null--object)

</details>

## 12. 🔴 What does `[10, 9, 1].sort()` print?

- A) `[1, 9, 10]`
- B) `[1, 10, 9]`
- C) `[10, 9, 1]`
- D) An error

<details><summary>Answer</summary>

**B.** By default `sort` compares string representations. A numeric order: `sort((a, b) => a - b)`.
→ [Gotcha 3](expert.en.md#3-the-default-sort-sorts-strings)

</details>

## 13. 🔴 Open question

Explain why `===` is recommended, and name the one sensible use of `==`.

<details><summary>Key points of a good answer</summary>

- `==` coerces types by complex rules (`[] == false`, `0 == ''`, `'0' == false`), and the results are non-obvious.
- `===` compares without coercion, so it is predictable.
- A justified case: `x == null` — checking for `null` and `undefined` at once.

</details>

→ [Expert](expert.en.md#the-loose-equality-algorithm)
