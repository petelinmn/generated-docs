---
title: "Тестирование: экспертный уровень"
lang: ru
track: javascript
kind: module
doc: expert
slug: 15-testing
part: 2
tags: [node-test, test-discovery, process-isolation, assert-strict, coverage, snapshot, flaky-tests, mock-timers]
versions: "Node.js 24.15 LTS (node:test, node:assert)"
status: draft
updated: 2026-11-23
---

# Тестирование: экспертный уровень

🌐 **Русский** · [English](expert.en.md)

**Уровни:** [overview](overview.ru.md) · [1. Основы](basics.ru.md) · [2. Продвинутый](advanced.ru.md) · **3. Эксперт** · [4. Квиз](quiz.ru.md)

Опирается на [базовую](basics.ru.md) и [продвинутую](advanced.ru.md) теорию. Все результаты получены запуском на Node.js 24.15.

## Справочник

### Флаги командной строки (из `node --help`)

| Флаг | Назначение |
|---|---|
| `--test` | запустить раннер (для `node --test` указывать отдельно не нужно) |
| `--test-concurrency=N` | число одновременно выполняемых тестовых файлов |
| `--test-name-pattern=…` / `--test-skip-pattern=…` | выполнить / пропустить тесты по имени |
| `--test-only` | выполнить только тесты с `only` |
| `--test-reporter=…` / `--test-reporter-destination=…` | формат и место вывода отчёта |
| `--test-timeout=…` | таймаут теста |
| `--test-force-exit` | принудительно завершить процесс после тестов |
| `--test-shard=…` | выполнить часть тестов (для распараллеливания в CI) |
| `--test-update-snapshots` | перегенерировать снимки |
| `--test-rerun-failures=…` | повторить упавшие по файлу состояния |
| `--test-global-setup=…` | глобальная подготовка перед всеми тестами |
| `--test-coverage-include/exclude/lines/branches/functions` | фильтры и пороги покрытия |
| `--experimental-test-coverage` | собрать покрытие |

> ⚠️ Проверить: действие `--test-concurrency`, `--test-force-exit`, `--test-shard`, `--test-rerun-failures` и `--test-global-setup` взято из справки и документации, в опытах не запускалось.

### Утверждения `node:assert` (в опыте)

| Метод | Особенности |
|---|---|
| `ok`, `equal`, `notEqual`, `deepEqual`, `notDeepEqual` | в `assert/strict` строгие (`===`, глубокое строгое сравнение) |
| `strictEqual`, `deepStrictEqual` | проверка по `Object.is` и глубокая строгая; `strictEqual(NaN, NaN)` проходит |
| `throws`, `doesNotThrow` | синхронные ошибки; второй аргумент — класс, объект (поля сверяются), регулярное выражение |
| `rejects`, `doesNotReject` | для промисов, **нужен `await`** |
| `match`, `doesNotMatch` | регулярные выражения |
| `ifError` | бросает, если значение не `null`/`undefined` |
| `partialDeepStrictEqual` | вложенное частичное сравнение (в опыте доступно в Node.js 24.15) |
| `fail` | безусловное падение |

Нестрогий режим (`import assert from 'node:assert'`): `equal('1', 1)` и `deepEqual({ a: '1' }, { a: 1 })` **проходят** (в опыте).

### Контекст теста `t`

`t.test(…)` — подтест; `t.diagnostic(msg)` — заметка в отчёте (в опыте `ℹ custom note`); `t.plan(n)` — ожидаемое число проверок (в опыте `t.plan(1)` с одним `t.assert.equal`); `t.assert` — утверждения, привязанные к тесту (нужны для `snapshot`); `t.mock` — моки с автоочисткой.

## Обнаружение тестовых файлов

При запуске **без аргументов** раннер рекурсивно ищет файлы по шаблонам (по документации):

| Шаблон | Пример |
|---|---|
| `**/*.test.{cjs,mjs,js}` | `math.test.js` |
| `**/*-test.{cjs,mjs,js}` | `other-test.js` |
| `**/*_test.{cjs,mjs,js}` | `math_test.js` |
| `**/test-*.{cjs,mjs,js}` | `test-math.js` |
| `**/test.{cjs,mjs,js}` | `test.js` |
| `**/test/**/*.{cjs,mjs,js}` | **любой** файл в папке `test` |

В опыте запустились `other-test.js` в корне, `lib/not-a-test.js` (имя оканчивается на `-test.js`) и `test/helper.js` (вспомогательный файл в папке `test`). Поэтому вспомогательный код **не кладите** в `test/` и не называйте так, чтобы имя подходило под шаблон; с аргументами (`node --test test/a.test.js`) выполняются только перечисленные файлы.

## Внутреннее устройство

- **Процесс на файл.** Каждый тестовый файл выполняется в отдельном процессе Node.js; в опыте глобальная переменная из первого файла была `1`, а во втором — `undefined`. Поэтому состояние между файлами не делится и порядок файлов не важен; внутри файла всё общее.
- **Асинхронность.** Раннер ожидает промис, который вернул тест; необработанная ошибка или отклонённый промис помечают тест упавшим. Тест без `await` на последнем асинхронном действии завершится раньше.
- **Таймаут.** При превышении тест отменяется с сообщением `test timed out after 20ms`; в сводке такой тест попадает в `cancelled` (в опыте `cancelled 1`).
- **Код выхода.** `1`, если есть упавшие или отменённые; `0` в остальных случаях (в опыте).
- **Мок времени.** `mock.timers` подменяет глобальные `setTimeout` и `Date` на время теста; `tick` вызывает таймеры синхронно.
- **Покрытие.** Данные собирает V8; раннер превращает их в отчёт (строки, ветки, функции). Строка считается покрытой, если выполнился любой её фрагмент, поэтому однострочные функции дают «100%» строк при непокрытой функции (в опыте `funcs 75%`).

> ⚠️ Проверить: детали сбора покрытия V8 и режимы `--test-concurrency` при общих ресурсах (порты, файлы) не проверялись.

## Подвохи

### 1. `only` без `--test-only`

Метка игнорируется (раннер выводит предупреждение); выполняются все тесты.

### 2. Свой текст ошибки дописывает разницу

`assert.strictEqual(1, 2, 'custom message')` бросает ошибку с `message`, равным `'custom message\n\n1 !== 2\n'`: `assert.throws(…, { message: 'custom message' })` в опыте **не прошёл** (Node.js 24.15).

### 3. Забытый `await` у `assert.rejects`

Тест завершится до проверки и будет помечен `✔`, но ошибка проявится позже: раннер напишет `generated asynchronous activity after the test ended` и пометит падающим весь файл (в опыте `pass 1`, `fail 1`, код выхода `1`).

### 4. Нестрогий `node:assert`

`equal('1', 1)` проходит: баги прячутся. Используйте `node:assert/strict`.

### 5. Вспомогательные файлы в `test/`

Раннер выполнит их как тесты (в опыте `helper executed as test file`).

### 6. Снимок при первом запуске

Тест падает, пока не создан файл снимка (`--test-update-snapshots`).

### 7. Покрытие строк обманчиво

Однострочная непокрытая функция даёт `100%` строк и `75%` функций.

### 8. Общее состояние между тестами одного файла

Файл — один процесс: изменённый глобальный объект или не сброшенный мок «протекает» в следующий тест.

### 9. Нестабильные тесты

Реальные таймеры, случайность, текущее время и порядок выполнения делают тесты недетерминированными: подменяйте их (`mock.timers`) и фиксируйте начальные данные.

## Компромиссы и альтернативы

| Инструмент | Плюсы | Минусы |
|---|---|---|
| `node:test` | встроен, без зависимостей, процесс на файл | меньше возможностей, чем у Jest/Vitest; часть функций экспериментальна |
| Jest | матчеры, моки, снимки, большая экосистема | зависимость, свой раннер |
| Vitest | быстрый, совместим с Vite, TypeScript | зависимость; не запускался здесь |
| Mocha + Chai | гибкость | сборка из частей |
| Playwright | сквозные и браузерные тесты | отдельный инструмент |
| Тесты по контракту | меньше моков | сложнее настроить |

## Вопросы с собеседований

<details><summary>1. Как запустить тесты в Node.js без сторонних пакетов?</summary>

`node --test`: раннер находит файлы по шаблонам и запускает; код выхода `0` или `1`.

→ [Базовая теория](basics.ru.md#первый-тест)

</details>

<details><summary>2. Чем <code>assert/strict</code> отличается от <code>assert</code>?</summary>

В строгом режиме `equal` и `deepEqual` сравнивают строго; в обычном — нестрого (`'1'` равно `1`).

→ [Подвох 4](#4-нестрогий-nodeassert)

</details>

<details><summary>3. Подвох: тест с <code>assert.rejects</code> без <code>await</code> показывает <code>✔</code>, хотя проверка не должна пройти. Что происходит?</summary>

Тест завершился раньше проверки; ошибка проявится после его окончания и уронит весь файл. Нужен `await`.

→ [Подвох 3](#3-забытый-await-у-assertrejects)

</details>

<details><summary>4. Как изолированы тестовые файлы?</summary>

Каждый файл — отдельный процесс; глобальное состояние не делится между файлами.

→ [Внутреннее устройство](#внутреннее-устройство)

</details>

<details><summary>5. Как подменить зависимость в тесте?</summary>

Передать её параметром и подставить `mock.fn`; или `t.mock.method(obj, 'name', impl)`.

→ [Продвинутая теория](advanced.ru.md#моки)

</details>

<details><summary>6. Как протестировать код с <code>setTimeout</code>, не дожидаясь?</summary>

`t.mock.timers.enable({ apis: ['setTimeout'] })` и `t.mock.timers.tick(ms)`.

→ [Продвинутая теория](advanced.ru.md#фальшивые-таймеры)

</details>

<details><summary>7. Подвох: почему <code>{ only: true }</code> не сработал?</summary>

Нужен флаг `--test-only`.

→ [Подвох 1](#1-only-без---test-only)

</details>

<details><summary>8. Что показывает покрытие и чем оно обманчиво?</summary>

Доли выполненных строк, веток, функций; высокие числа не означают хорошие проверки и могут скрывать непокрытые функции в одной строке.

→ [Подвох 7](#7-покрытие-строк-обманчиво)

</details>

<details><summary>9. Как работают снимки и где их хранить?</summary>

`t.assert.snapshot` сравнивает с файлом `*.snapshot`; первый запуск требует `--test-update-snapshots`; файл коммитят.

→ [Продвинутая теория](advanced.ru.md#снимки)

</details>

<details><summary>10. Подвох: почему тест падает на пустом месте, если передать свой текст в <code>assert.strictEqual</code>?</summary>

Текст не «чистый»: Node.js дописывает к нему разницу значений, и сравнение по точному тексту ошибки не совпадёт.

→ [Подвох 2](#2-свой-текст-ошибки-дописывает-разницу)

</details>

<details><summary>11. Что делает тест нестабильным и как это исправить?</summary>

Время, случайность, сеть, порядок и общее состояние. Подменять, фиксировать данные, изолировать тесты.

→ [Подвох 9](#9-нестабильные-тесты)

</details>

<details><summary>12. Какой код выхода у <code>node --test</code> и зачем это нужно?</summary>

`0` при успехе, `1` при падении; CI по коду выхода решает, прошла ли сборка.

→ [Базовая теория](basics.ru.md#код-возврата)

</details>

## Шпаргалка

```text
node --test [файлы]   --test-name-pattern  --test-only  --test-timeout  --test-reporter=dot
--experimental-test-coverage   --test-update-snapshots
import { test, describe, it, before, after, beforeEach, afterEach, mock } from 'node:test'
import assert from 'node:assert/strict'   // equal, deepEqual, throws, rejects (await!), match
mock.fn(impl) · t.mock.method(obj, 'm', impl) · t.mock.timers.enable({ apis: ['setTimeout', 'Date'] }); tick(ms)
t.assert.snapshot(value) · test(name, { timeout, skip, todo, only }, fn)
```

- Файл — отдельный процесс; внутри файла состояние общее.
- `only` работает только с `--test-only`.
- Не кладите вспомогательные файлы в `test/`.

## Связанные материалы

- Предыдущий уровень: [продвинутая теория](advanced.ru.md)
- Ошибки и `assert`: [14 · Ошибки и отладка](../14-errors-and-debugging/expert.ru.md)
- Таймеры и цикл событий: [13 · Цикл событий и параллелизм](../13-event-loop-and-concurrency/expert.ru.md)
- Модульные тесты в C#: [16 · Модульное тестирование](../../../csharp/modules/16-testing-unit/expert.ru.md)

## Источники

- [Node.js: Test runner](https://nodejs.org/docs/latest-v24.x/api/test.html)
- [Node.js: Test runner execution model](https://nodejs.org/docs/latest-v24.x/api/test.html#test-runner-execution-model)
- [Node.js: Assert](https://nodejs.org/docs/latest-v24.x/api/assert.html)
- [Node.js: Command-line options (test flags)](https://nodejs.org/docs/latest-v24.x/api/cli.html)
