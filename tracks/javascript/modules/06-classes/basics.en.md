---
title: "Classes: basics"
lang: en
track: javascript
kind: module
doc: basics
slug: 06-classes
part: 1
tags: [classes, constructor, methods, getters-setters, private-fields, static, inheritance, super]
versions: "ECMAScript 2025+, Node.js 24.15 LTS"
status: draft
updated: 2026-11-23
---

# Classes: basics

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [04 · Functions](../04-functions/overview.en.md), [05 · Objects and prototypes](../05-objects-and-prototypes/overview.en.md) · **Time:** ~2.5 h

All examples were run on Node.js 24.15 in an ES module file (strict mode).

## Why it matters

When you need many objects of the same kind with their own state and behavior, it is convenient to describe them with a template. A class puts the constructor, methods and data in one place and hides the internals.

## Declaring a class

```js
class Point {
  static count = 0;
  #secret = 42;
  x = 0;
  constructor(x, y) { this.x = x; this.y = y; Point.count++; }
  get len() { return Math.hypot(this.x, this.y); }
  set len(v) { throw new Error('read-only'); }
  toString() { return `(${this.x}, ${this.y})`; }
  reveal() { return this.#secret; }
  static origin() { return new Point(0, 0); }
}
const p = new Point(3, 4);
console.log(p.len, `${p}`, p.reveal(), Point.count, Point.origin().x);
console.log(typeof Point, Object.getOwnPropertyNames(Point.prototype), Object.keys(p));
try { p.len = 1; } catch (e) { console.log(e.name + ': ' + e.message); }
```

```text
5 (3, 4) 42 1 0
function [ 'constructor', 'len', 'toString', 'reveal' ] [ 'x', 'y' ]
Error: read-only
```

The elements here:

- **`constructor`** is called on `new` and sets the initial state. If a constructor is not needed, it can be omitted.
- **A method** (`toString`, `reveal`) lives on `Point.prototype`, not in each instance.
- **A getter/setter** (`get len`, `set len`) look like a property but call a function. The setter of `len` throws an error: the property is "read-only".
- **An instance field** (`x = 0;`) is created for each object before the constructor body; `x` and `y` appear in `Object.keys(p)`.
- **A private field** `#secret` is accessible only inside the class body and is invisible in `Object.keys`.
- **A static member** (`static count`, `static origin()`) belongs to the class: `Point.count`. `Point.count` in the output is `1`: the arguments of `console.log` are evaluated left to right, and the counter was read before the call `Point.origin()` (which creates the second instance).

A class is a function: `typeof Point` is `'function'`. Class methods are non-enumerable, and they do not show up in `Object.keys(Point.prototype)` (the result is an empty list).

## A class cannot be called without `new`

```js
try { Point(); } catch (e) { console.log(e.name + ': ' + e.message); }
```

```text
TypeError: Class constructor Point cannot be invoked without 'new'
```

A constructor function without `new` silently made `this` global or failed later; a class refuses at once and explicitly.

## A class is not hoisted

```js
try { new Later(); } catch (e) { console.log(e.name + ': ' + e.message); }
class Later {}
```

```text
ReferenceError: Cannot access 'Later' before initialization
```

Like `let`/`const`, a class is in the TDZ before its declaration ([module 03](../03-scopes-and-hoisting/basics.en.md#the-temporal-dead-zone-tdz)).

## Private members

A name with `#` is visible only inside the class body:

```js
const p = new Point(3, 4);
console.log(p['#secret'], Object.getOwnPropertyNames(p), JSON.stringify(p));
```

```text
undefined [ 'x', 'y' ] {"x":3,"y":4}
```

- Accessing `p.#secret` outside the class is a **syntax error** (`SyntaxError: Private field '#secret' must be declared in an enclosing class`): the program will not start at all.
- `p['#secret']` is an ordinary string name that the object does not have.
- Private fields appear neither in `getOwnPropertyNames` nor in `JSON.stringify`.

## Inheritance: extends and super

```js
class Animal {
  constructor(name) { this.name = name; }
  speak() { return `${this.name} makes a sound`; }
  static create(n) { return new this(n); }
  get kind() { return 'animal'; }
}
class Dog extends Animal {
  constructor(name) { super(name); this.tricks = []; }
  speak() { return super.speak() + ' (woof)'; }
  get kind() { return 'dog/' + super.kind; }
}
const d = new Dog('Rex');
console.log(d.speak(), d.kind, d instanceof Animal, Object.getPrototypeOf(Dog) === Animal, Dog.create('Fido') instanceof Dog);
```

```text
Rex makes a sound (woof) dog/animal true true true
```

- `class Dog extends Animal` links the prototypes: `Dog.prototype` inherits `Animal.prototype`, and `Dog` itself inherits `Animal` (static members are inherited: `Dog.create` works, and `new this(n)` creates exactly a `Dog`).
- `super(name)` calls the parent constructor; `super.speak()` and `super.kind` reach the parent's method and getter.
- In a derived constructor `this` is unavailable until `super(...)` is called:

```js
class Bad extends Animal { constructor() { this.x = 1; } }
try { new Bad(); } catch (e) { console.log(e.name + ': ' + e.message); }
```

```text
ReferenceError: Must call super constructor in derived class before accessing 'this' or returning from derived constructor
```

The same error occurs if the derived constructor is empty and does not call `super`.

## Beginner mistakes

- ❌ Calling a class without `new` → ✅ `new Point()`.
- ❌ Using `this` in a derived constructor before `super()` → ✅ `super(...)` on the first line.
- ❌ Accessing `obj.#x` from outside → ✅ a getter or a method.
- ❌ Expecting methods to appear in `Object.keys` → ✅ they are non-enumerable, while fields are enumerable.
- ❌ Putting mutable state in `static` when it belongs in an instance → ✅ an instance field.
- ❌ Building deep `extends` chains → ✅ composition.

## Related materials

- Next: [advanced theory](advanced.en.md) — private methods, field order, built-in classes, mixins
- Earlier: [05 · Objects and prototypes](../05-objects-and-prototypes/overview.en.md), [04 · Functions](../04-functions/overview.en.md#constructor-functions-and-new)

## Sources

- [MDN: Classes](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Classes)
- [MDN: Private elements](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Classes/Private_elements)
- [MDN: extends](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Classes/extends)
- [MDN: static](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Classes/static)
- [ECMAScript: Class Definitions](https://tc39.es/ecma262/#sec-class-definitions)
