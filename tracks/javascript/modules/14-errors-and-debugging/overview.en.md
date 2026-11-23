---
title: "Errors and debugging"
lang: en
track: javascript
kind: module
doc: overview
slug: 14-errors-and-debugging
part: 2
levels: [1, 2, 3, 4]
tags: [error, try-catch, error-cause, aggregate-error, custom-error, stack-trace, debugging, unhandled-rejection, explicit-resource-management]
versions: "ECMAScript 2025+, Node.js 24.15 LTS"
status: draft
updated: 2026-11-23
---

# Errors and debugging

🌐 [Русский](overview.ru.md) · **English**

## In short

Errors in JavaScript are `Error` objects (and subclasses) that are thrown with `throw` and caught with `try/catch`. An object has `message`, `name`, `stack` and an optional `cause`. Good error handling means your own class hierarchy, chains of causes, careful rethrowing and top-level handlers. Debugging means a readable call stack, `console`, the `debugger` statement and the Node.js debugger.

## Key ideas

- **You can throw anything, but you should throw an `Error`:** only it has a `stack`.
- **`try/catch/finally`:** `finally` always runs; a `return` in it overrides the rest.
- **`cause`** keeps the cause: errors form a chain.
- **`AggregateError`** gathers several errors (`Promise.any`).
- **A custom error class** extends `Error` and sets `name`; `instanceof` lets you catch by type.
- **`try/catch` does not catch errors of asynchronous callbacks;** for promises and `await` use `catch` and `try/catch` around `await`.
- **`stack` is built when the object is created,** not at `throw`.
- **Top-level handlers** (`uncaughtException`, `unhandledRejection`) are a safety net, not a way of handling.
- **`Error` is not serialized by `JSON.stringify`:** you need `toJSON` or explicit copying of fields.
- **Debugging:** `console.*`, `debugger`, `node --inspect`, async stack frames.

## Minimal example

```js
try {
  try { JSON.parse('{') } catch (c) { throw new Error('config failed', { cause: c }) }
} catch (err) {
  console.log(err.message, '<-', err.cause.name);
}
```

```text
config failed <- SyntaxError
```

The original error is not lost: it is in `cause`.

## When to use and when not

| Task | Choice |
|---|---|
| A programmer error or an unexpected situation | `throw new Error(...)` |
| An expected "refusal" (not found, invalid input) | your own subclass or a return value, not an exception |
| Adding context to a caught error | `new Error('…', { cause })` |
| Several independent errors | `AggregateError` |
| Releasing a resource | `finally` or `using` |
| A process crash | a top-level handler: log and exit |

- ❌ Do not swallow errors with an empty `catch`.
- ❌ Do not throw strings and "bare" objects.

## Material levels

| Level | What is inside |
|---|---|
| [1. Basic theory](basics.en.md) | `Error`, error types, `try/catch/finally`, `throw`, reading a stack |
| [2. Advanced theory](advanced.en.md) | custom classes, `cause`, `AggregateError`, catching by type, async errors, top-level handlers, Node.js error codes, debugging |
| [3. Expert theory](expert.en.md) | a reference, internals, pitfalls, interview questions |
| [4. Self-check](quiz.en.md) | quiz and [exercises](exercises.en.md) |
| 5. Runnable examples | not created yet — `/learn javascript 14-errors-and-debugging level 5` |
| 6. Tests | not created yet — `/learn javascript 14-errors-and-debugging level 6` |

## Related materials

- Prerequisites: [12 · Promises and async/await](../12-promises-and-async-await/overview.en.md), [13 · The event loop and concurrency](../13-event-loop-and-concurrency/overview.en.md)
- Next: module 15 · Testing (in the [roadmap](../../overview.en.md))
- Analogues in other tracks: [C#: exceptions](../../../csharp/modules/08-exceptions/overview.en.md), [Python](../../../python/overview.en.md) (exceptions)
