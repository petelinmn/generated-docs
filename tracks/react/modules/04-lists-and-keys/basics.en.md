---
title: "Lists, keys and identity: basics"
lang: en
track: react
kind: module
doc: basics
slug: 04-lists-and-keys
part: 1
tags: [react, keys, lists, identity, state]
versions: "React 19.3"
status: draft
updated: 2026-11-27
---

# Lists, keys and identity: basics

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [state and events](../03-state-and-events/overview.en.md) · **Time:** ~1.5 h

All results were produced with React 19.3.1.

## From data to elements

```tsx
const users = [{ id: 'u1', name: 'Ann' }, { id: 'u2', name: 'Bob' }];

<ul>
  {users.map(u => <li key={u.id}>{u.name}</li>)}
</ul>
```

`map` returns an array, and React renders each element of it.

## Rules for keys

- Unique among siblings (not across the whole application).
- Stable: the same item keeps the same key between renders.
- Written on the outermost element returned from `map`.
- Not passed to the component: if it needs the id, pass it as another prop.

```tsx
<ul>{['a', 'b'].map(x => <li>{x}</li>)}</ul>
// console: Each child in a list should have a unique "key" prop.

<ul>{['a', 'a'].map(x => <li key={x}>{x}</li>)}</ul>
// console: Encountered two children with the same key ...

<><ul><li key="1">x</li></ul><ul><li key="1">y</li></ul></>
// no message: keys have to be unique only among siblings
```

## Filtering and sorting

Compute the visible list during render; do not keep a second copy in state.

```tsx
const visible = products
  .filter(p => p.name.toLowerCase().includes(query.toLowerCase()))
  .toSorted((a, b) => a.name.localeCompare(b.name));

return <ul>{visible.map(p => <ProductRow key={p.id} product={p} />)}</ul>;
```

`toSorted` returns a new array and leaves the original intact.

## Beginner mistakes

- ❌ No `key` → ✅ an id from the data.
- ❌ `key` on an inner element → ✅ on the element returned by `map`.
- ❌ Reading `props.key` → ✅ pass a separate prop.
- ❌ `products.sort(...)` on state or props → ✅ `toSorted` or a copy.

## Related materials

- [Advanced theory](advanced.en.md)

## Sources

- [React: rendering lists](https://react.dev/learn/rendering-lists)
- [React: preserving and resetting state](https://react.dev/learn/preserving-and-resetting-state)
