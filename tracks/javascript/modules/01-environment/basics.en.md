---
title: "Environment and tools: basics"
lang: en
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

# Environment and tools: basics

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** a terminal and a text editor · **Time:** ~1.5 h

All examples were run on Node.js 24.15 and npm 11.12 (Windows, Git Bash). On other systems the output of paths and separators may differ.

## Why it matters

Any code has to run somewhere. JavaScript has two main hosts: the browser and Node.js. Node.js is easier to start with: it needs no HTML page and no server, just a file and a terminal. All examples in this track run in Node.js.

## Checking the installation

```text
$ node -v
v24.15.0
$ npm -v
11.12.1
```

Node.js is installed with the installer from nodejs.org or with a version manager (`nvm`, `fnm`); npm comes with it. For learning, take an LTS (long-term support) version.

## Three ways to run code

**An expression from the command line.** `-p` prints the result, `-e` only executes:

```text
$ node -p "1 + 1"
2
$ node -e "console.log(typeof window, typeof globalThis, typeof process)"
undefined object object
```

This shows the main difference from the browser: `window` is not defined, while `globalThis` and `process` exist.

**A file.** Create `args.js`:

```js
console.log(process.argv.slice(2));
console.log(typeof require, typeof module);
```

```text
$ node args.js a --flag=1 "b c"
[ 'a', '--flag=1', 'b c' ]
function object
```

`process.argv` is the array of arguments: positions 0 and 1 hold the paths to Node.js and to the script, so `slice(2)` keeps only the user's arguments. A quoted argument (`"b c"`) arrives as one string.

**The REPL (interactive console).** The command `node` with no arguments opens the `>` prompt, where every expression is executed and printed at once:

```text
$ node
> [1, 2, 3].map(n => n * 2)
[ 2, 4, 6 ]
> .exit
```

The line `Welcome to Node.js v24.15.0.` is printed at startup. The REPL is handy for quick checks.

## package.json

The `package.json` file describes a project. Create it with `npm init -y` (default answers):

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

The name comes from the folder name and `main` from the first file found. The main fields:

- `scripts` — named commands, run with `npm run <name>`; `npm test` is a shortcut for `npm run test`.
- `type` — the format of `.js` files: `"commonjs"` (as in this experiment) or `"module"`.
- `dependencies`/`devDependencies` — dependencies that appear after `npm install <package>` (no packages were installed in this experiment).

## ES modules and CommonJS

Turn on the modern module format: `npm pkg set type=module` changes `package.json`. Now `.js` is an ES module:

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

- `require` and `module` are not defined in ESM.
- `import.meta.filename` and `import.meta.dirname` give the path to the file and to its folder.
- `await` works at the top level of a file without an `async` wrapper.
- `this` at the top level of a module is `undefined`.

If you write `const fs = require('node:fs')` in such a project, you get an error:

```text
ReferenceError: require is not defined in ES module scope, you can use import instead
```

For the old format you can rename the file to `.cjs`: it is always read as CommonJS, and `require` works in it. A `.mjs` file is always read as ESM. More about the differences in the [advanced theory](advanced.en.md#esm-and-commonjs-differences).

## Scripts and built-in Node.js features

Scripts in `package.json` and the built-in test runner:

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

`node --test` finds `*.test.js` files and files in `test`/`tests` folders itself and runs them. Other useful built-in features:

```text
$ node --run hi          # run the script hi from package.json without npm
hi from script
$ printf 'GREETING=hi\nN=5\n' > .env
$ node --env-file=.env -p "process.env.GREETING + process.env.N"
hi5
```

`--env-file` loads environment variables from a file. Values are always strings: `N=5` arrives as `'5'`. A `.env` file with secrets must not be added to Git.

## Beginner mistakes

- ❌ Writing `require` in a project with `"type": "module"` → ✅ use `import` or a `.cjs` file.
- ❌ Expecting `window` or `document` in Node.js → ✅ those are browser objects.
- ❌ Forgetting the extension in an import (`'./lib'`) → ✅ ESM needs `'./lib.js'` (more in the [advanced theory](advanced.en.md#importing-files-and-json)).
- ❌ Taking `process.argv[0]` for the user's first argument → ✅ the user's arguments start at index 2.
- ❌ Committing `.env` → ✅ add it to `.gitignore`.

## Related materials

- Next: [advanced theory](advanced.en.md) — ESM versus CommonJS, imports, exit codes
- Next module: [02 · Types and values](../02-types-and-values/overview.en.md)
- Comparison: [Python and uv](../../../python/overview.en.md) — a similar set of tools

## Sources

- [Node.js: Command-line API](https://nodejs.org/docs/latest-v24.x/api/cli.html)
- [Node.js: Modules: ECMAScript modules](https://nodejs.org/docs/latest-v24.x/api/esm.html)
- [Node.js: Test runner](https://nodejs.org/docs/latest-v24.x/api/test.html)
- [Node.js: Process (`process.argv`)](https://nodejs.org/docs/latest-v24.x/api/process.html#processargv)
- [npm: package.json](https://docs.npmjs.com/cli/v11/configuring-npm/package-json)
