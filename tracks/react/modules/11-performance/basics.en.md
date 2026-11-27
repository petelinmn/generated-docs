---
title: "Rendering performance: basics"
lang: en
track: react
kind: module
doc: basics
slug: 11-performance
part: 3
tags: [react, performance, memoization, react-compiler, profiling]
versions: "React 19.3"
status: draft
updated: 2026-11-27
---

# Rendering performance: basics

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [props and composition](../02-props-and-composition/overview.en.md), [state](../03-state-and-events/overview.en.md) · **Time:** ~2 h

All results were produced with React 19.3.1.

## Why a component renders

- its state changed;
- its parent rendered;
- a context it reads changed.

Rendering means calling the function. The DOM changes only where the result differs, but the function and its children still run.

## Move state down

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

After one click in `Up`, `Expensive` runs once more; in `Down` it does not run at all.

## Pass elements instead of creating them

A wrapper with state that receives `children` does not re-render them: see [composition](../02-props-and-composition/advanced.en.md).

## Beginner mistakes

- ❌ Optimising without measuring → ✅ find the slow interaction first.
- ❌ All state in the top component → ✅ keep it where it is used.
- ❌ Thinking that a render always changes the DOM.

## Related materials

- [Advanced theory](advanced.en.md)

## Sources

- [React: memo](https://react.dev/reference/react/memo)
- [React: useMemo](https://react.dev/reference/react/useMemo)
- [React: useCallback](https://react.dev/reference/react/useCallback)
- [React Compiler: introduction](https://react.dev/learn/react-compiler/introduction)
