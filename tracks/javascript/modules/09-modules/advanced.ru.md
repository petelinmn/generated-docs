---
title: "Модули: продвинутая теория"
lang: ru
track: javascript
kind: module
doc: advanced
slug: 09-modules
part: 2
tags: [circular-dependencies, commonjs-interop, require-esm, package-exports, subpath-imports, dual-package-hazard, createrequire, node-builtins]
versions: "ECMAScript 2025+, Node.js 24.15 LTS"
status: draft
updated: 2026-11-23
---

# Модули: продвинутая теория

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · **2. Продвинутая** · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

Опирается на [базовую теорию](basics.ru.md). Все примеры выполнены на Node.js 24.15; файлы лежат в одной папке с `package.json` (`"type": "module"`).

## Ошибки импорта

Импорт имени, которого нет, — ошибка на этапе связывания, до выполнения кода:

```js
import { nothing } from './counter.js';
```

```text
SyntaxError: The requested module './counter.js' does not provide an export named 'nothing'
```

Это отличие от динамического `import()` и CommonJS: опечатку в имени видно сразу при запуске, а не при первом обращении.

## Циклические зависимости

```js
// ca.js
import { b } from './cb.js'; export const a = 'A'; console.log('ca sees b =', b);
// cb.js
import { a } from './ca.js'; export const b = 'B';
try { console.log('cb sees a =', a) } catch (e) { console.log('cb:', e.name + ': ' + e.message) }
```

```text
$ node ca.js
cb: ReferenceError: Cannot access 'a' before initialization
ca sees b = B
```

`ca.js` запускает `cb.js` первым, а тот обращается к `a`, который ещё не вычислен: имя находится в TDZ ([модуль 03](../03-scopes-and-hoisting/basics.ru.md#временная-мёртвая-зона-tdz)). Функции-объявления доступны сразу, а `const`/`let`/`class` — нет. Цикл лучше разорвать: вынести общее в третий модуль или обращаться к значению позже (внутри функции).

## Взаимодействие с CommonJS

Из ESM можно импортировать CommonJS-файлы:

```js
// lib.cjs
module.exports = { x: 1, f() { return 'cjs'; } }; module.exports.y = 2;
// named.cjs
exports.named = 1; exports.other = 2;
```

```js
import lib from './lib.cjs';
import * as nsC from './lib.cjs';
import { named, other } from './named.cjs';
console.log(lib, Object.keys(nsC), named, other);
import { createRequire } from 'node:module';
const require = createRequire(import.meta.url);
console.log(require('./lib.cjs').f());
```

```text
{ x: 1, f: [Function: f], y: 2 } [ 'default', 'module.exports', 'y' ] 1 2
cjs
```

- **Импорт по умолчанию** (`import lib from './lib.cjs'`) всегда возвращает `module.exports` целиком.
- **Именованные импорты** CommonJS работают, только если Node.js смог найти имена статическим анализом (`exports.named = …`). Для `module.exports = { x: 1, … }` имя `x` не определено: `import { x } from './lib.cjs'` падает с `SyntaxError: Named export 'x' not found. The requested module './lib.cjs' is a CommonJS module, which may not support all module.exports as named exports.` (в опыте), а в пространстве имён видны только `default`, `module.exports` и найденное `y`.
- **`createRequire(import.meta.url)`** возвращает функцию `require` внутри ESM — нужна для загрузки CommonJS и JSON старым способом.

### require() для ESM

В Node.js 24 CommonJS-файл может загрузить ES-модуль через `require`:

```js
// req.cjs
const m = require('./counter.js');
console.log(Object.keys(m));
```

```text
counter.js evaluated
[ 'PI', '__esModule', 'count', 'default', 'inc' ]
```

Результат — объект пространства имён с ключом `__esModule`. Это работает, если у ES-модуля нет `await` на верхнем уровне.

> ⚠️ Проверить: поведение `require(esm)` для модулей с `await` на верхнем уровне и в более старых версиях Node.js (флаг) в опытах не запускалось; по документации в таком случае бросается ошибка.

## package.json: exports и imports

Поле `exports` определяет **публичные** пути пакета и условия для `import` и `require`:

```json
{
  "name": "mylib", "type": "module",
  "exports": {
    ".": { "import": "./lib/index.js", "require": "./lib/index.cjs" },
    "./util": "./lib/util.js",
    "./package.json": "./package.json"
  },
  "imports": { "#int": "./internal/int.js" }
}
```

Пакет в `node_modules/mylib`; потребитель:

```js
import { which, s } from 'mylib';
import { util } from 'mylib/util';
console.log(which, s, util);
for (const spec of ['mylib/lib/hidden.js', 'mylib/internal/int.js']) { try { await import(spec) } catch (e) { console.log(e.code) } }
```

```text
esm internal util
ERR_PACKAGE_PATH_NOT_EXPORTED
ERR_PACKAGE_PATH_NOT_EXPORTED
```

- Условия `import` и `require` выбирают файл по способу загрузки: из ESM — `lib/index.js` (`which` равен `esm`), из CommonJS — `lib/index.cjs` (`cjs`).
- Внутренние файлы (`lib/hidden.js`, `internal/int.js`) **закрыты**: попытка импорта — `ERR_PACKAGE_PATH_NOT_EXPORTED`, даже если файл физически существует.
- **`imports`** задаёт «внутренние» псевдонимы пакета, которые начинаются с `#`: внутри пакета `import { secret } from '#int'` работает (в опыте), а снаружи эти имена по документации недоступны (не проверялось). В собственном проекте то же самое:

```text
// package.json: { "imports": { "#cnt": "./counter.js" } }
import { PI } from '#cnt'; console.log(PI);   // 3.14
```

### Dual package hazard

Если пакет поставляет и ESM, и CommonJS, один и тот же код может загрузиться **дважды** как два разных экземпляра:

```js
import { which as w1 } from 'mylib';
import { createRequire } from 'node:module';
const w2 = createRequire(import.meta.url)('mylib').which;
console.log(w1, w2, w1 === w2);
```

```text
esm cjs false
```

Разные файлы — разные модули со своим состоянием (синглтоны, кэши, `instanceof`). Чтобы избежать расхождений, пакет делают «чисто ESM» или оборачивают CommonJS-версию вокруг ESM.

## Встроенные модули и JSON

```js
import fs from 'node:fs';
import { readFileSync } from 'fs';
import * as p from 'node:path';
console.log(typeof fs.readFileSync, fs.readFileSync === readFileSync, typeof p.join, Object.keys(p).includes('default'));
```

```text
function true function true
```

- Встроенные модули доступны с префиксом `node:` и без него; префикс отличает их от пакетов npm.
- У встроенных модулей есть и `default`, и именованные экспорты (`fs` и `readFileSync` — одно значение).
- JSON импортируется с атрибутом `with { type: 'json' }` ([модуль 01](../01-environment/advanced.ru.md#импорт-файлов-и-json)).

## Ленивая загрузка

```js
const { default: main } = await import('./counter.js');
console.log(main(), 'loaded lazily');
```

```text
default loaded lazily
```

Динамический `import()` откладывает загрузку тяжёлых модулей до момента использования (редко используемые команды CLI, большие библиотеки).

## Лучшие практики

- Новый код — ESM; расширения в путях; встроенные модули с `node:`.
- Один файл-вход (`index.js`) с реэкспортом публичного API; детали не экспортируйте.
- Для пакета — `exports` с явным списком публичных путей.
- Разрывайте циклы: общее — в отдельный модуль.
- Избегайте побочных эффектов при импорте (запросы, запись в файлы): они выполняются один раз и неожиданно.
- Не публикуйте двойной пакет без необходимости.

## Частые ошибки

- ❌ `import { x } from './lib.cjs'` для `module.exports = {…}` → ✅ импорт по умолчанию и деструктуризация.
- ❌ Импортировать закрытый путь пакета → ✅ публичный из `exports`.
- ❌ Читать значение из циклической зависимости на верхнем уровне → ✅ читать позже, внутри функции.
- ❌ Смешивать `__dirname` и ESM → ✅ `import.meta.dirname`.
- ❌ Публиковать пакет с обоими форматами и общим состоянием → ✅ единый формат.

## Связанные материалы

- Дальше: [экспертная теория](expert.ru.md)
- Окружение и форматы: [01 · Окружение и инструменты](../01-environment/advanced.ru.md)
- `tsconfig` и модули в TypeScript: [TypeScript](../../../typescript/overview.ru.md)

## Источники

- [Node.js: Packages (`exports`, `imports`)](https://nodejs.org/docs/latest-v24.x/api/packages.html)
- [Node.js: Modules: CommonJS modules](https://nodejs.org/docs/latest-v24.x/api/modules.html)
- [Node.js: Loading ECMAScript modules using `require()`](https://nodejs.org/docs/latest-v24.x/api/modules.html#loading-ecmascript-modules-using-require)
- [Node.js: `module.createRequire`](https://nodejs.org/docs/latest-v24.x/api/module.html#modulecreaterequirefilename)
- [MDN: Circular imports](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Guide/Modules#circular_imports)
