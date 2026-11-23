---
title: "Цикл событий и параллелизм: экспертный уровень"
lang: ru
track: javascript
kind: module
doc: expert
slug: 13-event-loop-and-concurrency
part: 2
tags: [event-loop, libuv, microtask-starvation, timers-clamping, atomics, worker-threads, structured-clone, performance]
versions: "ECMAScript 2025+, Node.js 24.15 LTS"
status: draft
updated: 2026-11-23
---

# Цикл событий и параллелизм: экспертный уровень

🌐 **Русский** · [English](expert.en.md)

**Уровни:** [overview](overview.ru.md) · [1. Основы](basics.ru.md) · [2. Продвинутый](advanced.ru.md) · **3. Эксперт** · [4. Квиз](quiz.ru.md)

Опирается на [базовую](basics.ru.md) и [продвинутую](advanced.ru.md) теорию. Все результаты получены запуском на Node.js 24.15.

## Справочник

### Очереди и API

| Очередь | API | Когда выполняется |
|---|---|---|
| `nextTick` | `process.nextTick(fn, ...args)` | после текущей операции, раньше микрозадач (CommonJS) |
| Микрозадачи | `Promise.then`, `queueMicrotask(fn)`, `await` | после `nextTick`-очереди и после каждого колбэка |
| Таймеры | `setTimeout`, `setInterval` | фаза timers |
| Ввод-вывод | колбэки `fs`, сети | фаза poll |
| Check | `setImmediate(fn, ...args)` | после poll |
| Закрытие | `close`-колбэки | фаза close |

### Таймеры и ограничения

| Поведение | Результат в опыте |
|---|---|
| Задержка `0` и `1` | равнозначны: `setTimeout(…, 1)` и `setTimeout(…, 0)` срабатывают в порядке создания (`[ 't1', 't0' ]`) |
| Задержка больше `2**31 - 1` | `TimeoutOverflowWarning`, задержка заменяется на `1` |
| Объект таймера | `Timeout` с `ref()`, `unref()`, `hasRef()`, `refresh()`; `setImmediate` возвращает `Immediate` |
| Отмена | `clearTimeout`, `clearInterval`, `clearImmediate` |
| Исключение в колбэке | необработанное исключение процесса: ловится `process.on('uncaughtException')` |

### Atomics (в опыте)

| Операция | Результат |
|---|---|
| `Atomics.add(ia, i, v)` / `sub` / `and` / `or` / `xor` | возвращают **старое** значение (`add` → `0`, затем `load` → `5`) |
| `Atomics.compareExchange(ia, i, expected, replacement)` | записывает, если значение равно `expected`; возвращает старое |
| `Atomics.exchange`, `load`, `store` | обмен, чтение, запись |
| `Atomics.wait(ia, i, expected, timeout)` | `'timed-out'` по таймауту, `'not-equal'`, если значение другое |
| `Atomics.notify(ia, i, count)` | число разбуженных (`0`, если никто не ждал) |
| `Atomics.isLockFree(4)` | `true` |

`Atomics.wait` блокирует поток: в основном потоке Node.js он допустим, но останавливает цикл.

## Внутреннее устройство

- **libuv.** Цикл событий Node.js реализован в библиотеке libuv; V8 выполняет JavaScript и очередь микрозадач. Файловые операции и часть работы (DNS, crypto, zlib) libuv выполняет в **пуле потоков** (по умолчанию 4); сетевой ввод-вывод — через системные механизмы ожидания событий.
- **Один стек.** Все JavaScript-колбэки выполняются на одном потоке: рабочий поток — отдельный экземпляр V8 со своим циклом и кучей, поэтому объекты между потоками не разделяются, только копируются или передаются.
- **Голодание микрозадач.** Очередь микрозадач выполняется до опустошения: самовоспроизводящиеся микрозадачи не дают сработать таймеру. В опыте таймер `0` сработал только после 200001 микрозадачи (`timer after starvation, microtasks: 200001`).
- **Передача.** `postMessage(value, [transfer])` и `structuredClone(value, { transfer })` «отделяют» буфер: у отправителя `byteLength` становится `0`, свойство `detached` — `true` (в опыте `0 4 true`).
- **Ошибки вне цепочек.** Исключение в `queueMicrotask`/таймере не попадает ни в какой `try/catch`: оно становится необработанным исключением процесса (в опыте `uncaught: in microtask`, `uncaught: in timer` при обработчике `uncaughtException`; без него процесс завершается).
- **Измерения.** `performance.now()` монотонно; `Date.now()` может прыгнуть при смене системного времени.

> ⚠️ Проверить: размер пула потоков libuv по умолчанию (4), список операций, которые выполняются в нём, и переменная `UV_THREADPOOL_SIZE` взяты из документации и не запускались.

> ⚠️ Проверить: поведение `Atomics.wait` в браузерах (запрещён в основном потоке) и поведение потоков при `resourceLimits` не проверялись.

## Подвохи

### 1. `setTimeout(…, 0)` не сработает до микрозадач

Любой `then` или `queueMicrotask` выполнится раньше; в опыте таймер ждал 200001 микрозадачу.

### 2. Порядок `setTimeout` и `setImmediate`

В основном модуле он не гарантирован; внутри ввода-вывода `setImmediate` раньше (в опыте `io: immediate`, затем `io: timeout`).

### 3. `nextTick` и промисы

В CommonJS `nextTick` раньше промисов (`A F B C D E`), в ES-модуле позже ([модуль 12](../12-promises-and-async-await/expert.ru.md#внутреннее-устройство)).

### 4. Исключение в таймере или микрозадаче

Не попадает в `try/catch` вокруг `setTimeout`: процесс получит `uncaughtException`.

### 5. Таймер держит процесс

Живой `setInterval` не даёт процессу завершиться; `unref()` снимает это (в опыте процесс завершился после `end`).

### 6. Общий счётчик без `Atomics`

Обновления теряются; нужны `Atomics.add` (в опыте итог `1500`).

### 7. Передача обнуляет буфер

После `transfer` `byteLength` у отправителя — `0`.

### 8. Задержки не точные

`ms` — минимум; таймер, поставленный на 10 мс, после блокировки на 80 мс сработал позже (в опыте `timer fired late: true`).

### 9. Потоки не для ввода-вывода

Ввод-вывод уже асинхронен; поток дороже и не ускорит ожидание сети.

## Компромиссы и альтернативы

| Подход | Плюсы | Минусы |
|---|---|---|
| Порции с `setImmediate` | просто, один поток | дробить код, общее время дольше |
| Рабочие потоки | настоящий параллелизм CPU | копирование данных, запуск стоит дорого |
| Дочерние процессы | изоляция, любой язык | ещё дороже, обмен через каналы |
| `SharedArrayBuffer` + `Atomics` | без копирования | сложно, легко ошибиться |
| Кластер (`cluster`) | много процессов на одном порту | отдельная память; не запускался |
| Внешняя очередь задач | масштабирование на машины | инфраструктура; не запускалась |

## Вопросы с собеседований

<details><summary>1. Почему JavaScript однопоточный, а Node.js обрабатывает много запросов?</summary>

Ввод-вывод асинхронен: операции выполняются в фоне (ОС, пул потоков libuv), а JavaScript-колбэки идут на одном потоке по очереди.

→ [Базовая теория](basics.ru.md#один-поток)

</details>

<details><summary>2. Чем макрозадачи отличаются от микрозадач?</summary>

Микрозадачи (`then`, `queueMicrotask`) выполняются полностью после каждой макрозадачи (таймер, ввод-вывод); макрозадачи — по одной за итерацию.

→ [Базовая теория](basics.ru.md#макрозадачи-и-микрозадачи)

</details>

<details><summary>3. Подвох: в каком порядке сработают <code>setTimeout 0</code> и <code>setImmediate</code>?</summary>

В основном модуле не гарантирован; внутри колбэка ввода-вывода `setImmediate` раньше.

→ [Подвох 2](#2-порядок-settimeout-и-setimmediate)

</details>

<details><summary>4. Что такое <code>process.nextTick</code> и чем опасен рекурсивный?</summary>

Отдельная очередь Node.js, выполняется раньше микрозадач промисов; рекурсия не пускает ввод-вывод и таймеры.

→ [Базовая теория](basics.ru.md#setimmediate-и-processnexttick)

</details>

<details><summary>5. Как не заблокировать цикл тяжёлым вычислением?</summary>

Порции с `setImmediate` или рабочий поток.

→ [Базовая теория](basics.ru.md#блокировка-цикла)

</details>

<details><summary>6. Подвох: гарантирует ли <code>setTimeout(fn, 100)</code> вызов ровно через 100 мс?</summary>

Нет, это минимальная задержка; блокирующий код или загрузка цикла сдвинут вызов.

→ [Подвох 8](#8-задержки-не-точные)

</details>

<details><summary>7. Чем рабочий поток отличается от процесса?</summary>

Поток — в одном процессе, отдельный V8 и цикл, обмен сообщениями или общей памятью; процесс изолирован полностью, обмен через каналы.

→ [Продвинутая теория](advanced.ru.md#рабочие-потоки-worker-threads)

</details>

<details><summary>8. Как передать данные между потоками без копирования?</summary>

Transfer (`postMessage(v, [buf])`) или `SharedArrayBuffer`.

→ [Продвинутая теория](advanced.ru.md#сообщения-копия-и-передача)

</details>

<details><summary>9. Зачем нужен <code>Atomics</code>?</summary>

Атомарные операции над общей памятью: без них параллельные `a[0]++` теряют обновления.

→ [Продвинутая теория](advanced.ru.md#sharedarraybuffer-и-atomics)

</details>

<details><summary>10. Подвох: исключение внутри <code>setTimeout</code> — поймает ли его <code>try/catch</code> вокруг?</summary>

Нет: колбэк выполняется позже, в другом стеке. Исключение станет `uncaughtException`.

→ [Подвох 4](#4-исключение-в-таймере-или-микрозадаче)

</details>

<details><summary>11. Что такое голодание микрозадач?</summary>

Самовоспроизводящиеся микрозадачи не дают выполниться таймерам и вводу-выводу.

→ [Внутреннее устройство](#внутреннее-устройство)

</details>

<details><summary>12. Как измерить блокировку цикла?</summary>

`monitorEventLoopDelay()` из `perf_hooks` или замер `performance.now()` вокруг таймера.

→ [Продвинутая теория](advanced.ru.md#измерение-времени-и-задержки-цикла)

</details>

## Шпаргалка

```text
sync → nextTick (CJS) → микрозадачи → таймеры → poll (I/O) → setImmediate → close
setTimeout(fn, ms, ...args); t.unref(); clearTimeout(t)
setImmediate(fn) — после I/O; в I/O раньше setTimeout 0
new Worker(code, { eval: true, workerData }); parentPort.postMessage / on('message')
postMessage(v, [buf]) — transfer; SharedArrayBuffer + Atomics.add/load
performance.now(); monitorEventLoopDelay()
```

- Микрозадачи выполняются до конца раньше следующей макрозадачи.
- Потоки — для CPU, не для ввода-вывода.
- Общая память — только через `Atomics`.

## Связанные материалы

- Предыдущий уровень: [продвинутая теория](advanced.ru.md)
- Промисы: [12 · Промисы и async/await](../12-promises-and-async-await/expert.ru.md)
- Параллелизм в C#: [18 · Параллелизм](../../../csharp/modules/18-concurrency/expert.ru.md)
- Процессы и потоки в Linux: [Linux](../../../linux/overview.ru.md)

## Источники

- [Node.js: Event Loop, Timers, and process.nextTick()](https://nodejs.org/en/learn/asynchronous-work/event-loop-timers-and-nexttick)
- [Node.js: Worker threads](https://nodejs.org/docs/latest-v24.x/api/worker_threads.html)
- [Node.js: Timers](https://nodejs.org/docs/latest-v24.x/api/timers.html)
- [HTML: Event loops](https://html.spec.whatwg.org/multipage/webappapis.html#event-loops)
- [MDN: Atomics](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Atomics)
- [libuv: Design overview](https://docs.libuv.org/en/v1.x/design.html)
