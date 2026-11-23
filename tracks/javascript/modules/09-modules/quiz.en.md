---
title: "Quiz: modules"
lang: en
track: javascript
kind: module
doc: quiz
slug: 09-modules
part: 2
tags: [esm, import, export, commonjs, package-exports]
status: draft
updated: 2026-11-23
---

# Quiz: modules

🌐 [Русский](quiz.ru.md) · **English**

> Answer on your own first, then open the answer. The label shows the question level: 🟢 [basic](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 How many times is the body of `counter.js` executed if `main.js` and `a.js` both import it?

- A) Twice
- B) Once
- C) Every time a name is accessed
- D) Depends on the order

<details><summary>Answer</summary>

**B.** A module is executed once; the result is cached by file address.
→ [Basic theory](basics.en.md#export-and-import)

</details>

## 2. 🟢 What happens with `import { count } from './counter.js'; count = 5;`?

- A) `count` becomes `5`
- B) `TypeError: Assignment to constant variable.`
- C) `SyntaxError`
- D) Nothing

<details><summary>Answer</summary>

**B.** An imported name cannot be assigned: it is a reference to the exporter's binding.
→ [Basic theory](basics.en.md#export-and-import)

</details>

## 3. 🟢 `counter.js` exports `let count` and a function `inc()`. What does the importer see after `inc()`?

- A) The old value (a copy)
- B) The new value: bindings are live
- C) `undefined`
- D) An error

<details><summary>Answer</summary>

**B.** An import is a live reference to the exporter's variable.
→ [Basic theory](basics.en.md#export-and-import)

</details>

## 4. 🟢 In what order is the text printed?

```js
console.log('1 main body');
import './side.js';          // side.js prints 'side evaluated first'
console.log('2 after import statement');
```

- A) `1`, `side`, `2`
- B) `side`, `1`, `2`
- C) `1`, `2`, `side`
- D) `side`, `2`, `1`

<details><summary>Answer</summary>

**B.** Dependencies run before the module body, wherever the `import` is.
→ [Basic theory](basics.en.md#execution-order)

</details>

## 5. 🟡 What does `Object.keys` contain for `export * from './counter.js'` if `counter.js` has both named exports and `default`?

- A) Everything, including `default`
- B) Only the named ones, without `default`
- C) Only `default`
- D) Empty

<details><summary>Answer</summary>

**B.** `export *` does not carry `default`. You need `export { default } from …`.
→ [Basic theory](basics.en.md#re-exports)

</details>

## 6. 🟡 What does `await import('./counter.js') === await import('./counter.js')` return?

- A) `false`
- B) `true`
- C) `TypeError`
- D) Depends on the file size

<details><summary>Answer</summary>

**B.** A dynamic `import()` returns the same namespace object from the cache.
→ [Basic theory](basics.en.md#dynamic-import)

</details>

## 7. 🟡 `lib.cjs`: `module.exports = { x: 1 }`. What happens with `import { x } from './lib.cjs'`?

- A) `1` is returned
- B) `SyntaxError: Named export 'x' not found...`
- C) `undefined`
- D) `ERR_MODULE_NOT_FOUND`

<details><summary>Answer</summary>

**B.** The names of `module.exports = {…}` are not recognized statically; use `import lib from './lib.cjs'`.
→ [Advanced theory](advanced.en.md#interoperability-with-commonjs)

</details>

## 8. 🟡 What happens with `import('mylib/internal/int.js')` if this path is not listed in the package's `exports`?

- A) The file loads
- B) `ERR_PACKAGE_PATH_NOT_EXPORTED`
- C) `ERR_MODULE_NOT_FOUND`
- D) `undefined`

<details><summary>Answer</summary>

**B.** The `exports` field closes every path that is not listed, even if the file exists.
→ [Advanced theory](advanced.en.md#packagejson-exports-and-imports)

</details>

## 9. 🟡 Choose all true statements

- [ ] A) From ESM you can import a CommonJS file as a default import
- [ ] B) `__dirname` and `__filename` are available in ESM
- [ ] C) `createRequire(import.meta.url)` gives `require` inside ESM
- [ ] D) Imported bindings can be reassigned

<details><summary>Answer</summary>

**A, C.** B and D are wrong: instead of `__dirname` use `import.meta.dirname`, and assigning to an import is forbidden.
→ [Advanced theory](advanced.en.md#interoperability-with-commonjs)

</details>

## 10. 🔴 Find the mistake

> "`import { which } from 'mylib'` in ESM and `require('mylib').which` in CommonJS return the same module instance, since it is one package."

<details><summary>Answer</summary>

With conditional `exports`, ESM and CommonJS get different files, i.e. different instances with their own state (the dual package hazard): in the experiment `w1 === w2` is `false`.
→ [Gotcha 5](expert.en.md#5-double-loading-of-a-package)

</details>

## 11. 🔴 What happens if a module does a 50 ms top-level `await` and another module imports it?

- A) The importer runs at once and does not see the result
- B) The importer waits for the `await` to finish
- C) `TypeError`
- D) The import is ignored

<details><summary>Answer</summary>

**B.** In the experiment `after slow` was printed after `slow done`.
→ [Basic theory](basics.en.md#top-level-await)

</details>

## 12. 🔴 What does the code print for a cycle `ca.js` ↔ `cb.js` if `cb.js` reads `a` at the top level (`export const a = 'A'` in `ca.js`)?

- A) `A`
- B) `undefined`
- C) `ReferenceError: Cannot access 'a' before initialization`
- D) `SyntaxError`

<details><summary>Answer</summary>

**C.** `cb.js` is evaluated first, and `a` is still in the TDZ. Reading inside a function called later works (`getA()`).
→ [Advanced theory](advanced.en.md#circular-dependencies)

</details>

## 13. 🔴 Open question

Explain how ES modules are loaded: the phases, the cache and live bindings.

<details><summary>Key points of a good answer</summary>

- Phases: building the graph (finding and parsing files), linking (creating bindings, checking exports), evaluation (dependencies before dependents).
- A cache by address: every module is executed once; a repeated `import()` returns the same object.
- Bindings are live: an import is a reference to the exporter's variable, assignment is forbidden.
- Cycles are allowed, but touching a not yet evaluated `const` is a `ReferenceError`; a top-level `await` delays dependents.

</details>

→ [Expert theory](expert.en.md#internals)
