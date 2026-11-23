---
title: "Quiz: variables and scopes"
lang: en
track: javascript
kind: module
doc: quiz
slug: 03-scopes-and-hoisting
part: 1
tags: [let, const, var, hoisting, tdz, closure]
status: draft
updated: 2026-11-23
---

# Quiz: variables and scopes

🌐 [Русский](quiz.ru.md) · **English**

> Answer first, then open the answer. The mark shows which level a question comes from: 🟢 [basics](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 What does the code print?

```js
console.log(typeof v1, v1); var v1 = 5;
```

- A) `ReferenceError`
- B) `undefined undefined`
- C) `number 5`
- D) `object null`

<details><summary>Answer</summary>

**B.** The declaration `var v1` was hoisted, while the value is assigned later: until then `v1` is `undefined`.
→ [Basics](basics.en.md#hoisting)

</details>

## 2. 🟢 What happens if you read `let x` before the declaration line?

- A) We get `undefined`
- B) `ReferenceError: Cannot access 'x' before initialization`
- C) `SyntaxError`
- D) `TypeError`

<details><summary>Answer</summary>

**B.** The variable is in the temporal dead zone (TDZ).
→ [Basics](basics.en.md#the-temporal-dead-zone-tdz)

</details>

## 3. 🟢 What does `const arr = [1]; arr.push(2);` do?

- A) An error: `const` forbids changes
- B) It works: `arr` becomes `[1, 2]`
- C) It creates a copy of the array
- D) An error only in strict mode

<details><summary>Answer</summary>

**B.** `const` forbids reassignment (`arr = []` would raise a `TypeError`), not mutation of the content.
→ [Basics](basics.en.md#let-const-and-var)

</details>

## 4. 🟢 Is `leaked` visible after `if (true) { var leaked = 1; }`?

- A) No, it lives only in the block
- B) Yes, a `var` has function scope, not block scope
- C) Only inside the `if`
- D) An error at the declaration

<details><summary>Answer</summary>

**B.** With `let` instead of `var` the name would be unreachable outside the block.
→ [Basics](basics.en.md#block-scope)

</details>

## 5. 🟡 What does the code print?

```js
const fv = []; for (var a = 0; a < 3; a++) fv.push(() => a);
console.log(fv.map(f => f()));
```

- A) `[0, 1, 2]`
- B) `[3, 3, 3]`
- C) `[2, 2, 2]`
- D) `[undefined, undefined, undefined]`

<details><summary>Answer</summary>

**B.** A `var` is one variable for the whole loop, equal to `3` by the call time. With `let` the result is `[0, 1, 2]`.
→ [Basics](basics.en.md#a-closure-in-a-loop-var-and-let)

</details>

## 6. 🟡 What does a call `expr()` return before the line `var expr = function () {}`?

- A) `ReferenceError: expr is not defined`
- B) `TypeError: expr is not a function`
- C) `undefined`
- D) The call works

<details><summary>Answer</summary>

**B.** `expr` was hoisted with the value `undefined`. For a function declaration (`function expr() {}`) the call would be valid.
→ [Basics](basics.en.md#hoisting)

</details>

## 7. 🟡 In an ES module: `var gv = 1; console.log(globalThis.gv);`. What is printed?

- A) `1`
- B) `undefined`
- C) `null`
- D) An error

<details><summary>Answer</summary>

**B.** Top-level variables of a file belong to the module. In `node -e` the result would be `1`.
→ [Advanced](advanced.en.md#module-scope-and-global-scope)

</details>

## 8. 🟡 Select all correct statements

- [ ] A) In strict mode a function declared in a block is not visible outside it
- [ ] B) All `switch` branches share one block scope
- [ ] C) `let` allows redeclaration in the same scope
- [ ] D) A `catch (e)` parameter is visible only inside `catch`

<details><summary>Answer</summary>

**A, B, D.** C is wrong: a repeated `let` is a `SyntaxError`.
→ [Advanced](advanced.en.md#functions-in-blocks)

</details>

## 9. 🟡 What does `g()` return?

```js
let dflt = 'outer';
function g(a = dflt) { let dflt = 'body'; return a; }
```

- A) `'body'`
- B) `'outer'`
- C) `undefined`
- D) `ReferenceError`

<details><summary>Answer</summary>

**B.** Default values are evaluated in the parameter scope and do not see the body's variables.
→ [Advanced](advanced.en.md#the-scope-of-default-parameters)

</details>

## 10. 🔴 Find the flaw

> "`typeof` is safe: it never throws, even for an undeclared variable, so in the TDZ it will return `'undefined'` too."

<details><summary>Answer</summary>

For an undeclared name `typeof` returns `'undefined'`, but inside the TDZ (`typeof c1` before `const c1`) it throws a `ReferenceError`: the binding exists but is not initialized.
→ [Gotcha 1](expert.en.md#1-typeof-inside-the-tdz-fails)

</details>

## 11. 🔴 What does `for (const i = 0; i < 2; i++) {}` produce?

- A) Nothing, the loop runs
- B) `TypeError: Assignment to constant variable.`
- C) `SyntaxError`
- D) `ReferenceError`

<details><summary>Answer</summary>

**B.** `i++` reassigns a `const`. `for (const x of [1, 2])` works — the binding is created anew on each iteration.
→ [Gotcha 3](expert.en.md#3-const-in-a-for-loop)

</details>

## 12. 🔴 Open question

Explain how hoisting works and how the TDZ differs from the hoisting of `var`.

<details><summary>Key points of a good answer</summary>

- All bindings of a scope are created before its code runs; it is the creation of bindings that is "lifted", not the text.
- `var` and functions are initialized at once (`undefined` or the function body).
- `let`/`const`/`class` stay uninitialized until the declaration line — access gives a `ReferenceError`.

</details>

→ [Expert](expert.en.md#internals)
