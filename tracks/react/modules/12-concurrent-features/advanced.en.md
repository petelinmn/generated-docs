---
title: "Concurrent features: advanced"
lang: en
track: react
kind: module
doc: advanced
slug: 12-concurrent-features
part: 3
tags: [react, suspense, transitions, concurrent-rendering, error-boundaries]
versions: "React 19.3"
status: draft
updated: 2026-11-27
---

# Concurrent features: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

## Transitions

```tsx
function Tabs() {
  const [id, setId] = useState('one');
  const [isPending, start] = useTransition();
  return <>
    <button onClick={() => start(() => setId('two'))}>go</button>
    <span>{isPending ? 'pending' : 'idle'}</span>
    <Suspense fallback={<p>loading</p>}><Page id={id} /></Suspense>
  </>;
}
```

```text
while page two is loading
  plain setId:      idle      old page hidden, "loading" shown
  inside start():   pending   old page ONE stays on screen
afterwards: idle, TWO
```

A transition says: this update may wait and may be interrupted. React keeps the content that is already shown and reports `isPending` instead of switching to the fallback. Transition updates cannot control a text input.

## Asynchronous actions

```tsx
start(async () => {
  await save();
  startTransition(() => setMessage('done'));   // wrap updates after await again
});
// meanwhile isPending is true
```

## Deferred values

```tsx
function Results({ q }: { q: string }) {
  const deferred = useDeferredValue(q);
  return <List query={deferred} />;
}
// q changes from "a" to "ab": two renders, first with deferred = "a", then "ab"

useDeferredValue('real', 'placeholder');   // first render: placeholder, then real
```

`useDeferredValue` re-renders in the background with the new value. It is not a debounce: there is no fixed delay, and it helps only if the slow part can be skipped (for example with `memo`).

## Reading a promise with use

```tsx
const namePromise = fetchName();          // created once, outside the component

function Name() { return <b>{use(namePromise)}</b>; }

<Suspense fallback={<p>loading</p>}><Name /></Suspense>
```

The promise must be cached: one created during rendering is new every time. A rejected promise goes to the nearest error boundary. `use` cannot be called inside `try`/`catch`.

## Best practices

- Mark navigation and heavy updates as transitions.
- Show `isPending` so the user knows something happens.
- Take data promises from a framework or a data library that caches them.

## Common mistakes

- ❌ Creating the promise in the component → ✅ cache it.
- ❌ Setting state after `await` without `startTransition` → ✅ wrap it again.
- ❌ `useDeferredValue` with a child that is not memoized → ✅ it re-renders anyway.

## Related materials

- [Basics](basics.en.md)
- [Expert theory](expert.en.md)

## Sources

- [React: Suspense](https://react.dev/reference/react/Suspense)
- [React: useTransition](https://react.dev/reference/react/useTransition)
- [React: useDeferredValue](https://react.dev/reference/react/useDeferredValue)
- [React: use](https://react.dev/reference/react/use)
