---
title: "Functions: basics"
lang: en
track: javascript
kind: module
doc: basics
slug: 04-functions
part: 1
tags: [functions, arrow-functions, parameters, return, this, recursion, callbacks]
versions: "ECMAScript 2025+, Node.js 24.15 LTS"
status: draft
updated: 2026-11-23
---

# Functions: basics

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [02 · Types and values](../02-types-and-values/overview.en.md), [03 · Variables and scopes](../03-scopes-and-hoisting/overview.en.md) · **Time:** ~2.5 h

All examples were run on Node.js 24.15 in an ES module file (strict mode).

## Why it matters

A function is the main unit of code in JavaScript: event handlers, callbacks, modules and classes are built on it. The details of calling and `this` are the most frequent source of confusion.

## Three forms of functions

```js
function decl(a, b) { return a + b; }                       // a declaration
const expr = function (a, b) { return a + b; };             // a function expression
const named = function fact(n) { return n <= 1 ? 1 : n * fact(n - 1); };
const arrow = (a, b) => a + b;                              // an arrow function
console.log(decl.name, expr.name, named.name, arrow.name, decl.length, ((a, b = 1, ...c) => {}).length);
try { fact(3) } catch (e) { console.log(e.name + ': ' + e.message) }
```

```text
decl expr fact arrow 2 1
ReferenceError: fact is not defined
```

- A **declaration** is hoisted entirely: it can be called before the declaration line ([module 03](../03-scopes-and-hoisting/basics.en.md#hoisting)).
- An **expression** and an **arrow** are ordinary values assigned to a variable; only the variable is hoisted (with the value `undefined`).
- The `name` property comes from the variable's name or from the expression's name (`fact`). The name of a function expression is visible only inside it, so outside `fact` is not defined.
- `length` is the number of parameters before the first default value or `...rest`.
- An arrow with a single expression returns its value without `return`: `(a, b) => a + b`.

## Parameters and arguments

```js
function decl(a, b) { return a + b; }
console.log(decl(1), decl(1, 2, 3), decl('a', 'b'));
```

```text
NaN 3 ab
```

JavaScript does not check the number of arguments: extra ones are ignored, missing ones are `undefined` (so `1 + undefined` is `NaN`).

### Default values

```js
function dflt(a, b = a * 2, c = (() => 'lazy')()) { return [a, b, c]; }
console.log(dflt(1), dflt(1, undefined), dflt(1, null), dflt(1, 5));
let n = 0; const f = (a = ++n) => a;
console.log(f(), f(), f(10), n);
```

```text
[ 1, 2, 'lazy' ] [ 1, 2, 'lazy' ] [ 1, null, 'lazy' ] [ 1, 5, 'lazy' ]
1 2 10 2
```

- A default is substituted only for `undefined`; `null` stays `null`.
- It is evaluated **anew on every call** (`f()` gave `1`, then `2`) and only if the argument is not passed (`f(10)` did not increase `n`).
- A default can use previous parameters (`b = a * 2`).

### Rest parameters and `arguments`

```js
function rest(first, ...others) { return [first, others, arguments.length]; }
function args() { return [arguments.length, Array.isArray(arguments), typeof arguments, [...arguments]]; }
console.log(rest(1, 2, 3), rest(), args('a', 'b'));
```

```text
[ 1, [ 2, 3 ], 3 ] [ undefined, [], 0 ] [ 2, false, 'object', [ 'a', 'b' ] ]
```

- `...others` collects the rest into a **real array** (`[2, 3]`); with no arguments it is an empty array.
- `arguments` is an array-like object: it has `length` but no array methods (`Array.isArray` is `false`). In new code use `...rest`. An arrow function has no `arguments` of its own: inside an ordinary function it sees the outer function's `arguments` (in the experiment `outer('outerArg')` returned `'outerArg'`), and at the top level of a module `arguments` is not defined.

## Returning a value

A function without `return` returns `undefined`. Be careful with a line break:

```js
function noret() {}
function asi() { return
  42 }
console.log(noret(), asi());
```

```text
undefined undefined
```

After `return` a `;` is inserted automatically (ASI), so `42` is unreachable code. Start the returned expression on the same line: `return 42`, `return (` for multiline ones.

## this: the basics

In an ordinary function `this` is determined by **how it is called**:

```js
const obj = {
  name: 'obj',
  m() { return this?.name; },
  arrow: () => typeof this,
  nested() { return [1].map(() => this.name); },
  nestedFn() { return [1].map(function () { return this; }); },
};
console.log(obj.m(), obj.arrow(), obj.nested(), obj.nestedFn());
const detached = obj.m;
console.log(detached());
function strictThis() { return this; }
console.log(strictThis());
```

```text
obj undefined [ 'obj' ] [ undefined ]
undefined
undefined
```

- **A method call** `obj.m()` — `this` is `obj`.
- **An arrow** takes `this` from the surrounding code: in `nested` inside `map` it sees the method's `this` (`'obj'`), while at the top level of a module `this` is `undefined` (so `obj.arrow()` returned `'undefined'`).
- **An ordinary function inside `map`** (`nestedFn`) is called without an object: `this` is `undefined`.
- **A detached method** (`const detached = obj.m; detached()`) lost its object: `this?.name` returned `undefined`. Without `?.` there would be a `Cannot read properties of undefined` error.
- A plain call `strictThis()` in strict mode — `this` is `undefined` (in sloppy mode — the global object).

`call`, `apply` and `bind` set `this` explicitly — they are covered in the [advanced theory](advanced.en.md#call-apply-and-bind).

## Functions as values

A function can be passed as an argument (a callback) and returned:

```js
console.log([1, 2, 3].map(n => n * 2), ['1', '2', '3'].map(parseInt), ['1', '2', '3'].map(Number));
const compose = (...fs) => x => fs.reduceRight((v, f) => f(v), x);
console.log(compose(x => x + 1, x => x * 2)(5));
```

```text
[ 2, 4, 6 ] [ 1, NaN, NaN ] [ 1, 2, 3 ]
11
```

- `['1','2','3'].map(parseInt)` gives `[1, NaN, NaN]`: `map` passes three arguments to the function (the value, the index, the array), and `parseInt` takes the index as the radix. Wrap it: `.map(s => parseInt(s, 10))` or use `Number`.
- `compose` returns a new function that applies the passed ones from right to left: `5 * 2 + 1 = 11`.

## Recursion and the stack

```js
const depth = (n) => depth(n + 1);
try { depth(0) } catch (e) { console.log(e.name + ': ' + e.message) }
```

```text
RangeError: Maximum call stack size exceeded
```

Each call takes space on the call stack. Infinite or too deep recursion ends with a `RangeError`. For large depths use a loop or an explicit stack.

## Beginner mistakes

- ❌ Losing `this` of a method passed as a callback → ✅ an arrow wrapper or `bind`.
- ❌ `['1','2','3'].map(parseInt)` → ✅ `.map(Number)` or `s => parseInt(s, 10)`.
- ❌ A line break after `return` → ✅ the value on the same line.
- ❌ Expecting the number of arguments to be checked → ✅ check it yourself or use defaults.
- ❌ Using `arguments` in new code → ✅ `...rest`.
- ❌ An arrow as a method that needs the object's `this` → ✅ the shorthand method `m() {}`.

## Related materials

- Next: [advanced theory](advanced.en.md) — `call`/`apply`/`bind`, constructors, `new.target`
- Next module: [05 · Objects and prototypes](../05-objects-and-prototypes/overview.en.md)
- Earlier: [03 · Variables and scopes](../03-scopes-and-hoisting/overview.en.md) (closures)

## Sources

- [MDN: Functions](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Guide/Functions)
- [MDN: Arrow function expressions](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Functions/Arrow_functions)
- [MDN: Default parameters](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Functions/Default_parameters)
- [MDN: Rest parameters](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Functions/rest_parameters)
- [MDN: this](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Operators/this)
