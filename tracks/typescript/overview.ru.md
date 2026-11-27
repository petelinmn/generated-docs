---
title: "TypeScript"
lang: ru
track: typescript
kind: track
doc: overview
slug: typescript
tags: [typescript, type-system, nodejs]
versions: "TypeScript 7.0, Node.js 24 LTS"
default_level: 4
status: draft
updated: 2026-11-23
---

# TypeScript

🌐 **Русский** · [English](overview.en.md)

## Коротко

TypeScript — надмножество [JavaScript](../javascript/overview.ru.md) со статической системой типов. Типы проверяются на этапе компиляции и стираются: на выходе обычный JavaScript, поэтому в рантайме ничего не меняется. Система типов структурная (важна форма значения, а не имя типа) и очень выразительная: объединения, дженерики, условные и отображаемые типы. Начиная с версии 7.0 компилятор `tsc` — нативная программа, портированная на Go, и работает заметно быстрее прежнего. Node.js 24 умеет запускать `.ts`-файлы напрямую, стирая типы без проверки (type stripping), а проверку выполняет `tsc`.

**Пререквизиты:** модули 01–08 [трека JavaScript](../javascript/overview.ru.md).

## Окружение

| Что | Инструмент |
|---|---|
| Рантайм | Node.js 24 LTS |
| Компилятор и проверка типов | `typescript` (команда `tsc`), `npm install -D typescript` |
| Запуск `.ts` | `node файл.ts` (стирание типов) или `tsx` |
| Редактор | VS Code со встроенной поддержкой TypeScript |
| Код модуля | `code/` — `package.json`, `tsconfig.json`, `examples/exNN-name.ts`, `solutions/`, `tests/*.test.ts` |
| Запустить все тесты модуля | `npm test` в папке `code/` (vitest: `vitest run`) |
| Проверить типы | `npx tsc --noEmit` в папке `code/` |

## Дорожная карта

Прогресс изучения — в [PROGRESS.md](../../PROGRESS.md). Уровень детализации модулей по умолчанию — **4** (теория и самопроверка, без кода), см. [уровни детализации](../../README.ru.md#уровни-детализации).

### Часть 1. Основы

- [01 · Окружение и инструменты](modules/01-environment/overview.ru.md) — `tsc`, `tsconfig.json`, режим `strict`, запуск `.ts` в Node.js
- [02 · Базовые типы](modules/02-basic-types/overview.ru.md) — примитивы, массивы, кортежи, `any`/`unknown`/`never`, литеральные типы, `enum`
- [03 · Объекты и интерфейсы](modules/03-objects-and-interfaces/overview.ru.md) — `interface` и `type`, необязательные и `readonly` поля, структурная типизация
- [04 · Функции](modules/04-functions/overview.ru.md) — типы параметров и результата, перегрузки, `this`, `void`
- [05 · Объединения и сужение типов](modules/05-unions-and-narrowing/overview.ru.md) — union, intersection, защитники типов, размеченные объединения, исчерпывающая проверка
- [06 · Дженерики](modules/06-generics/overview.ru.md) — параметры типов, ограничения, значения по умолчанию
- [07 · Классы](modules/07-classes/overview.ru.md) — модификаторы доступа, `abstract`, `implements`, свойства-параметры
- [08 · Модули и конфигурация](modules/08-modules-and-tsconfig/overview.ru.md) — ESM и CommonJS, `module`, `moduleResolution`, ключевые опции `tsconfig`

### Часть 2. Продвинутые темы

- [09 · Служебные типы](modules/09-utility-types/overview.ru.md) — `Partial`, `Pick`, `Omit`, `Record`, `ReturnType`, `Awaited`
- [10 · Условные типы и `infer`](modules/10-conditional-types/overview.ru.md) — `extends ? :`, распределение по union, `infer`
- [11 · Отображаемые и шаблонные литеральные типы](modules/11-mapped-and-template-literal-types/overview.ru.md) — `keyof`, `typeof`, индексированный доступ, mapped types
- [12 · Типы для асинхронного кода](modules/12-async-types/overview.ru.md) — `Promise<T>`, `async`-функции, асинхронные итераторы
- [13 · Ошибки и валидация данных](modules/13-errors-and-validation/overview.ru.md) — `unknown` в `catch`, проверки на границах, Zod
- [14 · Тестирование: vitest](modules/14-testing-vitest/overview.ru.md) — тесты, моки, `expectTypeOf`, покрытие
- [15 · Файлы деклараций](modules/15-declaration-files/overview.ru.md) — `.d.ts`, `@types`, слияние деклараций, расширение модулей
- [16 · Сборка и монорепозитории](modules/16-build-and-monorepos/overview.ru.md) — project references, сборщики, публикация типов

### Часть 3. Темы для опытных

- [17 · Устройство системы типов](modules/17-type-system-internals/overview.ru.md) — совместимость, вариантность, `satisfies`, неочевидные дыры в строгости
- [18 · Программирование на уровне типов](modules/18-type-level-programming/overview.ru.md) — рекурсивные типы, brand-типы, типобезопасные API
- [19 · Декораторы](modules/19-decorators/overview.ru.md) — стандартные декораторы ECMAScript и их типизация
- [20 · Производительность компилятора](modules/20-compiler-performance/overview.ru.md) — диагностика `tsc`, `isolatedDeclarations`, project references
- [21 · TypeScript в продакшене](modules/21-typescript-in-production/overview.ru.md) — миграция с JavaScript, линтинг, проектирование API

## Библиотеки

- [Zod](libraries/zod/overview.ru.md) — схемы и вывод типов из них
- [vitest](libraries/vitest/overview.ru.md) — тестовый фреймворк
- [tsx](libraries/tsx/overview.ru.md) — запуск TypeScript в Node.js
- [type-fest](libraries/type-fest/overview.ru.md) — коллекция служебных типов
- [ts-pattern](libraries/ts-pattern/overview.ru.md) — сопоставление с образцом

## Рецепты

- [Типизированный API-клиент](recipes/typed-api-client/overview.ru.md) — типы запросов и ответов, проверка на границе
- [Проверенная конфигурация](recipes/validated-config/overview.ru.md) — окружение, схема и тип
- [Публикация библиотеки](recipes/library-publishing/overview.ru.md) — типы, ESM и CommonJS, `exports`
- [Монорепозиторий](recipes/monorepo-setup/overview.ru.md) — workspaces и project references

## Проекты

- [Типизированная CLI-утилита](projects/typed-cli/overview.ru.md) — утилита с проверкой аргументов
- [Типизированный REST-сервис](projects/typed-rest-service/overview.ru.md) — API со схемами и тестами

## Связанные треки

- [JavaScript](../javascript/overview.ru.md) — основа языка; без модулей 01–08 типы не имеют смысла
- [Python](../python/overview.ru.md) — аннотации типов Python (mypy, pyright) устроены иначе: сравните
- [C# / .NET](../csharp/overview.ru.md) — номинальная типизация против структурной
- [React](../react/overview.ru.md) — типы для компонентов, props и хуков

## Источники

- [TypeScript Handbook](https://www.typescriptlang.org/docs/handbook/intro.html)
- [TSConfig Reference](https://www.typescriptlang.org/tsconfig/)
- [Announcing TypeScript 7.0](https://devblogs.microsoft.com/typescript/announcing-typescript-7-0/)
- [Node.js: запуск TypeScript](https://nodejs.org/docs/latest-v24.x/api/typescript.html)
