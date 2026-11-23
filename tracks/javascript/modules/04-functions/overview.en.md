---
title: "Functions"
lang: en
track: javascript
kind: module
doc: overview
slug: 04-functions
part: 1
levels: [1, 2, 3, 4]
tags: [functions, arrow-functions, this, call-apply-bind, default-parameters, rest-parameters, arguments, new-target, recursion]
versions: "ECMAScript 2025+, Node.js 24.15 LTS"
status: draft
updated: 2026-11-23
---

# Functions

🌐 [Русский](overview.ru.md) · **English**

## In short

Functions in JavaScript are full-fledged values: they can be assigned to variables, passed to other functions and returned. There are three main forms: a **declaration** (`function f() {}`), an **expression** (`const f = function () {}`) and an **arrow function** (`const f = () => {}`). The main difficulty is the keyword **`this`**: in ordinary functions it depends on *how* the function was called, while in arrows it is taken from the surrounding code. Parameters can have default values and collect the rest of the arguments (`...rest`), extra arguments are ignored, and missing ones are `undefined`.

## Key ideas

- **Three forms:** a declaration (hoisted entirely), an expression and an arrow (hoisted as variables).
- **Arguments are not checked:** extra ones are dropped, missing ones are `undefined`.
- **Default values are evaluated on every call** and only trigger on `undefined` (not on `null`).
- **`...rest` collects the remaining arguments into a real array,** while `arguments` is a legacy array-like (arrows have none of their own).
- **`this` is set by the way of calling:** `obj.method()` — `obj`; a plain call `f()` — `undefined` (in strict mode); `call`/`apply`/`bind` set it explicitly; `new` creates an object.
- **An arrow function has no `this` or `arguments` of its own and cannot be a constructor.**
- **A method detached from its object loses `this`:** `const g = obj.m; g()`.
- **A function without `return` returns `undefined`;** a line break after `return` breaks the code.
- **Deep recursion overflows the call stack** (`RangeError`).

## Minimal example

```js
function add(a, b = 10, ...rest) { return [a + b, rest]; }
const double = (n) => n * 2;
console.log(add(1), add(1, 2, 3, 4), [1, 2, 3].map(double));
```

```text
[ 11, [] ] [ 3, [ 3, 4 ] ] [ 2, 4, 6 ]
```

## When to use and when not

| Task | Form |
|---|---|
| A named helper function | a declaration |
| A short callback (`map`, `filter`) | an arrow |
| An object or class method that needs `this` | the shorthand method `m() {}` |
| A constructor "the old way" | `function Person()` + `new` (today usually `class`) |
| Bind `this` or part of the arguments | `bind` |

- ❌ Do not use an arrow as an object method if you need that object's `this`.
- ❌ Do not pass a method as a callback without binding `this`.

## Material levels

| Level | What is inside |
|---|---|
| [1. Basic theory](basics.en.md) | function forms, parameters and arguments, returning a value, `this`, functions as values, recursion |
| [2. Advanced theory](advanced.en.md) | `call`/`apply`/`bind`, partial application, arrow limitations, constructor functions and `new`, `new.target`, function properties |
| [3. Expert theory](expert.en.md) | a reference of forms and `this` rules, how calling and constructing work, pitfalls, interview questions |
| [4. Self-check](quiz.en.md) | quiz and [exercises](exercises.en.md) |
| 5. Runnable examples | not created yet — `/learn javascript 04-functions level 5` |
| 6. Tests | not created yet — `/learn javascript 04-functions level 6` |

## Related materials

- Prerequisites: [02 · Types and values](../02-types-and-values/overview.en.md), [03 · Variables and scopes](../03-scopes-and-hoisting/overview.en.md) (hoisting, closures)
- Next: [05 · Objects and prototypes](../05-objects-and-prototypes/overview.en.md)
- Closures and functional style in depth: module 10 of the [track](../../overview.en.md) roadmap
- Analogues in other tracks: [Python: functions](../../../python/overview.en.md), [C#: methods and delegates](../../../csharp/overview.en.md)
- Typing functions: [TypeScript](../../../typescript/overview.en.md)
