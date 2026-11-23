---
title: "JavaScript"
lang: ru
track: javascript
kind: track
doc: overview
slug: javascript
tags: [javascript, ecmascript, nodejs]
versions: "ECMAScript 2025+, Node.js 24 LTS"
default_level: 4
status: draft
updated: 2026-11-23
---

# JavaScript

🌐 **Русский** · [English](overview.en.md)

## Коротко

JavaScript — язык с динамической слабой типизацией, прототипным наследованием и однопоточной моделью выполнения на цикле событий (event loop). Он стандартизован как ECMAScript (спецификация ECMA-262) и работает в браузерах и на сервере: в Node.js, Deno и Bun. Функции в нём — обычные значения, асинхронность встроена (промисы, `async`/`await`), а модули ES (ESM) — стандартный способ организации кода. Большая часть веб-интерфейсов и огромная часть серверных сервисов написаны на JavaScript, а [TypeScript](../typescript/overview.ru.md) добавляет к нему статические типы.

## Окружение

| Что | Инструмент |
|---|---|
| Рантайм | Node.js 24 LTS; для модулей про DOM — браузер |
| Менеджер пакетов | npm (поставляется с Node.js) |
| Редактор | VS Code, DevTools браузера, отладчик Node.js (`node --inspect`) |
| Код модуля | `code/` — `package.json` (`"type": "module"`), `examples/exNN-name.js`, `solutions/`, `tests/*.test.js` |
| Запустить все тесты модуля | `npm test` в папке `code/` (встроенный раннер: `node --test`) |
| Запустить один пример | `node examples/ex01-name.js` |
| Линтер и форматтер | ESLint, Prettier |

## Дорожная карта

Прогресс изучения — в [PROGRESS.md](../../PROGRESS.md). Уровень детализации модулей по умолчанию — **4** (теория и самопроверка, без кода), см. [уровни детализации](../../README.ru.md#уровни-детализации).

### Часть 1. Основы

- [01 · Окружение и инструменты](modules/01-environment/overview.ru.md) — Node.js, npm, `package.json`, REPL, DevTools, запуск скриптов
- [02 · Типы и значения](modules/02-types-and-values/overview.ru.md) — примитивы и объекты, `typeof`, приведение типов, `==` и `===`, `NaN`, BigInt, Symbol
- [03 · Переменные и области видимости](modules/03-scopes-and-hoisting/overview.ru.md) — `let`/`const`/`var`, всплытие (hoisting), TDZ, блочная область
- [04 · Функции](modules/04-functions/overview.ru.md) — объявления и выражения, стрелочные функции, параметры, `this`, `call`/`apply`/`bind`
- [05 · Объекты и прототипы](modules/05-objects-and-prototypes/overview.ru.md) — литералы, дескрипторы свойств, цепочка прототипов, деструктуризация, spread
- [06 · Классы](modules/06-classes/overview.ru.md) — синтаксис `class`, приватные поля, `static`, геттеры, наследование
- [07 · Массивы и коллекции](modules/07-arrays-and-collections/overview.ru.md) — методы массивов, `Map`, `Set`, `WeakMap`, типизированные массивы
- [08 · Строки, числа, даты, регулярные выражения](modules/08-strings-numbers-dates-regex/overview.ru.md) — `Intl`, `Date` и Temporal, `RegExp`

### Часть 2. Продвинутые темы

- [09 · Модули](modules/09-modules/overview.ru.md) — ESM и CommonJS, динамический `import`, `exports` в `package.json`
- [10 · Замыкания и функциональный стиль](modules/10-closures-and-functional/overview.ru.md) — замыкания, каррирование, функции высшего порядка, неизменяемость
- [11 · Итераторы и генераторы](modules/11-iterators-and-generators/overview.ru.md) — протокол итерации, `function*`, асинхронные генераторы, iterator helpers
- [12 · Промисы и async/await](modules/12-promises-and-async-await/overview.ru.md) — состояния промиса, `Promise.all`/`allSettled`/`any`, ошибки
- [13 · Цикл событий и конкурентность](modules/13-event-loop-and-concurrency/overview.ru.md) — очереди задач и микрозадач, таймеры, `AbortController`, worker-потоки
- [14 · Ошибки и отладка](modules/14-errors-and-debugging/overview.ru.md) — `Error`, `cause`, `AggregateError`, стек вызовов, отладчик
- [15 · Тестирование](modules/15-testing/overview.ru.md) — `node:test`, моки, покрытие, тесты асинхронного кода
- [16 · Node.js: файлы, потоки, процессы](modules/16-nodejs-files-streams-processes/overview.ru.md) — `fs`, streams, `child_process`, переменные окружения

### Часть 3. Темы для опытных

- [17 · Браузер: DOM и события](modules/17-browser-dom-and-events/overview.ru.md) — DOM, всплытие событий, `fetch`, хранилища, Web API
- [18 · Метапрограммирование](modules/18-metaprogramming/overview.ru.md) — `Proxy`, `Reflect`, символы, tagged templates, `WeakRef`
- [19 · Производительность и память](modules/19-performance-and-memory/overview.ru.md) — V8, скрытые классы, сборка мусора, профилирование
- [20 · Безопасность](modules/20-security/overview.ru.md) — XSS, prototype pollution, CSP, цепочка поставок npm
- [21 · HTTP-сервис на Node.js](modules/21-http-service/overview.ru.md) — модуль `http`, маршрутизация, middleware, Fastify, тесты

## Библиотеки

- [Zod](libraries/zod/overview.ru.md) — валидация данных на границах приложения
- [Fastify](libraries/fastify/overview.ru.md) — HTTP-сервер
- [pino](libraries/pino/overview.ru.md) — структурное логирование
- [date-fns](libraries/date-fns/overview.ru.md) — работа с датами
- [Commander](libraries/commander/overview.ru.md) — CLI-приложения

## Рецепты

- [CLI-приложение](recipes/cli-app/overview.ru.md) — аргументы, подкоманды, код возврата
- [REST API](recipes/rest-api/overview.ru.md) — маршруты, валидация, обработка ошибок
- [Конфигурация из окружения](recipes/config-from-env/overview.ru.md) — переменные окружения, `.env`, валидация
- [Запросы с повторами](recipes/fetch-with-retry/overview.ru.md) — таймаут, ретраи, `AbortSignal`

## Проекты

- [CLI-утилита](projects/cli-tool/overview.ru.md) — утилита, которая ставится через `npm`
- [REST-сервис](projects/rest-service/overview.ru.md) — API на Node.js с тестами и Docker

## Связанные треки

- [TypeScript](../typescript/overview.ru.md) — статические типы поверх JavaScript; нужны модули 01–08 этого трека
- [Python](../python/overview.ru.md) — другой динамический язык: удобно сравнивать модель выполнения и асинхронность
- [C# / .NET](../csharp/overview.ru.md) — `async`/`await` пришёл в JavaScript из C#, сравните реализации
- [Kubernetes](../kubernetes/overview.ru.md) — деплой Node.js-сервисов

## Источники

- [MDN Web Docs: JavaScript](https://developer.mozilla.org/en-US/docs/Web/JavaScript)
- [ECMAScript Language Specification (ECMA-262)](https://tc39.es/ecma262/)
- [Документация Node.js](https://nodejs.org/docs/latest-v24.x/api/)
- [Node.js: тестовый раннер](https://nodejs.org/docs/latest-v24.x/api/test.html)
