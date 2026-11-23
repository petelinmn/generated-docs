---
title: "Classes"
lang: en
track: javascript
kind: module
doc: overview
slug: 06-classes
part: 1
levels: [1, 2, 3, 4]
tags: [classes, inheritance, private-fields, static, getters-setters, super, mixins, new-target, extends]
versions: "ECMAScript 2025+, Node.js 24.15 LTS"
status: draft
updated: 2026-11-23
---

# Classes

🌐 [Русский](overview.ru.md) · **English**

## In short

The `class` syntax is a convenient notation for constructors and prototypes ([module 04](../04-functions/overview.en.md), [module 05](../05-objects-and-prototypes/overview.en.md)). A class describes a constructor, methods (they live on the prototype), getters and setters, static members and instance fields. Private members are written with `#` and are inaccessible from outside. Inheritance is `extends`, and calling the parent is `super`. Under the hood there are still prototypes: `typeof Point` is `'function'`. Unlike a constructor function, a class cannot be called without `new`, and its method bodies run in strict mode.

## Key ideas

- **`class`** is a constructor function with a prototype; calling it without `new` is a `TypeError`.
- **Methods live on the prototype and are non-enumerable;** instance fields are created in the object itself.
- **Getters and setters** define computed properties; **static members** belong to the class, not an instance.
- **Private members `#x`** are accessible only inside the class body; from outside it is a syntax error, not `undefined`.
- **`extends` and `super`:** in a derived constructor you must call `super(...)` before touching `this`.
- **A class is not hoisted like a function:** before the declaration it is in the TDZ ([module 03](../03-scopes-and-hoisting/overview.en.md)).
- **Fields are initialized after `super()`:** a method called from the parent constructor will not see the descendant's fields.
- **A method loses `this` when passed as a value;** an arrow field or `bind` helps.
- **Classes can extend built-ins (`Array`, `Error`)** and "mixins" can be assembled with functions.

## Minimal example

```js
class Point {
  #secret = 42;
  constructor(x, y) { this.x = x; this.y = y; }
  get len() { return Math.hypot(this.x, this.y); }
  reveal() { return this.#secret; }
  toString() { return `(${this.x}, ${this.y})`; }
}
const p = new Point(3, 4);
console.log(p.len, `${p}`, p.reveal(), Object.keys(p));
```

```text
5 (3, 4) 42 [ 'x', 'y' ]
```

`#secret` does not appear in `Object.keys`, and `len` is a getter on the prototype.

## When to use and when not

| Task | Choice |
|---|---|
| Many objects of one kind with behavior | `class` |
| A single object with data | a literal `{ ... }` ([module 05](../05-objects-and-prototypes/overview.en.md)) |
| A simple factory with a closure | a function ([module 04](../04-functions/overview.en.md)) |
| A typed data structure | `class` or `interface` in [TypeScript](../../../typescript/overview.en.md) |
| Extending behavior without inheritance | a mixin or composition |

- ❌ Do not build deep inheritance hierarchies: prefer composition.
- ❌ Do not call overridable methods from a parent constructor.

## Material levels

| Level | What is inside |
|---|---|
| [1. Basic theory](basics.en.md) | class declaration, constructor, methods, getters and setters, private fields, `static`, inheritance and `super` |
| [2. Advanced theory](advanced.en.md) | private methods and `#x in o`, static blocks, field initialization order, losing `this`, extending built-in classes, mixins, `new.target`, `Symbol.hasInstance` |
| [3. Expert theory](expert.en.md) | a reference of class elements, internals (prototypes, `super`, private names), pitfalls, interview questions |
| [4. Self-check](quiz.en.md) | quiz and [exercises](exercises.en.md) |
| 5. Runnable examples | not created yet — `/learn javascript 06-classes level 5` |
| 6. Tests | not created yet — `/learn javascript 06-classes level 6` |

## Related materials

- Prerequisites: [04 · Functions](../04-functions/overview.en.md) (`this`, `new`, `new.target`), [05 · Objects and prototypes](../05-objects-and-prototypes/overview.en.md)
- Next: module 07 · Arrays and collections (in the [track](../../overview.en.md) roadmap)
- Analogues in other tracks: [Python: OOP](../../../python/overview.en.md), [C#: OOP](../../../csharp/overview.en.md)
- Typing classes: [TypeScript](../../../typescript/overview.en.md)
