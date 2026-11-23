---
title: "Модули: базовая теория"
lang: ru
track: javascript
kind: module
doc: basics
slug: 09-modules
part: 2
tags: [esm, import, export, live-bindings, namespace-object, dynamic-import, top-level-await, re-export]
versions: "ECMAScript 2025+, Node.js 24.15 LTS"
status: draft
updated: 2026-11-23
---

# Модули: базовая теория

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [обзор](overview.ru.md) · **1. Базовая** · [2. Продвинутая](advanced.ru.md) · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [01 · Окружение и инструменты](../01-environment/overview.ru.md), [03 · Переменные и области видимости](../03-scopes-and-hoisting/overview.ru.md) · **Время:** ~2,5 ч

Все примеры выполнены на Node.js 24.15; в папке опыта `package.json` содержит `{"type":"module"}`.

## Зачем это нужно

Без модулей весь код лежит в одной общей области имён, где легко затереть чужую переменную. Модуль даёт собственную область видимости и явные зависимости: сразу видно, что от чего зависит.

## export и import

Файл `counter.js`:

```js
export let count = 0;
export function inc() { count++; }
export const PI = 3.14;
export default function main() { return 'default'; }
console.log('counter.js evaluated');
```

Файл `a.js` использует его и меняет состояние:

```js
import { count, inc } from './counter.js';
console.log('A sees', count); inc(); console.log('A after inc', count);
export const fromA = 1;
```

Файл `main.js`:

```js
import * as ns from './counter.js';
import main, { PI, count as c } from './counter.js';
import './a.js';
console.log(Object.keys(ns), typeof ns.default, main(), PI, c);
console.log(Object.isFrozen(ns), Object.prototype.toString.call(ns), Object.getPrototypeOf(ns));
try { ns.PI = 3 } catch (e) { console.log(e.name + ': ' + e.message) }
try { c = 5 } catch (e) { console.log(e.name + ': ' + e.message) }
```

```text
$ node main.js
counter.js evaluated
A sees 0
A after inc 1
[ 'PI', 'count', 'default', 'inc' ] function default 3.14 1
false [object Module] null
TypeError: Cannot assign to read only property 'PI' of object '[object Module]'
TypeError: Assignment to constant variable.
```

Что здесь видно:

- **Именованный экспорт** (`export const PI`) импортируется в фигурных скобках по имени, с возможным переименованием (`count as c`). **Экспорт по умолчанию** (`export default`) один на модуль и импортируется без скобок под любым именем (`main`).
- **Модуль выполняется один раз:** `counter.js evaluated` напечатано единожды, хотя файл импортируют и `main.js`, и `a.js`.
- **Привязки живые:** `a.js` вызвал `inc()`, и `main.js` увидел `c`, равное `1` (хотя импорт в `a.js` был сделан до этого). Импорт — ссылка на переменную экспортёра.
- **Присвоить импорту нельзя:** `c = 5` — `TypeError: Assignment to constant variable.`; `ns.PI = 3` — тоже ошибка.
- **Пространство имён** `ns` (`import * as ns`) — объект с модульной «меткой» `[object Module]`, без прототипа (`null`); ключи отсортированы по порядку кодов (`'PI'` перед `'count'`), а `default` — обычное имя `default`.
- **Побочный импорт** `import './a.js'` только выполняет модуль, ничего не импортируя.

## Порядок выполнения

Импорты обрабатываются до тела модуля, независимо от места в файле:

```js
// order.js
console.log('1 main body');
import './side.js';
console.log('2 after import statement');
```

```text
$ node order.js
side evaluated first
1 main body
2 after import statement
```

Зависимость `side.js` выполнилась **первой**, хотя `import` написан ниже первой строки. Поэтому модуль может безопасно использовать импортированное в любом месте.

## Реэкспорт

```js
// reexport.js
export * from './counter.js';
export { default as main2, PI as pi } from './counter.js';
export * as ns from './counter.js';
```

```text
$ node re.js        # import * as r from './reexport.js'; console.log(Object.keys(r))
counter.js evaluated
[ 'PI', 'count', 'inc', 'main2', 'ns', 'pi' ]
```

- `export * from` переэкспортирует все **именованные** экспорты — но **не `default`** (его нет в списке).
- `export { default as main2 }` переэкспортирует `default` под другим именем; `export * as ns` — всё пространство имён одним объектом.
- Реэкспорт удобен для «файлов-входов» (`index.js`), которые собирают API пакета.

## Динамический import()

`import()` — выражение, возвращающее промис пространства имён; адрес может вычисляться во время работы:

```js
// dyn.js
const m = await import('./counter.js');
console.log(Object.keys(m), m.default.name);
const again = await import('./counter.js');
console.log(m === again);
const name = './side.js'; await import(name);
try { await import('./nope.js') } catch (e) { console.log(e.code) }
console.log(import.meta.url.startsWith('file:'), typeof import.meta.resolve, import.meta.resolve('./counter.js').endsWith('counter.js'));
```

```text
$ node dyn.js
counter.js evaluated
[ 'PI', 'count', 'default', 'inc' ] main
true
side evaluated first
ERR_MODULE_NOT_FOUND
true function true
```

- Повторный `import()` возвращает **тот же** объект (`m === again` — `true`): модуль в кэше.
- Адрес может быть переменной; несуществующий файл — отклонённый промис с кодом `ERR_MODULE_NOT_FOUND`.
- Применение: ленивая загрузка тяжёлых библиотек и условные зависимости.
- `import.meta.url` — адрес текущего файла, `import.meta.resolve(...)` — адрес указанного модуля.

## await на верхнем уровне

В ESM `await` можно писать прямо в модуле. Импортирующие модули ждут его завершения:

```js
// slow.js
console.log('slow start'); await new Promise(r => setTimeout(r, 50)); console.log('slow done'); export const v = 1;
// tla.js
import './slow.js'; console.log('after slow');
```

```text
$ node tla.js
slow start
slow done
after slow
```

`after slow` напечатано только после того, как `slow.js` дождался таймера. Из-за этого `await` на верхнем уровне замедляет старт всех зависящих от модуля файлов: используйте его осторожно.

## Ошибки новичка

- ❌ Импортировать без расширения (`'./counter'`) → ✅ `'./counter.js'`.
- ❌ Присваивать импортированному имени → ✅ вызвать функцию экспортёра, которая меняет значение.
- ❌ Ожидать, что `export *` перенесёт `default` → ✅ `export { default } from …`.
- ❌ Ждать копию значения вместо живой ссылки → ✅ помнить, что `count` обновляется.
- ❌ Ставить `import` внутри `if` или функции → ✅ динамический `import()`.
- ❌ Много `await` на верхнем уровне → ✅ выносить ленивую загрузку в функции.

## Связанные материалы

- Дальше: [продвинутая теория](advanced.ru.md) — циклы, CommonJS, `exports`
- Ранее: [01 · Окружение и инструменты](../01-environment/advanced.ru.md#esm-и-commonjs-различия)

## Источники

- [MDN: JavaScript modules](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Guide/Modules)
- [MDN: import](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Statements/import)
- [MDN: export](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Statements/export)
- [MDN: Dynamic import()](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Operators/import)
- [Node.js: ECMAScript modules](https://nodejs.org/docs/latest-v24.x/api/esm.html)
