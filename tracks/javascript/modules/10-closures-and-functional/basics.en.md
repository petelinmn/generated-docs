---
title: "Closures and functional style: basics"
lang: en
track: javascript
kind: module
doc: basics
slug: 10-closures-and-functional
part: 2
tags: [closure, factory, private-state, iife, lexical-scope]
versions: "ECMAScript 2025+, Node.js 24.15 LTS"
status: draft
updated: 2026-11-23
---

# Closures and functional style: basics

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [03 · Variables and scopes](../03-scopes-and-hoisting/overview.en.md), [04 · Functions](../04-functions/overview.en.md) · **Time:** ~2 h

All examples were run on Node.js 24.15.

## Why it matters

Closures let you keep state inside a function and not expose it: counters, caches, settings, event handlers. Without them you would need global variables or a class for every small thing.

## What a closure is

A function in JavaScript remembers the scope in which it was created ([module 03](../03-scopes-and-hoisting/basics.en.md)). Called later, even elsewhere, it still sees those variables:

```js
function makeCounter() { let n = 0; return { inc: () => ++n, get: () => n }; }
const c1 = makeCounter(), c2 = makeCounter();
c1.inc(); c1.inc(); c2.inc();
console.log(c1.get(), c2.get());
```

```text
2 1
```

- Every call of `makeCounter` creates a **new** variable `n`, so the counters are independent.
- `n` is not accessible from outside: it can be changed only through `inc`. That is private state.

## A closure keeps the variable, not the value

Two functions created in the same call share one variable; changes are visible at once:

```js
function pair() { let v = 0; return [() => v++, () => v]; }
const [up, read] = pair(); up(); up(); console.log(read());

let x = 1; const getX = () => x; x = 2; console.log(getX());
```

```text
2
2
```

`getX` returned `2`, not `1`: it reads the variable at call time.

## Closures in a loop

With `var` there is one variable for the whole loop, with `let` a new one per iteration (details — [module 03](../03-scopes-and-hoisting/basics.en.md#a-closure-in-a-loop-var-and-let)):

```js
const fs = [];
for (var i = 0; i < 3; i++) fs.push(() => i);
console.log(fs.map(f => f()));
const gs = [];
for (let j = 0; j < 3; j++) gs.push(() => j);
console.log(gs.map(f => f()));
```

```text
[ 3, 3, 3 ]
[ 0, 1, 2 ]
```

## Factories and private state

A factory function returns an object or a function and hides the data:

```js
const bank = (() => { let balance = 100; return { deposit(x) { balance += x; return balance; } }; })();
console.log(bank.deposit(50), bank.balance);
```

```text
150 undefined
```

- Parentheses around a function and an immediate call `()` make an **IIFE** (immediately invoked function expression): it creates a scope and runs at once.
- `bank.balance` is `undefined`: the variable is not accessible from outside.
- For state with several methods a [class](../06-classes/overview.en.md) with `#` fields is often more convenient; a closure is simpler when there are one or two methods.

## Functions as values

A function can be passed and returned — the basis of the functional style:

```js
const not = f => (...a) => !f(...a);
console.log([1, 2, 3, 4].filter(not(n => n % 2)));
```

```text
[ 2, 4 ]
```

`not` takes a function and returns a new one — a **higher-order function**. More on the [next level](advanced.en.md).

## Beginner mistakes

- ❌ Expecting a copy of a value instead of a reference to a variable → ✅ remember that a closure sees changes.
- ❌ Creating functions in a loop with `var` → ✅ `let`.
- ❌ Assuming two calls of a factory share state → ✅ each has its own.
- ❌ Creating a class for one hidden value → ✅ a closure.
- ❌ Trying to read a "private" variable from outside → ✅ provide a getter function.

## Related materials

- Next: [advanced theory](advanced.en.md) — currying, composition, memoization, immutability
- Earlier: [04 · Functions](../04-functions/advanced.en.md#closures-independent-state)

## Sources

- [MDN: Closures](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Guide/Closures)
- [MDN: IIFE](https://developer.mozilla.org/en-US/docs/Glossary/IIFE)
- [MDN: Functions — scope and the function stack](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Guide/Functions#function_scope)
