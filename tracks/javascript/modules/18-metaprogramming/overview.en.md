---
title: "Metaprogramming"
lang: en
track: javascript
kind: module
doc: overview
slug: 18-metaprogramming
part: 3
levels: [1, 2, 3, 4]
tags: [proxy, reflect, symbol, well-known-symbols, tagged-template, weak-ref, property-descriptor, metaprogramming]
versions: "ECMAScript 2025+, Node.js 24.15 LTS"
status: draft
updated: 2026-11-23
---

# Metaprogramming

🌐 [Русский](overview.ru.md) · **English**

## In short

Metaprogramming is code that controls the behavior of the language itself and of other objects: it intercepts reading and writing of properties (`Proxy`), calls "internal" operations (`Reflect`), tunes conversions and iteration through symbols (`Symbol`), parses template strings (tagged templates) and works with weak references (`WeakRef`). These are powerful tools for libraries and frameworks; in application code they are used rarely and carefully.

## Key ideas

- **`Proxy` wraps an object and intercepts operations** (`get`, `set`, `has`, `deleteProperty`, `apply`, `construct`, …) through traps.
- **`Reflect` provides "ordinary" versions of the same operations;** a trap usually performs the action through `Reflect.*`.
- **A proxy obeys invariants:** you cannot "lie" about a non-modifiable property.
- **`Symbol` is a unique key** that does not appear in `Object.keys` and `JSON.stringify`.
- **Well-known symbols** (`Symbol.iterator`, `Symbol.toPrimitive`, `Symbol.toStringTag`, `Symbol.hasInstance`, …) tune built-in behavior.
- **A tagged template** receives the strings and values separately: the basis of safe templating and `String.raw`.
- **Property descriptors** (`defineProperty`) set `writable`, `enumerable`, `configurable`, getters and setters.
- **`WeakRef` and `WeakMap` do not stop garbage collection,** but the collector's behavior is nondeterministic.
- **A proxy is not equal to its target** and breaks methods with internal slots (`Map`, private fields).

## Minimal example

```js
const p = new Proxy({ a: 1 }, {
  get(t, k, r) { console.log('get', String(k)); return Reflect.get(t, k, r); },
});
console.log(p.a);
```

```text
get a
1
```

Reading the property went through the `get` trap.

## When to use and when not

| Task | Choice |
|---|---|
| Validating values on write, reactivity | `Proxy` with a `set` trap |
| Lazy or computed properties | getters, `Proxy` |
| Making an object iterable or controlling conversion | `Symbol.iterator`, `Symbol.toPrimitive` |
| Safe substitution of values into a template | a tagged template |
| Hidden "service" keys | `Symbol` |
| A cache that does not hold objects | `WeakMap`, `WeakRef` |
| Ordinary validation in an application | functions and classes, no proxy |

- ❌ Do not use `Proxy` where an ordinary function is enough: it slows access and complicates debugging.
- ❌ Do not rely on the moment of garbage collection and on `FinalizationRegistry` firing.

## Material levels

| Level | What is inside |
|---|---|
| [1. Basic theory](basics.en.md) | symbols, descriptors, `Proxy` and `Reflect`, tagged templates |
| [2. Advanced theory](advanced.en.md) | traps and invariants, well-known symbols, reactivity, proxy limitations, weak references |
| [3. Expert theory](expert.en.md) | a reference, an object's internal methods, pitfalls, interview questions |
| [4. Self-check](quiz.en.md) | quiz and [exercises](exercises.en.md) |
| 5. Runnable examples | not created yet — `/learn javascript 18-metaprogramming level 5` |
| 6. Tests | not created yet — `/learn javascript 18-metaprogramming level 6` |

## Related materials

- Prerequisites: [05 · Objects and prototypes](../05-objects-and-prototypes/overview.en.md), [06 · Classes](../06-classes/overview.en.md), [11 · Iterators and generators](../11-iterators-and-generators/overview.en.md)
- Next: module 19 · Performance and memory (in the [roadmap](../../overview.en.md))
- Analogues in other tracks: [C#: reflection and source generators](../../../csharp/modules/20-reflection-and-source-generators/overview.en.md), [Python](../../../python/overview.en.md) (magic methods, metaclasses)
