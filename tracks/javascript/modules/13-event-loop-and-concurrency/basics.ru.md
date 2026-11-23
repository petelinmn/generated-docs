---
title: "Цикл событий и параллелизм: основы"
lang: ru
track: javascript
kind: module
doc: basics
slug: 13-event-loop-and-concurrency
part: 2
tags: [event-loop, call-stack, macrotask, microtask, set-timeout, set-immediate, next-tick, blocking]
versions: "ECMAScript 2025+, Node.js 24.15 LTS"
status: draft
updated: 2026-11-23
---

# Цикл событий и параллелизм: основы

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [overview](overview.ru.md) · **1. Основы** · [2. Продвинутый](advanced.ru.md) · [3. Эксперт](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [12 · Промисы и async/await](../12-promises-and-async-await/overview.ru.md) · **Время:** ~2.5 ч

Все примеры запускались на Node.js 24.15; файлы лежат рядом с `package.json` (`{"type": "module"}`), если не сказано иное. Задержки в выводе нестрогие: точное время зависит от машины.

## Зачем это нужно

Чтобы понимать, **когда** выполняется код, почему «зависает» сервер и в каком порядке срабатывают таймеры и промисы. Без этого асинхронные ошибки кажутся случайными.

## Один поток

JavaScript выполняет код в одном потоке: есть **стек вызовов** (call stack), и пока он не опустеет, никакая другая задача не начнётся. Ввод-вывод, таймеры и сеть среда выполнения обрабатывает в фоне, а результат возвращает через **очереди**, из которых **цикл событий** (event loop) берёт задачи, когда стек свободен.

## Макрозадачи и микрозадачи

```js
console.log('1 sync');
setTimeout(() => console.log('4 timeout'), 0);
Promise.resolve().then(() => console.log('3 microtask'));
console.log('2 sync');
```

```text
1 sync
2 sync
3 microtask
4 timeout
```

- **Макрозадачи** (macrotasks): таймеры, ввод-вывод, `setImmediate`. Цикл берёт по одной.
- **Микрозадачи** (microtasks): обработчики `then`/`catch`/`finally`, продолжения после `await`, `queueMicrotask`. После каждой макрозадачи очередь микрозадач выполняется **целиком**, прежде чем начнётся следующая макрозадача.
- Поэтому промис всегда срабатывает раньше таймера `0`.

Подробности про промисы — в [модуле 12](../12-promises-and-async-await/expert.ru.md#внутреннее-устройство).

## Таймеры

```js
setTimeout((a, b) => console.log('args', a, b), 100, 'x', 'y');
let n = 0; const iv = setInterval(() => { if (++n === 3) { clearInterval(iv); console.log('interval x3'); } }, 3);
const t = setTimeout(() => {}, 1000);
console.log(typeof t, typeof t.ref, t.hasRef());
t.unref(); console.log(t.hasRef()); clearTimeout(t);
```

```text
object function true
false
interval x3
args x y
```

- `setTimeout(fn, ms, ...args)` вызывает `fn` не раньше чем через `ms` миллисекунд; дополнительные аргументы передаются в `fn`.
- `setInterval` повторяет вызов; его останавливают `clearInterval`.
- В Node.js таймер — **объект** (`typeof t` — `object`, не число, как в браузере) с методами `ref()`, `unref()`, `hasRef()`. `unref()` разрешает процессу завершиться, даже если таймер ещё не сработал.
- `ms` — **минимальная** задержка: фактическое срабатывание зависит от загрузки цикла.

## Блокировка цикла

Долгий синхронный код задерживает всё:

```js
const t0 = Date.now();
setTimeout(() => console.log('timer fired late:', Date.now() - t0 >= 80), 10);
while (Date.now() - t0 < 80);
console.log('blocked');
```

```text
blocked
timer fired late: true
```

Таймер на 10 мс сработал только после 80 мс блокирующего цикла. В сервере это значит, что один тяжёлый запрос тормозит все остальные.

**Как не блокировать:** разбейте работу на порции и отдавайте управление циклу через `setImmediate`:

```js
function heavy(n) { let s = 0; for (let i = 0; i < n; i++) s += i; return s; }
let ticks = 0; const iv = setInterval(() => ticks++, 1);
let done = 0;
await new Promise(res => { function step() { heavy(2e6); if (++done < 20) setImmediate(step); else res(); } step(); });
clearInterval(iv); console.log('chunked: timer ran during work', ticks > 0);
```

```text
chunked: timer ran during work true
```

Тот же объём работы одним блоком не дал бы таймеру сработать ни разу (в опыте — `blocking: timer ran during work false`). Для тяжёлых вычислений лучше [рабочий поток](advanced.ru.md#рабочие-потоки-worker-threads).

## setImmediate и process.nextTick

- `setImmediate(fn)` — вызвать `fn` на следующей итерации цикла, **после** текущего ввода-вывода.
- `process.nextTick(fn)` — вызвать `fn` **сразу после текущей операции**, раньше микрозадач промисов (в CommonJS).

```js
// CommonJS (.cjs)
Promise.resolve().then(() => { console.log('p1'); process.nextTick(() => console.log('tick from p1')); }).then(() => console.log('p2'));
process.nextTick(() => { console.log('n1'); Promise.resolve().then(() => console.log('promise from n1')); });
console.log('sync end');
```

```text
sync end
n1
p1
promise from n1
p2
tick from p1
```

Сначала очередь `nextTick` (`n1`), затем очередь промисов целиком (`p1`, `promise from n1`, `p2`), и только потом `nextTick`, поставленный из промиса (`tick from p1`). Рекурсивный `nextTick` может «заморозить» цикл, не пуская ни ввод-вывод, ни таймеры. В ES-модуле порядок отличается ([модуль 12](../12-promises-and-async-await/expert.ru.md#внутреннее-устройство)).

## Ошибки новичков

- ❌ Ждать, что `setTimeout(fn, 0)` выполнится сразу → ✅ после синхронного кода и микрозадач.
- ❌ Долгий синхронный цикл в обработчике запроса → ✅ порции или рабочий поток.
- ❌ Рассчитывать на точное время `setTimeout` → ✅ считать задержку минимальной.
- ❌ Рекурсивный `process.nextTick` → ✅ `setImmediate`.
- ❌ Использовать число-идентификатор таймера как в браузере → ✅ в Node.js это объект.
- ❌ Забыть `clearInterval` → ✅ процесс не завершится.

## Связанные материалы

- Дальше: [продвинутая теория](advanced.ru.md) — фазы цикла Node.js, рабочие потоки, общая память
- Ранее: [12 · Промисы и async/await](../12-promises-and-async-await/basics.ru.md)

## Источники

- [Node.js: The Node.js Event Loop](https://nodejs.org/en/learn/asynchronous-work/event-loop-timers-and-nexttick)
- [Node.js: Timers](https://nodejs.org/docs/latest-v24.x/api/timers.html)
- [MDN: The event loop](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Event_loop)
- [MDN: Using microtasks](https://developer.mozilla.org/en-US/docs/Web/API/HTML_DOM_API/Microtask_guide)
