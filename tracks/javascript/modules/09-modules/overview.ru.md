---
title: "Модули"
lang: ru
track: javascript
kind: module
doc: overview
slug: 09-modules
part: 2
levels: [1, 2, 3, 4]
tags: [esm, commonjs, import, export, dynamic-import, live-bindings, package-exports, circular-dependencies, top-level-await, interop]
versions: "ECMAScript 2025+, Node.js 24.15 LTS"
status: draft
updated: 2026-11-23
---

# Модули

🌐 **Русский** · [English](overview.en.md)

## Коротко

Модуль — файл, который явно объявляет, что он отдаёт (`export`) и что берёт у других (`import`). Модули ES (ESM) — стандарт языка: статические `import`/`export`, динамический `import()`, «живые» привязки и `await` на верхнем уровне. Рядом живёт старый формат CommonJS (`require`/`module.exports`), и их взаимодействие — главный источник путаницы в Node.js. Пакеты описывают точки входа полем `exports` в `package.json`, а модуль выполняется один раз и кэшируется.

## Ключевые идеи

- **Модуль выполняется один раз,** сколько бы файлов его ни импортировало; результат кэшируется по адресу файла.
- **Импорты поднимаются и выполняются раньше тела модуля:** зависимости вычисляются первыми.
- **Привязки «живые»:** импортированное имя — ссылка на переменную экспортёра, а не копия значения. Присвоить импортированному имени нельзя.
- **Объект пространства имён** (`import * as ns`) — только для чтения, без прототипа.
- **`export *` не переэкспортирует `default`.**
- **Динамический `import()`** возвращает промис пространства имён и позволяет загружать код лениво.
- **`await` на верхнем уровне** задерживает всех, кто импортирует модуль.
- **Циклические зависимости допустимы,** но обращение к ещё не вычисленному имени — `ReferenceError`.
- **CommonJS и ESM взаимодействуют:** из ESM `require` нет, но CJS можно импортировать; `require(esm)` в Node.js 24 работает.
- **Поле `exports`** в `package.json` ограничивает, какие пути пакета видны снаружи.

## Минимальный пример

```js
// counter.js
export let count = 0;
export function inc() { count++; }
export default function main() { return 'default'; }
```

```js
// main.js
import main, { count, inc } from './counter.js';
console.log(main(), count); inc(); console.log(count);
```

```text
default 0
1
```

`count` в `main.js` изменился после `inc()`: импорт — живая ссылка, а не копия.

## Когда применять, а когда нет

| Задача | Выбор |
|---|---|
| Новый код | ESM (`"type": "module"`) |
| Старый проект и пакеты | CommonJS, `createRequire`, `import` CJS по умолчанию |
| Загрузка по требованию, «тяжёлые» зависимости | динамический `import()` |
| Библиотека для обоих форматов | условные `exports` (с осторожностью: «dual package hazard») |
| Скрыть внутренности пакета | поле `exports` |

- ❌ Не смешивайте `import` и `require` в одном файле без нужды.
- ❌ Не полагайтесь на порядок выполнения циклических зависимостей.

## Уровни материала

| Уровень | Что внутри |
|---|---|
| [1. Базовая теория](basics.ru.md) | `export`/`import`, живые привязки, пространство имён, порядок выполнения, реэкспорт, динамический `import()`, `await` на верхнем уровне |
| [2. Продвинутая теория](advanced.ru.md) | циклы, ошибки импорта, взаимодействие с CommonJS, `require(esm)`, `exports` и `imports` в `package.json`, dual package hazard, встроенные модули |
| [3. Экспертная теория](expert.ru.md) | справочник синтаксиса, этапы загрузки, кэш модулей, подвохи, вопросы с собеседований |
| [4. Самопроверка](quiz.ru.md) | квиз и [упражнения](exercises.ru.md) |
| 5. Запускаемые примеры | ещё не создан — `/learn javascript 09-modules level 5` |
| 6. Тесты | ещё не создан — `/learn javascript 09-modules level 6` |

## Связанные материалы

- Пререквизиты: [01 · Окружение и инструменты](../01-environment/overview.ru.md) (ESM и CommonJS, `package.json`), [03 · Переменные и области видимости](../03-scopes-and-hoisting/overview.ru.md) (TDZ)
- Дальше: модуль 10 · Замыкания и функциональный стиль (в дорожной карте [трека](../../overview.ru.md))
- Асинхронность подробно: модуль 12 дорожной карты
- Аналоги в других треках: [Python: модули и пакеты](../../../python/overview.ru.md), [C#: сборки и пространства имён](../../../csharp/overview.ru.md)
- Модули и `tsconfig` в TypeScript: [TypeScript](../../../typescript/overview.ru.md)
