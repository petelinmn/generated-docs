---
title: "Closures and functional style: expert"
lang: en
track: javascript
kind: module
doc: expert
slug: 10-closures-and-functional
part: 2
tags: [closure, environment-record, lexical-environment, structured-clone, object-freeze, trampoline, tail-call, weakmap-private]
versions: "ECMAScript 2025+, Node.js 24.15 LTS"
status: draft
updated: 2026-11-23
---

# Closures and functional style: expert

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · **3. Expert** · [4. Quiz](quiz.en.md)

Builds on [basic](basics.en.md) and [advanced](advanced.en.md) theory. All results were obtained by running on Node.js 24.15.

## Reference

### Levels of object protection

| Operation | Add a property | Delete | Change a value | Check |
|---|---|---|---|---|
| `Object.preventExtensions(o)` | no | yes (in the experiment) | yes | `Object.isExtensible` |
| `Object.seal(o)` | no | no (in the experiment `delete` → `false`) | yes (in the experiment `x` became `2`) | `Object.isSealed` |
| `Object.freeze(o)` | no | no (in the experiment `delete` → `false`) | no | `Object.isFrozen` |

In strict mode a failed `delete` is `TypeError: Cannot delete property 'x' of #<Object>` (in the experiment, in an ES module).

The descriptor of a property of a frozen object: `{ value: 1, writable: false, enumerable: true, configurable: false }` (in the experiment). Freezing is shallow; deep freezing is done recursively:

```js
const deepFreeze = v => { Object.values(v).forEach(x => typeof x === 'object' && x !== null && deepFreeze(x)); return Object.freeze(v); };
const d = deepFreeze({ a: { b: { c: 1 } }, arr: [1, 2] });
try { d.a.b.c = 2 } catch (e) { console.log(e.message) }
try { d.arr.push(3) } catch (e) { console.log(e.message) }
```

```text
Cannot assign to read only property 'c' of object '#<Object>'
Cannot add property 2, object is not extensible
```

In sloppy mode a write to a frozen object is silently ignored (in the experiment `Object.freeze({ a: 1 })` after `o.a = 2; o.z = 1` gives `1 undefined`).

### Ways to copy

| Way | Depth | What it loses |
|---|---|---|
| `{ ...o }`, `Object.assign({}, o)` | shallow | getters turn into values (in the experiment `{ value: 7, … }`) |
| `structuredClone(o)` | deep | functions (`DataCloneError`), the prototype (a class becomes a plain object), the frozen flag |
| `JSON.parse(JSON.stringify(o))` | deep | `undefined` and functions vanish, `Date` → string, `Map`/`Set` → `{}` (in the experiment `{"d":"1970-01-01T00:00:00.000Z","m":{},"s":{}}`) |

`structuredClone` in the experiment: copies `Date`, `Set`, `Map`, `RegExp` and **circular references**; a class instance becomes a plain object (`k.constructor === Object`, `k instanceof K` is `false`); a `Symbol` is a `DataCloneError`; a copy of a frozen object is not frozen (`Object.isFrozen` is `false`).

### Copying array methods

| Mutates | Returns a copy |
|---|---|
| `sort()` | `toSorted()` |
| `reverse()` | `toReversed()` |
| `splice()` | `toSpliced()` |
| `arr[i] = x` | `with(i, x)` |

### What `fn.length` counts

The number of parameters **before the first default value or rest**: `((a, b = 2, ...r) => 0).length` is `1`, `((...a) => 0).length` is `0`. A function after `bind` gets a `name` like `bound foo` and a `length` smaller by the number of bound arguments (in the experiment `bound mul`, `1`).

## Internals

- **The environment.** Every function call creates an environment record with local variables and a reference to the outer one. When a function is created it remembers the environment where it is defined (the internal slot `[[Environment]]`). A name is looked up along the chain of environments: that is the closure.
- **A variable, not a value.** A closure refers to a binding in the environment, so `getX` sees `x = 2` and two functions from one call share one variable (in the experiment `pair()` → `2`).
- **An environment per call.** Two calls of `makeCounter` create two environments: hence the independence of the counters.
- **`let` in a loop header.** On every iteration a copy of the binding is created: the value is carried into a new environment before the increment. So a closure that changes `i` in the body changes only its own copy: in the experiment a loop with `i++` in both the body and the header gave `[ 1, 3 ]`.
- **`eval` and closures.** A direct `eval` sees the closure's variables (in the experiment `eval('secret')` returned `5`) even if the name `secret` does not occur in the function's code.
- **Memory.** An environment lives while at least one function that refers to it is reachable; the garbage collector frees it afterwards. What exactly the engine keeps in the environment (only the used variables or all of them) is an implementation detail.

> ⚠️ Verify: V8 optimizations in which a closure keeps not all variables of the outer function, and the exact cases of memory leaks due to a shared context, were not checked by running.

- **Recursion and the stack.** The call stack is limited: `sum(1e6)` with ordinary recursion gave `RangeError: Maximum call stack size exceeded`, while a trampoline version on a loop gave `500000500000`.
- **Tail call optimization.** The standard describes it, but Node.js (V8) does not perform it: genuine tail recursion `tail(n - 1, acc + n)` at `n = 1e6` also gives `RangeError: Maximum call stack size exceeded` (in the experiment). Do not rely on it.

> ⚠️ Verify: whether tail call optimization exists in other engines (for example Safari/JavaScriptCore) was not checked against documentation.

## Gotchas

### 1. `map(parseInt)`

`['1', '2', '3'].map(parseInt)` gives `[ 1, NaN, NaN ]`: the index goes into the `radix` parameter.

### 2. `Object.freeze` is not deep

`o.n.b = 3` changed a nested object of a frozen `o`; `Object.isFrozen(o.n)` is `false`.

### 3. Freezing and modes

In sloppy code a write is silently ignored (`1 undefined`), in strict code and modules it is a `TypeError`. The same code behaves differently.

### 4. `structuredClone` loses the prototype

A class instance becomes a plain object: methods and getters are gone.

### 5. Currying and default values

`curry((a, b = 2) => a + b)(1)` calls the function at once and returns `3`: `length` is `1`.

### 6. A second `bind` does not change `this`

`b1 = h.bind({ v: 1 })`, `b2 = b1.bind({ v: 2 })`: `b2()` returns `1`.

### 7. `this` inside an ordinary function in a closure

An ordinary function returned from a method loses `this`: in sloppy mode `this` is the global object (in the experiment `typeof this` is `object`), in strict mode `undefined`. An arrow takes `this` from the outer function (in the experiment `obj.regular()()` → `1`).

### 8. A shared cache grows

Memoization with object keys in a `Map` does not free memory; object keys are better kept in a `WeakMap` (`wm.get(key)` → `1`, `wm.has({})` → `false`).

### 9. A closure and `let` with a change inside the body

A copy of the binding is created on every iteration: functions created before `i++` in the body see the values `1` and `3`, not `0` and `2`.

## Trade-offs and alternatives

| Approach | Pros | Cons |
|---|---|---|
| Closures for state | simpler than a class, real privacy | no prototype: every instance holds its own copies of functions |
| Classes with `#` fields | methods in the prototype, `instanceof` | more ceremony |
| A `WeakMap` for private data | data outside the object, does not retain it | slower, indirection |
| Immutability by convention | fast, no wrappers | protects nothing |
| `Object.freeze`/`deepFreeze` | run-time protection | shallow, traversal cost |
| Immutable-structure libraries | structural sharing | a dependency; not run |
| A trampoline | recursion without stack growth | less readable |

## Interview questions

<details><summary>1. What is a closure?</summary>

A function together with a reference to the environment where it was created: it sees its variables even after the outer function has returned.

→ [Basic theory](basics.en.md#what-a-closure-is)

</details>

<details><summary>2. Does a closure keep a value or a variable?</summary>

A variable: `x = 2` made after `getX` was created is visible at the call.

→ [Basic theory](basics.en.md#a-closure-keeps-the-variable-not-the-value)

</details>

<details><summary>3. What does a loop with <code>var</code> and closures print, and how do you fix it?</summary>

`[ 3, 3, 3 ]`; replace `var` with `let`.

→ [Basic theory](basics.en.md#closures-in-a-loop)

</details>

<details><summary>4. How do you make private state without classes?</summary>

A factory with a local variable and functions that refer to it.

→ [Basic theory](basics.en.md#factories-and-private-state)

</details>

<details><summary>5. Trick: what does <code>['1','2','3'].map(parseInt)</code> return?</summary>

`[ 1, NaN, NaN ]`: the index arrives as the radix in the second argument.

→ [Gotcha 1](#1-mapparseint)

</details>

<details><summary>6. How does currying differ from partial application?</summary>

Currying turns a function of n arguments into a chain of one-argument functions (our implementation also allows groups); partial application fixes some arguments and returns a function of the rest (`bind`).

→ [Advanced theory](advanced.en.md#currying-and-partial-application)

</details>

<details><summary>7. How do <code>pipe</code> and <code>compose</code> differ?</summary>

The order: `pipe` is left to right, `compose` right to left. `pipe(inc, sq)(3)` = 16, `compose(inc, sq)(3)` = 10.

→ [Advanced theory](advanced.en.md#composition)

</details>

<details><summary>8. When must you not memoize?</summary>

For functions with side effects and a dependency on time or external state; also with object keys without normalization.

→ [Advanced theory](advanced.en.md#memoization)

</details>

<details><summary>9. Trick: an object was frozen. Can a nested property be changed?</summary>

Yes: `Object.freeze` is shallow (`o.n.b = 3` worked).

→ [Gotcha 2](#2-objectfreeze-is-not-deep)

</details>

<details><summary>10. How is <code>structuredClone</code> better than <code>JSON.parse(JSON.stringify())</code> and how is it worse?</summary>

Better: it copies `Date`, `Map`, `Set`, `RegExp` and circular references. Worse: it does not copy functions and the prototype.

→ [Ways to copy](#ways-to-copy)

</details>

<details><summary>11. Why does a write to a frozen object not fail in sloppy mode?</summary>

In sloppy mode a failed write is silently ignored; only strict mode and modules give an error.

→ [Gotcha 3](#3-freezing-and-modes)

</details>

<details><summary>12. Why does recursion a million levels deep fail and what can you do?</summary>

The call stack is limited (`RangeError`), V8 has no tail call optimization. Use a loop or a trampoline.

→ [Internals](#internals)

</details>

## Cheat sheet

```text
function mk() { let n = 0; return () => ++n }      // closure: a variable in the environment
const dbl = f.bind(null, 2)                          // partial application
const pipe = (...f) => x => f.reduce((v, g) => g(v), x)
Object.freeze(o) — shallow; structuredClone(o) — a deep copy without functions
arr.toSorted() / toReversed() / with(i, x) / toSpliced()  — no mutation
reduce(f, init) — always with an initial value
```

- A closure sees changes of the variable.
- `fn.length` stops at the first default value.
- V8 has no tail call optimization.

## Related materials

- Previous level: [advanced theory](advanced.en.md)
- Scopes: [03 · Variables and scopes](../03-scopes-and-hoisting/expert.en.md)
- Functions and `this`: [04 · Functions](../04-functions/expert.en.md)
- Private class fields: [06 · Classes](../06-classes/expert.en.md)

## Sources

- [ECMAScript: Function Objects (`[[Environment]]`)](https://tc39.es/ecma262/#sec-ecmascript-function-objects)
- [MDN: Closures](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Guide/Closures)
- [MDN: Object.freeze](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Object/freeze)
- [MDN: structuredClone() — supported types](https://developer.mozilla.org/en-US/docs/Web/API/Web_Workers_API/Structured_clone_algorithm)
- [MDN: Tail call optimization (Glossary)](https://developer.mozilla.org/en-US/docs/Glossary/Tail_call)
