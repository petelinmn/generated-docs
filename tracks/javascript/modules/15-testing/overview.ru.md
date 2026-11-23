---
title: "Тестирование"
lang: ru
track: javascript
kind: module
doc: overview
slug: 15-testing
part: 2
levels: [1, 2, 3, 4]
tags: [testing, node-test, assert, mock, fake-timers, coverage, snapshot, test-runner]
versions: "Node.js 24.15 LTS (node:test, node:assert)"
status: draft
updated: 2026-11-23
---

# Тестирование

🌐 **Русский** · [English](overview.en.md)

## Коротко

В Node.js есть встроенный тестовый раннер `node:test` и модуль утверждений `node:assert`: без зависимостей и настроек можно писать тесты, запускать их командой `node --test`, использовать хуки, подзадачи, моки, фальшивые таймеры, снимки и отчёт о покрытии. Хороший тест детерминирован, быстр, независим от других и проверяет поведение, а не реализацию.

## Ключевые идеи

- **`node --test`** находит и запускает тестовые файлы; каждый файл выполняется в **отдельном процессе**.
- **`test`, `describe`, `it`** описывают тесты и наборы; `skip` и `todo` помечают пропущенные.
- **`assert/strict`:** строгое сравнение (`equal`, `deepEqual`, `throws`, `rejects`); в обычном `node:assert` режим нестрогий.
- **Хуки** `before`, `after`, `beforeEach`, `afterEach` готовят и убирают окружение.
- **Асинхронные тесты** — просто `async`-функции; ошибку в промисе проверяют через `assert.rejects`.
- **Моки** (`mock.fn`, `mock.method`) подменяют зависимости и считают вызовы.
- **Фальшивые таймеры** (`mock.timers`) управляют `setTimeout` и `Date` без ожидания.
- **Покрытие и снимки:** `--experimental-test-coverage`, `t.assert.snapshot` с `--test-update-snapshots`.
- **Код возврата** `node --test` — `0` при успехе и `1` при падении: так тесты подключают к CI.

## Минимальный пример

```js
// math.test.js
import { test } from 'node:test';
import assert from 'node:assert/strict';
const add = (a, b) => a + b;
test('add works', () => { assert.equal(add(1, 2), 3); });
```

```text
$ node --test
✔ add works (0.616ms)
ℹ tests 1
ℹ pass 1
ℹ fail 0
```

## Когда применять и когда нет

| Задача | Выбор |
|---|---|
| Юнит-тесты функций и модулей | `node:test` + `assert/strict` |
| Подмена зависимости, проверка вызовов | `mock.fn`, `mock.method` |
| Код с таймерами и датами | `mock.timers` |
| Много готовых матчеров, снимки интерфейса, jest-экосистема | Jest или Vitest (см. [TypeScript](../../../typescript/overview.ru.md)) |
| Тесты в браузере | Playwright, Vitest browser mode (не запускались) |

- ❌ Не тестируйте детали реализации: тест ломается при каждом рефакторинге.
- ❌ Не ходите в сеть и файловую систему в юнит-тестах.

## Уровни материала

| Уровень | Что внутри |
|---|---|
| [1. Базовая теория](basics.ru.md) | `test`, `describe`/`it`, утверждения, хуки, асинхронные тесты, подзадачи, запуск |
| [2. Продвинутая теория](advanced.ru.md) | моки, фальшивые таймеры, фильтры, таймауты, покрытие, снимки, отчёты, структура тестов |
| [3. Экспертная теория](expert.ru.md) | справочник, обнаружение файлов, изоляция, подвохи, вопросы с собеседований |
| [4. Самопроверка](quiz.ru.md) | квиз и [упражнения](exercises.ru.md) |
| 5. Запускаемые примеры | пока не создан — `/learn javascript 15-testing level 5` |
| 6. Тесты | пока не создан — `/learn javascript 15-testing level 6` |

## Связанные материалы

- Пререквизиты: [14 · Ошибки и отладка](../14-errors-and-debugging/overview.ru.md), [12 · Промисы и async/await](../12-promises-and-async-await/overview.ru.md), [09 · Модули](../09-modules/overview.ru.md)
- Дальше: модуль 16 · Node.js: файлы, потоки, процессы (в [дорожной карте](../../overview.ru.md))
- Аналоги в других треках: [C#: модульное тестирование](../../../csharp/modules/16-testing-unit/overview.ru.md), [Python](../../../python/overview.ru.md) (pytest), [TypeScript](../../../typescript/overview.ru.md) (vitest)
