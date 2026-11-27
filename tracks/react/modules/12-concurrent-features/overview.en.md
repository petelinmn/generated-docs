---
title: "Concurrent features"
lang: en
track: react
kind: module
doc: overview
slug: 12-concurrent-features
part: 3
levels: [1, 2, 3, 4]
tags: [react, suspense, transitions, concurrent-rendering, error-boundaries]
versions: "React 19.3"
status: draft
updated: 2026-11-26
---

# Concurrent features

🌐 [Русский](overview.ru.md) · **English**

## In a nutshell

Rendering in React can be interrupted and postponed. That makes it possible to keep the interface responsive: mark an update as a transition so that urgent ones come first, show a placeholder while something is not ready with `Suspense`, and keep the old content on screen while the new one is being prepared.

## Key ideas

- `Suspense` shows a fallback while something inside is not ready.
- `lazy` loads component code on demand; `use` reads a cached promise.
- A transition marks an update as non-urgent and interruptible.
- A transition keeps already shown content instead of replacing it with a fallback.
- `useDeferredValue` lets a slow part of the screen lag behind.
- Errors from rendering are caught by error boundaries.
- Data fetched in an effect does not activate `Suspense`.

## Minimal example

```tsx
const Chart = lazy(() => import('./Chart'));

<Suspense fallback={<p>loading</p>}>
  <Chart />
</Suspense>
// first <p>loading</p>, then the chart
```

## When to use and when not

- ✅ Code splitting, switching views with loading data, slow lists while typing.
- ✅ Keeping a hidden panel's state with `Activity`.
- ❌ Transitions for a controlled input's own value.
- ❌ Expecting these tools to make slow code faster: they change its order and priority.

## Levels of this material

| Level | What's inside |
|---|---|
| [1. Basics](basics.en.md) | `Suspense`, `lazy`, error boundaries |
| [2. Advanced](advanced.en.md) | transitions, deferred values, `use` |
| [3. Expert](expert.en.md) | `Activity`, reference, gotchas, interview questions |
| [4. Self-check](quiz.en.md) | a quiz and [exercises](exercises.en.md) |
| 5. Runnable examples | not created yet — `/learn react 12-concurrent-features level 5` |
| 6. Tests | not created yet — `/learn react 12-concurrent-features level 6` |

## Related materials

- Prerequisites: [06 · Effects](../06-effects/overview.en.md), [11 · Rendering performance](../11-performance/overview.en.md)
- Promises: [JavaScript async](../../../javascript/modules/12-promises-and-async-await/overview.en.md)
- Dynamic import: [JavaScript modules](../../../javascript/modules/09-modules/overview.en.md)
- Next: the following module of the [React roadmap](../../overview.en.md)
- [13 · Actions](../13-actions/overview.en.md)
- [14 · Server rendering](../14-server-rendering/overview.en.md)
