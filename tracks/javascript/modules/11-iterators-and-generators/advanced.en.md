---
title: "Iterators and generators: advanced"
lang: en
track: javascript
kind: module
doc: advanced
slug: 11-iterators-and-generators
part: 2
tags: [generator, yield-delegation, iterator-return, iterator-helpers, async-generator, for-await, array-fromasync]
versions: "ECMAScript 2025+, Node.js 24.15 LTS"
status: draft
updated: 2026-11-23
---

# Iterators and generators: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

Builds on [basic theory](basics.en.md). All examples were run on Node.js 24.15.

## A two-way yield

`yield` is both an exit and an entry: the value from `next(x)` becomes the result of the `yield` expression. This turns a generator into a "coroutine":

```js
function* avg() { let sum = 0, n = 0; while (true) { const x = yield n ? sum / n : undefined; sum += x; n++; } }
const a = avg(); a.next(); console.log(a.next(10).value, a.next(20).value, a.next(60).value);
```

```text
10 15 30
```

The first `next()` only starts the generator up to the first `yield`; after that each `next(x)` passes a number and returns the current average.

## yield*: delegation

`yield*` hands out the values of another iterable, and its result is the `return` value of the nested generator:

```js
function* inner() { yield 'a'; return 'ret'; }
function* outer() { const r = yield* inner(); yield r; yield* [1, 2]; }
console.log([...outer()]);
```

```text
[ 'a', 'ret', 1, 2 ]
```

The `return` value of the nested generator (`'ret'`) went into `r` and was handed out by a separate `yield`.

## return() and throw()

`return(v)` finishes a generator early, `throw(e)` throws an exception at the `yield` point. `try`/`catch`/`finally` blocks inside a generator work:

```js
function* t() { try { yield 1; yield 2 } catch (e) { console.log('caught', e); yield 'recovered' } finally { console.log('cleanup') } }
const tg = t(); tg.next(); console.log(tg.throw('boom')); console.log(tg.return('bye')); console.log(tg.next());
const tg2 = t(); tg2.next(); console.log(tg2.return('early'));
```

```text
caught boom
{ value: 'recovered', done: false }
cleanup
{ value: 'bye', done: true }
{ value: undefined, done: true }
cleanup
{ value: 'early', done: true }
```

- `throw('boom')` was caught inside the generator, which continued and handed out `'recovered'`.
- `return('bye')` ran `finally` (`cleanup`) and finished the generator.
- For a finished generator `next()` returns `{ value: undefined, done: true }`.

## Closing iterators

If `for...of` is interrupted (`break`, `return`, an exception) or destructuring takes not all values, the iterator's `return()` is called — if it exists:

```js
const withReturn = { [Symbol.iterator]() { let i = 0; return { next: () => ({ value: i++, done: false }), return() { console.log('return called'); return { done: true }; } }; } };
for (const x of withReturn) { if (x === 1) break; }
const [first] = withReturn;
```

```text
return called
return called
```

That is why a generator with `try/finally` releases a resource on `break`. After a **normal** completion `return()` is not called; on an exception in the loop body it is.

## Iterator helpers

In ES2025 iterators have methods that work lazily and create no intermediate arrays: `map`, `filter`, `take`, `drop`, `flatMap`, `reduce`, `toArray`, `forEach`, `some`, `every`, `find`. They are available in Node.js 24:

```js
function* nat() { let n = 0; while (true) yield n++; }
console.log(nat().drop(3).take(2).toArray(), nat().take(5).reduce((a, b) => a + b, 0));
console.log(nat().take(5).some(x => x > 3), nat().take(5).every(x => x < 5), nat().find(x => x > 10));
console.log([1, 2].values().flatMap(x => [x, x * 10]).toArray());
console.log(typeof Iterator, Iterator.from([1, 2]).toArray());
```

```text
[ 3, 4 ] 10
true true 11
[ 1, 10, 2, 20 ]
function [ 1, 2 ]
```

- Helpers belong to **iterators**, not arrays: `[1, 2].values().map(...)` works, while `[1, 2].map(...)` is the ordinary array method. A helper's result is also an iterator (`[object Iterator Helper]`), so you need `toArray()` at the end.
- They are **lazy**: without a consumer the functions are not called:

```js
const log = []; const lazy = [1, 2, 3].values().map(x => { log.push(x); return x; }).take(2);
console.log(log); lazy.toArray(); console.log(log);
```

```text
[]
[ 1, 2 ]
```

- The result of `map` has no `length`: `[1, 2].values().map(x => x).length` is `undefined`.
- `Iterator.from(obj)` wraps an iterable or an object with `next()` into a real iterator.

## Async iterators and generators

An async iterator returns a **promise** of the result from `next()`. An async generator, `async function*`, uses `await` and `yield` together; it is walked with `for await...of`:

```js
async function* ag() { yield 1; await null; yield 2; }
for await (const v of ag()) console.log('av', v);
const a = ag(); console.log(await a.next(), await a.return('r'), await a.next());
console.log(Object.prototype.toString.call(a), typeof a[Symbol.asyncIterator]);
```

```text
av 1
av 2
{ value: 1, done: false } { value: 'r', done: true } { value: undefined, done: true }
[object AsyncGenerator] function
```

`for await` also works with ordinary iterables: it waits for values that are promises:

```js
for await (const v of [Promise.resolve('p1'), 'plain']) console.log(v);
```

```text
p1
plain
```

A typical scenario is paginated loading or a stream of events:

```js
const sleep = ms => new Promise(r => setTimeout(r, ms));
async function* pages(n) { for (let p = 1; p <= n; p++) { await sleep(1); yield { page: p, items: [p * 10, p * 10 + 1] }; } }
const all = []; for await (const { items } of pages(3)) all.push(...items); console.log(all);
```

```text
[ 10, 11, 20, 21, 30, 31 ]
```

- `for await` is **sequential**: the next step begins after the previous one finishes; for parallelism you need promises ([module 12](../12-promises-and-async-await/overview.en.md)).
- An error in an async generator becomes a rejected promise and is caught by a `try/catch` around `for await` (in the experiment `caught oops`).
- An early `break` calls `return()`: `finally` in an async generator runs (in the experiment `released`).
- `Array.fromAsync(ag())` collects an async iterable into an array: `[ 1, 2 ]`; for an array with promises too (`[ 1, 2 ]`).

## Best practices

- Make `[Symbol.iterator]` a generator: simpler and closed correctly.
- Always limit infinite generators (`take`, `break`).
- Release resources in the generator's `finally`.
- For long pipelines use iterator helpers instead of intermediate arrays.
- Do not use `for await` for independent requests that can be started simultaneously.

## Common mistakes

- ❌ Forgetting that the first `next(x)` discards `x` → ✅ the first call is a start.
- ❌ Expecting `yield*` to return "values" instead of `return` → ✅ the result of `yield*` is the nested `return`.
- ❌ Calling `.map` on a generator in an environment without iterator helpers → ✅ check the Node.js version (24 supports it).
- ❌ Parsing a big stream into an array in full → ✅ walk it lazily.
- ❌ Relying on `return()` after a normal completion → ✅ it is not called.

## Related materials

- Next: [expert theory](expert.en.md)
- Promises and `async`/`await`: [12 · Promises and async/await](../12-promises-and-async-await/overview.en.md)
- Array methods: [07 · Arrays and collections](../07-arrays-and-collections/advanced.en.md)

## Sources

- [MDN: Iterator helpers](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Iterator)
- [MDN: yield*](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Operators/yield*)
- [MDN: Generator.prototype.return](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Generator/return)
- [MDN: for await...of](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Statements/for-await...of)
- [MDN: async function*](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Statements/async_function*)
- [MDN: Array.fromAsync](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Array/fromAsync)
