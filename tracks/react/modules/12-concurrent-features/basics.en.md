---
title: "Concurrent features: basics"
lang: en
track: react
kind: module
doc: basics
slug: 12-concurrent-features
part: 3
tags: [react, suspense, transitions, concurrent-rendering, error-boundaries]
versions: "React 19.3"
status: draft
updated: 2026-11-27
---

# Concurrent features: basics

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [effects](../06-effects/overview.en.md), promises · **Time:** ~2 h

All results were produced with React 19.3.1.

## Loading code on demand

```tsx
const Chart = lazy(() => import('./Chart'));

<Suspense fallback={<p>loading</p>}>
  <Chart />
</Suspense>
// first <p>loading</p>, then the chart
```

`lazy` takes a function that returns a dynamic import of a module with a default export. While the code is loading the nearest `Suspense` boundary shows its fallback.

## Where to put the boundary

Everything inside the boundary is replaced:

```tsx
<Suspense fallback={<p>loading</p>}>
  <h1>title</h1>
  <Wait />               {/* suspends */}
</Suspense>
// <p>loading</p>  — the title is hidden too
```

Place boundaries around the parts that may load, not around the whole page.

## Errors

An error boundary shows a fallback when rendering below it throws. It can only be written as a class (or taken from a library).

```tsx
class Boundary extends Component<{ children: ReactNode }, { error: string }> {
  state = { error: '' };
  static getDerivedStateFromError(e: Error) { return { error: e.message }; }
  render() {
    return this.state.error ? <p role="alert">{this.state.error}</p> : this.props.children;
  }
}

<Boundary><Suspense fallback={<p>loading</p>}><Fail /></Suspense></Boundary>
// a rejected promise read by use() renders <p role="alert">no data</p>
```

## Beginner mistakes

- ❌ One boundary around the whole application → ✅ boundaries around sections.
- ❌ Fetching in an effect and expecting `Suspense` → ✅ it does not notice.
- ❌ No error boundary next to loading → ✅ add one: loading can fail.

## Related materials

- [Advanced theory](advanced.en.md)

## Sources

- [React: Suspense](https://react.dev/reference/react/Suspense)
- [React: useTransition](https://react.dev/reference/react/useTransition)
- [React: useDeferredValue](https://react.dev/reference/react/useDeferredValue)
- [React: use](https://react.dev/reference/react/use)
- [React: error boundaries](https://react.dev/reference/react/Component)
