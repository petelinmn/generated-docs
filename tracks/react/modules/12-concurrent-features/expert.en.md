---
title: "Concurrent features: expert"
lang: en
track: react
kind: module
doc: expert
slug: 12-concurrent-features
part: 3
tags: [react, suspense, transitions, concurrent-rendering, error-boundaries]
versions: "React 19.3"
status: draft
updated: 2026-11-27
---

# Concurrent features: expert

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · **3. Expert** · [4. Quiz](quiz.en.md)

## Under the hood

React may start rendering an update, pause or abandon it if a more urgent one arrives, and commit only complete results. A component "suspends" by not being ready (its code or data is pending); React then shows the nearest boundary's fallback, unless the update is a transition and the content is already visible.

## Activity

```tsx
<Activity mode={on ? 'visible' : 'hidden'}><Panel /></Activity>
// count to 1, hide:  display: none !important;  effects cleaned up
// show again:        the count is still 1;       effects run again
```

`<Activity>` (listed among React 19.2 features) hides a subtree without destroying its state.

## New in 19.3

React 19.3 made `<ViewTransition>` stable: it animates changes that happen in transitions and Suspense reveals. Details are in the release post.

## Reference

| API | Purpose |
|---|---|
| `<Suspense fallback>` | placeholder while not ready |
| `lazy(load)` | code splitting |
| `use(promise)` | read a cached promise |
| `useTransition()` | `[isPending, startTransition]` |
| `startTransition(fn)` | transition outside a component hook |
| `useDeferredValue(value, initialValue?)` | lagging copy of a value |
| `<Activity mode>` | hide keeping state |
| `<ViewTransition>` | animate transitions |
| `getDerivedStateFromError`, `componentDidCatch` | error boundary |

## Gotchas

### The boundary swallows the whole section

```tsx
<Suspense fallback={<p>loading</p>}>
  <h1>title</h1>
  <Wait />               {/* suspends */}
</Suspense>
// <p>loading</p>  — the title is hidden too
```

### A plain update hides the old page

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

### Errors in handlers are not caught

```tsx
<Boundary><button onClick={() => { throw new Error('in handler'); }}>x</button></Boundary>
// the button stays; the error is reported globally, the boundary does not catch it
```

### Updates after await

```tsx
start(async () => {
  await save();
  startTransition(() => setMessage('done'));   // wrap updates after await again
});
// meanwhile isPending is true
```

## Trade-offs and alternatives

| Choice | Pro | Con |
|---|---|---|
| Fallback spinner | simple | content jumps |
| Transition keeping old content | calm | the user needs a pending sign |
| Debounce | fewer requests | fixed delay |
| `useDeferredValue` | adapts to the device | does not reduce requests |
| Hidden with `Activity` | state kept | memory |

## Interview questions

**1. What is `Suspense` for?**

<details><summary>Answer</summary>

To show a fallback while a child is not ready.

</details>

**2. What activates it?**

<details><summary>Answer</summary>

`lazy` components, `use` with a cached promise, Suspense-enabled framework data. Not effect fetching.

</details>

**3. What is a transition?**

<details><summary>Answer</summary>

A non-urgent, interruptible update that does not hide already revealed content.

</details>

**4. `useTransition` or `useDeferredValue`?**

<details><summary>Answer</summary>

The first when you control the state update; the second when you only receive the value.

</details>

**5. Is `useDeferredValue` a debounce?**

<details><summary>Answer</summary>

No, there is no fixed delay.

</details>

**6. What do error boundaries not catch?**

<details><summary>Answer</summary>

Event handlers, asynchronous callbacks, server rendering and their own errors. Errors in `startTransition` functions are an exception and are caught.

</details>

**7. What does `Activity` do when hidden?**

<details><summary>Answer</summary>

Hides with `display: none`, cleans up effects, keeps state.

</details>

**8. Trick question: why does a promise created in render loop with `use`?**

<details><summary>Answer</summary>

Each render creates a new one, so it is never ready.

</details>

**9. Trick question: can an error boundary be a function component?**

<details><summary>Answer</summary>

No.

</details>

**10. Can you use `try`/`catch` around `use`?**

<details><summary>Answer</summary>

No.

</details>

**11. Why does a controlled input stay urgent?**

<details><summary>Answer</summary>

Typing must be reflected immediately; transition updates cannot control inputs.

</details>

## Related materials

- [Advanced theory](advanced.en.md)
- [Quiz](quiz.en.md) and [exercises](exercises.en.md)

## Sources

- [React: Suspense](https://react.dev/reference/react/Suspense)
- [React: useTransition](https://react.dev/reference/react/useTransition)
- [React: useDeferredValue](https://react.dev/reference/react/useDeferredValue)
- [React: use](https://react.dev/reference/react/use)
- [React: error boundaries](https://react.dev/reference/react/Component)
- [React: Activity](https://react.dev/reference/react/Activity)
- [React 19.3](https://react.dev/blog/2026/09/09/react-19-3)
