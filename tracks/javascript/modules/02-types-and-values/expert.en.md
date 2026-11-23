---
title: "Types and values: expert"
lang: en
track: javascript
kind: module
doc: expert
slug: 02-types-and-values
part: 1
tags: [type-coercion, abstract-equality, toprimitive, ieee-754, utf-16, bigint, nan, symbol]
versions: "ECMAScript 2025+, Node.js 24.15 LTS"
status: draft
updated: 2026-11-23
---

# Types and values: expert

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · **3. Expert** · [4. Quiz](quiz.en.md)

Builds on [basic](basics.en.md) and [advanced](advanced.en.md) theory. All results were obtained by running on Node.js 24.15 (an ES module, strict mode).

## Reference

### Language types and `typeof`

| Value | `typeof` | Note |
|---|---|---|
| `42`, `NaN`, `Infinity` | `'number'` | one type for integers and fractions |
| `'a'` | `'string'` | an immutable sequence of 16-bit code units |
| `true` | `'boolean'` | |
| `undefined` | `'undefined'` | also for an undeclared name when `typeof` is applied |
| `null` | `'object'` | a historical quirk |
| `10n` | `'bigint'` | |
| `Symbol('s')` | `'symbol'` | |
| `{}`, `[]`, `new Date()` | `'object'` | |
| a function, a class | `'function'` | functions are objects, but `typeof` singles them out |

### Conversion to a number (`Number(x)`, unary `+`)

| Input | Result |
|---|---|
| `'  12  '`, `'1e3'`, `'0x1f'`, `'0b11'` | `12`, `1000`, `31`, `3` |
| `'1_000'`, `'12px'`, `'abc'` | `NaN` |
| `''`, `'  '`, `'\n'`, `null`, `[]`, `false` | `0` |
| `undefined`, `{}` | `NaN` |
| `[5]`, `true` | `5`, `1` |
| `'Infinity'` | `Infinity` |

### Conversion to a string

| Input | `String(x)` |
|---|---|
| `null`, `undefined` | `'null'`, `'undefined'` |
| `[1, [2, 3]]` | `'1,2,3'` |
| `{}` | `'[object Object]'` |
| `-0`, `1e21`, `0.000001`, `1e-7` | `'0'`, `'1e+21'`, `'0.000001'`, `'1e-7'` |
| `Symbol('a')` | `'Symbol(a)'` — but `Symbol('a') + ''` and the template `${Symbol('a')}` raise a `TypeError` |

### Falsy values

`false`, `0`, `-0`, `0n`, `''`, `null`, `undefined`, `NaN` — exactly eight.

### Number checks

| Function | What it checks |
|---|---|
| `Number.isNaN(x)` | the value is exactly `NaN` (no coercion) |
| `Number.isInteger(x)` | an integer (`5.0` — yes) |
| `Number.isSafeInteger(x)` | an integer in the safe range (`2 ** 53` — no) |
| `Number.isFinite(x)` | a finite number without coercion (`'5'` — no, the global `isFinite('5')` — yes) |
| `Number.EPSILON` | a tolerance for comparing fractions |
| `Number.MIN_VALUE`, `Number.MAX_VALUE` | `5e-324` and `1.7976931348623157e+308`; `2 ** 1024` is `Infinity` |

## Internals

### Numbers: IEEE 754

`number` is a 64-bit binary floating-point number. Hence:

- `(0.1).toFixed(20)` is `'0.10000000000000000555'`: the decimal fraction `0.1` is a repeating fraction in binary, an approximation is stored;
- `2 ** 53 === 2 ** 53 + 1` is true: above 2⁵³ − 1 not all integers are representable;
- `Math.round(-0.5)` gives `-0`, `Math.round(2.5)` gives `3`, `Math.round(-2.5)` gives `-2` (rounding "toward +∞" on halves);
- `(1.005).toFixed(2)` is `'1.00'`: the number is stored as `1.00499999…`; `(2.5).toFixed(0)` is `'3'`, `(1.45).toFixed(1)` is `'1.4'`.

That is why money is stored in integer minimal units, and `toFixed` and `Math.round` do not replace rounding rules.

### Strings: UTF-16

Indexes and `length` count 16-bit code units:

```text
'😀'.length = 2 ; '😀'.codePointAt(0) = 128512 ; '😀'.charCodeAt(0) = 55357
[...'👨‍👩‍👧'].length = 5 ; 'é'.normalize('NFD').length = 2 ; 'é'.length = 1
```

An emoji is a "surrogate pair" of two units; a family emoji is assembled from three emoji and two invisible joiners (`[...]` gives 5). One character on screen does not equal one element by `length` or by `[...s]`: you need `Intl.Segmenter` (not run in the experiments).

### The loose equality algorithm

`x == y` per the specification (simplified):

1. If the types match — like `===`.
2. `null == undefined` is true; `null` and `undefined` are not equal to any other values (`null == 0` is false, `null == false` is false).
3. `number` and `string` — the string is converted to a number.
4. A `boolean` is converted to a number, then the comparison repeats (`'0' == false` → `'0' == 0` → `0 == 0`).
5. An object and a primitive — the object is converted to a primitive (`ToPrimitive`), then the comparison repeats (`[] == ''`, `[[]] == 0`, `[1,2] == '1,2'` are true).
6. `bigint` is compared with `number` by mathematical value (`2n == 2` is true).

### ToPrimitive: `valueOf` and `toString`

When an object is converted to a primitive, the methods `valueOf` and `toString` are called in an order that depends on a "hint":

```js
const o = { valueOf() { return 42 }, toString() { return 'str' } };
console.log(o + 1, `${o}`, o * 2, o == 42, String(o), [o] + '');
```

```text
43 str 84 true str str
```

- The `number` hint (arithmetic, `==`): `valueOf` first, then `toString`.
- The `string` hint (`${}`, `String()`): `toString` first.
- For `+` and `==` the default hint acts like `number` — but for `Date` it is `string`: `typeof (new Date(0) + 1)` is `'string'`, while `typeof (new Date(0) - 1)` is `'number'`.
- `Symbol.toPrimitive` can override the rules (not run).

## Gotchas

### 1. `typeof null === 'object'`

`null` is a primitive. The "is an object" check: `x !== null && typeof x === 'object'`.

### 2. `NaN`

`NaN !== NaN`; `Number.isNaN` does not coerce, the global `isNaN('abc')` returns `true`. `[NaN].includes(NaN)` is true, while `[NaN].indexOf(NaN)` gives `-1` (different comparison algorithms).

### 3. The default `sort` sorts strings

`[10, 9, 1].sort()` gives `[1, 10, 9]`; a numeric order needs `(a, b) => a - b`.

### 4. `parseInt` reads a prefix

`parseInt('0x1f')` is `31`, `parseInt('08')` is `8`, `parseInt(null)` is `NaN` (`'null'` is not a number), `parseInt('Infinity')` is `NaN`, whereas `Number('Infinity')` is `Infinity`.

### 5. Chained comparisons

`1 < 2 < 3` is true, while `3 > 2 > 1` is false: `3 > 2` is `true`, then `true > 1` is `1 > 1`.

### 6. `[] + {}` and `{} + []`

`[] + {}` gives `'[object Object]'`. The form `{} + []` depends on context: at the start of a statement (for example in `eval('{} + []')`) `{}` is an empty code block, `+[]` remains, and the result is `0`; in parentheses `({}) + []` is an object, and the result is `'[object Object]'` (in the experiment).

### 7. Wrappers and `instanceof`

`new String('x') === 'x'` is false, `new String('x') == 'x'` is true; `Boolean(new Boolean(false))` is `true`.

### 8. A date and `+`

`new Date(0) + 1` concatenates strings, `new Date(0) - 1` subtracts numbers; `new Date(NaN)` prints as `Invalid Date`, and `getTime()` gives `NaN`.

### 9. Empty array slots

`[, 1]` has length 2, but the array has no index `0` (`0 in [, 1]` is false); `[null, undefined, 1].join('-')` gives `'--1'`.

### 10. BigInt and mixed arithmetic

`BigInt(1.5)` is a `RangeError`; `Math.max(1n, 2n)` is `TypeError: Cannot convert a BigInt value to a number`; the comparisons `5n > 4` and `2n == 2` work, while `2n === 2` does not. `structuredClone` can copy `BigInt` and `Date` (in the experiment the result was `{ a: 1n, d: 1970-01-01T00:00:00.000Z }`).

## Trade-offs and alternatives

| Approach | Pros | Cons |
|---|---|---|
| Implicit coercion | short code | surprising results |
| Explicit `Number()`, `String()`, `Boolean()` | predictable | verbose |
| `===` everywhere | no hidden coercion | you must write `== null` deliberately |
| `BigInt` | exact large integers | does not mix with `number`, absent from `JSON` and `Math` |
| Integer minimal units for money | exactness | manual conversions on output |
| Static typing ([TypeScript](../../../typescript/overview.en.md)) | errors are visible before running | types are erased, boundaries need checking |

## Interview questions

<details><summary>1. What data types does JavaScript have?</summary>

Seven primitives (`number`, `string`, `boolean`, `undefined`, `null`, `bigint`, `symbol`) and objects.

→ [Basic theory](basics.en.md#types-and-typeof)

</details>

<details><summary>2. Why is <code>typeof null</code> equal to <code>'object'</code>?</summary>

A historical quirk of the language kept for compatibility; `null` is a primitive.

→ [Gotcha 1](#1-typeof-null--object)

</details>

<details><summary>3. Why is <code>0.1 + 0.2 !== 0.3</code>?</summary>

Numbers are stored as binary IEEE 754 fractions, and `0.1` and `0.2` are approximations. Compare with a tolerance or compute in integers.

→ [Basic theory](basics.en.md#numbers)

</details>

<details><summary>4. How do you check for <code>NaN</code> correctly?</summary>

`Number.isNaN(x)` or `Object.is(x, NaN)`; `x === NaN` is always false.

→ [Basic theory](basics.en.md#nan)

</details>

<details><summary>5. What is the difference between <code>==</code> and <code>===</code>?</summary>

`===` compares without coercion; `==` coerces types by a set of rules and gives surprising results (`[] == false`, `0 == ''`).

→ [The loose equality algorithm](#the-loose-equality-algorithm)

</details>

<details><summary>6. Trick: what does <code>[] == ![]</code> return and why?</summary>

`true`: `![]` is `false`, then the comparison `[] == false` converts both operands to `0`.

→ [Advanced theory](advanced.en.md#coercion-when--surprises)

</details>

<details><summary>7. List the falsy values.</summary>

`false`, `0`, `-0`, `0n`, `''`, `null`, `undefined`, `NaN`.

→ [Reference](#falsy-values)

</details>

<details><summary>8. How does <code>??</code> differ from <code>||</code>?</summary>

`??` substitutes a value only for `null` and `undefined`, `||` for any falsy value (`0`, `''`).

→ [Basic theory](basics.en.md#--and-)

</details>

<details><summary>9. Trick: what does <code>[10, 9, 1].sort()</code> print?</summary>

`[1, 10, 9]`: by default the sort is lexicographic, by the string representation.

→ [Gotcha 3](#3-the-default-sort-sorts-strings)

</details>

<details><summary>10. How are arguments passed to a function: by value or by reference?</summary>

Always by value, but for objects the value is a reference: a field can be changed, while reassigning the parameter is not visible outside.

→ [Basic theory](basics.en.md#values-and-references)

</details>

<details><summary>11. Why is <code>BigInt</code> needed and what are its limitations?</summary>

Exact integers of arbitrary length. It cannot be mixed with `number`, and has no `JSON.stringify` or `Math.max` support.

→ [Advanced theory](advanced.en.md#bigint)

</details>

<details><summary>12. Trick: why is <code>'😀'.length</code> equal to 2?</summary>

Strings are 16-bit UTF-16 code units; an emoji is a surrogate pair.

→ [Internals](#strings-utf-16)

</details>

<details><summary>13. In what order are <code>valueOf</code> and <code>toString</code> called?</summary>

In arithmetic `valueOf` first, in a string context (`${}`, `String()`) `toString` first.

→ [ToPrimitive](#toprimitive-valueof-and-tostring)

</details>

## Cheat sheet

```text
typeof x ; Array.isArray(x) ; x === null
Number.isNaN(x) ; Number.isInteger(x) ; Number.isSafeInteger(x) ; Object.is(a, b)
a === b ; a ?? b ; a?.b ; x == null
Number(x) ; String(x) ; Boolean(x) ; BigInt(x)
```

- Falsy: `false 0 -0 0n '' null undefined NaN`.
- `+` concatenates when a string is present; `-`, `*` convert to a number.
- Objects are compared by reference.
- A string is UTF-16; `length` ≠ the number of characters.

## Related materials

- Previous level: [advanced theory](advanced.en.md)
- Next module: [03 · Variables and scopes](../03-scopes-and-hoisting/overview.en.md)
- Static typing: [TypeScript](../../../typescript/overview.en.md)

## Sources

- [ECMAScript: Type conversion](https://tc39.es/ecma262/#sec-type-conversion)
- [ECMAScript: IsLooselyEqual](https://tc39.es/ecma262/#sec-islooselyequal)
- [MDN: Number](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Number)
- [MDN: String (UTF-16)](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/String#utf-16_characters_unicode_code_points_and_grapheme_clusters)
- [MDN: Symbol.toPrimitive](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Symbol/toPrimitive)
