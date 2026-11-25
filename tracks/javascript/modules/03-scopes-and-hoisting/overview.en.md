---
title: "Variables and scopes"
lang: en
track: javascript
kind: module
doc: overview
slug: 03-scopes-and-hoisting
part: 1
levels: [1, 2, 3, 4]
tags: [scope, hoisting, tdz, let, const, var, closure, block-scope, shadowing, strict-mode]
versions: "ECMAScript 2025+, Node.js 24.15 LTS"
status: draft
updated: 2026-11-23
---

# Variables and scopes

🌐 [Русский](overview.ru.md) · **English**

## In short

Variables in JavaScript are declared in three ways: `const` (cannot be reassigned), `let` (can) and the legacy `var`. They differ in **scope**: `let` and `const` live within a block `{ … }`, while `var` lives within the whole function. Declarations are **hoisted**: the engine knows about a variable before the line with its declaration runs. Until assigned, `var` holds `undefined`, while `let`/`const` and classes have a **temporal dead zone (TDZ)**: touching them before the declaration raises a `ReferenceError`. Because of the difference in scopes, the classic trap is closures in a loop with `var`.

## Key ideas

- **Use `const` by default,** `let` when the value must change, and do not use `var`.
- **`const` forbids reassignment, not mutation of the content:** an array or object declared with `const` can be changed.
- **Block scope:** `let`/`const` variables are not visible outside `{ }`; `var` "leaks" out of a block and a loop.
- **Hoisting:** `var` and function declarations (`function f() {}`) are available from the start of the scope; `var` holds `undefined` until assigned.
- **TDZ:** `let`, `const` and `class` exist from the start of the block, but cannot be accessed before the declaration.
- **Shadowing:** an inner variable with the same name hides the outer one.
- **The scope chain:** a name is looked up from the inside out; this is the basis of closures.
- **Top-level variables of a module file do not become global:** they are module variables.

## Minimal example

```js
const fv = [];
for (var a = 0; a < 3; a++) fv.push(() => a);
const fl = [];
for (let b = 0; b < 3; b++) fl.push(() => b);
console.log(fv.map(f => f()), fl.map(f => f()));
```

```text
[ 3, 3, 3 ] [ 0, 1, 2 ]
```

With `var` there is one variable for the whole loop, with `let` a new one on every iteration.

## When to use and when not

| Task | Choice |
|---|---|
| The value does not change | `const` |
| A counter, an accumulator | `let` |
| A variable in old code without blocks | `var` — only to maintain old code |
| Hide internal data | a function and a closure (module 10 of the roadmap) |

- ❌ Do not rely on `var` hoisting: declare and read a variable before using it.
- ❌ Do not treat `const` as an "immutable value": that needs `Object.freeze` (and it is shallow).

## Material levels

| Level | What is inside |
|---|---|
| [1. Basic theory](basics.en.md) | `var`/`let`/`const`, hoisting, the TDZ, `const` and mutability, block scope, closures in a loop, redeclaration, shadowing |
| [2. Advanced theory](advanced.en.md) | the scope chain, module and global scope, functions in blocks, the parameter scope, `switch`, `catch`, IIFE |
| [3. Expert theory](expert.en.md) | a reference of declaration forms, environments and execution contexts, pitfalls, interview questions |
| [4. Self-check](quiz.en.md) | quiz and [exercises](exercises.en.md) |
| 5. Runnable examples | not created yet — `/learn javascript 03-scopes-and-hoisting level 5` |
| 6. Tests | not created yet — `/learn javascript 03-scopes-and-hoisting level 6` |

## Related materials

- Prerequisites: [01 · Environment and tools](../01-environment/overview.en.md) (strict mode), [02 · Types and values](../02-types-and-values/overview.en.md)
- Next: [04 · Functions](../04-functions/overview.en.md)
- Closures in depth: [10 · Closures and functional style](../10-closures-and-functional/overview.en.md)
- Analogues in other tracks: [Python: control flow and functions](../../../python/modules/03-control-flow-and-functions/overview.en.md), [C#: variables and methods](../../../csharp/overview.en.md)
- Static types: [TypeScript](../../../typescript/overview.en.md)
