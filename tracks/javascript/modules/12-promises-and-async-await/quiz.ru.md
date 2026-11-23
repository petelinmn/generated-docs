---
title: "Квиз: промисы и async/await"
lang: ru
track: javascript
kind: module
doc: quiz
slug: 12-promises-and-async-await
part: 2
tags: [promise, async-await, promise-all, abort-signal, microtask]
status: draft
updated: 2026-11-23
---

# Квиз: промисы и async/await

🌐 **Русский** · [English](quiz.en.md)

> Сначала ответьте сами, потом откройте ответ. Метка показывает уровень вопроса: 🟢 [базовая](basics.ru.md) · 🟡 [продвинутая](advanced.ru.md) · 🔴 [экспертная](expert.ru.md).

## 1. 🟢 Что выведет код?

```js
const p = new Promise((res, rej) => { res(1); res(2); rej(new Error('x')); });
p.then(v => console.log(v));
```

- A) `1`
- B) `2`
- C) `Error: x`
- D) Ничего

<details><summary>Ответ</summary>

**A.** Промис меняет состояние один раз; повторные `res` и `rej` игнорируются.
→ [Базовая теория](basics.ru.md#состояния-промиса)

</details>

## 2. 🟢 В каком порядке выведутся буквы?

```js
console.log('A');
setTimeout(() => console.log('D'), 0);
Promise.resolve().then(() => console.log('C'));
console.log('B');
```

- A) `A B C D`
- B) `A C B D`
- C) `A D C B`
- D) `A B D C`

<details><summary>Ответ</summary>

**A.** Сначала синхронный код (`A`, `B`), затем микрозадача `then` (`C`), потом таймер (`D`).
→ [Внутреннее устройство](expert.ru.md#внутреннее-устройство)

</details>

## 3. 🟢 Что выведет код?

```js
Promise.resolve(1)
  .then(v => v + 1)
  .then(v => { throw new Error('boom ' + v) })
  .then(() => console.log('skipped'))
  .catch(e => { console.log('caught', e.message); return 'rec' })
  .then(v => console.log('after', v));
```

- A) `caught boom 2`, `after rec`
- B) `skipped`, `caught boom 2`
- C) `caught boom 2`, `after undefined`
- D) Только `caught boom 2`

<details><summary>Ответ</summary>

**A.** Ошибка пропускает `then` до `catch`; `catch` вернул `'rec'`, и цепочка продолжается.
→ [Базовая теория](basics.ru.md#then-catch-finally)

</details>

## 4. 🟢 Что возвращает `async`-функция, если внутри `return 1`?

- A) `1`
- B) Промис, выполненный значением `1`
- C) `undefined`
- D) Зависит от наличия `await`

<details><summary>Ответ</summary>

**B.** `async`-функция всегда возвращает промис.
→ [Базовая теория](basics.ru.md#async-и-await)

</details>

## 5. 🟡 Что выведет код?

```js
async function bad() { throw new Error('E') }
const r = await (async () => { try { return bad() } catch { return 'caught' } })().catch(e => 'escaped ' + e.message);
console.log(r);
```

- A) `caught`
- B) `escaped E`
- C) `undefined`
- D) `TypeError`

<details><summary>Ответ</summary>

**B.** `return bad()` без `await` возвращает отклонённый промис мимо `catch`. С `return await bad()` было бы `caught`.
→ [Продвинутая теория](advanced.ru.md#return-и-return-await)

</details>

## 6. 🟡 Что вернёт `Promise.all`, если один из промисов отклонён?

- A) Массив с результатами остальных
- B) Отклонённый промис с первой ошибкой
- C) Массив `{status, ...}`
- D) Он ждёт все и потом бросает все ошибки

<details><summary>Ответ</summary>

**B.** `all` отклоняется при первой ошибке (но остальные операции продолжают выполняться). Результаты всех даёт `allSettled`.
→ [Продвинутая теория](advanced.ru.md#комбинаторы)

</details>

## 7. 🟡 Что выведет код?

```js
const out = [];
[1, 2, 3].forEach(async x => { await sleep(5); out.push(x) });
console.log(out.length);
```

- A) `3`
- B) `0`
- C) `1`
- D) `undefined`

<details><summary>Ответ</summary>

**B.** `forEach` не ждёт возвращаемые промисы. Для ожидания используйте `for … of` или `Promise.all(arr.map(…))`.
→ [Продвинутая теория](advanced.ru.md#последовательно-и-параллельно)

</details>

## 8. 🟡 Что вернёт `Promise.any([Promise.reject(new Error('a')), Promise.reject(new Error('b'))])`?

- A) Отклонение `Error: a`
- B) Отклонение `AggregateError` с `errors` из обеих причин
- C) Выполнение `undefined`
- D) Выполнение массивом ошибок

<details><summary>Ответ</summary>

**B.** Если отклонены все, `any` отклоняется `AggregateError` (`All promises were rejected`).
→ [Продвинутая теория](advanced.ru.md#комбинаторы)

</details>

## 9. 🟡 Выберите все верные утверждения

- [ ] A) `Promise.race` останавливает проигравшие операции
- [ ] B) `AbortSignal.timeout(ms)` создаёт сигнал, срабатывающий через `ms`
- [ ] C) `Promise.allSettled` никогда не отклоняется
- [ ] D) `finally` получает результат промиса аргументом

<details><summary>Ответ</summary>

**B, C.** A неверно: `race` ничего не отменяет; D неверно: `finally` не получает аргументов.
→ [Продвинутая теория](advanced.ru.md#таймаут-и-отмена)

</details>

## 10. 🔴 Найдите ошибку

> «`Promise.resolve(p)` всегда создаёт новый промис».

<details><summary>Ответ</summary>

Если `p` — промис, возвращается тот же объект: `Promise.resolve(p) === p`. А `new Promise(r => r(p))` создаёт новый.
→ [Подвох 5](expert.ru.md#5-promiseresolve-не-всегда-создаёт-новый-объект)

</details>

## 11. 🔴 В каком порядке выведется текст в CommonJS-файле?

```js
console.log('1 sync');
setTimeout(() => console.log('5 timeout'), 0);
Promise.resolve().then(() => console.log('3 micro'));
process.nextTick(() => console.log('2 nextTick'));
```

- A) `1`, `3`, `2`, `5`
- B) `1`, `2`, `3`, `5`
- C) `1`, `5`, `2`, `3`
- D) `1`, `2`, `5`, `3`

<details><summary>Ответ</summary>

**B.** В CommonJS `nextTick` выполняется раньше очереди промисов, а таймер — после микрозадач. В ES-модуле `nextTick` оказался бы после промисов.
→ [Подвох 3](expert.ru.md#3-nexttick-и-промисы)

</details>

## 12. 🔴 Что произойдёт при необработанном отклонении в Node.js 24 без обработчика `unhandledRejection`?

- A) Ничего, ошибка игнорируется
- B) Предупреждение и продолжение работы
- C) Ошибка выводится и процесс завершается с кодом `1`
- D) Промис автоматически повторяется

<details><summary>Ответ</summary>

**C.** В опыте процесс завершился с ошибкой и кодом `1`.
→ [Продвинутая теория](advanced.ru.md#необработанные-отклонения)

</details>

## 13. 🔴 Открытый вопрос

Объясните, как `async`/`await` связан с промисами и очередью микрозадач, и почему `await` в цикле последователен, а `Promise.all` параллелен.

<details><summary>Ключевые мысли хорошего ответа</summary>

- `async`-функция возвращает промис; `await` приводит значение к промису и подписывает продолжение функции на его результат (микрозадача).
- Функция приостанавливается, управление возвращается вызывающему; после синхронного кода очередь микрозадач выполняется до таймеров.
- `await` в цикле начинает следующую операцию только после завершения предыдущей; `Promise.all` запускает все операции сразу и ждёт их вместе.
- Для ограничения параллелизма нужен пул, для отмены — `AbortSignal`; `race`/`all` ничего не отменяют.

</details>

→ [Экспертная теория](expert.ru.md#внутреннее-устройство)
