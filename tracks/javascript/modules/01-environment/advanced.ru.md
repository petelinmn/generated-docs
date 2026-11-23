---
title: "Окружение и инструменты: продвинутая теория"
lang: ru
track: javascript
kind: module
doc: advanced
slug: 01-environment
part: 1
tags: [esm, commonjs, strict-mode, globals, json-import, exit-code, nodejs]
versions: "Node.js 24.15 LTS, npm 11.12"
status: draft
updated: 2026-11-23
---

# Окружение и инструменты: продвинутая теория

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · **2. Продвинутая** · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

Опирается на [базовую теорию](basics.ru.md). Все примеры выполнены на Node.js 24.15.

## ESM и CommonJS: различия

| Свойство | ESM (`import`) | CommonJS (`require`) |
|---|---|---|
| Файл | `.mjs` или `.js` при `"type": "module"` | `.cjs` или `.js` при `"type": "commonjs"` |
| Подключение | `import x from './x.js'` | `const x = require('./x')` |
| Расширение в пути | обязательно (`./lib.js`) | можно опустить |
| `await` на верхнем уровне | да | нет |
| Строгий режим | всегда | только если включён вручную |
| `this` на верхнем уровне | `undefined` | `module.exports` |
| `__filename`, `__dirname` | нет (`import.meta.filename`, `import.meta.dirname`) | есть |

Проверки из опытов. Для CommonJS-файла `wrap.cjs`:

```js
console.log(arguments.length, typeof __filename, __filename.endsWith('wrap.cjs'), require.main === module);
```

```text
$ node wrap.cjs
5 string true true
```

`arguments.length` равен 5: CommonJS-файл на самом деле выполняется как тело функции с пятью параметрами (`exports`, `require`, `module`, `__filename`, `__dirname`). ESM такой обёртки нет, поэтому на верхнем уровне `this` — `undefined`, а не `module.exports`.

## Строгий режим

В ESM строгий режим включён всегда. В нестрогом коде некоторые ошибки молча игнорируются:

```js
// strict.js (ESM) и strict.cjs (CommonJS) — один и тот же код
try { undeclared = 5; } catch (e) { console.log(e.name + ': ' + e.message); }
```

```text
$ node strict.js
ReferenceError: undeclared is not defined
$ node strict.cjs
(ничего не выведено: присваивание создало глобальную переменную)
```

Ещё один пример — присваивание символу строки ([модуль 02](../02-types-and-values/basics.ru.md#строки)): в строгом режиме это `TypeError`, в нестрогом `node -e` молча ничего не делает. Строгий режим можно включить и в CommonJS директивой `'use strict';` первой строкой.

## Глобальные объекты Node.js

`globalThis` — стандартное имя глобального объекта и в браузере, и в Node.js. В Node.js есть собственные глобальные объекты и часть веб-API:

```text
$ node -e "console.log(typeof structuredClone, typeof queueMicrotask, typeof fetch, typeof window, typeof document, typeof navigator)"
function function function undefined undefined object
```

`fetch`, `structuredClone`, `queueMicrotask` и `navigator` есть, а `window` и `document` — нет. Модули встроенных библиотек подключают с префиксом `node:` (`import fs from 'node:fs'`); префикс отличает их от пакетов npm.

Область видимости скрипта различается: код из `node -e` выполняется в глобальной области, поэтому там `var gv = 1` создаёт свойство глобального объекта (`globalThis.gv === 1`), а `let gl` — нет (`undefined`). В файле (ESM или CommonJS) переменные верхнего уровня принадлежат модулю, а не глобальному объекту ([модуль 03](../03-scopes-and-hoisting/overview.ru.md)).

## Импорт файлов и JSON

В ESM расширение обязательно:

```js
import { x } from './lib';
```

```text
Error [ERR_MODULE_NOT_FOUND]: Cannot find module '…/js01/lib' imported from …/js01/noext.js
Did you mean to import "./lib.js"?
```

JSON подключается с атрибутом `type: 'json'`; без него импорт отклоняется:

```js
import data from './data.json' with { type: 'json' };
console.log(data);
import('./data.json').catch(e => console.log(e.code));
```

```text
{ a: 1 }
ERR_IMPORT_ATTRIBUTE_MISSING
```

## Код возврата процесса

Процесс сообщает системе результат числом: `0` — успех, ненулевое значение — ошибка. Его читают скрипты и CI.

```js
// exitcode.js
process.exitCode = 3;
console.log('done');
```

```text
$ node exitcode.js ; echo "exit=$?"
done
exit=3
$ node -e "process.exit(7)" ; echo "exit=$?"
exit=7
```

`process.exitCode` задаёт код, но позволяет программе завершиться штатно (дописать буферы вывода), а `process.exit(n)` завершает процесс немедленно. Первый вариант предпочтительнее.

## Проверка синтаксиса и другие флаги

`node --check файл.js` проверяет синтаксис без запуска:

```text
$ node --check lib.js && echo check-ok
check-ok
$ node --check bad.js        # const = ;
SyntaxError: Unexpected token '='
```

`node --watch` перезапускает скрипт при изменении файлов (в опыте проверено лишь наличие флага в `node --help`).

> ⚠️ Проверить: `node --watch`, `node --inspect` (отладчик Chrome DevTools), менеджеры версий `nvm`/`fnm`, поле `engines` и `npx` в опытах не запускались.

## Лучшие практики

- Новый проект — `"type": "module"`, импорты с расширением, встроенные модули с префиксом `node:`.
- Коды возврата: `process.exitCode`, а не `process.exit()` в обычном коде.
- Версии: фиксировать LTS-ветку Node.js для проекта (поле `engines` в `package.json`, файл `.nvmrc`).
- Тесты: `node --test` без лишних зависимостей, пока их возможностей хватает.
- Конфигурация: переменные окружения, `.env` не в Git.

## Частые ошибки

- ❌ Смешивать `import` и `require` в одном файле → ✅ выбрать один формат на файл; `require` в ESM — ошибка.
- ❌ Опустить расширение в ESM-импорте → ✅ писать `./lib.js`.
- ❌ Читать `__dirname` в ESM → ✅ `import.meta.dirname`.
- ❌ Звать `process.exit()` в середине записи в stdout → ✅ `process.exitCode` и естественное завершение.
- ❌ Ждать глобальную переменную от `var` в файле → ✅ это переменная модуля.

## Связанные материалы

- Дальше: [экспертная теория](expert.ru.md)
- Типы значений и строгий режим на примере строк: [02 · Типы и значения](../02-types-and-values/advanced.ru.md)
- Область видимости: [03 · Переменные и области видимости](../03-scopes-and-hoisting/overview.ru.md)

## Источники

- [Node.js: Modules: CommonJS modules](https://nodejs.org/docs/latest-v24.x/api/modules.html)
- [Node.js: Modules: ECMAScript modules](https://nodejs.org/docs/latest-v24.x/api/esm.html)
- [Node.js: Process exit codes](https://nodejs.org/docs/latest-v24.x/api/process.html#exit-codes)
- [Node.js: Globals](https://nodejs.org/docs/latest-v24.x/api/globals.html)
- [MDN: Strict mode](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Strict_mode)
