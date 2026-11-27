---
title: "Custom hooks: advanced"
lang: en
track: react
kind: module
doc: advanced
slug: 09-custom-hooks
part: 2
tags: [react, custom-hooks, hooks, use-sync-external-store, reuse]
versions: "React 19.3"
status: draft
updated: 2026-11-27
---

# Custom hooks: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

## A hook with an effect

```tsx
export function useDebounced<T>(value: T, ms: number): T {
  const [debounced, setDebounced] = useState(value);
  useEffect(() => {
    const id = setTimeout(() => setDebounced(value), ms);
    return () => clearTimeout(id);
  }, [value, ms]);
  return debounced;
}
// value goes a, ab, abc quickly:  immediately "abc:a", after the delay "abc:abc"
```

The cleanup of each change cancels the previous timer.

## Remembering in storage

```tsx
export function useStoredState(key: string, initial: string) {
  const [value, setValue] = useState(() => {
    try { return localStorage.getItem(key) ?? initial; } catch { return initial; }
  });
  useEffect(() => {
    try { localStorage.setItem(key, value); } catch { /* storage may be unavailable */ }
  }, [key, value]);
  return [value, setValue] as const;
}
// first render shows "guest" and stores it; after set('Ann') a remounted component shows "Ann"
```

Storage may be unavailable, so reads and writes are wrapped. This hook renders `initial` on a server, where there is no `localStorage`, which can differ from what the browser later shows.

## Subscribing to something outside React

```tsx
function subscribe(callback: () => void) {             // outside the component: stable
  window.addEventListener('online', callback);
  window.addEventListener('offline', callback);
  return () => {
    window.removeEventListener('online', callback);
    window.removeEventListener('offline', callback);
  };
}

export function useOnline() {
  return useSyncExternalStore(subscribe, () => navigator.onLine, () => true);
}
// server render uses the third argument and prints "online"
```

`useSyncExternalStore` is made for this. It also lets several components share a store:

```tsx
function useCount() { return useSyncExternalStore(store.subscribe, store.get); }
// two components using useCount(), click one:  both show 1
```

## Designing a hook

| Question | Guideline |
|---|---|
| What to return | a pair for state-like values, an object for several named things |
| Functions returned | wrap in `useCallback` if callers may put them into dependencies |
| Parameters | accept values, not refs, unless needed |
| Name | what it gives (`useOnline`), not when it runs |

## Best practices

- Keep a hook about one concern.
- Compose small hooks into larger ones.
- Test hooks through a component that uses them.

## Common mistakes

- ❌ `useMount(fn)` → ✅ use `useEffect` directly and name the purpose.
- ❌ Returning a new object or function each time → ✅ memoize what others depend on.
- ❌ Hiding a request inside a hook without handling races → ✅ as in [effects](../06-effects/advanced.en.md).

## Related materials

- [Basics](basics.en.md)
- [Expert theory](expert.en.md)

## Sources

- [React: reusing logic with custom hooks](https://react.dev/learn/reusing-logic-with-custom-hooks)
- [React: rules of Hooks](https://react.dev/reference/rules/rules-of-hooks)
- [React: useSyncExternalStore](https://react.dev/reference/react/useSyncExternalStore)
