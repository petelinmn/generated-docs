---
title: "Exercises: concurrent features"
lang: en
track: react
kind: module
doc: exercises
slug: 12-concurrent-features
part: 3
tags: [react, suspense, transitions, concurrent-rendering, error-boundaries]
versions: "React 19.3"
status: draft
updated: 2026-11-27
---

# Exercises: concurrent features

🌐 [Русский](exercises.ru.md) · **English**

> Solve it yourself before opening the hints. `Boundary` is the class from [basics](basics.en.md).

## 1. 🟢 A chart on demand

**Task:** load the chart code only after a button click.

<details><summary>Hint</summary>

`lazy`, `Suspense` and a boolean state.

</details>

<details><summary>Solution</summary>

```tsx
const Chart = lazy(() => import('./Chart'));

function Dashboard() {
  const [show, setShow] = useState(false);
  return <>
    <button onClick={() => setShow(true)}>Show chart</button>
    {show && <Suspense fallback={<p>loading</p>}><Chart /></Suspense>}
  </>;
}
```

</details>

## 2. 🟡 A responsive search

**Task:** typing stays smooth while a long list filters; dim the list while it lags.

<details><summary>Hint</summary>

`useDeferredValue` plus `memo`.

</details>

<details><summary>Solution</summary>

```tsx
const SlowList = memo(function SlowList({ query }: { query: string }) {
  return <ul>{items.filter(i => i.includes(query)).map(i => <li key={i}>{i}</li>)}</ul>;
});

function Search() {
  const [query, setQuery] = useState('');
  const deferred = useDeferredValue(query);
  return <>
    <input aria-label="query" value={query} onChange={e => setQuery(e.target.value)} />
    <div style={{ opacity: query === deferred ? 1 : 0.5 }}><SlowList query={deferred} /></div>
  </>;
}
```

</details>

## 3. 🔴 Pages with errors

**Task:** switch pages in a transition; a failed page shows its error, and switching again recovers.

<details><summary>Hint</summary>

An error boundary keeps its error until it is remounted.

</details>

<details><summary>Solution</summary>

```tsx
function Pages({ load }: { load: (id: string) => Promise<string> }) {
  const [id, setId] = useState('home');
  const [isPending, start] = useTransition();
  return <>
    {['home', 'about'].map(page =>
      <button key={page} disabled={isPending} onClick={() => start(() => setId(page))}>{page}</button>)}
    <Boundary key={id}>
      <Suspense fallback={<p>loading</p>}><Content promise={load(id)} /></Suspense>
    </Boundary>
  </>;
}

function Content({ promise }: { promise: Promise<string> }) {
  return <article>{use(promise)}</article>;
}
// load must return the same promise for the same id (a cache)
```

`key={id}` gives each page a fresh boundary.

</details>
