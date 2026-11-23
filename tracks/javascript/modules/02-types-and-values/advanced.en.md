---
title: "Types and values: advanced"
lang: en
track: javascript
kind: module
doc: advanced
slug: 02-types-and-values
part: 1
tags: [bigint, symbol, wrapper-objects, object-is, coercion, json, typeof]
versions: "ECMAScript 2025+, Node.js 24.15 LTS"
status: draft
updated: 2026-11-23
---

# Types and values: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

Builds on [basic theory](basics.en.md). All examples were run on Node.js 24.15 in an ES module file.

## BigInt

`BigInt` stores integers of arbitrary length; a literal is written with the suffix `n`:

```js
console.log(2n ** 64n, 10n / 3n, typeof 1n, 1n === 1, 1n == 1, BigInt(Number.MAX_SAFE_INTEGER) + 2n);
try { 1n + 1 } catch (e) { console.log(e.name + ': ' + e.message) }
```

```text
18446744073709551616n 3n bigint false true 9007199254740993n
TypeError: Cannot mix BigInt and other types, use explicit conversions
```

- Division is integer division: `10n / 3n` is `3n`.
- `1n === 1` is false (different types), `1n == 1` is true.
- You cannot mix `BigInt` and `number` in arithmetic: you need an explicit `BigInt(x)` or `Number(x)`.
- A `BigInt` beyond the safe integer range gives exact values (`…993n`), unlike `number`.

## Symbol

`Symbol` is a unique value, usually used as a property key:

```js
const a = Symbol('id'), b = Symbol('id');
console.log(a === b, a.toString(), a.description, Symbol.for('k') === Symbol.for('k'));
```

```text
false Symbol(id) id true
```

Every call of `Symbol('id')` creates a new value; the description is only for debugging. `Symbol.for('k')` returns a shared symbol from the global registry. The use of symbols (keys that do not collide with other people's, and "well-known symbols" such as `Symbol.iterator`) is covered in modules 05 and 11 of the roadmap.

## Wrapper objects

Primitives have methods because the language temporarily wraps them in objects (`'x'.toUpperCase()`). Wrappers created explicitly are a source of confusion:

```js
console.log(typeof new String('x'), new String('x') === 'x', new String('x') == 'x');
console.log(typeof Object(1), Object(1) instanceof Number, (5).toFixed(2), 'x'.constructor.name);
console.log(Boolean(new Boolean(false)));
```

```text
object false true
object true 5.00 String
true
```

`new Boolean(false)` is an object, and any object is truthy, so `Boolean(new Boolean(false))` is `true`. Do not create wrappers with `new`; the functions `String(x)`, `Number(x)`, `Boolean(x)` without `new` merely convert a value.

## Object.is and zeros

`Object.is` compares by the "SameValue" algorithm: it differs from `===` only for `NaN` and zeros:

```js
console.log(Object.is(-0, 0), 0 === -0, Object.is(NaN, NaN), NaN === NaN);
```

```text
false true true false
```

`-0` is a separate value: `0 === -0` is true but `Object.is(-0, 0)` is false.

## Coercion: when `==` surprises

```js
console.log([] == false, [0] == false, [1] == true, '' == 0, '0' == false, null == false, NaN == NaN, [] == ![]);
```

```text
true true true true true false false true
```

- An object compared with a primitive is converted to a string or a number: `[] == false`: the array gives `''`, then `0 == 0`.
- `[] == ![]` is true: `![]` is `false`, then the left side is converted the same way.
- `null == false` is false: with `==` `null` equals only `null` and `undefined`.

These results show why `==` should be avoided. The full algorithm is in the [expert theory](expert.en.md#the-loose-equality-algorithm).

## typeof of an undeclared variable

```js
console.log(typeof notDeclared);
try { notDeclared } catch (e) { console.log(e.name + ': ' + e.message) }
```

```text
undefined
ReferenceError: notDeclared is not defined
```

`typeof` is the only operator that safely touches an undeclared name. Reading the variable directly raises a `ReferenceError` (unlike the `undefined` of a declared but uninitialized one, see [module 03](../03-scopes-and-hoisting/overview.en.md)).

## JSON and types

`JSON.stringify` cannot handle some values:

```js
console.log(JSON.stringify({ a: undefined, b: () => 1, c: NaN, d: new Date(0), e: [undefined] }));
try { JSON.stringify({ x: 1n }) } catch (e) { console.log(e.name + ': ' + e.message) }
```

```text
{"c":null,"d":"1970-01-01T00:00:00.000Z","e":[null]}
TypeError: Do not know how to serialize a BigInt
```

- Properties with `undefined`, functions and symbols are skipped; in an array they become `null`.
- `NaN` becomes `null`.
- A date becomes an ISO 8601 string.
- `BigInt` causes an error: convert it by hand.

## Best practices

- Compare only with `===`, and do the emptiness check as `x == null`.
- `Number.isNaN`, `Number.isInteger`, `Number.isSafeInteger` instead of the global analogues.
- Money and identifiers above 2⁵³: store as integer minimal units or as `BigInt`/a string.
- Defaults through `??`, not `||`, when `0` or `''` are valid.
- Do not create wrappers `new String/Number/Boolean`.

> ⚠️ Verify: `Intl.NumberFormat`, `Number.prototype.toFixed` for rounding money and `Decimal` as a standard proposal were not run.

## Common mistakes

- ❌ Mixing `BigInt` and `number` → ✅ explicit conversions.
- ❌ Serializing objects with `BigInt` into JSON → ✅ convert to a string beforehand.
- ❌ `new Boolean(false)` in a condition → ✅ primitives.
- ❌ Expecting `JSON.stringify` to keep `undefined` and functions → ✅ they are lost.
- ❌ Considering `-0` and `0` indistinguishable everywhere → ✅ `Object.is` tells them apart.

## Related materials

- Next: [expert theory](expert.en.md)
- Strict mode and assignment errors: [01 · Environment and tools](../01-environment/advanced.en.md#strict-mode)
- Scopes and `ReferenceError`: [03 · Variables and scopes](../03-scopes-and-hoisting/overview.en.md)

## Sources

- [MDN: BigInt](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/BigInt)
- [MDN: Symbol](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Symbol)
- [MDN: Object.is()](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Object/is)
- [MDN: JSON.stringify()](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/JSON/stringify)
- [MDN: Equality comparisons and sameness](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Guide/Equality_comparisons_and_sameness)
