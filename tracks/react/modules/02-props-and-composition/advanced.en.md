---
title: "Props and composition: advanced"
lang: en
track: react
kind: module
doc: advanced
slug: 02-props-and-composition
part: 1
tags: [react, props, composition, children, typescript]
versions: "React 19.3"
status: draft
updated: 2026-11-27
---

# Props and composition: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

## Accepting the rest of the props

```tsx
type ButtonProps = ComponentProps<'button'> & { variant?: 'primary' | 'plain' };

function Button({ variant = 'plain', className = '', ...rest }: ButtonProps) {
  return <button type="button" {...rest} className={`btn ${variant} ${className}`.trim()} />;
}

<Button disabled>Save</Button>
// <button type="button" disabled="" class="btn plain">Save</button>
<Button variant="primary" type="submit" className="wide">Go</Button>
// <button type="submit" class="btn primary wide">Go</button>
```

`ComponentProps<'button'>` types everything a `<button>` accepts. Order matters: what is written after `{...rest}` overrides it, what is written before can be overridden. Here `type` has a default the caller may change, while `className` is merged rather than replaced.

## Slots

A component with several content areas takes several element props (`header`, `actions`, `children`). The component decides the layout; the caller decides the content.

## Render props

When the content depends on data the component holds, pass a function:

```tsx
function List<T>({ items, renderItem, getKey }: {
  items: T[]; renderItem: (item: T) => ReactNode; getKey: (item: T) => string;
}) {
  return <ul>{items.map(item => <li key={getKey(item)}>{renderItem(item)}</li>)}</ul>;
}

<List items={[{ id: 'a', n: 'Ann' }]} getKey={u => u.id} renderItem={u => <b>{u.n}</b>} />
// <ul><li><b>Ann</b></li></ul>
```

## Specialization

A specific component renders a general one with fixed or preset props. React uses composition for this, not inheritance.

```tsx
function Dialog({ children }: { children: ReactNode }) { return <div role="dialog">{children}</div>; }

function ConfirmDialog({ question }: { question: string }) {
  return <Dialog><p>{question}</p><Button variant="primary">Yes</Button></Dialog>;
}
// <div role="dialog"><p>Sure?</p><button type="button" class="btn primary">Yes</button></div>
```

## Refs

```tsx
function MyInput({ ref }: { ref?: Ref<HTMLInputElement> }) {
  return <input ref={ref} />;                   // React 19: ref is an ordinary prop
}

function Form() {
  const ref = useRef<HTMLInputElement>(null);
  return <><MyInput ref={ref} /><button onClick={() => ref.current?.focus()}>focus</button></>;
}
```

Before React 19 a function component needed `forwardRef`; in React 19 `ref` is a prop of function components.

## Composition and re-rendering

```tsx
let slow = 0;
function Slow() { slow++; return <p>slow</p>; }

function Inside() {
  const [n, setN] = useState(0);
  return <div><button onClick={() => setN(n + 1)}>{n}</button><Slow /></div>;
}

function Wrapper({ children }: { children: ReactNode }) {
  const [n, setN] = useState(0);
  return <div><button onClick={() => setN(n + 1)}>{n}</button>{children}</div>;
}

// <Inside />, two clicks:                     slow = 3
// <Wrapper><Slow /></Wrapper>, two clicks:    slow = 1
```

When `Inside` updates, it creates a new `<Slow />` element and `Slow` runs again. `Wrapper` receives the element ready-made from its parent: the element is the same object, so React skips it.

## Best practices

- Name event props `onSomething`, handlers `handleSomething`.
- Prefer content (`children`, slots) to configuration flags.
- Keep the prop list small; group data that always travels together.
- Extend native elements with `ComponentProps` instead of listing attributes by hand.

## Common mistakes

- ❌ `{...rest}` written last when your own values must win → ✅ write them after the spread.
- ❌ Replacing the caller's `className` → ✅ merge it.
- ❌ Spreading a whole data object into a DOM element → ✅ pass only the attributes you mean.
- ❌ Passing a value through five layers → ✅ pass a ready element, or use context.

## Related materials

- [Basics](basics.en.md)
- [Expert theory](expert.en.md)

## Sources

- [React: passing props to a component](https://react.dev/learn/passing-props-to-a-component)
- [React: thinking in React](https://react.dev/learn/thinking-in-react)
- [React: sharing state between components](https://react.dev/learn/sharing-state-between-components)
- [React 19 release](https://react.dev/blog/2024/12/05/react-19)
- [React: memo](https://react.dev/reference/react/memo)
