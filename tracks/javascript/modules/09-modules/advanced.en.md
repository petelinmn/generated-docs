---
title: "Modules: advanced"
lang: en
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

# Modules: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

Builds on [basic theory](basics.en.md). All examples were run on Node.js 24.15; the files sit in one folder with `package.json` (`"type": "module"`).

## Import errors

Importing a name that does not exist is an error at the linking stage, before any code runs:

```js
import { nothing } from './counter.js';
```

```text
SyntaxError: The requested module './counter.js' does not provide an export named 'nothing'
```

This differs from dynamic `import()` and CommonJS: a typo in a name is seen at once at startup, not on first access.

## Circular dependencies

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

`ca.js` starts `cb.js` first, and it touches `a`, which has not been evaluated yet: the name is in the TDZ ([module 03](../03-scopes-and-hoisting/basics.en.md#the-temporal-dead-zone-tdz)). Function declarations are available at once, while `const`/`let`/`class` are not. It is better to break a cycle: move the shared part into a third module or touch the value later (inside a function).

## Interoperability with CommonJS

From ESM you can import CommonJS files:

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

- The **default import** (`import lib from './lib.cjs'`) always returns the whole `module.exports`.
- **Named imports** of CommonJS work only if Node.js could find the names by static analysis (`exports.named = …`). For `module.exports = { x: 1, … }` the name `x` is not defined: `import { x } from './lib.cjs'` fails with `SyntaxError: Named export 'x' not found. The requested module './lib.cjs' is a CommonJS module, which may not support all module.exports as named exports.` (in the experiment), and the namespace shows only `default`, `module.exports` and the found `y`.
- **`createRequire(import.meta.url)`** returns a `require` function inside ESM — needed for loading CommonJS and JSON the old way.

### require() for ESM

In Node.js 24 a CommonJS file can load an ES module through `require`:

```js
// req.cjs
const m = require('./counter.js');
console.log(Object.keys(m));
```

```text
counter.js evaluated
[ 'PI', '__esModule', 'count', 'default', 'inc' ]
```

The result is a namespace object with the key `__esModule`. This works if the ES module has no top-level `await`.

> ⚠️ Verify: the behavior of `require(esm)` for modules with top-level `await` and in older Node.js versions (a flag) was not run in the experiments; per the documentation an error is thrown in that case.

## package.json: exports and imports

The `exports` field defines a package's **public** paths and conditions for `import` and `require`:

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

The package is in `node_modules/mylib`; the consumer:

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

- The conditions `import` and `require` choose the file by the way of loading: from ESM — `lib/index.js` (`which` is `esm`), from CommonJS — `lib/index.cjs` (`cjs`).
- Internal files (`lib/hidden.js`, `internal/int.js`) are **closed**: an attempt to import them is `ERR_PACKAGE_PATH_NOT_EXPORTED`, even though the file physically exists.
- **`imports`** defines the package's "internal" aliases that begin with `#`: inside the package `import { secret } from '#int'` works (in the experiment), while from outside these names are unavailable per the documentation (not checked). In your own project it is the same:

```text
// package.json: { "imports": { "#cnt": "./counter.js" } }
import { PI } from '#cnt'; console.log(PI);   // 3.14
```

### The dual package hazard

If a package ships both ESM and CommonJS, the same code can be loaded **twice** as two different instances:

```js
import { which as w1 } from 'mylib';
import { createRequire } from 'node:module';
const w2 = createRequire(import.meta.url)('mylib').which;
console.log(w1, w2, w1 === w2);
```

```text
esm cjs false
```

Different files are different modules with their own state (singletons, caches, `instanceof`). To avoid divergence, a package is made "ESM only" or the CommonJS version is wrapped around the ESM one.

## Built-in modules and JSON

```js
import fs from 'node:fs';
import { readFileSync } from 'fs';
import * as p from 'node:path';
console.log(typeof fs.readFileSync, fs.readFileSync === readFileSync, typeof p.join, Object.keys(p).includes('default'));
```

```text
function true function true
```

- Built-in modules are available with the `node:` prefix and without it; the prefix distinguishes them from npm packages.
- Built-in modules have both a `default` and named exports (`fs` and `readFileSync` are one value).
- JSON is imported with the attribute `with { type: 'json' }` ([module 01](../01-environment/advanced.en.md#importing-files-and-json)).

## Lazy loading

```js
const { default: main } = await import('./counter.js');
console.log(main(), 'loaded lazily');
```

```text
default loaded lazily
```

A dynamic `import()` postpones loading heavy modules until they are used (rarely used CLI commands, big libraries).

## Best practices

- New code — ESM; extensions in paths; built-in modules with `node:`.
- One entry file (`index.js`) re-exporting the public API; do not export details.
- For a package — `exports` with an explicit list of public paths.
- Break cycles: put shared things in a separate module.
- Avoid side effects on import (requests, writing to files): they run once and unexpectedly.
- Do not publish a dual package without need.

## Common mistakes

- ❌ `import { x } from './lib.cjs'` for `module.exports = {…}` → ✅ the default import and destructuring.
- ❌ Importing a closed path of a package → ✅ a public one from `exports`.
- ❌ Reading a value from a circular dependency at the top level → ✅ read later, inside a function.
- ❌ Mixing `__dirname` and ESM → ✅ `import.meta.dirname`.
- ❌ Publishing a package with both formats and shared state → ✅ a single format.

## Related materials

- Next: [expert theory](expert.en.md)
- Environment and formats: [01 · Environment and tools](../01-environment/advanced.en.md)
- `tsconfig` and modules in TypeScript: [TypeScript](../../../typescript/overview.en.md)

## Sources

- [Node.js: Packages (`exports`, `imports`)](https://nodejs.org/docs/latest-v24.x/api/packages.html)
- [Node.js: Modules: CommonJS modules](https://nodejs.org/docs/latest-v24.x/api/modules.html)
- [Node.js: Loading ECMAScript modules using `require()`](https://nodejs.org/docs/latest-v24.x/api/modules.html#loading-ecmascript-modules-using-require)
- [Node.js: `module.createRequire`](https://nodejs.org/docs/latest-v24.x/api/module.html#modulecreaterequirefilename)
- [MDN: Circular imports](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Guide/Modules#circular_imports)
