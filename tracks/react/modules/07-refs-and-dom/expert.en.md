---
title: "Refs and the DOM: expert"
lang: en
track: react
kind: module
doc: expert
slug: 07-refs-and-dom
part: 2
tags: [react, refs, dom, portals, hooks]
versions: "React 19.3"
status: draft
updated: 2026-11-27
---

# Refs and the DOM: expert

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · **3. Expert** · [4. Quiz](quiz.en.md)

## Under the hood

`useRef` returns the same object on every render of an instance; React never looks inside it. For `ref` on an element, React attaches during the commit and detaches when the element goes away or the ref changes.

### A narrow handle

```tsx
type Handle = { focus(): void; clear(): void };

function Field({ ref }: { ref: Ref<Handle> }) {
  const input = useRef<HTMLInputElement>(null);
  useImperativeHandle(ref, () => ({
    focus: () => input.current!.focus(),
    clear: () => { input.current!.value = ''; },
  }), []);
  return <input ref={input} defaultValue="abc" />;
}
// the parent's ref has only focus and clear, not the DOM node
```

## Reference

| API | Purpose |
|---|---|
| `useRef(initial)` | a persistent box `{ current }` |
| `ref={object}` | React puts the DOM node into `current` |
| `ref={fn}` | called with the node; may return cleanup |
| `useImperativeHandle(ref, create, deps)` | what the parent's ref receives |
| `createPortal(children, node)` | render into another DOM node |
| `flushSync(fn)` | apply updates immediately |

| Task | Method on the node |
|---|---|
| Focus | `focus()`, `blur()` |
| Scroll | `scrollIntoView()` |
| Size and position | `getBoundingClientRect()` |
| Media | `play()`, `pause()` |
| Native dialog | `showModal()`, `close()` |

## Gotchas

### An inline callback ref re-attaches

```tsx
<p ref={() => { log.push('attach'); return () => { log.push('cleanup'); }; }}>{n}</p>
// render n=1, then n=2:  attach | cleanup | attach
```

### Writing a ref during render

```tsx
function Dirty() {
  const r = useRef(0);
  r.current++;                 // writing a ref during render
  return <p>{r.current}</p>;
}
// inside <StrictMode>, the first screen shows 2
```

### The DOM is not updated yet

```tsx
setItems(i => [...i, 'p']);
list.current!.lastElementChild!.textContent;     // 'a'  — the DOM is not updated yet

flushSync(() => setItems(i => [...i, 's']));
list.current!.lastElementChild!.textContent;     // 's'
```

### A component that ignores `ref`

```tsx
function MyInput({ ref }: { ref?: Ref<HTMLInputElement> }) {
  return <input ref={ref} />;          // React 19: ref is a prop
}
// a component that does not use the ref prop leaves ref.current === null
```

## Trade-offs and alternatives

| Need | Option | Note |
|---|---|---|
| Control a child | props | the normal way |
| | a ref handle | imperative, for focus, scroll, play |
| Overlay above everything | portal | manage focus yourself |
| | native `<dialog>` and popover | browser handles much of it |
| Measure size changes | `ResizeObserver` in an effect | instead of polling |

## Interview questions

**1. Ref versus state?**

<details><summary>Answer</summary>

A ref changes immediately and causes no render; state causes one and is used for what is shown.

</details>

**2. Why not read a ref during render?**

<details><summary>Answer</summary>

Rendering must be pure, and the value may be stale or not yet set.

</details>

**3. When does React set a DOM ref?**

<details><summary>Answer</summary>

During the commit.

</details>

**4. How do you keep refs for a list?**

<details><summary>Answer</summary>

One ref holding a map, filled by callback refs.

</details>

**5. What changed for callback refs in React 19?**

<details><summary>Answer</summary>

They may return a cleanup function; without it React calls them with `null`.

</details>

**6. What is `useImperativeHandle` for?**

<details><summary>Answer</summary>

To expose chosen methods to a parent instead of the DOM node. Prefer props when possible.

</details>

**7. What is a portal?**

<details><summary>Answer</summary>

Rendering children into another DOM node while keeping them in the same React tree.

</details>

**8. Do events from a portal bubble to the React parent?**

<details><summary>Answer</summary>

Yes.

</details>

**9. Trick question: why does a callback ref run on every render?**

<details><summary>Answer</summary>

An inline function is new each time, so React detaches the old one and attaches the new one.

</details>

**10. Trick question: you set state and immediately read the DOM. What do you see?**

<details><summary>Answer</summary>

The old DOM, unless the update was wrapped in `flushSync`.

</details>

## Related materials

- [Advanced theory](advanced.en.md)
- [Quiz](quiz.en.md) and [exercises](exercises.en.md)

## Sources

- [React: useRef](https://react.dev/reference/react/useRef)
- [React: manipulating the DOM with refs](https://react.dev/learn/manipulating-the-dom-with-refs)
- [React: common components, ref callback](https://react.dev/reference/react-dom/components/common)
- [React: useImperativeHandle](https://react.dev/reference/react/useImperativeHandle)
- [React: createPortal](https://react.dev/reference/react-dom/createPortal)
