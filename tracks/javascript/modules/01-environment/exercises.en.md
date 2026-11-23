---
title: "Exercises: environment and tools"
lang: en
track: javascript
kind: module
doc: exercises
slug: 01-environment
part: 1
tags: [nodejs, npm, esm, commonjs, node-test]
status: draft
updated: 2026-11-23
---

# Exercises: environment and tools

🌐 [Русский](exercises.ru.md) · **English**

> Solve it yourself; hints and solutions are under spoilers. Work in an empty folder; you need Node.js 24 and npm.

## 1. 🟢 Command-line arguments

**Task:** write `hello.js` that prints `Hello, <name>`, where the name is the first argument, or `world` without an argument.

**Expected result:** `node hello.js Ada` → `Hello, Ada`; `node hello.js` → `Hello, world`.

<details><summary>Hint</summary>

User arguments start at index 2; the default value is set with `??`.

</details>

<details><summary>Solution</summary>

```js
console.log('Hello,', process.argv[2] ?? 'world');
```

```text
$ node hello.js Ada
Hello, Ada
```

</details>

## 2. 🟢 A project with package.json and ESM

**Task:** run `npm init -y`, switch the project to ES modules and create `esm.js` that prints `typeof require` and the value of a top-level `await Promise.resolve('ok')`.

**Expected result:** `undefined` and `ok`.

<details><summary>Hint</summary>

`npm pkg set type=module`.

</details>

<details><summary>Solution</summary>

```js
console.log(typeof require);
console.log(await Promise.resolve('ok'));
```

```text
$ node esm.js
undefined
ok
```

</details>

## 3. 🟡 One code, two formats

**Task:** save the line `undeclared = 5; console.log(typeof globalThis.undeclared);` into `a.js` (a project with `"type": "module"`) and into `a.cjs`. Compare the results and explain.

**Expected result:** `a.js` — `ReferenceError: undeclared is not defined`; `a.cjs` — prints `number`.

<details><summary>Hint</summary>

ESM is strict mode, CommonJS is sloppy mode.

</details>

<details><summary>Solution</summary>

In ESM assigning to an undeclared variable is a strict-mode error. In `.cjs` sloppy mode creates a global variable, and `typeof globalThis.undeclared` is `number`.

```text
$ node a.js
ReferenceError: undeclared is not defined
$ node a.cjs
number
```

</details>

## 4. 🟡 Imports

**Task:** create `lib.js` (`export const x = 1;`) and `data.json` (`{"a": 1}`). Import `x` without an extension, then with one; import the JSON with and without the attribute.

**Expected result:** `ERR_MODULE_NOT_FOUND` and the hint `Did you mean to import "./lib.js"?`; `{ a: 1 }` with the attribute and `ERR_IMPORT_ATTRIBUTE_MISSING` without it (a dynamic `import()`).

<details><summary>Hint</summary>

`import data from './data.json' with { type: 'json' }`.

</details>

<details><summary>Solution</summary>

The code and output are in the [advanced theory](advanced.en.md#importing-files-and-json).

</details>

## 5. 🔴 Tests, environment variables and an exit code

**Task:** (a) add a test `tests/a.test.js` on `node:test` and run `npm test` with the script `node --test`. (b) Create `.env` with `N=5` and show that `process.env.N + 1` is `'51'`. (c) Write a script that finishes with code 3 and check the code in the shell.

**Expected result:** (a) `pass 1`; (b) `51`; (c) `exit=3`.

<details><summary>Hint</summary>

(b) `node --env-file=.env -p "process.env.N + 1"`; (c) `process.exitCode = 3`, then `echo $?`.

</details>

<details><summary>Solution</summary>

```js
// tests/a.test.js
import { test } from 'node:test';
import assert from 'node:assert/strict';
test('sum', () => { assert.equal(1 + 1, 2); });
```

```text
$ npm test            # pass 1, fail 0
$ node --env-file=.env -p "process.env.N + 1"
51
$ node exitcode.js ; echo "exit=$?"
done
exit=3
```

`process.env` values are strings, so `+ 1` concatenates `'5'` and `'1'`.

</details>
