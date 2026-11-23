---
title: "Промисы и async/await: продвинутый уровень"
lang: ru
track: javascript
kind: module
doc: advanced
slug: 12-promises-and-async-await
part: 2
tags: [promise-all, promise-allsettled, promise-race, promise-any, abort-signal, timeout, concurrency-limit, retry, unhandled-rejection]
versions: "ECMAScript 2025+, Node.js 24.15 LTS"
status: draft
updated: 2026-11-23
---

# Промисы и async/await: продвинутый уровень

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [overview](overview.ru.md) · [1. Основы](basics.ru.md) · **2. Продвинутый** · [3. Эксперт](expert.ru.md) · [4. Квиз](quiz.ru.md)

Опирается на [базовую теорию](basics.ru.md). Все примеры запускались на Node.js 24.15; помощник `sleep` — из базового уровня.

## Комбинаторы

| Метод | Выполнен, когда | Отклонён, когда |
|---|---|---|
| `Promise.all` | выполнены все (массив значений) | отклонён любой (первая ошибка) |
| `Promise.allSettled` | всегда после завершения всех | никогда |
| `Promise.race` | завершился первый (успех или ошибка) | первый завершился ошибкой |
| `Promise.any` | выполнен любой первый | отклонены все (`AggregateError`) |

```js
console.log(await Promise.all([1, sleep(5, 'a'), Promise.resolve('b')]));
try { await Promise.all([sleep(5, 'ok'), Promise.reject(new Error('fail')), sleep(50)]) } catch (e) { console.log('all rejected', e.message) }
const r = await Promise.allSettled([1, Promise.reject(new Error('no'))]);
console.log(r.map(x => x.status === 'fulfilled' ? x.value : x.reason.message));
console.log(await Promise.race([sleep(30, 'slow'), sleep(5, 'fast')]));
console.log(await Promise.any([Promise.reject(new Error('a')), sleep(5, 'any ok')]));
try { await Promise.any([Promise.reject(new Error('a')), Promise.reject(new Error('b'))]) } catch (e) { console.log(e.constructor.name, e.errors.map(x => x.message), e.message) }
```

```text
[ 1, 'a', 'b' ]
all rejected fail
[ 1, 'no' ]
fast
any ok
AggregateError [ 'a', 'b' ] All promises were rejected
```

- Элементы могут быть не промисами: они превращаются в выполненные.
- Элементы `allSettled` — объекты `{ status: 'fulfilled', value }` или `{ status: 'rejected', reason }`.
- Для пустых входных данных: `all([])` и `allSettled([])` дают `[]`; `any([])` отклоняется `AggregateError` с пустым `errors`.
- Проигравшие участники не отменяются: `race` оставил медленный промис работать (в опыте `loser still ran true`), а после отклонения `all` другая операция всё равно завершилась (`other finished 1`).

## Вспомогательные методы

```js
const r = Promise.withResolvers(); setTimeout(r.resolve, 1, 'wr'); console.log(await r.promise);
console.log(await Promise.try(() => { throw new Error('sync') }).catch(e => 'try ' + e.message));
```

```text
wr
try sync
```

- `Promise.withResolvers()` возвращает `{ promise, resolve, reject }` — удобно, когда завершение вызывается из другого места.
- `Promise.try(fn)` запускает `fn` и оборачивает синхронное исключение в отклонённый промис. Оба метода доступны в Node.js 24.

## return и return await

Внутри `try/catch` `return promise` не перехватит ошибку промиса, а `return await promise` — перехватит:

```js
async function bad() { throw new Error('E') }
const r1 = await (async () => { try { return bad() } catch { return 'caught' } })().catch(e => 'escaped ' + e.message);
const r2 = await (async () => { try { return await bad() } catch { return 'caught' } })();
console.log(r1, r2);
```

```text
escaped E caught
```

Без `await` ошибка «уходит» мимо `catch` вызывающей функции. Вне `try/catch` разницы нет.

## Последовательно и параллельно

Разные способы перебрать массив с `async`-функцией:

```js
const out = []; [1, 2, 3].forEach(async x => { await sleep(5); out.push(x) }); console.log('after forEach', out.length);
await sleep(20); console.log(out);
const out2 = []; for (const x of [1, 2, 3]) { await sleep(1); out2.push(x) } console.log(out2);
console.log(await Promise.all([1, 2, 3].map(async x => { await sleep(4 - x); return x * 2 })));
```

```text
after forEach 0
[ 1, 2, 3 ]
[ 1, 2, 3 ]
[ 2, 4, 6 ]
```

| Способ | Поведение |
|---|---|
| `forEach(async …)` | не ждёт: `out.length` сразу `0`; ошибки не попадают в `try/catch` снаружи |
| `for … of` + `await` | строго по очереди |
| `Promise.all(arr.map(async …))` | параллельно, порядок результатов сохраняется независимо от скорости |

Сохраните промисы **до** `await`, чтобы операции шли вместе: в опыте функции `a` и `b` стартовали подряд (`a start`, `b start`), а закончились в порядке скорости (`b end`, `a end`).

## Таймаут и отмена

**Таймаут** через `Promise.race`:

```js
const timeout = (p, ms) => Promise.race([p, new Promise((_, rej) => setTimeout(rej, ms, new Error('timeout')))]);
try { await timeout(sleep(50), 5) } catch (e) { console.log(e.message) }
```

```text
timeout
```

`race` не останавливает исходную операцию. Для настоящей отмены операция должна принимать **`AbortSignal`**:

```js
const { setTimeout: st } = await import('node:timers/promises');
console.log(await st(5, 'timers/promises'));
try { await st(50, 'v', { signal: AbortSignal.timeout(5) }) } catch (e) { console.log(e.name, e.code, e.cause?.name) }
```

```text
timers/promises
AbortError ABORT_ERR TimeoutError
```

- `node:timers/promises` даёт промисные `setTimeout`, `setImmediate`, `setInterval`.
- `AbortSignal.timeout(ms)` — сигнал, который сработает через `ms`; причина — `TimeoutError` (`The operation was aborted due to timeout`).
- `new AbortController()` и `controller.abort()` — ручная отмена; причина по умолчанию — `AbortError` (`This operation was aborted`), или любое значение, переданное в `abort('custom')`.
- `signal.throwIfAborted()` бросает причину, если сигнал уже сработал; `AbortSignal.any([...])` объединяет сигналы.
- Таймер `AbortSignal.timeout` не удерживает процесс: если ждёт только он, Node.js завершится (`Warning: Detected unsettled top-level await`). Операция, которой передан сигнал, ведёт себя нормально.

> ⚠️ Проверить: поддержка `AbortSignal` в `fetch`, потоках и файловых API Node.js в этом материале не запускалась.

## Ограничение параллелизма

`Promise.all` по тысяче элементов запустит тысячу операций сразу. Пул ограничивает число одновременных:

```js
async function pool(items, n, fn) { const res = []; let i = 0; const run = async () => { while (i < items.length) { const k = i++; res[k] = await fn(items[k]); } }; await Promise.all(Array.from({ length: n }, run)); return res; }
let active = 0, max = 0;
console.log(await pool([1, 2, 3, 4, 5, 6], 2, async x => { active++; max = Math.max(max, active); await sleep(5); active--; return x * x }), 'max', max);
```

```text
[ 1, 4, 9, 16, 25, 36 ] max 2
```

Одновременно работало не больше двух; порядок результатов сохранён.

## Повтор

```js
let tries = 0; const flaky = async () => { if (++tries < 3) throw new Error('fail' + tries); return 'ok after ' + tries };
async function retry(fn, n) { for (let i = 0; ; i++) { try { return await fn() } catch (e) { if (i >= n) throw e } } }
console.log(await retry(flaky, 5));
```

```text
ok after 3
```

Здесь `return await` обязателен: иначе ошибка ушла бы мимо `catch`. В реальном коде добавляют паузу между попытками (экспоненциальную), предел и проверку типа ошибки.

## Необработанные отклонения

Если отклонённый промис не получил обработчик к концу очереди микрозадач, Node.js сообщает об этом:

- по умолчанию процесс завершается с ошибкой и кодом `1` (в опыте для `Promise.reject(new Error('unhandled default'))` вывелась ошибка, код выхода `1`);
- событие `process.on('unhandledRejection', (reason, promise) => …)` позволяет перехватить, и тогда процесс не падает;
- если обработчик добавлен позже, срабатывает `rejectionHandled` (в опыте для `late`).

```js
process.on('unhandledRejection', (reason, promise) => { console.log('unhandled:', reason.message, promise instanceof Promise); });
Promise.reject(new Error('nobody catches'));
```

```text
unhandled: nobody catches true
```

Обработчик `unhandledRejection` — «страховка» для логирования, а не замена `catch`.

## Лучшие практики

- Каждая цепочка заканчивается `catch`, каждая `async`-функция — либо ловит ошибки, либо передаёт их вызывающему.
- Независимые операции запускайте через `Promise.all`; ограничивайте число одновременных через пул.
- Нужны все исходы — `allSettled`; первый успех — `any`.
- Операциям давайте `AbortSignal`, а не только `Promise.race`.
- Внутри `try/catch` пишите `return await`.
- Не используйте `forEach` с `async`.

## Частые ошибки

- ❌ `forEach(async …)` и ожидание результата → ✅ `for … of` или `Promise.all(map(…))`.
- ❌ `return promise` внутри `try/catch` → ✅ `return await promise`.
- ❌ `Promise.all` по огромному массиву → ✅ пул.
- ❌ Считать, что `race` останавливает проигравшего → ✅ отмена через `AbortSignal`.
- ❌ `await` по очереди для независимых запросов → ✅ `Promise.all`.
- ❌ Положиться на `unhandledRejection` вместо обработки → ✅ `catch` в цепочке.

## Связанные материалы

- Дальше: [экспертная теория](expert.ru.md)
- Асинхронные итераторы: [11 · Итераторы и генераторы](../11-iterators-and-generators/advanced.ru.md#асинхронные-итераторы-и-генераторы)
- Ошибки и отладка: модуль 14 дорожной карты ([трек](../../overview.ru.md))

## Источники

- [MDN: Promise — статические методы](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Promise)
- [MDN: AbortSignal](https://developer.mozilla.org/en-US/docs/Web/API/AbortSignal)
- [Node.js: Timers Promises API](https://nodejs.org/docs/latest-v24.x/api/timers.html#timers-promises-api)
- [Node.js: Event 'unhandledRejection'](https://nodejs.org/docs/latest-v24.x/api/process.html#event-unhandledrejection)
- [Node.js: `--unhandled-rejections`](https://nodejs.org/docs/latest-v24.x/api/cli.html#--unhandled-rejectionsmode)
