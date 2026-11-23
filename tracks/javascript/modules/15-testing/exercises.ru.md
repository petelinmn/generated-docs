---
title: "Упражнения: тестирование"
lang: ru
track: javascript
kind: module
doc: exercises
slug: 15-testing
part: 2
tags: [node-test, assert, mock, fake-timers, hooks]
status: draft
updated: 2026-11-23
---

# Упражнения: тестирование

🌐 **Русский** · [English](exercises.en.md)

> Решайте самостоятельно, подсказки и решения под спойлерами. Рабочая папка с `package.json` (`{"type": "module"}`); код под тестом лежит в `lib/util.js`, тесты — в `test/ex.test.js`. Запуск: `node --test`.

Код под тестом (`lib/util.js`):

```js
export function debounce(fn, ms) { let t; return (...a) => { clearTimeout(t); t = setTimeout(() => fn(...a), ms); }; }
export async function retry(fn, times) { for (let i = 0; ; i++) { try { return await fn() } catch (e) { if (i >= times) throw e } } }
export function slugify(s) { return s.trim().toLowerCase().replace(/\s+/g, '-'); }
```

Все решения вместе (`test/ex.test.js`) дали 9 пройденных тестов.

## 1. 🟢 Тесты для `slugify`

**Задача:** напишите три теста: слова приводятся к нижнему регистру и соединяются дефисом (`'  Hello Big World '` → `'hello-big-world'`), одно слово остаётся словом, а `null` приводит к `TypeError`.

**Ожидаемый результат:** три зелёных теста набора `slugify`.

<details><summary>Подсказка</summary>

`assert.equal` для значений и `assert.throws(() => slugify(null), TypeError)` для ошибки.

</details>

<details><summary>Решение</summary>

```js
import { test, describe, it, beforeEach, mock } from 'node:test';
import assert from 'node:assert/strict';
import { slugify, debounce, retry } from '../lib/util.js';

describe('slugify', () => {
  it('lowercases and joins words', () => { assert.equal(slugify('  Hello Big World '), 'hello-big-world'); });
  it('keeps a single word', () => { assert.equal(slugify('Word'), 'word'); });
  it('throws for non-strings', () => { assert.throws(() => slugify(null), TypeError); });
});
```

```text
▶ slugify
  ✔ lowercases and joins words
  ✔ keeps a single word
  ✔ throws for non-strings
✔ slugify
```

</details>

## 2. 🟢 Хук `beforeEach`

**Задача:** в наборе есть массив `items`. Первый тест добавляет в него элемент, второй проверяет, что массив снова пуст. Добейтесь этого хуком, а не порядком тестов.

**Ожидаемый результат:** оба теста проходят.

<details><summary>Подсказка</summary>

`beforeEach(() => { items = []; })`.

</details>

<details><summary>Решение</summary>

```js
describe('beforeEach resets state', () => {
  let items;
  beforeEach(() => { items = []; });
  it('first test sees an empty array', () => { items.push(1); assert.deepEqual(items, [1]); });
  it('second test sees an empty array again', () => { assert.deepEqual(items, []); });
});
```

```text
▶ beforeEach resets state
  ✔ first test sees an empty array
  ✔ second test sees an empty array again
✔ beforeEach resets state
```

</details>

## 3. 🟡 Мок и повторные попытки

**Задача:** проверьте `retry` с помощью `mock.fn`: функция дважды бросает ошибку, на третий раз возвращает `'ok'` (число вызовов — `3`). Второй тест: функция всегда падает; при `times = 2` должно быть три вызова и отклонение с исходным сообщением.

**Ожидаемый результат:** оба теста проходят.

<details><summary>Подсказка</summary>

`f.mock.mockImplementationOnce(impl, номерВызова)`; для отклонения — `await assert.rejects(…)`.

</details>

<details><summary>Решение</summary>

```js
test('retry succeeds on the third attempt', async () => {
  const f = mock.fn(async () => 'ok');
  f.mock.mockImplementationOnce(async () => { throw new Error('fail 1') }, 0);
  f.mock.mockImplementationOnce(async () => { throw new Error('fail 2') }, 1);
  assert.equal(await retry(f, 5), 'ok');
  assert.equal(f.mock.callCount(), 3);
});
test('retry gives up', async () => {
  const f = mock.fn(async () => { throw new Error('always') });
  await assert.rejects(retry(f, 2), { message: 'always' });
  assert.equal(f.mock.callCount(), 3);
});
```

```text
✔ retry succeeds on the third attempt
✔ retry gives up
```

</details>

## 4. 🟡 `debounce` с фальшивыми таймерами

**Задача:** проверьте, что `debounce(spy, 100)` после вызовов `d(1)`, пауза 50 мс, `d(2)` вызывает `spy` один раз и только через 100 мс после последнего вызова, с аргументом `2`. Реальные таймеры не используйте.

**Ожидаемый результат:** тест проходит мгновенно.

<details><summary>Подсказка</summary>

`t.mock.timers.enable({ apis: ['setTimeout'] })`, затем `tick(50)`, `tick(99)`, `tick(1)`.

</details>

<details><summary>Решение</summary>

```js
test('debounce calls once with the last arguments', (t) => {
  t.mock.timers.enable({ apis: ['setTimeout'] });
  const spy = mock.fn();
  const d = debounce(spy, 100);
  d(1); t.mock.timers.tick(50); d(2); t.mock.timers.tick(99);
  assert.equal(spy.mock.callCount(), 0);
  t.mock.timers.tick(1);
  assert.equal(spy.mock.callCount(), 1);
  assert.deepEqual(spy.mock.calls[0].arguments, [2]);
});
```

```text
✔ debounce calls once with the last arguments
```

</details>

## 5. 🔴 Частичное сравнение

**Задача:** у объекта `user` много полей. Проверьте только `id` и `meta.tz`, не перечисляя остальные.

**Ожидаемый результат:** тест проходит.

<details><summary>Подсказка</summary>

`assert.partialDeepStrictEqual(actual, expectedPart)`.

</details>

<details><summary>Решение</summary>

```js
test('partial structural check', () => {
  const user = { id: 1, name: 'A', roles: ['x', 'y'], meta: { created: 1, tz: 'UTC' } };
  assert.partialDeepStrictEqual(user, { id: 1, meta: { tz: 'UTC' } });
});
```

```text
✔ partial structural check
```

В Node.js 24.15 метод был доступен (в опыте).

</details>
