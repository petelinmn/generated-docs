---
title: "Refs and the DOM: advanced"
lang: en
track: react
kind: module
doc: advanced
slug: 07-refs-and-dom
part: 2
tags: [react, refs, dom, portals, hooks]
versions: "React 19.3"
status: draft
updated: 2026-11-27
---

# Refs and the DOM: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

## Callback refs

Instead of an object, `ref` accepts a function.

```tsx
<p ref={el => {
  box.current = el;                      // called with the node
  return () => { box.current = null; };  // React 19: cleanup on removal
}}>x</p>

// without a returned cleanup React calls the callback again with null:  element | null
```

## Many elements

Hooks cannot be called in a loop, so keep one ref with a map:

```tsx
const map = useRef(new Map<string, HTMLLIElement>());

{items.map(x => (
  <li key={x} ref={el => { map.current.set(x, el!); return () => { map.current.delete(x); }; }}>{x}</li>
))}

// later:  map.current.get('b')?.scrollIntoView();
```

## Passing a ref to your component

```tsx
function MyInput({ ref }: { ref?: Ref<HTMLInputElement> }) {
  return <input ref={ref} />;          // React 19: ref is a prop
}
// a component that does not use the ref prop leaves ref.current === null
```

Before React 19 this needed `forwardRef`.

## Updating, then using the DOM

```tsx
setItems(i => [...i, 'p']);
list.current!.lastElementChild!.textContent;     // 'a'  — the DOM is not updated yet

flushSync(() => setItems(i => [...i, 's']));
list.current!.lastElementChild!.textContent;     // 's'
```

`flushSync` forces the update to happen at once. It hurts performance, so use it only when you need the new DOM right away, such as scrolling to an added item.

## Portals

```tsx
function Modal() {
  return (
    <div onClick={() => log.push('parent heard')}>
      {createPortal(<button>in</button>, document.getElementById('modal')!)}
    </div>
  );
}
// DOM:    #root contains <div></div>;  #modal contains <button>in</button>
// click the button:  parent heard
```

A portal changes only where the DOM node lives. Context and events follow the React tree. It is used for dialogs, tooltips and menus that must escape `overflow` or stacking of their parent. A dialog still needs focus management and keyboard support.

## Best practices

- Prefer declarative props (`autoFocus`, `open`) when they exist.
- Clean up timers and observers held in refs.
- Expose a narrow handle rather than a DOM node.

## Common mistakes

- ❌ A new callback ref doing expensive work every render → ✅ define it outside or memoize.
- ❌ Reading layout in `useEffect` and then moving things → ✅ `useLayoutEffect`.
- ❌ Expecting events from a portal not to reach React parents → ✅ they do.

## Related materials

- [Basics](basics.en.md)
- [Expert theory](expert.en.md)

## Sources

- [React: useRef](https://react.dev/reference/react/useRef)
- [React: manipulating the DOM with refs](https://react.dev/learn/manipulating-the-dom-with-refs)
- [React: common components, ref callback](https://react.dev/reference/react-dom/components/common)
- [React: useImperativeHandle](https://react.dev/reference/react/useImperativeHandle)
- [React: createPortal](https://react.dev/reference/react-dom/createPortal)
