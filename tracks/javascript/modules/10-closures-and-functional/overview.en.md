---
title: "Closures and functional style"
lang: en
track: javascript
kind: module
doc: overview
slug: 10-closures-and-functional
part: 2
levels: [1, 2, 3, 4]
tags: [closure, higher-order-function, currying, partial-application, composition, memoization, immutability, pure-function]
versions: "ECMAScript 2025+, Node.js 24.15 LTS"
status: draft
updated: 2026-11-26
---

# Closures and functional style

🌐 [Русский](overview.ru.md) · **English**

## In short

A closure is a function together with the variables of outer scopes that it "remembers". Because of it, functions create functions with their own hidden state: counters, factories, caches. The functional style is built on this: higher-order functions, currying, composition, pure functions and immutable data.

## Key ideas

- **A closure keeps a reference to a variable, not its value:** changes are visible.
- **Every call of a factory creates its own environment,** so two counters do not interfere.
- **Closures are a way to get private state** without classes.
- **A higher-order function** takes or returns a function (`map`, `filter`, `reduce`, `bind`).
- **Currying and partial application** fix some arguments; `fn.length` is the number of parameters before the first default value or rest.
- **Composition** (`pipe`, `compose`) assembles functions into a pipeline.
- **Memoization** caches the result of a pure function; object keys need care.
- **`Object.freeze` is shallow;** for copies use spread, `structuredClone`, `toSorted`/`with`.
- **Recursion is limited by the stack;** use a loop or a trampoline instead.

## Minimal example

```js
function makeCounter() { let n = 0; return { inc: () => ++n, get: () => n }; }
const c1 = makeCounter(), c2 = makeCounter();
c1.inc(); c1.inc(); c2.inc();
console.log(c1.get(), c2.get());
```

```text
2 1
```

Every call of `makeCounter` has its own variable `n`.

## When to use and when not

| Task | Choice |
|---|---|
| Private state without a class | a closure |
| Configuring a function "in advance" | partial application, `bind` |
| A chain of transformations | `pipe`/`compose`, `map`/`filter` chains |
| An expensive pure function | memoization |
| Shared state of an object with methods | a class ([module 06](../06-classes/overview.en.md)) |

- ❌ Do not memoize functions with side effects or a dependency on time.
- ❌ Do not treat `Object.freeze` as deep.

## Material levels

| Level | What is inside |
|---|---|
| [1. Basic theory](basics.en.md) | a closure, factories, private state, loops, IIFE |
| [2. Advanced theory](advanced.en.md) | higher-order functions, currying, composition, memoization, immutability |
| [3. Expert theory](expert.en.md) | a reference, internals, pitfalls, interview questions |
| [4. Self-check](quiz.en.md) | quiz and [exercises](exercises.en.md) |
| 5. Runnable examples | not created yet — `/learn javascript 10-closures-and-functional level 5` |
| 6. Tests | not created yet — `/learn javascript 10-closures-and-functional level 6` |

## Related materials

- Prerequisites: [03 · Variables and scopes](../03-scopes-and-hoisting/overview.en.md), [04 · Functions](../04-functions/overview.en.md)
- Next: [11 · Iterators and generators](../11-iterators-and-generators/overview.en.md)
- Modules and their state: [09 · Modules](../09-modules/overview.en.md)
- Analogues in other tracks: [C#: delegates, events and lambdas](../../../csharp/modules/09-delegates-events-lambdas/overview.en.md), [Python](../../../python/overview.en.md)
- [Closures and state snapshots in React](../../../react/modules/03-state-and-events/overview.en.md)
