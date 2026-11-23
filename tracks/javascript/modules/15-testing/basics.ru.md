---
title: "Тестирование: основы"
lang: ru
track: javascript
kind: module
doc: basics
slug: 15-testing
part: 2
tags: [node-test, assert, describe, hooks, async-test, subtests, skip, todo]
versions: "Node.js 24.15 LTS (node:test, node:assert)"
status: draft
updated: 2026-11-23
---

# Тестирование: основы

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [overview](overview.ru.md) · **1. Основы** · [2. Продвинутый](advanced.ru.md) · [3. Эксперт](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [09 · Модули](../09-modules/overview.ru.md), [12 · Промисы и async/await](../12-promises-and-async-await/overview.ru.md), [14 · Ошибки и отладка](../14-errors-and-debugging/overview.ru.md) · **Время:** ~2.5 ч

Все примеры запускались на Node.js 24.15 в папке с `package.json` (`{"type": "module"}`). Тестируемый код лежит в `lib/math.js`:

```js
export const add = (a, b) => a + b;
export function divide(a, b) { if (b === 0) throw new RangeError('division by zero'); return a / b; }
export async function fetchUser(id, client) { const r = await client.get(`/users/${id}`); if (!r.ok) throw new Error('not ok'); return r.json(); }
export function untested(x) { return x > 0 ? 'pos' : 'neg'; }
```

## Зачем это нужно

Тесты фиксируют ожидаемое поведение и ловят регрессии при изменениях. Встроенный раннер Node.js позволяет начать без установки пакетов.

## Первый тест

Файл `test/basic.test.js`:

```js
import { test, describe, it } from 'node:test';
import assert from 'node:assert/strict';
import { add, divide } from '../lib/math.js';

test('add works', () => { assert.equal(add(1, 2), 3); });

describe('divide', () => {
  it('divides', () => { assert.equal(divide(6, 3), 2); });
  it('throws on zero', () => { assert.throws(() => divide(1, 0), { name: 'RangeError', message: 'division by zero' }); });
  it('deep equality', () => { assert.deepEqual({ a: [1, 2] }, { a: [1, 2] }); });
  it.skip('skipped one', () => { assert.fail('never runs'); });
  it.todo('todo one');
});
```

Запуск: `node --test`.

```text
✔ add works (0.616ms)
▶ divide
  ✔ divides (0.1988ms)
  ✔ throws on zero (0.6042ms)
  ✔ deep equality (0.4495ms)
  ﹣ skipped one (0.0885ms) # SKIP
  ✔ todo one (0.0621ms) # TODO
✔ divide (1.7438ms)
```

- `test(name, fn)` объявляет тест. Тест **проходит**, если функция завершилась без исключения, и **падает**, если она бросила ошибку.
- `describe(name, fn)` группирует тесты в набор, `it` — синоним `test` внутри набора.
- `it.skip(…)` пропускает тест (`# SKIP`), `it.todo(…)` помечает планируемый (`# TODO`, не считается падением).
- Времена выполнения будут другими.

## Утверждения (assert)

Модуль `node:assert` бросает `AssertionError`, если условие не выполнено. Импортируйте **строгий** режим — `node:assert/strict`:

| Утверждение | Проверяет |
|---|---|
| `assert.ok(x)` | значение истинно |
| `assert.equal(a, b)` | строгое равенство по `Object.is` (в опыте `assert.strictEqual(NaN, NaN)` проходит) |
| `assert.notEqual(a, b)` | строгое неравенство |
| `assert.deepEqual(a, b)` | глубокое строгое равенство объектов, массивов, `Map`, `Set` |
| `assert.throws(fn, expected)` | `fn` бросает ошибку, подходящую под `expected` (класс, объект, регулярное выражение) |
| `assert.rejects(promise, expected)` | промис отклоняется |
| `assert.match(str, re)` | строка соответствует регулярному выражению |
| `assert.fail(msg)` | безусловное падение |

В обычном `node:assert` без `/strict` `equal` и `deepEqual` сравнивают нестрого (`'1'` равно `1`): ошибки проскальзывают, поэтому используйте строгий режим ([экспертная теория](expert.ru.md#справочник)).

При падении раннер показывает разницу:

```text
✖ fails with diff
  AssertionError [ERR_ASSERTION]: Expected values to be strictly deep-equal:
  + actual - expected

    {
      a: 1,
      b: [
        1,
  +     2
  -     3
      ]
    }
```

## Хуки

Хуки готовят окружение и убирают за собой:

```js
import { describe, it, before, after, beforeEach, afterEach } from 'node:test';
describe('hooks', () => {
  const order = [];
  before(() => order.push('before'));
  after(() => { order.push('after'); console.log('# hooks order:', order.join(',')); });
  beforeEach(() => order.push('beforeEach'));
  afterEach(() => order.push('afterEach'));
  it('first', () => order.push('first'));
  it('second', () => order.push('second'));
});
```

```text
# hooks order: before,beforeEach,first,afterEach,beforeEach,second,afterEach,after
```

`before` и `after` выполняются один раз на набор, `beforeEach` и `afterEach` — вокруг каждого теста.

## Асинхронные тесты и подзадачи

```js
import { test } from 'node:test';
import assert from 'node:assert/strict';
test('async and rejects', async () => {
  await assert.rejects(Promise.reject(new TypeError('bad')), TypeError);
  assert.equal(await Promise.resolve(5), 5);
});
test('subtests', async (t) => {
  await t.test('child 1', () => assert.ok(true));
  await t.test('child 2', () => assert.ok(true));
});
```

```text
✔ async and rejects (0.2662ms)
▶ subtests
  ✔ child 1 (0.1486ms)
  ✔ child 2 (0.0661ms)
✔ subtests (0.5283ms)
```

- Асинхронный тест — функция `async`; раннер дожидается промиса. **Не забывайте `await`** перед `assert.rejects`: иначе тест закончится раньше проверки.
- Подзадачи создают через `t.test(...)` с `await`.

## Код возврата

`node --test` завершается с кодом `0`, если все тесты прошли, и `1`, если хоть один упал (в опыте). Поэтому эту команду ставят в CI и в `npm test`:

```json
{ "scripts": { "test": "node --test" } }
```

## Ошибки новичков

- ❌ Забыть `await` перед асинхронным утверждением → ✅ `await assert.rejects(…)`.
- ❌ Использовать нестрогий `node:assert` → ✅ `node:assert/strict`.
- ❌ Тесты, зависящие друг от друга и от порядка → ✅ независимые тесты и `beforeEach`.
- ❌ Проверять реализацию, а не поведение → ✅ вход и результат.
- ❌ Сравнивать объекты через `equal` → ✅ `deepEqual`.
- ❌ Ходить в сеть и файлы в юнит-тестах → ✅ моки ([продвинутая теория](advanced.ru.md)).

## Связанные материалы

- Дальше: [продвинутая теория](advanced.ru.md) — моки, фальшивые таймеры, покрытие, снимки
- Ранее: [14 · Ошибки и отладка](../14-errors-and-debugging/basics.ru.md)

## Источники

- [Node.js: Test runner](https://nodejs.org/docs/latest-v24.x/api/test.html)
- [Node.js: Assert](https://nodejs.org/docs/latest-v24.x/api/assert.html)
- [Node.js: Running tests from the command line](https://nodejs.org/docs/latest-v24.x/api/test.html#running-tests-from-the-command-line)
