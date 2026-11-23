---
title: "Objects and prototypes"
lang: en
track: javascript
kind: module
doc: overview
slug: 05-objects-and-prototypes
part: 1
levels: [1, 2, 3, 4]
tags: [objects, prototype, prototype-chain, property-descriptors, destructuring, spread, object-create, freeze, structured-clone]
versions: "ECMAScript 2025+, Node.js 24.15 LTS"
status: draft
updated: 2026-11-23
---

# Objects and prototypes

🌐 [Русский](overview.ru.md) · **English**

## In short

An object in JavaScript is a set of "key — value" pairs (properties). Keys are strings or symbols, values are anything. The easiest way to create objects is a **literal** `{ a: 1 }`. Every object has a **prototype** — another object that JavaScript consults if a property is not found on the object itself. The prototype chain is the basis of inheritance in the language. Properties have **attributes** (writability, enumerability, configurability), and an object can be frozen. Copies of objects are **shallow** by default: nested objects stay shared. For taking objects and arrays apart there are **destructuring** and the spread operator `...`.

## Key ideas

- **A literal is the main way:** shorthand `{ x, y }`, computed keys `[expr]`, methods, getters and setters.
- **Keys are strings (or symbols):** `o[1]` and `o['1']` are the same property; the order of keys is defined (integer ones ascending, then strings in insertion order).
- **`in` checks the prototype chain too,** while `Object.hasOwn(o, k)` checks only own properties.
- **A prototype is a fallback source of properties:** reading goes along the chain, while writing creates an own property on the object.
- **`Object.keys`, `values`, `entries` list only own enumerable properties;** `for...in` also catches inherited ones.
- **Copying with `{ ...o }` and `Object.assign` is shallow;** for a deep copy use `structuredClone`.
- **`Object.freeze` protects only the top level from writes** (in strict mode a violation is a `TypeError`).
- **Destructuring** extracts fields and elements by a pattern, supports defaults and the rest `...rest`.

## Minimal example

```js
const proto = { hello() { return 'hi ' + this.name; } };
const child = Object.create(proto);
child.name = 'c';
const { name, ...rest } = { name: 'x', a: 1, b: 2 };
console.log(child.hello(), Object.keys(child), name, rest);
```

```text
hi c [ 'name' ] x { a: 1, b: 2 }
```

## When to use and when not

| Task | Tool |
|---|---|
| Record data as a set of fields | a literal `{ ... }` |
| A dictionary with arbitrary keys | `Map` (module 07 of the roadmap) or a prototype-less object `Object.create(null)` |
| A shallow copy | `{ ...o }` |
| A deep copy | `structuredClone(o)` |
| Forbid changes | `Object.freeze` (shallow) |
| Inheritance "along a chain" | `Object.create(proto)`, today more often `class` (module 06) |

- ❌ Do not use a plain object as a dictionary for keys from user input without protection (keys like `__proto__`).
- ❌ Do not treat `{ ...o }` as a deep copy.

## Material levels

| Level | What is inside |
|---|---|
| [1. Basic theory](basics.en.md) | literals, access, `in`/`hasOwn`/`delete`, key order, `keys`/`entries`, copying, destructuring, prototype basics |
| [2. Advanced theory](advanced.en.md) | property descriptors, `freeze`/`seal`, deep copying, prototype traps, prototype-less objects, symbols as keys |
| [3. Expert theory](expert.en.md) | a reference of `Object` methods, key order rules, property lookup, pitfalls, interview questions |
| [4. Self-check](quiz.en.md) | quiz and [exercises](exercises.en.md) |
| 5. Runnable examples | not created yet — `/learn javascript 05-objects-and-prototypes level 5` |
| 6. Tests | not created yet — `/learn javascript 05-objects-and-prototypes level 6` |

## Related materials

- Prerequisites: [02 · Types and values](../02-types-and-values/overview.en.md) (values and references), [04 · Functions](../04-functions/overview.en.md) (`this`, `new`)
- Next: [06 · Classes](../06-classes/overview.en.md)
- Analogues in other tracks: [Python: OOP and dataclasses](../../../python/overview.en.md), [C#: classes and records](../../../csharp/overview.en.md)
- Typing objects: [TypeScript](../../../typescript/overview.en.md)
