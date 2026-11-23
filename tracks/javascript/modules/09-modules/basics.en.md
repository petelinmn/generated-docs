---
title: "Modules: basics"
lang: en
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

# Modules: basics

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [01 · Environment and tools](../01-environment/overview.en.md), [03 · Variables and scopes](../03-scopes-and-hoisting/overview.en.md) · **Time:** ~2.5 h

All examples were run on Node.js 24.15; in the experiment folder `package.json` contains `{"type":"module"}`.

## Why it matters

Without modules all code lives in one shared namespace where it is easy to overwrite someone else's variable. A module gives its own scope and explicit dependencies: you can see at once what depends on what.

## export and import

The file `counter.js`:

```js
export let count = 0;
export function inc() { count++; }
export const PI = 3.14;
export default function main() { return 'default'; }
console.log('counter.js evaluated');
```

The file `a.js` uses it and changes the state:

```js
import { count, inc } from './counter.js';
console.log('A sees', count); inc(); console.log('A after inc', count);
export const fromA = 1;
```

The file `main.js`:

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

What is visible here:

- A **named export** (`export const PI`) is imported in braces by name, optionally renamed (`count as c`). A **default export** (`export default`) is one per module and is imported without braces under any name (`main`).
- **A module is executed once:** `counter.js evaluated` was printed a single time although the file is imported by both `main.js` and `a.js`.
- **Bindings are live:** `a.js` called `inc()`, and `main.js` saw `c` equal to `1` (even though the import in `a.js` was made earlier). An import is a reference to the exporter's variable.
- **You cannot assign to an import:** `c = 5` is `TypeError: Assignment to constant variable.`; `ns.PI = 3` is an error too.
- **The namespace** `ns` (`import * as ns`) is an object with the module tag `[object Module]` and no prototype (`null`); the keys are sorted by code order (`'PI'` before `'count'`), and `default` is an ordinary name `default`.
- **A side-effect import** `import './a.js'` only runs the module, importing nothing.

## Execution order

Imports are processed before the module body, wherever they are in the file:

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

The dependency `side.js` ran **first**, although the `import` is written below the first line. So a module can safely use imports anywhere.

## Re-exports

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

- `export * from` re-exports all **named** exports — but **not `default`** (it is not in the list).
- `export { default as main2 }` re-exports `default` under another name; `export * as ns` re-exports the whole namespace as one object.
- A re-export is handy for "entry files" (`index.js`) that assemble a package's API.

## Dynamic import()

`import()` is an expression returning a promise of a namespace; the address can be computed at run time:

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

- A repeated `import()` returns the **same** object (`m === again` is `true`): the module is in the cache.
- The address can be a variable; a nonexistent file is a rejected promise with the code `ERR_MODULE_NOT_FOUND`.
- Use: lazy loading of heavy libraries and conditional dependencies.
- `import.meta.url` is the address of the current file, `import.meta.resolve(...)` is the address of the given module.

## Top-level await

In ESM you can write `await` right in the module. Importing modules wait for it to finish:

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

`after slow` was printed only after `slow.js` waited for the timer. Because of this, top-level `await` slows the start of all files that depend on the module: use it carefully.

## Beginner mistakes

- ❌ Importing without an extension (`'./counter'`) → ✅ `'./counter.js'`.
- ❌ Assigning to an imported name → ✅ call the exporter's function that changes the value.
- ❌ Expecting `export *` to carry `default` → ✅ `export { default } from …`.
- ❌ Expecting a copy of the value instead of a live reference → ✅ remember that `count` is updated.
- ❌ Putting `import` inside an `if` or a function → ✅ a dynamic `import()`.
- ❌ Many top-level `await`s → ✅ move lazy loading into functions.

## Related materials

- Next: [advanced theory](advanced.en.md) — cycles, CommonJS, `exports`
- Earlier: [01 · Environment and tools](../01-environment/advanced.en.md#esm-and-commonjs-differences)

## Sources

- [MDN: JavaScript modules](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Guide/Modules)
- [MDN: import](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Statements/import)
- [MDN: export](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Statements/export)
- [MDN: Dynamic import()](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Operators/import)
- [Node.js: ECMAScript modules](https://nodejs.org/docs/latest-v24.x/api/esm.html)
