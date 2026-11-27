---
title: "Components and JSX: expert"
lang: en
track: react
kind: module
doc: expert
slug: 01-components-and-jsx
part: 1
tags: [react, jsx, components, rendering]
versions: "React 19.3"
status: draft
updated: 2026-11-27
---

# Components and JSX: expert

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · **3. Expert** · [4. Quiz](quiz.en.md)

## Under the hood

### Elements

The compiler turns JSX into calls of `jsx()` from `react/jsx-runtime` (the automatic runtime), so no `import React` is needed for JSX itself. The result is a plain object:

```tsx
const el = <h1 id="a">x</h1>;

String(el.$$typeof);          // 'Symbol(react.transitional.element)'
el.type;                      // 'h1'
Object.isFrozen(el);          // true   (development build)
Object.isFrozen(el.props);    // true   (development build)

isValidElement(el);                           // true
isValidElement({ type: 'h1', props: {} });    // false

jsx('h1', { className: 't', children: 'Hi' }); // what the compiler emits: <h1 class="t">Hi</h1>
```

An element is a description, not a DOM node and not a component instance. The `$$typeof` symbol marks genuine elements: an object that arrives as JSON cannot contain a symbol, so it is not accepted as an element.

### Trigger, render, commit

| Stage | What happens |
|---|---|
| Trigger | the initial render, or a state update in the component or an ancestor |
| Render | React calls the components and builds the new tree of elements |
| Commit | React changes only the DOM nodes that differ |

Rendering a component also renders the components it returns. A render is not a DOM update.

## Reference

| JSX | Compiles to the idea of |
|---|---|
| `<div a="1">x</div>` | type `'div'`, props `{ a: '1', children: 'x' }` |
| `<Card />` | type `Card` (the function itself) |
| `<UI.Badge />` | type `UI.Badge` |
| `<>...</>` | type `Fragment` |
| `<div {...props} />` | props spread from an object |
| `{/* comment */}` | nothing |

| Attribute | Note |
|---|---|
| `className`, `htmlFor` | instead of `class`, `for` |
| `style` | an object; names in camelCase; unitless numbers get `px` where CSS needs a unit |
| `aria-*`, `data-*` | written with dashes |
| `dangerouslySetInnerHTML={{ __html }}` | inserts raw HTML |
| `key` | identity among siblings, not passed to the component |
| `ref` | access to a DOM node; in React 19 a function component receives it as a prop |

| Rendering API | Purpose |
|---|---|
| `createRoot(node).render(element)` | client application |
| `hydrateRoot(node, element)` | attach to server-rendered HTML |
| `renderToStaticMarkup(element)` | HTML string without React attributes, as in these examples |
| `createElement`, `cloneElement`, `isValidElement`, `Fragment` | working with elements directly |

## Gotchas

### A lowercase name is a tag, not your function

```tsx
function my() { return <em>custom</em>; }
<my />        // <my></my>   — an unknown HTML tag, the function is not called
```

### An object cannot be a child

```tsx
<p>{{ a: 1 }}</p>
// Error: Objects are not valid as a React child (found: object with keys {a}). ...
```

A common cause is rendering a `Date` or a whole record instead of one of its fields.

### A line break after a tag eats the space

```tsx
<p>
  one <i>two</i>
  three
</p>
// <p>one <i>two</i>three</p>

<p>
  one <i>two</i>{' '}
  three
</p>
// <p>one <i>two</i> three</p>
```

JSX removes whitespace that contains a line break. Write `{' '}` where a space is needed.

### An impure component gives surprising output

```tsx
let guests = 0;
function Impure() {
  guests++;                       // side effect during render
  return <h2>Guest {guests}</h2>;
}

// client render in development inside <StrictMode>, two <Impure />:
// <h2>Guest 2</h2><h2>Guest 4</h2>
```

On the server, where each component was called once, the same code printed `Guest 1` and `Guest 2`.

## Trade-offs and alternatives

| Choice | Pro | Con |
|---|---|---|
| JSX | markup and logic together, type checking | needs a build step |
| `createElement` by hand | no compiler | verbose |
| Server rendering and frameworks | fast first view, search engines | more moving parts |
| Client-only (Vite SPA) | simple to deploy | blank until JavaScript loads |
| Plain DOM code | no dependency | manual synchronisation of data and view |

## Interview questions

**1. What is JSX?**

<details><summary>Answer</summary>

A syntax extension compiled into function calls that create element objects.

</details>

**2. How does an element differ from a component?**

<details><summary>Answer</summary>

A component is a function; an element is an immutable object describing what to render: a type and props.

</details>

**3. Why must component names be capitalised?**

<details><summary>Answer</summary>

A lowercase tag is treated as a built-in HTML element.

</details>

**4. What are the stages of an update?**

<details><summary>Answer</summary>

Trigger, render (calling components), commit (changing the DOM).

</details>

**5. Does a render always change the DOM?**

<details><summary>Answer</summary>

No. React changes only the nodes that differ.

</details>

**6. What does StrictMode do?**

<details><summary>Answer</summary>

In development it double-invokes functions that should be pure and runs extra checks; production is unaffected.

</details>

**7. Why is user text safe in JSX?**

<details><summary>Answer</summary>

It is escaped. `dangerouslySetInnerHTML` removes this protection. URLs are a separate matter: React 19 blocks `javascript:` URLs, but other schemes such as `data:` pass through, so validate addresses from users.

</details>

**8. What is a fragment for?**

<details><summary>Answer</summary>

To return several elements without an extra DOM node.

</details>

**9. Trick question: what does `{items.length && <List />}` show for an empty array?**

<details><summary>Answer</summary>

`0`.

</details>

**10. Trick question: can you write `if` inside JSX braces?**

<details><summary>Answer</summary>

No, only expressions. Use a ternary, `&&`, or an `if` before `return`.

</details>

**11. Why not declare a component inside another one?**

<details><summary>Answer</summary>

Each render creates a new function, so React treats it as a different type and resets its state.

</details>

## Related materials

- [Advanced theory](advanced.en.md)
- [Quiz](quiz.en.md) and [exercises](exercises.en.md)

## Sources

- [React: writing markup with JSX](https://react.dev/learn/writing-markup-with-jsx)
- [React: render and commit](https://react.dev/learn/render-and-commit)
- [React: StrictMode](https://react.dev/reference/react/StrictMode)
- [React 19 release](https://react.dev/blog/2024/12/05/react-19)
- [React versions](https://react.dev/versions)
