---
title: "Custom hooks: basics"
lang: en
track: react
kind: module
doc: basics
slug: 09-custom-hooks
part: 2
tags: [react, custom-hooks, hooks, use-sync-external-store, reuse]
versions: "React 19.3"
status: draft
updated: 2026-11-27
---

# Custom hooks: basics

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [effects](../06-effects/overview.en.md) · **Time:** ~1.5 h

All results were produced with React 19.3.1; the hooks pass `tsc --strict`.

## A first custom hook

```tsx
export function useToggle(initial = false) {
  const [on, setOn] = useState(initial);
  const toggle = useCallback(() => setOn(v => !v), []);
  return [on, toggle] as const;
}
```

`as const` keeps the tuple type, so the caller can destructure with correct types.

## Each call is separate

```tsx
function Two({ id }: { id: string }) {
  const [on, toggle] = useToggle();
  return <button id={id} onClick={toggle}>{String(on)}</button>;
}
// <Two id="a" /><Two id="b" />, click a:  true false
```

## The rules

- Call hooks at the top level: not in conditions, loops, nested functions or `try` blocks, and not after an early return.
- Call them only from components or other hooks.
- Name a hook `use` plus a capital letter, so that people and the linter recognise it.

## Beginner mistakes

- ❌ Expecting two components to share a hook's state → ✅ lift it up or use a store.
- ❌ A helper without hooks named `useFormat` → ✅ `format`.
- ❌ A hook called inside an event handler → ✅ call it in the component and use its result in the handler.

## Related materials

- [Advanced theory](advanced.en.md)

## Sources

- [React: reusing logic with custom hooks](https://react.dev/learn/reusing-logic-with-custom-hooks)
- [React: rules of Hooks](https://react.dev/reference/rules/rules-of-hooks)
- [React: useSyncExternalStore](https://react.dev/reference/react/useSyncExternalStore)
