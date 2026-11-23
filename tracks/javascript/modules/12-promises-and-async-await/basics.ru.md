---
title: "Промисы и async/await: основы"
lang: ru
track: javascript
kind: module
doc: basics
slug: 12-promises-and-async-await
part: 2
tags: [promise, then, catch, finally, async-await, error-handling, thenable]
versions: "ECMAScript 2025+, Node.js 24.15 LTS"
status: draft
updated: 2026-11-23
---

# Промисы и async/await: основы

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [overview](overview.ru.md) · **1. Основы** · [2. Продвинутый](advanced.ru.md) · [3. Эксперт](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [04 · Функции](../04-functions/overview.ru.md), [11 · Итераторы и генераторы](../11-iterators-and-generators/overview.ru.md) · **Время:** ~3 ч

Все примеры запускались на Node.js 24.15; файлы лежат рядом с `package.json` (`{"type": "module"}`). Везде используется помощник:

```js
const sleep = (ms, v) => new Promise(r => setTimeout(r, ms, v));
```

## Зачем это нужно

Чтение файла, сетевой запрос, таймер выполняются «в фоне», а результат приходит позже. Промис — единый способ описать такой результат и скомбинировать несколько операций, не вкладывая обратные вызовы друг в друга.

## Состояния промиса

Промис находится в одном из трёх состояний: ожидание (pending), выполнен (fulfilled), отклонён (rejected). Переход происходит один раз:

```js
const p = new Promise((res, rej) => { console.log('executor sync'); res(1); res(2); rej(new Error('x')); });
console.log(p);
p.then(v => console.log('then', v));
console.log('after then registered');
```

```text
executor sync
Promise { 1 }
after then registered
then 1
```

- Функция-исполнитель (executor) выполняется **синхронно**, сразу при создании.
- Повторные `res(2)` и `rej(...)` игнорируются: первое значение — `1`.
- Обработчик `then` вызван **после** синхронного кода (`after then registered` напечатано раньше `then 1`), даже если промис уже выполнен.
- Исключение в исполнителе превращается в отклонение: `new Promise(() => { throw new Error('executor throws') }).catch(...)` перехватывает его.

## then, catch, finally

```js
Promise.resolve(1)
  .then(v => v + 1)
  .then(v => { throw new Error('boom ' + v) })
  .then(() => console.log('skipped'))
  .catch(e => { console.log('caught', e.message); return 'rec' })
  .then(v => console.log('after catch', v))
  .finally(() => console.log('finally'));
```

```text
caught boom 2
after catch rec
finally
```

- `then` возвращает **новый** промис: значение, которое вернул обработчик, становится его результатом.
- Исключение в обработчике отклоняет следующий промис; цепочка пропускает `then` без обработчика ошибок (`skipped` не напечатано) до ближайшего `catch`.
- После `catch` цепочка продолжается: `catch` вернул `'rec'`, и следующий `then` получил его.
- `finally` выполняется в любом случае и не получает значение; результат цепочки он не меняет.

Если обработчик возвращает промис, цепочка ждёт его (результат «выпрямляется»):

```js
Promise.resolve().then(() => Promise.resolve('flat')).then(v => console.log(v));
```

```text
flat
```

## async и await

`async function` всегда возвращает промис. Внутри `await` приостанавливает функцию, пока промис не завершится, и возвращает его значение (или бросает ошибку):

```js
async function f() { return 1 }
console.log(f() instanceof Promise, Object.prototype.toString.call(f));
async function bad() { throw new Error('async boom') }
async function useTry() { try { await bad() } catch (e) { return 'handled ' + e.message } finally { console.log('fin') } }
useTry().then(console.log);
```

```text
true [object AsyncFunction]
fin
handled async boom
```

- Значение `return` оборачивается в промис; `throw` внутри — отклонённый промис.
- Ошибки ловятся обычным `try/catch/finally`.
- `await` можно применить к любому значению: не-промис возвращается как есть, а thenable (объект с методом `then`) ждётся (`await 5` → `5`).
- `await` допустим только внутри `async`-функции или на верхнем уровне ES-модуля ([модуль 09](../09-modules/basics.ru.md#await-на-верхнем-уровне)).

## Последовательно и одновременно

Два `await` подряд выполняются **по очереди**; запустить операции одновременно можно, начав обе до первого `await`:

```js
const t = async (ms, tag) => { await sleep(ms); return tag };
let t0 = Date.now(); await t(30, 'a'); await t(30, 'b'); const seq = Date.now() - t0;
t0 = Date.now(); await Promise.all([t(30, 'a'), t(30, 'b')]); const par = Date.now() - t0;
console.log('seq>=55', seq >= 55, 'par<55', par < 55);
```

```text
seq>=55 true par<55 true
```

Последовательно — около 60 мс, одновременно — около 30 мс (границы в выводе нестрогие, точное время зависит от машины). Подробнее — [продвинутая теория](advanced.ru.md#последовательно-и-параллельно).

## Ошибки новичков

- ❌ Забыть `await` и работать с промисом как со значением → ✅ `await` или `.then`.
- ❌ Оставить промис без `catch` → ✅ обрабатывать ошибки (иначе процесс завершится).
- ❌ Вложенные `then` («лесенка») → ✅ плоская цепочка или `async`/`await`.
- ❌ `await` по очереди для независимых операций → ✅ `Promise.all`.
- ❌ Считать, что `finally` меняет результат → ✅ он только выполняет действие.
- ❌ Рассчитывать, что код в `then` выполнится сразу → ✅ он выполняется после синхронного кода.

## Связанные материалы

- Дальше: [продвинутая теория](advanced.ru.md) — комбинаторы, таймаут, отмена, пул, повтор
- Ранее: [11 · Итераторы и генераторы](../11-iterators-and-generators/advanced.ru.md#асинхронные-итераторы-и-генераторы)

## Источники

- [MDN: Using promises](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Guide/Using_promises)
- [MDN: Promise](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Promise)
- [MDN: async function](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Statements/async_function)
- [MDN: await](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Operators/await)
