---
title: "Components and JSX: basics"
lang: en
track: react
kind: module
doc: basics
slug: 01-components-and-jsx
part: 1
tags: [react, jsx, components, rendering]
versions: "React 19.3"
status: draft
updated: 2026-11-27
---

# Components and JSX: basics

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** JavaScript functions, arrays, modules; basic HTML · **Time:** ~2 h

## Creating a project

The React documentation recommends starting production applications with a framework. For learning, a plain Vite project is enough. Create React App is deprecated.

```bash
npm create vite@latest my-app -- --template react-ts
cd my-app
npm install
npm run dev
```

The examples in this module were run with React 19.3.1. To show the produced HTML they use `renderToStaticMarkup`.

## A component

```tsx
import { renderToStaticMarkup } from 'react-dom/server';

function Greeting() {
  const name = 'Ann';
  return <h1 className="title">Hello, {name}!</h1>;
}

console.log(renderToStaticMarkup(<Greeting />));
// <h1 class="title">Hello, Ann!</h1>
```

A component is a function. Its name starts with a capital letter, and you use it as a tag. In an application a root renders the top component into a DOM node:

```tsx
import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <Greeting />
  </StrictMode>,
);
```

## JSX rules

| HTML | JSX |
|---|---|
| several top-level elements | one root, or a fragment `<>...</>` |
| `<img>` may stay open | every tag closed: `<img />` |
| `class`, `for` | `className`, `htmlFor` |
| `tabindex`, `maxlength` | `tabIndex`, `maxLength` |
| `style="color:red"` | `style={{ color: 'red' }}` |

`aria-*` and `data-*` keep their dashes.

```tsx
// one root: a fragment adds no DOM node
<>
  <li>a</li>
  <li>b</li>
</>
// <li>a</li><li>b</li>

<a href="/x" aria-label="go" data-id="7" tabIndex={0}>go</a>
<label htmlFor="n">N<input id="n" disabled /></label>
<div style={{ color: 'red', fontSize: 12, lineHeight: 2 }} />
// <div style="color:red;font-size:12px;line-height:2"></div>
```

## Curly braces

Inside braces goes any **expression**: a variable, a calculation, a function call. Statements such as `if` and `for` cannot be written there.

## Lists and conditions

```tsx
const products = [
  { id: 1, name: 'Pen', inStock: true },
  { id: 2, name: 'Book', inStock: false },
];

function Product({ name, inStock }: { name: string; inStock: boolean }) {
  return <li className={inStock ? 'in' : 'out'}>{name}{!inStock && ' (sold out)'}</li>;
}

<ul>{products.map(p => <Product key={p.id} name={p.name} inStock={p.inStock} />)}</ul>
// <ul><li class="in">Pen</li><li class="out">Book (sold out)</li></ul>
```

`map` turns data into elements, each with a stable `key`. For conditions use `? :` or `&&`.

## Beginner mistakes

- ❌ `function button()` used as `<button />` → ✅ capitalise component names.
- ❌ `class=` → ✅ `className=`.
- ❌ A list without `key` → ✅ a stable identifier from the data.
- ❌ Calling a component as `Greeting()` → ✅ use it as `<Greeting />`.

## Related materials

- [Advanced theory](advanced.en.md)

## Sources

- [React: writing markup with JSX](https://react.dev/learn/writing-markup-with-jsx)
- [React: render and commit](https://react.dev/learn/render-and-commit)
- [React: StrictMode](https://react.dev/reference/react/StrictMode)
- [React: creating an app](https://react.dev/learn/creating-a-react-app)
- [React: build from scratch](https://react.dev/learn/build-a-react-app-from-scratch)
