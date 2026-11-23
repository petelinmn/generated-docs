---
title: "Цикл событий и параллелизм: продвинутый уровень"
lang: ru
track: javascript
kind: module
doc: advanced
slug: 13-event-loop-and-concurrency
part: 2
tags: [event-loop-phases, poll-phase, worker-threads, message-channel, transferable, shared-array-buffer, atomics, perf-hooks]
versions: "ECMAScript 2025+, Node.js 24.15 LTS"
status: draft
updated: 2026-11-23
---

# Цикл событий и параллелизм: продвинутый уровень

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [overview](overview.ru.md) · [1. Основы](basics.ru.md) · **2. Продвинутый** · [3. Эксперт](expert.ru.md) · [4. Квиз](quiz.ru.md)

Опирается на [базовую теорию](basics.ru.md). Все примеры запускались на Node.js 24.15.

## Фазы цикла событий Node.js

Одна итерация цикла в Node.js проходит фазы (по документации Node.js):

| Фаза | Что выполняется |
|---|---|
| timers | колбэки `setTimeout` и `setInterval`, срок которых наступил |
| pending callbacks | отложенные системные колбэки |
| idle, prepare | внутренние нужды |
| poll | ввод-вывод: получение новых событий, выполнение колбэков ввода-вывода; цикл может ждать здесь |
| check | колбэки `setImmediate` |
| close callbacks | колбэки закрытия (например, `socket.on('close')`) |

Между колбэками каждой фазы выполняются очередь `process.nextTick` и микрозадачи промисов. Из этого следует практический вывод: **внутри колбэка ввода-вывода** `setImmediate` всегда сработает раньше `setTimeout(…, 0)`:

```js
// CommonJS (.cjs)
const fs = require('node:fs');
fs.readFile(__filename, () => {
  setTimeout(() => console.log('io: timeout'), 0);
  setImmediate(() => console.log('io: immediate'));
  process.nextTick(() => console.log('io: nextTick'));
  Promise.resolve().then(() => console.log('io: promise'));
});
```

```text
io: nextTick
io: promise
io: immediate
io: timeout
```

После колбэка идут `nextTick` и промисы, затем фаза check (`setImmediate`), и лишь потом следующая итерация и фаза timers. А вот **в основном модуле** порядок `setTimeout(…, 0)` и `setImmediate` не гарантирован: он зависит от загруженности процесса.

> ⚠️ Проверить: недетерминированность порядка `setTimeout 0` и `setImmediate` в основном модуле взята из документации Node.js и повторно не запускалась.

## Рабочие потоки (worker threads)

Модуль `node:worker_threads` запускает JavaScript в отдельном потоке со своим циклом событий и своим окружением V8. Общение — через сообщения:

```js
import { Worker, isMainThread } from 'node:worker_threads';
const code = `
import { parentPort, workerData, threadId } from 'node:worker_threads';
let s = 0; for (let i = 0; i < workerData.n; i++) s += i;
parentPort.postMessage({ s, threadId });
parentPort.on('message', m => { if (m === 'ping') parentPort.postMessage('pong'); else if (m === 'bye') process.exit(0); });
`;
const w = new Worker(code, { eval: true, workerData: { n: 1e6 } });
const msgs = [];
w.on('message', m => { msgs.push(m); if (m.s !== undefined) w.postMessage('ping'); else if (m === 'pong') w.postMessage('bye'); });
await new Promise(r => w.on('exit', r));
console.log(msgs, isMainThread);
```

```text
[ { s: 499999500000, threadId: 1 }, 'pong' ] true
```

- `new Worker(file или код, { eval: true, workerData })` создаёт поток; `workerData` копируется в него.
- Внутри потока `parentPort.postMessage(...)` отправляет сообщение родителю, `parentPort.on('message', …)` принимает.
- `isMainThread` — `true` в основном потоке; `threadId` у потока-работника — `1`.
- Поток завершается, когда в нём нечего ждать, или по `process.exit()` внутри (событие `exit` у родителя).
- Ошибку внутри потока родитель получает событием `error` (в опыте `worker boom`):

```js
const bad = new Worker(`throw new Error('worker boom')`, { eval: true });
await new Promise(r => bad.on('error', e => { console.log('worker error:', e.message); r(); }));
```

```text
worker error: worker boom
```

Число потоков разумно привязывать к числу ядер: `os.availableParallelism()`.

## Сообщения: копия и передача

Данные в `postMessage` копируются алгоритмом **structured clone** (поддерживаются `Date`, `Map`, `Set`, `ArrayBuffer`, циклические ссылки; не поддерживаются функции). Крупные буферы можно **передать** (transfer) без копирования: у отправителя буфер «обнуляется»:

```js
import { MessageChannel } from 'node:worker_threads';
const ab = new ArrayBuffer(8); const { port1, port2 } = new MessageChannel();
const got = new Promise(r => port2.once('message', r));
port1.postMessage({ ab, d: new Date(0), m: new Map([[1, 2]]) }, [ab]);
const m = await got; console.log(ab.byteLength, m.ab.byteLength, m.d instanceof Date, m.m);
port1.close();
try { new MessageChannel().port1.postMessage({ f() {} }) } catch (e) { console.log(e.name) }
```

```text
0 8 true Map(1) { 1 => 2 }
DataCloneError
```

- После передачи у отправителя `ab.byteLength` — `0`, у получателя — `8`.
- `MessageChannel` создаёт пару портов: сообщение, отправленное в `port1`, приходит в `port2`.
- Функции не копируются: `DataCloneError`.

## SharedArrayBuffer и Atomics

`SharedArrayBuffer` — память, **общая** для потоков. Обычные операции над ней гонятся (race condition); `Atomics` дают атомарные операции:

```js
import { Worker } from 'node:worker_threads';
const sab = new SharedArrayBuffer(4); const ia = new Int32Array(sab);
const wcode = `import { workerData } from 'node:worker_threads'; const a = new Int32Array(workerData); for (let i = 0; i < 1000; i++) Atomics.add(a, 0, 1);`;
const ws = [0, 1, 2, 3].map(() => new Worker(wcode, { eval: true, workerData: sab }));
await Promise.all(ws.map(w => new Promise(r => w.on('exit', r))));
console.log('atomic total', Atomics.load(ia, 0));
```

```text
atomic total 4000
```

Четыре потока по 1000 раз увеличили один счётчик: итог `4000` благодаря `Atomics.add`. С обычным `a[0]++` результат был бы меньше и непредсказуем. Также есть `Atomics.wait`/`notify` для ожидания и пробуждения потоков.

> ⚠️ Проверить: потерю обновлений при обычном `a[0]++` на общей памяти в этом материале не запускали; утверждение следует из модели памяти.

## Измерение времени и задержки цикла

```js
import { performance, monitorEventLoopDelay } from 'node:perf_hooks';
const a = performance.now(); await new Promise(r => setTimeout(r, 20)); console.log('elapsed>=19', performance.now() - a >= 19);
console.log(typeof monitorEventLoopDelay);
```

```text
elapsed>=19 true
function
```

- `performance.now()` — монотонное время с высокой точностью (не зависит от смены системных часов), в отличие от `Date.now()`.
- `monitorEventLoopDelay()` возвращает гистограмму задержек цикла: рост значений — сигнал, что цикл блокируется.

## Лучшие практики

- Не блокируйте цикл: тяжёлое — в рабочий поток или порциями.
- Создавайте рабочие потоки заранее (пул): запуск потока стоит времени и памяти.
- Крупные буферы передавайте, а не копируйте.
- Общую память защищайте `Atomics`; по возможности обменивайтесь сообщениями.
- Замеряйте `performance.now()`, а не `Date.now()`.
- Следите за задержкой цикла в продакшене.

## Частые ошибки

- ❌ Рабочий поток для ввода-вывода → ✅ достаточно асинхронных API.
- ❌ Запуск потока на каждый запрос → ✅ пул.
- ❌ Ждать порядок `setTimeout 0` и `setImmediate` в основном модуле → ✅ полагаться только на порядок внутри ввода-вывода.
- ❌ Общий счётчик без `Atomics` → ✅ `Atomics.add`.
- ❌ Отправлять функции в `postMessage` → ✅ данные.
- ❌ Читать переданный буфер у отправителя → ✅ после transfer он пуст.

## Связанные материалы

- Дальше: [экспертная теория](expert.ru.md)
- Промисы и микрозадачи: [12 · Промисы и async/await](../12-promises-and-async-await/advanced.ru.md)
- Параллелизм в C#: [18 · Параллелизм](../../../csharp/modules/18-concurrency/overview.ru.md)

## Источники

- [Node.js: Event Loop, Timers, and process.nextTick()](https://nodejs.org/en/learn/asynchronous-work/event-loop-timers-and-nexttick)
- [Node.js: Worker threads](https://nodejs.org/docs/latest-v24.x/api/worker_threads.html)
- [MDN: Atomics](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Atomics)
- [MDN: SharedArrayBuffer](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/SharedArrayBuffer)
- [Node.js: perf_hooks](https://nodejs.org/docs/latest-v24.x/api/perf_hooks.html)
