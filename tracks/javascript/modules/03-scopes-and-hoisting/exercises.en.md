---
title: "Exercises: variables and scopes"
lang: en
track: javascript
kind: module
doc: exercises
slug: 03-scopes-and-hoisting
part: 1
tags: [let, const, var, hoisting, tdz, closure]
status: draft
updated: 2026-11-23
---

# Exercises: variables and scopes

🌐 [Русский](exercises.ru.md) · **English**

> Solve it yourself; hints and solutions are under spoilers. Run the code in an ES module file (`"type": "module"`).

## 1. 🟢 Hoisting

**Task:** predict the output and check: `console.log(typeof a, a); var a = 1;` and then a call `f()` before the declaration `function f() { return 'ok'; }`.

**Expected result:** `undefined undefined`, then `ok`.

<details><summary>Hint</summary>

`var` is hoisted as `undefined`, a function entirely.

</details>

<details><summary>Solution</summary>

```js
console.log(typeof a, a); var a = 1;
console.log(f()); function f() { return 'ok'; }
```

```text
undefined undefined
ok
```

</details>

## 2. 🟢 The TDZ

**Task:** get a `ReferenceError` from `let`, from `const` and from `class` by touching them before the declaration. Print the messages.

**Expected result:** `ReferenceError: Cannot access 'l1' before initialization` and similar ones for `c1` and `K`.

<details><summary>Hint</summary>

Wrap the accesses in `try`/`catch` and print `e.name + ': ' + e.message`.

</details>

<details><summary>Solution</summary>

```js
const t = (f) => { try { f() } catch (e) { console.log(e.name + ': ' + e.message) } };
t(() => { console.log(l1); let l1 = 1; });
t(() => { console.log(c1); const c1 = 1; });
t(() => new K()); class K {}
```

```text
ReferenceError: Cannot access 'l1' before initialization
ReferenceError: Cannot access 'c1' before initialization
ReferenceError: Cannot access 'K' before initialization
```

</details>

## 3. 🟡 Closures in a loop

**Task:** create three functions in a loop that return the indexes `0`, `1`, `2` in two ways: with `let` and with `var` plus a helper function (an IIFE). Show that `var` unaided gives `3, 3, 3`.

**Expected result:** `[3, 3, 3]`, `[0, 1, 2]`, `[0, 1, 2]`.

<details><summary>Hint</summary>

The IIFE: `((n) => () => n)(a)`.

</details>

<details><summary>Solution</summary>

```js
const v = []; for (var a = 0; a < 3; a++) v.push(() => a);
const l = []; for (let b = 0; b < 3; b++) l.push(() => b);
const w = []; for (var c = 0; c < 3; c++) w.push(((n) => () => n)(c));
console.log(v.map(f => f()), l.map(f => f()), w.map(f => f()));
```

```text
[ 3, 3, 3 ] [ 0, 1, 2 ] [ 0, 1, 2 ]
```

</details>

## 4. 🟡 Module scope

**Task:** in an ES module declare `var gv = 1; let gl = 2;` and print `globalThis.gv`, `globalThis.gl`. Then do the same in `node -e`. Explain the difference.

**Expected result:** in a file `undefined undefined`; in `node -e` — `1 undefined`.

<details><summary>Hint</summary>

A file is a module with its own scope, `-e` is the global scope, but `let` does not create properties of the global object.

</details>

<details><summary>Solution</summary>

```text
$ node module.js
undefined undefined
$ node -e "var gv=1; let gl=2; console.log(globalThis.gv, globalThis.gl)"
1 undefined
```

</details>

## 5. 🔴 Functions in blocks and default parameters

**Task:** (a) show the difference between strict and sloppy mode on a function declared in a block (`.js` in an ES module and `.cjs`). (b) Predict the result of `((a, b = () => a) => { var a = 2; return b(); })(1)` and explain.

**Expected result:** (a) `ReferenceError: inner is not defined` in the module, `in block` in `.cjs`; (b) `1`.

<details><summary>Hint</summary>

(b) The closure in the default value is created in the parameter scope, and `var a` in the body is a different variable.

</details>

<details><summary>Solution</summary>

The code and the output for (a) are in the [advanced theory](advanced.en.md#functions-in-blocks).

```js
console.log(((a, b = () => a) => { var a = 2; return b(); })(1));
```

```text
1
```

`b` sees the parameter `a`, equal to `1`; `var a = 2` in the body creates a separate variable.

</details>
