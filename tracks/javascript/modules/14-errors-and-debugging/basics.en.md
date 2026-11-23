---
title: "Errors and debugging: basics"
lang: en
track: javascript
kind: module
doc: basics
slug: 14-errors-and-debugging
part: 2
tags: [error, try-catch, finally, throw, stack-trace, error-types]
versions: "ECMAScript 2025+, Node.js 24.15 LTS"
status: draft
updated: 2026-11-23
---

# Errors and debugging: basics

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [04 · Functions](../04-functions/overview.en.md), [12 · Promises and async/await](../12-promises-and-async-await/overview.en.md) · **Time:** ~2 h

All examples were run on Node.js 24.15; the files sit next to a `package.json` (`{"type": "module"}`).

## Why it matters

Every program meets errors: invalid input, an unavailable file, a bug. You must be able to throw, catch and read them, and not lose context.

## The Error object

```js
const e = new Error('boom', { cause: new TypeError('root') });
console.log(e.name, e.message, e.cause.name, e.cause.message, Object.keys(e), Object.getOwnPropertyNames(e));
console.log(String(e), Object.prototype.toString.call(e), e instanceof Error, typeof e.stack, e.stack.split('\n')[0]);
```

```text
Error boom TypeError root [] [ 'stack', 'message', 'cause' ]
Error: boom [object Error] true string Error: boom
```

- `name` is the type (`'Error'`), `message` the text, `stack` a string with the call stack, `cause` the cause (the second argument `{ cause }`).
- The own properties `message`, `stack`, `cause` are **not enumerable**: `Object.keys(e)` is empty.
- `String(e)` is `Error: boom` (`name: message`), the first line of `stack` is the same.

## Built-in error types

| Type | When | Example message (in the experiment) |
|---|---|---|
| `TypeError` | a wrong value type | `Cannot read properties of null (reading 'x')` |
| `ReferenceError` | an undeclared variable | `undefinedVar is not defined` |
| `RangeError` | a value outside the allowed range | `Invalid array length`, `Maximum call stack size exceeded` |
| `SyntaxError` | invalid syntax, `JSON.parse` | `Expected property name or '}' in JSON at position 1 (line 1 column 2)` |
| `URIError` | `decodeURIComponent` etc. | `URI malformed` |
| `EvalError` | legacy, not thrown in modern code | — |
| `AggregateError` | several errors at once | see the [advanced level](advanced.en.md#aggregateerror) |

All of them inherit from `Error`. Other examples from the experiment: `1n + 1` is `TypeError: Cannot mix BigInt and other types, use explicit conversions`, `'x'.repeat(-1)` is `RangeError: Invalid count value: -1`. Message wording depends on the engine, so in code compare the type and your own codes, not the text.

## throw and try/catch/finally

```js
function f1() { try { return 'try' } finally { console.log('finally runs') } }
function f2() { try { throw new Error('lost') } finally { return 'finally wins' } }
console.log(f1(), f2());
try { try { throw new RangeError('r') } catch (x) { if (!(x instanceof TypeError)) throw x } } catch { console.log('rethrown and caught') }
for (const v of [1, 'str', null, { a: 1 }]) { try { throw v } catch (x) { console.log(typeof x, x instanceof Error) } }
```

```text
finally runs
try finally wins
rethrown and caught
number false
string false
object false
object false
```

- `finally` runs **always**: after `return` and after `throw`.
- A `return` inside `finally` overrides the result of `try` and **swallows** a thrown error (`f2` returned `'finally wins'`, the `lost` error is lost).
- `catch { … }` without a parameter is allowed when the error itself is not needed.
- A foreign error must be **rethrown**: `if (!(x instanceof TypeError)) throw x`.
- You can throw any value (`throw 1`, `throw 'str'`), but it will have no stack — throw an `Error`.

## Reading a call stack

```js
function a() { b() } function b() { c() } function c() { console.log(new Error('here').stack) } a();
```

```text
Error: here
    at c (…/a.js:38:70)
    at b (…/a.js:38:37)
    at a (…/a.js:38:16)
    …
```

- The first line is `name: message`; below are **frames** from the place where the error was created to the entry point: `at function (file:line:column)`.
- Read the stack from top to bottom: the top frame is where the error was created, the lower ones are who called.
- Line numbers and paths in your run will differ.

## Asynchronous errors

An error in a promise or an `async` function is caught with `catch` or a `try/catch` around `await` ([module 12](../12-promises-and-async-await/basics.en.md#async-and-await)). But `try/catch` will not catch an error in a callback that runs later (in a timer, an event handler): it runs on a different stack ([module 13](../13-event-loop-and-concurrency/expert.en.md#gotchas)).

## Beginner mistakes

- ❌ Throwing strings (`throw 'oops'`) → ✅ `throw new Error('oops')`.
- ❌ An empty `catch {}` → ✅ handle, log or rethrow.
- ❌ Comparing the message text → ✅ the type or your own code.
- ❌ `return` in `finally` → ✅ cleanup only.
- ❌ Expecting `try/catch` to catch an error from `setTimeout` → ✅ `try/catch` inside the callback.
- ❌ Losing the original error when wrapping → ✅ `{ cause }`.

## Related materials

- Next: [advanced theory](advanced.en.md) — custom classes, `cause`, top-level handlers, debugging
- Earlier: [12 · Promises and async/await](../12-promises-and-async-await/advanced.en.md#unhandled-rejections)

## Sources

- [MDN: Error](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Error)
- [MDN: try...catch](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Statements/try...catch)
- [MDN: throw](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Statements/throw)
- [Node.js: Errors](https://nodejs.org/docs/latest-v24.x/api/errors.html)
