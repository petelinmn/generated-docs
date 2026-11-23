---
title: "Exercises: errors and debugging"
lang: en
track: javascript
kind: module
doc: exercises
slug: 14-errors-and-debugging
part: 2
tags: [error, custom-error, error-cause, aggregate-error, finally]
status: draft
updated: 2026-11-23
---

# Exercises: errors and debugging

🌐 [Русский](exercises.ru.md) · **English**

> Solve on your own; hints and solutions are under spoilers. Run the code in a file next to a `package.json` (`{"type": "module"}`).

## 1. 🟢 An error hierarchy

**Task:** create `AppError` (extends `Error`, sets `name` from the class name) and `ValidationError` with a `field` property and the message `invalid <field>`. Print `name`, `message`, `field`, the `instanceof` results and `String(error)`.

**Expected result:** `ValidationError invalid email email true true ValidationError: invalid email`.

<details><summary>Hint</summary>

`this.name = this.constructor.name` in the `AppError` constructor.

</details>

<details><summary>Solution</summary>

```js
class AppError extends Error { constructor(m, o) { super(m, o); this.name = this.constructor.name } }
class ValidationError extends AppError { constructor(field) { super(`invalid ${field}`); this.field = field } }
const v = new ValidationError('email');
console.log(v.name, v.message, v.field, v instanceof AppError, v instanceof Error, String(v));
```

```text
ValidationError invalid email email true true ValidationError: invalid email
```

</details>

## 2. 🟢 A chain of causes

**Task:** the function `parseConfig(text)` calls `JSON.parse` and on error throws `AppError('cannot parse config')` keeping the original error. Print the chain of error names.

**Expected result:** `AppError <- SyntaxError`.

<details><summary>Hint</summary>

`new AppError('…', { cause: e })`; walk: `for (let x = e; x; x = x.cause)`.

</details>

<details><summary>Solution</summary>

```js
function parseConfig(text) { try { return JSON.parse(text) } catch (e) { throw new AppError('cannot parse config', { cause: e }) } }
try { parseConfig('{') } catch (e) { const chain = []; for (let x = e; x; x = x.cause) chain.push(x.name); console.log(chain.join(' <- ')) }
```

```text
AppError <- SyntaxError
```

`AppError` is from exercise 1.

</details>

## 3. 🟡 Serializing an error

**Task:** create `HttpError(status, message)` with a `toJSON` method that returns `name`, `message` and `status`. Compare with `JSON.stringify(new Error('x'))`.

**Expected result:** `{"name":"HttpError","message":"not found","status":404} {}`.

<details><summary>Hint</summary>

The `toJSON()` method is called by `JSON.stringify` automatically.

</details>

<details><summary>Solution</summary>

```js
class HttpError extends AppError { constructor(status, m) { super(m); this.status = status } toJSON() { return { name: this.name, message: this.message, status: this.status } } }
console.log(JSON.stringify(new HttpError(404, 'not found')), JSON.stringify(new Error('x')));
```

```text
{"name":"HttpError","message":"not found","status":404} {}
```

</details>

## 4. 🟡 Several errors at once

**Task:** the function `validate(user)` checks `name` and `age`, collects all found `ValidationError`s and throws `AggregateError('validation failed')`. For valid data it returns `true`.

**Expected result:** `AggregateError validation failed [ 'name', 'age' ]` and `true`.

<details><summary>Hint</summary>

Accumulate errors in an array and throw `new AggregateError(errs, 'validation failed')` once at the end.

</details>

<details><summary>Solution</summary>

```js
function validate(user) { const errs = []; if (!user.name) errs.push(new ValidationError('name')); if (!user.age || user.age < 0) errs.push(new ValidationError('age')); if (errs.length) throw new AggregateError(errs, 'validation failed'); return true }
try { validate({}) } catch (e) { console.log(e.name, e.message, e.errors.map(x => x.field)) }
console.log(validate({ name: 'A', age: 1 }));
```

```text
AggregateError validation failed [ 'name', 'age' ]
true
```

</details>

## 5. 🔴 The order of `try/catch/finally`

**Task:** the function `run(fail)` writes `try` to a log, on error `catch <message>`, always `finally`. Without an error it returns `'ok'`, with an error `'recovered'`. Call it twice and print the results and the log.

**Expected result:** `ok recovered try, finally, try, catch boom, finally`.

<details><summary>Hint</summary>

`finally` runs both after a `return` in `try` and after a `return` in `catch`.

</details>

<details><summary>Solution</summary>

```js
const log = [];
function run(fail) { try { log.push('try'); if (fail) throw new Error('boom'); return 'ok' } catch (e) { log.push('catch ' + e.message); return 'recovered' } finally { log.push('finally') } }
console.log(run(false), run(true), log.join(', '));
```

```text
ok recovered try, finally, try, catch boom, finally
```

</details>
