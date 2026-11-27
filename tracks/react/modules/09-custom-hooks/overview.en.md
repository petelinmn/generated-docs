---
title: "Custom hooks"
lang: en
track: react
kind: module
doc: overview
slug: 09-custom-hooks
part: 2
levels: [1, 2, 3, 4]
tags: [react, custom-hooks, hooks, use-sync-external-store, reuse]
versions: "React 19.3"
status: draft
updated: 2026-11-26
---

# Custom hooks

🌐 [Русский](overview.ru.md) · **English**

## In a nutshell

A custom hook is a function whose name starts with `use` and which calls other hooks. It lets components reuse logic that involves state and effects: subscriptions, timers, storage, requests. Each component that calls it gets its own state: the hook shares the logic, not the data.

## Key ideas

- The name starts with `use` and a capital letter.
- Hooks are called only from components and other hooks, at the top level.
- Every call has its own state and effects.
- The body runs on every render of the component.
- Return stable values when others may depend on them.
- Data really shared between components lives in a store, context or a common parent.
- A function that calls no hooks does not need the prefix.

## Minimal example

```tsx
export function useToggle(initial = false) {
  const [on, setOn] = useState(initial);
  const toggle = useCallback(() => setOn(v => !v), []);
  return [on, toggle] as const;
}
```

## When to use and when not

- ✅ The same effect or state logic appears in several components.
- ✅ Hiding the details of a browser API or a subscription.
- ❌ Wrapping a lifecycle moment, such as "run on mount".
- ❌ Pure calculations: an ordinary function is enough.

## Levels of this material

| Level | What's inside |
|---|---|
| [1. Basics](basics.en.md) | writing a hook, rules, independent state |
| [2. Advanced](advanced.en.md) | hooks with effects, stored values, external stores |
| [3. Expert](expert.en.md) | `useSyncExternalStore`, `useDebugValue`, gotchas, interview questions |
| [4. Self-check](quiz.en.md) | a quiz and [exercises](exercises.en.md) |
| 5. Runnable examples | not created yet — `/learn react 09-custom-hooks level 5` |
| 6. Tests | not created yet — `/learn react 09-custom-hooks level 6` |

## Related materials

- Prerequisites: [06 · Effects](../06-effects/overview.en.md), [07 · Refs and the DOM](../07-refs-and-dom/overview.en.md), [08 · Context and reducers](../08-context-and-reducers/overview.en.md)
- Closures that hooks rely on: [closures in JavaScript](../../../javascript/modules/10-closures-and-functional/overview.en.md)
- Generic types for hooks: [TypeScript track](../../../typescript/overview.en.md)
- Next: the following module of the [React roadmap](../../overview.en.md)
- [10 · Testing](../10-testing/overview.en.md)
