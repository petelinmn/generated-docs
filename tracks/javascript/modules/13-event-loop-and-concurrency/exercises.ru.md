---
title: "Упражнения: цикл событий и параллелизм"
lang: ru
track: javascript
kind: module
doc: exercises
slug: 13-event-loop-and-concurrency
part: 2
tags: [event-loop, set-immediate, worker-threads, atomics, timers]
status: draft
updated: 2026-11-23
---

# Упражнения: цикл событий и параллелизм

🌐 **Русский** · [English](exercises.en.md)

> Решайте самостоятельно, подсказки и решения под спойлерами. Код запускается в файле с `package.json` (`{"type": "module"}`), если не сказано иное.

## 1. 🟢 Порядок вывода

**Задача:** предскажите порядок вывода в файле CommonJS (`.cjs`), затем проверьте запуском.

```js
console.log('A');
process.nextTick(() => console.log('B'));
Promise.resolve().then(() => console.log('C'));
queueMicrotask(() => console.log('D'));
setTimeout(() => console.log('E'), 0);
console.log('F');
```

**Ожидаемый результат:** `A`, `F`, `B`, `C`, `D`, `E`.

<details><summary>Подсказка</summary>

Сначала синхронный код, затем очередь `nextTick`, затем микрозадачи по порядку, затем таймер.

</details>

<details><summary>Решение</summary>

```text
A
F
B
C
D
E
```

В ES-модуле `B` оказался бы после `C` и `D`.

</details>

## 2. 🟢 Таймер, который не держит процесс

**Задача:** создайте `setInterval`, который не должен мешать завершению процесса, и убедитесь, что программа завершается, напечатав только `end`.

**Ожидаемый результат:** `end`.

<details><summary>Подсказка</summary>

`setInterval(…).unref()`.

</details>

<details><summary>Решение</summary>

```js
setInterval(() => console.log('never printed'), 1000).unref();
console.log('end');
```

```text
end
```

</details>

## 3. 🟡 Сумма порциями

**Задача:** напишите `sumChunked(n, chunk)`, которая суммирует числа от `0` до `n - 1` порциями, отдавая управление циклу через `setImmediate`. Проверьте, что параллельный таймер `setInterval(…, 1)` успел сработать за время вычисления.

**Ожидаемый результат:** `12499997500000 true` для `n = 5e6`.

<details><summary>Подсказка</summary>

Внутри промиса — функция `step`, которая обрабатывает `chunk` чисел и вызывает `setImmediate(step)`, пока не дойдёт до `n`.

</details>

<details><summary>Решение</summary>

```js
const sumChunked = (n, chunk = 1e5) => new Promise(res => { let i = 0, s = 0; (function step() { const end = Math.min(i + chunk, n); for (; i < end; i++) s += i; if (i < n) setImmediate(step); else res(s); })(); });
let ticks = 0; const iv = setInterval(() => ticks++, 1);
const total = await sumChunked(5e6); clearInterval(iv);
console.log(total, ticks > 0);
```

```text
12499997500000 true
```

Значение `ticks` зависит от машины, поэтому проверяется только факт срабатывания.

</details>

## 4. 🟡 Вычисление в рабочем потоке

**Задача:** напишите `runWorker(n)`, которая запускает рабочий поток, считающий сумму `0 … n - 1`, и возвращает промис с результатом. Запустите три вычисления параллельно.

**Ожидаемый результат:** `499999500000` для `n = 1e6`, и `45,4950` для `10` и `100`.

<details><summary>Подсказка</summary>

`new Worker(код, { eval: true, workerData: n })`; результат — `parentPort.postMessage`; в родителе `w.once('message', res)` и `w.once('error', rej)`.

</details>

<details><summary>Решение</summary>

```js
import { Worker } from 'node:worker_threads';
const runWorker = (n) => new Promise((res, rej) => {
  const w = new Worker(`import { parentPort, workerData } from 'node:worker_threads'; let s = 0; for (let i = 0; i < workerData; i++) s += i; parentPort.postMessage(s);`, { eval: true, workerData: n });
  w.once('message', res); w.once('error', rej);
});
console.log(await runWorker(1e6), (await Promise.all([runWorker(10), runWorker(100)])).join(','));
```

```text
499999500000 45,4950
```

</details>

## 5. 🔴 Общий счётчик с `Atomics`

**Задача:** три рабочих потока по 500 раз увеличивают одно число в `SharedArrayBuffer`. Получите итог без потерь.

**Ожидаемый результат:** `1500`.

<details><summary>Подсказка</summary>

`Atomics.add(a, 0, 1)` внутри потока; общий буфер передаётся через `workerData`; итог — `Atomics.load`.

</details>

<details><summary>Решение</summary>

```js
import { Worker } from 'node:worker_threads';
const sab = new SharedArrayBuffer(4); const ia = new Int32Array(sab);
const code = `import { workerData } from 'node:worker_threads'; const a = new Int32Array(workerData); for (let i = 0; i < 500; i++) Atomics.add(a, 0, 1);`;
await Promise.all([1, 2, 3].map(() => new Promise(r => new Worker(code, { eval: true, workerData: sab }).on('exit', r))));
console.log(Atomics.load(ia, 0));
```

```text
1500
```

</details>
