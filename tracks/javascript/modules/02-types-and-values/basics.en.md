---
title: "Types and values: basics"
lang: en
track: javascript
kind: module
doc: basics
slug: 02-types-and-values
part: 1
tags: [types, typeof, numbers, strings, null, undefined, coercion, equality, truthy-falsy, nullish-coalescing, optional-chaining]
versions: "ECMAScript 2025+, Node.js 24.15 LTS"
status: draft
updated: 2026-11-23
---

# Types and values: basics

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [01 · Environment and tools](../01-environment/overview.en.md) · **Time:** ~2 h

All examples were run on Node.js 24.15 in an ES module file (strict mode, `"type": "module"`).

## Why it matters

Most "strange" results in JavaScript come from implicit type coercion and quirks of numbers. If you know the rules, the behavior becomes predictable, and bugs become rare.

## Types and `typeof`

The `typeof` operator returns a string naming the type:

```js
for (const v of [42, 'a', true, undefined, null, 10n, Symbol('s'), {}, [], () => {}, class A {}, NaN, new Date()])
  console.log(String(typeof v).padEnd(10), Object.prototype.toString.call(v));
```

```text
number     [object Number]
string     [object String]
boolean    [object Boolean]
undefined  [object Undefined]
object     [object Null]
bigint     [object BigInt]
symbol     [object Symbol]
object     [object Object]
object     [object Array]
function   [object Function]
function   [object Function]
number     [object Number]
object     [object Date]
```

- **Primitives:** `number`, `string`, `boolean`, `undefined`, `null`, `bigint`, `symbol`.
- **Objects:** everything else — plain objects, arrays, dates, functions.
- `typeof null` is `'object'` — a historical mistake of the language kept for compatibility: `null` is a primitive.
- `typeof NaN` is `'number'`: "not a number" still has the type number.
- An array is detected by `Array.isArray(x)`, not by `typeof`.

## Numbers

The `number` type is one for integers and fractions alike; it is a 64-bit floating-point number (IEEE 754):

```js
console.log(0.1 + 0.2, 0.1 + 0.2 === 0.3, Math.abs(0.1 + 0.2 - 0.3) < Number.EPSILON);
console.log(Number.MAX_SAFE_INTEGER, Number.MAX_SAFE_INTEGER + 1, Number.MAX_SAFE_INTEGER + 2);
console.log(1 / 0, -1 / 0, 0 / 0);
```

```text
0.30000000000000004 false true
9007199254740991 9007199254740992 9007199254740992
Infinity -Infinity NaN
```

- Fractions cannot be represented exactly in binary, hence `0.1 + 0.2 !== 0.3`. Compare with a tolerance (`Number.EPSILON`) or work with integer minimal units (cents).
- Integers are exact only up to `Number.MAX_SAFE_INTEGER` (2⁵³ − 1): above it neighboring values "stick together" (`…992 + 1` and `…992 + 2` give the same result).
- Division by zero is not an error: you get `Infinity`, `-Infinity` or `NaN`.

### NaN

`NaN` ("not a number") is the result of an undefined numeric operation. It is not equal even to itself:

```js
console.log(NaN === NaN, Object.is(NaN, NaN));
console.log(isNaN('abc'), Number.isNaN('abc'));
console.log(Number('12px'), parseInt('12px'), Number(''), Number(null), Number(undefined), Number([]), Number([5]), Number(true));
```

```text
false true
true false
NaN 12 0 0 NaN 0 5 1
```

`Number.isNaN(x)` checks that the value is exactly `NaN`. The global `isNaN('abc')` first converts the argument to a number and therefore returns `true` for any non-numeric string. Conversions: `Number('12px')` is strict (`NaN`), `parseInt('12px')` reads the start of the string (`12`), and an empty string and `null` give `0`.

## Strings

A string is an immutable sequence of 16-bit UTF-16 code units:

```js
const s = 'héllo 😀';
console.log(s.length, [...s].length, s[0], s.toUpperCase());
let t = 'abc';
try { t[0] = 'X'; } catch (e) { console.log(e.name + ': ' + e.message); }
console.log(t, `sum: ${1 + 2}`);
```

```text
8 7 h HÉLLO 😀
TypeError: Cannot assign to read only property '0' of string 'abc'
abc sum: 3
```

- `s.length` is 8 while there are 7 characters: the emoji takes two code units. `[...s]` iterates by characters (code points).
- You cannot change a character of a string; in strict mode the attempt raises a `TypeError`, in sloppy mode it silently does nothing.
- Strings in backticks are template literals: `${expression}` is substituted into the text.

## null and undefined

```js
let u;
console.log(u, typeof u, null == undefined, null === undefined, typeof null, void 0);
```

```text
undefined undefined true false object undefined
```

- `undefined` is the value "nothing has been assigned" (a declared variable without a value, a missing property, a function without `return`).
- `null` is an intentionally "empty" value that a programmer assigns.
- `null == undefined` is true, while `null === undefined` is false.

## Coercion and comparison

Operators often convert values to another type. The results (all obtained by running):

```js
console.log('5' + 3, '5' - 3, '5' * '2', 5 + null, 5 + undefined, true + true);
console.log([] + [], [] + {}, [1, 2] + [3]);
console.log('3' > '12', 3 > '12', '10' == 10, 0 == '', null == 0, null >= 0, undefined == 0);
```

```text
53 2 10 5 NaN 2
 [object Object] 1,23
true false true true false true false
```

(The second line starts with an empty string — the result of `[] + []`.)

- **`+`** concatenates when a string is present: `'5' + 3` is `'53'`. The other arithmetic operators (`-`, `*`) convert to a number.
- **Strings are compared character by character:** `'3' > '12'` is true because `'3'` comes after `'1'`; `3 > '12'` converts the string to a number.
- **`==`** coerces types by complex rules: `0 == ''` is true, `null == 0` is false, yet `null >= 0` is true.

A practical rule: **always compare with `===` and `!==`.** The only sensible use of `==` is `x == null` (true for `null` and `undefined` at once).

## Truthiness: truthy and falsy

In conditions a value is converted to a boolean. There are exactly eight falsy values:

```js
const falsy = [false, 0, -0, 0n, '', null, undefined, NaN];
console.log(falsy.every(v => !v));
console.log(Boolean('0'), Boolean('false'), Boolean([]), Boolean({}), Boolean(' '), Boolean(-1));
```

```text
true
true true true true true true
```

Everything else is truthy: a non-empty string (even `'0'` and `'false'`), any array (even an empty one), any object.

## `||`, `??` and `?.`

```js
const cfg = { port: 0, name: '', opt: null };
console.log(cfg.port || 3000, cfg.port ?? 3000, cfg.name || 'anon', cfg.name ?? 'anon', cfg.opt ?? 'dflt');
console.log(cfg.x?.y, cfg.x?.[0], cfg.fn?.());
```

```text
3000 0 anon  dflt
undefined undefined undefined
```

- `a || b` takes `b` if `a` is falsy — including for `0` and `''`. This is a frequent cause of bugs with ports and counters.
- `a ?? b` takes `b` only if `a` is `null` or `undefined`.
- `a?.b`, `a?.[i]`, `a?.()` return `undefined` if `a` is `null`/`undefined`, instead of an error.

## Values and references

Primitives are copied by value, objects by reference:

```js
let p = 1, q = p; q++;
console.log(p, q);
const o1 = { n: 1 }, o2 = o1; o2.n++;
console.log(o1.n, o1 === o2, { n: 2 } === { n: 2 });
function mutate(obj) { obj.n = 99; obj = { n: 0 }; }
mutate(o1);
console.log(o1.n);
```

```text
1 2
2 true false
99
```

- Changing `q` did not affect `p`.
- `o1` and `o2` are references to one object; two literals with identical content are different objects, so `{n:2} === {n:2}` is false.
- The function received a copy of the reference: changing a field is visible outside (`99`), while reassigning `obj = {n:0}` is not.

## Beginner mistakes

- ❌ `typeof x === 'object'` as an object check → ✅ also account for `null` and `Array.isArray`.
- ❌ `x == y` → ✅ `x === y`.
- ❌ `if (x === NaN)` → ✅ `Number.isNaN(x)`.
- ❌ `port || 3000` when `0` is valid → ✅ `port ?? 3000`.
- ❌ Comparing objects with `===` by content → ✅ compare fields or use a dedicated function.
- ❌ Adding money as fractions `0.1 + 0.2` → ✅ work with integer minimal units.

## Related materials

- Next: [advanced theory](advanced.en.md) — `BigInt`, `Symbol`, wrappers, `Object.is`, JSON
- Next module: [03 · Variables and scopes](../03-scopes-and-hoisting/overview.en.md)
- Earlier: [01 · Environment and tools](../01-environment/overview.en.md) (strict mode in ESM and in CommonJS)

## Sources

- [MDN: Data structures and types](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Guide/Data_structures)
- [MDN: Equality comparisons and sameness](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Guide/Equality_comparisons_and_sameness)
- [MDN: Nullish coalescing operator](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Operators/Nullish_coalescing)
- [ECMAScript: Types](https://tc39.es/ecma262/#sec-ecmascript-data-types-and-values)
