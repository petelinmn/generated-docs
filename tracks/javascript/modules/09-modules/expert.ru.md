---
title: "Модули: экспертная теория"
lang: ru
track: javascript
kind: module
doc: expert
slug: 09-modules
part: 2
tags: [module-records, module-linking, module-evaluation, live-bindings, namespace-exotic-object, module-map, top-level-await, cjs-lexer]
versions: "ECMAScript 2025+, Node.js 24.15 LTS"
status: draft
updated: 2026-11-23
---

# Модули: экспертная теория

🌐 **Русский** · [English](expert.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · [2. Продвинутая](advanced.ru.md) · **3. Экспертная** · [4. Квиз](quiz.ru.md)

Опирается на [базовую](basics.ru.md) и [продвинутую](advanced.ru.md) теорию. Все результаты получены запуском на Node.js 24.15.

## Справочник

### Формы export

| Запись | Смысл |
|---|---|
| `export const x = 1; export function f() {} export class K {}` | именованные экспорты объявлений |
| `export { a, b as c }` | экспорт существующих привязок, в том числе под другим именем (в опыте `export { arrow as renamed }`) |
| `export default expr` | экспорт по умолчанию (в опыте `export default 42`) |
| `export { default } from './m.js'`, `export { default as x } from …` | реэкспорт `default` |
| `export * from './m.js'` | реэкспорт всех именованных, **без** `default` |
| `export * as ns from './m.js'` | пространство имён как один экспорт |

### Формы import

| Запись | Смысл |
|---|---|
| `import d from './m.js'` | экспорт по умолчанию |
| `import { a, b as c } from './m.js'` | именованные |
| `import d, { a } from './m.js'` | оба |
| `import * as ns from './m.js'` | объект пространства имён |
| `import './m.js'` | только выполнить |
| `import data from './x.json' with { type: 'json' }` | JSON с атрибутом |
| `import('./m.js')` | динамический, промис пространства имён |
| `import.meta.url`, `.filename`, `.dirname`, `.resolve()` | сведения о модуле (в опыте) |

### Коды ошибок (в опыте)

| Код или сообщение | Причина |
|---|---|
| `SyntaxError: The requested module './counter.js' does not provide an export named 'nothing'` | нет такого экспорта |
| `SyntaxError: Named export 'x' not found. The requested module './lib.cjs' is a CommonJS module…` | имя CommonJS не распознано |
| `ERR_MODULE_NOT_FOUND` | файл не найден |
| `ERR_PACKAGE_PATH_NOT_EXPORTED` | путь закрыт полем `exports` |
| `ERR_IMPORT_ATTRIBUTE_MISSING` | JSON без атрибута |
| `ReferenceError: Cannot access 'a' before initialization` | цикл, имя в TDZ |
| `TypeError: Assignment to constant variable.` | присваивание импорту |

## Внутреннее устройство

- **Три этапа загрузки.** (1) *Построение графа:* найти файлы, разобрать их, собрать записи модулей по всем `import`/`export`. (2) *Связывание (instantiation):* создать привязки для экспортов и связать с ними импорты; здесь же ловится «нет такого экспорта». (3) *Выполнение (evaluation):* вычислить тела модулей в порядке «зависимости раньше зависящих» (в опыте `side evaluated first`).
- **Живые привязки.** Импортируемое имя — не копия, а *ссылка на привязку* модуля-экспортёра. Поэтому `count` в `main.js` увидел `1` после вызова `inc()` в другом модуле (в опыте), а присвоить импорту нельзя.
- **Карта модулей (module map).** Загрузчик кэширует модули по адресу: повторные `import` и `import()` возвращают тот же объект (в опыте `m === again`, `counter.js evaluated` один раз). Один и тот же файл, загруженный как ESM и как CommonJS, — это **два** экземпляра (в опыте `esm cjs false`).
- **Объект пространства имён** — «экзотический» объект: ключи отсортированы, свойства только для чтения, прототип `null`, `Object.prototype.toString` даёт `[object Module]` (в опыте). Он не заморожен (`Object.isFrozen(ns)` — `false`), но записи в него отклоняются.
- **Циклы.** Граф с циклом связывается целиком, а вычисляется в порядке обхода; модуль цикла видит привязки ещё не выполненного модуля в TDZ (в опыте `cb: ReferenceError`). Объявления функций инициализируются при связывании и доступны.
- **`await` на верхнем уровне.** Модуль с TLA вычисляется асинхронно; все зависящие модули ждут его завершения (в опыте `after slow` после `slow done`). Родственные модули без зависимостей друг от друга могут вычисляться параллельно.
- **Взаимодействие с CommonJS.** Загрузчик анализирует CommonJS-файл статически, ищет «явные» экспорты (`exports.named = …`) и создаёт именованные экспорты; кроме них — `default` и (в Node.js 24) ключ `'module.exports'` (в опыте `[ 'default', 'module.exports', 'y' ]`). `require(esm)` синхронно связывает и вычисляет ESM-граф, поэтому невозможен при TLA; результат — пространство имён с `__esModule` (в опыте).
- **Разрешение путей.** Для ESM адрес должен быть полным (с расширением): загрузчик не подбирает расширения (в опыте `ERR_MODULE_NOT_FOUND` с подсказкой). Для пакетов Node.js читает `exports`/`imports` и условия (`import`, `require`, `default`).

> ⚠️ Проверить: точные детали статического анализа CommonJS (используемая библиотека, какие шаблоны распознаются), порядок параллельного вычисления при TLA, поведение `require(esm)` при TLA и условия `node`/`default`/`types` в `exports` в опытах не проверялись.

## Подвохи

### 1. `export *` не переносит `default`

Ключи реэкспорта: `[ 'PI', 'count', 'inc', 'main2', 'ns', 'pi' ]` — без `default` (в опыте).

### 2. Импорт — живая ссылка, но не переменная

Присваивание импорту — `TypeError`; чтобы изменить значение, экспортёр должен дать функцию (`inc()`).

### 3. Циклы и TDZ

Обращение к `const` цикла на верхнем уровне падает; функции работают.

### 4. Именованный импорт из CommonJS ненадёжен

`import { x } from './lib.cjs'` падает для `module.exports = { x }`; помогает импорт по умолчанию.

### 5. Двойная загрузка пакета

ESM- и CommonJS-входы — разные экземпляры: `w1 === w2` — `false` (в опыте).

### 6. `exports` скрывает всё остальное

Любой путь, не перечисленный в `exports`, закрыт (`ERR_PACKAGE_PATH_NOT_EXPORTED`), даже `package.json`, если его не перечислили.

### 7. Побочные эффекты при импорте

`console.log` в теле модуля срабатывает при первой загрузке (в опыте `counter.js evaluated`), даже если ничего не использовано.

### 8. `await` блокирует зависимых

Долгий `await` на верхнем уровне задерживает все импортирующие модули (в опыте `slow done` прежде `after slow`).

### 9. Динамический `import()` и пути

Адрес относителен модуля, в котором вызван `import()`; в CommonJS он относителен файла, а не `process.cwd()`.

> ⚠️ Проверить: подвох 9 про CommonJS взят из документации и не запускался.

## Компромиссы и альтернативы

| Подход | Плюсы | Минусы |
|---|---|---|
| ESM | стандарт, статический анализ, живые привязки, TLA | расширения в путях, нюансы с CommonJS |
| CommonJS | совместим со старым миром | динамический, нет TLA, не стандарт |
| Двойной пакет | работает везде | риск двух экземпляров |
| Пакет только ESM | один формат | CommonJS-потребители зависят от `require(esm)` (в Node.js 24.15 в опыте работает) |
| Сборщики (esbuild, Rollup, Vite) | единый файл, tree shaking, минификация | дополнительный шаг; не запускались |
| Динамический `import()` | ленивая загрузка, условные зависимости | асинхронный, затрудняет статический анализ |

## Вопросы с собеседований

<details><summary>1. Чем отличаются именованный экспорт и экспорт по умолчанию?</summary>

Именованных у модуля много, импортируются в `{}` по имени; `default` один, импортируется под любым именем без скобок.

→ [Базовая теория](basics.ru.md#export-и-import)

</details>

<details><summary>2. Что значит «живая привязка»?</summary>

Импортированное имя — ссылка на переменную экспортёра: изменения видны, а присваивание импорту запрещено.

→ [Внутреннее устройство](#внутреннее-устройство)

</details>

<details><summary>3. Сколько раз выполняется модуль, импортированный из пяти файлов?</summary>

Один раз: результат кэшируется по адресу файла.

→ [Базовая теория](basics.ru.md#export-и-import)

</details>

<details><summary>4. Ловушка: что вернёт <code>Object.keys(await import('./reexport.js'))</code> при <code>export *</code> из модуля с <code>default</code>?</summary>

Список без `default`: `export *` его не переносит.

→ [Подвох 1](#1-export--не-переносит-default)

</details>

<details><summary>5. Когда выполняются тела модулей и в каком порядке?</summary>

После построения и связывания графа; зависимости раньше зависящих; импорты «поднимаются».

→ [Базовая теория](basics.ru.md#порядок-выполнения)

</details>

<details><summary>6. Что происходит при циклической зависимости?</summary>

Граф связывается, но первый вычисляемый модуль цикла обращается к ещё не вычисленным именам: для `const`/`let`/`class` — `ReferenceError` (TDZ), для функций работает.

→ [Продвинутая теория](advanced.ru.md#циклические-зависимости)

</details>

<details><summary>7. Ловушка: <code>import { x } from './lib.cjs'</code> падает. Почему?</summary>

Именованные экспорты CommonJS определяются статическим анализом; для `module.exports = {…}` имя не распознано. Нужен `import lib from './lib.cjs'`.

→ [Подвох 4](#4-именованный-импорт-из-commonjs-ненадёжен)

</details>

<details><summary>8. Как использовать <code>require</code> в ESM?</summary>

`createRequire(import.meta.url)` из `node:module`.

→ [Продвинутая теория](advanced.ru.md#взаимодействие-с-commonjs)

</details>

<details><summary>9. Для чего поле <code>exports</code> в <code>package.json</code>?</summary>

Определяет публичные пути пакета и условия (`import`/`require`); остальные пути закрыты.

→ [Продвинутая теория](advanced.ru.md#packagejson-exports-и-imports)

</details>

<details><summary>10. Что такое dual package hazard?</summary>

Один пакет, загруженный и как ESM, и как CommonJS, даёт два независимых экземпляра со своим состоянием.

→ [Подвох 5](#5-двойная-загрузка-пакета)

</details>

<details><summary>11. Как работает <code>await</code> на верхнем уровне?</summary>

Модуль вычисляется асинхронно, а все импортирующие модули ждут его завершения.

→ [Подвох 8](#8-await-блокирует-зависимых)

</details>

<details><summary>12. Когда нужен динамический <code>import()</code>?</summary>

Ленивая загрузка, условные зависимости, адрес вычисляется во время работы.

→ [Базовая теория](basics.ru.md#динамический-import)

</details>

## Шпаргалка

```text
export const a / function f / class K ; export { a as b } ; export default x
export * from './m.js' (без default) ; export { default } from './m.js' ; export * as ns from './m.js'
import d, { a as b } from './m.js' ; import * as ns ; import './m.js' ; await import('./m.js')
import data from './x.json' with { type: 'json' }
import { createRequire } from 'node:module'  → require()
package.json: "exports": { ".": { "import": …, "require": … } }, "imports": { "#x": … }
```

- Модуль выполняется один раз; импорты живые.
- Расширения в путях обязательны; `node:` для встроенных.
- Для CommonJS — импорт по умолчанию.

## Связанные материалы

- Предыдущий уровень: [продвинутая теория](advanced.ru.md)
- Окружение и форматы: [01 · Окружение и инструменты](../01-environment/expert.ru.md)
- Области видимости и TDZ: [03 · Переменные и области видимости](../03-scopes-and-hoisting/expert.ru.md)

## Источники

- [ECMAScript: Modules](https://tc39.es/ecma262/#sec-modules)
- [Node.js: Packages](https://nodejs.org/docs/latest-v24.x/api/packages.html)
- [Node.js: ECMAScript modules — interoperability with CommonJS](https://nodejs.org/docs/latest-v24.x/api/esm.html#interoperability-with-commonjs)
- [MDN: import.meta](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Operators/import.meta)
- [MDN: Top-level await](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Operators/await#top_level_await)
