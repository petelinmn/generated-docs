---
title: "Rendering performance: advanced"
lang: en
track: react
kind: module
doc: advanced
slug: 11-performance
part: 3
tags: [react, performance, memoization, react-compiler, profiling]
versions: "React 19.3"
status: draft
updated: 2026-11-27
---

# Rendering performance: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

## memo

`memo(Component)` skips rendering when all props are the same by `Object.is`. It helps only if the props really stay the same:

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

## useMemo and useCallback

`useMemo` keeps a computed value, `useCallback` keeps a function; both recompute when a dependency changes.

```tsx
const total = useMemo(() => list.reduce((a, b) => a + b, 0), [list]);
// render with the same list array and another prop: not recomputed
// render with a new array with the same numbers:    recomputed
```

Use them to keep props of memoized components stable, or to avoid an expensive calculation. They are optimisations: React may discard the cache in specific cases, so code must work without them.

## Measuring

- The React DevTools profiler shows what rendered and why.
- `<Profiler>` reports timings from code:

```tsx
<Profiler id="app" onRender={(id, phase, actualDuration, baseDuration, startTime, commitTime) => report(id, phase, actualDuration)}>
  <App />
</Profiler>
// phase: 'mount', 'update' or 'nested-update'
```

It is disabled in production builds by default. Development builds also add React tracks to the browser's Performance panel. Measure the production build for real numbers.

## Long lists

Rendering thousands of rows is slow even with memoization. A virtualised list renders only the visible ones.

## Best practices

- Fix the structure first, memoize second.
- Give stable keys.
- Compute derived data during render and memoize only what is expensive.

## Common mistakes

- ❌ `memo` on a component whose props are new each time → ✅ stabilise them or remove `memo`.
- ❌ A custom comparator that ignores a prop → stale output.
- ❌ Judging speed in the development build.

## Related materials

- [Basics](basics.en.md)
- [Expert theory](expert.en.md)

## Sources

- [React: memo](https://react.dev/reference/react/memo)
- [React: useMemo](https://react.dev/reference/react/useMemo)
- [React: useCallback](https://react.dev/reference/react/useCallback)
- [React Compiler: introduction](https://react.dev/learn/react-compiler/introduction)
- [React Compiler 1.0](https://react.dev/blog/2025/10/07/react-compiler-1)
- [React: Profiler](https://react.dev/reference/react/Profiler)
- [React: Performance tracks](https://react.dev/reference/dev-tools/react-performance-tracks)
