---
title: "Quiz: lists, keys and identity"
lang: en
track: react
kind: module
doc: quiz
slug: 04-lists-and-keys
part: 1
tags: [react, keys, lists, identity, state]
versions: "React 19.3"
status: draft
updated: 2026-11-27
---

# Quiz: lists, keys and identity

🌐 [Русский](quiz.ru.md) · **English**

> Answer first, then open the answer. The mark shows the level: 🟢 [basics](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 What is `key` for?

- A) Styling
- B) Recognising an item between renders
- C) Sorting the list
- D) Accessibility

<details><summary>Answer</summary>

**B.**

→ [Basics](basics.en.md)

</details>

## 2. 🟢 Which is the best key?

- A) The array index
- B) `Math.random()`
- C) The record id
- D) The current time

<details><summary>Answer</summary>

**C.**

→ [Basics](basics.en.md)

</details>

## 3. 🟢 Among what must keys be unique?

- A) The whole app
- B) Siblings
- C) The component file
- D) The page

<details><summary>Answer</summary>

**B.**

→ [Basics](basics.en.md)

</details>

## 4. 🟢 Where is the key written?

- A) On the `<ul>`
- B) On the outermost element returned by `map`
- C) Inside the item component
- D) In state

<details><summary>Answer</summary>

**B.**

→ [Basics](basics.en.md)

</details>

## 5. 🟡 Index keys: items `a b`, `a` has count 1. The list is reversed. What is shown?

- A) `b:0 a:1`
- B) `b:1 a:0`
- C) `b:0 a:0`
- D) An error

<details><summary>Answer</summary>

**B.** The state stays at position 0.

→ [Advanced](advanced.en.md)

</details>

## 6. 🟡 Find the bug

```tsx
items.map(item => (
  <>
    <dt key={item.id}>{item.term}</dt>
    <dd>{item.def}</dd>
  </>
))
```

<details><summary>Answer</summary>

The key must be on the fragment: `<Fragment key={item.id}>`.

→ [Advanced](advanced.en.md)

</details>

## 7. 🟡 Select ways that keep a component's state

- A) Hiding it with `hidden`
- B) Removing it with `&&`
- C) Reordering siblings with stable keys
- D) Changing its `key`

<details><summary>Answer</summary>

**A, C.**

→ [Advanced](advanced.en.md)

</details>

## 8. 🟡 Where should an id for a new client-side item be created?

- A) In render
- B) In the handler that adds the item
- C) In `map`
- D) In CSS

<details><summary>Answer</summary>

**B.**

→ [Advanced](advanced.en.md)

</details>

## 9. 🔴 `{warn && <p/>}` appears before `<List/>`. Does `List` lose its state when the warning appears?

- A) Yes
- B) No

<details><summary>Answer</summary>

**B.** The `false` already occupied that position.

→ [Expert](expert.en.md)

</details>

## 10. 🔴 An element with the same key moves from one `<ul>` to another. Its state?

- A) Kept
- B) Reset
- C) Merged
- D) Undefined

<details><summary>Answer</summary>

**B.**

→ [Expert](expert.en.md)

</details>

## 11. 🔴 A list uses `key={Math.random()}`. What do users notice?

- A) Nothing
- B) Inputs lose text and focus after re-renders
- C) Only slower first load
- D) Duplicated rows

<details><summary>Answer</summary>

**B.**

→ [Expert](expert.en.md)

</details>

## 12. 🔴 Open question: rows of a table have an editable comment, and after sorting comments end up at other rows. Why and how to fix it?

<details><summary>Key points</summary>

- Keys are indexes (or positions), so state and inputs stay in place.
- Use a stable id from the data.
- If comments must survive removal of rows, store them in the parent keyed by id.

→ [Expert](expert.en.md)

</details>
