---
title: "Refs and the DOM: basics"
lang: en
track: react
kind: module
doc: basics
slug: 07-refs-and-dom
part: 2
tags: [react, refs, dom, portals, hooks]
versions: "React 19.3"
status: draft
updated: 2026-11-27
---

# Refs and the DOM: basics

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [effects](../06-effects/overview.en.md) · **Time:** ~1.5 h

All results were produced with React 19.3.1 in jsdom.

## A ref to a DOM node

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

Give a ref to `ref` of an element, and after the commit `ref.current` is that node.

## Ref or state

| | Ref | State |
|---|---|---|
| Change causes a render | no | yes |
| Read in render | no | yes |
| Value after changing | immediately | on the next render |
| Typical content | DOM node, timer id | what is shown |

```tsx
const ref = useRef(0);
<button onClick={() => { ref.current++; }}>{ref.current}</button>
// two clicks: ref.current is 2, no render, the button still shows 0
// after any state change elsewhere in the component it shows 2
```

## When the ref is available

```tsx
function When() {
  const ref = useRef<HTMLDivElement>(null);
  log.push(ref.current === null ? 'render null' : 'render DIV');
  useEffect(() => { log.push('effect ' + ref.current!.tagName); });
  return <div ref={ref} />;
}
// first mount:  render null | effect DIV
```

During the first render the node does not exist yet. Use refs in event handlers and effects.

## Remembering a value

```tsx
const id = useRef<ReturnType<typeof setTimeout>>(undefined);
const start = () => { clearTimeout(id.current); id.current = setTimeout(send, 1000); };
const cancel = () => clearTimeout(id.current);
```

## Beginner mistakes

- ❌ Showing `ref.current` in JSX → ✅ state.
- ❌ Using a ref during render → ✅ in a handler or an effect.
- ❌ Removing a React node with `remove()` → ✅ change state.
- ❌ A module variable for a timer id → ✅ a ref: each instance gets its own.

## Related materials

- [Advanced theory](advanced.en.md)

## Sources

- [React: useRef](https://react.dev/reference/react/useRef)
- [React: manipulating the DOM with refs](https://react.dev/learn/manipulating-the-dom-with-refs)
