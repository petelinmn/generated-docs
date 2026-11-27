---
title: "Exercises: state and events"
lang: en
track: react
kind: module
doc: exercises
slug: 03-state-and-events
part: 1
tags: [react, state, use-state, events, hooks]
versions: "React 19.3"
status: draft
updated: 2026-11-27
---

# Exercises: state and events

🌐 [Русский](exercises.ru.md) · **English**

> Solve it yourself before opening the hints.

## 1. 🟢 A stepper

**Task:** buttons `-`, `+` and `+2`; the value never goes below zero.

<details><summary>Hint</summary>

Why would `change(1); change(1)` with `setN(n + 1)` add only one?

</details>

<details><summary>Solution</summary>

```tsx
function Stepper() {
  const [n, setN] = useState(0);
  const change = (by: number) => setN(x => Math.max(0, x + by));
  return (
    <>
      <button onClick={() => change(-1)}>-</button>
      <output>{n}</output>
      <button onClick={() => change(+1)}>+</button>
      <button onClick={() => { change(+1); change(+1); }}>+2</button>
    </>
  );
}
```

The updater function makes two calls in a row add up.

</details>

## 2. 🟡 A to-do list without mutation

**Task:** write `add`, `toggle` and `remove` that return new arrays.

<details><summary>Hint</summary>

Spread, `map`, `filter`.

</details>

<details><summary>Solution</summary>

```tsx
type Todo = { id: number; text: string; done: boolean };

const add = (list: Todo[], id: number, text: string): Todo[] => [...list, { id, text, done: false }];
const toggle = (list: Todo[], id: number) => list.map(t => (t.id === id ? { ...t, done: !t.done } : t));
const remove = (list: Todo[], id: number) => list.filter(t => t.id !== id);

// in the component:  setTodos(list => toggle(list, id));
```

Only the changed item is a new object; the others are reused.

</details>

## 3. 🔴 A fresh editor for each user

**Task:** an input is initialised from the user's name; switching users must show the new name.

<details><summary>Hint</summary>

`useState` uses its initial value once. What gives a component a new identity?

</details>

<details><summary>Solution</summary>

```tsx
function Editor({ userId, initialName }: { userId: string; initialName: string }) {
  const [name, setName] = useState(initialName);
  return <input value={name} onChange={e => setName(e.target.value)} />;
}

// in the parent: a different user gets a fresh state
<Editor key={user.id} userId={user.id} initialName={user.name} />
```

An effect that copies the prop into state would render the old value first; the `key` avoids that.

</details>
