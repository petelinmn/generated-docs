---
title: "Ошибки и отладка: основы"
lang: ru
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

# Ошибки и отладка: основы

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [overview](overview.ru.md) · **1. Основы** · [2. Продвинутый](advanced.ru.md) · [3. Эксперт](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [04 · Функции](../04-functions/overview.ru.md), [12 · Промисы и async/await](../12-promises-and-async-await/overview.ru.md) · **Время:** ~2 ч

Все примеры запускались на Node.js 24.15; файлы лежат рядом с `package.json` (`{"type": "module"}`).

## Зачем это нужно

Любая программа сталкивается с ошибками: неверный ввод, недоступный файл, баг. Важно уметь их бросать, ловить, читать и не терять контекст.

## Объект Error

```js
const e = new Error('boom', { cause: new TypeError('root') });
console.log(e.name, e.message, e.cause.name, e.cause.message, Object.keys(e), Object.getOwnPropertyNames(e));
console.log(String(e), Object.prototype.toString.call(e), e instanceof Error, typeof e.stack, e.stack.split('\n')[0]);
```

```text
Error boom TypeError root [] [ 'stack', 'message', 'cause' ]
Error: boom [object Error] true string Error: boom
```

- `name` — тип (`'Error'`), `message` — текст, `stack` — строка со стеком вызовов, `cause` — причина (второй аргумент `{ cause }`).
- Свои свойства `message`, `stack`, `cause` **не перечисляются**: `Object.keys(e)` пуст.
- `String(e)` — `Error: boom` (`name: message`), первая строка `stack` — то же.

## Встроенные типы ошибок

| Тип | Когда | Пример сообщения (в опыте) |
|---|---|---|
| `TypeError` | неверный тип значения | `Cannot read properties of null (reading 'x')` |
| `ReferenceError` | необъявленная переменная | `undefinedVar is not defined` |
| `RangeError` | значение вне допустимого диапазона | `Invalid array length`, `Maximum call stack size exceeded` |
| `SyntaxError` | неверный синтаксис, `JSON.parse` | `Expected property name or '}' in JSON at position 1 (line 1 column 2)` |
| `URIError` | `decodeURIComponent` и др. | `URI malformed` |
| `EvalError` | устаревший, в современном коде не бросается | — |
| `AggregateError` | несколько ошибок сразу | см. [продвинутый уровень](advanced.ru.md#aggregateerror) |

Все они наследуют от `Error`. Другие примеры из опыта: `1n + 1` — `TypeError: Cannot mix BigInt and other types, use explicit conversions`, `'x'.repeat(-1)` — `RangeError: Invalid count value: -1`. Формулировки сообщений зависят от движка, поэтому в коде сравнивают тип и свои коды, а не текст.

## throw и try/catch/finally

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

- `finally` выполняется **всегда**: и после `return`, и после `throw`.
- `return` внутри `finally` перекрывает результат `try` и **проглатывает** брошенную ошибку (`f2` вернул `'finally wins'`, ошибка `lost` потеряна).
- `catch { … }` без параметра — допустимо, когда сама ошибка не нужна.
- Чужую ошибку нужно **пробросить** дальше: `if (!(x instanceof TypeError)) throw x`.
- Бросать можно любое значение (`throw 1`, `throw 'str'`), но у него не будет стека — бросайте `Error`.

## Чтение стека вызовов

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

- Первая строка — `name: message`; ниже — **кадры** (frames) от места создания ошибки к точке входа: `at функция (файл:строка:столбец)`.
- Читать стек нужно сверху вниз: верхний кадр — где ошибку создали, нижние — кто вызывал.
- Номера строк и пути в вашем запуске будут другими.

## Асинхронные ошибки

Ошибка в промисе или `async`-функции ловится `catch` или `try/catch` вокруг `await` ([модуль 12](../12-promises-and-async-await/basics.ru.md#async-и-await)). А вот `try/catch` не поймает ошибку в колбэке, который выполнится позже (в таймере, обработчике события): он выполняется в другом стеке ([модуль 13](../13-event-loop-and-concurrency/expert.ru.md#подвохи)).

## Ошибки новичков

- ❌ Бросать строки (`throw 'oops'`) → ✅ `throw new Error('oops')`.
- ❌ Пустой `catch {}` → ✅ обработать, залогировать или пробросить.
- ❌ Сравнивать текст сообщения → ✅ тип или собственный код.
- ❌ `return` в `finally` → ✅ только очистка.
- ❌ Ждать, что `try/catch` поймает ошибку из `setTimeout` → ✅ `try/catch` внутри колбэка.
- ❌ Терять исходную ошибку при оборачивании → ✅ `{ cause }`.

## Связанные материалы

- Дальше: [продвинутая теория](advanced.ru.md) — свои классы, `cause`, обработчики верхнего уровня, отладка
- Ранее: [12 · Промисы и async/await](../12-promises-and-async-await/advanced.ru.md#необработанные-отклонения)

## Источники

- [MDN: Error](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Error)
- [MDN: try...catch](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Statements/try...catch)
- [MDN: throw](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Statements/throw)
- [Node.js: Errors](https://nodejs.org/docs/latest-v24.x/api/errors.html)
