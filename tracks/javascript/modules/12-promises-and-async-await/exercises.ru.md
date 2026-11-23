---
title: "Упражнения: промисы и async/await"
lang: ru
track: javascript
kind: module
doc: exercises
slug: 12-promises-and-async-await
part: 2
tags: [promise, async-await, promisify, promise-allsettled, abort-signal, concurrency-limit, retry]
status: draft
updated: 2026-11-23
---

# Упражнения: промисы и async/await

🌐 **Русский** · [English](exercises.en.md)

> Решайте самостоятельно, подсказки и решения под спойлерами. Код запускается в файле с `package.json` (`{"type": "module"}`); везде используется `const sleep = (ms, v) => new Promise(r => setTimeout(r, ms, v));`.

## 1. 🟢 Порядок вывода

**Задача:** предскажите порядок вывода, затем проверьте запуском.

```js
console.log('A');
setTimeout(() => console.log('D'), 0);
Promise.resolve().then(() => console.log('C'));
(async () => { console.log('B'); await null; console.log('C2'); })();
console.log('A2');
```

**Ожидаемый результат:** `A`, `B`, `A2`, `C`, `C2`, `D`.

<details><summary>Подсказка</summary>

Тело `async`-функции выполняется синхронно до первого `await`; всё после `await` — микрозадача, таймер — после микрозадач.

</details>

<details><summary>Решение</summary>

```text
A
B
A2
C
C2
D
```

`A`, `B` и `A2` — синхронный код (`B` печатается внутри `async`-функции до `await`). Затем очередь микрозадач: `C` (поставлен раньше), `C2` (продолжение после `await null`). Таймер срабатывает последним.

</details>

## 2. 🟢 Обёртка над колбэком

**Задача:** напишите `promisify(fn)`, превращающую функцию в стиле `fn(...args, cb)` (колбэк `(err, value)`) в функцию, возвращающую промис. Проверьте успех и ошибку.

**Ожидаемый результат:** `data`, `cb failed`.

<details><summary>Подсказка</summary>

`new Promise((res, rej) => fn(...a, (err, v) => err ? rej(err) : res(v)))`.

</details>

<details><summary>Решение</summary>

```js
function readLater(cb) { setTimeout(() => cb(null, 'data'), 1); }
function failLater(cb) { setTimeout(() => cb(new Error('cb failed')), 1); }
const promisify = fn => (...a) => new Promise((res, rej) => fn(...a, (err, v) => err ? rej(err) : res(v)));
console.log(await promisify(readLater)());
try { await promisify(failLater)() } catch (e) { console.log(e.message) }
```

```text
data
cb failed
```

В Node.js для этого есть готовый `util.promisify`.

</details>

## 3. 🟡 Итог по `allSettled`

**Задача:** запустите три операции, одна из них отклоняется. Выведите массив успешных значений и массив сообщений об ошибках.

**Ожидаемый результат:** `[ 'a', 'c' ] [ 'bad' ]`.

<details><summary>Подсказка</summary>

`Promise.allSettled` и `filter` по `status`.

</details>

<details><summary>Решение</summary>

```js
const jobs = [sleep(2, 'a'), Promise.reject(new Error('bad')), sleep(1, 'c')];
const res = await Promise.allSettled(jobs);
console.log(res.filter(r => r.status === 'fulfilled').map(r => r.value), res.filter(r => r.status === 'rejected').map(r => r.reason.message));
```

```text
[ 'a', 'c' ] [ 'bad' ]
```

</details>

## 4. 🟡 Таймаут с отменой

**Задача:** напишите `withTimeout(fn, ms)`, которая вызывает `fn(signal)` и отменяет операцию через `AbortController` по истечении `ms`. Операция `work(ms)(signal)` слушает сигнал. Проверьте быструю и медленную операции.

**Ожидаемый результат:** `done`, `timeout`.

<details><summary>Подсказка</summary>

`ac.abort(new Error('timeout'))`; в операции — `signal.addEventListener('abort', …)`; в конце `clearTimeout`.

</details>

<details><summary>Решение</summary>

```js
function withTimeout(fn, ms) { const ac = new AbortController(); const t = setTimeout(() => ac.abort(new Error('timeout')), ms); return fn(ac.signal).finally(() => clearTimeout(t)); }
const work = (ms) => (signal) => new Promise((res, rej) => { const t = setTimeout(res, ms, 'done'); signal.addEventListener('abort', () => { clearTimeout(t); rej(signal.reason); }); });
console.log(await withTimeout(work(5), 50));
try { await withTimeout(work(50), 5) } catch (e) { console.log(e.message) }
```

```text
done
timeout
```

</details>

## 5. 🔴 Пул с повтором

**Задача:** напишите `pool(items, n, fn)` (не больше `n` одновременных операций, порядок результатов сохраняется) и `retry(fn, times)`. Обработайте `[1, 2, 3, 4]`: чётные числа должны падать на первой попытке и проходить на второй. Покажите число попыток.

**Ожидаемый результат:** `[ 10, 20, 30, 40 ] { '1': 1, '2': 2, '3': 1, '4': 2 }`.

<details><summary>Подсказка</summary>

Пул — `n` «воркеров» с общим индексом; в `retry` — `return await` внутри `try`.

</details>

<details><summary>Решение</summary>

```js
async function pool(items, n, fn) { const out = []; let i = 0; await Promise.all(Array.from({ length: n }, async () => { while (i < items.length) { const k = i++; out[k] = await fn(items[k]); } })); return out; }
const attempts = {};
const flaky = async x => { attempts[x] = (attempts[x] ?? 0) + 1; if (x % 2 === 0 && attempts[x] < 2) throw new Error('retry ' + x); return x * 10; };
async function retry(fn, times) { for (let i = 0; ; i++) { try { return await fn() } catch (e) { if (i >= times) throw e } } }
console.log(await pool([1, 2, 3, 4], 2, x => retry(() => flaky(x), 2)), attempts);
```

```text
[ 10, 20, 30, 40 ] { '1': 1, '2': 2, '3': 1, '4': 2 }
```

</details>
