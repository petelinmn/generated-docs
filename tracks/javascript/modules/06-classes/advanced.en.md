---
title: "Classes: advanced"
lang: en
track: javascript
kind: module
doc: advanced
slug: 06-classes
part: 1
tags: [private-methods, static-blocks, field-initialization, this-binding, extending-builtins, mixins, new-target, symbol-hasinstance]
versions: "ECMAScript 2025+, Node.js 24.15 LTS"
status: draft
updated: 2026-11-23
---

# Classes: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

Builds on [basic theory](basics.en.md). All examples were run on Node.js 24.15 in an ES module file (strict mode).

## Private methods, static private members and blocks

```js
class Point {
  static count = 0;
  #secret = 42;
  constructor(x, y) { this.x = x; this.y = y; Point.count++; }
  #priv() { return 'p'; }
  callPriv() { return this.#priv(); }
  static #sp = 'static private';
  static getSp() { return Point.#sp; }
  static { Point.initialized = true; }
  has(o) { return #secret in o; }
}
const p = new Point(3, 4);
console.log(p.callPriv(), Point.getSp(), Point.initialized, p.has(p), p.has({}));
```

```text
p static private true true false
```

- `#priv()` is a private method; `static #sp` is a private static field (accessed through the class name inside its body).
- `static { ... }` is a **static block**: code that runs once when the class is created; useful for complex initialization of static fields.
- `#secret in o` is a **brand check**: true if the object was created by this class and has the private field; unlike accessing `o.#secret`, it does not throw for foreign objects.

Accessing `p.#secret` outside the class does not run because of a syntax error: `SyntaxError: Private field '#secret' must be declared in an enclosing class`.

## Field initialization order

A descendant's fields are created **after** `super()` returns. This is a trap if the parent constructor calls an overridable method:

```js
class Base { constructor() { this.init(); } init() { this.a = 'base'; } }
class Derived extends Base {
  b = 'field';
  init() { this.a = 'derived'; this.seenB = this.b; }
}
console.log(new Derived());
```

```text
Derived { a: 'derived', seenB: undefined, b: 'field' }
```

The descendant's `init` was called from the `Base` constructor while the field `b` was not created yet, so `this.b` is `undefined`. Do not call overridable methods from a constructor.

## Losing `this` and arrow fields

Methods live on the prototype, not in the instance, so they lose `this` when passed as values:

```js
class F { arrow = () => this.v; v = 1; method() { return this.v; } }
const f = new F();
const { arrow, method } = f;
console.log(arrow(), Object.hasOwn(f, 'arrow'), Object.hasOwn(f, 'method'));
try { method(); } catch (e) { console.log(e.name + ': ' + e.message); }
class C { constructor() { this.onClick = this.onClick.bind(this); } onClick() { return this instanceof C; } }
const { onClick } = new C();
console.log(onClick());
```

```text
1 true false
TypeError: Cannot read properties of undefined (reading 'v')
true
```

- **An arrow field** (`arrow = () => this.v`) always sees the instance, but is created anew in **every** object (`hasOwn(f, 'arrow')` is `true`), while a method is not.
- **`bind` in the constructor** is another way: an own property copy bound to the instance.
- For event handlers and callbacks use one of these techniques.

## Extending built-in classes

```js
class Stack extends Array { peek() { return this.at(-1); } }
const s = Stack.from([1, 2, 3]);
console.log(s.peek(), s instanceof Array, s.map(x => x) instanceof Stack, s.length, Array.isArray(s));
class MyErr extends Error { constructor(m, code) { super(m); this.name = 'MyErr'; this.code = code; } }
const e = new MyErr('boom', 7);
console.log(e instanceof Error, e.name, e.message, e.code, String(e));
```

```text
3 true true 3 true
true MyErr boom 7 MyErr: boom
```

- A subclass of `Array` gets all the array methods, and `map` returns an instance of `Stack` (the "species" constructor is used).
- A custom error is a descendant of `Error`: call `super(message)`, set `name` and extra fields.

## Mixins

When you need to add behavior without inheritance, use a function that returns a class:

```js
const Serializable = (B) => class extends B { toJSON() { return { ...this }; } };
class User { constructor(n) { this.n = n; } }
class SU extends Serializable(User) {}
console.log(JSON.stringify(new SU('a')), SU.name);
```

```text
{"n":"a"} SU
```

`Serializable(User)` creates an intermediate class between `SU` and `User`. This technique allows multiple "mixing in", which `extends` (a single parent) lacks.

## new.target and abstract classes

```js
class Abs { constructor() { if (new.target === Abs) throw new TypeError('abstract'); } }
try { new Abs(); } catch (e) { console.log(e.name + ': ' + e.message); }
console.log(new (class Impl extends Abs {})() instanceof Abs);
```

```text
TypeError: abstract
true
```

In a constructor `new.target` is the class `new` was called on. This is how direct creation of an "abstract" base class is forbidden ([module 04](../04-functions/advanced.en.md#newtarget)).

## Customizing `instanceof` and `toString`

```js
class Even { static [Symbol.hasInstance](n) { return n % 2 === 0; } }
console.log(2 instanceof Even, 3 instanceof Even);
class Tagged { get [Symbol.toStringTag]() { return 'T'; } }
console.log(String(new Tagged()));
```

```text
true false
[object T]
```

`Symbol.hasInstance` overrides `instanceof`, and `Symbol.toStringTag` the result of `Object.prototype.toString` and `String(obj)`. These are "well-known symbols" (metaprogramming, module 18 of the roadmap).

## Static inheritance and class expressions

```js
class A { static s = 'A.s'; static who() { return this.name; } }
class B extends A {}
console.log(B.s, B.who(), Object.hasOwn(B, 's'));
const E = class Named {};
console.log(E.name, typeof Named);
```

```text
A.s B false
Named undefined
```

- Static members are inherited through the prototype of constructors: `B.s` is read from `A`, and `B` has no own `s`. Inside a static method `this` is the class it was called on (`B.who()` is `'B'`).
- For a class expression the name `Named` is visible only inside the class; outside, `typeof Named` is `'undefined'`, and the variable's name is `E`.

## Best practices

- Prefer composition and small hierarchies (one or two levels).
- Make state private (`#`), expose it through methods and getters.
- Do not call overridable methods from a constructor.
- For handlers — an arrow field or `bind`.
- Abstractness through `new.target`; for interfaces — convention or TypeScript.
- Errors — descendants of `Error` with a `name`/`code` field.

## Common mistakes

- ❌ Calling an overridable method in the parent constructor → ✅ move the initialization into an explicit method.
- ❌ Passing a method as a callback without binding → ✅ an arrow field or `bind`.
- ❌ Accessing another object's `#x` without a check → ✅ `#x in obj`.
- ❌ Expecting a copy of a static field on a descendant → ✅ it is shared and read through the chain.
- ❌ Building multiple inheritance → ✅ mixins or composition.

## Related materials

- Next: [expert theory](expert.en.md)
- `this`, `bind`, `new.target`: [04 · Functions](../04-functions/advanced.en.md)
- Prototypes: [05 · Objects and prototypes](../05-objects-and-prototypes/advanced.en.md)

## Sources

- [MDN: Class static initialization blocks](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Classes/Static_initialization_blocks)
- [MDN: Private elements](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Classes/Private_elements)
- [MDN: Mix-ins](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Classes/extends#mix-ins)
- [MDN: Symbol.hasInstance](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Symbol/hasInstance)
- [MDN: Symbol.toStringTag](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Symbol/toStringTag)
