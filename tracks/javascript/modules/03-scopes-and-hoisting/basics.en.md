---
title: "Variables and scopes: basics"
lang: en
track: javascript
kind: module
doc: basics
slug: 03-scopes-and-hoisting
part: 1
tags: [let, const, var, hoisting, tdz, block-scope, closure, shadowing]
versions: "ECMAScript 2025+, Node.js 24.15 LTS"
status: draft
updated: 2026-11-23
---

# Variables and scopes: basics

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [01 · Environment and tools](../01-environment/overview.en.md), [02 · Types and values](../02-types-and-values/overview.en.md) · **Time:** ~2 h

All examples were run on Node.js 24.15 in an ES module file (strict mode).

## Why it matters

Scope defines where a variable exists and who can see it. A misunderstanding of scopes gives variable "leaks", surprising `undefined`s and the classic closure bugs in loops.

## let, const and var

- **`const`** — a variable that cannot be reassigned; it is declared with a mandatory value.
- **`let`** — a variable whose value can change.
- **`var`** — the old way; it has a different scope and hoisting, so it is not used in new code.

```js
const arr = [1]; arr.push(2);
console.log(arr);
try { const k = 1; k = 2; } catch (e) { console.log(e.name + ': ' + e.message); }
```

```text
[ 1, 2 ]
TypeError: Assignment to constant variable.
```

`const` forbids **reassignment**: the array `arr` can be changed, while `k = 2` is an error. The content of an object is not frozen:

```js
const frozen = Object.freeze({ a: 1, nested: { b: 1 } });
frozen.nested.b = 2;
try { frozen.a = 2 } catch (e) { console.log(e.name + ': ' + e.message) }
console.log(frozen);
```

```text
TypeError: Cannot assign to read only property 'a' of object '#<Object>'
{ a: 1, nested: { b: 2 } }
```

`Object.freeze` protects only the top level of properties (a shallow freeze): the nested object stayed mutable.

## Block scope

`let` and `const` are visible only inside the block `{ … }`, while `var` "leaks" from a block into the function:

```js
if (true) { var leaked = 'var leaks'; let kept = 'let stays'; }
console.log(leaked, typeof kept);
for (var i = 0; i < 3; i++) {}
console.log('i after var loop:', i);
```

```text
var leaks undefined
i after var loop: 3
```

`typeof kept` is `'undefined'` because there is no such name outside the block; `var i` stays available after the loop.

## Hoisting

Declarations are "lifted" to the start of their scope: the engine knows about them before the code runs.

```js
console.log(typeof v1, v1); var v1 = 5; console.log(v1);
console.log(hoisted()); function hoisted() { return 'function declarations are hoisted'; }
try { expr() } catch (e) { console.log(e.name + ': ' + e.message) } var expr = function () {};
```

```text
undefined undefined
5
function declarations are hoisted
TypeError: expr is not a function
```

- `var v1` was hoisted but without a value: until the line with the assignment it is `undefined`.
- A function declaration (`function hoisted() {}`) is hoisted entirely: it can be called before the declaration.
- A function **expression** (`var expr = function () {}`) is hoisted as a `var`: until the assignment it is `undefined`, and the call gives `TypeError: expr is not a function`.

## The temporal dead zone (TDZ)

`let`, `const` and `class` are "hoisted" too, but before the declaration line they sit in a dead zone:

```js
try { console.log(l1); let l1 = 1; } catch (e) { console.log(e.name + ': ' + e.message) }
try { console.log(typeof c1); const c1 = 1; } catch (e) { console.log(e.name + ': ' + e.message) }
try { new K(); class K {} } catch (e) { console.log(e.name + ': ' + e.message) }
```

```text
ReferenceError: Cannot access 'l1' before initialization
ReferenceError: Cannot access 'c1' before initialization
ReferenceError: Cannot access 'K' before initialization
```

Even `typeof` inside the TDZ raises an error. This protects against using a variable before it has a value: an explicit error is better than a silent `undefined`.

## A closure in a loop: var and let

The classic trap: functions created in a loop remember the **variable**, not its value at the moment of creation.

```js
const fv = []; for (var a = 0; a < 3; a++) fv.push(() => a);
const fl = []; for (let b = 0; b < 3; b++) fl.push(() => b);
console.log(fv.map(f => f()), fl.map(f => f()));
const timeouts = [];
for (var j = 0; j < 3; j++) setTimeout(() => timeouts.push(j), 0);
setTimeout(() => console.log('setTimeout var:', timeouts), 5);
```

```text
[ 3, 3, 3 ] [ 0, 1, 2 ]
setTimeout var: [ 3, 3, 3 ]
```

With `var` there is one variable `a` for the whole loop, and by the time the functions are called it equals `3`. With `let` each iteration creates its own copy, so the functions see `0`, `1`, `2`.

## Redeclaration and shadowing

```js
var dup = 1; var dup = 2; console.log(dup);
try { eval('let z = 1; let z = 2;') } catch (e) { console.log(e.name + ': ' + e.message) }
let sh = 'outer'; function f() { let sh = 'inner'; return sh; }
console.log(f(), sh);
```

```text
2
SyntaxError: Identifier 'z' has already been declared
inner outer
```

- `var` allows redeclaration, `let` and `const` do not (`SyntaxError`, the error is seen even before execution).
- **Shadowing:** the inner `sh` hides the outer one but does not change it.

## Beginner mistakes

- ❌ `var` in new code → ✅ `const`, and `let` when needed.
- ❌ Reading a variable before its declaration (`let x`) → ✅ declare at the top of the block.
- ❌ Treating `const` as immutable → ✅ `const` fixes the binding, not the content.
- ❌ Closures in a loop with `var` → ✅ `let` in the loop header.
- ❌ Calling a function expression before the assignment → ✅ a function declaration or code order.
- ❌ Declaring the same variable twice → ✅ unique names; `let` and `const` protect you with an error.

## Related materials

- Next: [advanced theory](advanced.en.md) — the scope chain, module scope, functions in blocks
- Earlier: [02 · Types and values](../02-types-and-values/overview.en.md) (`typeof` of an undeclared variable)
- Closures in depth: module 10 of the [track](../../overview.en.md) roadmap

## Sources

- [MDN: let](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Statements/let)
- [MDN: const](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Statements/const)
- [MDN: Hoisting](https://developer.mozilla.org/en-US/docs/Glossary/Hoisting)
- [MDN: Closures](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Guide/Closures)
- [ECMAScript: Declarations and the Variable Statement](https://tc39.es/ecma262/#sec-declarations-and-the-variable-statement)
