---
title: "Lists, keys and identity"
lang: en
track: react
kind: module
doc: overview
slug: 04-lists-and-keys
part: 1
levels: [1, 2, 3, 4]
tags: [react, keys, lists, identity, state]
versions: "React 19.3"
status: draft
updated: 2026-11-26
---

# Lists, keys and identity

🌐 [Русский](overview.ru.md) · **English**

## In a nutshell

A list in React is an array of elements. Each needs a `key`: a name by which React recognises the same item on the next render. Keys decide which component keeps which state, which DOM node is reused and which is created anew. The same rules about identity explain when state survives a change of the interface and when it disappears.

## Key ideas

- A key identifies an item among its siblings.
- Take keys from the data: a database id, or one created when the item is created.
- An index as a key follows the position, not the item.
- A random key creates every item anew on each render.
- Without keys React warns and falls back to the position.
- State survives while the same component type stays at the same position.
- Changing `key` is the way to reset a component on purpose.

## Minimal example

```tsx
const users = [{ id: 'u1', name: 'Ann' }, { id: 'u2', name: 'Bob' }];

<ul>
  {users.map(u => <li key={u.id}>{u.name}</li>)}
</ul>
```

## When to use and when not

- ✅ Any array of elements: use stable keys.
- ✅ Resetting a form or a view for another record: change its `key`.
- ❌ An index as a key for a list that is reordered, filtered or edited.
- ❌ Generating keys during render.

## Levels of this material

| Level | What's inside |
|---|---|
| [1. Basics](basics.en.md) | `map`, `key`, where keys go, filtering and sorting |
| [2. Advanced](advanced.en.md) | index and random keys compared, fragments, resetting state |
| [3. Expert](expert.en.md) | how identity works, positions and slots, gotchas, interview questions |
| [4. Self-check](quiz.en.md) | a quiz and [exercises](exercises.en.md) |
| 5. Runnable examples | not created yet — `/learn react 04-lists-and-keys level 5` |
| 6. Tests | not created yet — `/learn react 04-lists-and-keys level 6` |

## Related materials

- Prerequisites: [03 · State and events](../03-state-and-events/overview.en.md)
- `map`, `filter`, `toSorted`: [arrays in JavaScript](../../../javascript/modules/07-arrays-and-collections/overview.en.md)
- The same notion of identity in data: [data modeling](../../../postgresql/modules/03-data-modeling-ddl/overview.en.md)
- Next: the following module of the [React roadmap](../../overview.en.md)
- [05 · Forms](../05-forms/overview.en.md)
- [11 · Rendering performance](../11-performance/overview.en.md)
