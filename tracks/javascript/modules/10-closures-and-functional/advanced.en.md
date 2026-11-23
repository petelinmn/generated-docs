---
title: "Closures and functional style: advanced"
lang: en
track: javascript
kind: module
doc: advanced
slug: 10-closures-and-functional
part: 2
tags: [higher-order-function, currying, partial-application, composition, pipe, memoization, immutability, structured-clone, object-freeze]
versions: "ECMAScript 2025+, Node.js 24.15 LTS"
status: draft
updated: 2026-11-23
---

# Closures and functional style: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

Builds on [basic theory](basics.en.md). All examples were run on Node.js 24.15.

## Higher-order functions

The built-in array methods ([module 07](../07-arrays-and-collections/basics.en.md)) take functions. The trap is passing a function with extra parameters: `map` calls it with three arguments (element, index, array):

```js
console.log([1, 2, 3].map(String), ['1', '2', '3'].map(Number), ['1', '2', '3'].map(parseInt));
```

```text
[ '1', '2', '3' ] [ 1, 2, 3 ] [ 1, NaN, NaN ]
```

`parseInt` received the index as its second argument (the radix), so `parseInt('2', 1)` is `NaN`. The fix: `map(s => parseInt(s, 10))` or `map(Number)`.

`reduce` folds an array into one value; on an empty array without an initial value it fails:

```js
console.log([1, 2, 3].reduce((a, b) => a + b), [[1], [2]].reduce((a, b) => a.concat(b), []));
try { [].reduce((a, b) => a + b) } catch (e) { console.log(e.name + ': ' + e.message) }
```

```text
6 [ 1, 2 ]
TypeError: Reduce of empty array with no initial value
```

Always pass an initial value.

## Currying and partial application

**Partial application** fixes some arguments. The simplest way is `bind`:

```js
const mul = (a, b) => a * b; const dbl = mul.bind(null, 2); console.log(dbl(5), dbl.name, dbl.length);
```

```text
10 bound mul 1
```

**Currying** turns a function of several arguments into a chain of functions. A universal implementation relies on `fn.length`:

```js
const curry = fn => function c(...a) { return a.length >= fn.length ? fn(...a) : (...b) => c(...a, ...b); };
const add3 = curry((a, b, c) => a + b + c);
console.log(add3(1)(2)(3), add3(1, 2)(3), add3(1)(2, 3));
console.log(((a, b = 2, ...r) => 0).length, ((a, b) => 0).length);
```

```text
6 6 6
1 2
```

`length` counts parameters **before the first default value or rest**, so this `curry` calls the function as soon as it has received `length` arguments: parameters after a default value or `...rest` cannot be passed (for `(a, b = 2) => …` the call `f(1)` runs the function at once). For such functions the number of arguments is specified explicitly.

## Composition

`pipe` applies functions left to right, `compose` right to left:

```js
const pipe = (...fns) => x => fns.reduce((v, f) => f(v), x);
const compose = (...fns) => x => fns.reduceRight((v, f) => f(v), x);
const inc = x => x + 1, sq = x => x * x;
console.log(pipe(inc, sq)(3), compose(inc, sq)(3));
```

```text
16 10
```

`pipe(inc, sq)(3)` is `sq(inc(3))` = 16, and `compose(inc, sq)(3)` is `inc(sq(3))` = 10. For composition the functions must take one argument: that is why currying and partial application go side by side.

## Memoization

Memoization remembers the result by arguments; it fits only **pure** functions (the result depends only on the arguments, no side effects):

```js
function memo(fn) { const cache = new Map(); return (k) => { if (cache.has(k)) return cache.get(k); const v = fn(k); cache.set(k, v); return v; }; }
let calls = 0; const slowSq = memo(n => { calls++; return n * n; });
slowSq(4); slowSq(4); slowSq(5); console.log(calls);
const m2 = memo(o => o.a); console.log(m2({ a: 1 }), m2({ a: 1 }));
```

```text
2
1 1
```

- For numbers and strings the cache works: the function was called twice (for `4` and `5`), not three times.
- Object keys are compared **by reference** ([module 07](../07-arrays-and-collections/advanced.en.md)): `{ a: 1 }` twice is two different keys, and the cache grows. For such cases you need key serialization or a `WeakMap`.
- A `Map` cache is never cleared: for long-lived functions a memory leak is possible.

## once

```js
const once = fn => { let done = false, r; return (...a) => done ? r : (done = true, r = fn(...a)); };
const init = once(() => { console.log('init'); return 42; }); console.log(init(), init());
```

```text
init
42 42
```

The initialization ran once; repeated calls return the saved result.

## Immutability

Immutable data simplifies reasoning: a function does not change its arguments but returns a new value.

```js
'use strict';
const o = Object.freeze({ a: 1, n: { b: 2 } });
try { o.a = 2 } catch (e) { console.log(e.name + ': ' + e.message) }
o.n.b = 3; console.log(o.n.b, Object.isFrozen(o), Object.isFrozen(o.n));
```

```text
TypeError: Cannot assign to read only property 'a' of object '#<Object>'
3 true false
```

- `Object.freeze` is **shallow**: the nested object `o.n` stayed mutable.
- In sloppy mode an assignment is silently ignored; in strict mode ([module 02](../02-types-and-values/overview.en.md) and ES modules) it is a `TypeError`.
- Deep freezing is done recursively (`deepFreeze`), as shown in the [expert theory](expert.en.md#reference).

**Copies instead of mutations:**

```js
const s = { a: 1, n: { b: 2 } };
const s2 = { ...s, a: 5 }; console.log(s2.n === s.n, s.a);
const sc = structuredClone(s); console.log(sc.n === s.n, sc);
const arr = [3, 1, 2];
console.log(arr.toSorted(), arr, arr.toReversed(), arr.with(0, 9), arr.toSpliced(1, 1), arr);
```

```text
true 1
false { a: 1, n: { b: 2 } }
[ 1, 2, 3 ] [ 3, 1, 2 ] [ 2, 1, 3 ] [ 9, 1, 2 ] [ 3, 2 ] [ 3, 1, 2 ]
```

- `{ ...s }` is a **shallow** copy: `s2.n === s.n` is `true`.
- `structuredClone` makes a deep copy, but cannot copy functions (`DataCloneError`, see the [expert theory](expert.en.md#reference)).
- The methods `toSorted`, `toReversed`, `with`, `toSpliced` return a new array and do not change the original (unlike `sort`, `reverse`, `splice`).

## Best practices

- Pass explicit arrows to `map`/`filter` instead of "bare" functions with several parameters.
- Always set an initial value in `reduce`.
- Memoize only pure functions and limit the cache size.
- Copy before changing (spread, `toSorted`) instead of mutating arguments.
- Remember that `freeze` and spread are shallow.

## Common mistakes

- ❌ `['1', '2', '3'].map(parseInt)` → ✅ `map(Number)`.
- ❌ `reduce` without an initial value on a possibly empty array → ✅ `reduce(f, init)`.
- ❌ Currying a function with default values via `fn.length` → ✅ an explicit number of arguments.
- ❌ Memoization with an object as a key → ✅ a string key or a `WeakMap`.
- ❌ Expecting `Object.freeze` to protect nested objects → ✅ `deepFreeze`.

## Related materials

- Next: [expert theory](expert.en.md)
- Array methods: [07 · Arrays and collections](../07-arrays-and-collections/advanced.en.md)
- Functions, `bind`, `length`: [04 · Functions](../04-functions/advanced.en.md)

## Sources

- [MDN: Array.prototype.reduce](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Array/reduce)
- [MDN: Function.prototype.bind](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Function/bind)
- [MDN: Function: length](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Function/length)
- [MDN: Object.freeze](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Object/freeze)
- [MDN: structuredClone](https://developer.mozilla.org/en-US/docs/Web/API/Window/structuredClone)
- [MDN: Array.prototype.toSorted](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Array/toSorted)
