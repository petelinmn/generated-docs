---
title: "Iterators and generators: expert"
lang: en
track: javascript
kind: module
doc: expert
slug: 11-iterators-and-generators
part: 2
tags: [iteration-protocol, generator-states, iterator-prototype, async-iteration, iterator-close, for-await, events-on]
versions: "ECMAScript 2025+, Node.js 24.15 LTS"
status: draft
updated: 2026-11-23
---

# Iterators and generators: expert

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · **3. Expert** · [4. Quiz](quiz.en.md)

Builds on [basic](basics.en.md) and [advanced](advanced.en.md) theory. All results were obtained by running on Node.js 24.15.

## Reference

### Protocols

| Protocol | What is required | Result of `next()` |
|---|---|---|
| Iterable | a method `[Symbol.iterator]()` that returns an iterator | — |
| Iterator | a method `next()`; `return()` and `throw()` are optional | `{ value, done }` |
| Async iterable | a method `[Symbol.asyncIterator]()` | — |
| Async iterator | a method `next()` | a promise of `{ value, done }` |

### Methods of a generator object

| Method | Action | Result in the experiment |
|---|---|---|
| `next(x)` | resumes; `x` is the result of `yield` | `{ value, done }`; for the first call `x` is discarded |
| `return(v)` | finishes; runs `finally` | `{ value: v, done: true }` |
| `throw(e)` | throws `e` at the `yield` point | the result of the next `yield` or an exception outward |

For a generator that **has not started**, `return('x')` finishes it at once (`{ value: 'x', done: true }`, then `next()` → `{ value: undefined, done: true }`). For a **completed** one, `throw(err)` throws the exception outward (in the experiment `rethrown late`).

### Generator states

| State | When |
|---|---|
| suspendedStart | created, `next()` not called yet |
| suspendedYield | stopped at a `yield` |
| executing | the body is running |
| completed | returned, threw, or was finished by `return()` |

Calling `next()` on a running generator is `TypeError: Generator is already running` (in the experiment).

### Iterator helpers (in the experiment)

`Iterator.prototype`: `drop`, `every`, `filter`, `find`, `flatMap`, `forEach`, `map`, `reduce`, `some`, `take`, `toArray`. Static members of `Iterator`: `from`. `new Iterator()` is `TypeError: Abstract class Iterator not directly constructable`; `typeof Iterator` is `function`.

### Consumers of iterables and closing

| Consumer | Calls `return()` |
|---|---|
| `for...of` on `break`/`return`/an exception | yes |
| `for...of` on normal completion | no |
| Destructuring that took not all values (`[a, b] = it`) | yes (in the experiment `closed`) |
| Destructuring that exhausted the iterator | no |
| Spread, `Array.from` | no (they read to the end) |

## Internals

- **The generator's prototype chain.** A generator object → `g.prototype` (every generator function has its own) → `%GeneratorPrototype%` with the methods `next`, `return`, `throw` → `Iterator.prototype` (in the experiment all links are `true`). That is why a generator has iterator helpers and a `[Symbol.iterator]` returning the object itself (`go[Symbol.iterator]() === go` is `true`).
- **A state machine.** A generator is a function that the engine turns into a state machine: local variables and the stopping point are kept in the object between `next()` calls. Hence the states in the table above.
- **`yield` and precedence.** `yield` has a very low precedence: `yield 1 + 1` hands out `2` (in the experiment). The result of `yield` is usually parenthesized: `const x = (yield a) + 1`.
- **`yield` in context.** `[yield 'a', yield 'b']` is two suspensions; the values from `next(1)` and `next(2)` are collected into an array (in the experiment `{ value: [ 1, 2 ], done: true }`).
- **Closing an iterator.** The engine calls `return()` in operations that finish before the end. An error in `return()` itself on a normal exit replaces the result; with an exception the original error is kept.

> ⚠️ Verify: the exact rules of error precedence from `return()` and the behavior on exit through `throw` in a loop were taken from the specification, not by running.

- **Async generators and the queue.** An async generator has an internal queue of requests: several consecutive `next()` calls made without waiting are executed in order (in the experiment four parallel `next()` calls gave `1:false 2:false 3:false undefined:true`).
- **`for await` and synchronous iterables.** If an object has no `[Symbol.asyncIterator]`, `for await` takes `[Symbol.iterator]` and awaits every value (in the experiment `fa x`, `fa y`). If both exist, the async one is used.
- **Closing in `for await`.** `break` calls the async iterator's `return()` (in the experiment `async closed`).
- **Node.js streams.** Readable streams (`stream.Readable`) and `events.on(emitter, name)` implement async iteration: `for await (const [v] of on(ee, 'd', { signal }))` gets `ev 1`, `ev 2`, and on the abort signal ends with `AbortError` (in the experiment).

> ⚠️ Verify: iterating `Readable` streams with `for await` and the details of backpressure were not run (see [module 16](../../overview.en.md) of the roadmap).

## Gotchas

### 1. The first `next(x)` discards `x`

`g.next('ignored')` — the value goes nowhere; the generator only starts up to the first `yield`.

### 2. `for...of` does not see `return`

`function* r() { yield 1; return 99 }` → `[...r()]` is `[ 1 ]`.

### 3. An iterator is consumed

`const it = [1, 2, 3][Symbol.iterator](); [...it]` gives `[ 1, 2, 3 ]`, a second time `[]`. If some values were already taken, spread returns the rest (`[ 2, 3 ]`).

### 4. `return()` is not always called

Not called after a normal completion and with spread; called on `break`, `throw` in the loop body and incomplete destructuring.

### 5. `new` on a generator

`new gen()` is `TypeError: gen is not a constructor`.

### 6. Re-entry

Calling `next()` from inside the generator itself is `TypeError: Generator is already running`.

### 7. A string by code points

`[...'👨‍👩'].length` is `3` (man, zero-width joiner, woman), not `1`: iteration by code points is not iteration by "visible characters" ([module 08](../08-strings-numbers-dates-regex/expert.en.md)).

### 8. `for await` is sequential

It will not run tasks in parallel; for parallelism use `Promise.all` ([module 12](../12-promises-and-async-await/overview.en.md)).

### 9. `yield` without parentheses in an expression

`const x = yield a + 1` hands out `a + 1`, not `a`: the precedence of `yield` is low.

## Trade-offs and alternatives

| Approach | Pros | Cons |
|---|---|---|
| A generator | short code, laziness, closing | no "looking back", a single pass |
| A hand-written iterator | full control, `return()` | a lot of code |
| A whole array | simplicity, `length`, indexes | all memory at once |
| Iterator helpers | lazy pipelines without arrays | a modern environment is needed (available in Node.js 24; other versions and browsers were not checked) |
| Async generators | stream processing | sequential, harder to debug |
| Streams | backpressure, bytes | a more complex API; not run |
| Third-party libraries (RxJS etc.) | operators, cancellation | a dependency; not run |

## Interview questions

<details><summary>1. How does an iterable differ from an iterator?</summary>

An iterable has `[Symbol.iterator]()` returning an iterator; an iterator has `next()` and returns `{ value, done }`. Built-in iterators are themselves iterable.

→ [Basic theory](basics.en.md#the-iteration-protocol)

</details>

<details><summary>2. Which operations consume iterables?</summary>

`for...of`, spread, destructuring, `Array.from`, `new Map`/`new Set`, `Promise.all`, and others.

→ [Basic theory](basics.en.md#what-accepts-iterables)

</details>

<details><summary>3. What does <code>yield</code> do?</summary>

It pauses the generator and hands a value to the caller; the result of the expression will be the value from the next `next(x)`.

→ [Advanced theory](advanced.en.md#a-two-way-yield)

</details>

<details><summary>4. Trick: what does the first <code>next('x')</code> return?</summary>

The value of the first `yield`; the argument `'x'` is discarded.

→ [Gotcha 1](#1-the-first-nextx-discards-x)

</details>

<details><summary>5. How does <code>yield*</code> differ from <code>yield</code>?</summary>

It delegates to a nested iterable; the result of `yield*` is the nested generator's `return`.

→ [Advanced theory](advanced.en.md#yield-delegation)

</details>

<details><summary>6. When is an iterator's <code>return()</code> called?</summary>

On an early exit from `for...of` (`break`, `return`, an exception) and incomplete destructuring; not after a normal completion and not in spread.

→ [Closing iterators](advanced.en.md#closing-iterators)

</details>

<details><summary>7. Trick: what does <code>[...r()]</code> print for <code>function* r() { yield 1; return 99 }</code>?</summary>

`[ 1 ]`: the `return` value does not get into the result.

→ [Gotcha 2](#2-forof-does-not-see-return)

</details>

<details><summary>8. How do you make an infinite sequence safe?</summary>

Limit it with `take(n)` or `break`; the `take`/`drop` helpers are lazy.

→ [Basic theory](basics.en.md#laziness-and-infinite-sequences)

</details>

<details><summary>9. How does an async iterator differ from a synchronous one?</summary>

`next()` returns a promise; the method is named `[Symbol.asyncIterator]`; it is walked with `for await`.

→ [Advanced theory](advanced.en.md#async-iterators-and-generators)

</details>

<details><summary>10. Trick: does <code>for await</code> run iterations in parallel?</summary>

No, sequentially: the next step begins after the previous one.

→ [Gotcha 8](#8-for-await-is-sequential)

</details>

<details><summary>11. What happens if you call <code>throw</code> on a completed generator?</summary>

The exception is thrown outward.

→ [Reference](#methods-of-a-generator-object)

</details>

<details><summary>12. Why does a generator need <code>try/finally</code>?</summary>

To release a resource on `break`, `return()` and an exception: `finally` runs on closing.

→ [Advanced theory](advanced.en.md#return-and-throw)

</details>

## Cheat sheet

```text
obj[Symbol.iterator]() → { next() → { value, done }, return?() }
function* g() { const x = yield v; return r }   // g.next(x) / return(v) / throw(e)
yield* other   // delegation; the result is the nested return
for (const v of it) {…}   // break → it.return()
async function* ag() { yield await p }   // for await (const v of ag())
it.map(f).filter(p).take(n).toArray()   // iterator helpers (checked on Node.js 24)
```

- The first `next(x)` ignores `x`; `for...of` ignores `return`.
- A generator is walked once.
- `for await` is sequential.

## Related materials

- Previous level: [advanced theory](advanced.en.md)
- Arrays, `Map`, `Set`: [07 · Arrays and collections](../07-arrays-and-collections/expert.en.md)
- Closures: [10 · Closures and functional style](../10-closures-and-functional/expert.en.md)
- Asynchrony: [12 · Promises and async/await](../12-promises-and-async-await/overview.en.md)

## Sources

- [ECMAScript: Iteration](https://tc39.es/ecma262/#sec-iteration)
- [ECMAScript: Generator Objects](https://tc39.es/ecma262/#sec-generator-objects)
- [MDN: Iteration protocols](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Iteration_protocols)
- [MDN: Generator](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Generator)
- [Node.js: events.on](https://nodejs.org/docs/latest-v24.x/api/events.html#eventsonemitter-eventname-options)
