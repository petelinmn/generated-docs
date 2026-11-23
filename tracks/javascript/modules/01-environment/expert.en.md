---
title: "Environment and tools: expert"
lang: en
track: javascript
kind: module
doc: expert
slug: 01-environment
part: 1
tags: [nodejs, v8, esm, commonjs, module-wrapper, package-json, cli-flags, exit-code]
versions: "Node.js 24.15 LTS (V8 13.6), npm 11.12"
status: draft
updated: 2026-11-23
---

# Environment and tools: expert

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · **3. Expert** · [4. Quiz](quiz.en.md)

Builds on [basic](basics.en.md) and [advanced](advanced.en.md) theory. What was confirmed by running on Node.js 24.15 is marked "in the experiment".

## Reference

### `node` command-line flags

| Flag | Purpose |
|---|---|
| `-v`, `--version` | the Node.js version (in the experiment `v24.15.0`) |
| `-p`, `--print "expression"` | evaluate and print the result (in the experiment) |
| `-e`, `--eval "code"` | execute code (in the experiment) |
| `-i`, `--interactive` | open the REPL (in the experiment through a pipe) |
| `--check file` | check the syntax without running (in the experiment) |
| `--test` | run the built-in test runner (in the experiment) |
| `--run <script>` | run a script from `package.json` (in the experiment) |
| `--env-file=<file>` | load environment variables (in the experiment) |
| `--watch` | restart on file changes (present in `--help`) |
| `--inspect` | enable the debugger |

> ⚠️ Verify: `--watch`, `--inspect`, `--import`, `--require` and other flags not marked "in the experiment" come from the documentation and were not run. The full list is `node --help` and the [CLI documentation](https://nodejs.org/docs/latest-v24.x/api/cli.html).

### `package.json` fields

| Field | Value |
|---|---|
| `name`, `version` | the package identifier and version (in the experiment the name is from the folder, the version `1.0.0`) |
| `type` | `"commonjs"` (the default of `npm init -y` in the experiment) or `"module"` |
| `main` | the entry point for CommonJS (in the experiment the first file found) |
| `scripts` | named commands for `npm run` (in the experiment `test`, `hello`, `hi`) |
| `dependencies`, `devDependencies` | dependencies |

> ⚠️ Verify: `exports`, `imports`, `bin`, `engines`, `workspaces`, `files` are described in the npm and Node.js documentation but were not used in the experiments (`exports` is covered in module 09 of the roadmap).

### Globals and modules

| What | Where |
|---|---|
| `process`, `console`, `globalThis` | everywhere (in the experiment) |
| `fetch`, `structuredClone`, `queueMicrotask`, `navigator` | Node.js 24 (in the experiment type `function`/`object`) |
| `window`, `document` | the browser only (in the experiment `undefined`) |
| `require`, `module`, `__filename`, `__dirname` | CommonJS only (in the experiment) |
| `import.meta.filename`, `import.meta.dirname`, `import.meta.url` | ESM only (in the experiment) |

## Internals

- **V8 and libuv.** Node.js is the V8 engine (it runs JavaScript; in the experiment `process.versions.v8` = `13.6.233.17-node.48`) plus its own I/O library and APIs (`fs`, `net`, `http`). The program's single-threaded code runs in V8, while I/O is serviced in the background (the [event loop](../../overview.en.md), module 13 of the roadmap).
- **The CommonJS wrapper.** A `.cjs` file is run as the body of a function with the parameters `exports`, `require`, `module`, `__filename`, `__dirname` — in the experiment `arguments.length` is 5. That is why `var` does not reach the global object (in the experiment `typeof globalThis.gv` is `undefined`).
- **The ESM loader.** ES modules are loaded asynchronously: the dependency graph is built first, then the code runs. That is why top-level `await` is allowed and paths must be complete (with an extension): the loader does not guess extensions (in the experiment `ERR_MODULE_NOT_FOUND` with the hint `Did you mean to import "./lib.js"?`).
- **Format detection.** The file format is set by the extension (`.mjs`, `.cjs`) or, for `.js`, by the `type` field of the nearest `package.json`. If there is no `package.json` at all, Node.js's default rules apply.
- **Import attributes.** A JSON module needs `with { type: 'json' }`; without the attribute a dynamic `import()` is rejected with `ERR_IMPORT_ATTRIBUTE_MISSING` (in the experiment).
- **Exit codes.** `process.exitCode` is read on normal termination, `process.exit(n)` ends at once (in the experiment codes 3 and 7).

> ⚠️ Verify: the algorithm for detecting the format when `type` is absent (syntax detection), mutual loading `require(esm)` and `import cjs`, and `exports` resolution come from the documentation and were not checked by running.

## Gotchas

### 1. `npm init -y` creates `"type": "commonjs"`

A new project is CommonJS by default (in the experiment with npm 11.12). For ESM change `type` by hand (`npm pkg set type=module`).

### 2. `"main"` is filled in with the "first file found"

In the experiment `main` became `args.js` because it is the first `.js` in the folder. For applications this usually does not matter, for libraries it does.

### 3. `this` at the top level differs

ESM: `undefined`; CommonJS: `module.exports` (in the experiment). Code that uses `this` at the top level behaves differently.

### 4. Strictness depends on the format

The same file as `.js` (ESM) and `.cjs` behaves differently: `undeclared = 5` is an error versus a silent global variable (in the experiment).

### 5. `--env-file` gives only strings

`N=5` arrives as `'5'` (in the experiment `GREETING + N` gave `'hi5'`). Numbers and booleans must be converted explicitly.

### 6. `node -e` is the global scope

`var` from `-e` lands in `globalThis` (in the experiment), but from a file it does not. Experiments in `-e` may not repeat in a file.

### 7. `process.exit()` cuts off output

An immediate exit can lose unsent output; finish the program normally through `process.exitCode`.

## Trade-offs and alternatives

| Approach | Pros | Cons |
|---|---|---|
| Node.js + npm | the de facto standard, built-in tests and `--env-file` | one runtime of several |
| Deno, Bun | a different set of built-in tools, own managers | not run |
| pnpm, yarn | faster or stricter than npm | not run |
| ESM | a language standard, top-level `await` | extensions in paths, not all old packages |
| CommonJS | compatible with old packages | non-standard, sloppy by default |
| The built-in `node:test` | no dependencies | a smaller ecosystem than vitest/jest |

## Interview questions

<details><summary>1. How does Node.js differ from a browser as a JavaScript runtime?</summary>

There is no `window` or `document`, but there are `process`, `fs` and Node.js's module system. The engine is the same (V8 in Node.js and Chrome), but the set of APIs differs.

→ [Basic theory](basics.en.md#three-ways-to-run-code)

</details>

<details><summary>2. How do you get command-line arguments?</summary>

`process.argv`; the first two elements are the paths to Node.js and to the script, the user's start at index 2.

→ [Basic theory](basics.en.md#three-ways-to-run-code)

</details>

<details><summary>3. What is <code>package.json</code> for?</summary>

It describes a project: name, version, scripts, dependencies and the module format (`type`).

→ [Basic theory](basics.en.md#packagejson)

</details>

<details><summary>4. Trick: <code>require is not defined in ES module scope</code>. What happened?</summary>

The file is read as ESM (`"type": "module"` or `.mjs`), which has no `require`. You need `import` or a `.cjs` file.

→ [Basic theory](basics.en.md#es-modules-and-commonjs)

</details>

<details><summary>5. How does ESM differ from CommonJS?</summary>

`import`/`export` instead of `require`/`module.exports`, a mandatory extension, top-level `await`, always strict mode, `this` is `undefined`, no `__dirname`.

→ [Advanced theory](advanced.en.md#esm-and-commonjs-differences)

</details>

<details><summary>6. Trick: what is <code>arguments.length</code> at the top level of a CommonJS file?</summary>

Five — the file runs as a function with the parameters `exports`, `require`, `module`, `__filename`, `__dirname`.

→ [Internals](#internals)

</details>

<details><summary>7. How do you import a JSON file in ESM?</summary>

`import data from './data.json' with { type: 'json' }`. Without the attribute — the error `ERR_IMPORT_ATTRIBUTE_MISSING`.

→ [Advanced theory](advanced.en.md#importing-files-and-json)

</details>

<details><summary>8. How does <code>process.exitCode</code> differ from <code>process.exit()</code>?</summary>

`exitCode` sets the code and lets the process finish normally, `exit()` cuts the process off at once.

→ [Advanced theory](advanced.en.md#the-process-exit-code)

</details>

<details><summary>9. How do you run tests without third-party libraries?</summary>

`node --test` (or `npm test` with a script), tests using the `node:test` and `node:assert` modules.

→ [Basic theory](basics.en.md#scripts-and-built-in-nodejs-features)

</details>

<details><summary>10. How do you load environment variables from a file?</summary>

`node --env-file=.env`; the values are strings, and a file with secrets must not get into Git.

→ [Gotcha 5](#5---env-file-gives-only-strings)

</details>

<details><summary>11. Trick: <code>var x</code> in <code>node -e</code> lands in <code>globalThis</code> but not in a file. Why?</summary>

`-e` code runs in the global scope, while a file runs inside a module (the CommonJS wrapper or the ESM module scope).

→ [Gotcha 6](#6-node--e-is-the-global-scope)

</details>

## Cheat sheet

```text
node -v ; npm -v
node -p "expr" ; node -e "code" ; node
node file.js args…            # process.argv.slice(2)
npm init -y ; npm pkg set type=module
npm test ; node --test ; node --run script
node --env-file=.env file.js
node --check file.js
process.exitCode = 1
```

- ESM: `import`, a mandatory extension, top-level `await`, `import.meta.dirname`.
- CommonJS: `require`, `module.exports`, `__dirname`.
- Built-in modules — with the `node:` prefix.

## Related materials

- Previous level: [advanced theory](advanced.en.md)
- Next module: [02 · Types and values](../02-types-and-values/overview.en.md)
- Modules and testing in depth — modules 09 and 15 of the [track](../../overview.en.md) roadmap

## Sources

- [Node.js: Command-line options](https://nodejs.org/docs/latest-v24.x/api/cli.html)
- [Node.js: Modules: Packages](https://nodejs.org/docs/latest-v24.x/api/packages.html)
- [Node.js: Process](https://nodejs.org/docs/latest-v24.x/api/process.html)
- [npm: package.json](https://docs.npmjs.com/cli/v11/configuring-npm/package-json)
- [V8](https://v8.dev/docs)
