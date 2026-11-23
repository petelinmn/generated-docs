---
title: "Objects and prototypes: advanced"
lang: en
track: javascript
kind: module
doc: advanced
slug: 05-objects-and-prototypes
part: 1
tags: [property-descriptors, define-property, freeze, seal, structured-clone, deep-copy, null-prototype, symbols, prototype-pitfalls]
versions: "ECMAScript 2025+, Node.js 24.15 LTS"
status: draft
updated: 2026-11-23
---

# Objects and prototypes: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

Builds on [basic theory](basics.en.md). All examples were run on Node.js 24.15 in an ES module file (strict mode).

## Property descriptors

Every property has attributes. For an ordinary property they are all `true`; an accessor has `get` and `set` instead of `value` and `writable`:

```js
console.log(Object.getOwnPropertyDescriptor({ a: 1 }, 'a'));
console.log(Object.getOwnPropertyDescriptor({ get g() { return 1 } }, 'g'));
const d = {};
Object.defineProperty(d, 'ro', { value: 1, enumerable: false });
console.log(Object.getOwnPropertyDescriptor(d, 'ro'), Object.keys(d), JSON.stringify(d));
try { d.ro = 2; } catch (e) { console.log(e.name + ': ' + e.message); }
```

```text
{ value: 1, writable: true, enumerable: true, configurable: true }
{ get: [Function: get g], set: undefined, enumerable: true, configurable: true }
{ value: 1, writable: false, enumerable: false, configurable: false } [] {}
TypeError: Cannot assign to read only property 'ro' of object '#<Object>'
```

- `writable` — can the value be changed; `enumerable` — is it visible in `keys`, `for...in`, `JSON`; `configurable` — can the property be deleted or redefined.
- `Object.defineProperty` creates `false` attributes by default: the property `ro` is not writable, not enumerable and not configurable.
- Hidden (non-enumerable) properties do not appear in `Object.keys` or `JSON.stringify`.

## freeze, seal and preventExtensions

Three levels of protecting an object; in strict mode a violation is a `TypeError`:

```js
const fz = Object.freeze({ a: 1 });
const sl = Object.seal({ a: 1 });
const pe = Object.preventExtensions({ a: 1 });
const t = (f) => { try { f() } catch (e) { console.log(e.name + ': ' + e.message) } };
t(() => { fz.a = 2 }); t(() => { sl.b = 1 }); sl.a = 5; t(() => { delete sl.a }); t(() => { pe.b = 1 }); delete pe.a;
console.log(fz, sl, pe, Object.isFrozen(fz), Object.isSealed(sl), Object.isExtensible(pe));
```

```text
TypeError: Cannot assign to read only property 'a' of object '#<Object>'
TypeError: Cannot add property b, object is not extensible
TypeError: Cannot delete property 'a' of #<Object>
TypeError: Cannot add property b, object is not extensible
{ a: 1 } { a: 5 } {} true true false
```

| Method | Add | Delete | Change values |
|---|---|---|---|
| `preventExtensions` | no | yes | yes |
| `seal` | no | no | yes |
| `freeze` | no | no | no |

All three act on the top level: nested objects stay mutable ([module 03](../03-scopes-and-hoisting/basics.en.md#let-const-and-var)). In sloppy mode a violation is silently ignored.

## Deep copying

A shallow copy leaves nested objects shared ([basic theory](basics.en.md#copying-shallow)). For a deep copy there is `structuredClone`:

```js
const sc = structuredClone({ d: new Date(0), m: new Map([[1, 2]]), s: new Set([1]) });
console.log(sc);
const cyc = {}; cyc.self = cyc;
console.log(structuredClone(cyc).self !== undefined);
try { structuredClone({ f() {} }) } catch (e) { console.log(e.name + ': ' + e.message) }
console.log(JSON.parse(JSON.stringify({ d: new Date(0), u: undefined, m: new Map([[1, 2]]) })));
try { JSON.stringify(cyc) } catch (e) { console.log(e.name + ': ' + e.message.split('\n')[0]) }
```

```text
{ d: 1970-01-01T00:00:00.000Z, m: Map(1) { 1 => 2 }, s: Set(1) { 1 } }
true
DataCloneError: f() {} could not be cloned.
{ d: '1970-01-01T00:00:00.000Z', m: {} }
TypeError: Converting circular structure to JSON
```

- `structuredClone` copies `Date`, `Map`, `Set` and circular references, but not functions (`DataCloneError`).
- Cloning through `JSON.parse(JSON.stringify(x))` is unreliable: a date becomes a string, a `Map` becomes `{}`, `undefined` vanishes, and circular references raise an error.
- `structuredClone` does not keep prototypes and class methods: the copy of a class instance is a plain object (in the experiment `Object.getPrototypeOf(copy) === Object.prototype`, and `typeof copy.m` is `'undefined'`; private fields are not carried over either).
- It can copy `Error`, `RegExp` and typed arrays (in the experiment an `Error` stays an `Error`, and `/a/g` and a `Uint8Array` are preserved).

## Prototype traps

**Freezing a prototype blocks writes on descendants:**

```js
const base2 = Object.freeze({ ro: 1 });
const der2 = Object.create(base2);
try { der2.ro = 2 } catch (e) { console.log(e.name + ': ' + e.message) }
console.log(der2.ro);
```

```text
TypeError: Cannot assign to read only property 'ro' of object '#<Object>'
1
```

If a prototype's property is not writable, assigning on a descendant (in strict mode) also fails instead of creating an own one.

**Mutable data on a prototype is shared:**

```js
const sh = { list: [] };
const a1 = Object.create(sh), a2 = Object.create(sh);
a1.list.push(1);
console.log(a2.list);
```

```text
[ 1 ]
```

`a1.list` is `sh.list`: `push` changed the shared array. Mutable state must be created in the object itself (in a constructor).

## Prototype-less objects

`Object.create(null)` creates a "clean" object without inherited properties:

```js
const dict = Object.create(null);
dict.k = 1;
console.log('toString' in dict, dict instanceof Object, Object.getPrototypeOf(Object.prototype));
try { String(dict) } catch (e) { console.log(e.name + ': ' + e.message) }
```

```text
false false null
TypeError: Cannot convert object to primitive value
```

Such an object is safe as a dictionary (no collisions with `toString`, `__proto__`), but it lacks the `Object.prototype` methods: it cannot be turned into a string. `Object.groupBy` returns exactly such an object. For dictionaries `Map` is more often used.

## `__proto__` and `setPrototypeOf`

```js
const pr = { a: 1 };
const ch = { __proto__: pr, b: 2 };
const x = {}; Object.setPrototypeOf(x, pr);
console.log(ch.a, Object.getPrototypeOf(ch) === pr, '__proto__' in {}, Object.keys(JSON.parse('{"__proto__": 1}')), x.a);
```

```text
1 true true [ '__proto__' ] 1
```

- In a literal `{ __proto__: pr }` sets the prototype; `JSON.parse` creates an **own** property `__proto__` (safe).
- `Object.setPrototypeOf` changes the prototype of an existing object; according to MDN it is an expensive operation, so prefer `Object.create` at creation time (the slowdown was not measured in the experiments).

## Symbols as keys

```js
const sym = Symbol('hid');
const so = { [sym]: 1, vis: 2 };
console.log(Object.keys(so), JSON.stringify(so), Object.getOwnPropertySymbols(so).length, so[sym]);
```

```text
[ 'vis' ] {"vis":2} 1 1
```

Symbol keys do not appear in `keys` and `JSON` — they are used for service properties that must not collide with other people's ([module 02](../02-types-and-values/advanced.en.md#symbol)).

## Best practices

- `Object.hasOwn` instead of `hasOwnProperty`; `Object.keys`/`entries` instead of `for...in`.
- Do not extend `Object.prototype` or other people's prototypes.
- Copying: `{ ...o }` for flat objects, `structuredClone` for nested data.
- Immutable data: `Object.freeze` for top-level constants (know that it is shallow).
- Dictionaries: `Map` or `Object.create(null)`; never write keys from user input into a plain object without a check.
- Mutable state — in the instance, not in the prototype.

> ⚠️ Verify: prototype pollution (writing to `Object.prototype` through malicious keys) is covered in module 20 of the roadmap and was not run in the experiments.

## Common mistakes

- ❌ Cloning objects with dates, `Map`, `undefined` through `JSON.parse(JSON.stringify(x))` → ✅ `structuredClone`.
- ❌ Expecting `freeze` to freeze nested objects → ✅ freeze recursively or design flat data.
- ❌ Putting an array or object on a prototype → ✅ create it in the constructor.
- ❌ `String(Object.create(null))` → ✅ `Map` or a plain object.
- ❌ Assigning on a descendant a property defined on a frozen prototype → ✅ `Object.defineProperty` on the descendant.

## Related materials

- Next: [expert theory](expert.en.md)
- Constructors, `new` and `prototype`: [04 · Functions](../04-functions/advanced.en.md#constructor-functions-and-new)
- Classes: module 06 of the [track](../../overview.en.md) roadmap

## Sources

- [MDN: Object.defineProperty()](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Object/defineProperty)
- [MDN: Object.freeze()](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Object/freeze)
- [MDN: structuredClone()](https://developer.mozilla.org/en-US/docs/Web/API/Window/structuredClone)
- [MDN: Object.create()](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Object/create)
- [MDN: Object.groupBy()](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Object/groupBy)
