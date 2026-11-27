---
title: "State and events"
lang: en
track: react
kind: module
doc: overview
slug: 03-state-and-events
part: 1
levels: [1, 2, 3, 4]
tags: [react, state, use-state, events, hooks]
versions: "React 19.3"
status: draft
updated: 2026-11-26
---

# State and events

🌐 [Русский](overview.ru.md) · **English**

## In a nutshell

State is the memory of a component: data that survives between renders and whose change causes a new render. Events are how a user's action reaches your code. The usual cycle is: an event happens, a handler sets state, React renders again and updates the page. Most surprises come from one rule: during a render, state is a fixed snapshot.

## Key ideas

- A local variable is lost between renders and does not trigger one.
- `useState` returns the current value and a function that requests a new render.
- Within one render the value does not change, even right after setting it.
- Several updates in a handler are applied together.
- Pass a function `x => x + 1` when the next value depends on the previous one.
- Never mutate objects or arrays in state; create new ones.
- State belongs to a position in the tree; `key` or a different type resets it.

## Minimal example

```tsx
function Counter() {
  const [n, setN] = useState(0);
  return <button onClick={() => setN(n + 1)}>{n}</button>;
}
```

## When to use and when not

- ✅ Data that changes and must be shown: input, selection, open or closed.
- ✅ Data a component must remember between renders.
- ❌ Values that can be computed from props or other state: compute them during render.
- ❌ Values that do not affect the screen: a ref is the tool for those.

## Levels of this material

| Level | What's inside |
|---|---|
| [1. Basics](basics.en.md) | `useState`, event handlers, propagation, a form |
| [2. Advanced](advanced.en.md) | snapshots, batching, updaters, objects and arrays, resetting state |
| [3. Expert](expert.en.md) | how state is stored, `flushSync`, reference, gotchas, interview questions |
| [4. Self-check](quiz.en.md) | a quiz and [exercises](exercises.en.md) |
| 5. Runnable examples | not created yet — `/learn react 03-state-and-events level 5` |
| 6. Tests | not created yet — `/learn react 03-state-and-events level 6` |

## Related materials

- Prerequisites: [02 · Props and composition](../02-props-and-composition/overview.en.md)
- Closures behind snapshots: [closures in JavaScript](../../../javascript/modules/10-closures-and-functional/overview.en.md)
- Event loop and timers: [JavaScript event loop](../../../javascript/modules/13-event-loop-and-concurrency/overview.en.md)
- DOM events underneath: [browser DOM and events](../../../javascript/modules/17-browser-dom-and-events/overview.en.md)
- Next: the following module of the [React roadmap](../../overview.en.md)
- [04 · Lists, keys and identity](../04-lists-and-keys/overview.en.md)
- [06 · Effects](../06-effects/overview.en.md)
