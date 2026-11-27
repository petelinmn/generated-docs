---
title: "Quiz: components and JSX"
lang: en
track: react
kind: module
doc: quiz
slug: 01-components-and-jsx
part: 1
tags: [react, jsx, components, rendering]
versions: "React 19.3"
status: draft
updated: 2026-11-27
---

# Quiz: components and JSX

🌐 [Русский](quiz.ru.md) · **English**

> Answer first, then open the answer. The mark shows the level: 🟢 [basics](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 What is a React component?

- A) An HTML file
- B) A function returning JSX
- C) A CSS class
- D) A DOM node

<details><summary>Answer</summary>

**B.**

→ [Basics](basics.en.md)

</details>

## 2. 🟢 Which is correct JSX?

- A) `<div class="a"></div>`
- B) `<img className="a">`
- C) `<div className="a" />`
- D) `<div style="color:red" />`

<details><summary>Answer</summary>

**C.** A uses `class`, B is not closed, D passes a string to `style`.

→ [Basics](basics.en.md)

</details>

## 3. 🟢 What do the braces `{}` accept?

- A) Any statement
- B) An expression
- C) Only strings
- D) Only variables

<details><summary>Answer</summary>

**B.**

→ [Basics](basics.en.md)

</details>

## 4. 🟢 How do you return two elements without a wrapper node?

- A) A fragment `<>...</>`
- B) A `<div>`
- C) Two `return`s
- D) It is impossible

<details><summary>Answer</summary>

**A.**

→ [Basics](basics.en.md)

</details>

## 5. 🟡 What does this render?

```tsx
<p>{0}{false}{null}{'x'}</p>
```

<details><summary>Answer</summary>

```text
<p>0x</p>
```

`false` and `null` are skipped, `0` is text.

→ [Advanced](advanced.en.md)

</details>

## 6. 🟡 Find the bug

```tsx
const messages = [];
return <div>{messages.length && <ul>...</ul>}</div>;
```

<details><summary>Answer</summary>

It renders `0`. Use `messages.length > 0 &&`.

→ [Advanced](advanced.en.md)

</details>

## 7. 🟡 What happens with `<p>{'<b>x</b>'}</p>`?

- A) Bold text
- B) The tags are shown as text
- C) An error
- D) Nothing

<details><summary>Answer</summary>

**B.** Strings are escaped.

→ [Advanced](advanced.en.md)

</details>

## 8. 🟡 Select all statements about pure components

- [ ] A) The same input gives the same output
- [ ] B) They may change a global counter during render
- [ ] C) React may call them more than once
- [ ] D) Randomness during render breaks purity

<details><summary>Answer</summary>

**A, C, D.**

→ [Advanced](advanced.en.md)

</details>

## 9. 🟡 `const tag = "h2"`. How do you render it?

- A) `<tag>`
- B) `const Tag = tag; <Tag />`
- C) `<{tag}>`
- D) `<"h2">`

<details><summary>Answer</summary>

**B.** A lowercase `<tag>` would create an element named `tag`.

→ [Advanced](advanced.en.md)

</details>

## 10. 🔴 What does JSX compile into?

- A) DOM nodes
- B) Calls that create element objects
- C) HTML strings
- D) Web components

<details><summary>Answer</summary>

**B.**

→ [Expert](expert.en.md)

</details>

## 11. 🔴 What does this render?

```tsx
<p>
  one <i>two</i>
  three
</p>
```

<details><summary>Answer</summary>

```text
<p>one <i>two</i>three</p>
```

The line break after the tag is removed with the whitespace.

→ [Expert](expert.en.md)

</details>

## 12. 🔴 Open question: why does a counter in a global variable work badly as component data?

<details><summary>Key points</summary>

- Changing it does not trigger a render.
- It is shared by all instances.
- Rendering becomes impure and results depend on how many times React called the component.

→ [Expert](expert.en.md)

</details>
