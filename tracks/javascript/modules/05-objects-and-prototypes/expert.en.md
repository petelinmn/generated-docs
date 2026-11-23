---
title: "Objects and prototypes: expert"
lang: en
track: javascript
kind: module
doc: expert
slug: 05-objects-and-prototypes
part: 1
tags: [object-api, own-keys-order, property-lookup, prototype-chain, internal-slots, null-prototype, descriptors]
versions: "ECMAScript 2025+, Node.js 24.15 LTS"
status: draft
updated: 2026-11-23
---

# Objects and prototypes: expert

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · **3. Expert** · [4. Quiz](quiz.en.md)

Builds on [basic](basics.en.md) and [advanced](advanced.en.md) theory. All results were obtained by running on Node.js 24.15 (an ES module, strict mode).

## Reference

### Static methods of `Object`

| Method | What it does |
|---|---|
| `Object.keys(o)`, `values(o)`, `entries(o)` | arrays of own enumerable string keys, values, pairs |
| `Object.fromEntries(iter)` | an object from pairs (in the experiment it works with a `Map` too) |
| `Object.assign(target, ...src)` | copies own enumerable properties (including symbol ones); later ones overwrite earlier ones |
| `Object.hasOwn(o, k)` | whether there is an own property |
| `Object.create(proto, descriptors?)` | an object with a given prototype (in the experiment `Object.create({...}, { own: { value: 2, enumerable: true } })`) |
| `Object.getPrototypeOf(o)`, `setPrototypeOf(o, p)` | reading and changing the prototype |
| `Object.getOwnPropertyNames(o)` | all own string keys, including non-enumerable ones (for the string `'ab'` — `'0'`, `'1'`, `'length'`) |
| `Object.getOwnPropertySymbols(o)` | own symbol keys |
| `Object.getOwnPropertyDescriptor(o, k)`, `getOwnPropertyDescriptors(o)` | descriptor(s) |
| `Object.defineProperty(o, k, d)` | create or change a property with attributes |
| `Object.freeze`, `seal`, `preventExtensions` and `isFrozen`, `isSealed`, `isExtensible` | protection levels |
| `Object.groupBy(iter, fn)` | grouping; the result has no prototype |
| `Object.is(a, b)` | SameValue comparison |

> ⚠️ Verify: `Object.defineProperties`, `Object.entries` for symbol keys and `Object.getOwnPropertyDescriptors` for classes come from the documentation; they were not run in the experiments (descriptors of a literal were checked).

### The order of own keys

1. Integer index keys (`'1'`, `'2'`) ascending.
2. The other strings — in creation order.
3. Symbols — in creation order (not part of `Object.keys`).

In the experiment `Object.keys({ b: 1, 2: 1, a: 1, 1: 1, [Symbol('s')]: 1 })` returned `[ '1', '2', 'b', 'a' ]`.

### Attributes and protection

| Operation | Result in strict mode |
|---|---|
| Writing to a non-`writable` property | `TypeError: Cannot assign to read only property 'a' of object '#<Object>'` |
| Adding a property to a non-extensible object | `TypeError: Cannot add property b, object is not extensible` |
| Deleting a property of a sealed object | `TypeError: Cannot delete property 'a' of #<Object>` |
| Destructuring `null` | `TypeError: Cannot destructure property 'z' of 'null' as it is null.` |
| `String(Object.create(null))` | `TypeError: Cannot convert object to primitive value` |
| `structuredClone` with a function | `DataCloneError: f() {} could not be cloned.` |

## Internals

- **The internal slot `[[Prototype]]`.** It is a reference to a prototype or `null`. It is read by `Object.getPrototypeOf` and the (legacy) accessor `__proto__` from `Object.prototype`. In a literal `{ __proto__: p }` sets the prototype, while the key `"__proto__"` from `JSON.parse` is an ordinary own property (in the experiment).
- **Reading `o.k` (`[[Get]]`).** An own property is looked up; if there is none, the request goes to the prototype and so on up to `null`; if nothing is found — `undefined`. For an accessor `get` is called with the original object as `this`.
- **Writing `o.k = v` (`[[Set]]`).** The property is looked up along the chain: if a non-`writable` one is found (in the experiment a frozen prototype) or an accessor without `set`, the write is rejected (in strict mode a `TypeError`); if an accessor with `set` is found, it is called; otherwise an **own** property is created on `o` (in the experiment `der.v = 2` did not change `base.v`).
- **Enumerability.** `Object.keys`, `entries`, `JSON.stringify`, `Object.assign` and spread take only own enumerable properties into account; `for...in` adds inherited enumerable ones.
- **Methods of built-in objects are not enumerable:** `Object.keys(class.prototype)` is an empty list, the descriptor of `toString` has `enumerable: false` (in the experiment).
- **An object and keys.** Non-string keys are converted by `ToPropertyKey` (in the experiment an object and an array became strings), symbols stay symbols.
- **Further.** Engines store an object's "shape" (hidden class) and cache lookups, so the order in which properties are added affects speed; details are in module 19 of the roadmap.

> ⚠️ Verify: writing to an accessor without `set` and calling `set` when writing through a prototype (per the specification), the details of internal slots (`[[Extensible]]`), proxy objects (`Proxy` changes `[[Get]]`/`[[Set]]`) and the structure of hidden classes in V8 were not checked in the experiments.

## Gotchas

### 1. Key order is not write order

Numeric keys float to the front and are sorted (`[ '1', '2', 'b', 'a' ]`).

### 2. `o[1]` and `o['1']` are one property; an object as a key is `'[object Object]'`

So `o[{}] = 'c'` and `o[{ other: 1 }] = 'd'` overwrite one property.

### 3. A shallow copy and shared nested objects

`{ ...orig }` leaves `n` shared: `sp.n.v = 99` changed `orig` too (in the experiment).

### 4. A frozen prototype blocks writes on a descendant

The assignment `der2.ro = 2` fails although `der2` has no own `ro` (in the experiment).

### 5. Mutable state on a prototype is shared

`a1.list.push(1)` changed the list that is also visible through `a2` (in the experiment).

### 6. A prototype-less object does not convert to a string

`String(Object.create(null))` is a `TypeError`; the same in a template string. Logging with `console.log` is safe.

### 7. Destructuring `null` and `undefined` fails

You need a default (`= {}`) or a prior check.

### 8. `JSON.stringify` loses data

`undefined`, functions and symbols are skipped; `Date` becomes a string; `Map` becomes `{}`; a circular reference is a `TypeError` (in the experiment).

### 9. `Object.freeze` is shallow

`frozen.nested.b = 2` worked; deep protection needs recursion.

### 10. `for...in` and inherited keys

`for (const k in fi)` gave `[ 'own', 'inherited' ]`, `Object.keys(fi)` only `[ 'own' ]`.

## Trade-offs and alternatives

| Approach | Pros | Cons |
|---|---|---|
| A plain object as a dictionary | a literal, JSON | collisions with `__proto__`/`toString`, string keys, no size |
| `Object.create(null)` | no inherited keys | no `Object.prototype` methods |
| `Map` | any keys, order, `size` | not serialized to JSON directly |
| `{ ...o }` | concise | a shallow copy |
| `structuredClone` | a deep copy, cycles, `Date`, `Map` | loses prototypes and functions |
| `Object.freeze` | cheap | shallow |
| Classes (module 06) | structure and encapsulation | syntax, more code |

## Interview questions

<details><summary>1. What is a prototype and the prototype chain?</summary>

The object consulted when a property is missing on the object itself; the chain ends with `null`.

→ [Basic theory](basics.en.md#prototypes-the-basics)

</details>

<details><summary>2. How does <code>in</code> differ from <code>Object.hasOwn</code>?</summary>

`in` checks the prototype chain too, `hasOwn` only own properties.

→ [Basic theory](basics.en.md#access-checking-and-deletion)

</details>

<details><summary>3. Trick: will <code>der.v = 2</code> change the value on <code>base</code>?</summary>

No: the write creates an own property on `der`. The prototype's value is visible again after `delete der.v`.

→ [Basic theory](basics.en.md#prototypes-the-basics)

</details>

<details><summary>4. In what order are an object's keys listed?</summary>

First integer indexes ascending, then strings in insertion order, then symbols.

→ [Reference](#the-order-of-own-keys)

</details>

<details><summary>5. How does a shallow copy differ from a deep one?</summary>

A shallow copy copies one level and nested objects are shared; a deep one copies recursively (`structuredClone`).

→ [Advanced theory](advanced.en.md#deep-copying)

</details>

<details><summary>6. How do <code>freeze</code>, <code>seal</code> and <code>preventExtensions</code> differ?</summary>

`preventExtensions` forbids adding; `seal` also deleting; `freeze` also changing values. All are shallow.

→ [Advanced theory](advanced.en.md#freeze-seal-and-preventextensions)

</details>

<details><summary>7. Trick: why is <code>JSON.parse(JSON.stringify(x))</code> a bad way to copy?</summary>

`undefined`, functions and symbols are lost; `Date` becomes a string, `Map` becomes `{}`; cycles give an error.

→ [Gotcha 8](#8-jsonstringify-loses-data)

</details>

<details><summary>8. What is a property descriptor?</summary>

A set of attributes: `value` (or `get`/`set`), `writable`, `enumerable`, `configurable`.

→ [Advanced theory](advanced.en.md#property-descriptors)

</details>

<details><summary>9. What is <code>Object.create(null)</code> for?</summary>

A dictionary object without inherited keys; the price is the absence of `toString` and other methods.

→ [Advanced theory](advanced.en.md#prototype-less-objects)

</details>

<details><summary>10. Trick: what happens if you put an array on a prototype and change it through an instance?</summary>

The array is shared by all instances: the change is visible everywhere.

→ [Gotcha 5](#5-mutable-state-on-a-prototype-is-shared)

</details>

<details><summary>11. How does destructuring work with a default value and a rest?</summary>

`{ a = 1, ...rest } = obj` reads `a` (a default on `undefined`) and collects the other fields into a new object.

→ [Basic theory](basics.en.md#destructuring)

</details>

<details><summary>12. How does <code>for...in</code> differ from <code>Object.keys</code>?</summary>

`for...in` includes inherited enumerable keys, `Object.keys` only own ones.

→ [Gotcha 10](#10-forin-and-inherited-keys)

</details>

## Cheat sheet

```text
{ x, y, [k]: v, m() {}, get g() {} }   Object.hasOwn(o, k)   k in o
Object.keys / values / entries / fromEntries   Object.groupBy
{ ...o }   Object.assign({}, o)   structuredClone(o)
const { a = 1, ...rest } = o   const [x, , z = 0, ...r] = arr
Object.create(proto)   Object.create(null)   Object.getPrototypeOf(o)
Object.defineProperty   Object.freeze / seal / preventExtensions
```

- Keys are strings/symbols; order: indexes, strings, symbols.
- A copy is shallow; for a deep one — `structuredClone`.
- A write creates an own property; a read goes along the chain.

## Related materials

- Previous level: [advanced theory](advanced.en.md)
- Constructor functions and `new`: [04 · Functions](../04-functions/overview.en.md)
- Classes and prototype safety — modules 06 and 20 of the [track](../../overview.en.md) roadmap

## Sources

- [ECMAScript: Ordinary Object Internal Methods](https://tc39.es/ecma262/#sec-ordinary-object-internal-methods-and-internal-slots)
- [ECMAScript: OrdinaryOwnPropertyKeys](https://tc39.es/ecma262/#sec-ordinaryownpropertykeys)
- [MDN: Object](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Object)
- [MDN: Enumerability and ownership of properties](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Guide/Enumerability_and_ownership_of_properties)
- [MDN: Object.prototype.__proto__](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Object/proto)
