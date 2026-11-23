---
title: "Окружение и инструменты: базовая теория"
lang: ru
track: javascript
kind: module
doc: basics
slug: 01-environment
part: 1
tags: [nodejs, npm, package-json, repl, process-argv, esm, node-test, env-file]
versions: "Node.js 24.15 LTS, npm 11.12"
status: draft
updated: 2026-11-23
---

# Окружение и инструменты: базовая теория

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [обзор](overview.ru.md) · **1. Базовая** · [2. Продвинутая](advanced.ru.md) · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** терминал и текстовый редактор · **Время:** ~1,5 ч

Все примеры выполнены на Node.js 24.15 и npm 11.12 (Windows, Git Bash). На других системах вывод пути и разделителей может отличаться.

## Зачем это нужно

Любой код нужно где-то выполнять. В JavaScript есть два основных «хозяина»: браузер и Node.js. Node.js проще для старта: ему не нужны HTML-страница и сервер, достаточно файла и терминала. Все примеры этого трека запускаются в Node.js.

## Проверка установки

```text
$ node -v
v24.15.0
$ npm -v
11.12.1
```

Node.js ставится установщиком с nodejs.org или менеджером версий (`nvm`, `fnm`); npm приходит вместе с ним. Для обучения берите версию LTS (долгосрочной поддержки).

## Три способа выполнить код

**Выражение из командной строки.** `-p` печатает результат, `-e` только выполняет:

```text
$ node -p "1 + 1"
2
$ node -e "console.log(typeof window, typeof globalThis, typeof process)"
undefined object object
```

Здесь видно главное отличие от браузера: `window` не определён, зато есть `globalThis` и `process`.

**Файл.** Создайте `args.js`:

```js
console.log(process.argv.slice(2));
console.log(typeof require, typeof module);
```

```text
$ node args.js a --flag=1 "b c"
[ 'a', '--flag=1', 'b c' ]
function object
```

`process.argv` — массив аргументов: в позициях 0 и 1 лежат пути к Node.js и к скрипту, поэтому `slice(2)` оставляет только пользовательские. Аргумент в кавычках (`"b c"`) приходит одной строкой.

**REPL (интерактивная консоль).** Команда `node` без аргументов открывает приглашение `>`, где каждое выражение сразу выполняется и печатается:

```text
$ node
> [1, 2, 3].map(n => n * 2)
[ 2, 4, 6 ]
> .exit
```

Вывод `Welcome to Node.js v24.15.0.` появляется при запуске. REPL удобен для быстрых проверок.

## package.json

Файл `package.json` описывает проект. Создадим его командой `npm init -y` (ответы по умолчанию):

```json
{
  "name": "js01",
  "version": "1.0.0",
  "description": "",
  "main": "args.js",
  "scripts": {
    "test": "echo \"Error: no test specified\" && exit 1"
  },
  "keywords": [],
  "author": "",
  "license": "ISC",
  "type": "commonjs"
}
```

Имя берётся из имени папки, `main` — из найденного файла. Главные поля:

- `scripts` — именованные команды, запуск `npm run <имя>`; `npm test` — сокращение для `npm run test`.
- `type` — формат `.js`-файлов: `"commonjs"` (так в этом опыте) или `"module"`.
- `dependencies`/`devDependencies` — зависимости, которые появляются после `npm install <пакет>` (в этом опыте пакеты не ставились).

## ES-модули и CommonJS

Включим современный формат модулей: `npm pkg set type=module` изменяет `package.json`. Теперь `.js` — это модуль ES:

```js
// esm.js
import { argv } from 'node:process';
console.log(typeof require, typeof module);
console.log(import.meta.filename.endsWith('esm.js'), import.meta.dirname === process.cwd());
const r = await Promise.resolve('top-level await works');
console.log(r);
console.log(this);
```

```text
$ node esm.js
undefined undefined
true true
top-level await works
undefined
```

- `require` и `module` в ESM не определены.
- `import.meta.filename` и `import.meta.dirname` дают путь к файлу и его папке.
- `await` работает на верхнем уровне файла без обёртки `async`.
- `this` на верхнем уровне модуля равен `undefined`.

Если в таком проекте написать `const fs = require('node:fs')`, получится ошибка:

```text
ReferenceError: require is not defined in ES module scope, you can use import instead
```

Для старого формата можно переименовать файл в `.cjs`: он всегда читается как CommonJS, и `require` в нём работает. Файл `.mjs` всегда читается как ESM. Подробнее о различиях — в [продвинутой теории](advanced.ru.md#esm-и-commonjs-различия).

## Скрипты и встроенные возможности Node.js

Скрипты в `package.json` и встроенный тестовый раннер:

```js
// tests/a.test.js
import { test } from 'node:test';
import assert from 'node:assert/strict';
test('sum', () => { assert.equal(1 + 1, 2); });
```

```text
$ npm pkg set scripts.test="node --test"
$ npm test
> js01@1.0.0 test
> node --test

✔ sum (0.5892ms)
ℹ tests 1
ℹ pass 1
ℹ fail 0
```

`node --test` сам находит файлы `*.test.js` и в папках `test`/`tests` и запускает их. Другие полезные встроенные возможности:

```text
$ node --run hi          # запустить скрипт hi из package.json без npm
hi from script
$ printf 'GREETING=hi\nN=5\n' > .env
$ node --env-file=.env -p "process.env.GREETING + process.env.N"
hi5
```

`--env-file` загружает переменные окружения из файла. Значения всегда строки: `N=5` приходит как `'5'`. Файл `.env` с секретами нельзя добавлять в Git.

## Ошибки новичка

- ❌ Писать `require` в проекте с `"type": "module"` → ✅ использовать `import` или файл `.cjs`.
- ❌ Ждать, что `window` или `document` есть в Node.js → ✅ это браузерные объекты.
- ❌ Забывать расширение в импорте (`'./lib'`) → ✅ в ESM нужно `'./lib.js'` (подробнее в [продвинутой теории](advanced.ru.md#импорт-файлов-и-json)).
- ❌ Считать `process.argv[0]` первым аргументом пользователя → ✅ пользовательские начинаются с индекса 2.
- ❌ Коммитить `.env` → ✅ добавлять в `.gitignore`.

## Связанные материалы

- Дальше: [продвинутая теория](advanced.ru.md) — различия ESM и CommonJS, импорт, коды возврата
- Следующий модуль: [02 · Типы и значения](../02-types-and-values/overview.ru.md)
- Сравнение: [Python и uv](../../../python/overview.ru.md) — аналогичный набор инструментов

## Источники

- [Node.js: Command-line API](https://nodejs.org/docs/latest-v24.x/api/cli.html)
- [Node.js: Modules: ECMAScript modules](https://nodejs.org/docs/latest-v24.x/api/esm.html)
- [Node.js: Test runner](https://nodejs.org/docs/latest-v24.x/api/test.html)
- [Node.js: Process (`process.argv`)](https://nodejs.org/docs/latest-v24.x/api/process.html#processargv)
- [npm: package.json](https://docs.npmjs.com/cli/v11/configuring-npm/package-json)
