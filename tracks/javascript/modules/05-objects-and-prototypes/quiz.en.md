---
title: "Quiz: objects and prototypes"
lang: en
track: javascript
kind: module
doc: quiz
slug: 05-objects-and-prototypes
part: 1
tags: [objects, prototype, destructuring, spread, freeze, structured-clone]
status: draft
updated: 2026-11-23
---

# Quiz: objects and prototypes

🌐 [Русский](quiz.ru.md) · **English**

> Answer first, then open the answer. The mark shows which level a question comes from: 🟢 [basics](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 What does `Object.keys({ b: 1, 2: 1, a: 1, 1: 1 })` return?

- A) `['b', '2', 'a', '1']`
- B) `['1', '2', 'b', 'a']`
- C) `['b', 'a', '1', '2']`
- D) `['a', 'b', '1', '2']`

<details><summary>Answer</summary>

**B.** Integer keys come first in ascending order, then strings in insertion order.
→ [Basics](basics.en.md#keys-and-their-order)

</details>

## 2. 🟢 What do `'toString' in {}` and `Object.hasOwn({}, 'toString')` return?

- A) `true` and `true`
- B) `true` and `false`
- C) `false` and `false`
- D) `false` and `true`

<details><summary>Answer</summary>

**B.** `in` checks the prototype chain too (`toString` lives on `Object.prototype`), `hasOwn` only own properties.
→ [Basics](basics.en.md#access-checking-and-deletion)

</details>

## 3. 🟢 What happens to `orig.n.v` after the code?

```js
const orig = { n: { v: 1 } };
const sp = { ...orig };
sp.n.v = 99;
```

- A) `1`
- B) `99`
- C) `undefined`
- D) An error

<details><summary>Answer</summary>

**B.** Spread copies one level: the nested object `n` is shared.
→ [Basics](basics.en.md#copying-shallow)

</details>

## 4. 🟢 What does `const { z } = null;` give?

- A) `undefined`
- B) `null`
- C) `TypeError`
- D) `ReferenceError`

<details><summary>Answer</summary>

**C.** `TypeError: Cannot destructure property 'z' of 'null' as it is null.` A safe form: `const { z } = obj ?? {}`.
→ [Basics](basics.en.md#destructuring)

</details>

## 5. 🟡 `base = { v: 1 }`, `der = Object.create(base)`. What does `der.v = 2; console.log(base.v, der.v);` print?

- A) `2 2`
- B) `1 2`
- C) `1 1`
- D) `2 1`

<details><summary>Answer</summary>

**B.** The write creates an own property on `der`, the prototype does not change.
→ [Basics](basics.en.md#prototypes-the-basics)

</details>

## 6. 🟡 What is the result of `Object.freeze` on the nested object `{ a: 1, nested: { b: 1 } }` after `frozen.nested.b = 2`?

- A) `TypeError`
- B) The value changed: `freeze` is shallow
- C) The value did not change
- D) `nested` was deleted

<details><summary>Answer</summary>

**B.** Only top-level properties are frozen.
→ [Advanced](advanced.en.md#freeze-seal-and-preventextensions)

</details>

## 7. 🟡 What does `structuredClone` do with a function inside an object?

- A) Copies the function
- B) Skips the property
- C) Throws a `DataCloneError`
- D) Replaces it with `null`

<details><summary>Answer</summary>

**C.** `DataCloneError: f() {} could not be cloned.` `JSON.stringify` would silently skip the property.
→ [Advanced](advanced.en.md#deep-copying)

</details>

## 8. 🟡 What does `JSON.parse(JSON.stringify({ d: new Date(0), u: undefined, m: new Map([[1, 2]]) }))` return?

- A) `{ d: Date, u: undefined, m: Map }`
- B) `{ d: '1970-01-01T00:00:00.000Z', m: {} }`
- C) `{ d: 0, m: [[1, 2]] }`
- D) `TypeError`

<details><summary>Answer</summary>

**B.** The date became a string, the `Map` an empty object, and `undefined` vanished.
→ [Advanced](advanced.en.md#deep-copying)

</details>

## 9. 🟡 Select all correct statements

- [ ] A) `Object.keys` lists inherited properties
- [ ] B) `for...in` lists inherited enumerable properties
- [ ] C) `Object.create(null)` returns an object without a prototype
- [ ] D) `String(Object.create(null))` returns `'[object Object]'`

<details><summary>Answer</summary>

**B, C.** A is wrong (only own ones); D is wrong: `TypeError: Cannot convert object to primitive value`.
→ [Advanced](advanced.en.md#prototype-less-objects)

</details>

## 10. 🔴 Find the flaw

> "Let us put an array on the prototype: `const proto = { list: [] }` — every descendant will have its own list."

<details><summary>Answer</summary>

There is one array for everyone: `a1.list.push(1)` changes the list that `a2` sees too. Mutable state must be created in the object itself.
→ [Gotcha 5](expert.en.md#5-mutable-state-on-a-prototype-is-shared)

</details>

## 11. 🔴 `base2 = Object.freeze({ ro: 1 })`, `der2 = Object.create(base2)`. What happens on `der2.ro = 2` in strict mode?

- A) An own property `ro` appears on `der2`
- B) `TypeError: Cannot assign to read only property 'ro'`
- C) `base2.ro` changes
- D) Nothing, silently

<details><summary>Answer</summary>

**B.** If the inherited property is not writable, the assignment is rejected instead of creating an own one.
→ [Gotcha 4](expert.en.md#4-a-frozen-prototype-blocks-writes-on-a-descendant)

</details>

## 12. 🔴 Open question

Explain how reading and writing properties work through the prototype chain.

<details><summary>Key points of a good answer</summary>

- Reading: an own property is looked up, then on the prototype and onward up to `null`; not found — `undefined`.
- Writing: creates (or changes) an own property; exceptions — an inherited non-`writable` property and an accessor.
- `hasOwn` and `keys` see only own properties, `in` and `for...in` — inherited ones too.

</details>

→ [Expert](expert.en.md#internals)
