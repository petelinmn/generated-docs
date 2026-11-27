---
title: "Context and reducers"
lang: en
track: react
kind: module
doc: overview
slug: 08-context-and-reducers
part: 2
levels: [1, 2, 3, 4]
tags: [react, context, use-reducer, state-management, hooks]
versions: "React 19.3"
status: draft
updated: 2026-11-27
---

# Context and reducers

🌐 [Русский](overview.ru.md) · **English**

## In a nutshell

A reducer moves the rules for changing state into one function: components describe what happened, and the function decides what the new state is. Context makes a value available to a whole subtree without passing it through every level. They are often used together, but they solve different problems: one organises updates, the other delivers data.

## Key ideas

- A reducer takes the state and an action and returns the next state.
- It must be pure and must not mutate.
- `dispatch` keeps the same identity between renders.
- A consumer reads the closest provider above it; without one, the default.
- Every consumer re-renders when the provided value changes.
- A new object as the value counts as a change.
- Try props and `children` before reaching for context.

## Minimal example

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

## When to use and when not

- ✅ Reducer: several related values, many kinds of updates.
- ✅ Context: theme, current user, language, a shared dispatch.
- ❌ Context for data one level away: pass a prop.
- ❌ One huge context with everything: every change re-renders every reader.

## Levels of this material

| Level | What's inside |
|---|---|
| [1. Basics](basics.en.md) | `useReducer`, `createContext`, `useContext` |
| [2. Advanced](advanced.en.md) | the two together, re-rendering, splitting contexts |
| [3. Expert](expert.en.md) | `use`, reference, gotchas, interview questions |
| [4. Self-check](quiz.en.md) | a quiz and [exercises](exercises.en.md) |
| 5. Runnable examples | not created yet — `/learn react 08-context-and-reducers level 5` |
| 6. Tests | not created yet — `/learn react 08-context-and-reducers level 6` |

## Related materials

- Prerequisites: [03 · State and events](../03-state-and-events/overview.en.md), [02 · Props and composition](../02-props-and-composition/overview.en.md)
- The same idea of events producing state: [event sourcing and CQRS](../../../system-design/modules/13-event-sourcing-and-cqrs/overview.en.md)
- Discriminated unions for actions: [TypeScript track](../../../typescript/overview.en.md)
- Next: the following module of the [React roadmap](../../overview.en.md)
