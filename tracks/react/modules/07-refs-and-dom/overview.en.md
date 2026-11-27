---
title: "Refs and the DOM"
lang: en
track: react
kind: module
doc: overview
slug: 07-refs-and-dom
part: 2
levels: [1, 2, 3, 4]
tags: [react, refs, dom, portals, hooks]
versions: "React 19.3"
status: draft
updated: 2026-11-27
---

# Refs and the DOM

🌐 [Русский](overview.ru.md) · **English**

## In a nutshell

A ref is a box that survives renders and whose change does not cause one. It serves two purposes: remembering values the screen does not depend on (timer ids, previous values), and reaching the DOM nodes React created, to focus, scroll or measure them. A portal is a related tool: it puts part of the interface into another place in the DOM while keeping it in the same React tree.

## Key ideas

- `ref.current` can change without a render.
- Do not read or write it during rendering.
- React sets DOM refs during the commit, after render.
- A callback ref may return a cleanup function (React 19).
- Function components receive `ref` as a prop (React 19).
- `useImperativeHandle` exposes a small API instead of the whole node.
- Events from a portal bubble through the React tree.

## Minimal example

```tsx
function Search() {
  const input = useRef<HTMLInputElement>(null);
  return (
    <>
      <input ref={input} />
      <button onClick={() => input.current?.focus()}>Focus</button>
    </>
  );
}
```

## When to use and when not

- ✅ Focus, text selection, scrolling, measuring, media playback.
- ✅ Timer ids and other values that are not shown.
- ❌ Data that appears on screen: that is state.
- ❌ Changing DOM nodes that React manages.

## Levels of this material

| Level | What's inside |
|---|---|
| [1. Basics](basics.en.md) | `useRef`, DOM access, ref versus state |
| [2. Advanced](advanced.en.md) | callback refs, lists of refs, `ref` as a prop, `flushSync`, portals |
| [3. Expert](expert.en.md) | `useImperativeHandle`, reference, gotchas, interview questions |
| [4. Self-check](quiz.en.md) | a quiz and [exercises](exercises.en.md) |
| 5. Runnable examples | not created yet — `/learn react 07-refs-and-dom level 5` |
| 6. Tests | not created yet — `/learn react 07-refs-and-dom level 6` |

## Related materials

- Prerequisites: [06 · Effects](../06-effects/overview.en.md)
- The DOM API itself: [browser DOM and events](../../../javascript/modules/17-browser-dom-and-events/overview.en.md)
- `ref` as a prop in composition: [02 · Props and composition](../02-props-and-composition/overview.en.md)
- Next: the following module of the [React roadmap](../../overview.en.md)
