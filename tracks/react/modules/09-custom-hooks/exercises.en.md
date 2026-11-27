---
title: "Exercises: custom hooks"
lang: en
track: react
kind: module
doc: exercises
slug: 09-custom-hooks
part: 2
tags: [react, custom-hooks, hooks, use-sync-external-store, reuse]
versions: "React 19.3"
status: draft
updated: 2026-11-27
---

# Exercises: custom hooks

🌐 [Русский](exercises.ru.md) · **English**

> Solve it yourself before opening the hints.

## 1. 🟢 A counter hook

**Task:** return `count`, `increment` and `reset`; the functions should keep their identity.

<details><summary>Hint</summary>

`useCallback` and an updater function.

</details>

<details><summary>Solution</summary>

```tsx
function useCounter(initial = 0) {
  const [count, setCount] = useState(initial);
  const increment = useCallback(() => setCount(c => c + 1), []);
  const reset = useCallback(() => setCount(initial), [initial]);
  return { count, increment, reset };
}
```

</details>

## 2. 🟡 useInterval

**Task:** call the latest callback every `ms` milliseconds; `null` pauses. Changing the callback must not restart the timer.

<details><summary>Hint</summary>

React 19.2 has a hook for the "latest callback" part.

</details>

<details><summary>Solution</summary>

```tsx
function useInterval(callback: () => void, ms: number | null) {
  const onTick = useEffectEvent(callback);
  useEffect(() => {
    if (ms === null) return;
    const id = setInterval(() => onTick(), ms);
    return () => clearInterval(id);
  }, [ms]);
}
```

</details>

## 3. 🔴 useMediaQuery

**Task:** report whether a CSS media query matches and follow changes.

<details><summary>Hint</summary>

An external store: `window.matchMedia` and `useSyncExternalStore`.

</details>

<details><summary>Solution</summary>

```tsx
function subscribeQuery(query: string) {
  return (callback: () => void) => {
    const list = window.matchMedia(query);
    list.addEventListener('change', callback);
    return () => list.removeEventListener('change', callback);
  };
}

function useMediaQuery(query: string) {
  const subscribe = useMemo(() => subscribeQuery(query), [query]);
  return useSyncExternalStore(subscribe, () => window.matchMedia(query).matches, () => false);
}
```

The server snapshot is `false`, so server-rendered markup shows the non-matching variant until the browser takes over.

</details>
