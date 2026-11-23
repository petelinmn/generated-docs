---
title: "Iterators and generators"
lang: en
track: javascript
kind: module
doc: overview
slug: 11-iterators-and-generators
part: 2
levels: [1, 2, 3, 4]
tags: [iterator, iterable, generator, iteration-protocol, async-iterator, async-generator, iterator-helpers, lazy-evaluation]
versions: "ECMAScript 2025+, Node.js 24.15 LTS"
status: draft
updated: 2026-11-23
---

# Iterators and generators

🌐 [Русский](overview.ru.md) · **English**

## In short

The iteration protocol is an agreement by which `for...of`, spread, destructuring, `Map`, `Set` and `Array.from` walk over values. An iterable can create an iterator, and the iterator hands out values one at a time through `next()`. A generator (`function*`) is the simplest way to write an iterator: a function that can be paused at `yield`. The asynchronous versions (`for await`, `async function*`) walk over values that arrive over time.

## Key ideas

- **An iterable** has a method `[Symbol.iterator]()` that returns an iterator.
- **An iterator** has `next()` that returns `{ value, done }`.
- **`for...of`, spread, destructuring** work with any iterable; a plain object `{}` is not iterable.
- **A generator** pauses at `yield` and resumes on `next()`; it is lazy and can be infinite.
- **`next(x)` passes a value into** the generator as the result of `yield`.
- **`return()` and `throw()`** finish a generator or throw an exception into it; `finally` runs.
- **An early exit from `for...of`** calls the iterator's `return()`: resources are released.
- **Iterator helpers** (`map`, `filter`, `take`, `drop`, `toArray`, …) work lazily directly on iterators.
- **Async iterators** (`Symbol.asyncIterator`, `async function*`, `for await`) walk over data as it arrives.

## Minimal example

```js
function* nat() { let n = 0; while (true) yield n++; }
console.log(nat().filter(x => x % 2).map(x => x * x).take(4).toArray());
```

```text
[ 1, 9, 25, 49 ]
```

The infinite sequence is processed lazily: only what is needed for four results is computed.

## When to use and when not

| Task | Choice |
|---|---|
| Your own type that supports `for...of` | `[Symbol.iterator]` (often a generator) |
| A lazy or infinite sequence | a generator |
| Paginated loading, a stream of events | an async generator + `for await` |
| A simple transformation of a ready array | array methods ([module 07](../07-arrays-and-collections/overview.en.md)) |
| Parallel requests | promises ([module 12](../12-promises-and-async-await/overview.en.md)), not `for await` |

- ❌ Do not use generators where an array is enough: overhead with no benefit.
- ❌ Do not expect `for await` to run steps in parallel: it is sequential.

## Material levels

| Level | What is inside |
|---|---|
| [1. Basic theory](basics.en.md) | the iteration protocol, `for...of`, your own iterable, generators, `yield`, laziness |
| [2. Advanced theory](advanced.en.md) | `next(x)`, `yield*`, `return`/`throw`, closing iterators, iterator helpers, async iterators |
| [3. Expert theory](expert.en.md) | a reference, internals, pitfalls, interview questions |
| [4. Self-check](quiz.en.md) | quiz and [exercises](exercises.en.md) |
| 5. Runnable examples | not created yet — `/learn javascript 11-iterators-and-generators level 5` |
| 6. Tests | not created yet — `/learn javascript 11-iterators-and-generators level 6` |

## Related materials

- Prerequisites: [07 · Arrays and collections](../07-arrays-and-collections/overview.en.md), [10 · Closures and functional style](../10-closures-and-functional/overview.en.md)
- Next: [12 · Promises and async/await](../12-promises-and-async-await/overview.en.md)
- Analogues in other tracks: [C#: LINQ and `yield return`](../../../csharp/modules/07-linq/overview.en.md), [Python](../../../python/overview.en.md) (generators and iterators)
