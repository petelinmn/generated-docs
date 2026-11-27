---
title: "Effects"
lang: en
track: react
kind: module
doc: overview
slug: 06-effects
part: 2
levels: [1, 2, 3, 4]
tags: [react, effects, use-effect, hooks, cleanup]
versions: "React 19.3"
status: draft
updated: 2026-11-26
---

# Effects

🌐 [Русский](overview.ru.md) · **English**

## In a nutshell

An effect synchronises a component with something outside React: a connection, a timer, a browser API, a request. It runs after React has committed the changes to the screen, and can return a cleanup function that undoes what it did. Many things that look like they need an effect do not: values computed from data, and reactions to what a user did.

## Key ideas

- Rendering must stay pure; effects are where side effects go.
- The dependency array says which values the effect reacts to.
- Cleanup runs before the next run and on unmount.
- In development `StrictMode` runs setup and cleanup one extra time.
- An effect sees the values of the render that created it.
- Responses can arrive out of order: ignore or abort the old ones.
- If something can be computed during render, do not use an effect.

## Minimal example

```tsx
useEffect(() => {
  const connection = createConnection(room);
  connection.connect();
  return () => connection.disconnect();      // cleanup
}, [room]);                                   // run again when room changes
```

## When to use and when not

- ✅ Subscriptions, timers, connections, non-React widgets.
- ✅ Loading data when there is no framework or data library for it.
- ❌ Transforming props or state for display.
- ❌ Reacting to a click: do it in the handler.

## Levels of this material

| Level | What's inside |
|---|---|
| [1. Basics](basics.en.md) | setup, cleanup, dependencies |
| [2. Advanced](advanced.en.md) | order of calls, stale values, races, when no effect is needed |
| [3. Expert](expert.en.md) | `useEffectEvent`, `useLayoutEffect`, reference, gotchas, interview questions |
| [4. Self-check](quiz.en.md) | a quiz and [exercises](exercises.en.md) |
| 5. Runnable examples | not created yet — `/learn react 06-effects level 5` |
| 6. Tests | not created yet — `/learn react 06-effects level 6` |

## Related materials

- Prerequisites: [03 · State and events](../03-state-and-events/overview.en.md)
- Promises and cancellation: [JavaScript async](../../../javascript/modules/12-promises-and-async-await/overview.en.md)
- Closures behind stale values: [closures in JavaScript](../../../javascript/modules/10-closures-and-functional/overview.en.md)
- Timeouts and retries on the server side: [reliability patterns](../../../system-design/modules/09-reliability-patterns/overview.en.md)
- Next: the following module of the [React roadmap](../../overview.en.md)
- [07 · Refs and the DOM](../07-refs-and-dom/overview.en.md)
- [09 · Custom hooks](../09-custom-hooks/overview.en.md)
- [12 · Concurrent features](../12-concurrent-features/overview.en.md)
