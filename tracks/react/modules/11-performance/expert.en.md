---
title: "Rendering performance: expert"
lang: en
track: react
kind: module
doc: expert
slug: 11-performance
part: 3
tags: [react, performance, memoization, react-compiler, profiling]
versions: "React 19.3"
status: draft
updated: 2026-11-27
---

# Rendering performance: expert

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · **3. Expert** · [4. Quiz](quiz.en.md)

## Under the hood

When a component renders, React renders the elements it returns. It may skip a child when the element is the very same object as last time, or when a `memo` component receives props equal one by one. State updates are batched, and only the committed differences touch the DOM.

## React Compiler

React Compiler 1.0 is stable. It is a build-time tool (for example a Babel plugin) that adds memoization automatically. It works with React 17 and later; versions before 19 need a `target` setting and the `react-compiler-runtime` package.

```bash
npm install --save-dev --save-exact babel-plugin-react-compiler@latest
```

```tsx
// source
function Total({ list, label }) {
  const total = list.reduce((a, b) => a + b, 0);
  return <p>{label}: {total}</p>;
}
```

```js
// output of babel-plugin-react-compiler 1.0.0
import { c as _c } from "react/compiler-runtime";
function Total(t0) {
  const $ = _c(5);
  const { list, label } = t0;
  let t1;
  if ($[0] !== list) {
    t1 = list.reduce(_temp, 0);
    $[0] = list;
    $[1] = t1;
  } else {
    t1 = $[1];
  }
  const total = t1;
  let t2;
  if ($[2] !== label || $[3] !== total) {
    t2 = <p>{label}: {total}</p>;
    $[2] = label;
    $[3] = total;
    $[4] = t2;
  } else {
    t2 = $[4];
  }
  return t2;
}
function _temp(a, b) { return a + b; }
```

The compiled function keeps previous inputs and results and recomputes only what depends on a changed value. It relies on code that follows the rules of React; lint rules in `eslint-plugin-react-hooks` report violations such as setting state during render. `useMemo` and `useCallback` still work and serve as an escape hatch.

## Reference

| Tool | Purpose |
|---|---|
| `memo(C, areEqual?)` | skip rendering when props are equal |
| `useMemo(fn, deps)` | keep a value |
| `useCallback(fn, deps)` | keep a function |
| `<Profiler id onRender>` | measure from code |
| React Compiler | memoize at build time |
| Virtualisation library | render only visible rows |
| `useTransition`, `useDeferredValue` | keep input responsive: see the next module |

## Gotchas

### A comparator that ignores a prop

```tsx
const ById = memo(
  function ById({ user }: { user: { id: number; name: string } }) { return <i>{user.name}</i>; },
  (a, b) => a.user.id === b.user.id,        // ignores the name
);
// render with {id: 1, name: 'Ann'}, then {id: 1, name: 'Bob'}:  the screen still shows Ann
```

### A memoized value and mutation

```tsx
const sum = useMemo(() => total(list), [list]);
list.push(4);        // mutation: the same array, so the memoized value is stale
```

### A changing key remounts everything

```tsx
{[1, 2, 3].map(i => <Row key={`${salt}-${i}`} />)}
// same salt: nothing remounts;  salt changes: all three rows are mounted again
```

### memo and new props

```tsx
function Parent() {
  const [n, setN] = useState(0);
  const style = useMemo(() => ({ color: 'red' }), []);
  const on = useCallback(() => {}, []);
  return <>
    <button onClick={() => setN(n + 1)}>{n}</button>
    <Plain />                                  {/* renders */}
    <Memo n={1} />                             {/* skipped */}
    <MemoObj style={{ color: 'red' }} />       {/* renders: new object */}
    <MemoFn on={() => {}} />                   {/* renders: new function */}
    <MemoStable style={style} on={on} />       {/* skipped */}
    <MemoChildren><b /></MemoChildren>         {/* renders: new element */}
  </>;
}
```

## Trade-offs and alternatives

| Choice | Pro | Con |
|---|---|---|
| Restructure state and composition | no extra cost | needs thought about design |
| Manual memoization | precise | code noise, easy to get wrong |
| React Compiler | automatic | build step, rules must be followed |
| Virtualisation | huge lists | unmounted rows, find-in-page and accessibility work |
| External store with selectors | fine-grained subscriptions | a dependency |

## Interview questions

**1. Why does a component re-render?**

<details><summary>Answer</summary>

Its state changed, its parent rendered, or a context it reads changed.

</details>

**2. What does `memo` compare?**

<details><summary>Answer</summary>

Each prop with `Object.is`.

</details>

**3. Why can `memo` do nothing?**

<details><summary>Answer</summary>

An object, function or element prop is new on every render.

</details>

**4. `useMemo` versus `useCallback`?**

<details><summary>Answer</summary>

The first keeps a computed value, the second a function.

</details>

**5. Is memoization a guarantee?**

<details><summary>Answer</summary>

No, it is an optimisation.

</details>

**6. What does React Compiler do?**

<details><summary>Answer</summary>

At build time it memoizes values and elements inside components and hooks automatically.

</details>

**7. Do you still need `useMemo` with the compiler?**

<details><summary>Answer</summary>

Rarely: they remain an escape hatch.

</details>

**8. How do you measure rendering performance?**

<details><summary>Answer</summary>

DevTools profiler, Performance tracks, `<Profiler>`.

</details>

**9. Trick question: a memoized child receives `children`. Is it skipped when the parent renders?**

<details><summary>Answer</summary>

Not if the element is created in the parent: it is a new object.

</details>

**10. Trick question: setting the same state value — is a render guaranteed not to happen?**

<details><summary>Answer</summary>

Children are skipped, but React may still call the component.

</details>

**11. Why can a long list stay slow after `memo`?**

<details><summary>Answer</summary>

The list and thousands of nodes still have to be processed; virtualise it.

</details>

## Related materials

- [Advanced theory](advanced.en.md)
- [Quiz](quiz.en.md) and [exercises](exercises.en.md)

## Sources

- [React: memo](https://react.dev/reference/react/memo)
- [React: useMemo](https://react.dev/reference/react/useMemo)
- [React: useCallback](https://react.dev/reference/react/useCallback)
- [React Compiler: introduction](https://react.dev/learn/react-compiler/introduction)
- [React Compiler 1.0](https://react.dev/blog/2025/10/07/react-compiler-1)
- [React: Profiler](https://react.dev/reference/react/Profiler)
- [React: Performance tracks](https://react.dev/reference/dev-tools/react-performance-tracks)
