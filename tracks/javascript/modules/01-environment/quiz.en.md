---
title: "Quiz: environment and tools"
lang: en
track: javascript
kind: module
doc: quiz
slug: 01-environment
part: 1
tags: [nodejs, npm, esm, commonjs, process-argv]
status: draft
updated: 2026-11-23
---

# Quiz: environment and tools

🌐 [Русский](quiz.ru.md) · **English**

> Answer first, then open the answer. The mark shows which level a question comes from: 🟢 [basics](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 What does `node -p "1 + 1"` print?

- A) Nothing, `-p` only executes code
- B) `2`
- C) `11`
- D) An error

<details><summary>Answer</summary>

**B.** `-p` prints the result of the expression, while `-e` only executes code.
→ [Basics](basics.en.md#three-ways-to-run-code)

</details>

## 2. 🟢 `node args.js a --flag=1`. What is in `process.argv.slice(2)`?

- A) `['node', 'args.js', 'a', '--flag=1']`
- B) `['a', '--flag=1']`
- C) `['args.js', 'a', '--flag=1']`
- D) `[]`

<details><summary>Answer</summary>

**B.** The first two elements of `process.argv` are the paths to Node.js and to the script.
→ [Basics](basics.en.md#three-ways-to-run-code)

</details>

## 3. 🟢 Which `package.json` field defines the format of `.js` files?

- A) `main`
- B) `type`
- C) `format`
- D) `engines`

<details><summary>Answer</summary>

**B.** `"type": "module"` is ESM, `"commonjs"` is CommonJS.
→ [Basics](basics.en.md#packagejson)

</details>

## 4. 🟢 In a project with `"type": "module"` you wrote `require('node:fs')`. The result?

- A) It works
- B) `ReferenceError: require is not defined in ES module scope`
- C) `SyntaxError`
- D) A warning

<details><summary>Answer</summary>

**B.** For CommonJS rename the file to `.cjs` or use `import`.
→ [Basics](basics.en.md#es-modules-and-commonjs)

</details>

## 5. 🟡 How do you get the path of the current folder in ESM?

- A) `__dirname`
- B) `import.meta.dirname`
- C) `process.argv[1]`
- D) `module.path`

<details><summary>Answer</summary>

**B.** `__dirname` and `__filename` exist only in CommonJS.
→ [Advanced](advanced.en.md#esm-and-commonjs-differences)

</details>

## 6. 🟡 What happens on `import { x } from './lib'` in ESM if the file `lib.js` exists?

- A) `lib.js` is imported
- B) `ERR_MODULE_NOT_FOUND` with the hint `Did you mean to import "./lib.js"?`
- C) `lib.json` is imported
- D) It silently returns `undefined`

<details><summary>Answer</summary>

**B.** The ESM loader does not guess extensions.
→ [Advanced](advanced.en.md#importing-files-and-json)

</details>

## 7. 🟡 The code `undeclared = 5;` runs in `strict.js` (ESM) and in `strict.cjs`. The result?

- A) The same in both
- B) In ESM a `ReferenceError`, in `.cjs` a global variable is created
- C) In ESM a global variable is created, in `.cjs` a `ReferenceError`
- D) A `ReferenceError` in both

<details><summary>Answer</summary>

**B.** ESM is always strict, CommonJS is sloppy by default.
→ [Advanced](advanced.en.md#strict-mode)

</details>

## 8. 🟡 Select all correct statements

- [ ] A) `process.exitCode = 3` lets the program finish normally with code 3
- [ ] B) `process.exit(7)` ends the process at once
- [ ] C) `fetch` is absent in Node.js 24
- [ ] D) `window` is not defined in Node.js

<details><summary>Answer</summary>

**A, B, D.** C is wrong: in Node.js 24 `fetch` is a function (in the experiment `typeof fetch` is `function`).
→ [Advanced](advanced.en.md#nodejs-globals)

</details>

## 9. 🔴 Find the flaw

> "`--env-file=.env` with `N=5` puts the number 5 into `process.env.N`, so `process.env.N + 1` equals 6."

<details><summary>Answer</summary>

Environment variables are always strings: `process.env.N + 1` is `'51'`. Numbers must be converted explicitly (`Number(...)`).
→ [Gotcha 5](expert.en.md#5---env-file-gives-only-strings)

</details>

## 10. 🔴 Why is `arguments.length` 5 at the top level of a CommonJS file?

- A) A Node.js bug
- B) The file runs as the body of a function with five parameters: `exports`, `require`, `module`, `__filename`, `__dirname`
- C) The file has five lines
- D) It is the number of command-line arguments

<details><summary>Answer</summary>

**B.** That is the CommonJS wrapper; ESM has no such wrapper.
→ [Internals](expert.en.md#internals)

</details>

## 11. 🔴 Open question

Explain why the same file can behave differently as a `.js` in a project with `"type": "module"` and as a `.cjs`.

<details><summary>Key points of a good answer</summary>

- The file format is set by the extension or the `type` field of the nearest `package.json`.
- ESM: strict mode, `import`, top-level `await`, `this` is `undefined`, no `require` and `__dirname`.
- CommonJS: a wrapper function, `require`, `module.exports`, sloppy mode by default.

</details>

→ [Expert](expert.en.md#trade-offs-and-alternatives)
