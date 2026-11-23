---
title: "Modules: expert"
lang: en
track: javascript
kind: module
doc: expert
slug: 09-modules
part: 2
tags: [module-records, module-linking, module-evaluation, live-bindings, namespace-exotic-object, module-map, top-level-await, cjs-lexer]
versions: "ECMAScript 2025+, Node.js 24.15 LTS"
status: draft
updated: 2026-11-23
---

# Modules: expert

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · **3. Expert** · [4. Quiz](quiz.en.md)

Builds on [basic](basics.en.md) and [advanced](advanced.en.md) theory. All results were obtained by running on Node.js 24.15.

## Reference

### Forms of export

| Syntax | Meaning |
|---|---|
| `export const x = 1; export function f() {} export class K {}` | named exports of declarations |
| `export { a, b as c }` | export of existing bindings, including under another name (in the experiment `export { arrow as renamed }`) |
| `export default expr` | the default export (in the experiment `export default 42`) |
| `export { default } from './m.js'`, `export { default as x } from …` | re-exporting `default` |
| `export * from './m.js'` | re-exporting all named ones, **without** `default` |
| `export * as ns from './m.js'` | the namespace as one export |

### Forms of import

| Syntax | Meaning |
|---|---|
| `import d from './m.js'` | the default export |
| `import { a, b as c } from './m.js'` | named ones |
| `import d, { a } from './m.js'` | both |
| `import * as ns from './m.js'` | the namespace object |
| `import './m.js'` | only run it |
| `import data from './x.json' with { type: 'json' }` | JSON with an attribute |
| `import('./m.js')` | dynamic, a promise of a namespace |
| `import.meta.url`, `.filename`, `.dirname`, `.resolve()` | information about the module (in the experiment) |

### Error codes (in the experiment)

| Code or message | Cause |
|---|---|
| `SyntaxError: The requested module './counter.js' does not provide an export named 'nothing'` | no such export |
| `SyntaxError: Named export 'x' not found. The requested module './lib.cjs' is a CommonJS module…` | a CommonJS name was not recognized |
| `ERR_MODULE_NOT_FOUND` | file not found |
| `ERR_PACKAGE_PATH_NOT_EXPORTED` | the path is closed by `exports` |
| `ERR_IMPORT_ATTRIBUTE_MISSING` | JSON without an attribute |
| `ReferenceError: Cannot access 'a' before initialization` | a cycle, the name is in the TDZ |
| `TypeError: Assignment to constant variable.` | assignment to an import |

## Internals

- **Three loading phases.** (1) *Building the graph:* find the files, parse them, assemble module records from all `import`/`export`. (2) *Linking (instantiation):* create bindings for exports and connect imports to them; this is also where "no such export" is caught. (3) *Evaluation:* run the module bodies in the order "dependencies before dependents" (in the experiment `side evaluated first`).
- **Live bindings.** An imported name is not a copy but a *reference to the binding* of the exporting module. That is why `count` in `main.js` saw `1` after `inc()` was called in another module (in the experiment), and an import cannot be assigned to.
- **The module map.** The loader caches modules by address: repeated `import`s and `import()`s return the same object (in the experiment `m === again`, `counter.js evaluated` once). The same file loaded as ESM and as CommonJS gives **two** instances (in the experiment `esm cjs false`).
- **The namespace object** is an "exotic" object: keys are sorted, properties are read-only, the prototype is `null`, `Object.prototype.toString` gives `[object Module]` (in the experiment). It is not frozen (`Object.isFrozen(ns)` is `false`), yet writes to it are rejected.
- **Cycles.** A graph with a cycle is linked as a whole and evaluated in traversal order; a module in the cycle sees the bindings of a not yet executed module in the TDZ (in the experiment `cb: ReferenceError`). Function declarations are initialized at linking and are available.
- **Top-level `await`.** A module with TLA is evaluated asynchronously; all dependent modules wait for it to finish (in the experiment `after slow` after `slow done`). Sibling modules with no dependencies on each other can be evaluated in parallel.
- **Interoperability with CommonJS.** The loader analyzes a CommonJS file statically, looks for "explicit" exports (`exports.named = …`) and creates named exports; besides them — `default` and (in Node.js 24) the key `'module.exports'` (in the experiment `[ 'default', 'module.exports', 'y' ]`). `require(esm)` synchronously links and evaluates the ESM graph, so it is impossible with TLA; the result is a namespace with `__esModule` (in the experiment).
- **Path resolution.** For ESM the address must be complete (with an extension): the loader does not guess extensions (in the experiment `ERR_MODULE_NOT_FOUND` with a hint). For packages Node.js reads `exports`/`imports` and conditions (`import`, `require`, `default`).

> ⚠️ Verify: the exact details of CommonJS static analysis (the library used, which patterns are recognized), the order of parallel evaluation under TLA, the behavior of `require(esm)` under TLA and the conditions `node`/`default`/`types` in `exports` were not checked in the experiments.

## Gotchas

### 1. `export *` does not carry `default`

The re-export keys: `[ 'PI', 'count', 'inc', 'main2', 'ns', 'pi' ]` — without `default` (in the experiment).

### 2. An import is a live reference, but not a variable

Assigning to an import is a `TypeError`; to change a value the exporter must provide a function (`inc()`).

### 3. Cycles and the TDZ

Touching a `const` of a cycle at the top level fails; functions work.

### 4. A named import from CommonJS is unreliable

`import { x } from './lib.cjs'` fails for `module.exports = { x }`; the default import helps.

### 5. Double loading of a package

The ESM and CommonJS entries are different instances: `w1 === w2` is `false` (in the experiment).

### 6. `exports` hides everything else

Any path not listed in `exports` is closed (`ERR_PACKAGE_PATH_NOT_EXPORTED`), even `package.json` if it was not listed.

### 7. Side effects on import

A `console.log` in a module body fires on first load (in the experiment `counter.js evaluated`), even if nothing is used.

### 8. `await` blocks dependents

A long top-level `await` delays all importing modules (in the experiment `slow done` before `after slow`).

### 9. Dynamic `import()` and paths

The address is relative to the module where `import()` is called; in CommonJS it is relative to the file, not `process.cwd()`.

> ⚠️ Verify: gotcha 9 about CommonJS comes from the documentation and was not run.

## Trade-offs and alternatives

| Approach | Pros | Cons |
|---|---|---|
| ESM | a standard, static analysis, live bindings, TLA | extensions in paths, nuances with CommonJS |
| CommonJS | compatible with the old world | dynamic, no TLA, not a standard |
| A dual package | works everywhere | the risk of two instances |
| An ESM-only package | one format | CommonJS consumers depend on `require(esm)` (works in Node.js 24.15 in the experiment) |
| Bundlers (esbuild, Rollup, Vite) | a single file, tree shaking, minification | an extra step; not run |
| A dynamic `import()` | lazy loading, conditional dependencies | asynchronous, hinders static analysis |

## Interview questions

<details><summary>1. How do a named export and a default export differ?</summary>

A module has many named ones, imported in `{}` by name; `default` is one, imported under any name without braces.

→ [Basic theory](basics.en.md#export-and-import)

</details>

<details><summary>2. What does a "live binding" mean?</summary>

An imported name is a reference to the exporter's variable: changes are visible, and assigning to an import is forbidden.

→ [Internals](#internals)

</details>

<details><summary>3. How many times is a module imported from five files executed?</summary>

Once: the result is cached by file address.

→ [Basic theory](basics.en.md#export-and-import)

</details>

<details><summary>4. Trick: what does <code>Object.keys(await import('./reexport.js'))</code> return for <code>export *</code> from a module with a <code>default</code>?</summary>

A list without `default`: `export *` does not carry it.

→ [Gotcha 1](#1-export--does-not-carry-default)

</details>

<details><summary>5. When do module bodies run and in what order?</summary>

After the graph is built and linked; dependencies before dependents; imports are "hoisted".

→ [Basic theory](basics.en.md#execution-order)

</details>

<details><summary>6. What happens with a circular dependency?</summary>

The graph is linked, but the first evaluated module of the cycle touches names that are not yet evaluated: for `const`/`let`/`class` a `ReferenceError` (TDZ), functions work.

→ [Advanced theory](advanced.en.md#circular-dependencies)

</details>

<details><summary>7. Trick: <code>import { x } from './lib.cjs'</code> fails. Why?</summary>

Named exports of CommonJS are determined by static analysis; for `module.exports = {…}` the name is not recognized. You need `import lib from './lib.cjs'`.

→ [Gotcha 4](#4-a-named-import-from-commonjs-is-unreliable)

</details>

<details><summary>8. How do you use <code>require</code> in ESM?</summary>

`createRequire(import.meta.url)` from `node:module`.

→ [Advanced theory](advanced.en.md#interoperability-with-commonjs)

</details>

<details><summary>9. What is the <code>exports</code> field in <code>package.json</code> for?</summary>

It defines a package's public paths and conditions (`import`/`require`); the other paths are closed.

→ [Advanced theory](advanced.en.md#packagejson-exports-and-imports)

</details>

<details><summary>10. What is the dual package hazard?</summary>

One package loaded both as ESM and as CommonJS gives two independent instances with their own state.

→ [Gotcha 5](#5-double-loading-of-a-package)

</details>

<details><summary>11. How does top-level <code>await</code> work?</summary>

The module is evaluated asynchronously, and all importing modules wait for it to finish.

→ [Gotcha 8](#8-await-blocks-dependents)

</details>

<details><summary>12. When is a dynamic <code>import()</code> needed?</summary>

Lazy loading, conditional dependencies, an address computed at run time.

→ [Basic theory](basics.en.md#dynamic-import)

</details>

## Cheat sheet

```text
export const a / function f / class K ; export { a as b } ; export default x
export * from './m.js' (no default) ; export { default } from './m.js' ; export * as ns from './m.js'
import d, { a as b } from './m.js' ; import * as ns ; import './m.js' ; await import('./m.js')
import data from './x.json' with { type: 'json' }
import { createRequire } from 'node:module'  → require()
package.json: "exports": { ".": { "import": …, "require": … } }, "imports": { "#x": … }
```

- A module runs once; imports are live.
- Extensions in paths are required; `node:` for built-ins.
- For CommonJS — the default import.

## Related materials

- Previous level: [advanced theory](advanced.en.md)
- Environment and formats: [01 · Environment and tools](../01-environment/expert.en.md)
- Scopes and the TDZ: [03 · Variables and scopes](../03-scopes-and-hoisting/expert.en.md)

## Sources

- [ECMAScript: Modules](https://tc39.es/ecma262/#sec-modules)
- [Node.js: Packages](https://nodejs.org/docs/latest-v24.x/api/packages.html)
- [Node.js: ECMAScript modules — interoperability with CommonJS](https://nodejs.org/docs/latest-v24.x/api/esm.html#interoperability-with-commonjs)
- [MDN: import.meta](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Operators/import.meta)
- [MDN: Top-level await](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Operators/await#top_level_await)
