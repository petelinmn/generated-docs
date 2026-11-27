---
title: "State and events: expert"
lang: en
track: react
kind: module
doc: expert
slug: 03-state-and-events
part: 1
tags: [react, state, use-state, events, hooks]
versions: "React 19.3"
status: draft
updated: 2026-11-27
---

# State and events: expert

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · **3. Expert** · [4. Quiz](quiz.en.md)

## Under the hood

React keeps state outside your function, per position in the tree. On each render the Hooks of a component are matched with the stored values **by the order of calls**, which is why the order must not change:

```tsx
function Cond({ extra }: { extra: boolean }) {
  if (extra) { useState(0); }          // breaks the rules of Hooks
  const [n] = useState(1);
  return <p>{n}</p>;
}
// rendering extra={false}, then extra={true}:
// Error: Rendered more hooks than during the previous render.
```

A setter puts an update into a queue; React processes the queue on the next render, applying values and updater functions in order. The setter's identity is stable, and it returns nothing.

### StrictMode

```tsx
// inside <StrictMode>, development:
// component body on mount: 2 calls
// setN(v => v + 1) once: the updater runs 2 times, the value becomes 1
```

Updater functions must therefore be pure.

### Forcing an update into the DOM

```tsx
onClick={() => {
  console.log(box.textContent);                  // 0
  flushSync(() => setN(n + 1));                  // from 'react-dom'
  console.log(box.textContent);                  // 1: the DOM is already updated
}}
```

`flushSync` can significantly hurt performance; use it as a last resort.

## Reference

| Call | Effect |
|---|---|
| `useState(value)` | initial value, used once |
| `useState(fn)` | `fn` computes the initial value on the first render |
| `set(value)` | replace on the next render |
| `set(prev => next)` | compute from the queued value |
| same value (`Object.is`) | the subtree is not re-rendered |

| Event feature | Syntax |
|---|---|
| Handler | `onClick={handle}` |
| Capture phase | `onClickCapture` |
| Stop propagation | `e.stopPropagation()` |
| Cancel the browser default | `e.preventDefault()` |
| Source and current element | `e.target`, `e.currentTarget` |
| No propagation | `onScroll` |

| What happens to state | When |
|---|---|
| kept | same component type at the same position |
| reset | different type, different `key`, or removed |

## Gotchas

### A function as a state value

```tsx
const [f, setF] = useState<() => string>(() => () => 'first');
setF(() => () => 'second');     // wrap: otherwise React treats a function as an updater
```

### Initializer or call

```tsx
useState(expensive);      // called once, on the first render
useState(expensive());    // called on every render; the result is ignored after the first
// after the first render and two updates: 1 call versus 3
```

### A component declared inside another

```tsx
function Parent() {
  const [t, setT] = useState(0);
  function Inner() {                       // a new function on every render
    const [n, setN] = useState(0);
    return <button onClick={() => setN(n + 1)}>inner {n}</button>;
  }
  return <><Inner /><button onClick={() => setT(t + 1)}>outer {t}</button></>;
}
// click inner:  inner 1 outer 0
// click outer:  inner 0 outer 1
```

### An unconditional update during render

```tsx
function Loop() {
  const [n, setN] = useState(0);
  setN(n + 1);                        // unconditional set during render
  return <p>{n}</p>;
}
// Error: Too many re-renders. React limits the number of renders to prevent an infinite loop.
```

### target is not currentTarget

```tsx
<div id="outer" onClick={e => console.log((e.target as HTMLElement).id, e.currentTarget.id)}>
  <button id="inner">x</button>
</div>
// click the button:  inner outer
```

## Trade-offs and alternatives

| Option | Pro | Con |
|---|---|---|
| Several `useState` | independent, simple | related updates are scattered |
| One state object | grouped | must copy on every change |
| `useReducer` | update logic in one function | more code |
| A ref | no re-render | the screen does not update |
| Derived values | never out of sync | recomputed on each render |
| An external store | shared across the app | a dependency and its own rules |

## Interview questions

**1. Why does a log right after `setN` show the old value?**

<details><summary>Answer</summary>

`n` is a constant of the current render; the new value appears in the next one.

</details>

**2. `setN(n + 1)` three times: what is the result?**

<details><summary>Answer</summary>

`n + 1`. With `x => x + 1` it would be `n + 3`.

</details>

**3. What is batching?**

<details><summary>Answer</summary>

React applies several updates in one render; since React 18 also in timeouts and promises.

</details>

**4. Why must state not be mutated?**

<details><summary>Answer</summary>

React compares by reference and may skip the update; old snapshots are also corrupted.

</details>

**5. Does setting the same value cause a render?**

<details><summary>Answer</summary>

Children are skipped; React may still call the component itself once.

</details>

**6. How does lazy initialization work?**

<details><summary>Answer</summary>

Pass a function: it is called on the initial render only (twice in StrictMode development).

</details>

**7. How do you reset a component's state?**

<details><summary>Answer</summary>

Give it a different `key`.

</details>

**8. Why can Hooks not be called conditionally?**

<details><summary>Answer</summary>

React matches state to Hooks by the order of calls.

</details>

**9. What is `flushSync` for?**

<details><summary>Answer</summary>

To apply an update to the DOM immediately, for example before measuring. It hurts performance and is a last resort.

</details>

**10. Trick question: is setting state during render always an error?**

<details><summary>Answer</summary>

An unconditional one loops. A guarded one for the same component is a supported, rarely needed pattern.

</details>

**11. Trick question: `e.target` and `e.currentTarget`?**

<details><summary>Answer</summary>

`target` is where the event happened, `currentTarget` is the element whose handler is running.

</details>

## Related materials

- [Advanced theory](advanced.en.md)
- [Quiz](quiz.en.md) and [exercises](exercises.en.md)

## Sources

- [React: useState](https://react.dev/reference/react/useState)
- [React: responding to events](https://react.dev/learn/responding-to-events)
- [React: state as a snapshot](https://react.dev/learn/state-as-a-snapshot)
- [React: preserving and resetting state](https://react.dev/learn/preserving-and-resetting-state)
- [React: updating arrays in state](https://react.dev/learn/updating-arrays-in-state)
- [React: flushSync](https://react.dev/reference/react-dom/flushSync)
- [React: rules of Hooks](https://react.dev/reference/rules/rules-of-hooks)
- [React 18: automatic batching](https://react.dev/blog/2022/03/29/react-v18)
