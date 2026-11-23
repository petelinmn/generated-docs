---
title: "Functions: advanced"
lang: en
track: javascript
kind: module
doc: advanced
slug: 04-functions
part: 1
tags: [call, apply, bind, partial-application, arrow-functions, constructor-function, new, new-target, function-properties]
versions: "ECMAScript 2025+, Node.js 24.15 LTS"
status: draft
updated: 2026-11-23
---

# Functions: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

Builds on [basic theory](basics.en.md). All examples were run on Node.js 24.15 in an ES module file; where stated, a sloppy `.cjs` was used.

## call, apply and bind

These function methods let you set `this` explicitly:

```js
const obj = { name: 'obj', m() { return this?.name; } };
function strictThis() { return this; }
console.log(strictThis(), strictThis.call(5), strictThis.call(null));
console.log(obj.m.call({ name: 'other' }), obj.m.apply({ name: 'applied' }, []));
const bound = obj.m.bind({ name: 'bound' });
console.log(bound(), bound.call({ name: 'x' }), bound.name, bound.length);
```

```text
undefined 5 null
other applied
bound bound bound m 0
```

- `f.call(thisArg, a, b)` calls `f` at once, the arguments are listed; `f.apply(thisArg, [a, b])` is the same but with the arguments as an array (today `f(...args)` is written more often).
- `f.bind(thisArg, …)` **returns a new function** with `this` fixed.
- In strict mode `this` is not wrapped: `strictThis.call(5)` returned the primitive `5`, and `call(null)` returned `null`. In sloppy `.cjs`: `call(5)` gives a wrapper object (`typeof this` is `'object'`), and `call(null)` gives the global object.
- A bound function cannot be rebound: `bound.call({ name: 'x' })` still returned `'bound'`. The name of such a function is `'bound m'`.

### Partial application

`bind` fixes the first arguments too:

```js
function add(a, b, c) { return a + b + c; }
const add5 = add.bind(null, 5);
console.log(add5(1, 2), add5.length);
```

```text
8 2
```

`add5` is a function with the two remaining parameters (`length` is `2`).

## Limitations of arrow functions

An arrow has no `this`, `arguments` or `new.target` of its own and cannot be a constructor:

```js
const arrow = (a, b) => a + b;
try { new arrow() } catch (e) { console.log(e.name + ': ' + e.message) }
```

```text
TypeError: arrow is not a constructor
```

- `call`, `apply` and `bind` do not change an arrow's `this`: it is taken from where it is defined.
- So arrows are good for callbacks inside methods (`this` is kept), while object methods are better written as `m() {}`.

## Constructor functions and `new`

Before `class` appeared, objects were created by functions called with `new`:

```js
function Person(name) { this.name = name; }
Person.prototype.hi = function () { return 'hi ' + this.name; };
const p = new Person('Ann');
console.log(p.hi(), p instanceof Person, p.constructor === Person);
try { Person('x') } catch (e) { console.log(e.name + ': ' + e.message) }
```

```text
hi Ann true true
TypeError: Cannot set properties of undefined (setting 'name')
```

`new Person('Ann')` creates an empty object, links it to `Person.prototype`, calls the function with `this` equal to that object and returns it. Without `new` in strict mode `this` is `undefined`, and the assignment fails. Prototypes are covered in [module 05](../05-objects-and-prototypes/overview.en.md), and the `class` syntax in module 06 of the roadmap.

### new.target

Inside a function `new.target` shows whether it was called with `new`:

```js
function NT() { return new.target === undefined ? 'call' : 'new'; }
console.log(NT(), new NT() instanceof NT);
```

```text
call true
```

(`new NT()` returns an object, because a function called with `new` returns `this` unless it returns another object.)

## Functions are objects

A function has properties, and you can add more:

```js
function decl(a, b) { return a + b; }
decl.custom = 1;
console.log(decl.custom, Object.keys(decl), typeof decl.call, decl.toString().slice(0, 30));
console.log(Object.prototype.toString.call(function* () {}), Object.prototype.toString.call(async () => {}), (async () => {}).constructor.name);
```

```text
1 [ 'custom' ] function function decl(a, b) { return a
[object GeneratorFunction] [object AsyncFunction] AsyncFunction
```

`toString()` returns the function's source text. Generator and async functions are separate kinds (covered in modules 11 and 12 of the roadmap).

## Calling with spread

`...` expands an array into arguments:

```js
console.log(Math.max(...[1, 5, 3]), Math.max.apply(null, [1, 5, 3]));
```

```text
5 5
```

## Closures: independent state

Each call of a factory creates its own environment:

```js
const counter = () => { let c = 0; return { inc: () => ++c, get: () => c }; };
const c1 = counter(), c2 = counter(); c1.inc(); c1.inc(); c2.inc();
console.log(c1.get(), c2.get());
```

```text
2 1
```

## Best practices

- Named helper functions as declarations; callbacks as arrows.
- Do not rely on `arguments`: `...rest` and parameter destructuring are clearer.
- When passing a method as a callback, bind `this` (`bind` or an arrow wrapper).
- Default values instead of `undefined` checks inside.
- Write constructors as a `class` (module 06).
- To check a call with `new` explicitly, use `new.target`.

## Common mistakes

- ❌ `bound.call(other)` hoping to change `this` → ✅ `bind` fixes `this` for good.
- ❌ `new` on an arrow → ✅ an ordinary function or a `class`.
- ❌ Calling a constructor without `new` → ✅ `new`, a `new.target` check, or a `class` (it throws by itself).
- ❌ `Math.max.apply` with a huge array → ✅ there is a limit on the number of arguments; use a loop or `reduce`.
- ❌ Expecting `call(null)` in strict mode to give the global object → ✅ it is `null`.

> ⚠️ Verify: the limit on the number of arguments for `apply`/spread and the behavior of `Function.prototype.toString` for built-in functions were not checked in the experiments.

## Related materials

- Next: [expert theory](expert.en.md)
- Prototypes and `new`: [05 · Objects and prototypes](../05-objects-and-prototypes/overview.en.md)
- Closures: [03 · Variables and scopes](../03-scopes-and-hoisting/advanced.en.md#the-scope-chain)

## Sources

- [MDN: Function.prototype.bind()](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Function/bind)
- [MDN: Function.prototype.call()](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Function/call)
- [MDN: new operator](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Operators/new)
- [MDN: new.target](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Operators/new.target)
- [MDN: Function](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Function)
