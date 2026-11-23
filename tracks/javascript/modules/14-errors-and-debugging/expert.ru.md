---
title: "Ошибки и отладка: экспертный уровень"
lang: ru
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

# Ошибки и отладка: экспертный уровень

🌐 **Русский** · [English](expert.en.md)

**Уровни:** [overview](overview.ru.md) · [1. Основы](basics.ru.md) · [2. Продвинутый](advanced.ru.md) · **3. Эксперт** · [4. Квиз](quiz.ru.md)

Опирается на [базовую](basics.ru.md) и [продвинутую](advanced.ru.md) теорию. Все результаты получены запуском на Node.js 24.15.

## Справочник

### Конструктор и свойства

| Член | Что это | Замечания (в опыте) |
|---|---|---|
| `new Error(message, { cause })` | конструктор; `Error(...)` без `new` тоже создаёт ошибку | `Error('no new') instanceof Error` — `true`; `Error.length` — `1` |
| `message` | собственное свойство, **не перечисляется** | при `new Error()` без аргумента собственного свойства нет; `new Error(42).message` — `'42'` (строка), `new Error({ a: 1 }).message` — `'[object Object]'` |
| `name` | свойство прототипа (`Error.prototype.name`) | в подклассе без `this.name = …` остаётся `'Error'` |
| `cause` | собственное свойство, **только если** передан ключ `cause` в опциях | `{ cause: 0 }` → `0`; `{}` → свойства нет (`'cause' in e` — `false`); `{ cause: undefined }` → свойство есть |
| `stack` | нестандартное строковое свойство V8 | первая строка — `name: message` |
| `Error.prototype.toString()` | `name: message` | для `{ name: 'N', message: 'M' }` → `N: M`, для `{}` → `Error` |
| `Error.captureStackTrace(obj, fn)` | V8: записать стек в `obj`, скрыв кадры выше `fn` | у `obj` появляется `stack` |
| `Error.stackTraceLimit` | V8: число кадров (по умолчанию `10`) | `1` → в `stack` две строки |
| `Error.prepareStackTrace(err, frames)` | V8: свой формат стека | `frames[0].getLineNumber()` — число |
| `Error.isError(x)` | проверка «настоящей» ошибки, в том числе из другого контекста | в Node.js 24.15 — `function` |
| `AggregateError(errors, message)` | `errors` — итерируемый объект | наследует от `Error` |
| `DOMException(message, name)` | веб-исключение, доступно в Node.js | `de instanceof Error` — `true`, `de.code` — `20` для `AbortError`, тег `[object DOMException]` |

Свойства `Error.prototype` (в опыте): `constructor`, `name`, `message`, `toString`.

### Встроенные ошибки и коды Node.js

| Источник | Что бросается |
|---|---|
| Ошибки языка | `TypeError`, `RangeError`, `ReferenceError`, `SyntaxError`, `URIError` |
| Системные операции | `Error` с `code` (`ENOENT`), `syscall`, `errno` |
| Внутренние ошибки Node.js | `Error` с `code` вида `ERR_*` (`ERR_MODULE_NOT_FOUND`, `ERR_UNHANDLED_ERROR`, `ERR_ASSERTION`) |
| `node:assert` | `AssertionError` (`code: 'ERR_ASSERTION'`, `generatedMessage`) |
| Отмена | `AbortError` (`DOMException`, `code` `20`), `TimeoutError` |

## Внутреннее устройство

- **Когда строится стек.** **Кадры** фиксируются при **создании** объекта, а не при `throw`: ошибка, созданная в строке 1 и брошенная из другой функции, показывает строку создания (в опыте `d.js:1:16`, `e === shared`). Поэтому «общую» ошибку-константу создавать заранее вредно: у неё чужой стек.
- **Текст стека.** Строка `stack` форматируется лениво, при первом обращении: в опыте после `m.message = 'new'` заголовок стека был `Error: new`, а после `n.name = 'Custom'` — `Custom: x`. Не рассчитывайте на это поведение: оно не определено стандартом.
- **`stack` не часть языка.** Стандарт ECMAScript не описывает `stack`; формат V8 (`at функция (файл:строка:столбец)`) отличается в других движках.
- **Производные классы.** `class X extends Error` вызывает `super(message, options)`, который создаёт объект, фиксирует стек и ставит `cause`. До `super` обращение к `this` — `ReferenceError: Must call super constructor in derived class before accessing 'this' or returning from derived constructor` (в опыте).
- **`finally` и переопределение.** `return` или `throw` в `finally` заменяют исход `try`: `throw new Error('from finally')` перекрыл `inner`.
- **Необработанные ошибки.** Исключение без `catch` доходит до цикла событий, превращается в событие `uncaughtException`; без обработчика процесс завершается с ненулевым кодом ([модуль 13](../13-event-loop-and-concurrency/expert.ru.md#подвохи)).
- **Асинхронные кадры.** V8 восстанавливает цепочку `await`, поэтому в стеке видны `at async a2` (в опыте).

> ⚠️ Проверить: формат и содержимое `stack` в других средах (Firefox, Safari, Deno, Bun), поведение `Error.prepareStackTrace` при вложенных вызовах и точная семантика ленивого форматирования заголовка по спецификации не проверялись.

## Подвохи

### 1. `return` в `finally` проглатывает ошибку

`f2` вернул `'finally wins'`; брошенная `lost` потеряна.

### 2. Подкласс без `name`

`new Plain('x').name` — `Error`, хотя `constructor.name` — `Plain`.

### 3. `JSON.stringify(error)` — `{}`

Поля `message` и `stack` не перечисляются; нужен `toJSON`.

### 4. `instanceof Error` не работает для чужого контекста

`vm`-ошибка: `foreign instanceof Error` — `false`, а `Error.isError(foreign)` — `true`.

### 5. Стек указывает на место создания

Ошибка, созданная заранее и брошенная позже, показывает старый стек.

### 6. Исход `{ cause }` зависит от ключа

`new Error('m', {})` не создаёт свойство `cause`; `{ cause: undefined }` создаёт его.

### 7. Новая ошибка в `catch` без `cause` теряет исходную

`throw new Error('second')` внутри `catch` оставляет `cause === undefined`.

### 8. Бросок после `resolve` в промисе игнорируется

`new Promise(res => { res('ok'); throw new Error('ignored') })` выполнится значением `'ok'` (в опыте).

### 9. `console.log(error)` показывает больше, чем `String(error)`

`console.log` выводит `stack` и `cause`; `String` — только `name: message`.

## Компромиссы и альтернативы

| Подход | Плюсы | Минусы |
|---|---|---|
| Исключения | стек, всплытие, `finally` | дорого при частом использовании; легко проглотить |
| Возврат результата (`{ ok, value, error }`) | явная обработка, без исключений | шаблонный код, легко забыть проверку |
| Коды ошибок (`code`) | стабильно, сериализуется | нужна дисциплина |
| Иерархия классов | ловля по типу | `instanceof` ломается между контекстами |
| `assert` | быстрые проверки инвариантов | только для программных ошибок |
| Библиотеки результатов (neverthrow и др.) | типобезопасные результаты | зависимость; не запускались |

## Вопросы с собеседований

<details><summary>1. Чем <code>throw</code> отличается от <code>return</code> с ошибкой?</summary>

`throw` прерывает выполнение и «всплывает» по стеку до `catch`; `return` просто отдаёт значение вызывающему, который обязан проверить его.

→ [Базовая теория](basics.ru.md#throw-и-trycatchfinally)

</details>

<details><summary>2. Что выполнится в <code>finally</code> после <code>return</code> в <code>try</code>?</summary>

Блок `finally` выполнится всегда, затем вернётся результат `try` (если `finally` не переопределит исход).

→ [Базовая теория](basics.ru.md#throw-и-trycatchfinally)

</details>

<details><summary>3. Подвох: что вернёт <code>f()</code>, если в <code>try</code> брошено исключение, а в <code>finally</code> — <code>return 'x'</code>?</summary>

`'x'`: ошибка проглочена.

→ [Подвох 1](#1-return-в-finally-проглатывает-ошибку)

</details>

<details><summary>4. Зачем <code>cause</code>?</summary>

Сохранить исходную ошибку при оборачивании, чтобы видеть всю цепочку.

→ [Продвинутая теория](advanced.ru.md#цепочка-причин-cause)

</details>

<details><summary>5. Подвох: почему <code>JSON.stringify(new Error('x'))</code> возвращает <code>{}</code>?</summary>

`message` и `stack` — неперечисляемые свойства.

→ [Подвох 3](#3-jsonstringifyerror--)

</details>

<details><summary>6. Как создать свой тип ошибки и корректно её ловить?</summary>

`class X extends Error`, задать `name`, при ловле — `instanceof` и проброс неизвестных.

→ [Продвинутая теория](advanced.ru.md#свои-классы-ошибок)

</details>

<details><summary>7. Почему <code>try/catch</code> не ловит ошибку в <code>setTimeout</code>?</summary>

Колбэк выполняется позже в другом стеке; ошибку нужно ловить внутри него.

→ [Продвинутая теория](advanced.ru.md#асинхронные-ошибки-и-обработчики-верхнего-уровня)

</details>

<details><summary>8. Что такое <code>AggregateError</code>?</summary>

Ошибка со списком `errors`; её бросает `Promise.any`, если отклонены все.

→ [Продвинутая теория](advanced.ru.md#aggregateerror)

</details>

<details><summary>9. Подвох: в какой момент фиксируется стек — при <code>new Error</code> или при <code>throw</code>?</summary>

Кадры — при создании; стек показывает место создания, а не броска.

→ [Подвох 5](#5-стек-указывает-на-место-создания)

</details>

<details><summary>10. Что делать в обработчике <code>uncaughtException</code>?</summary>

Залогировать и корректно завершить процесс; продолжать работу небезопасно.

→ [Продвинутая теория](advanced.ru.md#асинхронные-ошибки-и-обработчики-верхнего-уровня)

</details>

<details><summary>11. Чем <code>Error.isError</code> лучше <code>instanceof Error</code>?</summary>

Работает для ошибок из других контекстов (`vm`, `iframe`).

→ [Подвох 4](#4-instanceof-error-не-работает-для-чужого-контекста)

</details>

<details><summary>12. Как скрыть служебные кадры в стеке своей ошибки?</summary>

`Error.captureStackTrace(this, ConstructorOrFn)`: кадры выше указанной функции отбрасываются.

→ [Справочник](#конструктор-и-свойства)

</details>

## Шпаргалка

```text
throw new Error('msg', { cause })     // всегда Error
class E extends Error { constructor(m, o) { super(m, o); this.name = 'E' } }
try { … } catch (e) { if (!(e instanceof E)) throw e } finally { cleanup }
e.name, e.message, e.cause, e.stack;  Error.isError(x);  Error.captureStackTrace(o)
process.on('uncaughtException' | 'unhandledRejection', …) — залогировать и выйти
JSON.stringify(err) → {}  (нужен toJSON)
```

- `return`/`throw` в `finally` перекрывают исход.
- Стек указывает на место создания.
- Ловить только то, что умеете обработать.

## Связанные материалы

- Предыдущий уровень: [продвинутая теория](advanced.ru.md)
- Промисы: [12 · Промисы и async/await](../12-promises-and-async-await/expert.ru.md)
- Цикл событий: [13 · Цикл событий и параллелизм](../13-event-loop-and-concurrency/expert.ru.md)
- Исключения в C#: [08 · Исключения](../../../csharp/modules/08-exceptions/expert.ru.md)

## Источники

- [ECMAScript: Error Objects](https://tc39.es/ecma262/#sec-error-objects)
- [MDN: Error](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Error)
- [V8: Stack trace API](https://v8.dev/docs/stack-trace-api)
- [Node.js: Errors](https://nodejs.org/docs/latest-v24.x/api/errors.html)
- [MDN: DOMException](https://developer.mozilla.org/en-US/docs/Web/API/DOMException)
