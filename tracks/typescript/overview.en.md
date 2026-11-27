---
title: "TypeScript"
lang: en
track: typescript
kind: track
doc: overview
slug: typescript
tags: [typescript, type-system, nodejs]
versions: "TypeScript 7.0, Node.js 24 LTS"
default_level: 4
status: draft
updated: 2026-11-23
---

# TypeScript

🌐 [Русский](overview.ru.md) · **English**

## In a nutshell

TypeScript is a superset of [JavaScript](../javascript/overview.en.md) with a static type system. Types are checked at compile time and erased: the output is plain JavaScript, so nothing changes at runtime. The type system is structural (the shape of a value matters, not the name of its type) and very expressive: unions, generics, conditional and mapped types. Starting with version 7.0 the `tsc` compiler is a native program ported to Go and runs noticeably faster than before. Node.js 24 can run `.ts` files directly by erasing the types without checking them (type stripping), and the checking is done by `tsc`.

**Prerequisites:** modules 01–08 of the [JavaScript track](../javascript/overview.en.md).

## Environment

| What | Tool |
|---|---|
| Runtime | Node.js 24 LTS |
| Compiler and type checker | `typescript` (the `tsc` command), `npm install -D typescript` |
| Running `.ts` | `node file.ts` (type stripping) or `tsx` |
| Editor | VS Code with built-in TypeScript support |
| Module code | `code/` — `package.json`, `tsconfig.json`, `examples/exNN-name.ts`, `solutions/`, `tests/*.test.ts` |
| Run all module tests | `npm test` inside `code/` (vitest: `vitest run`) |
| Check types | `npx tsc --noEmit` inside `code/` |

## Roadmap

Learning progress is tracked in [PROGRESS.md](../../PROGRESS.md). Modules are generated at detail level **4** by default (theory and self-check, no code), see [detail levels](../../README.en.md#detail-levels).

### Part 1. Fundamentals

- [01 · Environment and tools](modules/01-environment/overview.en.md) — `tsc`, `tsconfig.json`, `strict` mode, running `.ts` in Node.js
- [02 · Basic types](modules/02-basic-types/overview.en.md) — primitives, arrays, tuples, `any`/`unknown`/`never`, literal types, `enum`
- [03 · Objects and interfaces](modules/03-objects-and-interfaces/overview.en.md) — `interface` and `type`, optional and `readonly` fields, structural typing
- [04 · Functions](modules/04-functions/overview.en.md) — parameter and return types, overloads, `this`, `void`
- [05 · Unions and narrowing](modules/05-unions-and-narrowing/overview.en.md) — union, intersection, type guards, discriminated unions, exhaustiveness checks
- [06 · Generics](modules/06-generics/overview.en.md) — type parameters, constraints, defaults
- [07 · Classes](modules/07-classes/overview.en.md) — access modifiers, `abstract`, `implements`, parameter properties
- [08 · Modules and configuration](modules/08-modules-and-tsconfig/overview.en.md) — ESM and CommonJS, `module`, `moduleResolution`, key `tsconfig` options

### Part 2. Advanced topics

- [09 · Utility types](modules/09-utility-types/overview.en.md) — `Partial`, `Pick`, `Omit`, `Record`, `ReturnType`, `Awaited`
- [10 · Conditional types and `infer`](modules/10-conditional-types/overview.en.md) — `extends ? :`, distribution over a union, `infer`
- [11 · Mapped and template literal types](modules/11-mapped-and-template-literal-types/overview.en.md) — `keyof`, `typeof`, indexed access, mapped types
- [12 · Types for asynchronous code](modules/12-async-types/overview.en.md) — `Promise<T>`, `async` functions, async iterators
- [13 · Errors and data validation](modules/13-errors-and-validation/overview.en.md) — `unknown` in `catch`, checks at boundaries, Zod
- [14 · Testing: vitest](modules/14-testing-vitest/overview.en.md) — tests, mocks, `expectTypeOf`, coverage
- [15 · Declaration files](modules/15-declaration-files/overview.en.md) — `.d.ts`, `@types`, declaration merging, module augmentation
- [16 · Build and monorepos](modules/16-build-and-monorepos/overview.en.md) — project references, bundlers, publishing types

### Part 3. Expert topics

- [17 · Type system internals](modules/17-type-system-internals/overview.en.md) — assignability, variance, `satisfies`, non-obvious gaps in soundness
- [18 · Type-level programming](modules/18-type-level-programming/overview.en.md) — recursive types, brand types, type-safe APIs
- [19 · Decorators](modules/19-decorators/overview.en.md) — standard ECMAScript decorators and their typing
- [20 · Compiler performance](modules/20-compiler-performance/overview.en.md) — `tsc` diagnostics, `isolatedDeclarations`, project references
- [21 · TypeScript in production](modules/21-typescript-in-production/overview.en.md) — migrating from JavaScript, linting, API design

## Libraries

- [Zod](libraries/zod/overview.en.md) — schemas and type inference from them
- [vitest](libraries/vitest/overview.en.md) — a testing framework
- [tsx](libraries/tsx/overview.en.md) — running TypeScript in Node.js
- [type-fest](libraries/type-fest/overview.en.md) — a collection of utility types
- [ts-pattern](libraries/ts-pattern/overview.en.md) — pattern matching

## Recipes

- [A typed API client](recipes/typed-api-client/overview.en.md) — request and response types, checking at the boundary
- [Validated configuration](recipes/validated-config/overview.en.md) — environment, schema and type
- [Publishing a library](recipes/library-publishing/overview.en.md) — types, ESM and CommonJS, `exports`
- [A monorepo](recipes/monorepo-setup/overview.en.md) — workspaces and project references

## Projects

- [A typed CLI utility](projects/typed-cli/overview.en.md) — a utility with argument checking
- [A typed REST service](projects/typed-rest-service/overview.en.md) — an API with schemas and tests

## Related tracks

- [JavaScript](../javascript/overview.en.md) — the basis of the language; without modules 01–08 types make no sense
- [Python](../python/overview.en.md) — Python type annotations (mypy, pyright) work differently: compare
- [C# / .NET](../csharp/overview.en.md) — nominal typing versus structural typing
- [React](../react/overview.en.md) — types for components, props and hooks

## Sources

- [TypeScript Handbook](https://www.typescriptlang.org/docs/handbook/intro.html)
- [TSConfig Reference](https://www.typescriptlang.org/tsconfig/)
- [Announcing TypeScript 7.0](https://devblogs.microsoft.com/typescript/announcing-typescript-7-0/)
- [Node.js: running TypeScript](https://nodejs.org/docs/latest-v24.x/api/typescript.html)
