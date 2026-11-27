---
title: "Exercises: refs and the DOM"
lang: en
track: react
kind: module
doc: exercises
slug: 07-refs-and-dom
part: 2
tags: [react, refs, dom, portals, hooks]
versions: "React 19.3"
status: draft
updated: 2026-11-27
---

# Exercises: refs and the DOM

🌐 [Русский](exercises.ru.md) · **English**

> Solve it yourself before opening the hints.

## 1. 🟢 Autofocus

**Task:** a labelled input that receives focus when it appears.

<details><summary>Hint</summary>

When does the node exist?

</details>

<details><summary>Solution</summary>

```tsx
function AutoFocusInput({ label }: { label: string }) {
  const input = useRef<HTMLInputElement>(null);
  useEffect(() => { input.current?.focus(); }, []);
  return <label>{label} <input ref={input} /></label>;
}
```

</details>

## 2. 🟡 A debounced search

**Task:** call `onSearch` 300 ms after the user stops typing; never after the component is gone.

<details><summary>Hint</summary>

The timer id must survive renders; clean it up on unmount.

</details>

<details><summary>Solution</summary>

```tsx
function Debounced({ onSearch }: { onSearch: (q: string) => void }) {
  const timer = useRef<ReturnType<typeof setTimeout>>(undefined);
  useEffect(() => () => clearTimeout(timer.current), []);
  return (
    <input
      aria-label="search"
      onChange={e => {
        const value = e.target.value;
        clearTimeout(timer.current);
        timer.current = setTimeout(() => onSearch(value), 300);
      }}
    />
  );
}
```

</details>

## 3. 🔴 Scroll to the new message

**Task:** after adding a message, scroll the list to it.

<details><summary>Hint</summary>

The new `<li>` is not in the DOM right after `setMessages`.

</details>

<details><summary>Solution</summary>

```tsx
function Chat() {
  const [messages, setMessages] = useState(['hi']);
  const list = useRef<HTMLUListElement>(null);

  function add(text: string) {
    flushSync(() => setMessages(m => [...m, text]));
    list.current?.lastElementChild?.scrollIntoView?.({ block: 'nearest' });
  }

  return (
    <>
      <ul ref={list}>{messages.map((m, i) => <li key={i}>{m}</li>)}</ul>
      <button onClick={() => add('new')}>Add</button>
    </>
  );
}
```

The optional call `scrollIntoView?.()` keeps the code working where the method does not exist, as in jsdom.

</details>
