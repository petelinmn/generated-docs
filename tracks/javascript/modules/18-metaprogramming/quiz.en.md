---
title: "Quiz: metaprogramming"
lang: en
track: javascript
kind: module
doc: quiz
slug: 18-metaprogramming
part: 3
tags: [proxy, reflect, symbol, tagged-template, weak-ref]
status: draft
updated: 2026-11-23
---

# Quiz: metaprogramming

🌐 [Русский](quiz.ru.md) · **English**

> Answer on your own first, then open the answer. The label shows the question level: 🟢 [basic](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 What does `Symbol('a') === Symbol('a')` equal?

- A) `true`
- B) `false`
- C) `TypeError`
- D) Depends on the description

<details><summary>Answer</summary>

**B.** Every symbol is unique; for a shared symbol use `Symbol.for('a')`.
→ [Basic theory](basics.en.md#symbols)

</details>

## 2. 🟢 What does `Object.keys({ [Symbol('s')]: 1, x: 2 })` print?

- A) `[ 'x' ]`
- B) `[ Symbol(s), 'x' ]`
- C) `[]`
- D) `[ 'x', 'Symbol(s)' ]`

<details><summary>Answer</summary>

**A.** Symbol keys do not appear in `Object.keys`; `Object.getOwnPropertySymbols` and `Reflect.ownKeys` see them.
→ [Basic theory](basics.en.md#symbols)

</details>

## 3. 🟢 What does a `get` trap do?

- A) Intercepts reading a property of the proxy
- B) Intercepts a function call
- C) Intercepts writing
- D) Creates a proxy

<details><summary>Answer</summary>

**A.** Writing is intercepted by `set`, a call by `apply`.
→ [Basic theory](basics.en.md#proxy-intercepting-operations)

</details>

## 4. 🟢 What does `String.raw` return for a template with the text `a\nb` (a backslash and the letter `n`)?

- A) `a`, a line break, `b`
- B) `a\nb` as four characters (`\` and `n` are kept)
- C) An error
- D) `anb`

<details><summary>Answer</summary>

**B.** `String.raw` does not process escape sequences.
→ [Basic theory](basics.en.md#tagged-template-strings)

</details>

## 5. 🟡 What happens if a `set` trap returns `false` in an ES module?

- A) The write is silently ignored
- B) `TypeError: 'set' on proxy: trap returned falsish…`
- C) The value is written
- D) The proxy is disabled

<details><summary>Answer</summary>

**B.** Module code is strict; in sloppy mode the write is silently lost.
→ [Expert theory](expert.en.md#1-a-set-trap-without-return-true)

</details>

## 6. 🟡 What does the code print?

```js
class Temp { constructor(c) { this.c = c } [Symbol.toPrimitive](hint) { return hint === 'string' ? `${this.c}°C` : this.c } }
const t = new Temp(20); console.log(`${t}`, +t, t + 1);
```

- A) `20°C 20 21`
- B) `[object Object] NaN [object Object]1`
- C) `20 20 21`
- D) `20°C 20 20°C1`

<details><summary>Answer</summary>

**A.** The template asks for `hint = 'string'`, `+t` and `t + 1` for `'number'`/`'default'` (a number is returned).
→ [Advanced theory](advanced.en.md#well-known-symbols)

</details>

## 7. 🟡 What happens with `new Proxy(new Map(), {}).set('a', 1)`?

- A) The write succeeds
- B) `TypeError: Method Map.prototype.set called on incompatible receiver`
- C) `undefined`
- D) It hangs

<details><summary>Answer</summary>

**B.** `Map` has an internal slot, the proxy does not. Methods must be bound to the target.
→ [Advanced theory](advanced.en.md#proxy-limitations-and-invariants)

</details>

## 8. 🟡 What does `Reflect.set(Object.freeze({ a: 1 }), 'a', 2)` return?

- A) `true`
- B) `false`
- C) `TypeError`
- D) `2`

<details><summary>Answer</summary>

**B.** `Reflect.set` returns a `boolean` and does not throw.
→ [Basic theory](basics.en.md#reflect)

</details>

## 9. 🟡 Choose all true statements

- [ ] A) A `WeakMap` can be iterated with `for...of`
- [ ] B) A `WeakMap` key is an object or a non-registered symbol
- [ ] C) `WeakRef.deref()` may return `undefined`
- [ ] D) A `FinalizationRegistry` callback is called at a predictable moment

<details><summary>Answer</summary>

**B, C.** A is wrong: weak collections cannot be iterated; D is wrong: garbage collection is nondeterministic.
→ [Advanced theory](advanced.en.md#weak-references)

</details>

## 10. 🔴 Find the mistake

> "`Object.defineProperty(o, 'x', { value: 1 })` creates an ordinary enumerable property that can be changed."

<details><summary>Answer</summary>

By default `writable`, `enumerable` and `configurable` are `false`: the property is immutable and not visible in `Object.keys`.
→ [Gotcha 8](expert.en.md#8-defineproperty-creates-a-hidden-immutable-property)

</details>

## 11. 🔴 What happens?

```js
const frozen = Object.freeze({ x: 1 });
const lie = new Proxy(frozen, { get() { return 2 } });
lie.x;
```

- A) `2`
- B) `1`
- C) A `TypeError` because of an invariant violation
- D) `undefined`

<details><summary>Answer</summary>

**C.** The engine checks the trap's result against the target's immutable property.
→ [Advanced theory](advanced.en.md#proxy-limitations-and-invariants)

</details>

## 12. 🔴 Will the tag function receive **the same** `strings` object if you call `` f`x` `` twice from one place in the code (in a loop)?

- A) A new object each time
- B) The same one, and it is frozen
- C) Depends on the substituted values
- D) The same one, but mutable

<details><summary>Answer</summary>

**B.** The `strings` array is created once per place in the code and is frozen.
→ [Internals](expert.en.md#internals)

</details>

## 13. 🔴 Open question

Explain how `Proxy` works at the level of internal methods and what limitations a proxy has with built-in types and private fields.

<details><summary>Key points of a good answer</summary>

- Every object has internal methods (`[[Get]]`, `[[Set]]`, `[[OwnPropertyKeys]]`…); in a proxy they call traps or the operation on the target.
- Invariants: a trap cannot break the target's guarantees (frozen and non-configurable properties).
- Internal slots (`Map`, `Set`, `Date`, `Promise`) and `#` fields belong to the real object; `this` = the proxy breaks methods: bind to the target, `Reflect.get(t, k, t)`.
- A proxy is not equal to its target and slows access; for ordinary validation functions and classes are enough.

</details>

→ [Expert theory](expert.en.md#internals)
