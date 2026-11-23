---
title: "Упражнения: ошибки и отладка"
lang: ru
track: javascript
kind: module
doc: exercises
slug: 14-errors-and-debugging
part: 2
tags: [error, custom-error, error-cause, aggregate-error, finally]
status: draft
updated: 2026-11-23
---

# Упражнения: ошибки и отладка

🌐 **Русский** · [English](exercises.en.md)

> Решайте самостоятельно, подсказки и решения под спойлерами. Код запускается в файле с `package.json` (`{"type": "module"}`).

## 1. 🟢 Иерархия ошибок

**Задача:** создайте `AppError` (наследует `Error`, задаёт `name` по имени класса) и `ValidationError` с полем `field` и сообщением `invalid <field>`. Выведите `name`, `message`, `field`, результаты `instanceof` и `String(error)`.

**Ожидаемый результат:** `ValidationError invalid email email true true ValidationError: invalid email`.

<details><summary>Подсказка</summary>

`this.name = this.constructor.name` в конструкторе `AppError`.

</details>

<details><summary>Решение</summary>

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

## 2. 🟢 Цепочка причин

**Задача:** функция `parseConfig(text)` вызывает `JSON.parse` и при ошибке бросает `AppError('cannot parse config')` с сохранением исходной ошибки. Выведите цепочку имён ошибок.

**Ожидаемый результат:** `AppError <- SyntaxError`.

<details><summary>Подсказка</summary>

`new AppError('…', { cause: e })`; обход: `for (let x = e; x; x = x.cause)`.

</details>

<details><summary>Решение</summary>

```js
function parseConfig(text) { try { return JSON.parse(text) } catch (e) { throw new AppError('cannot parse config', { cause: e }) } }
try { parseConfig('{') } catch (e) { const chain = []; for (let x = e; x; x = x.cause) chain.push(x.name); console.log(chain.join(' <- ')) }
```

```text
AppError <- SyntaxError
```

`AppError` — из упражнения 1.

</details>

## 3. 🟡 Сериализация ошибки

**Задача:** создайте `HttpError(status, message)` с методом `toJSON`, который возвращает `name`, `message` и `status`. Сравните с `JSON.stringify(new Error('x'))`.

**Ожидаемый результат:** `{"name":"HttpError","message":"not found","status":404} {}`.

<details><summary>Подсказка</summary>

Метод `toJSON()` вызывается `JSON.stringify` автоматически.

</details>

<details><summary>Решение</summary>

```js
class HttpError extends AppError { constructor(status, m) { super(m); this.status = status } toJSON() { return { name: this.name, message: this.message, status: this.status } } }
console.log(JSON.stringify(new HttpError(404, 'not found')), JSON.stringify(new Error('x')));
```

```text
{"name":"HttpError","message":"not found","status":404} {}
```

</details>

## 4. 🟡 Несколько ошибок сразу

**Задача:** функция `validate(user)` проверяет `name` и `age`, собирает все найденные `ValidationError` и бросает `AggregateError('validation failed')`. Для корректных данных возвращает `true`.

**Ожидаемый результат:** `AggregateError validation failed [ 'name', 'age' ]` и `true`.

<details><summary>Подсказка</summary>

Накопите ошибки в массив и бросайте `new AggregateError(errs, 'validation failed')` один раз в конце.

</details>

<details><summary>Решение</summary>

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

## 5. 🔴 Порядок `try/catch/finally`

**Задача:** функция `run(fail)` пишет в журнал `try`, при ошибке — `catch <сообщение>`, всегда — `finally`. Без ошибки возвращает `'ok'`, при ошибке — `'recovered'`. Вызовите её дважды и выведите результаты и журнал.

**Ожидаемый результат:** `ok recovered try, finally, try, catch boom, finally`.

<details><summary>Подсказка</summary>

`finally` выполняется и после `return` в `try`, и после `return` в `catch`.

</details>

<details><summary>Решение</summary>

```js
const log = [];
function run(fail) { try { log.push('try'); if (fail) throw new Error('boom'); return 'ok' } catch (e) { log.push('catch ' + e.message); return 'recovered' } finally { log.push('finally') } }
console.log(run(false), run(true), log.join(', '));
```

```text
ok recovered try, finally, try, catch boom, finally
```

</details>
