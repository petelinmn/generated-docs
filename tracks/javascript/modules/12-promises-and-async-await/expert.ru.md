---
title: "Промисы и async/await: экспертный уровень"
lang: ru
track: javascript
kind: module
doc: expert
slug: 12-promises-and-async-await
part: 2
tags: [promise, microtask, job-queue, nexttick, thenable, async-stack-trace, unhandled-rejection, aggregate-error]
versions: "ECMAScript 2025+, Node.js 24.15 LTS"
status: draft
updated: 2026-11-23
---

# Промисы и async/await: экспертный уровень

🌐 **Русский** · [English](expert.en.md)

**Уровни:** [overview](overview.ru.md) · [1. Основы](basics.ru.md) · [2. Продвинутый](advanced.ru.md) · **3. Эксперт** · [4. Квиз](quiz.ru.md)

Опирается на [базовую](basics.ru.md) и [продвинутую](advanced.ru.md) теорию. Все результаты получены запуском на Node.js 24.15.

## Справочник

### Конструктор и статические методы

| Член | Что делает |
|---|---|
| `new Promise(executor)` | `executor(resolve, reject)` вызывается синхронно; исключение в нём — отклонение |
| `Promise.resolve(x)` | для промиса возвращает **тот же** объект, иначе создаёт выполненный (в опыте `Promise.resolve(p) === p` — `true`) |
| `Promise.reject(e)` | отклонённый промис |
| `Promise.all`, `allSettled`, `race`, `any` | комбинаторы ([продвинутая теория](advanced.ru.md#комбинаторы)) |
| `Promise.withResolvers()` | `{ promise, resolve, reject }` |
| `Promise.try(fn)` | запускает `fn`, синхронное исключение превращает в отклонение |

### Методы экземпляра

| Метод | Особенности |
|---|---|
| `then(onFulfilled, onRejected)` | возвращает новый промис; не-функции пропускают значение дальше (в опыте `then(null).then(undefined)` передал `'pass'`) |
| `catch(onRejected)` | то же, что `then(undefined, onRejected)` |
| `finally(fn)` | `fn` без аргументов; возвращённое значение игнорируется, исключение в `fn` отклоняет результат (в опыте `in finally`) |

### Что возвращает `async`-функция

Всегда промис; `return x` — выполнить значением `x` (если `x` промис или thenable — «выпрямить»), `throw e` — отклонить. `Object.prototype.toString.call(f)` — `[object AsyncFunction]`.

### Типичные ошибки Node.js, связанные с промисами

| Что | Что происходит |
|---|---|
| `resolve` самим промисом | `TypeError: Chaining cycle detected for promise #<Promise>` (в опыте) |
| `Promise.any([])` или все отклонены | `AggregateError`: `errors` — массив причин, `message` — `All promises were rejected` |
| Необработанное отклонение | вывод ошибки и код выхода `1`, если нет обработчика `unhandledRejection` |
| Незавершённый `await` верхнего уровня | предупреждение и код выхода `13` (в опыте с `AbortSignal.timeout`) |

## Внутреннее устройство

- **Очередь микрозадач (microtask queue).** Реакции `then`/`catch`/`finally` и продолжения после `await` ставятся в очередь микрозадач. Она полностью исполняется после текущей синхронной части и **до** следующей макрозадачи (таймер, ввод-вывод). Поэтому все `then` срабатывают раньше `setTimeout(…, 0)`.
- **Порядок в опыте (CommonJS):** `1 sync` → `2 nextTick` → `3 micro` (промис) → `4 queueMicrotask` → `5 timeout`. В **ES-модуле** `process.nextTick` срабатывает **после** очереди промисов (в опыте `2 nextTick` вывелся после всех `then`, перед `5 timeout`), потому что сам модуль выполняется внутри микрозадачи. Не полагайтесь на порядок `nextTick` и промисов.
- **Выпрямление.** Если `then` вернул промис или thenable, результат подключается к нему отдельной реакцией: на это уходят дополнительные тики очереди. Поэтому цепочка с возвратом промиса из `then` может завершиться позже, чем ожидается.
- **`await`.** `await x` приводит `x` к промису и подписывает продолжение функции на его результат; функция приостанавливается, управление возвращается вызывающему.
- **Thenable.** У объекта с методом `then` движок вызывает `then` как асинхронную задачу; в опыте объект `{ then(res) { n++; res('t') } }` после `await Promise.resolve(th); await th;` получил `then calls 2`: каждый `await`/`resolve` вызывает `then` заново, результат не запоминается.
- **Асинхронные трассировки стека.** V8 восстанавливает цепочку `await`: ошибка из `inner`, вызванной в `outer`, показывает `at inner (…)` и `at async outer (…)` (в опыте). Кадры `async` — признак восстановленной трассы.
- **Необработанные отклонения.** После опустошения очереди микрозадач Node.js проверяет, у каких отклонённых промисов нет обработчика, и вызывает `unhandledRejection` (в опыте для `Promise.reject(...)` и для `late`); если обработчик добавлен позднее, генерируется `rejectionHandled` (в опыте для `late`).

> ⚠️ Проверить: точное число тиков очереди при выпрямлении промиса в `then` и в `async`-функции разных движков не измерялось; режимы `--unhandled-rejections` (`warn`, `strict`, `none` и др.) не запускались.

- **Ресурсы и процесс.** Промис сам не держит процесс живым: его удерживают таймеры и открытые дескрипторы. Таймер `AbortSignal.timeout` не удерживает (в опыте процесс завершился с предупреждением).

## Подвохи

### 1. `forEach(async …)` не ждёт

`[1, 2, 3].forEach(async x => …)` возвращает управление сразу: `out.length` — `0`.

### 2. `return promise` в `try/catch`

Ошибка не перехватывается: `escaped E`, а с `return await` — `caught`.

### 3. `nextTick` и промисы

В CommonJS `nextTick` раньше промисов, в ES-модуле — позже.

### 4. Исполнитель выполняется синхронно

Код внутри `new Promise(…)` запускается сразу; асинхронными бывают только `then`-обработчики.

### 5. `Promise.resolve` не всегда создаёт новый объект

`Promise.resolve(p) === p` — `true`, а `new Promise(r => r(p)) === p` — `false`.

### 6. `finally` может подменить исход

Исключение в `finally` отклоняет результат (`in finally`); возвращённое значение игнорируется (`finally keeps keep`); отклонение при этом сохраняется (`finally rethrows e1`).

### 7. `then(null)` пропускает значение

`Promise.resolve('pass').then(null).then(undefined)` доставит `'pass'`.

### 8. `race` и `all` ничего не отменяют

Проигравшие операции работают дальше; для остановки нужен `AbortSignal`.

### 9. Таймер `AbortSignal.timeout` не удерживает процесс

Если больше ничего не ждёт, Node.js завершится с предупреждением `unsettled top-level await`.

### 10. Thenable вызывается повторно

Каждое приведение объекта с `then` вызывает метод заново (`then calls 2`).

## Компромиссы и альтернативы

| Подход | Плюсы | Минусы |
|---|---|---|
| Промисы и цепочки | композиция, комбинаторы | громоздко при ветвлении |
| `async`/`await` | линейный код, `try/catch` | легко потерять параллелизм и `return await` |
| Колбэки | просты, без накладных расходов | «ад колбэков», ручная обработка ошибок |
| Асинхронные итераторы | поток значений | последовательные |
| Библиотеки (Bluebird, p-limit, p-retry) | пул, повтор, отмена | зависимость; не запускались |
| События (`EventEmitter`) | много значений за время | нет одного результата, ошибки через событие |
| Потоки (streams) | обратное давление | сложнее API; не запускались |

## Вопросы с собеседований

<details><summary>1. Что такое промис и какие у него состояния?</summary>

Объект, представляющий будущий результат; состояния pending, fulfilled, rejected; переход один раз.

→ [Базовая теория](basics.ru.md#состояния-промиса)

</details>

<details><summary>2. Синхронно или асинхронно выполняется функция-исполнитель?</summary>

Синхронно; асинхронны только обработчики `then`.

→ [Подвох 4](#4-исполнитель-выполняется-синхронно)

</details>

<details><summary>3. Как работает цепочка <code>then</code>?</summary>

Каждый `then` возвращает новый промис; значение или исключение обработчика определяет его состояние; ошибка идёт до ближайшего `catch`.

→ [Базовая теория](basics.ru.md#then-catch-finally)

</details>

<details><summary>4. Подвох: чем <code>return promise</code> отличается от <code>return await promise</code> в <code>try/catch</code>?</summary>

Первое не перехватывает ошибку промиса, второе — перехватывает.

→ [Подвох 2](#2-return-promise-в-trycatch)

</details>

<details><summary>5. В каком порядке выполнятся <code>setTimeout 0</code>, <code>then</code>, <code>queueMicrotask</code>, <code>nextTick</code>?</summary>

Сначала синхронный код, затем `nextTick` (в CommonJS) и микрозадачи (`then`, `queueMicrotask`), потом таймер. В ES-модуле `nextTick` после промисов.

→ [Внутреннее устройство](#внутреннее-устройство)

</details>

<details><summary>6. Чем <code>Promise.all</code> отличается от <code>allSettled</code>, <code>race</code> и <code>any</code>?</summary>

`all` — все успешны или первая ошибка; `allSettled` — результаты всех; `race` — первый завершившийся; `any` — первый успешный.

→ [Продвинутая теория](advanced.ru.md#комбинаторы)

</details>

<details><summary>7. Подвох: почему <code>forEach(async …)</code> не работает как ожидается?</summary>

`forEach` игнорирует возвращаемые промисы и не ждёт их.

→ [Подвох 1](#1-foreachasync--не-ждёт)

</details>

<details><summary>8. Как ограничить число одновременных операций?</summary>

Пул из `n` «воркеров», каждый берёт следующий элемент; `await Promise.all(workers)`.

→ [Продвинутая теория](advanced.ru.md#ограничение-параллелизма)

</details>

<details><summary>9. Как отменить асинхронную операцию?</summary>

`AbortController`/`AbortSignal`: операция должна принимать сигнал; `Promise.race` останавливает только ожидание.

→ [Продвинутая теория](advanced.ru.md#таймаут-и-отмена)

</details>

<details><summary>10. Что произойдёт с необработанным отклонением в Node.js?</summary>

По умолчанию процесс завершится с кодом `1`; перехватить можно через `process.on('unhandledRejection')`.

→ [Продвинутая теория](advanced.ru.md#необработанные-отклонения)

</details>

<details><summary>11. Подвох: возвращает ли <code>Promise.resolve(p)</code> новый промис, если <code>p</code> — промис?</summary>

Нет, тот же объект; `new Promise(r => r(p))` создаёт новый.

→ [Подвох 5](#5-promiseresolve-не-всегда-создаёт-новый-объект)

</details>

<details><summary>12. Как <code>finally</code> влияет на результат цепочки?</summary>

Не меняет значение или причину; но исключение внутри `finally` отклоняет результат.

→ [Подвох 6](#6-finally-может-подменить-исход)

</details>

## Шпаргалка

```text
new Promise((res, rej) => …)   // исполнитель синхронный; res/rej — один раз
p.then(f).catch(g).finally(h)  // then возвращает новый промис
async function f() { try { return await g() } catch (e) {…} }   // return await — в try
await Promise.all([…])  // параллельно; allSettled / race / any
Promise.withResolvers(); Promise.try(fn)
AbortSignal.timeout(ms); controller.abort(); st(ms, v, { signal })
process.on('unhandledRejection', (reason, promise) => …)
```

- `then`-обработчики — микрозадачи: после синхронного кода, до таймеров.
- `forEach(async …)` не ждёт.
- `race` и `all` ничего не отменяют.

## Связанные материалы

- Предыдущий уровень: [продвинутая теория](advanced.ru.md)
- Итераторы и генераторы: [11 · Итераторы и генераторы](../11-iterators-and-generators/expert.ru.md)
- Функции: [04 · Функции](../04-functions/expert.ru.md)
- Цикл событий: модуль 13 дорожной карты ([трек](../../overview.ru.md))
- Аналог в C#: [10 · async/await](../../../csharp/modules/10-async-await/expert.ru.md)

## Источники

- [ECMAScript: Promise Objects](https://tc39.es/ecma262/#sec-promise-objects)
- [ECMAScript: Async Function Objects](https://tc39.es/ecma262/#sec-async-function-objects)
- [MDN: Promise](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Promise)
- [MDN: Microtask guide](https://developer.mozilla.org/en-US/docs/Web/API/HTML_DOM_API/Microtask_guide)
- [Node.js: process.nextTick](https://nodejs.org/docs/latest-v24.x/api/process.html#processnexttickcallback-args)
- [V8: Fast async](https://v8.dev/blog/fast-async)
