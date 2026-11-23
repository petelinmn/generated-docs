---
title: "Exercises: modules"
lang: en
track: javascript
kind: module
doc: exercises
slug: 09-modules
part: 2
tags: [esm, import, export, re-export, dynamic-import, circular-dependencies, package-exports]
status: draft
updated: 2026-11-23
---

# Exercises: modules

🌐 [Русский](exercises.ru.md) · **English**

> Solve on your own; hints and solutions are under spoilers. Work in an empty folder with `package.json` (`{"type": "module"}`).

## 1. 🟢 A live binding

**Task:** create `state.js` exporting `n` (initially `0`) and a function `inc()`. In `ex1.js` print `n`, call `inc()` twice, print `n` again and try to assign `n = 10`.

**Expected result:** `0`, `2`, `TypeError: Assignment to constant variable.`

<details><summary>Hint</summary>

`export let n = 0;` and `export const inc = () => ++n;`.

</details>

<details><summary>Solution</summary>

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

## 2. 🟢 An entry file (barrel)

**Task:** there is `math.js` (`add`, `PI`, a default class `Counter`) and `text.js` (`upper`). Assemble the public API in `index.js`: all named exports of both files and `Counter` as a named export. Print the keys and check that `default` is not among them.

**Expected result:** `[ 'Counter', 'PI', 'add', 'upper' ] true false`.

<details><summary>Hint</summary>

`export * from …` and `export { default as Counter } from './math.js'`.

</details>

<details><summary>Solution</summary>

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

## 3. 🟡 Lazy loading

**Task:** the module `heavy.js` prints `heavy evaluated` when loaded and exports a function as default. Load it only on demand via `import()` and make sure a repeated call does not execute the module again.

**Expected result:** `before`, `heavy evaluated`, `heavy result`, `true`.

<details><summary>Hint</summary>

`const load = () => import('./heavy.js')`; compare the two results.

</details>

<details><summary>Solution</summary>

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

## 4. 🟡 Breaking a cycle

**Task:** `ca2.js` and `cb2.js` import each other. `ca2.js` exports `const a = 'A'`, and `cb2.js` must give access to `a` without hitting the TDZ. Rewrite `cb2.js` so that the value is read later (via a function `getA`).

**Expected result:** `cb2 loaded`, then `ca2: A`.

<details><summary>Hint</summary>

Instead of reading `a` at the top level of `cb2.js`, export `export const getA = () => a;`.

</details>

<details><summary>Solution</summary>

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

## 5. 🔴 Public paths of a package

**Task:** create a package in `node_modules/mylib` with `exports` where `.` leads to `lib/index.js` (for `import`) and `lib/index.cjs` (for `require`), and `./util` to `lib/util.js`. Close everything else. Show that `import('mylib/lib/hidden.js')` is rejected and that ESM and CommonJS get different instances.

**Expected result:** `esm internal util`, `cjs`, `ERR_PACKAGE_PATH_NOT_EXPORTED` for closed paths, `esm cjs false` for the comparison.

<details><summary>Hint</summary>

`"exports": { ".": { "import": "./lib/index.js", "require": "./lib/index.cjs" }, "./util": "./lib/util.js" }`; from ESM call `createRequire(import.meta.url)('mylib')`.

</details>

<details><summary>Solution</summary>

The manifest, files and output are in the [advanced theory](advanced.en.md#packagejson-exports-and-imports) and [here](advanced.en.md#the-dual-package-hazard).

</details>
