---
title: "Iterators and generators: basics"
lang: en
track: javascript
kind: module
doc: basics
slug: 11-iterators-and-generators
part: 2
tags: [iterator, iterable, for-of, spread, generator, yield, lazy-evaluation]
versions: "ECMAScript 2025+, Node.js 24.15 LTS"
status: draft
updated: 2026-11-23
---

# Iterators and generators: basics

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [07 · Arrays and collections](../07-arrays-and-collections/overview.en.md), [10 · Closures and functional style](../10-closures-and-functional/overview.en.md) · **Time:** ~2.5 h

All examples were run on Node.js 24.15; the files sit next to a `package.json` (`{"type": "module"}`).

## Why it matters

Walking over values is everywhere: arrays, strings, `Map`, `Set`, query results. The iteration protocol gives one way to walk over any source and lets you describe your own. Generators give lazy sequences, including infinite ones.

## The iteration protocol

An iterator is an object with a method `next()` that returns `{ value, done }`. An iterable is an object that has a method `[Symbol.iterator]()` returning an iterator:

```js
const it = [10, 20][Symbol.iterator]();
console.log(it.next(), it.next(), it.next(), typeof it[Symbol.iterator], it[Symbol.iterator]() === it);
console.log(Object.prototype.toString.call(it), Object.prototype.toString.call(function* () {}()));
```

```text
{ value: 10, done: false } { value: 20, done: false } { value: undefined, done: true } function true
[object Array Iterator] [object Generator]
```

- While values remain, `done` is `false`; after the end — `{ value: undefined, done: true }`.
- Built-in iterators have their own `[Symbol.iterator]` that returns themselves (`true`): an iterator is itself iterable.

## What accepts iterables

```js
console.log([...'a😀b'], [...new Map([[1, 'x']])], [...new Set([1, 1, 2])], Object.entries({ a: 1 }));
console.log([...[1, 2].entries()], [...[1, 2].keys()]);
```

```text
[ 'a', '😀', 'b' ] [ [ 1, 'x' ] ] [ 1, 2 ] [ [ 'a', 1 ] ]
[ [ 0, 1 ], [ 1, 2 ] ] [ 0, 1 ]
```

- A string is iterated by **Unicode code points**, not code units: `😀` is one element ([module 08](../08-strings-numbers-dates-regex/overview.en.md)).
- `Map` yields `[key, value]` pairs, `Set` yields values without repeats.
- Consumers of iterables: `for...of`, spread (`...`), destructuring, `Array.from`, `new Map(...)`, `new Set(...)`, `Promise.all`, and others.
- A plain object is not iterable: `Object.entries(obj)` turns it into an iterable array of pairs.

```js
try { for (const x of {}) ; } catch (e) { console.log(e.name + ': ' + e.message) }
try { [...123] } catch (e) { console.log(e.name + ': ' + e.message) }
```

```text
TypeError: {} is not iterable
TypeError: 123 is not iterable
```

## Your own iterable

```js
class Range {
  constructor(a, b) { this.a = a; this.b = b }
  [Symbol.iterator]() {
    let i = this.a, b = this.b;
    return { next: () => i < b ? { value: i++, done: false } : { value: undefined, done: true } };
  }
}
console.log([...new Range(0, 3)], Array.from(new Range(1, 4)), Math.max(...new Range(0, 5)));
const [p, q] = new Range(5, 10); console.log(p, q);
for (const v of new Range(0, 2)) console.log('v', v);
```

```text
[ 0, 1, 2 ] [ 1, 2, 3 ] 4
5 6
v 0
v 1
```

The state (`i`) lives in a closure: every call of `[Symbol.iterator]()` creates an independent walk.

## Generators

A function with an asterisk, `function*`, is a generator. A call does not run the body but returns a generator object; `yield` pauses execution and hands out a value:

```js
function* gen() { console.log('start'); const x = yield 1; console.log('got', x); const y = yield x * 2; return y; }
const g = gen();
console.log(g.next('ignored')); console.log(g.next(5)); console.log(g.next('end')); console.log(g.next());
```

```text
start
{ value: 1, done: false }
got 5
{ value: 10, done: false }
{ value: 'end', done: true }
{ value: undefined, done: true }
```

- The body starts running only at the first `next()`; the argument of the first `next` is discarded.
- `next(5)` returns `5` as the result of the expression `yield 1`.
- `return` sets the last value with `done: true`; after that — `{ value: undefined, done: true }`.
- A generator can be walked **once**: a repeated `[...once]` gives an empty array.

The same `Range` is shorter with a generator:

```js
class Countdown { constructor(n) { this.n = n } *[Symbol.iterator]() { for (let i = this.n; i > 0; i--) yield i; } }
console.log([...new Countdown(3)]);
```

```text
[ 3, 2, 1 ]
```

## Laziness and infinite sequences

A generator computes the next value only on request, so it can be infinite:

```js
function* nat() { let n = 0; while (true) yield n++; }
function* take(n, it) { let i = 0; for (const v of it) { if (i++ >= n) return; yield v; } }
console.log([...take(5, nat())]);
function* r() { yield 1; return 99; } console.log([...r()]);
```

```text
[ 0, 1, 2, 3, 4 ]
[ 1 ]
```

- `take` stops the infinite walk: `for...of` does not reach the end.
- `for...of` and spread **ignore** the `return` value (`99` did not get into the result).

## Beginner mistakes

- ❌ Walking a plain object with `for...of` → ✅ `Object.entries(obj)`.
- ❌ Expecting `[...nat()]` of an infinite generator to finish → ✅ limit with `take` or `break`.
- ❌ Reusing a generator → ✅ create a new one by calling the function.
- ❌ Expecting `return` to produce a value in `for...of` → ✅ use `yield`.
- ❌ Calling `new` on a generator → ✅ an ordinary call (`new gen()` is a `TypeError`).

## Related materials

- Next: [advanced theory](advanced.en.md) — a two-way `yield`, `yield*`, helpers, async iterators
- Earlier: [07 · Arrays and collections](../07-arrays-and-collections/basics.en.md#iteration)

## Sources

- [MDN: Iteration protocols](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Iteration_protocols)
- [MDN: function*](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Statements/function*)
- [MDN: yield](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Operators/yield)
- [MDN: for...of](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Statements/for...of)
