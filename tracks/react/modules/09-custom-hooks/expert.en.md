---
title: "Custom hooks: expert"
lang: en
track: react
kind: module
doc: expert
slug: 09-custom-hooks
part: 2
tags: [react, custom-hooks, hooks, use-sync-external-store, reuse]
versions: "React 19.3"
status: draft
updated: 2026-11-27
---

# Custom hooks: expert

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · **3. Expert** · [4. Quiz](quiz.en.md)

## Under the hood

A custom hook is not a special thing for React: its hook calls are simply part of the calling component's list of hooks, matched by order. That is why the rules of Hooks apply inside it too, and why each component that calls it has separate state.

### useDebugValue

`useDebugValue(label, format?)` adds a label to your hook in React DevTools; the formatting function runs only when the component is inspected. It is mostly useful in shared libraries.

## Reference

| API | Purpose |
|---|---|
| custom `useX` | reuse stateful logic |
| `useSyncExternalStore(subscribe, getSnapshot, getServerSnapshot?)` | read an external store |
| `useDebugValue` | label in DevTools |
| `useCallback`, `useMemo` | keep returned values stable |
| `useEffectEvent` | non-reactive logic inside an effect |

| Return style | When |
|---|---|
| single value | one result |
| `[value, action] as const` | behaves like state |
| object | three or more named members |

## Gotchas

### getSnapshot returning a new object

```tsx
useSyncExternalStore(store.subscribe, () => ({ v: n }));   // a new object on every call
// console: The result of getSnapshot should be cached to avoid an infinite loop
// Error:   Maximum update depth exceeded. ...
```

### An unstable returned value

```tsx
function useBad() { return { value: 1 }; }          // a new object every render

const obj = useBad();
useEffect(() => { runs++; }, [obj]);
// three renders:  the effect ran 3 times
```

### A shared hook is not shared data

```tsx
function Two({ id }: { id: string }) {
  const [on, toggle] = useToggle();
  return <button id={id} onClick={toggle}>{String(on)}</button>;
}
// <Two id="a" /><Two id="b" />, click a:  true false
```

## Trade-offs and alternatives

| Reuse of | Tool |
|---|---|
| calculation | a plain function |
| markup | a component |
| stateful logic | a custom hook |
| data between components | props, context or a store |
| server data | a data library with caching |

## Interview questions

**1. What is a custom hook?**

<details><summary>Answer</summary>

A function starting with `use` that calls hooks and packages reusable logic.

</details>

**2. Do two callers share state?**

<details><summary>Answer</summary>

No.

</details>

**3. Why the `use` prefix?**

<details><summary>Answer</summary>

So readers and the linter know the rules of Hooks apply.

</details>

**4. Where can hooks be called?**

<details><summary>Answer</summary>

At the top level of components and custom hooks.

</details>

**5. What is `useSyncExternalStore` for?**

<details><summary>Answer</summary>

Subscribing to a store outside React and reading a consistent snapshot.

</details>

**6. What must `getSnapshot` return?**

<details><summary>Answer</summary>

The same immutable value until the store changes.

</details>

**7. Why declare `subscribe` outside the component?**

<details><summary>Answer</summary>

A new function each render makes React resubscribe.

</details>

**8. What is the third argument of `useSyncExternalStore`?**

<details><summary>Answer</summary>

The snapshot for server rendering and hydration; without it server rendering fails.

</details>

**9. What is `useDebugValue`?**

<details><summary>Answer</summary>

A label for a custom hook in React DevTools.

</details>

**10. Trick question: a function without `use` calls `useState`. Does it fail?**

<details><summary>Answer</summary>

It may work at run time, but the linter cannot check it, and it is against the rules.

</details>

**11. Trick question: is every reusable function a hook?**

<details><summary>Answer</summary>

No, only those that call hooks.

</details>

## Related materials

- [Advanced theory](advanced.en.md)
- [Quiz](quiz.en.md) and [exercises](exercises.en.md)

## Sources

- [React: reusing logic with custom hooks](https://react.dev/learn/reusing-logic-with-custom-hooks)
- [React: rules of Hooks](https://react.dev/reference/rules/rules-of-hooks)
- [React: useSyncExternalStore](https://react.dev/reference/react/useSyncExternalStore)
- [React: useDebugValue](https://react.dev/reference/react/useDebugValue)
- [React: useEffectEvent](https://react.dev/reference/react/useEffectEvent)
