---
title: "Variables and scopes: advanced"
lang: en
track: javascript
kind: module
doc: advanced
slug: 03-scopes-and-hoisting
part: 1
tags: [scope-chain, module-scope, global-scope, function-in-block, default-parameters, switch, catch, iife]
versions: "ECMAScript 2025+, Node.js 24.15 LTS"
status: draft
updated: 2026-11-23
---

# Variables and scopes: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

Builds on [basic theory](basics.en.md). All examples were run on Node.js 24.15; where stated, a sloppy `.cjs` file was used.

## The scope chain

A name is looked up in the current scope, then in the enclosing one, and so on up to the global one. A function "remembers" the scope in which it was created — this is a **closure**:

```js
const mk = () => { let count = 0; return () => ++count; };
const c = mk(); c();
console.log(c(), typeof count);
```

```text
2 undefined
```

The inner function sees `count` and changes it between calls, while from outside `count` is unreachable (`typeof count` is `'undefined'`). This is how private data is created; more in module 10 of the roadmap.

## Module scope and global scope

Top-level variables of a **file** belong to the module, not to the global object:

```js
// an ES module file
var gv = 1; let gl = 2;
console.log(globalThis.gv, globalThis.gl, this);
```

```text
undefined undefined undefined
```

In a browser classic script a top-level `var` would create a `window` property. In Node.js (both ESM and CommonJS) it does not: a file is a module. But code from `node -e` runs in the global scope:

```text
$ node -e "var gv=1; let gl=2; console.log(globalThis.gv, globalThis.gl)"
1 undefined
```

`var` landed in `globalThis`, `let` did not: at the top level of a script `let` and `const` live in a separate "global lexical" scope and are not properties of the global object.

## Functions in blocks

In strict mode (including ESM) a function declared in a block is visible only inside it:

```js
{ function inner() { return 'in block'; } }
try { console.log(inner()); } catch (e) { console.log(e.name + ': ' + e.message) }
```

```text
ReferenceError: inner is not defined
```

In sloppy code (a `.cjs` file) the behavior is historically different:

```js
// s.cjs (sloppy mode)
{ function inner() { return 'in block'; } }
console.log(inner());
console.log(typeof hoistedFn);
if (true) { function hoistedFn() {} }
console.log(typeof hoistedFn);
```

```text
in block
undefined
function
```

A function from a block becomes available in the enclosing function after the block runs, and before it — `undefined`. These quirks matter only for reading old code; write in strict mode.

## The scope of default parameters

Default values are evaluated in the parameters' own scope, where the function body is not visible yet:

```js
let dflt = 'outer';
function g(a = dflt, dflt2 = a) { let dflt = 'body'; return [a, dflt2, dflt]; }
console.log(g());
```

```text
[ 'outer', 'outer', 'body' ]
```

The parameter `a` took `dflt` from the outer scope (not `'body'` from the function body), and the second parameter saw the first.

## switch and catch

All `switch` branches share one block scope, so identical `let`s in different `case`s conflict:

```js
eval("switch (1) { case 1: let q = 1; break; case 2: let q = 2; }");
```

```text
SyntaxError: Identifier 'q' has already been declared
```

The fix is separate braces in each branch: `case 1: { let q = 1; break; }`.

The `catch` parameter is visible only inside `catch`, while a `var` from it "leaks" out:

```js
try { throw new Error('x') } catch (e) { var fromCatch = e.message }
console.log(fromCatch, typeof e);
```

```text
x undefined
```

## IIFE: an immediately invoked function

Before block scope and modules existed, a function called at once was used to isolate variables:

```js
const r = (() => { const secret = 42; return secret * 2; })();
console.log(r);
```

```text
84
```

`secret` is unreachable from outside. Today a `{ }` block or a module is enough for this, while an IIFE is found in old code and where top-level `async` is needed without a module.

## Best practices

- Declare variables as close to their use as possible and in the smallest scope.
- `const` by default; `let` only when reassigning.
- Do not create global variables: in strict mode that is a `ReferenceError` ([module 01](../01-environment/advanced.en.md#strict-mode)).
- Do not rely on hoisting and on functions in blocks.
- Wrap `case`s that have declarations in braces.

## Common mistakes

- ❌ Expecting a top-level `var` of a file to become a `globalThis` property → ✅ it is a module variable.
- ❌ Calling a function from a block outside the block → ✅ declare it in the right scope.
- ❌ Repeating `let` in several `case`s → ✅ braces around each branch.
- ❌ Using a default parameter that refers to a variable of the function body → ✅ the value comes from the outer scope.
- ❌ Testing scope in `node -e` and carrying the conclusions over to a file → ✅ in a file the module rules apply.

## Related materials

- Next: [expert theory](expert.en.md)
- Strict mode: [01 · Environment and tools](../01-environment/advanced.en.md)
- `typeof` and `ReferenceError`: [02 · Types and values](../02-types-and-values/advanced.en.md#typeof-of-an-undeclared-variable)

## Sources

- [MDN: Scope](https://developer.mozilla.org/en-US/docs/Glossary/Scope)
- [MDN: Functions — Block-level functions](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Statements/function#block-level_function_declaration)
- [MDN: Default parameters — scope](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Functions/Default_parameters#scope_effects)
- [MDN: IIFE](https://developer.mozilla.org/en-US/docs/Glossary/IIFE)
- [ECMAScript: Executable Code and Execution Contexts](https://tc39.es/ecma262/#sec-executable-code-and-execution-contexts)
