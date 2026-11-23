---
title: "Окружение и инструменты: экспертная теория"
lang: ru
track: javascript
kind: module
doc: expert
slug: 01-environment
part: 1
tags: [nodejs, v8, esm, commonjs, module-wrapper, package-json, cli-flags, exit-code]
versions: "Node.js 24.15 LTS (V8 13.6), npm 11.12"
status: draft
updated: 2026-11-23
---

# Окружение и инструменты: экспертная теория

🌐 **Русский** · [English](expert.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · [2. Продвинутая](advanced.ru.md) · **3. Экспертная** · [4. Квиз](quiz.ru.md)

Опирается на [базовую](basics.ru.md) и [продвинутую](advanced.ru.md) теорию. Что подтверждено запуском на Node.js 24.15 — помечено «в опыте».

## Справочник

### Флаги командной строки `node`

| Флаг | Назначение |
|---|---|
| `-v`, `--version` | версия Node.js (в опыте `v24.15.0`) |
| `-p`, `--print "выражение"` | выполнить и напечатать результат (в опыте) |
| `-e`, `--eval "код"` | выполнить код (в опыте) |
| `-i`, `--interactive` | открыть REPL (в опыте через pipe) |
| `--check файл` | проверить синтаксис, не запуская (в опыте) |
| `--test` | запустить встроенный тестовый раннер (в опыте) |
| `--run <скрипт>` | выполнить скрипт из `package.json` (в опыте) |
| `--env-file=<файл>` | загрузить переменные окружения (в опыте) |
| `--watch` | перезапуск при изменении файлов (есть в `--help`) |
| `--inspect` | включить отладчик |

> ⚠️ Проверить: `--watch`, `--inspect`, `--import`, `--require` и другие флаги, кроме отмеченных «в опыте», взяты из документации и не запускались. Полный список — `node --help` и [документация CLI](https://nodejs.org/docs/latest-v24.x/api/cli.html).

### Поля `package.json`

| Поле | Значение |
|---|---|
| `name`, `version` | идентификатор и версия пакета (в опыте имя — из папки, версия `1.0.0`) |
| `type` | `"commonjs"` (по умолчанию в `npm init -y` в опыте) или `"module"` |
| `main` | точка входа для CommonJS (в опыте — первый найденный файл) |
| `scripts` | именованные команды для `npm run` (в опыте `test`, `hello`, `hi`) |
| `dependencies`, `devDependencies` | зависимости |

> ⚠️ Проверить: `exports`, `imports`, `bin`, `engines`, `workspaces`, `files` описаны в документации npm и Node.js, но в опытах не использовались (`exports` разбирается в модуле 09 дорожной карты).

### Глобальные объекты и модули

| Что | Где |
|---|---|
| `process`, `console`, `globalThis` | везде (в опыте) |
| `fetch`, `structuredClone`, `queueMicrotask`, `navigator` | Node.js 24 (в опыте тип `function`/`object`) |
| `window`, `document` | только в браузере (в опыте `undefined`) |
| `require`, `module`, `__filename`, `__dirname` | только в CommonJS (в опыте) |
| `import.meta.filename`, `import.meta.dirname`, `import.meta.url` | только в ESM (в опыте) |

## Внутреннее устройство

- **V8 и libuv.** Node.js — это движок V8 (выполняет JavaScript; в опыте `process.versions.v8` = `13.6.233.17-node.48`) плюс собственная библиотека ввода-вывода и API (`fs`, `net`, `http`). Однопоточный код программы выполняется V8, а ввод-вывод обслуживается в фоне ([цикл событий](../../overview.ru.md), модуль 13 дорожной карты).
- **Обёртка CommonJS.** Файл `.cjs` выполняется как тело функции с параметрами `exports`, `require`, `module`, `__filename`, `__dirname` — в опыте `arguments.length` равен 5. Поэтому `var` не попадает в глобальный объект (в опыте `typeof globalThis.gv` равен `undefined`).
- **ESM-загрузчик.** Модули ES загружаются асинхронно: сначала строится граф зависимостей, затем код выполняется. Именно поэтому `await` на верхнем уровне допустим, а пути должны быть полными (с расширением): загрузчик не подбирает расширения (в опыте `ERR_MODULE_NOT_FOUND` с подсказкой `Did you mean to import "./lib.js"?`).
- **Определение формата.** Формат файла задаётся расширением (`.mjs`, `.cjs`) либо полем `type` ближайшего `package.json` для `.js`. Если `package.json` нет вовсе, действуют правила по умолчанию Node.js.
- **Атрибуты импорта.** JSON-модуль требует `with { type: 'json' }`; без атрибута динамический `import()` отклоняется кодом `ERR_IMPORT_ATTRIBUTE_MISSING` (в опыте).
- **Коды возврата.** `process.exitCode` читается при штатном завершении, `process.exit(n)` завершает немедленно (в опыте коды 3 и 7).

> ⚠️ Проверить: алгоритм определения формата при отсутствии `type` (обнаружение синтаксиса), взаимная загрузка `require(esm)` и `import cjs`, разрешение `exports` — по документации, запуском не проверялись.

## Подвохи

### 1. `npm init -y` создаёт `"type": "commonjs"`

Новый проект по умолчанию — CommonJS (в опыте с npm 11.12). Для ESM нужно поменять `type` вручную (`npm pkg set type=module`).

### 2. `"main"` подставляется «первым найденным файлом»

В опыте `main` стал `args.js`, потому что это первый `.js` в папке. Для приложений это обычно не имеет значения, для библиотек — важно.

### 3. `this` на верхнем уровне разный

ESM: `undefined`; CommonJS: `module.exports` (в опыте). Код, который использует `this` на верхнем уровне, ведёт себя по-разному.

### 4. Строгость зависит от формата

Один и тот же файл в `.js` (ESM) и `.cjs` ведёт себя по-разному: `undeclared = 5` — ошибка и тихая глобальная переменная (в опыте).

### 5. `--env-file` даёт только строки

`N=5` приходит как `'5'` (в опыте `GREETING + N` дало `'hi5'`). Числа и булевы значения нужно приводить явно.

### 6. `node -e` — глобальная область

`var` из `-e` попадает в `globalThis` (в опыте), а из файла — нет. Эксперименты в `-e` могут не повторяться в файле.

### 7. `process.exit()` обрывает вывод

Немедленный выход может потерять неотправленный вывод; штатно завершайте программу через `process.exitCode`.

## Компромиссы и альтернативы

| Подход | Плюсы | Минусы |
|---|---|---|
| Node.js + npm | де-факто стандарт, встроенные тесты и `--env-file` | один рантайм из нескольких |
| Deno, Bun | другой набор встроенных инструментов, свои менеджеры | не запускались |
| pnpm, yarn | быстрее или строже npm | не запускались |
| ESM | стандарт языка, `await` на верхнем уровне | расширения в путях, не все старые пакеты |
| CommonJS | совместим со старыми пакетами | нестандартный, нестрогий по умолчанию |
| Встроенный `node:test` | без зависимостей | меньше экосистемы, чем у vitest/jest |

## Вопросы с собеседований

<details><summary>1. Чем Node.js отличается от браузера как среда выполнения JavaScript?</summary>

Нет `window` и `document`, зато есть `process`, `fs` и модульная система Node.js. Движок тот же (V8 в Node.js и Chrome), но набор API разный.

→ [Базовая теория](basics.ru.md#три-способа-выполнить-код)

</details>

<details><summary>2. Как получить аргументы командной строки?</summary>

`process.argv`; первые два элемента — пути к Node.js и к скрипту, пользовательские — с индекса 2.

→ [Базовая теория](basics.ru.md#три-способа-выполнить-код)

</details>

<details><summary>3. Зачем нужен <code>package.json</code>?</summary>

Описывает проект: имя, версию, скрипты, зависимости и формат модулей (`type`).

→ [Базовая теория](basics.ru.md#packagejson)

</details>

<details><summary>4. Ловушка: <code>require is not defined in ES module scope</code>. Что случилось?</summary>

Файл читается как ESM (`"type": "module"` или `.mjs`), где `require` нет. Нужен `import` либо файл `.cjs`.

→ [Базовая теория](basics.ru.md#es-модули-и-commonjs)

</details>

<details><summary>5. Чем ESM отличается от CommonJS?</summary>

`import`/`export` вместо `require`/`module.exports`, обязательное расширение, `await` на верхнем уровне, всегда строгий режим, `this` равен `undefined`, нет `__dirname`.

→ [Продвинутая теория](advanced.ru.md#esm-и-commonjs-различия)

</details>

<details><summary>6. Ловушка: что такое <code>arguments.length</code> на верхнем уровне CommonJS-файла?</summary>

Пять — файл выполняется как функция с параметрами `exports`, `require`, `module`, `__filename`, `__dirname`.

→ [Внутреннее устройство](#внутреннее-устройство)

</details>

<details><summary>7. Как подключить JSON-файл в ESM?</summary>

`import data from './data.json' with { type: 'json' }`. Без атрибута — ошибка `ERR_IMPORT_ATTRIBUTE_MISSING`.

→ [Продвинутая теория](advanced.ru.md#импорт-файлов-и-json)

</details>

<details><summary>8. Чем <code>process.exitCode</code> отличается от <code>process.exit()</code>?</summary>

`exitCode` задаёт код и позволяет завершиться штатно, `exit()` обрывает процесс сразу.

→ [Продвинутая теория](advanced.ru.md#код-возврата-процесса)

</details>

<details><summary>9. Как запустить тесты без сторонних библиотек?</summary>

`node --test` (или `npm test` со скриптом), тесты на модулях `node:test` и `node:assert`.

→ [Базовая теория](basics.ru.md#скрипты-и-встроенные-возможности-nodejs)

</details>

<details><summary>10. Как загрузить переменные окружения из файла?</summary>

`node --env-file=.env`; значения — строки, файл с секретами не должен попадать в Git.

→ [Подвох 5](#5---env-file-даёт-только-строки)

</details>

<details><summary>11. Ловушка: <code>var x</code> в <code>node -e</code> попадает в <code>globalThis</code>, а в файле нет. Почему?</summary>

Код `-e` выполняется в глобальной области, а файл — внутри модуля (обёртка CommonJS или область модуля ESM).

→ [Подвох 6](#6-node--e--глобальная-область)

</details>

## Шпаргалка

```text
node -v ; npm -v
node -p "expr" ; node -e "code" ; node
node file.js args…            # process.argv.slice(2)
npm init -y ; npm pkg set type=module
npm test ; node --test ; node --run script
node --env-file=.env file.js
node --check file.js
process.exitCode = 1
```

- ESM: `import`, расширение обязательно, `await` на верхнем уровне, `import.meta.dirname`.
- CommonJS: `require`, `module.exports`, `__dirname`.
- Встроенные модули — с префиксом `node:`.

## Связанные материалы

- Предыдущий уровень: [продвинутая теория](advanced.ru.md)
- Следующий модуль: [02 · Типы и значения](../02-types-and-values/overview.ru.md)
- Подробно о модулях и тестировании — модули 09 и 15 дорожной карты [трека](../../overview.ru.md)

## Источники

- [Node.js: Command-line options](https://nodejs.org/docs/latest-v24.x/api/cli.html)
- [Node.js: Modules: Packages](https://nodejs.org/docs/latest-v24.x/api/packages.html)
- [Node.js: Process](https://nodejs.org/docs/latest-v24.x/api/process.html)
- [npm: package.json](https://docs.npmjs.com/cli/v11/configuring-npm/package-json)
- [V8](https://v8.dev/docs)
