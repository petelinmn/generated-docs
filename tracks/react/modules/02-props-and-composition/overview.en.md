---
title: "Props and composition"
lang: en
track: react
kind: module
doc: overview
slug: 02-props-and-composition
part: 1
levels: [1, 2, 3, 4]
tags: [react, props, composition, children, typescript]
versions: "React 19.3"
status: draft
updated: 2026-11-26
---

# Props and composition

🌐 [Русский](overview.ru.md) · **English**

## In a nutshell

Props are the inputs of a component: the parent passes them, the child reads them and never changes them. Composition means building larger components out of smaller ones, mostly by passing elements as `children` or as other props. Together they decide how data flows through an application and how reusable its parts are.

## Key ideas

- Props are read-only; data flows from parent to child.
- A child reports back by calling a function it received as a prop.
- A default value applies when a prop is missing or `undefined`, not `null`.
- `children` and element props let a component wrap content it knows nothing about.
- Shared data lives in the closest common parent.
- `...rest` lets a component accept everything the underlying element accepts.
- Passing elements as `children` can avoid needless re-rendering.

## Minimal example

```tsx
function Avatar({ name, size = 40 }: { name: string; size?: number }) {
  return <img alt={name} width={size} />;
}

<Avatar name="Ann" />                     // <img alt="Ann" width="40"/>
<Avatar name="Bob" size={undefined} />    // <img alt="Bob" width="40"/>
<Avatar name="Eve" size={null} />         // <img alt="Eve"/>   (a type error in TypeScript)
```

## When to use and when not

- ✅ Reusable pieces: buttons, cards, dialogs, layouts.
- ✅ Passing content rather than many boolean flags.
- ❌ Copying a prop into state "just in case": the copy will not update.
- ❌ Passing a value through many layers that do not use it; consider composition or context.

## Levels of this material

| Level | What's inside |
|---|---|
| [1. Basics](basics.en.md) | props, defaults, TypeScript types, `children`, callbacks |
| [2. Advanced](advanced.en.md) | rest props, slots, render props, specialization, refs, re-rendering |
| [3. Expert](expert.en.md) | `Children` and `cloneElement`, reference, gotchas, interview questions |
| [4. Self-check](quiz.en.md) | a quiz and [exercises](exercises.en.md) |
| 5. Runnable examples | not created yet — `/learn react 02-props-and-composition level 5` |
| 6. Tests | not created yet — `/learn react 02-props-and-composition level 6` |

## Related materials

- Prerequisites: [01 · Components and JSX](../01-components-and-jsx/overview.en.md)
- Typing props: [TypeScript track](../../../typescript/overview.en.md)
- Destructuring and rest: [objects in JavaScript](../../../javascript/modules/05-objects-and-prototypes/overview.en.md)
- The same idea elsewhere: [composition in application architecture](../../../csharp/modules/21-architecture/overview.en.md)
- Next: the following module of the [React roadmap](../../overview.en.md)
- [03 · State and events](../03-state-and-events/overview.en.md)
