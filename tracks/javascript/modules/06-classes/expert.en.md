---
title: "Classes: expert"
lang: en
track: javascript
kind: module
doc: expert
slug: 06-classes
part: 1
tags: [class-elements, constructor, super, private-names, field-initialization, derived-constructor, strict-mode, prototype-chain]
versions: "ECMAScript 2025+, Node.js 24.15 LTS"
status: draft
updated: 2026-11-23
---

# Classes: expert

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · **3. Expert** · [4. Quiz](quiz.en.md)

Builds on [basic](basics.en.md) and [advanced](advanced.en.md) theory. All results were obtained by running on Node.js 24.15 (an ES module; where stated, a sloppy `.cjs` file).

## Reference

### Elements of a class body

| Element | Syntax | Where it lives | Enumerable |
|---|---|---|---|
| constructor | `constructor(...) {}` | the class itself (a function) | — |
| method | `m() {}` | `Class.prototype` | no |
| getter/setter | `get x() {}` / `set x(v) {}` | `Class.prototype` | no |
| instance field | `x = 1;` | each instance | yes |
| private field/method | `#x = 1;` / `#m() {}` | the instance (hidden) | not visible in keys |
| static method/field | `static m() {}` / `static s = 1;` | the class itself | a field — yes, a method — no |
| static block | `static { ... }` | runs when the class is created | — |
| private static | `static #x` | the class itself (hidden) | not visible |
| computed name | `[expr]() {}` | like an ordinary member | — |

> ⚠️ Verify: the `accessor` keyword (auto-accessors, decorators) and computed names with generators/`async` were not run in the experiments.

### Constructor rules

| Case | Result |
|---|---|
| Calling a class without `new` | `TypeError: Class constructor Point cannot be invoked without 'new'` |
| `this` in a derived constructor before `super()` | `ReferenceError: Must call super constructor in derived class before accessing 'this' or returning from derived constructor` |
| An empty derived constructor | the same `ReferenceError` on `new` |
| `class Y extends null` | `TypeError: Super constructor null of Y is not a constructor` on `new Y()` |
| A constructor returns an object | `new` returns that object (`{ custom: true }`); the descendant's fields are added to it |
| A constructor returns a primitive | ignored (the standard rule of `new`, [module 04](../04-functions/expert.en.md#internals)) |

> ⚠️ Verify: returning a primitive from a class constructor (only returning an object was checked in the experiment).

## Internals

- **A class is a function.** `typeof Point` is `'function'`; it has `[[Construct]]`, and the `prototype` property is **not writable** (in the experiment `writable: false`, for an ordinary function — `true`). `Object.getPrototypeOf(X) === Function.prototype` (in the experiment), and for a descendant it is the parent class: `Object.getPrototypeOf(Z) === X`, which is why static members are inherited.
- **Composition.** `Object.getOwnPropertyNames(class { static a = 1; static m() {} })` returned `[ 'length', 'name', 'prototype', 'm', 'a' ]`: a static method is created at definition, a static field at initialization (later).
- **A class body is always strict.** Even in a sloppy `.cjs`, a method called as `m()` gets `this === undefined`, and `m.call(5)` gets the primitive `5` (in the experiment).
- **Methods are non-enumerable.** The descriptor of a method on the prototype has `enumerable: false` (in the experiment `toString`), so `for...in` and `Object.keys` do not see them.
- **The order of creating an instance (`new Derived()`).** The base class constructor creates the object and initializes **its** fields; then control returns to the derived constructor after `super()`, and only then are the descendant's fields initialized. So from the `Base` constructor the descendant's field `b` is not visible (in the experiment `seenB: undefined`). For a base class the fields are initialized before the constructor body (`ctor sees 1`).
- **`super`.** `super.method()` looks up the method in the prototype of the object where the current method is defined (the "home object") but calls it with the current `this`. That is why `super.kind` in a descendant's getter reads the parent's getter on the same instance.
- **Private names.** `#x` is not a string property but an entry in a separate "private" table of the instance; access outside the class body is forbidden at compile time (`SyntaxError`). `#x in o` checks for such an entry (a brand check, in the experiment `true`/`false`).
- **A class expression.** The name `Named` of `const E = class Named {}` is bound only inside the class; `E.name` is `'Named'`, outside `typeof Named` is `'undefined'` (in the experiment).

> ⚠️ Verify: the exact structure of internal slots (`[[HomeObject]]`, `[[Fields]]`, private environments) and V8's optimizations for private fields come from the specification and were not checked in the experiments.

## Gotchas

### 1. The parent constructor calls an overridable method

The descendant's fields are not created yet: `this.b` is `undefined` (in the experiment).

### 2. A method loses `this`

`const { method } = f; method()` — `TypeError: Cannot read properties of undefined (reading 'v')` (in the experiment). Even in sloppy code the class body is strict, so `this` is not replaced by the global object.

### 3. An arrow field is created on every instance

`Object.hasOwn(f, 'arrow')` is `true`, for a method — `false`; with millions of instances this costs memory.

### 4. Static members are inherited by reference

`B.s` reads `A.s` (`Object.hasOwn(B, 's')` is `false`); the write `B.s = 1` creates an own property on `B` and hides the parent's.

### 5. A `class` is not hoisted

Before the declaration — `ReferenceError: Cannot access 'Later' before initialization` (in the experiment).

### 6. You cannot access a private name from outside

Not `undefined` but a `SyntaxError` at file parse time: the program will not start.

### 7. `Array` and `map`

For `Stack extends Array` the method `map` returns a `Stack` (in the experiment `true`): "species" constructors are inherited too; with a non-standard subclass constructor this can break code.

### 8. An error without `name`

A subclass of `Error` without its own `name` prints as `Error: ...`. Set `this.name` (in the experiment `MyErr: boom`).

### 9. Returning an object from a constructor

It replaces the result of `new` (`{ custom: true }`); `instanceof` does not work for it.

## Trade-offs and alternatives

| Approach | Pros | Cons |
|---|---|---|
| `class` | clear syntax, private members, `extends`/`super` | `this` and the field order need care |
| Factory functions and closures | real privacy, no `this` | no `instanceof`, a copy of methods per object |
| Literals and `Object.create` | minimal syntax | no privacy and `super` |
| Mixins | reuse without a deep hierarchy | harder to debug the chain |
| Composition | flexibility | more glue code |
| TypeScript | modifiers, `abstract`, `implements` | types are erased; a different level of checking |

## Interview questions

<details><summary>1. How does a class differ from a constructor function?</summary>

A class cannot be called without `new`, its body is strict, methods are non-enumerable, there are `extends`/`super`, private members, and `prototype` is not writable; in essence it is the same prototype model.

→ [Internals](#internals)

</details>

<details><summary>2. Where do class methods and instance fields live?</summary>

Methods — on `Class.prototype`; fields — in the instances themselves.

→ [Reference](#elements-of-a-class-body)

</details>

<details><summary>3. What are private fields and how do they differ from the <code>_name</code> convention?</summary>

`#x` are inaccessible from outside at the language level (a parse error) and do not appear in keys and `JSON`; `_name` is just a convention.

→ [Basic theory](basics.en.md#private-members)

</details>

<details><summary>4. Trick: what happens if a derived constructor touches <code>this</code> before <code>super()</code>?</summary>

`ReferenceError: Must call super constructor ...`.

→ [Reference](#constructor-rules)

</details>

<details><summary>5. In what order are fields initialized under inheritance?</summary>

The base class's fields — before its constructor body; the descendant's fields — after `super()` returns.

→ [Internals](#internals)

</details>

<details><summary>6. Trick: why does a method passed as a callback break?</summary>

It is detached from the instance, and `this` becomes `undefined` (a class body is strict). The fix: an arrow field or `bind`.

→ [Gotcha 2](#2-a-method-loses-this)

</details>

<details><summary>7. How do you make an abstract class?</summary>

In the constructor check `new.target === Abs` and throw an error.

→ [Advanced theory](advanced.en.md#newtarget-and-abstract-classes)

</details>

<details><summary>8. How does <code>super</code> work in a method and in a constructor?</summary>

In a constructor `super(...)` calls the parent constructor and creates `this`; in a method `super.m()` calls the parent prototype's method with the current `this`.

→ [Basic theory](basics.en.md#inheritance-extends-and-super)

</details>

<details><summary>9. Are static members inherited?</summary>

Yes, through the chain of constructor prototypes; `this` in a static method is the class it was called on.

→ [Advanced theory](advanced.en.md#static-inheritance-and-class-expressions)

</details>

<details><summary>10. How do you add behavior without inheritance?</summary>

A mixin — a function that returns a class (`class extends B`) — or composition.

→ [Advanced theory](advanced.en.md#mixins)

</details>

<details><summary>11. What does <code>#x in obj</code> do?</summary>

It checks that the object has the private field `#x` (created by this class), without throwing for foreign objects.

→ [Advanced theory](advanced.en.md#private-methods-static-private-members-and-blocks)

</details>

<details><summary>12. How do you write a custom error?</summary>

`class MyErr extends Error { constructor(m, code) { super(m); this.name = 'MyErr'; this.code = code; } }`.

→ [Advanced theory](advanced.en.md#extending-built-in-classes)

</details>

## Cheat sheet

```text
class A { #p = 1; x = 0; static s = 0; static { … }
  constructor() {} get g() {} set g(v) {} m() {} static sm() {} #pm() {} }
class B extends A { constructor() { super(); } m() { return super.m(); } }
new A() is required ; this after super() ; #p in o ; new.target
```

- Methods on the prototype (non-enumerable), fields in the instance.
- The descendant's fields — after `super()`.
- A method as a callback → an arrow field or `bind`.
- Classes are strict; not hoisted.

## Related materials

- Previous level: [advanced theory](advanced.en.md)
- Functions and `this`: [04 · Functions](../04-functions/overview.en.md)
- Objects and prototypes: [05 · Objects and prototypes](../05-objects-and-prototypes/overview.en.md)
- Metaprogramming and decorators — module 18 of the [track](../../overview.en.md) roadmap and TypeScript ([decorators](../../../typescript/overview.en.md))

## Sources

- [ECMAScript: Class Definitions](https://tc39.es/ecma262/#sec-class-definitions)
- [ECMAScript: Runtime Semantics: ClassDefinitionEvaluation](https://tc39.es/ecma262/#sec-runtime-semantics-classdefinitionevaluation)
- [MDN: Classes](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Classes)
- [MDN: super](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Operators/super)
- [MDN: Public class fields](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Classes/Public_class_fields)
