---
title: "Objects and prototypes: basics"
lang: en
track: javascript
kind: module
doc: basics
slug: 05-objects-and-prototypes
part: 1
tags: [objects, literals, property-access, destructuring, spread, object-keys, prototype, object-create]
versions: "ECMAScript 2025+, Node.js 24.15 LTS"
status: draft
updated: 2026-11-23
---

# Objects and prototypes: basics

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [02 · Types and values](../02-types-and-values/overview.en.md), [04 · Functions](../04-functions/overview.en.md) · **Time:** ~2.5 h

All examples were run on Node.js 24.15 in an ES module file (strict mode).

## Why it matters

Almost everything in JavaScript except primitives is an object: arrays, functions, dates, settings, server responses. The ability to create, read, copy and take objects apart is needed in every program.

## Literals

```js
const key = 'dyn';
const o = {
  a: 1,
  'b-c': 2,
  [key + '1']: 3,
  f() { return 1 },
  get g() { return 'getter' },
  set s(v) { this._s = v },
};
o.s = 9;
console.log(o['b-c'], o.g, Object.keys(o));
const x = 1, y = 2;
console.log({ x, y });
```

```text
2 getter [ 'a', 'b-c', 'dyn1', 'f', 'g', 's', '_s' ]
{ x: 1, y: 2 }
```

- Keys that are not a valid name (`'b-c'`) are written in quotes and read with `o['b-c']`.
- `[key + '1']` is a **computed key**: the expression in brackets gives the property name (`dyn1`).
- `{ x, y }` is shorthand for `{ x: x, y: y }`; `f() {}` is shorthand for a method.
- `get`/`set` define accessor properties. The assignment `o.s = 9` called the setter, which created the field `_s`; `g` and `s` are printed as `[Getter]` and `[Setter]` in `console.log(o)`.

## Access, checking and deletion

```js
const o = { a: 1 };
console.log('a' in o, 'toString' in o, Object.hasOwn(o, 'toString'), o.nope, delete o.a, 'a' in o);
```

```text
true true false undefined true false
```

- Reading a nonexistent property returns `undefined` (not an error).
- `'toString' in o` is `true`: the `in` operator looks at the prototype chain too; `Object.hasOwn(o, k)` checks only own properties.
- `delete o.a` removes the property and returns `true`.
- Reading a property of `undefined` or `null` is an error; the safe way is `o?.a?.b` ([module 02](../02-types-and-values/basics.en.md#--and-)).

## Keys and their order

```js
console.log(Object.keys({ b: 1, 2: 1, a: 1, 1: 1, [Symbol('s')]: 1 }));
const o = {}; o[1] = 'a'; o['1'] = 'b'; o[{}] = 'c'; o[[1, 2]] = 'd';
console.log(Object.keys(o), o[1]);
```

```text
[ '1', '2', 'b', 'a' ]
[ '1', '[object Object]', '1,2' ] b
```

- Keys are always strings (or symbols): `o[1]` and `o['1']` are one property; an object used as a key becomes `'[object Object]'`, an array becomes `'1,2'`.
- The enumeration order: first "index" keys (`'1'`, `'2'`) ascending, then the other strings in insertion order; symbols do not appear in `Object.keys`.

## Enumeration and conversions

```js
console.log(Object.entries({ a: 1, b: 2 }), Object.fromEntries([['a', 1], ['b', 2]]));
console.log(Object.values({ a: 1, b: 2 }), Object.keys('ab'), Object.entries([7, 8]));
console.log(Object.groupBy([1, 2, 3, 4], n => (n % 2 ? 'odd' : 'even')));
```

```text
[ [ 'a', 1 ], [ 'b', 2 ] ] { a: 1, b: 2 }
[ 1, 2 ] [ '0', '1' ] [ [ '0', 7 ], [ '1', 8 ] ]
[Object: null prototype] { odd: [ 1, 3 ], even: [ 2, 4 ] }
```

- `Object.keys`, `Object.values`, `Object.entries` return arrays of own enumerable properties; `Object.fromEntries` builds an object from pairs (the inverse of `entries`).
- For strings and arrays the keys are indexes (`'0'`, `'1'`).
- `Object.groupBy` groups elements by a key; the result is a prototype-less object (`[Object: null prototype]`).

## Copying: shallow

```js
const orig = { n: { v: 1 }, k: 1 };
const sp = { ...orig, k: 2 };
sp.n.v = 99;
console.log(orig.n.v, orig.k, sp.k);
console.log(Object.assign({}, { a: 1 }, { a: 2, b: 3 }), { ...null, ...undefined, ...'ab', ...[9] });
```

```text
99 1 2
{ a: 2, b: 3 } { '0': 9, '1': 'b' }
```

- `{ ...orig }` and `Object.assign({}, orig)` copy own enumerable properties **one level deep**: the nested object `n` stayed shared (`orig.n.v` became `99`).
- Later sources overwrite earlier ones (`a: 2`).
- `...null` and `...undefined` are ignored; a string and an array expand into indexes.

Deep copies are in the [advanced theory](advanced.en.md#deep-copying).

## Destructuring

```js
const { a: aa, b: { c: cc = 5 } = {}, ...restObj } = { a: 1, d: 4, e: 5 };
console.log(aa, cc, restObj);
const [p0, , p2 = 'dflt', ...pr] = [1, 2, undefined, 4, 5];
console.log(p0, p2, pr);
function fn({ x = 1, y } = {}) { return [x, y]; }
console.log(fn(), fn({ y: 2 }));
let sa = 1, sb = 2; [sa, sb] = [sb, sa];
console.log(sa, sb);
try { const { z } = null; } catch (e) { console.log(e.name + ': ' + e.message); }
```

```text
1 5 { d: 4, e: 5 }
1 dflt [ 4, 5 ]
[ 1, undefined ] [ 1, 2 ]
2 1
TypeError: Cannot destructure property 'z' of 'null' as it is null.
```

- `{ a: aa }` reads the field `a` into the variable `aa`; the nested pattern `b: { c: cc = 5 } = {}` takes `c` and substitutes defaults when there is no value.
- `...restObj` collects the remaining fields into a new object; `...pr` — the rest of the array.
- To skip an element use a comma (`[p0, , p2]`); a default value triggers on `undefined`.
- A parameter can be destructured right in the signature; `= {}` makes the argument optional.
- Swapping variables: `[sa, sb] = [sb, sa]`.
- Destructuring `null`/`undefined` is a `TypeError`.

## Prototypes: the basics

Every object has an internal reference to a prototype. A property is looked up on the object, then on the prototype, then on the prototype's prototype — up to `null`:

```js
const proto = { hello() { return 'hi ' + this.name } };
const child = Object.create(proto);
child.name = 'c';
console.log(child.hello(), Object.getPrototypeOf(child) === proto, proto.isPrototypeOf(child));
console.log(Object.keys(child), child.hasOwnProperty('hello'));
const fi = Object.create({ inherited: 1 }); fi.own = 2;
const keys = []; for (const k in fi) keys.push(k);
console.log(keys, Object.keys(fi));
```

```text
hi c true true
[ 'name' ] false
[ 'own', 'inherited' ] [ 'own' ]
```

- `Object.create(proto)` creates an object with a given prototype; `hello` is found on the prototype but is not an own property of `child`.
- `for...in` lists inherited enumerable properties too, `Object.keys` only own ones. You usually need `Object.keys`/`entries`.
- Writing always creates an own property without changing the prototype:

```js
const base = { v: 1 }; const der = Object.create(base);
der.v = 2; console.log(base.v, der.v);
delete der.v; console.log(der.v);
```

```text
1 2
1
```

After `delete der.v` the own property is gone, and the prototype's `v` is visible again. Details and pitfalls are in the [advanced theory](advanced.en.md#prototype-traps).

## Beginner mistakes

- ❌ `{ ...o }` as a deep copy → ✅ `structuredClone(o)`.
- ❌ `'x' in o` as a check for an own field → ✅ `Object.hasOwn(o, 'x')`.
- ❌ Reading a nested field of `undefined` → ✅ `o?.a?.b`.
- ❌ Using an object as an object key → ✅ `Map`.
- ❌ Destructuring a value that may be `null` without a default → ✅ `= {}`.
- ❌ Expecting insertion order for numeric keys → ✅ numeric keys go in ascending order.

## Related materials

- Next: [advanced theory](advanced.en.md) — descriptors, `freeze`, deep copying, prototype traps
- Earlier: [04 · Functions](../04-functions/overview.en.md) (constructors and `new`)
- Classes on top of prototypes: module 06 of the [track](../../overview.en.md) roadmap

## Sources

- [MDN: Working with objects](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Guide/Working_with_objects)
- [MDN: Inheritance and the prototype chain](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Guide/Inheritance_and_the_prototype_chain)
- [MDN: Destructuring](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Operators/Destructuring)
- [MDN: Spread syntax](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Operators/Spread_syntax)
- [MDN: Object](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Object)
