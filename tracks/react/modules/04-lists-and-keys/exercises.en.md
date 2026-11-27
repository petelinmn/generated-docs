---
title: "Exercises: lists, keys and identity"
lang: en
track: react
kind: module
doc: exercises
slug: 04-lists-and-keys
part: 1
tags: [react, keys, lists, identity, state]
versions: "React 19.3"
status: draft
updated: 2026-11-27
---

# Exercises: lists, keys and identity

🌐 [Русский](exercises.ru.md) · **English**

> Solve it yourself before opening the hints.

## 1. 🟢 A grouped menu

**Task:** for every group show a heading and a list of its items, without extra wrapper elements.

<details><summary>Hint</summary>

Two levels of `map`, and a fragment that takes a key.

</details>

<details><summary>Solution</summary>

```tsx
type Group = { id: string; title: string; items: { id: string; label: string }[] };

function Menu({ groups }: { groups: Group[] }) {
  return <>
    {groups.map(g => (
      <Fragment key={g.id}>
        <h3>{g.title}</h3>
        <ul>{g.items.map(i => <li key={i.id}>{i.label}</li>)}</ul>
      </Fragment>
    ))}
  </>;
}
```

</details>

## 2. 🟡 Stable ids for new rows

**Task:** rows are added on the client; give each an id that never changes.

<details><summary>Hint</summary>

When exactly is a row born?

</details>

<details><summary>Solution</summary>

```tsx
type Row = { id: string; text: string };

function useRows() {
  const [rows, setRows] = useState<Row[]>([]);
  const add = (text: string) => setRows(rs => [...rs, { id: crypto.randomUUID(), text }]);
  return { rows, add };
}
```

`crypto.randomUUID()` is available in browsers only in secure contexts (HTTPS or localhost).

</details>

## 3. 🔴 Tabs that remember drafts

**Task:** switching tabs must not lose the text typed in each of them.

<details><summary>Hint</summary>

Removing a component loses its state. What else can hide it?

</details>

<details><summary>Solution</summary>

```tsx
function Tabs({ tabs }: { tabs: { id: string; title: string }[] }) {
  const [active, setActive] = useState(tabs[0].id);
  return <>
    {tabs.map(t => <button key={t.id} onClick={() => setActive(t.id)}>{t.title}</button>)}
    {tabs.map(t => (
      <section key={t.id} hidden={t.id !== active}>
        <textarea placeholder={t.title} />
      </section>
    ))}
  </>;
}
```

All panels stay mounted; `hidden` only removes them from view.

</details>
