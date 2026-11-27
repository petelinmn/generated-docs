---
title: "State and events: basics"
lang: en
track: react
kind: module
doc: basics
slug: 03-state-and-events
part: 1
tags: [react, state, use-state, events, hooks]
versions: "React 19.3"
status: draft
updated: 2026-11-27
---

# State and events: basics

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [props and composition](../02-props-and-composition/overview.en.md) · **Time:** ~2 h

All results were produced with React 19.3.1.

## Why a variable is not enough

```tsx
function Local() {
  let n = 0;
  return <button onClick={() => { n++; }}>{n}</button>;   // the screen stays 0
}
```

The variable is created anew on every render, and changing it does not make React render.

## useState

```tsx
function Counter() {
  const [n, setN] = useState(0);
  return <button onClick={() => setN(n + 1)}>{n}</button>;
}
```

`useState(0)` gives a pair: the value for this render and a setter. Calling the setter schedules a render in which the value is new. Each instance of a component has its own state. Hooks such as `useState` are called only at the top level of a component.

## Event handlers

Pass a function; do not call it.

```tsx
<button onClick={log.push('called during render')}>p</button>
// the function runs while rendering, and React reports:
// Expected `onClick` listener to be a function, instead got a value of `number` type.
```

An event travels from the target up to its ancestors (all React events propagate except `onScroll`). `stopPropagation` stops it, and `onClickCapture` runs on the way down.

```tsx
<div onClick={() => log.push('div')}>
  <button onClick={() => log.push('button')}>p</button>
  <button onClick={e => { e.stopPropagation(); log.push('stopped'); }}>s</button>
</div>
// click p, then s:  button,div,stopped

<div onClickCapture={() => log.push('capture div')}>
  <button onClick={() => log.push('button')}>p</button>
</div>
// click:  capture div,button
```

## A form

```tsx
function NameForm() {
  const [name, setName] = useState('');
  const [sent, setSent] = useState('');

  function submit(e: SubmitEvent<HTMLFormElement>) {   // SubmitEvent from 'react'
    e.preventDefault();                                // no page reload
    setSent(name);
  }

  return (
    <form onSubmit={submit}>
      <input value={name} onChange={e => setName(e.target.value)} />
      <button>ok</button>
      <p>{sent}</p>
    </form>
  );
}
```

`preventDefault` cancels the browser's own reaction. `value` together with `onChange` makes the input controlled by state.

## Beginner mistakes

- ❌ `onClick={save()}` → ✅ `onClick={save}`.
- ❌ A local variable for data on screen → ✅ state.
- ❌ Forgetting `preventDefault` on submit → ✅ the page reloads otherwise.
- ❌ Reading the new value right after setting it → ✅ compute it in a variable first.

## Related materials

- [Advanced theory](advanced.en.md)

## Sources

- [React: useState](https://react.dev/reference/react/useState)
- [React: responding to events](https://react.dev/learn/responding-to-events)
- [React: state as a snapshot](https://react.dev/learn/state-as-a-snapshot)
- [React: preserving and resetting state](https://react.dev/learn/preserving-and-resetting-state)
