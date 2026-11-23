---
title: "Quiz: errors and debugging"
lang: en
track: javascript
kind: module
doc: quiz
slug: 14-errors-and-debugging
part: 2
tags: [error, try-catch, error-cause, custom-error, stack-trace]
status: draft
updated: 2026-11-23
---

# Quiz: errors and debugging

🌐 [Русский](quiz.ru.md) · **English**

> Answer on your own first, then open the answer. The label shows the question level: 🟢 [basic](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 What does `f()` return?

```js
function f() { try { return 'try' } finally { console.log('finally runs') } }
```

- A) Prints `finally runs`, returns `'try'`
- B) Returns `'try'`, `finally` does not run
- C) Prints `finally runs`, returns `undefined`
- D) Throws an error

<details><summary>Answer</summary>

**A.** `finally` always runs, then the result of `try` is returned.
→ [Basic theory](basics.en.md#throw-and-trycatchfinally)

</details>

## 2. 🟢 What does `f()` return?

```js
function f() { try { throw new Error('lost') } finally { return 'finally wins' } }
```

- A) Throws `Error: lost`
- B) `'finally wins'`
- C) `undefined`
- D) `'lost'`

<details><summary>Answer</summary>

**B.** A `return` in `finally` overrides the outcome of `try` and swallows the error.
→ [Gotcha 1](expert.en.md#1-return-in-finally-swallows-an-error)

</details>

## 3. 🟢 What does `Object.keys(new Error('x'))` print?

- A) `[ 'message' ]`
- B) `[ 'message', 'stack' ]`
- C) `[]`
- D) `[ 'name', 'message' ]`

<details><summary>Answer</summary>

**C.** The own `message` and `stack` are not enumerable.
→ [Basic theory](basics.en.md#the-error-object)

</details>

## 4. 🟢 What error does `undefinedVar` throw for an undeclared variable?

- A) `TypeError`
- B) `ReferenceError`
- C) `SyntaxError`
- D) `RangeError`

<details><summary>Answer</summary>

**B.** `ReferenceError: undefinedVar is not defined`.
→ [Basic theory](basics.en.md#built-in-error-types)

</details>

## 5. 🟡 What does the code print?

```js
class Plain extends Error { }
console.log(new Plain('x').name, String(new Plain('x')));
```

- A) `Plain Plain: x`
- B) `Error Error: x`
- C) `Error Plain: x`
- D) `Plain Error: x`

<details><summary>Answer</summary>

**B.** `name` stays `Error` until set explicitly: `this.name = this.constructor.name`.
→ [Advanced theory](advanced.en.md#custom-error-classes)

</details>

## 6. 🟡 What does `JSON.stringify(new Error('boom'))` print?

- A) `{"message":"boom"}`
- B) `{}`
- C) `"Error: boom"`
- D) `{"name":"Error","message":"boom"}`

<details><summary>Answer</summary>

**B.** The fields are not enumerable; you need `toJSON` or explicit copying.
→ [Advanced theory](advanced.en.md#serializing-errors)

</details>

## 7. 🟡 What will `cause` show?

```js
try { try { throw new Error('first') } catch (e) { throw new Error('second') } } catch (e) { console.log(e.message, e.cause) }
```

- A) `second Error: first`
- B) `second undefined`
- C) `first undefined`
- D) `second second`

<details><summary>Answer</summary>

**B.** Without `{ cause: e }` the original error is lost.
→ [Advanced theory](advanced.en.md#the-chain-of-causes-cause)

</details>

## 8. 🟡 What happens with `emit('error', new Error('x'))` on an `EventEmitter` without an `error` listener?

- A) Nothing
- B) The exception is thrown
- C) The event goes to `console.error`
- D) A promise is rejected

<details><summary>Answer</summary>

**B.** An `error` event without a listener throws (in the experiment `emit error throws: no listener`).
→ [Advanced theory](advanced.en.md#asynchronous-errors-and-top-level-handlers)

</details>

## 9. 🟡 Choose all true statements

- [ ] A) `try/catch` catches errors thrown in a `setTimeout` callback if it was created inside the `try`
- [ ] B) `catch { … }` without a parameter is allowed
- [ ] C) `Promise.any` throws `AggregateError` if all are rejected
- [ ] D) A good practice is to continue working after `uncaughtException`

<details><summary>Answer</summary>

**B, C.** A is wrong: the callback runs on a different stack; D is wrong: the program state is unknown, you must log and exit.
→ [Advanced theory](advanced.en.md#asynchronous-errors-and-top-level-handlers)

</details>

## 10. 🔴 Find the mistake

> "An error created in advance and thrown later from another function shows the throw site in its stack."

<details><summary>Answer</summary>

Stack frames are captured at **creation** of the object: the stack shows the place of `new Error`, not of `throw`.
→ [Gotcha 5](expert.en.md#5-the-stack-points-to-the-place-of-creation)

</details>

## 11. 🔴 What do `new Error('m', {})` and `new Error('m', { cause: undefined })` give for `'cause' in e`?

- A) Both `true`
- B) Both `false`
- C) The first `false`, the second `true`
- D) The first `true`, the second `false`

<details><summary>Answer</summary>

**C.** The `cause` property is created only if the options have a `cause` key, even with the value `undefined`.
→ [Gotcha 6](expert.en.md#6-the-outcome-of--cause--depends-on-the-key)

</details>

## 12. 🔴 What does `foreign instanceof Error` return for an error created in another context (`vm.runInNewContext('new Error("x")')`)?

- A) `true`
- B) `false`; `Error.isError` works for the check
- C) `TypeError`
- D) `null`

<details><summary>Answer</summary>

**B.** The other context has its own `Error`. `Error.isError(foreign)` returns `true`.
→ [Gotcha 4](expert.en.md#4-instanceof-error-does-not-work-for-a-foreign-context)

</details>

## 13. 🔴 Open question

Design error handling for a small service: which error classes, how to keep context, what to do in top-level handlers.

<details><summary>Key points of a good answer</summary>

- A base class `AppError` and subclasses (`NotFound`, `ValidationError`) with `name`, `code`/`status` and `toJSON`.
- Wrapping with `{ cause }`: context is added, the original error is kept.
- Catching by type in one place (for example in the request handler); unknown errors are rethrown.
- `uncaughtException` and `unhandledRejection` handlers log and terminate the process; `stack` is not given to the client.

</details>

→ [Advanced theory](advanced.en.md)
