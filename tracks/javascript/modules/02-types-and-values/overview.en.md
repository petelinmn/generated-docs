---
title: "Types and values"
lang: en
track: javascript
kind: module
doc: overview
slug: 02-types-and-values
part: 1
levels: [1, 2, 3, 4]
tags: [types, typeof, coercion, equality, nan, bigint, symbol, null, undefined, truthy-falsy, nullish-coalescing]
versions: "ECMAScript 2025+, Node.js 24.15 LTS"
status: draft
updated: 2026-11-23
---

# Types and values

🌐 [Русский](overview.ru.md) · **English**

## In short

In JavaScript **values** have a type, while variables do not: one variable can hold a number and then a string. There are seven **primitive** types (`number`, `string`, `boolean`, `undefined`, `null`, `bigint`, `symbol`) and one compound type — the **object** (arrays, functions and dates belong to it). Primitives are immutable and copied by value, objects are passed by reference. The language readily converts types by itself (`'5' + 3` gives `'53'`, while `'5' - 3` gives `2`), so you need to know the coercion rules and always prefer the strict comparison `===`.

## Key ideas

- **`typeof` returns a string naming the type,** but with well-known exceptions: `typeof null` is `'object'`, and `typeof` of a function is `'function'`.
- **All numbers are 64-bit floating-point (IEEE 754):** `0.1 + 0.2 !== 0.3`, integers are exact only up to `Number.MAX_SAFE_INTEGER`, and there are `NaN`, `Infinity` and `-0`. For large integers there is `BigInt`.
- **`NaN` is not equal to itself;** check with `Number.isNaN`, not the global `isNaN`.
- **Strings are immutable** and consist of 16-bit code units: an emoji takes two.
- **`undefined` means "no value", `null` means "intentionally empty".**
- **`==` coerces types, `===` does not:** use `===`, and keep `== null` for checking `null` and `undefined` at once.
- **Falsy values: `false`, `0`, `-0`, `0n`, `''`, `null`, `undefined`, `NaN`.** Everything else is truthy, including `'0'`, `[]` and `{}`.
- **`??` substitutes a value only for `null`/`undefined`,** while `||` does so for any falsy value; `?.` accesses properties safely.

## Minimal example

```js
console.log(typeof 42, typeof 'a', typeof null, typeof undefined);
console.log(0.1 + 0.2, '5' + 3, '5' - 3, 0 || 3000, 0 ?? 3000);
```

```text
number string object undefined
0.30000000000000004 53 2 3000 0
```

## When to use and when not

| Task | Tool |
|---|---|
| Strict comparison | `===`, `!==` |
| Checking for `NaN` | `Number.isNaN(x)` |
| A default only for `null`/`undefined` | `x ?? value` |
| Exact arithmetic with large integers | `BigInt` |
| Money and exact decimal fractions | integer minimal units (cents) |
| A copy of an object rather than a reference | module 05 of the roadmap ([objects and prototypes](../../overview.en.md)) |

- ❌ Do not use `==` except for the check `x == null`.
- ❌ Do not treat `typeof x === 'object'` as a check for "this is an object": `null` (and arrays) get in.

## Material levels

| Level | What is inside |
|---|---|
| [1. Basic theory](basics.en.md) | types and `typeof`, numbers, strings, `null` and `undefined`, coercion and comparison, truthiness, `??` and `?.`, values and references |
| [2. Advanced theory](advanced.en.md) | `BigInt`, `Symbol`, wrapper objects, `Object.is`, non-obvious coercion, JSON, `typeof` of an undeclared variable |
| [3. Expert theory](expert.en.md) | a reference of conversions, the `==` algorithm, how numbers and strings work, pitfalls, interview questions |
| [4. Self-check](quiz.en.md) | quiz and [exercises](exercises.en.md) |
| 5. Runnable examples | not created yet — `/learn javascript 02-types-and-values level 5` |
| 6. Tests | not created yet — `/learn javascript 02-types-and-values level 6` |

## Related materials

- Prerequisites: [01 · Environment and tools](../01-environment/overview.en.md)
- Next: [03 · Variables and scopes](../03-scopes-and-hoisting/overview.en.md)
- Analogues in other tracks: [Python: types and data structures](../../../python/overview.en.md), [C#: types and variables](../../../csharp/overview.en.md)
- Static types on top of JavaScript: [TypeScript](../../../typescript/overview.en.md)
