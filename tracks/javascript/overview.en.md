---
title: "JavaScript"
lang: en
track: javascript
kind: track
doc: overview
slug: javascript
tags: [javascript, ecmascript, nodejs]
versions: "ECMAScript 2025+, Node.js 24 LTS"
default_level: 4
status: draft
updated: 2026-11-23
---

# JavaScript

🌐 [Русский](overview.ru.md) · **English**

## In a nutshell

JavaScript is a language with dynamic weak typing, prototype-based inheritance and a single-threaded execution model built on an event loop. It is standardized as ECMAScript (the ECMA-262 specification) and runs in browsers and on the server: in Node.js, Deno and Bun. Functions are ordinary values, asynchrony is built in (promises, `async`/`await`), and ES modules (ESM) are the standard way to organize code. Most web interfaces and a huge share of server services are written in JavaScript, and [TypeScript](../typescript/overview.en.md) adds static types to it.

## Environment

| What | Tool |
|---|---|
| Runtime | Node.js 24 LTS; a browser for the DOM modules |
| Package manager | npm (ships with Node.js) |
| Editor | VS Code, browser DevTools, the Node.js debugger (`node --inspect`) |
| Module code | `code/` — `package.json` (`"type": "module"`), `examples/exNN-name.js`, `solutions/`, `tests/*.test.js` |
| Run all module tests | `npm test` inside `code/` (the built-in runner: `node --test`) |
| Run one example | `node examples/ex01-name.js` |
| Linter and formatter | ESLint, Prettier |

## Roadmap

Learning progress is tracked in [PROGRESS.md](../../PROGRESS.md). Modules are generated at detail level **4** by default (theory and self-check, no code), see [detail levels](../../README.en.md#detail-levels).

### Part 1. Fundamentals

- [01 · Environment and tools](modules/01-environment/overview.en.md) — Node.js, npm, `package.json`, the REPL, DevTools, running scripts
- [02 · Types and values](modules/02-types-and-values/overview.en.md) — primitives and objects, `typeof`, type coercion, `==` and `===`, `NaN`, BigInt, Symbol
- [03 · Variables and scopes](modules/03-scopes-and-hoisting/overview.en.md) — `let`/`const`/`var`, hoisting, the TDZ, block scope
- [04 · Functions](modules/04-functions/overview.en.md) — declarations and expressions, arrow functions, parameters, `this`, `call`/`apply`/`bind`
- [05 · Objects and prototypes](modules/05-objects-and-prototypes/overview.en.md) — literals, property descriptors, the prototype chain, destructuring, spread
- [06 · Classes](modules/06-classes/overview.en.md) — `class` syntax, private fields, `static`, getters, inheritance
- [07 · Arrays and collections](modules/07-arrays-and-collections/overview.en.md) — array methods, `Map`, `Set`, `WeakMap`, typed arrays
- [08 · Strings, numbers, dates, regular expressions](modules/08-strings-numbers-dates-regex/overview.en.md) — `Intl`, `Date` and Temporal, `RegExp`

### Part 2. Advanced topics

- [09 · Modules](modules/09-modules/overview.en.md) — ESM and CommonJS, dynamic `import`, `exports` in `package.json`
- [10 · Closures and functional style](modules/10-closures-and-functional/overview.en.md) — closures, currying, higher-order functions, immutability
- [11 · Iterators and generators](modules/11-iterators-and-generators/overview.en.md) — the iteration protocol, `function*`, async generators, iterator helpers
- [12 · Promises and async/await](modules/12-promises-and-async-await/overview.en.md) — promise states, `Promise.all`/`allSettled`/`any`, errors
- [13 · The event loop and concurrency](modules/13-event-loop-and-concurrency/overview.en.md) — task and microtask queues, timers, `AbortController`, worker threads
- [14 · Errors and debugging](modules/14-errors-and-debugging/overview.en.md) — `Error`, `cause`, `AggregateError`, the call stack, the debugger
- [15 · Testing](modules/15-testing/overview.en.md) — `node:test`, mocks, coverage, testing async code
- [16 · Node.js: files, streams, processes](modules/16-nodejs-files-streams-processes/overview.en.md) — `fs`, streams, `child_process`, environment variables

### Part 3. Expert topics

- [17 · Browser: the DOM and events](modules/17-browser-dom-and-events/overview.en.md) — the DOM, event bubbling, `fetch`, storage, Web APIs
- [18 · Metaprogramming](modules/18-metaprogramming/overview.en.md) — `Proxy`, `Reflect`, symbols, tagged templates, `WeakRef`
- [19 · Performance and memory](modules/19-performance-and-memory/overview.en.md) — V8, hidden classes, garbage collection, profiling
- [20 · Security](modules/20-security/overview.en.md) — XSS, prototype pollution, CSP, the npm supply chain
- [21 · An HTTP service on Node.js](modules/21-http-service/overview.en.md) — the `http` module, routing, middleware, Fastify, tests

## Libraries

- [Zod](libraries/zod/overview.en.md) — data validation at application boundaries
- [Fastify](libraries/fastify/overview.en.md) — an HTTP server
- [pino](libraries/pino/overview.en.md) — structured logging
- [date-fns](libraries/date-fns/overview.en.md) — working with dates
- [Commander](libraries/commander/overview.en.md) — CLI applications

## Recipes

- [A CLI application](recipes/cli-app/overview.en.md) — arguments, subcommands, exit code
- [A REST API](recipes/rest-api/overview.en.md) — routes, validation, error handling
- [Configuration from the environment](recipes/config-from-env/overview.en.md) — environment variables, `.env`, validation
- [Requests with retries](recipes/fetch-with-retry/overview.en.md) — timeout, retries, `AbortSignal`

## Projects

- [A CLI utility](projects/cli-tool/overview.en.md) — a utility installed through `npm`
- [A REST service](projects/rest-service/overview.en.md) — a Node.js API with tests and Docker

## Related tracks

- [TypeScript](../typescript/overview.en.md) — static types on top of JavaScript; modules 01–08 of this track are required
- [Python](../python/overview.en.md) — another dynamic language: handy for comparing the execution model and asynchrony
- [C# / .NET](../csharp/overview.en.md) — `async`/`await` came to JavaScript from C#, compare the implementations
- [Kubernetes](../kubernetes/overview.en.md) — deploying Node.js services

## Sources

- [MDN Web Docs: JavaScript](https://developer.mozilla.org/en-US/docs/Web/JavaScript)
- [ECMAScript Language Specification (ECMA-262)](https://tc39.es/ecma262/)
- [Node.js documentation](https://nodejs.org/docs/latest-v24.x/api/)
- [Node.js: the test runner](https://nodejs.org/docs/latest-v24.x/api/test.html)
