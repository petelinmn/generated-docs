---
title: "Variables and scopes: expert"
lang: en
track: javascript
kind: module
doc: expert
slug: 03-scopes-and-hoisting
part: 1
tags: [environment-record, execution-context, hoisting, tdz, closure, var, let, const, per-iteration-binding]
versions: "ECMAScript 2025+, Node.js 24.15 LTS"
status: draft
updated: 2026-11-23
---

# Variables and scopes: expert

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · **3. Expert** · [4. Quiz](quiz.en.md)

Builds on [basic](basics.en.md) and [advanced](advanced.en.md) theory. All results were obtained by running on Node.js 24.15 (an ES module, strict mode) unless stated otherwise.

## Reference

### Declaration forms

| Form | Scope | Hoisting | Redeclaration | Reassignment |
|---|---|---|---|---|
| `var x` | function (or module) | yes, value `undefined` | allowed (in the same function) | yes |
| `let x` | block | yes, but TDZ | `SyntaxError` | yes |
| `const x` | block | yes, but TDZ | `SyntaxError` | `TypeError: Assignment to constant variable.` |
| `function f() {}` | function (a block — in strict mode) | entirely, with its body | allowed among `var`s in a function | yes |
| `var f = function () {}` | like `var` | the name is hoisted, value `undefined` | allowed | yes |
| `class C {}` | block | yes, but TDZ | `SyntaxError` | yes (the name), the content stays |
| a function parameter | the parameter scope | — | `var a` in the body does not create a new one (`params(7)` returned `7`) | yes |
| a `catch (e)` parameter | the `catch` block | — | — | yes |

> ⚠️ Verify: the behavior of `import` as a hoisted binding and of `using` declarations (explicit resource management) was not run in this material.

### Error messages

| Situation | Error |
|---|---|
| Reading `let`/`const`/`class` before the declaration | `ReferenceError: Cannot access 'x' before initialization` |
| Touching an undeclared name | `ReferenceError: x is not defined` |
| Assigning to `const` | `TypeError: Assignment to constant variable.` |
| A repeated `let`/`const` in one scope | `SyntaxError: Identifier 'x' has already been declared` |
| Calling a `var` equal to `undefined` | `TypeError: expr is not a function` |
| `let x = x + 1` | `ReferenceError: Cannot access 'x' before initialization` (the name is already shadowed) |
| `for (const i = 0; i < 2; i++)` | `TypeError: Assignment to constant variable.` |

## Internals

- **Environments (Environment Records).** The specification describes a scope as an *environment*: a "name → value" table with a reference to the outer environment. A lookup goes along the chain of references — this is the scope chain.
- **Bindings are created before execution.** Before running the body of a function, block or module, the engine creates all the bindings declared in it (instantiation). For `var` and functions a binding gets a value at once (`undefined` or the function body); for `let`/`const`/`class` it stays **uninitialized** until the declaration runs — that is the TDZ. Hoisting is a consequence of this step, not a physical "move of code".
- **A function and a `var` with the same name.** Inside a function the function declaration wins: `typeof f1` before `var f1 = 1` is `'function'`, and after the assignment it is `'number'`; a `var f2` without a value does not overwrite the function (`'function'`). At the top level of a **module** a function is a lexical binding, so `var q = 1; function q(){}` is a `SyntaxError: Identifier 'q' has already been declared`.
- **A binding per iteration.** In `for (let i = …; …; i++)` each iteration creates a new environment into which the value of `i` is copied. That is why closures see their own values (`[0, 1, 2]`), and `i++` inside a closure changes only "its" copy: two passes of `fns2.map(f => f())` gave `[0, 1, 2]` and then `[1, 2, 3]`.
- **The parameter scope.** Parameters with defaults get a separate environment; `var a` in the body creates a different variable, and a closure from a parameter sees the parameter, not the body `a` (in the experiment `((a, b = () => a) => { var a = 2; return b(); })(1)` returned `1`).
- **The global scope.** The global environment consists of an object environment (`var` and functions become properties of `globalThis`) and a declarative part (`let`, `const`, `class` — without properties). In a Node.js file the top level belongs to the module: `var topVar` has no descriptor on `globalThis` (in the experiment `undefined`).
- **`this`.** In an ES module `this` at the top level and inside an ordinary function called without an object is `undefined` (in the experiment `typeof this` was `'undefined'` for an ordinary and an arrow function).
- **A closure.** A function keeps a reference to the environment where it was created. The environment lives while it is referenced: that is how the `count` counter from `mk()` keeps existing after `mk` returns.

> ⚠️ Verify: how V8 lays out variables (registers, contexts on the heap) and the analysis of which variables are captured by closures come from general knowledge of the engine and were not checked; the specification does not require such behavior.

## Gotchas

### 1. `typeof` inside the TDZ fails

`typeof c1` before `const c1` is a `ReferenceError`. The safety of `typeof` for undeclared names does not extend to the TDZ.

### 2. Hoisting of a function expression

`var expr = function () {}` is hoisted as `undefined`: a call before the assignment is `TypeError: expr is not a function`, not a `ReferenceError`.

### 3. `const` in a `for` loop

`for (const i = 0; i < 2; i++)` fails on the second step (`TypeError`), while `for (const x of [1, 2])` works: a new binding is created on each iteration.

### 4. Shadowing hides the outer name entirely

`let x = 1; { let x = x + 1; }` is a `ReferenceError`: inside the block `x` already means the inner, not yet initialized variable.

### 5. Static class fields and the TDZ

`class A { static s = B.v }` before the declaration of `class B` is a `ReferenceError` (`B` is in the TDZ).

### 6. `var` from `catch`, `if`, a loop "leaks"

A `var` has function scope, not block scope (in the experiment `leaked`, `fromCatch` and `i` are visible after the block).

### 7. Functions in blocks depend on the mode

In strict mode a function from a block is visible only in the block; in sloppy mode it "hoists" into the function after the block runs (in the experiment in `.cjs`).

### 8. Default parameters and the function body

A default value does not see `let`/`var` from the body (in the experiment `[ 'outer', 'outer', 'body' ]`).

### 9. `eval` in strict mode

`eval('var ev = 1')` in a module does not create a variable outside: `typeof ev` is `'undefined'` (in the experiment).

## Trade-offs and alternatives

| Approach | Pros | Cons |
|---|---|---|
| `const` by default | fewer accidental reassignments, clear code | does not protect the content |
| `let` | flexibility | can be changed by accident |
| `var` | compatibility with old code | function scope, hoisting, redeclarations |
| `Object.freeze` | protects the top level from writes | shallow |
| Modules and blocks instead of an IIFE | isolation without extra functions | require ESM |
| Linters (`no-var`, `prefer-const`, `no-use-before-define`) | find problems in advance | not run |

## Interview questions

<details><summary>1. How do <code>var</code>, <code>let</code> and <code>const</code> differ?</summary>

Scope (function versus block), hoisting (`undefined` versus the TDZ), redeclaration (allowed only for `var`), reassignment (not allowed for `const`).

→ [Reference](#declaration-forms)

</details>

<details><summary>2. What is hoisting?</summary>

Bindings are created before the scope's code runs: `var` gets `undefined`, a function gets its body, `let`/`const`/`class` stay uninitialized (the TDZ).

→ [Basic theory](basics.en.md#hoisting)

</details>

<details><summary>3. What is the TDZ and why does <code>typeof</code> fail in it?</summary>

The period from the start of a block to the declaration when the binding exists but is not initialized. Any access, including `typeof`, is a `ReferenceError`.

→ [Gotcha 1](#1-typeof-inside-the-tdz-fails)

</details>

<details><summary>4. Trick: what does a loop with <code>var</code> and <code>setTimeout</code> print?</summary>

`3, 3, 3`: one variable for the whole loop; by the call time it is `3`. With `let` — `0, 1, 2`.

→ [Basic theory](basics.en.md#a-closure-in-a-loop-var-and-let)

</details>

<details><summary>5. Does <code>const</code> allow changing an object?</summary>

Yes, only reassignment is forbidden. For protection use `Object.freeze` (shallow).

→ [Basic theory](basics.en.md#let-const-and-var)

</details>

<details><summary>6. Trick: <code>var expr = function () {}</code> is called before the declaration. What happens?</summary>

`TypeError: expr is not a function`: `expr` was hoisted with the value `undefined`.

→ [Gotcha 2](#2-hoisting-of-a-function-expression)

</details>

<details><summary>7. What is a closure?</summary>

A function together with a reference to the environment where it was created; the environment lives while it is referenced.

→ [Internals](#internals)

</details>

<details><summary>8. Why does <code>let</code> in a loop header solve the closure problem?</summary>

Each iteration creates a new environment with a copy of the variable.

→ [Internals](#internals)

</details>

<details><summary>9. Does a top-level <code>var</code> in a file become a property of <code>globalThis</code>?</summary>

In a Node.js file — no, it is a module variable. In a classic browser script or `node -e` — yes.

→ [Advanced theory](advanced.en.md#module-scope-and-global-scope)

</details>

<details><summary>10. Trick: what does <code>let x = 1; { let x = x + 1; }</code> return?</summary>

A `ReferenceError`: the inner `x` shadows the outer one and is in the TDZ.

→ [Gotcha 4](#4-shadowing-hides-the-outer-name-entirely)

</details>

<details><summary>11. How do functions in blocks work in strict and sloppy mode?</summary>

Strict — visible only in the block; sloppy — available in the enclosing function after the block runs.

→ [Advanced theory](advanced.en.md#functions-in-blocks)

</details>

<details><summary>12. Why braces in a <code>case</code> with <code>let</code>?</summary>

All <code>switch</code> branches share one scope, and identical <code>let</code>s give a <code>SyntaxError</code>.

→ [Advanced theory](advanced.en.md#switch-and-catch)

</details>

## Cheat sheet

```text
const by default ; let when reassigning ; var — do not use
TDZ: let/const/class before the declaration → ReferenceError
var and function are hoisted; a function expression — like var (undefined)
for (let …) — a new binding per iteration; for (var …) — one per loop
a { } block — scope for let/const/class; var — function scope
the top level of an ES module — module variables, not globalThis
```

## Related materials

- Previous level: [advanced theory](advanced.en.md)
- Types and values: [02 · Types and values](../02-types-and-values/overview.en.md)
- Environment and strict mode: [01 · Environment and tools](../01-environment/overview.en.md)
- Closures and functions — modules 04 and 10 of the [track](../../overview.en.md) roadmap

## Sources

- [ECMAScript: Environment Records](https://tc39.es/ecma262/#sec-environment-records)
- [ECMAScript: Let and Const Declarations](https://tc39.es/ecma262/#sec-let-and-const-declarations)
- [ECMAScript: FunctionDeclarationInstantiation](https://tc39.es/ecma262/#sec-functiondeclarationinstantiation)
- [MDN: Temporal dead zone](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Statements/let#temporal_dead_zone_tdz)
- [MDN: Closures](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Guide/Closures)
