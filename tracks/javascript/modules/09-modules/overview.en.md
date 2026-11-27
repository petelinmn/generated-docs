---
title: "Modules"
lang: en
track: javascript
kind: module
doc: overview
slug: 09-modules
part: 2
levels: [1, 2, 3, 4]
tags: [esm, commonjs, import, export, dynamic-import, live-bindings, package-exports, circular-dependencies, top-level-await, interop]
versions: "ECMAScript 2025+, Node.js 24.15 LTS"
status: draft
updated: 2026-11-26
---

# Modules

🌐 [Русский](overview.ru.md) · **English**

## In short

A module is a file that explicitly declares what it gives away (`export`) and what it takes from others (`import`). ES modules (ESM) are the language standard: static `import`/`export`, dynamic `import()`, "live" bindings and top-level `await`. The old CommonJS format (`require`/`module.exports`) lives alongside, and their interaction is the main source of confusion in Node.js. Packages describe entry points with the `exports` field in `package.json`, and a module is executed once and cached.

## Key ideas

- **A module is executed once,** however many files import it; the result is cached by file address.
- **Imports are hoisted and run before the module body:** dependencies are evaluated first.
- **Bindings are "live":** an imported name is a reference to the exporter's variable, not a copy of the value. You cannot assign to an imported name.
- **A namespace object** (`import * as ns`) is read-only and has no prototype.
- **`export *` does not re-export `default`.**
- **A dynamic `import()`** returns a promise of a namespace and allows loading code lazily.
- **Top-level `await`** delays everyone who imports the module.
- **Circular dependencies are allowed,** but touching a name that is not yet evaluated is a `ReferenceError`.
- **CommonJS and ESM interoperate:** ESM has no `require`, but CJS can be imported; `require(esm)` works in Node.js 24.
- **The `exports` field** in `package.json` limits which paths of a package are visible from outside.

## Minimal example

```js
// counter.js
export let count = 0;
export function inc() { count++; }
export default function main() { return 'default'; }
```

```js
// main.js
import main, { count, inc } from './counter.js';
console.log(main(), count); inc(); console.log(count);
```

```text
default 0
1
```

`count` in `main.js` changed after `inc()`: an import is a live reference, not a copy.

## When to use and when not

| Task | Choice |
|---|---|
| New code | ESM (`"type": "module"`) |
| An old project and packages | CommonJS, `createRequire`, the default import of CJS |
| Loading on demand, "heavy" dependencies | a dynamic `import()` |
| A library for both formats | conditional `exports` (carefully: the "dual package hazard") |
| Hiding a package's internals | the `exports` field |

- ❌ Do not mix `import` and `require` in one file without need.
- ❌ Do not rely on the execution order of circular dependencies.

## Material levels

| Level | What is inside |
|---|---|
| [1. Basic theory](basics.en.md) | `export`/`import`, live bindings, the namespace, execution order, re-exports, dynamic `import()`, top-level `await` |
| [2. Advanced theory](advanced.en.md) | cycles, import errors, interoperability with CommonJS, `require(esm)`, `exports` and `imports` in `package.json`, the dual package hazard, built-in modules |
| [3. Expert theory](expert.en.md) | a syntax reference, loading phases, the module cache, pitfalls, interview questions |
| [4. Self-check](quiz.en.md) | quiz and [exercises](exercises.en.md) |
| 5. Runnable examples | not created yet — `/learn javascript 09-modules level 5` |
| 6. Tests | not created yet — `/learn javascript 09-modules level 6` |

## Related materials

- Prerequisites: [01 · Environment and tools](../01-environment/overview.en.md) (ESM and CommonJS, `package.json`), [03 · Variables and scopes](../03-scopes-and-hoisting/overview.en.md) (TDZ)
- Next: [10 · Closures and functional style](../10-closures-and-functional/overview.en.md)
- Asynchrony in depth: module 12 of the roadmap
- Analogues in other tracks: [Python: modules and packages](../../../python/overview.en.md), [C#: assemblies and namespaces](../../../csharp/overview.en.md)
- Modules and `tsconfig` in TypeScript: [TypeScript](../../../typescript/overview.en.md)
- [React: components and JSX](../../../react/modules/01-components-and-jsx/overview.en.md)
- [Code splitting with lazy in React](../../../react/modules/12-concurrent-features/overview.en.md)
