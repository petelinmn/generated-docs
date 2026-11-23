---
title: "Quiz: iterators and generators"
lang: en
track: javascript
kind: module
doc: quiz
slug: 11-iterators-and-generators
part: 2
tags: [iterator, generator, yield, async-iterator, iterator-helpers]
status: draft
updated: 2026-11-23
---

# Quiz: iterators and generators

🌐 [Русский](quiz.ru.md) · **English**

> Answer on your own first, then open the answer. The label shows the question level: 🟢 [basic](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 What does an object need for `for...of` to work on it?

- A) A `length` property
- B) A method `[Symbol.iterator]()` returning an iterator
- C) A `forEach` method
- D) To be an array

<details><summary>Answer</summary>

**B.** That is the definition of an iterable. A plain object `{}` without such a method is `TypeError: {} is not iterable`.
→ [Basic theory](basics.en.md#the-iteration-protocol)

</details>

## 2. 🟢 What does `it.next()` return after the end of the sequence?

- A) `undefined`
- B) `{ value: undefined, done: true }`
- C) `null`
- D) An error

<details><summary>Answer</summary>

**B.** The end is signaled by the flag `done: true`.
→ [Basic theory](basics.en.md#the-iteration-protocol)

</details>

## 3. 🟢 What does the code print?

```js
function* gen() { const x = yield 1; return x; }
const g = gen();
console.log(g.next('a').value, g.next('b').value);
```

- A) `a b`
- B) `1 b`
- C) `1 a`
- D) `undefined b`

<details><summary>Answer</summary>

**B.** The argument of the first `next` is discarded; `next('b')` becomes the result of `yield 1` and is then returned via `return`.
→ [Basic theory](basics.en.md#generators)

</details>

## 4. 🟢 What does the code print?

```js
function* r() { yield 1; return 99; }
console.log([...r()]);
```

- A) `[ 1, 99 ]`
- B) `[ 1 ]`
- C) `[ 99 ]`
- D) `[]`

<details><summary>Answer</summary>

**B.** Spread ignores the `return` value.
→ [Basic theory](basics.en.md#laziness-and-infinite-sequences)

</details>

## 5. 🟢 What does a repeated `[...it]` give for `it = [1, 2, 3][Symbol.iterator]()`?

- A) `[ 1, 2, 3 ]`
- B) `[]`
- C) `[ 1 ]`
- D) An error

<details><summary>Answer</summary>

**B.** The iterator is exhausted; a new walk needs a new iterator.
→ [Gotcha 3](expert.en.md#3-an-iterator-is-consumed)

</details>

## 6. 🟡 What does the code print?

```js
function* inner() { yield 'a'; return 'ret'; }
function* outer() { const r = yield* inner(); yield r; }
console.log([...outer()]);
```

- A) `[ 'a' ]`
- B) `[ 'a', 'ret' ]`
- C) `[ 'ret' ]`
- D) `[ 'a', undefined ]`

<details><summary>Answer</summary>

**B.** The result of `yield*` is the `return` value of the nested generator.
→ [Advanced theory](advanced.en.md#yield-delegation)

</details>

## 7. 🟡 How many times is the iterator's `return()` called?

```js
for (const x of withReturn) { if (x === 1) break; }
const [first] = withReturn;
```

(`withReturn` is an infinite iterable with a `return()` method.)

- A) `0`
- B) `1`
- C) `2`
- D) Endlessly

<details><summary>Answer</summary>

**C.** Both the early `break` and the incomplete destructuring close the iterator.
→ [Advanced theory](advanced.en.md#closing-iterators)

</details>

## 8. 🟡 What does the code print?

```js
const log = []; const lazy = [1, 2, 3].values().map(x => { log.push(x); return x; }).take(2);
console.log(log);
```

- A) `[ 1, 2, 3 ]`
- B) `[ 1, 2 ]`
- C) `[]`
- D) `[ 1 ]`

<details><summary>Answer</summary>

**C.** Iterator helpers are lazy: without a consumer the functions are not called.
→ [Advanced theory](advanced.en.md#iterator-helpers)

</details>

## 9. 🟡 Choose all true statements

- [ ] A) `for await` can also walk ordinary iterables
- [ ] B) `for await` runs iterations in parallel
- [ ] C) An error in `async function*` is caught by a `try/catch` around `for await`
- [ ] D) `break` in `for await` calls the iterator's `return()`

<details><summary>Answer</summary>

**A, C, D.** B is wrong: `for await` is sequential.
→ [Advanced theory](advanced.en.md#async-iterators-and-generators)

</details>

## 10. 🔴 Find the mistake

> "`new gen()` creates a new generator if `gen` is declared as `function* gen() {}`."

<details><summary>Answer</summary>

Generators cannot be called with `new`: `TypeError: gen is not a constructor`. A generator is created by an ordinary call `gen()`.
→ [Gotcha 5](expert.en.md#5-new-on-a-generator)

</details>

## 11. 🔴 What does the code print?

```js
function* y2() { const r = yield 1 + 1; console.log('r', r); }
const yy = y2(); console.log(yy.next().value); yy.next('v');
```

- A) `1`, then `r v`
- B) `2`, then `r v`
- C) `2`, then `r undefined`
- D) `SyntaxError`

<details><summary>Answer</summary>

**B.** `yield` has a low precedence: `1 + 1` = `2` is handed out; `next('v')` becomes the result of `yield`.
→ [Gotcha 9](expert.en.md#9-yield-without-parentheses-in-an-expression)

</details>

## 12. 🔴 What does `[...'👨‍👩'].length` print?

- A) `1`
- B) `2`
- C) `3`
- D) `6`

<details><summary>Answer</summary>

**C.** A string is iterated by code points: man, zero-width joiner, woman.
→ [Gotcha 7](expert.en.md#7-a-string-by-code-points)

</details>

## 13. 🔴 Open question

Explain how a generator works "under the hood" and why it suits lazy and infinite sequences.

<details><summary>Key points of a good answer</summary>

- A call of `function*` does not run the body but creates an object in the suspendedStart state.
- `next()` resumes execution up to `yield`; local variables and the stopping point are kept between calls (a state machine).
- Values are computed on request, so a sequence can be infinite, and the consumer decides where to stop (`take`, `break`).
- Closing (`return()`, `finally`) releases resources; the prototype chain leads to `Iterator.prototype` with the helpers.

</details>

→ [Expert theory](expert.en.md#internals)
