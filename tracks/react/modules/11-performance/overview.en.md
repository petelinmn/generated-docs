---
title: "Rendering performance"
lang: en
track: react
kind: module
doc: overview
slug: 11-performance
part: 3
levels: [1, 2, 3, 4]
tags: [react, performance, memoization, react-compiler, profiling]
versions: "React 19.3"
status: draft
updated: 2026-11-27
---

# Rendering performance

🌐 [Русский](overview.ru.md) · **English**

## In a nutshell

React re-renders a component when its state changes, when its parent renders, or when a context it reads changes. Usually this is cheap enough. When it is not, the answer is first to find what is slow, then to render less: move state closer to where it is used, pass ready elements, memoize, or let React Compiler do the memoization at build time.

## Key ideas

- Measure before optimising.
- A parent's render renders its children unless something lets React skip them.
- State kept close to its users affects fewer components.
- `memo` compares props by reference.
- `useMemo` and `useCallback` keep values and functions the same between renders.
- Memoization is an optimisation, never a part of correctness.
- React Compiler memoizes automatically at build time.

## Minimal example

```tsx
// state at the top: typing re-renders Expensive
function Up() {
  const [t, setT] = useState('');
  return <><button onClick={() => setT(t + 'x')}>{t}</button><Expensive /></>;
}

// state moved into the field: Expensive is not rendered again
function Field() {
  const [t, setT] = useState('');
  return <button onClick={() => setT(t + 'x')}>{t}</button>;
}
function Down() { return <><Field /><Expensive /></>; }
```

## When to use and when not

- ✅ A measured slow interaction.
- ✅ Heavy components or long lists that render often.
- ❌ Wrapping everything in `memo` in advance.
- ❌ Relying on memoization to make code correct.

## Levels of this material

| Level | What's inside |
|---|---|
| [1. Basics](basics.en.md) | why components render, moving state down |
| [2. Advanced](advanced.en.md) | `memo`, `useMemo`, `useCallback`, measuring |
| [3. Expert](expert.en.md) | React Compiler, reference, gotchas, interview questions |
| [4. Self-check](quiz.en.md) | a quiz and [exercises](exercises.en.md) |
| 5. Runnable examples | not created yet — `/learn react 11-performance level 5` |
| 6. Tests | not created yet — `/learn react 11-performance level 6` |

## Related materials

- Prerequisites: [02 · Props and composition](../02-props-and-composition/overview.en.md), [04 · Lists and keys](../04-lists-and-keys/overview.en.md), [08 · Context](../08-context-and-reducers/overview.en.md)
- Measuring and percentiles: [design process](../../../system-design/modules/01-design-process/overview.en.md)
- Main thread and long tasks: [event loop in JavaScript](../../../javascript/modules/13-event-loop-and-concurrency/overview.en.md)
- Next: the following module of the [React roadmap](../../overview.en.md)
