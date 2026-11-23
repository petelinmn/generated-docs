---
title: "Quiz: classes"
lang: en
track: javascript
kind: module
doc: quiz
slug: 06-classes
part: 1
tags: [classes, inheritance, private-fields, super, static]
status: draft
updated: 2026-11-23
---

# Quiz: classes

🌐 [Русский](quiz.ru.md) · **English**

> Answer first, then open the answer. The mark shows which level a question comes from: 🟢 [basics](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 What happens on `Point()` without `new` if `Point` is a class?

- A) `undefined` is returned
- B) `TypeError: Class constructor Point cannot be invoked without 'new'`
- C) A global variable is created
- D) An object is returned

<details><summary>Answer</summary>

**B.** A class cannot be called like an ordinary function.
→ [Basics](basics.en.md#a-class-cannot-be-called-without-new)

</details>

## 2. 🟢 Where does a method `toString` declared in a class body live?

- A) In every instance
- B) On `Class.prototype`
- C) In the class itself as a static one
- D) In `Object.prototype`

<details><summary>Answer</summary>

**B.** The method is shared by all instances and is non-enumerable. Fields (`x = 0;`) live in the instance.
→ [Basics](basics.en.md#declaring-a-class)

</details>

## 3. 🟢 What does `Object.keys(p)` return for an instance with fields `x`, `y` and `#secret`?

- A) `['x', 'y', '#secret']`
- B) `['x', 'y']`
- C) `['#secret']`
- D) `[]`

<details><summary>Answer</summary>

**B.** Private fields are visible neither in `Object.keys` nor in `JSON.stringify`.
→ [Basics](basics.en.md#private-members)

</details>

## 4. 🟢 What happens on `p.#secret` outside the class?

- A) `undefined`
- B) `TypeError`
- C) `SyntaxError`: the program does not start
- D) The field's value

<details><summary>Answer</summary>

**C.** Accessing a private name outside a class is a parse error: `Private field '#secret' must be declared in an enclosing class`.
→ [Basics](basics.en.md#private-members)

</details>

## 5. 🟡 What does the code print?

```js
class Base { constructor() { this.init(); } init() { this.a = 'base'; } }
class Derived extends Base { b = 'field'; init() { this.a = 'derived'; this.seenB = this.b; } }
console.log(new Derived());
```

- A) `Derived { a: 'derived', seenB: 'field', b: 'field' }`
- B) `Derived { a: 'derived', seenB: undefined, b: 'field' }`
- C) `Derived { a: 'base', b: 'field' }`
- D) `ReferenceError`

<details><summary>Answer</summary>

**B.** The descendant's fields are created after `super()`, while `init` was called from the `Base` constructor earlier.
→ [Advanced](advanced.en.md#field-initialization-order)

</details>

## 6. 🟡 What happens on `const { method } = new F(); method();` if `method() { return this.v; }`?

- A) The value of `v` is returned
- B) `TypeError: Cannot read properties of undefined (reading 'v')`
- C) `undefined`
- D) `ReferenceError`

<details><summary>Answer</summary>

**B.** `this` is lost; a class body is strict. The fixes: an arrow field or `bind`.
→ [Advanced](advanced.en.md#losing-this-and-arrow-fields)

</details>

## 7. 🟡 What does `s.map(x => x) instanceof Stack` return for `class Stack extends Array`?

- A) `false`
- B) `true`
- C) `TypeError`
- D) Depends on the length

<details><summary>Answer</summary>

**B.** `map` creates the result through the "species" constructor, that is, `Stack`.
→ [Advanced](advanced.en.md#extending-built-in-classes)

</details>

## 8. 🟡 What are `B.s` and `Object.hasOwn(B, 's')` for `class A { static s = 'A.s' }`, `class B extends A {}`?

- A) `'A.s'` and `true`
- B) `'A.s'` and `false`
- C) `undefined` and `false`
- D) `'A.s'` and `undefined`

<details><summary>Answer</summary>

**B.** Static members are inherited through the chain of constructor prototypes; `B` has no own `s`.
→ [Advanced](advanced.en.md#static-inheritance-and-class-expressions)

</details>

## 9. 🟡 Select all correct statements

- [ ] A) An arrow field is created anew in every instance
- [ ] B) `#x in obj` does not throw for foreign objects
- [ ] C) A class is hoisted like a function declaration
- [ ] D) `new.target` in a constructor shows which class `new` was called on

<details><summary>Answer</summary>

**A, B, D.** C is wrong: before the declaration a class is in the TDZ.
→ [Advanced](advanced.en.md#losing-this-and-arrow-fields)

</details>

## 10. 🔴 Find the flaw

> "A derived constructor may first write `this.x = 1` and call `super()` at the end — the order does not matter."

<details><summary>Answer</summary>

In a derived constructor `this` does not exist until `super(...)` is called: `ReferenceError: Must call super constructor ...`. Call `super` first.
→ [Reference](expert.en.md#constructor-rules)

</details>

## 11. 🔴 What does `X.prototype.m.call(5)` give for a class method in a sloppy `.cjs` file where `m() { return this; }`?

- A) A `Number` wrapper object
- B) The primitive `5`
- C) The global object
- D) `undefined`

<details><summary>Answer</summary>

**B.** A class body is always strict: `this` is neither wrapped nor replaced.
→ [Internals](expert.en.md#internals)

</details>

## 12. 🔴 Open question

Compare classes and factory functions with closures: when to choose which?

<details><summary>Key points of a good answer</summary>

- Classes: clear structure, `instanceof`, inheritance, methods on the prototype (saving memory), private `#` members.
- Factories: true privacy through a closure, no `this` problems, but a copy of methods per object and no `instanceof`.
- The choice depends on the number of objects, the need for inheritance and tooling (TypeScript, frameworks).

</details>

→ [Expert](expert.en.md#trade-offs-and-alternatives)
