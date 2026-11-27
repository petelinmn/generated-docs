---
title: "Props and composition: expert"
lang: en
track: react
kind: module
doc: expert
slug: 02-props-and-composition
part: 1
tags: [react, props, composition, children, typescript]
versions: "React 19.3"
status: draft
updated: 2026-11-27
---

# Props and composition: expert

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · **3. Expert** · [4. Quiz](quiz.en.md)

## Under the hood

Every render of a parent creates new element objects with new props objects. React compares the elements at each position; if the element is the very same object as before and nothing else requires it, the subtree is not rendered again. That is why content passed in from above behaves differently from content created inside.

### Inspecting children

`children` is whatever was written: nothing, a single value or an array.

```tsx
<Count />                       // Children.count: 0, children is undefined
<Count>a</Count>                // 1, a string
<Count>{'a'}{'b'}</Count>       // 2, an array
<Count>{false}</Count>          // 1, a boolean: empty nodes are counted too
```

`Children.count` counts empty nodes too and does not look inside fragments or rendered components.

### Cloning

```tsx
function Highlight({ children }: { children: ReactElement<{ className?: string }> }) {
  return cloneElement(children, { className: 'hl' });
}
<Highlight><p className="old">x</p></Highlight>   // <p class="hl">x</p>
```

`cloneElement` merges new props over the old ones. The React documentation calls both `Children` and `cloneElement` uncommon and a source of fragile code; render props, context, an array of data or an explicit wrapper component are the alternatives.

## Reference

| Technique | Syntax | Use for |
|---|---|---|
| Prop with default | `{ size = 40 }` | optional settings |
| Boolean shorthand | `<input disabled />` | `true` flags |
| Spread | `<A {...props} />` | forwarding |
| Rest | `({ a, ...rest })` | accepting native attributes |
| `children` | `<A>content</A>` | wrapping |
| Slot props | `footer={<B />}` | several areas |
| Render prop | `renderItem={x => <B x={x} />}` | content that needs the component's data |
| Callback prop | `onSelect={id => ...}` | reporting events |
| `ref` prop | `<A ref={r} />` | access to a DOM node (React 19) |

| Type | Meaning |
|---|---|
| `ReactNode` | anything that can be a child |
| `ReactElement<P>` | one element with props `P` |
| `ComponentProps<'tag'>` | props of a native element |
| `PropsWithChildren<P>` | `P` plus optional `children` |

## Gotchas

### Spread order decides who wins

```tsx
<Show a="1" {...{ a: '2', b: '3' }} />   // props: {"a":"2","b":"3"}
<Show {...{ a: '2' }} a="1" />           // props: {"a":"1"}
<Show key="k1" a="x" />                  // props: {"a":"x"}   — key is not passed
```

### Props are frozen in development

```tsx
function Mutate(props: { a: string }) {
  props.a = 'z';     // TypeError in development: the props object is frozen
  return null;
}
```

Do not rely on the error: in any build, changing props is wrong.

### State copied from a prop does not follow it

```tsx
function Name({ name }: { name: string }) {
  const [copy] = useState(name);        // taken only on the first render
  return <p>{copy}|{name}</p>;
}

// render <Name name="Ann" />, then <Name name="Bob" /> in the same place:
// <p>Ann|Bob</p>
```

Use the prop directly. If the copy is intentional, name the prop `initialName`.

### A default object is new on every render

`function List({ items = [] })` creates a new array each time the prop is absent, which defeats comparisons by reference in memoized children and effect dependencies. Declare the default outside the component.

## Trade-offs and alternatives

| Approach | Pro | Con |
|---|---|---|
| Many small props | explicit | long lists |
| One object prop | short | easy to create a new object each render |
| `children` and slots | flexible, fewer re-renders | the component cannot control the content |
| Render prop | access to internal data | more verbose |
| Context | no passing through layers | hidden dependency |
| Compound components with context | declarative API | more code |

## Interview questions

**1. Can a component change its props?**

<details><summary>Answer</summary>

No. They are read-only; new values come with a new render from the parent.

</details>

**2. When is a default value used?**

<details><summary>Answer</summary>

When the prop is missing or `undefined`; not for `null`, `0` or `false`.

</details>

**3. What is `children`?**

<details><summary>Answer</summary>

A prop holding what is written between the tags of the component.

</details>

**4. How does a child pass data to its parent?**

<details><summary>Answer</summary>

By calling a function received in props.

</details>

**5. What is lifting state up?**

<details><summary>Answer</summary>

Moving state to the closest common parent of the components that need it.

</details>

**6. Composition or inheritance?**

<details><summary>Answer</summary>

React code reuses UI by composition: wrapping, slots, specialization.

</details>

**7. Why are `Children` and `cloneElement` discouraged?**

<details><summary>Answer</summary>

They are uncommon and can lead to fragile code; render props, context or explicit components are clearer.

</details>

**8. How can `children` reduce re-rendering?**

<details><summary>Answer</summary>

The wrapper receives ready elements, so updating its own state does not require rendering them again.

</details>

**9. Trick question: can a component read `props.key`?**

<details><summary>Answer</summary>

No. `key` is used by React and not passed on; pass the value under another name.

</details>

**10. Trick question: you did `useState(props.name)` and the parent changed `name`. What is shown?**

<details><summary>Answer</summary>

The old value from the state: the initial value is used only once.

</details>

**11. How do you type a wrapper over a native element?**

<details><summary>Answer</summary>

`ComponentProps<'button'>` intersected with your own props.

</details>

## Related materials

- [Advanced theory](advanced.en.md)
- [Quiz](quiz.en.md) and [exercises](exercises.en.md)

## Sources

- [React: passing props to a component](https://react.dev/learn/passing-props-to-a-component)
- [React: thinking in React](https://react.dev/learn/thinking-in-react)
- [React: sharing state between components](https://react.dev/learn/sharing-state-between-components)
- [React: Children](https://react.dev/reference/react/Children)
- [React: cloneElement](https://react.dev/reference/react/cloneElement)
- [React: choosing the state structure](https://react.dev/learn/choosing-the-state-structure)
