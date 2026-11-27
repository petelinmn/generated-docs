---
title: "Effects: expert"
lang: en
track: react
kind: module
doc: expert
slug: 06-effects
part: 2
tags: [react, effects, use-effect, hooks, cleanup]
versions: "React 19.3"
status: draft
updated: 2026-11-27
---

# Effects: expert

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · **3. Expert** · [4. Quiz](quiz.en.md)

## Under the hood

After a render React commits the changes to the DOM and then processes effects: for each effect whose dependencies changed it runs the old cleanup and the new setup. Effects normally run after the browser has painted, but React may run them before the paint, for example when the update was caused by an interaction. Do not rely on the exact moment.

### useEffectEvent

Added in React 19.2. An effect event is a function that always sees the latest props and state, and is not a reason for the effect to run again.

```tsx
function Chat({ room, theme }: { room: string; theme: string }) {
  const onMessage = useEffectEvent(() => { show(room, theme); });   // sees the latest props

  useEffect(() => {
    const connection = createConnection(room);
    connection.on('message', onMessage);
    connection.connect();
    return () => connection.disconnect();
  }, [room]);                                  // theme is not a reason to reconnect
}
// theme changes from light to dark: one connection, and the next message sees "dark"
```

Rules: call it only from effects (including layout and insertion effects) or other effect events, not during render; do not pass it to other components or hooks; do not list it in the dependencies.

## Reference

| Hook | Runs | Use for |
|---|---|---|
| `useEffect` | after the commit | synchronising with external systems |
| `useLayoutEffect` | before the repaint, blocking it | measuring and correcting layout |
| `useInsertionEffect` | before layout effects | CSS-in-JS libraries inserting styles |
| `useEffectEvent` | when an effect calls it | reading fresh values without re-running |

| Event | What React calls |
|---|---|
| mount | setup |
| a dependency changed | old cleanup, then new setup |
| unmount | cleanup |
| StrictMode, development | one extra setup and cleanup on mount |

## Gotchas

### An async function as an effect

```tsx
useEffect(async () => { await load(); }, []);
// console: ... must not return anything besides a function, which is used for clean-up.
// on unmount: TypeError: destroy is not a function

useEffect(() => {
  async function run() { await load(); }
  run();
}, []);
```

### A dependency that is new every render

```tsx
const options = { v };                               // a new object on every render
useEffect(() => { counts.none++; });                 // no array
useEffect(() => { counts.empty++; }, []);
useEffect(() => { counts.obj++; }, [options]);
useEffect(() => { counts.prim++; }, [v]);
// three renders with the same v:  {"none":3,"empty":1,"obj":3,"prim":1}
```

### State set from an effect that depends on it

```tsx
const [o, setO] = useState({});
useEffect(() => { setO({}); }, [o]);    // a new object each time: never settles
// console: Maximum update depth exceeded. This can happen when a component calls setState inside useEffect, ...
```

### An effect that derives data renders twice

```tsx
// avoid: an extra render with an outdated value
const [full, setFull] = useState('');
useEffect(() => { setFull(first + ' Smith'); }, [first]);
// renders:  "" then "Ann Smith"

// instead
const full = first + ' Smith';
```

## Trade-offs and alternatives

| Task | Option | Note |
|---|---|---|
| Loading data | effect with `fetch` | handle races, errors and caching yourself |
| | a data library | caching, retries, deduplication |
| | a framework's loader or Server Components | data before the page is shown |
| External store | `useSyncExternalStore` | made for subscriptions |
| Derived data | calculation, `useMemo` if expensive | no effect |

## Interview questions

**1. When does an effect run?**

<details><summary>Answer</summary>

After the commit, on the client. Its timing relative to painting can differ.

</details>

**2. What is cleanup for?**

<details><summary>Answer</summary>

To undo the setup before the next run and on unmount.

</details>

**3. `[]` versus no array?**

<details><summary>Answer</summary>

`[]` runs once after the first commit; without an array it runs after every commit.

</details>

**4. How are dependencies compared?**

<details><summary>Answer</summary>

With `Object.is`.

</details>

**5. Why does the effect run twice in development?**

<details><summary>Answer</summary>

StrictMode adds a setup and cleanup cycle to expose missing cleanup.

</details>

**6. `useEffect` versus `useLayoutEffect`?**

<details><summary>Answer</summary>

The layout effect runs before the repaint and blocks it; use it to measure and adjust the layout.

</details>

**7. What is `useEffectEvent`?**

<details><summary>Answer</summary>

A way to call, from an effect, logic that reads the latest props and state without making the effect re-run.

</details>

**8. How do you handle out-of-order responses?**

<details><summary>Answer</summary>

Set an ignore flag in cleanup, or abort the request.

</details>

**9. Trick question: why can an effect not be `async`?**

<details><summary>Answer</summary>

It would return a promise instead of a cleanup function; React then fails when it calls it.

</details>

**10. Trick question: do effects run on the server?**

<details><summary>Answer</summary>

No.

</details>

**11. Which runs first, the parent effect or the child effect?**

<details><summary>Answer</summary>

The child.

</details>

## Related materials

- [Advanced theory](advanced.en.md)
- [Quiz](quiz.en.md) and [exercises](exercises.en.md)

## Sources

- [React: useEffect](https://react.dev/reference/react/useEffect)
- [React: you might not need an effect](https://react.dev/learn/you-might-not-need-an-effect)
- [React: useLayoutEffect](https://react.dev/reference/react/useLayoutEffect)
- [React: useEffectEvent](https://react.dev/reference/react/useEffectEvent)
- [React 19.2](https://react.dev/blog/2025/10/01/react-19-2)
