---
title: "Metaprogramming: expert"
lang: en
track: javascript
kind: module
doc: expert
slug: 18-metaprogramming
part: 3
tags: [proxy-traps, internal-methods, reflect-api, well-known-symbols, property-descriptors, weakref-gc, tagged-template-cache]
versions: "ECMAScript 2025+, Node.js 24.15 LTS"
status: draft
updated: 2026-11-23
---

# Metaprogramming: expert

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · **3. Expert** · [4. Quiz](quiz.en.md)

Builds on [basic](basics.en.md) and [advanced](advanced.en.md) theory. All results were obtained by running on Node.js 24.15.

## Reference

### Proxy traps and the operations they intercept

| Trap | Which operation it intercepts |
|---|---|
| `get(t, k, receiver)` | reading a property `p.k`, `p[k]` |
| `set(t, k, v, receiver)` | writing `p.k = v` |
| `has(t, k)` | the operator `k in p` |
| `deleteProperty(t, k)` | `delete p.k` |
| `ownKeys(t)` | `Object.keys`, `Reflect.ownKeys`, `for...in`, `JSON.stringify` |
| `getOwnPropertyDescriptor(t, k)` | `Object.getOwnPropertyDescriptor`, and also `Object.keys` and spread: for each key |
| `defineProperty(t, k, desc)` | `Object.defineProperty` |
| `getPrototypeOf(t)` | `Object.getPrototypeOf`, `instanceof` |
| `setPrototypeOf(t, proto)` | `Object.setPrototypeOf` |
| `isExtensible(t)`, `preventExtensions(t)` | `Object.isExtensible`, `Object.preventExtensions` |
| `apply(t, thisArg, args)` | calling a target function |
| `construct(t, args, newTarget)` | `new p(...)` |

In the experiment, with the traps present: `Object.keys(p)` called `ownKeys`, then `getOwnPropertyDescriptor`; `p instanceof Object` — `getPrototypeOf`; `Object.freeze(p)` — `preventExtensions`, `ownKeys`, `getOwnPropertyDescriptor`, `defineProperty`; spread `{ ...p }` — `ownKeys`, `getOwnPropertyDescriptor`, `get`.

### Reflect

Every trap has a `Reflect.*` function with the same name (except `Reflect.ownKeys`, which implements "all keys"). The difference from the old APIs: `Reflect.defineProperty`, `set`, `deleteProperty` return a `boolean` and do not throw (in the experiment `Reflect.set` on a frozen object is `false`).

### Well-known symbols (list)

`Symbol.iterator`, `asyncIterator`, `hasInstance`, `isConcatSpreadable`, `match`, `matchAll`, `replace`, `search`, `species`, `split`, `toPrimitive`, `toStringTag`, `unscopables`; plus `Symbol.dispose`, `Symbol.asyncDispose` for explicit resource management ([module 14](../14-errors-and-debugging/advanced.en.md#releasing-resources-finally-and-using)). `Symbol.dispose` in Node.js 24.15 is a symbol (in the experiment `symbol`).

### Property descriptors

| How it was created | `writable` | `enumerable` | `configurable` |
|---|---|---|---|
| Assignment `o.x = 1`, a literal `{ x: 1 }` | `true` | `true` | `true` |
| `Object.defineProperty(o, 'x', { value: 1 })` | `false` | `false` | `false` |
| A property after `Object.freeze` | `false` | unchanged | `false` |

The descriptor of a frozen property: `{ value: 1, writable: false, enumerable: true, configurable: false }` ([module 10](../10-closures-and-functional/expert.en.md#reference)).

## Internals

- **An object's internal methods.** Every object in the specification has a set of "internal methods" (`[[Get]]`, `[[Set]]`, `[[HasProperty]]`, `[[Delete]]`, `[[OwnPropertyKeys]]`, …). Ordinary objects implement them in the standard way; **exotic** objects (arrays, strings, arguments, `Proxy`) override some. A proxy is an object whose every internal method calls the corresponding trap or, if there is none, the operation on the target.
- **Invariants.** After a trap runs, the engine checks the result against the target's state: you cannot report that a non-configurable property does not exist, replace the value of an immutable property, and so on (in the experiment `TypeError: 'get' on proxy: property 'x' is a read-only and non-configurable…`).
- **Internal slots.** `Map`, `Set`, `Date`, `Promise` and objects with `#` fields have slots tied to a specific object; a proxy has none, so methods with `this` = the proxy fail (in the experiment `Method Map.prototype.set called on incompatible receiver`, `Cannot read private member #x`).
- **Tagged templates.** The `strings` array is created **once per place** in the source code and cached: a repeated call from the same place gets the same object (in the experiment `same strings object each call true`), it is frozen (`Object.isFrozen` is `true`) and has `raw`. This allows caching a result by `strings`.
- **Symbols.** A symbol is a primitive with a unique identity; a symbol key is stored apart from string keys (the order of `Reflect.ownKeys`: integers, strings, symbols). Registry symbols (`Symbol.for`) cannot be used as keys of weak collections (in the experiment `TypeError`).
- **Weak references.** `WeakRef` and weak collections do not keep a value from being collected; when exactly it is collected is up to the engine. Within one "job" `WeakRef.deref()` returns the same result (per the specification the object stays alive until the end of the current job).

> ⚠️ Verify: the list of well-known symbols comes from the specification; holding a `WeakRef` target within a job and the timing of garbage collection were not checked in the experiments.

## Gotchas

### 1. A `set` trap without `return true`

In strict mode — `TypeError: 'set' on proxy: trap returned falsish for property 'x'` (in the experiment); in sloppy mode the write is silently lost.

### 2. A proxy ≠ its target

`new Proxy(target, {}) === target` is `false`; `Array.isArray(proxy)` for an array is `true`.

### 3. Private fields and `Map` through a proxy

A `TypeError` because of slots: bind methods to the target.

### 4. Invariants

You cannot return another value for a frozen property.

### 5. `Reflect.get(t, k, t)` versus `Reflect.get(t, k, r)`

With `receiver` = the proxy a getter sees `this` = the proxy, with the target — the target (in the experiment `child` and `base`).

### 6. A symbol cannot be glued to a string

`s + ''` is `TypeError: Cannot convert a Symbol value to a string`.

### 7. A symbol key is hidden

`JSON.stringify` and `Object.keys` do not see it, `Reflect.ownKeys` does.

### 8. `defineProperty` creates a hidden immutable property

Without flags all are `false` (in the experiment).

### 9. A tagged template: one `strings` per place

Cache by `strings`, but do not change the array: it is frozen.

### 10. `WeakRef.deref()` may return `undefined`

Always check the result.

## Trade-offs and alternatives

| Approach | Pros | Cons |
|---|---|---|
| `Proxy` | universal interception, transparency | slower, breaks slots and private fields, hard to debug |
| Getters and setters | simple, fast | one property at a time |
| `Object.defineProperty` | control over flags | verbose |
| Explicit methods (`set()`/`get()`) | predictable | an inconvenient API |
| Classes with `#` | real encapsulation | cannot "peek" from outside |
| Code generation (`new Function`) | speed | unsafe, hard to debug |
| Decorators and compilers (TypeScript, Babel) | metadata at build time | build tooling; not run |

## Interview questions

<details><summary>1. What is a <code>Proxy</code> and which traps do you know?</summary>

A wrapper object that intercepts operations on the target; traps `get`, `set`, `has`, `deleteProperty`, `ownKeys`, `apply`, `construct`, `getPrototypeOf`, and others.

→ [Basic theory](basics.en.md#proxy-intercepting-operations)

</details>

<details><summary>2. What is <code>Reflect</code> for?</summary>

It performs the standard action inside a trap and returns a `boolean` instead of throwing; it supports `receiver`.

→ [Basic theory](basics.en.md#reflect)

</details>

<details><summary>3. Trick: what happens if a <code>set</code> trap returns <code>false</code>?</summary>

In strict mode a `TypeError`, in sloppy mode the write is silently ignored.

→ [Gotcha 1](#1-a-set-trap-without-return-true)

</details>

<details><summary>4. Trick: why does <code>new Proxy(new Map(), {}).set('a', 1)</code> fail?</summary>

`Map` has an internal slot, the proxy does not: `this` = the proxy is invalid. Bind the method to the target.

→ [Gotcha 3](#3-private-fields-and-map-through-a-proxy)

</details>

<details><summary>5. How does a symbol differ from a string as a key?</summary>

It is unique, not visible to `Object.keys`/`JSON.stringify`/`for...in`; visible to `Object.getOwnPropertySymbols` and `Reflect.ownKeys`.

→ [Basic theory](basics.en.md#symbols)

</details>

<details><summary>6. What is <code>Symbol.toPrimitive</code>?</summary>

A method with a `hint` argument (`'string'`, `'number'`, `'default'`) that controls converting an object to a primitive.

→ [Advanced theory](advanced.en.md#well-known-symbols)

</details>

<details><summary>7. What does the tag function of a tagged template receive?</summary>

An array of strings `strings` (with `strings.raw`) and the values of the substitutions as separate arguments.

→ [Basic theory](basics.en.md#tagged-template-strings)

</details>

<details><summary>8. Trick: how does <code>defineProperty</code> configure a property by default?</summary>

`writable`, `enumerable`, `configurable` — all `false`.

→ [Gotcha 8](#8-defineproperty-creates-a-hidden-immutable-property)

</details>

<details><summary>9. What are proxy invariants?</summary>

Engine checks: a trap cannot break the target's guarantees (for example, replace the value of a non-configurable property).

→ [Advanced theory](advanced.en.md#proxy-limitations-and-invariants)

</details>

<details><summary>10. What are <code>WeakMap</code> and <code>WeakRef</code> for and what is the risk?</summary>

To keep data about objects without preventing their collection; the timing of collection is nondeterministic, a key is only an object or a local symbol.

→ [Advanced theory](advanced.en.md#weak-references)

</details>

<details><summary>11. What does <code>Proxy.revocable</code> do?</summary>

It creates a proxy whose access can be revoked by calling `revoke()`.

→ [Advanced theory](advanced.en.md#a-revocable-proxy-and-the-receiver)

</details>

<details><summary>12. Why should you not use <code>eval</code> and <code>Function</code> on input strings?</summary>

It is arbitrary code execution: an injection risk.

→ [Advanced theory](advanced.en.md#functions-newtarget-eval)

</details>

## Cheat sheet

```text
new Proxy(target, { get(t, k, r), set(t, k, v, r) → true, has, deleteProperty, ownKeys, apply, construct })
Reflect.get/set/has/deleteProperty/ownKeys/defineProperty/apply/construct(…)  // set/define/delete → boolean
Symbol('d') is unique; Symbol.for('k') — from the registry; Reflect.ownKeys sees symbols
[Symbol.iterator] / [Symbol.toPrimitive](hint) / [Symbol.toStringTag] / static [Symbol.hasInstance]
tag`a${1}b` → tag(strings, ...vals); String.raw`\n`
Object.defineProperty(o, k, { value, writable, enumerable, configurable }) — all false by default
WeakMap/WeakSet (key — an object), WeakRef.deref() → object | undefined
```

- A proxy obeys invariants and breaks slots.
- `set` returns `true`.
- Do not rely on garbage collection.

## Related materials

- Previous level: [advanced theory](advanced.en.md)
- Objects and prototypes: [05 · Objects and prototypes](../05-objects-and-prototypes/expert.en.md)
- Classes and private fields: [06 · Classes](../06-classes/expert.en.md)
- Iteration: [11 · Iterators and generators](../11-iterators-and-generators/expert.en.md)

## Sources

- [ECMAScript: Proxy Objects](https://tc39.es/ecma262/#sec-proxy-objects)
- [ECMAScript: Reflect](https://tc39.es/ecma262/#sec-reflect-object)
- [ECMAScript: Well-Known Symbols](https://tc39.es/ecma262/#sec-well-known-symbols)
- [MDN: Meta programming](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Guide/Meta_programming)
- [MDN: Object.defineProperty](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Object/defineProperty)
