---
title: "Errors and debugging: advanced"
lang: en
track: javascript
kind: module
doc: advanced
slug: 14-errors-and-debugging
part: 2
tags: [custom-error, error-cause, aggregate-error, uncaught-exception, unhandled-rejection, error-code, console, debugger, using]
versions: "ECMAScript 2025+, Node.js 24.15 LTS"
status: draft
updated: 2026-11-23
---

# Errors and debugging: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

Builds on [basic theory](basics.en.md). All examples were run on Node.js 24.15.

## Custom error classes

```js
class AppError extends Error {
  constructor(message, options) { super(message, options); this.name = this.constructor.name; }
}
class NotFound extends AppError { constructor(what) { super(`${what} not found`); this.what = what; this.status = 404; } }
const nf = new NotFound('user');
console.log(nf.name, nf.message, nf instanceof NotFound, nf instanceof AppError, nf instanceof Error, nf.stack.split('\n')[0], String(nf));
class Plain extends Error { } console.log(new Plain('x').name, String(new Plain('x')), new Plain('x').constructor.name);
```

```text
NotFound user not found true true true NotFound: user not found NotFound: user not found
Error Error: x Plain
```

- A subclass of `Error` with `extends` keeps `instanceof` and the stack.
- `name` stays `Error` by default: without `this.name = …` (`Plain`) the error text is `Error: x`, although `constructor.name` is `Plain`. So `name` is set explicitly (here via `this.constructor.name`).
- Extra fields (`status`, `what`, `code`) make an error suitable for handling by data, not by text.
- A hierarchy (`AppError` → `NotFound`) lets you catch both the general and the specific type.

## The chain of causes: cause

```js
function readConfig() { try { JSON.parse('{') } catch (c) { throw new Error('config failed', { cause: c }) } }
try { readConfig() } catch (err) { let n = 0; for (let x = err; x; x = x.cause) console.log(' '.repeat(n++) + x.name + ': ' + x.message) }
```

```text
Error: config failed
 SyntaxError: Expected property name or '}' in JSON at position 1 (line 1 column 2)
```

When wrapping an error, add context and keep the original in `cause`. Then the log shows the whole chain, not only the top message.

## AggregateError

`AggregateError` keeps a list of errors in `errors`; `Promise.any` throws it if all are rejected ([module 12](../12-promises-and-async-await/advanced.en.md#combinators)):

```js
const ag = new AggregateError([new Error('a'), new TypeError('b')], 'many');
console.log(ag.name, ag.message, ag.errors.length, ag instanceof Error);
```

```text
AggregateError many 2 true
```

## Catching by type

JavaScript has no several `catch` blocks by type: you check `instanceof` inside one:

```js
function classify(e) { return e instanceof RangeError ? 'range' : e instanceof TypeError ? 'type' : 'other' }
console.log(classify(new RangeError()), classify(new TypeError()), classify(1));
```

```text
range type other
```

Errors you cannot handle should be **rethrown** (`throw e`), otherwise you hide bugs. For errors from another context (`vm`, `iframe`) `instanceof` does not work; `Error.isError(x)` fits:

```js
import vm from 'node:vm';
const foreign = vm.runInNewContext('new Error("foreign")');
console.log(foreign instanceof Error, Error.isError(foreign), Object.prototype.toString.call(foreign));
console.log(Error.isError(new TypeError('x')), Error.isError({ name: 'Error', message: 'm' }));
```

```text
false true [object Error]
true false
```

`Error.isError` is available in Node.js 24.15 (in the experiment); in older environments use `Object.prototype.toString.call(x) === '[object Error]'`.

## Asynchronous errors and top-level handlers

An error in a timer callback lands in no `try/catch`: it turns into an uncaught process exception, just like an unhandled promise rejection ([module 12](../12-promises-and-async-await/advanced.en.md#unhandled-rejections)):

```js
process.on('uncaughtException', (err, origin) => { console.log('uncaughtException:', err.message, origin); });
process.on('unhandledRejection', (r) => { console.log('unhandledRejection:', r.message); });
setTimeout(() => { throw new Error('from timer') }, 1);
Promise.reject(new Error('from promise'));
setTimeout(() => { process.exitCode = 3; console.log('exitCode set', process.exitCode) }, 20);
process.on('exit', code => console.log('exit event', code));
```

```text
unhandledRejection: from promise
uncaughtException: from timer uncaughtException
exitCode set 3
exit event 3
```

- `uncaughtException` and `unhandledRejection` are the **last line of defense**: log and shut the process down properly. Continuing after `uncaughtException` is unsafe: the program state is unknown.
- `process.exitCode` sets the exit code without exiting at once; the `exit` event receives this code.
- In an `EventEmitter` an `error` event without a listener **throws** (in the experiment `emit error throws: no listener`); if the value is not an error — `ERR_UNHANDLED_ERROR`.

## Node.js error codes

Node.js errors have a stable `code` field; it is more reliable than the text:

```js
import fs from 'node:fs';
try { await import('./missing.js') } catch (e) { console.log(e.code) }
try { fs.readFileSync('/nope/x') } catch (e) { console.log(e.code, e.syscall, e.name) }
```

```text
ERR_MODULE_NOT_FOUND
ENOENT open Error
```

System errors (`ENOENT`, `EACCES`, …) carry `code`, `syscall` and a numeric `errno` (the `errno` value depends on the OS: `-4058` in the experiment on Windows).

## Serializing errors

`JSON.stringify(new Error('x'))` gives `{}`: the `message` and `stack` fields are not enumerable. You need `toJSON` or explicit copying:

```js
const e = new Error('boom');
console.log(JSON.stringify(e), JSON.stringify({ ...e, message: e.message }));
class HttpError extends Error { constructor(s, m) { super(m); this.name = 'HttpError'; this.status = s } toJSON() { return { name: this.name, message: this.message, status: this.status } } }
console.log(JSON.stringify(new HttpError(404, 'nf')), JSON.stringify({ e: new HttpError(500, 'x') }));
```

```text
{} {"message":"boom"}
{"name":"HttpError","message":"nf","status":404} {"e":{"name":"HttpError","message":"x","status":500}}
```

Do not send `stack` to a client: it reveals the internals.

## Releasing resources: finally and using

The classic way is `try/finally`. Node.js 24 also has explicit resource management (`using`): an object with a `[Symbol.dispose]()` method is released on leaving the block:

```js
{ using r = { [Symbol.dispose]() { console.log('disposed r') } }; console.log('in block'); }
```

```text
in block
disposed r
```

> ⚠️ Verify: support for `using` in other Node.js versions and browsers, `await using` and `DisposableStack` were not checked.

## Debugging

**console.** Besides `log` there are useful methods (output from the experiment):

```js
console.assert(1 === 2, 'assert message');
console.table([{ a: 1, b: 'x' }, { a: 2, b: 'y' }]);
console.group('G'); console.log('inside'); console.groupEnd();
console.dir({ a: { b: { c: { d: 1 } } } }, { depth: 0 });
console.count(); console.count('x'); console.count();
console.log('%s is %d years and %o', 'Bob', 42, [1]);
```

```text
Assertion failed: assert message
┌─────────┬───┬─────┐
│ (index) │ a │ b   │
├─────────┼───┼─────┤
│ 0       │ 1 │ 'x' │
│ 1       │ 2 │ 'y' │
└─────────┴───┴─────┘
G
  inside
{ a: [Object] }
default: 1
x: 1
default: 2
Bob is 42 years and [ 1, [length]: 1 ]
```

- `console.assert` writes to **stderr** only if the condition is false; `console.error` and `console.warn` also go to stderr, `console.log` to stdout.
- `console.table`, `console.group`, `console.count`, `console.dir` with `depth` help inspect structures.
- Format specifiers: `%s`, `%d`, `%i`, `%f`, `%j`, `%o`, `%O`.

**The `debugger` statement.** Without an attached debugger it does nothing (in the experiment the code after it ran); with a debugger — a stop.

**The Node.js debugger.** Running `node --inspect-brk app.js` opens an inspector port: Chrome DevTools (`chrome://inspect`) or an IDE connect (breakpoints, steps, inspecting variables). The console debugger is `node inspect app.js`.

> ⚠️ Verify: attaching a debugger (`--inspect`, `--inspect-brk`, `node inspect`), `--enable-source-maps` and `--stack-trace-limit` were not run in this material; described from the Node.js documentation.

**The stack.** `Error.stackTraceLimit` (`10` by default, in the experiment) limits the number of frames: at `1` the `stack` has two lines (the message and one frame). V8 restores async frames (`at async a2`). `Error.captureStackTrace(obj, fn)` writes a stack into an arbitrary object and can hide service frames above `fn`.

## Best practices

- Throw `Error` and its subclasses; set `name`.
- When wrapping an error, pass `{ cause }`.
- Catch only what you can handle; rethrow the rest.
- Compare the type and `code`, not the message text.
- Do not use `return` in `finally`.
- In top-level handlers log and terminate the process.
- Do not send `stack` to clients.

## Common mistakes

- ❌ Forgetting `this.name` in your class → ✅ `this.name = this.constructor.name`.
- ❌ `JSON.stringify(error)` → ✅ `toJSON` or an explicit set of fields.
- ❌ Continuing after `uncaughtException` → ✅ log and exit.
- ❌ `emit('error')` without a listener → ✅ an `error` listener.
- ❌ Catching everything and staying silent → ✅ rethrow the unknown.
- ❌ Relying on `instanceof` for errors from another context → ✅ `Error.isError`.

## Related materials

- Next: [expert theory](expert.en.md)
- Asynchrony: [12 · Promises and async/await](../12-promises-and-async-await/advanced.en.md)
- Exceptions in C#: [08 · Exceptions](../../../csharp/modules/08-exceptions/overview.en.md)

## Sources

- [MDN: Error.cause](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Error/cause)
- [MDN: AggregateError](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/AggregateError)
- [MDN: Error.isError](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Error/isError)
- [Node.js: Process events (`uncaughtException`)](https://nodejs.org/docs/latest-v24.x/api/process.html#event-uncaughtexception)
- [Node.js: Debugging guide](https://nodejs.org/en/learn/getting-started/debugging)
- [Node.js: console](https://nodejs.org/docs/latest-v24.x/api/console.html)
- [MDN: using](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Statements/using)
