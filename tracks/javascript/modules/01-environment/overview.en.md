---
title: "Environment and tools"
lang: en
track: javascript
kind: module
doc: overview
slug: 01-environment
part: 1
levels: [1, 2, 3, 4]
tags: [nodejs, npm, package-json, esm, commonjs, repl, node-test, env-file]
versions: "Node.js 24.15 LTS, npm 11.12"
status: draft
updated: 2026-11-23
---

# Environment and tools

🌐 [Русский](overview.ru.md) · **English**

## In short

JavaScript can run in a browser and in **Node.js** — a runtime built on the V8 engine. For the first steps you only need Node.js: it runs files (`node file.js`), evaluates expressions from the command line (`node -p`, `node -e`) and has an interactive console (the REPL). The package manager **npm** is installed together with Node.js, and a project is described by a `package.json` file. Its `"type"` field decides how Node.js reads `.js` files: as ES modules (`"module"`) or as CommonJS (`"commonjs"`).

## Key ideas

- **`node file.js` runs a program;** `node -p "expression"` prints the result, `node -e "code"` executes code, and `node` with no arguments opens the REPL.
- **Command-line arguments are in `process.argv`:** the first two elements are the path to Node.js and to the script, then come yours.
- **`package.json` is the project's passport:** name, version, scripts, dependencies and `"type"`.
- **There are two module formats: ESM and CommonJS.** ESM has no `require` but has `import`, `import.meta` and top-level `await`. The extensions `.mjs` and `.cjs` fix the format regardless of `"type"`.
- **ESM runs in strict mode,** CommonJS by default in sloppy mode (for example, assigning to an undeclared variable).
- **Node.js 24 has a built-in test runner** (`node --test`, the `node:test` module), `.env` support (`--env-file`) and script running (`node --run`).
- **There are no browser globals:** `window` and `document` are absent, while `globalThis` and `process` exist.

## Minimal example

```js
// hello.js
console.log('Hello,', process.argv[2] ?? 'world');
```

```text
$ node hello.js Ada
Hello, Ada
$ node -p "1 + 1"
2
```

## When to use and when not

| Task | Tool |
|---|---|
| Check an expression | `node -p "..."` or the REPL |
| Run a script | `node file.js` |
| Run tests | `node --test` / `npm test` |
| Add a third-party library | `npm install <package>` |
| Work with the DOM, `window` | a browser (module 17 of the roadmap) |

- ❌ Do not install globally the packages a project needs: pin them in `package.json`.
- ❌ Do not write new code in CommonJS without a reason: the standard is ES modules.

## Material levels

| Level | What is inside |
|---|---|
| [1. Basic theory](basics.en.md) | running code, the REPL, arguments, `package.json`, ESM and CommonJS, scripts, `--env-file`, `node --test` |
| [2. Advanced theory](advanced.en.md) | ESM versus CommonJS, strict mode, Node.js globals, importing JSON, exit codes, Node.js versions |
| [3. Expert theory](expert.en.md) | reference of flags and fields, the CommonJS module wrapper, path resolution, pitfalls, interview questions |
| [4. Self-check](quiz.en.md) | quiz and [exercises](exercises.en.md) |
| 5. Runnable examples | not created yet — `/learn javascript 01-environment level 5` |
| 6. Tests | not created yet — `/learn javascript 01-environment level 6` |

## Related materials

- Next: [02 · Types and values](../02-types-and-values/overview.en.md)
- Modules in depth: module 09 · Modules (in the [track](../../overview.en.md) roadmap)
- Testing in depth: module 15 · Testing (in the [track](../../overview.en.md) roadmap)
- Analogues in other tracks: [Python: environment](../../../python/overview.en.md) — uv and venv; [C#: the .NET platform](../../../csharp/overview.en.md)
- TypeScript on top of this environment: [TypeScript](../../../typescript/overview.en.md)
