---
title: "Environment and tools: advanced"
lang: en
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

# Environment and tools: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

Builds on [basic theory](basics.en.md). All examples were run on Node.js 24.15.

## ESM and CommonJS: differences

| Property | ESM (`import`) | CommonJS (`require`) |
|---|---|---|
| File | `.mjs` or `.js` with `"type": "module"` | `.cjs` or `.js` with `"type": "commonjs"` |
| Importing | `import x from './x.js'` | `const x = require('./x')` |
| Extension in the path | required (`./lib.js`) | may be omitted |
| Top-level `await` | yes | no |
| Strict mode | always | only if turned on by hand |
| `this` at the top level | `undefined` | `module.exports` |
| `__filename`, `__dirname` | no (`import.meta.filename`, `import.meta.dirname`) | yes |

Checks from the experiments. For the CommonJS file `wrap.cjs`:

```js
console.log(arguments.length, typeof __filename, __filename.endsWith('wrap.cjs'), require.main === module);
```

```text
$ node wrap.cjs
5 string true true
```

`arguments.length` is 5: a CommonJS file is actually run as the body of a function with five parameters (`exports`, `require`, `module`, `__filename`, `__dirname`). ESM has no such wrapper, so at the top level `this` is `undefined`, not `module.exports`.

## Strict mode

In ESM strict mode is always on. In sloppy code some errors are silently ignored:

```js
// strict.js (ESM) and strict.cjs (CommonJS) — the same code
try { undeclared = 5; } catch (e) { console.log(e.name + ': ' + e.message); }
```

```text
$ node strict.js
ReferenceError: undeclared is not defined
$ node strict.cjs
(nothing printed: the assignment created a global variable)
```

Another example is assigning to a string index ([module 02](../02-types-and-values/basics.en.md#strings)): in strict mode it is a `TypeError`, in sloppy `node -e` it silently does nothing. Strict mode can be turned on in CommonJS too with the directive `'use strict';` on the first line.

## Node.js globals

`globalThis` is the standard name of the global object in both the browser and Node.js. Node.js has its own globals and part of the web APIs:

```text
$ node -e "console.log(typeof structuredClone, typeof queueMicrotask, typeof fetch, typeof window, typeof document, typeof navigator)"
function function function undefined undefined object
```

`fetch`, `structuredClone`, `queueMicrotask` and `navigator` exist, while `window` and `document` do not. Built-in library modules are imported with the `node:` prefix (`import fs from 'node:fs'`); the prefix distinguishes them from npm packages.

The scope of a script differs: code from `node -e` runs in the global scope, so there `var gv = 1` creates a property of the global object (`globalThis.gv === 1`) while `let gl` does not (`undefined`). In a file (ESM or CommonJS) top-level variables belong to the module, not to the global object ([module 03](../03-scopes-and-hoisting/overview.en.md)).

## Importing files and JSON

In ESM the extension is required:

```js
import { x } from './lib';
```

```text
Error [ERR_MODULE_NOT_FOUND]: Cannot find module '…/js01/lib' imported from …/js01/noext.js
Did you mean to import "./lib.js"?
```

JSON is imported with the attribute `type: 'json'`; without it the import is rejected:

```js
import data from './data.json' with { type: 'json' };
console.log(data);
import('./data.json').catch(e => console.log(e.code));
```

```text
{ a: 1 }
ERR_IMPORT_ATTRIBUTE_MISSING
```

## The process exit code

A process reports its result to the system as a number: `0` is success, a non-zero value is an error. Scripts and CI read it.

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

`process.exitCode` sets the code but lets the program finish normally (flush output buffers), while `process.exit(n)` ends the process at once. The first is preferable.

## Syntax check and other flags

`node --check file.js` checks the syntax without running:

```text
$ node --check lib.js && echo check-ok
check-ok
$ node --check bad.js        # const = ;
SyntaxError: Unexpected token '='
```

`node --watch` restarts the script when files change (in the experiment only the presence of the flag in `node --help` was checked).

> ⚠️ Verify: `node --watch`, `node --inspect` (the Chrome DevTools debugger), the version managers `nvm`/`fnm`, the `engines` field and `npx` were not run in the experiments.

## Best practices

- A new project: `"type": "module"`, imports with extensions, built-in modules with the `node:` prefix.
- Exit codes: `process.exitCode`, not `process.exit()` in ordinary code.
- Versions: pin the LTS line of Node.js for the project (the `engines` field in `package.json`, an `.nvmrc` file).
- Tests: `node --test` without extra dependencies while its features are enough.
- Configuration: environment variables, `.env` not in Git.

## Common mistakes

- ❌ Mixing `import` and `require` in one file → ✅ pick one format per file; `require` in ESM is an error.
- ❌ Omitting the extension in an ESM import → ✅ write `./lib.js`.
- ❌ Reading `__dirname` in ESM → ✅ `import.meta.dirname`.
- ❌ Calling `process.exit()` in the middle of writing to stdout → ✅ `process.exitCode` and natural termination.
- ❌ Expecting a global variable from `var` in a file → ✅ it is a module variable.

## Related materials

- Next: [expert theory](expert.en.md)
- Value types and strict mode, illustrated with strings: [02 · Types and values](../02-types-and-values/advanced.en.md)
- Scope: [03 · Variables and scopes](../03-scopes-and-hoisting/overview.en.md)

## Sources

- [Node.js: Modules: CommonJS modules](https://nodejs.org/docs/latest-v24.x/api/modules.html)
- [Node.js: Modules: ECMAScript modules](https://nodejs.org/docs/latest-v24.x/api/esm.html)
- [Node.js: Process exit codes](https://nodejs.org/docs/latest-v24.x/api/process.html#exit-codes)
- [Node.js: Globals](https://nodejs.org/docs/latest-v24.x/api/globals.html)
- [MDN: Strict mode](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Strict_mode)
