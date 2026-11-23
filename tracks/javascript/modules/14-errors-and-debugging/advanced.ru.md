---
title: "Ошибки и отладка: продвинутый уровень"
lang: ru
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

# Ошибки и отладка: продвинутый уровень

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [overview](overview.ru.md) · [1. Основы](basics.ru.md) · **2. Продвинутый** · [3. Эксперт](expert.ru.md) · [4. Квиз](quiz.ru.md)

Опирается на [базовую теорию](basics.ru.md). Все примеры запускались на Node.js 24.15.

## Свои классы ошибок

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

- Подкласс `Error` с `extends` сохраняет `instanceof` и стек.
- `name` по умолчанию остаётся `Error`: без `this.name = …` (`Plain`) в тексте ошибки будет `Error: x`, хотя `constructor.name` — `Plain`. Поэтому `name` задают явно (здесь — через `this.constructor.name`).
- Дополнительные поля (`status`, `what`, `code`) делают ошибку пригодной для обработки по данным, а не по тексту.
- Иерархия (`AppError` → `NotFound`) позволяет ловить и общий, и конкретный тип.

## Цепочка причин: cause

```js
function readConfig() { try { JSON.parse('{') } catch (c) { throw new Error('config failed', { cause: c }) } }
try { readConfig() } catch (err) { let n = 0; for (let x = err; x; x = x.cause) console.log(' '.repeat(n++) + x.name + ': ' + x.message) }
```

```text
Error: config failed
 SyntaxError: Expected property name or '}' in JSON at position 1 (line 1 column 2)
```

Оборачивая ошибку, добавляйте контекст и сохраняйте исходную в `cause`. Тогда в журнале видна вся цепочка, а не только верхнее сообщение.

## AggregateError

`AggregateError` хранит список ошибок в `errors`; его бросает `Promise.any`, если отклонены все ([модуль 12](../12-promises-and-async-await/advanced.ru.md#комбинаторы)):

```js
const ag = new AggregateError([new Error('a'), new TypeError('b')], 'many');
console.log(ag.name, ag.message, ag.errors.length, ag instanceof Error);
```

```text
AggregateError many 2 true
```

## Ловля по типу

В JavaScript нет нескольких `catch` по типам: проверяют `instanceof` внутри одного:

```js
function classify(e) { return e instanceof RangeError ? 'range' : e instanceof TypeError ? 'type' : 'other' }
console.log(classify(new RangeError()), classify(new TypeError()), classify(1));
```

```text
range type other
```

Ошибки, которые вы не умеете обрабатывать, **пробрасывайте** (`throw e`), иначе скроете баги. Для ошибок из другого контекста (`vm`, `iframe`) `instanceof` не работает; для них подходит `Error.isError(x)`:

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

`Error.isError` доступен в Node.js 24.15 (в опыте); в более старых средах используйте `Object.prototype.toString.call(x) === '[object Error]'`.

## Асинхронные ошибки и обработчики верхнего уровня

Ошибка в колбэке таймера не попадает ни в какой `try/catch`: она превращается в необработанное исключение процесса, как и необработанное отклонение промиса ([модуль 12](../12-promises-and-async-await/advanced.ru.md#необработанные-отклонения)):

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

- `uncaughtException` и `unhandledRejection` — **последний рубеж**: залогировать и корректно завершить процесс. Продолжать работу после `uncaughtException` небезопасно: состояние программы неизвестно.
- `process.exitCode` задаёт код завершения без немедленного выхода; событие `exit` получает этот код.
- В `EventEmitter` событие `error` без слушателя **бросает исключение** (в опыте `emit error throws: no listener`); если передана не ошибка — `ERR_UNHANDLED_ERROR`.

## Коды ошибок Node.js

У ошибок Node.js есть стабильное поле `code`; по нему надёжнее, чем по тексту:

```js
import fs from 'node:fs';
try { await import('./missing.js') } catch (e) { console.log(e.code) }
try { fs.readFileSync('/nope/x') } catch (e) { console.log(e.code, e.syscall, e.name) }
```

```text
ERR_MODULE_NOT_FOUND
ENOENT open Error
```

Системные ошибки (`ENOENT`, `EACCES`, …) несут `code`, `syscall` и числовой `errno` (значение `errno` зависит от ОС: в опыте на Windows `-4058`).

## Сериализация ошибок

`JSON.stringify(new Error('x'))` даёт `{}`: поля `message` и `stack` не перечисляются. Нужны `toJSON` или явное копирование:

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

Не отправляйте клиенту `stack`: он раскрывает внутреннее устройство.

## Освобождение ресурсов: finally и using

Классический способ — `try/finally`. В Node.js 24 доступно и явное управление ресурсами (`using`): объект с методом `[Symbol.dispose]()` освобождается при выходе из блока:

```js
{ using r = { [Symbol.dispose]() { console.log('disposed r') } }; console.log('in block'); }
```

```text
in block
disposed r
```

> ⚠️ Проверить: поддержка `using` в других версиях Node.js и браузерах, `await using` и `DisposableStack` не проверялись.

## Отладка

**console.** Кроме `log` есть полезные методы (вывод из опыта):

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

- `console.assert` пишет в **stderr**, только если условие ложно; `console.error` и `console.warn` — тоже в stderr, `console.log` — в stdout.
- `console.table`, `console.group`, `console.count`, `console.dir` с `depth` помогают смотреть структуры.
- Спецификаторы форматирования: `%s`, `%d`, `%i`, `%f`, `%j`, `%o`, `%O`.

**Оператор `debugger`.** Без подключённого отладчика ничего не делает (в опыте код после него выполнился); с отладчиком — остановка.

**Отладчик Node.js.** Запуск `node --inspect-brk app.js` открывает порт инспектора: подключаются Chrome DevTools (`chrome://inspect`) или IDE (точки останова, шаги, просмотр переменных). Консольный отладчик — `node inspect app.js`.

> ⚠️ Проверить: подключение отладчика (`--inspect`, `--inspect-brk`, `node inspect`), `--enable-source-maps` и `--stack-trace-limit` в этом материале не запускались; описано по документации Node.js.

**Стек.** `Error.stackTraceLimit` (по умолчанию `10`, в опыте) ограничивает число кадров: при `1` в `stack` две строки (сообщение и один кадр). Асинхронные кадры V8 восстанавливает (`at async a2`). `Error.captureStackTrace(obj, fn)` записывает стек в произвольный объект и может скрыть служебные кадры выше `fn`.

## Лучшие практики

- Бросайте `Error` и его подклассы; задавайте `name`.
- Оборачивая ошибку, передавайте `{ cause }`.
- Ловите только то, что умеете обработать; остальное пробрасывайте.
- Сравнивайте тип и `code`, а не текст сообщения.
- Не используйте `return` в `finally`.
- В обработчиках верхнего уровня логируйте и завершайте процесс.
- Не отправляйте `stack` клиентам.

## Частые ошибки

- ❌ Забыть `this.name` в своём классе → ✅ `this.name = this.constructor.name`.
- ❌ `JSON.stringify(error)` → ✅ `toJSON` или явный набор полей.
- ❌ Продолжать работу после `uncaughtException` → ✅ залогировать и выйти.
- ❌ `emit('error')` без слушателя → ✅ слушатель `error`.
- ❌ Ловить всё и молчать → ✅ пробрасывать неизвестные.
- ❌ Полагаться на `instanceof` для ошибок из другого контекста → ✅ `Error.isError`.

## Связанные материалы

- Дальше: [экспертная теория](expert.ru.md)
- Асинхронность: [12 · Промисы и async/await](../12-promises-and-async-await/advanced.ru.md)
- Исключения в C#: [08 · Исключения](../../../csharp/modules/08-exceptions/overview.ru.md)

## Источники

- [MDN: Error.cause](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Error/cause)
- [MDN: AggregateError](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/AggregateError)
- [MDN: Error.isError](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Error/isError)
- [Node.js: Process events (`uncaughtException`)](https://nodejs.org/docs/latest-v24.x/api/process.html#event-uncaughtexception)
- [Node.js: Debugging guide](https://nodejs.org/en/learn/getting-started/debugging)
- [Node.js: console](https://nodejs.org/docs/latest-v24.x/api/console.html)
- [MDN: using](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Statements/using)
