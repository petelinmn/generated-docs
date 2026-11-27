---
title: "React"
lang: en
track: react
kind: track
doc: overview
slug: react
tags: [react, jsx, components, hooks, frontend]
versions: "React 19.3"
default_level: 4
status: draft
updated: 2026-11-27
---

# React

🌐 [Русский](overview.ru.md) · **English**

## In a nutshell

React is a JavaScript library for building user interfaces from components. A component is a function that returns a description of the screen; when its data changes, React calls it again and updates what differs in the page. It is used for web applications, usually together with a framework such as Next.js or React Router, and for mobile applications through React Native.

**Prerequisites:** [JavaScript](../javascript/overview.en.md): functions, arrays, objects, modules, promises. TypeScript is used in typed examples.

## Environment

| What | Tool |
|---|---|
| Runtime | Node.js, for the build tool and tests |
| Project | Vite: `npm create vite@latest my-app -- --template react-ts` |
| Library | React 19.3 |
| Editor | VS Code with TypeScript support |
| Module code | at levels 5–6: `code/` with `package.json`, `examples/exNN-name.tsx`, `solutions/`, `tests/*.test.tsx` |
| Run all module tests | `npm test` (`vitest run`) inside `code/`, with Testing Library |
| Check types | `npx tsc --noEmit` |

## Roadmap

Learning progress is tracked in [PROGRESS.md](../../PROGRESS.md). Modules are generated at detail level **4** (theory and self-check) by default.

### Part 1. Fundamentals

- [01 · Components and JSX](modules/01-components-and-jsx/overview.en.md) — components, JSX rules, purity, render and commit
- [02 · Props and composition](modules/02-props-and-composition/overview.en.md) — props, defaults, `children`, composition
- [03 · State and events](modules/03-state-and-events/overview.en.md) — `useState`, snapshots, batching, events
- [04 · Lists, keys and identity](modules/04-lists-and-keys/overview.en.md) — `key`, preserving and resetting state
- [05 · Forms](modules/05-forms/overview.en.md) — controlled and uncontrolled inputs

### Part 2. Advanced topics

- [06 · Effects](modules/06-effects/overview.en.md) — `useEffect`, cleanup, when an effect is not needed
- [07 · Refs and the DOM](modules/07-refs-and-dom/overview.en.md) — `useRef`, focus, measurements, portals
- [08 · Context and reducers](modules/08-context-and-reducers/overview.en.md) — `useReducer`, `createContext`
- [09 · Custom hooks](modules/09-custom-hooks/overview.en.md) — rules of hooks, reuse of logic
- [10 · Testing](modules/10-testing/overview.en.md) — Vitest and Testing Library

### Part 3. Expert topics

- [11 · Rendering performance](modules/11-performance/overview.en.md) — profiling, memoization, React Compiler
- [12 · Concurrent features](modules/12-concurrent-features/overview.en.md) — transitions, deferred values, Suspense
- [13 · Actions](modules/13-actions/overview.en.md) — async transitions, form actions, optimistic updates
- [14 · Server rendering](modules/14-server-rendering/overview.en.md) — SSR, hydration, Server Components

## Libraries

- [React Router](libraries/react-router/overview.en.md) — routing
- [TanStack Query](libraries/tanstack-query/overview.en.md) — server data and caching
- [Zustand](libraries/zustand/overview.en.md) — a small state store

## Recipes

- [Data fetching](recipes/data-fetching/overview.en.md) — loading, errors, cancellation
- [Form validation](recipes/form-validation/overview.en.md) — client and server errors

## Projects

- [Task board](projects/task-board/overview.en.md) — state, forms, routing, tests

## Related tracks

- [JavaScript](../javascript/overview.en.md) — the language React is written in
- [TypeScript](../typescript/overview.en.md) — types for props and hooks
- [C# / .NET](../csharp/overview.en.md) — a typical backend for the application

## Sources

- [React documentation: learn](https://react.dev/learn)
- [React versions](https://react.dev/versions)
- [React: creating an app](https://react.dev/learn/creating-a-react-app)
