---
title: "Exercises: rendering performance"
lang: en
track: react
kind: module
doc: exercises
slug: 11-performance
part: 3
tags: [react, performance, memoization, react-compiler, profiling]
versions: "React 19.3"
status: draft
updated: 2026-11-27
---

# Exercises: rendering performance

🌐 [Русский](exercises.ru.md) · **English**

> Solve it yourself before opening the hints. `Item` is `{ id: number; name: string }`.

## 1. 🟢 A chart that should not care

**Task:** typing in a search box must not re-render `HeavyChart`.

<details><summary>Hint</summary>

Where does the query state need to live?

</details>

<details><summary>Solution</summary>

```tsx
function SearchBox() {
  const [query, setQuery] = useState('');
  return <input aria-label="search" value={query} onChange={e => setQuery(e.target.value)} />;
}

function Page() {
  return <><SearchBox /><HeavyChart /></>;
}
```

No memoization is needed.

</details>

## 2. 🟡 Rows that stay still

**Task:** removing one row must not re-render the others.

<details><summary>Hint</summary>

`memo` plus a stable callback.

</details>

<details><summary>Solution</summary>

```tsx
const Row = memo(function Row({ item, onRemove }: { item: Item; onRemove: (id: number) => void }) {
  return <li>{item.name} <button onClick={() => onRemove(item.id)}>x</button></li>;
});

function List() {
  const [items, setItems] = useState<Item[]>(initial);
  const onRemove = useCallback((id: number) => setItems(list => list.filter(i => i.id !== id)), []);
  return <ul>{items.map(item => <Row key={item.id} item={item} onRemove={onRemove} />)}</ul>;
}
```

The updater function lets `onRemove` have no dependencies.

</details>

## 3. 🔴 Filtering only when needed

**Task:** recompute the filtered rows only when the rows or the query change.

<details><summary>Hint</summary>

`useMemo` and its dependencies.

</details>

<details><summary>Solution</summary>

```tsx
function Table({ rows, query }: { rows: Item[]; query: string }) {
  const visible = useMemo(
    () => rows.filter(r => r.name.toLowerCase().includes(query.toLowerCase())),
    [rows, query],
  );
  return <ul>{visible.map(r => <li key={r.id}>{r.name}</li>)}</ul>;
}
```

It helps only if the parent does not create a new `rows` array on every render.

</details>
