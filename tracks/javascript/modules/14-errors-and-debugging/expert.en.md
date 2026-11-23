---
title: "Errors and debugging: expert"
lang: en
track: javascript
kind: module
doc: expert
slug: 14-errors-and-debugging
part: 2
tags: [error, stack-trace, capture-stack-trace, prepare-stack-trace, error-cause, dom-exception, error-is-error, finally]
versions: "ECMAScript 2025+, Node.js 24.15 LTS"
status: draft
updated: 2026-11-23
---

# Errors and debugging: expert

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · **3. Expert** · [4. Quiz](quiz.en.md)

Builds on [basic](basics.en.md) and [advanced](advanced.en.md) theory. All results were obtained by running on Node.js 24.15.

## Reference

### The constructor and properties

| Member | What it is | Notes (in the experiment) |
|---|---|---|
| `new Error(message, { cause })` | the constructor; `Error(...)` without `new` also creates an error | `Error('no new') instanceof Error` is `true`; `Error.length` is `1` |
| `message` | an own property, **not enumerable** | with `new Error()` and no argument there is no own property; `new Error(42).message` is `'42'` (a string), `new Error({ a: 1 }).message` is `'[object Object]'` |
| `name` | a prototype property (`Error.prototype.name`) | in a subclass without `this.name = …` it stays `'Error'` |
| `cause` | an own property, **only if** the `cause` key is passed in the options | `{ cause: 0 }` → `0`; `{}` → no property (`'cause' in e` is `false`); `{ cause: undefined }` → the property exists |
| `stack` | a non-standard V8 string property | the first line is `name: message` |
| `Error.prototype.toString()` | `name: message` | for `{ name: 'N', message: 'M' }` → `N: M`, for `{}` → `Error` |
| `Error.captureStackTrace(obj, fn)` | V8: write a stack into `obj`, hiding the frames above `fn` | `obj` gets a `stack` |
| `Error.stackTraceLimit` | V8: the number of frames (`10` by default) | `1` → two lines in `stack` |
| `Error.prepareStackTrace(err, frames)` | V8: your own stack format | `frames[0].getLineNumber()` is a number |
| `Error.isError(x)` | a check for a "real" error, including from another context | in Node.js 24.15 it is a `function` |
| `AggregateError(errors, message)` | `errors` is an iterable | inherits from `Error` |
| `DOMException(message, name)` | a web exception, available in Node.js | `de instanceof Error` is `true`, `de.code` is `20` for `AbortError`, tag `[object DOMException]` |

The properties of `Error.prototype` (in the experiment): `constructor`, `name`, `message`, `toString`.

### Built-in errors and Node.js codes

| Source | What is thrown |
|---|---|
| Language errors | `TypeError`, `RangeError`, `ReferenceError`, `SyntaxError`, `URIError` |
| System operations | `Error` with `code` (`ENOENT`), `syscall`, `errno` |
| Node.js internal errors | `Error` with a `code` like `ERR_*` (`ERR_MODULE_NOT_FOUND`, `ERR_UNHANDLED_ERROR`, `ERR_ASSERTION`) |
| `node:assert` | `AssertionError` (`code: 'ERR_ASSERTION'`, `generatedMessage`) |
| Cancellation | `AbortError` (`DOMException`, `code` `20`), `TimeoutError` |

## Internals

- **When the stack is built.** The **frames** are captured at **creation** of the object, not at `throw`: an error created on line 1 and thrown from another function shows the creation line (in the experiment `d.js:1:16`, `e === shared`). So creating a "shared" constant error in advance is harmful: it has a foreign stack.
- **The stack text.** The `stack` string is formatted lazily, on first access: in the experiment after `m.message = 'new'` the stack header was `Error: new`, and after `n.name = 'Custom'` — `Custom: x`. Do not rely on this behavior: it is not defined by the standard.
- **`stack` is not part of the language.** The ECMAScript standard does not describe `stack`; the V8 format (`at function (file:line:column)`) differs in other engines.
- **Derived classes.** `class X extends Error` calls `super(message, options)`, which creates the object, captures the stack and sets `cause`. Before `super`, touching `this` is a `ReferenceError: Must call super constructor in derived class before accessing 'this' or returning from derived constructor` (in the experiment).
- **`finally` and overriding.** A `return` or `throw` in `finally` replaces the outcome of `try`: `throw new Error('from finally')` overrode `inner`.
- **Unhandled errors.** An exception without `catch` reaches the event loop and turns into an `uncaughtException` event; without a handler the process terminates with a nonzero code ([module 13](../13-event-loop-and-concurrency/expert.en.md#gotchas)).
- **Async frames.** V8 restores the chain of `await`s, so `at async a2` is visible in the stack (in the experiment).

> ⚠️ Verify: the format and content of `stack` in other environments (Firefox, Safari, Deno, Bun), the behavior of `Error.prepareStackTrace` with nested calls and the exact semantics of lazy header formatting per the specification were not checked.

## Gotchas

### 1. `return` in `finally` swallows an error

`f2` returned `'finally wins'`; the thrown `lost` is lost.

### 2. A subclass without `name`

`new Plain('x').name` is `Error`, although `constructor.name` is `Plain`.

### 3. `JSON.stringify(error)` is `{}`

The `message` and `stack` fields are not enumerable; you need `toJSON`.

### 4. `instanceof Error` does not work for a foreign context

A `vm` error: `foreign instanceof Error` is `false`, while `Error.isError(foreign)` is `true`.

### 5. The stack points to the place of creation

An error created in advance and thrown later shows the old stack.

### 6. The outcome of `{ cause }` depends on the key

`new Error('m', {})` does not create a `cause` property; `{ cause: undefined }` does.

### 7. A new error in `catch` without `cause` loses the original

`throw new Error('second')` inside `catch` leaves `cause === undefined`.

### 8. A throw after `resolve` in a promise is ignored

`new Promise(res => { res('ok'); throw new Error('ignored') })` is fulfilled with `'ok'` (in the experiment).

### 9. `console.log(error)` shows more than `String(error)`

`console.log` prints `stack` and `cause`; `String` prints only `name: message`.

## Trade-offs and alternatives

| Approach | Pros | Cons |
|---|---|---|
| Exceptions | a stack, bubbling, `finally` | expensive when used often; easy to swallow |
| Returning a result (`{ ok, value, error }`) | explicit handling, no exceptions | boilerplate, easy to forget the check |
| Error codes (`code`) | stable, serializable | needs discipline |
| A class hierarchy | catching by type | `instanceof` breaks across contexts |
| `assert` | quick invariant checks | only for programmer errors |
| Result libraries (neverthrow etc.) | type-safe results | a dependency; not run |

## Interview questions

<details><summary>1. How does <code>throw</code> differ from <code>return</code> with an error?</summary>

`throw` interrupts execution and "bubbles" up the stack to a `catch`; `return` just hands a value to the caller, who must check it.

→ [Basic theory](basics.en.md#throw-and-trycatchfinally)

</details>

<details><summary>2. What runs in <code>finally</code> after a <code>return</code> in <code>try</code>?</summary>

The `finally` block always runs, then the result of `try` is returned (unless `finally` overrides the outcome).

→ [Basic theory](basics.en.md#throw-and-trycatchfinally)

</details>

<details><summary>3. Trick: what does <code>f()</code> return if <code>try</code> throws and <code>finally</code> has <code>return 'x'</code>?</summary>

`'x'`: the error is swallowed.

→ [Gotcha 1](#1-return-in-finally-swallows-an-error)

</details>

<details><summary>4. What is <code>cause</code> for?</summary>

To keep the original error when wrapping, so the whole chain is visible.

→ [Advanced theory](advanced.en.md#the-chain-of-causes-cause)

</details>

<details><summary>5. Trick: why does <code>JSON.stringify(new Error('x'))</code> return <code>{}</code>?</summary>

`message` and `stack` are non-enumerable properties.

→ [Gotcha 3](#3-jsonstringifyerror-is-)

</details>

<details><summary>6. How do you create your own error type and catch it correctly?</summary>

`class X extends Error`, set `name`; when catching — `instanceof` and rethrowing the unknown.

→ [Advanced theory](advanced.en.md#custom-error-classes)

</details>

<details><summary>7. Why does <code>try/catch</code> not catch an error in <code>setTimeout</code>?</summary>

The callback runs later on a different stack; the error must be caught inside it.

→ [Advanced theory](advanced.en.md#asynchronous-errors-and-top-level-handlers)

</details>

<details><summary>8. What is <code>AggregateError</code>?</summary>

An error with a list `errors`; `Promise.any` throws it if all are rejected.

→ [Advanced theory](advanced.en.md#aggregateerror)

</details>

<details><summary>9. Trick: when is the stack captured — at <code>new Error</code> or at <code>throw</code>?</summary>

The frames — at creation; the stack shows the place of creation, not of the throw.

→ [Gotcha 5](#5-the-stack-points-to-the-place-of-creation)

</details>

<details><summary>10. What should you do in an <code>uncaughtException</code> handler?</summary>

Log and shut the process down properly; continuing is unsafe.

→ [Advanced theory](advanced.en.md#asynchronous-errors-and-top-level-handlers)

</details>

<details><summary>11. How is <code>Error.isError</code> better than <code>instanceof Error</code>?</summary>

It works for errors from other contexts (`vm`, `iframe`).

→ [Gotcha 4](#4-instanceof-error-does-not-work-for-a-foreign-context)

</details>

<details><summary>12. How do you hide service frames in your error's stack?</summary>

`Error.captureStackTrace(this, ConstructorOrFn)`: the frames above the given function are dropped.

→ [Reference](#the-constructor-and-properties)

</details>

## Cheat sheet

```text
throw new Error('msg', { cause })     // always an Error
class E extends Error { constructor(m, o) { super(m, o); this.name = 'E' } }
try { … } catch (e) { if (!(e instanceof E)) throw e } finally { cleanup }
e.name, e.message, e.cause, e.stack;  Error.isError(x);  Error.captureStackTrace(o)
process.on('uncaughtException' | 'unhandledRejection', …) — log and exit
JSON.stringify(err) → {}  (needs toJSON)
```

- `return`/`throw` in `finally` override the outcome.
- The stack points to the place of creation.
- Catch only what you can handle.

## Related materials

- Previous level: [advanced theory](advanced.en.md)
- Promises: [12 · Promises and async/await](../12-promises-and-async-await/expert.en.md)
- The event loop: [13 · The event loop and concurrency](../13-event-loop-and-concurrency/expert.en.md)
- Exceptions in C#: [08 · Exceptions](../../../csharp/modules/08-exceptions/expert.en.md)

## Sources

- [ECMAScript: Error Objects](https://tc39.es/ecma262/#sec-error-objects)
- [MDN: Error](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Error)
- [V8: Stack trace API](https://v8.dev/docs/stack-trace-api)
- [Node.js: Errors](https://nodejs.org/docs/latest-v24.x/api/errors.html)
- [MDN: DOMException](https://developer.mozilla.org/en-US/docs/Web/API/DOMException)
