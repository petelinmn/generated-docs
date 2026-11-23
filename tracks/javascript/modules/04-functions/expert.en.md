---
title: "Functions: expert"
lang: en
track: javascript
kind: module
doc: expert
slug: 04-functions
part: 1
tags: [this-binding, call-construct, arrow-functions, arguments-object, function-length, new-target, strict-mode]
versions: "ECMAScript 2025+, Node.js 24.15 LTS"
status: draft
updated: 2026-11-23
---

# Functions: expert

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · **3. Expert** · [4. Quiz](quiz.en.md)

Builds on [basic](basics.en.md) and [advanced](advanced.en.md) theory. All results were obtained by running on Node.js 24.15 (an ES module, strict mode) unless stated otherwise.

## Reference

### Function forms

| Form | Hoisting | Own `this` | `arguments` | `new` | `prototype` |
|---|---|---|---|---|---|
| `function f() {}` | entirely | yes (by the way of calling) | yes | yes | present |
| `const f = function () {}` | like `const`/`var` | yes | yes | yes | present |
| `const f = () => {}` | like `const`/`var` | no (lexical) | no (sees the outer one) | no — `TypeError: arrow is not a constructor` | none |
| a method `m() {}` in an object/class | — | yes | yes | no | none |
| `function*`, `async function`, `async () => {}` | as above | yes (an arrow — no) | yes (an arrow — no) | no | a generator has one |

> ⚠️ Verify: the absence of `prototype` and the impossibility of `new` for methods, generators and async functions, as well as for the `async` arrow, come from the specification and were not run (the arrow was checked).

### `this` rules (in order of priority)

| Way of calling | `this` |
|---|---|
| `new F()` | a new object |
| `bound()` after `f.bind(t)` | `t` (cannot be changed by another `call`) |
| `f.call(t)`, `f.apply(t)` | `t` (in strict mode without conversion: `call(5)` → `5`, `call(null)` → `null`) |
| `obj.f()` | `obj` |
| `f()` | `undefined` in strict mode; the global object in sloppy mode |
| an arrow | the outer scope's `this` |

In sloppy code (`.cjs`): `f()` returned the global object, `call(5)` a wrapper object (`typeof this` is `'object'`), `call(null)` the global object, and a detached method `g()` reads `globalThis.name` and returns `undefined` without an error.

### Function properties

| Property | Value |
|---|---|
| `name` | the name from the declaration, the variable, or `bound <name>` for `bind` |
| `length` | the number of parameters before the first one with a default or `...rest` (in the experiment `(a, b = 1, ...c) => {}` has `length` `1`; `bind(null, 5)` of three parameters — `2`) |
| `prototype` | for ordinary functions and classes — the object used by `new` |
| `toString()` | the function's source text |

## Internals

- **Internal methods.** Any function is an object with an internal method `[[Call]]`; constructors also have `[[Construct]]`. Arrows and methods do not, so `new arrow()` is a `TypeError`.
- **`new F(args)`** per the specification creates an object with the prototype `F.prototype`, calls `F` with it as `this`, and returns the call's result if it is an object, otherwise the created object.
- **Lexical `this`.** An arrow does not create its own `this` binding: it looks `this` up along the chain of environments ([module 03](../03-scopes-and-hoisting/expert.en.md#internals)) like an ordinary variable. So `call`/`bind` do not affect it, and `nested()` in `map` sees the method's `this`.
- **The mode is determined by the code, not the call.** A strict function gets `this` as is; a sloppy one substitutes the global object for `undefined`/`null` and wraps primitives.
- **Default values.** Parameters with defaults create a separate scope: expressions are evaluated left to right on every call (in the experiment `f()` increased the counter).
- **The call stack.** Each call adds a frame; on overflow `RangeError: Maximum call stack size exceeded`. V8 has no tail-call optimization (in the experiment `depth(n + 1)` failed with an error).

> ⚠️ Verify: the stack depth in numbers, tail-call optimization in other engines and the details of `[[Construct]]` for derived classes were not checked in the experiments.

## Gotchas

### 1. A detached method loses `this`

`const g = obj.m; g()` — `this` is `undefined` (strict mode): the result is `undefined` with `this?.name`, or `TypeError: Cannot read properties of undefined` with `this.name` (in the experiment on a class). In sloppy mode `this` is the global object, and there is no error, but the value is wrong.

### 2. `map(parseInt)`

`['1','2','3'].map(parseInt)` gives `[1, NaN, NaN]`: the index lands in the second parameter `radix`.

### 3. ASI and `return`

`return` followed by a line break returns `undefined` (in the experiment `asi()`).

### 4. `null` does not trigger a default

`dflt(1, null)` returned `[1, null, 'lazy']`: a default triggers only on `undefined`.

### 5. An arrow as a method

`obj.arrow()` returned `typeof this` equal to `'undefined'`: `this` is taken from the module's environment, not from `obj`.

### 6. A constructor without `new`

In strict mode `Person('x')` is a `TypeError: Cannot set properties of undefined (setting 'name')`. In sloppy mode the same assignment would create a property of the global object.

> ⚠️ Verify: the claim about sloppy mode in gotcha 6 follows from the `this` rules (in sloppy mode `f()` returned the global object), but the creation of a global property by a constructor itself was not run.

### 7. `arguments` and arrows

An arrow has no `arguments` of its own: at the top level of a module it is not defined, and inside an ordinary function the arrow sees the outer function's `arguments` (in the experiment).

### 8. `length` does not equal the number of declared parameters

Parameters with defaults and `...rest` are not counted.

### 9. A binding cannot be changed

`bound.call({ name: 'x' })` still uses the `this` fixed in `bind`.

## Trade-offs and alternatives

| Approach | Pros | Cons |
|---|---|---|
| An arrow | lexical `this`, short | no own `this`/`arguments`, not a constructor |
| A method `m() {}` | own `this`, a name, `super` works | loses `this` when passed around |
| `bind` | explicit fixing | creates a new function, cannot be rebound |
| A class field with an arrow | `this` is always the instance | a copy per instance (module 06) |
| A `class` instead of a constructor function | protection from calling without `new` | syntax, inheritance through `extends` |
| A functional style without `this` | easier to test | a different code style |

## Interview questions

<details><summary>1. How do a function declaration, a function expression and an arrow differ?</summary>

A declaration is hoisted entirely; an expression and an arrow are variable values; an arrow has no `this` or `arguments` of its own and is not a constructor.

→ [Basic theory](basics.en.md#three-forms-of-functions)

</details>

<details><summary>2. How is <code>this</code> determined?</summary>

By the way of calling: `new`, `bind`, `call`/`apply`, an object method, a plain call. For an arrow — from the outer scope.

→ [Reference](#this-rules-in-order-of-priority)

</details>

<details><summary>3. Trick: why does a method passed as a callback "lose" <code>this</code>?</summary>

The function is called without an object, so `this` is `undefined` (strict mode) or the global object. The fix: `bind` or an arrow wrapper.

→ [Gotcha 1](#1-a-detached-method-loses-this)

</details>

<details><summary>4. What do <code>call</code>, <code>apply</code> and <code>bind</code> do?</summary>

`call`/`apply` call a function with a given `this` (arguments as a list or an array), `bind` returns a new function with `this` fixed and possibly the first arguments.

→ [Advanced theory](advanced.en.md#call-apply-and-bind)

</details>

<details><summary>5. Trick: what does <code>['1','2','3'].map(parseInt)</code> return?</summary>

`[1, NaN, NaN]`: `map` passes the index as the second argument (`radix`).

→ [Gotcha 2](#2-mapparseint)

</details>

<details><summary>6. Why is <code>...rest</code> better than <code>arguments</code>?</summary>

It is a real array, works in arrows, and is explicit in the signature.

→ [Basic theory](basics.en.md#rest-parameters-and-arguments)

</details>

<details><summary>7. When does a parameter's default value trigger?</summary>

Only for `undefined` (including when the argument is not passed); `null` is not replaced. It is evaluated on every call.

→ [Gotcha 4](#4-null-does-not-trigger-a-default)

</details>

<details><summary>8. What does <code>new</code> do?</summary>

It creates an object with the prototype `F.prototype`, calls `F` with that `this`, and returns the object (or an explicitly returned object).

→ [Internals](#internals)

</details>

<details><summary>9. Trick: what does <code>function f() { return\n 42 }</code> return?</summary>

`undefined`: ASI inserts `;` after `return`.

→ [Gotcha 3](#3-asi-and-return)

</details>

<details><summary>10. Why can an arrow not be used with <code>new</code>?</summary>

It has no internal `[[Construct]]` method and no `prototype`: `TypeError: arrow is not a constructor`.

→ [Advanced theory](advanced.en.md#limitations-of-arrow-functions)

</details>

<details><summary>11. What is <code>new.target</code>?</summary>

A value inside a function showing whether it was called with `new` (otherwise `undefined`); used to guard against calling without `new` and for abstract classes.

→ [Advanced theory](advanced.en.md#newtarget)

</details>

<details><summary>12. How does <code>this</code> behave differently in strict and sloppy mode?</summary>

In strict `f()` gives `undefined`, `call(5)` gives `5`, `call(null)` gives `null`; in sloppy — the global object and a wrapper.

→ [Reference](#this-rules-in-order-of-priority)

</details>

## Cheat sheet

```text
function f() {}  const f = function () {}  const f = () => {}
f.call(t, a, b)  f.apply(t, [a, b])  const g = f.bind(t, a)
f(...args)  function (a, b = 1, ...rest)
this: new > bind > call/apply > obj.f() > f() (undefined in strict) ; an arrow — the outer this
new.target ; f.name ; f.length (without default and rest)
```

- A method as a callback → `bind` or an arrow.
- `return` and the value — on one line.
- An arrow is not a constructor and has no own `this`.

## Related materials

- Previous level: [advanced theory](advanced.en.md)
- Next module: [05 · Objects and prototypes](../05-objects-and-prototypes/overview.en.md)
- Scopes: [03 · Variables and scopes](../03-scopes-and-hoisting/overview.en.md)

## Sources

- [ECMAScript: Function Objects](https://tc39.es/ecma262/#sec-function-objects)
- [ECMAScript: ECMAScript Function Objects ([[Call]], [[Construct]])](https://tc39.es/ecma262/#sec-ecmascript-function-objects)
- [MDN: this](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Operators/this)
- [MDN: Function.length](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Function/length)
- [MDN: Strict mode](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Strict_mode)
