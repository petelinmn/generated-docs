---
title: "Context and reducers: expert"
lang: en
track: react
kind: module
doc: expert
slug: 08-context-and-reducers
part: 2
tags: [react, context, use-reducer, state-management, hooks]
versions: "React 19.3"
status: draft
updated: 2026-11-27
---

# Context and reducers: expert

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · **3. Expert** · [4. Quiz](quiz.en.md)

## Under the hood

`useReducer` keeps a queue of actions; on the next render React runs the reducer over them in order. `useState` is the same mechanism with a built-in reducer. For context React looks up the tree for the nearest provider and re-renders consumers when its value is not `Object.is` equal to the previous one.

### use

```tsx
function Maybe({ on }: { on: boolean }) {
  if (!on) return <i>off</i>;
  const theme = use(Theme);              // allowed after a condition
  return <i>{theme}</i>;
}
```

`use` reads a context like `useContext` but may be called in conditions and loops. It cannot be used to read context in Server Components.

## Reference

| API | Purpose |
|---|---|
| `useReducer(reducer, initial)` | state with a reducer |
| `useReducer(reducer, arg, init)` | `init(arg)` computes the initial state once |
| `dispatch(action)` | request an update |
| `createContext(default)` | create a context |
| `<Context value={v}>` | provide (React 19) |
| `<Context.Provider value={v}>` | the older way to provide |
| `useContext(Context)` | read the closest value |
| `use(Context)` | read, allowed conditionally |

## Gotchas

### A mutating reducer doubles its effect

```tsx
function mutating(s: { items: string[] }, a: string) {
  s.items.push(a);
  return { items: s.items };
}
// inside <StrictMode>, one dispatch('a'):  the screen shows  a,a
```

### undefined is not "no provider"

```tsx
<Theme value={undefined}><Label /></Theme>   // <b></b> — not the default 'light'
```

### You cannot consume what you provide

```tsx
function Self() {
  const theme = useContext(Theme);                         // from above, not from below
  return <Theme value="inner">{theme}<Label /></Theme>;    // light<b>inner</b>
}
```

### An unmemoized value

```tsx
<User value={{ name: 'Ann' }}>            {/* a new object on every render */}
const value = useMemo(() => ({ name: 'Ann' }), []);
<User value={value}>                      {/* the same object */}
```

```text
two re-renders of the provider's owner:
new object every time:  consumer 2, memo consumer 2, memo component without context 0
memoized value:         consumer 0, memo consumer 0, memo component without context 0
```

## Trade-offs and alternatives

| Need | Option | Note |
|---|---|---|
| Share between close components | lift state up | explicit |
| Skip middle layers | pass elements as `children` | no hidden dependency |
| App-wide, rarely changing | context | simple |
| App-wide, often changing, selective reading | an external store | subscribers choose what they read |
| Server data | a data library | caching and refetching |

## Interview questions

**1. `useState` or `useReducer`?**

<details><summary>Answer</summary>

`useReducer` when updates are many and related; the rules sit in one testable function.

</details>

**2. Why must a reducer be pure?**

<details><summary>Answer</summary>

React may call it more than once; StrictMode does so in development.

</details>

**3. Is `dispatch` stable?**

<details><summary>Answer</summary>

Yes.

</details>

**4. When is the default value of a context used?**

<details><summary>Answer</summary>

Only when there is no provider above.

</details>

**5. Does `memo` stop updates from context?**

<details><summary>Answer</summary>

No.

</details>

**6. How do you reduce re-renders caused by context?**

<details><summary>Answer</summary>

Memoize the value, split contexts, move state closer to where it is used.

</details>

**7. How does `use` differ from `useContext`?**

<details><summary>Answer</summary>

`use` may be called inside conditions and loops.

</details>

**8. Is context a state manager?**

<details><summary>Answer</summary>

It only delivers a value; state and updates come from `useState`, `useReducer` or a store.

</details>

**9. Trick question: `<Theme value={undefined}>` — what do consumers get?**

<details><summary>Answer</summary>

`undefined`, not the default.

</details>

**10. Trick question: can a component read the context it provides itself?**

<details><summary>Answer</summary>

No, `useContext` looks above the component.

</details>

**11. Can a reducer return the same state?**

<details><summary>Answer</summary>

Yes; React skips re-rendering children, though it may call the component.

</details>

## Related materials

- [Advanced theory](advanced.en.md)
- [Quiz](quiz.en.md) and [exercises](exercises.en.md)

## Sources

- [React: useReducer](https://react.dev/reference/react/useReducer)
- [React: createContext](https://react.dev/reference/react/createContext)
- [React: useContext](https://react.dev/reference/react/useContext)
- [React: passing data deeply with context](https://react.dev/learn/passing-data-deeply-with-context)
- [React: use](https://react.dev/reference/react/use)
