---
title: "Упражнения: модули"
lang: ru
track: javascript
kind: module
doc: exercises
slug: 09-modules
part: 2
tags: [esm, import, export, re-export, dynamic-import, circular-dependencies, package-exports]
status: draft
updated: 2026-11-23
---

# Упражнения: модули

🌐 **Русский** · [English](exercises.en.md)

> Решайте самостоятельно, подсказки и решения под спойлерами. Работайте в пустой папке с `package.json` (`{"type": "module"}`).

## 1. 🟢 Живая привязка

**Задача:** создайте `state.js` с экспортом `n` (по умолчанию `0`) и функцией `inc()`. В `ex1.js` выведите `n`, вызовите `inc()` дважды, выведите `n` снова и попробуйте присвоить `n = 10`.

**Ожидаемый результат:** `0`, `2`, `TypeError: Assignment to constant variable.`

<details><summary>Подсказка</summary>

`export let n = 0;` и `export const inc = () => ++n;`.

</details>

<details><summary>Решение</summary>

```js
// state.js
export let n = 0;
export const inc = () => ++n;
// ex1.js
import { n, inc } from './state.js';
console.log(n); inc(); inc(); console.log(n);
try { n = 10 } catch (e) { console.log(e.name + ': ' + e.message) }
```

```text
0
2
TypeError: Assignment to constant variable.
```

</details>

## 2. 🟢 Файл-вход (barrel)

**Задача:** есть `math.js` (`add`, `PI`, класс по умолчанию `Counter`) и `text.js` (`upper`). Соберите в `index.js` публичный API: все именованные экспорты обоих файлов и `Counter` как именованный экспорт. Выведите ключи и проверьте, нет ли среди них `default`.

**Ожидаемый результат:** `[ 'Counter', 'PI', 'add', 'upper' ] true false`.

<details><summary>Подсказка</summary>

`export * from …` и `export { default as Counter } from './math.js'`.

</details>

<details><summary>Решение</summary>

```js
// index.js
export * from './math.js';
export * from './text.js';
export { default as Counter } from './math.js';
// ex2.js
import * as api from './index.js';
import Counter from './math.js';
console.log(Object.keys(api), api.Counter === Counter, 'default' in api);
```

```text
[ 'Counter', 'PI', 'add', 'upper' ] true false
```

</details>

## 3. 🟡 Ленивая загрузка

**Задача:** модуль `heavy.js` печатает `heavy evaluated` при загрузке и экспортирует по умолчанию функцию. Загружайте его только по требованию через `import()` и убедитесь, что повторный вызов не выполняет модуль заново.

**Ожидаемый результат:** `before`, `heavy evaluated`, `heavy result`, `true`.

<details><summary>Подсказка</summary>

`const load = () => import('./heavy.js')`; сравните два результата.

</details>

<details><summary>Решение</summary>

```js
const load = () => import('./heavy.js');
console.log('before');
const a = await load(); console.log(a.default());
const b = await load(); console.log(a === b);
```

```text
before
heavy evaluated
heavy result
true
```

</details>

## 4. 🟡 Разрыв цикла

**Задача:** `ca2.js` и `cb2.js` импортируют друг друга. `ca2.js` экспортирует `const a = 'A'`, а `cb2.js` должен давать доступ к `a`, не падая на TDZ. Перепишите `cb2.js` так, чтобы значение читалось позже (через функцию `getA`).

**Ожидаемый результат:** `cb2 loaded`, затем `ca2: A`.

<details><summary>Подсказка</summary>

Вместо чтения `a` на верхнем уровне `cb2.js` экспортируйте `export const getA = () => a;`.

</details>

<details><summary>Решение</summary>

```js
// ca2.js
import { getA } from './cb2.js';
export const a = 'A';
console.log('ca2:', getA());
// cb2.js
import { a } from './ca2.js';
export const getA = () => a;
console.log('cb2 loaded');
```

```text
$ node ca2.js
cb2 loaded
ca2: A
```

</details>

## 5. 🔴 Публичные пути пакета

**Задача:** создайте в `node_modules/mylib` пакет с `exports`, где `.` ведёт на `lib/index.js` (для `import`) и `lib/index.cjs` (для `require`), а `./util` — на `lib/util.js`. Закройте остальное. Покажите, что `import('mylib/lib/hidden.js')` отклоняется, и что ESM и CommonJS получают разные экземпляры.

**Ожидаемый результат:** `esm internal util`, `cjs`, `ERR_PACKAGE_PATH_NOT_EXPORTED` для закрытых путей, `esm cjs false` для сравнения.

<details><summary>Подсказка</summary>

`"exports": { ".": { "import": "./lib/index.js", "require": "./lib/index.cjs" }, "./util": "./lib/util.js" }`; из ESM вызовите `createRequire(import.meta.url)('mylib')`.

</details>

<details><summary>Решение</summary>

Манифест, файлы и вывод — в [продвинутой теории](advanced.ru.md#packagejson-exports-и-imports) и [здесь](advanced.ru.md#dual-package-hazard).

</details>
