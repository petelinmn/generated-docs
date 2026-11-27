---
title: "Components and JSX"
lang: en
track: react
kind: module
doc: overview
slug: 01-components-and-jsx
part: 1
levels: [1, 2, 3, 4]
tags: [react, jsx, components, rendering]
versions: "React 19.3"
status: draft
updated: 2026-11-26
---

# Components and JSX

🌐 [Русский](overview.ru.md) · **English**

## In a nutshell

React builds a user interface out of components: functions that take data and return a description of what should be on the screen. The description is written in JSX, a syntax that looks like HTML inside JavaScript. When data changes, React calls the components again and updates only the parts of the page that differ. This module covers components and JSX: the foundation of everything else.

## Key ideas

- A component is a function whose name starts with a capital letter and which returns JSX.
- JSX is stricter than HTML: one root, every tag closed, attributes in camelCase.
- Curly braces insert JavaScript expressions.
- `false`, `null`, `undefined` and `true` render nothing; `0` and `NaN` render as text.
- Inserted strings are escaped, so user input does not become markup.
- Rendering must be pure: the same input gives the same output.
- React renders (calls components) and then commits (changes the DOM).

## Minimal example

```tsx
import { renderToStaticMarkup } from 'react-dom/server';

function Greeting() {
  const name = 'Ann';
  return <h1 className="title">Hello, {name}!</h1>;
}

console.log(renderToStaticMarkup(<Greeting />));
// <h1 class="title">Hello, Ann!</h1>
```

## When to use and when not

- ✅ Interfaces with changing data and reusable parts.
- ✅ Applications that grow: components keep them manageable.
- ❌ A static page with no interaction: plain HTML is enough.
- ❌ One small script on an existing page: it may not justify a build step.

## Levels of this material

| Level | What's inside |
|---|---|
| [1. Basics](basics.en.md) | project setup, components, JSX rules, lists and conditions |
| [2. Advanced](advanced.en.md) | what can be rendered, dynamic components, escaping, purity |
| [3. Expert](expert.en.md) | elements inside, render and commit, reference, gotchas, interview questions |
| [4. Self-check](quiz.en.md) | a quiz and [exercises](exercises.en.md) |
| 5. Runnable examples | not created yet — `/learn react 01-components-and-jsx level 5` |
| 6. Tests | not created yet — `/learn react 01-components-and-jsx level 6` |

## Related materials

- Prerequisites: [functions in JavaScript](../../../javascript/modules/04-functions/overview.en.md), [modules](../../../javascript/modules/09-modules/overview.en.md), [arrays](../../../javascript/modules/07-arrays-and-collections/overview.en.md)
- Types for React code: [TypeScript track](../../../typescript/overview.en.md)
- The DOM that React manages: [browser DOM and events](../../../javascript/modules/17-browser-dom-and-events/overview.en.md)
- Next: the following module of the [React roadmap](../../overview.en.md)
- [02 · Props and composition](../02-props-and-composition/overview.en.md)
