---
title: "Context and reducers: basics"
lang: en
track: react
kind: module
doc: basics
slug: 08-context-and-reducers
part: 2
tags: [react, context, use-reducer, state-management, hooks]
versions: "React 19.3"
status: draft
updated: 2026-11-27
---

# Context and reducers: basics

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [state and events](../03-state-and-events/overview.en.md) · **Time:** ~2 h

All results were produced with React 19.3.1.

## useReducer

```tsx
type State = { count: number };
type Action = { type: 'inc' } | { type: 'add'; by: number };

function reducer(state: State, action: Action): State {
  switch (action.type) {
    case 'inc': return { count: state.count + 1 };
    case 'add': return { count: state.count + action.by };
  }
}

function Counter() {
  const [state, dispatch] = useReducer(reducer, { count: 0 });
  return <button onClick={() => dispatch({ type: 'inc' })}>{state.count}</button>;
}
```

`dispatch` sends an action; React calls the reducer with the current state and renders with the result. The union type of actions lets TypeScript check each branch.

```tsx
dispatch({ type: 'inc' }); dispatch({ type: 'inc' }); dispatch({ type: 'inc' });
console.log(state.count);   // 0: the current render still has the old state
// next screen: 3, one render
```

## Context

```tsx
const Theme = createContext('light');

function Label() {
  return <b>{useContext(Theme)}</b>;
}

<>
  <Label />
  <Theme value="dark">
    <Label />
    <Theme value="blue"><Label /></Theme>
  </Theme>
</>
// <b>light</b><b>dark</b><b>blue</b>
```

`createContext(default)` creates it, a provider sets the value for the subtree, `useContext` reads the closest one. Since React 19 the context itself is the provider (`<Theme value=...>`); `Theme.Provider` is the older form.

## Beginner mistakes

- ❌ Changing `state` inside the reducer → ✅ return a new object.
- ❌ A request or a timer inside the reducer → ✅ in the handler or an effect.
- ❌ Forgetting the provider and silently getting the default → ✅ a hook that checks.
- ❌ Context instead of a single prop → ✅ pass the prop.

## Related materials

- [Advanced theory](advanced.en.md)

## Sources

- [React: useReducer](https://react.dev/reference/react/useReducer)
- [React: createContext](https://react.dev/reference/react/createContext)
- [React: useContext](https://react.dev/reference/react/useContext)
- [React: passing data deeply with context](https://react.dev/learn/passing-data-deeply-with-context)
