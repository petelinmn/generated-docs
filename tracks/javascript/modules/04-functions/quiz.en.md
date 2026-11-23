---
title: "Quiz: functions"
lang: en
track: javascript
kind: module
doc: quiz
slug: 04-functions
part: 1
tags: [functions, this, arrow-functions, bind, default-parameters]
status: draft
updated: 2026-11-23
---

# Quiz: functions

🌐 [Русский](quiz.ru.md) · **English**

> Answer first, then open the answer. The mark shows which level a question comes from: 🟢 [basics](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 What does `decl(1)` return for `function decl(a, b) { return a + b; }`?

- A) `1`
- B) `NaN`
- C) An error: an argument is missing
- D) `undefined`

<details><summary>Answer</summary>

**B.** The missing argument is `undefined`, and `1 + undefined` is `NaN`.
→ [Basics](basics.en.md#parameters-and-arguments)

</details>

## 2. 🟢 What does `dflt(1, null)` return for `function dflt(a, b = 2) { return [a, b]; }`?

- A) `[1, 2]`
- B) `[1, null]`
- C) `[1, undefined]`
- D) An error

<details><summary>Answer</summary>

**B.** A default value is substituted only for `undefined`.
→ [Basics](basics.en.md#default-values)

</details>

## 3. 🟢 What does the function return?

```js
function f() { return
  42 }
```

- A) `42`
- B) `undefined`
- C) `null`
- D) `SyntaxError`

<details><summary>Answer</summary>

**B.** A `;` is inserted automatically after `return`, so `42` is not reached.
→ [Basics](basics.en.md#returning-a-value)

</details>

## 4. 🟢 What is `['1','2','3'].map(parseInt)`?

- A) `[1, 2, 3]`
- B) `[1, NaN, NaN]`
- C) `['1','2','3']`
- D) An error

<details><summary>Answer</summary>

**B.** `map` passes the index as the second argument, and `parseInt` takes it as the radix.
→ [Basics](basics.en.md#functions-as-values)

</details>

## 5. 🟡 What does `detached()` return?

```js
const obj = { name: 'obj', m() { return this?.name; } };
const detached = obj.m;
```

- A) `'obj'`
- B) `undefined`
- C) `TypeError`
- D) `''`

<details><summary>Answer</summary>

**B.** In strict mode the `this` of a detached method is `undefined`; `?.` prevents the error. Without `?.` there would be a `TypeError`.
→ [Basics](basics.en.md#this-the-basics)

</details>

## 6. 🟡 What is `bound()` after `const bound = obj.m.bind({ name: 'bound' })` and `bound.call({ name: 'x' })`?

- A) `'bound'` in both cases
- B) `'bound'` and `'x'`
- C) `'obj'` and `'x'`
- D) `undefined` in both

<details><summary>Answer</summary>

**A.** `bind` fixes `this` for good; `call` cannot change it.
→ [Advanced](advanced.en.md#call-apply-and-bind)

</details>

## 7. 🟡 What happens on `new arrow()`, where `arrow = (a, b) => a + b`?

- A) An object is created
- B) `TypeError: arrow is not a constructor`
- C) `undefined`
- D) `ReferenceError`

<details><summary>Answer</summary>

**B.** Arrows have no `[[Construct]]` and no `prototype`.
→ [Advanced](advanced.en.md#limitations-of-arrow-functions)

</details>

## 8. 🟡 What is the length of `((a, b = 1, ...c) => {}).length`?

- A) `3`
- B) `2`
- C) `1`
- D) `0`

<details><summary>Answer</summary>

**C.** `length` counts parameters before the first default value or `...rest`.
→ [Basics](basics.en.md#three-forms-of-functions)

</details>

## 9. 🟡 Select all correct statements

- [ ] A) `arguments` is a real array
- [ ] B) `...rest` is a real array
- [ ] C) An arrow has no `this` of its own
- [ ] D) A default value is evaluated once when the function is defined

<details><summary>Answer</summary>

**B, C.** A is wrong (`Array.isArray(arguments)` is `false`); D is wrong: a default is evaluated on every call.
→ [Basics](basics.en.md#rest-parameters-and-arguments)

</details>

## 10. 🟡 What does `strictThis.call(5)` return in an ES module for `function strictThis() { return this; }`?

- A) A `Number` object
- B) The primitive `5`
- C) `undefined`
- D) The global object

<details><summary>Answer</summary>

**B.** In strict mode `this` is not wrapped. In sloppy code it would be a wrapper object.
→ [Reference](expert.en.md#this-rules-in-order-of-priority)

</details>

## 11. 🔴 Find the flaw

> "An arrow function is just a short form of an ordinary one, so `obj = { m: () => this.name }` returns the object's name."

<details><summary>Answer</summary>

An arrow has no `this` of its own: it is taken from the surrounding scope (in a module — `undefined`), not from `obj`. For a method use `m() { return this.name }`.
→ [Gotcha 5](expert.en.md#5-an-arrow-as-a-method)

</details>

## 12. 🔴 What happens on `Person('x')` without `new` for `function Person(name) { this.name = name; }` in strict mode?

- A) A global property `name` is created
- B) `TypeError: Cannot set properties of undefined (setting 'name')`
- C) A `Person` object is returned
- D) Nothing

<details><summary>Answer</summary>

**B.** `this` is `undefined`, and setting a property fails.
→ [Gotcha 6](expert.en.md#6-a-constructor-without-new)

</details>

## 13. 🔴 Open question

List how `this` is determined for the different ways of calling, and explain why a method passed as a callback breaks.

<details><summary>Key points of a good answer</summary>

- `new` — a new object; `bind` — the fixed one; `call`/`apply` — the given one; `obj.f()` — `obj`; `f()` — `undefined` (strict mode) or the global object; an arrow — the outer `this`.
- Passing a method as a value detaches it from the object, so `this` is lost; the fix: `bind`, an arrow wrapper or a class field method.

</details>

→ [Expert](expert.en.md#this-rules-in-order-of-priority)
