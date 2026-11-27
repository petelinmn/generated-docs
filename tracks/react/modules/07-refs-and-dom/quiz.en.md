---
title: "Quiz: refs and the DOM"
lang: en
track: react
kind: module
doc: quiz
slug: 07-refs-and-dom
part: 2
tags: [react, refs, dom, portals, hooks]
versions: "React 19.3"
status: draft
updated: 2026-11-27
---

# Quiz: refs and the DOM

🌐 [Русский](quiz.ru.md) · **English**

> Answer first, then open the answer. The mark shows the level: 🟢 [basics](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 What happens when `ref.current` changes?

- A) A render
- B) Nothing visible by itself
- C) An effect runs
- D) An error

<details><summary>Answer</summary>

**B.**

→ [Basics](basics.en.md)

</details>

## 2. 🟢 How do you focus an input from a button?

- A) `ref.current.focus()` in the handler
- B) Set state `focus`
- C) `document.write`
- D) In the render body

<details><summary>Answer</summary>

**A.**

→ [Basics](basics.en.md)

</details>

## 3. 🟢 What is `ref.current` for a DOM ref during the first render?

- A) The node
- B) `null`
- C) `undefined` always
- D) An empty object

<details><summary>Answer</summary>

**B.**

→ [Basics](basics.en.md)

</details>

## 4. 🟢 Where should a timer id be kept?

- A) A module variable
- B) A ref
- C) State shown on screen
- D) A prop

<details><summary>Answer</summary>

**B.**

→ [Basics](basics.en.md)

</details>

## 5. 🟡 A button shows `{ref.current}` and the handler does `ref.current++`. After two clicks it shows

- A) 2
- B) 0
- C) 1
- D) nothing

<details><summary>Answer</summary>

**B.** No render was requested.

→ [Advanced](advanced.en.md)

</details>

## 6. 🟡 Find the bug

```tsx
setItems(list => [...list, item]);
listRef.current.lastElementChild.scrollIntoView();
```

<details><summary>Answer</summary>

The DOM is not updated yet; wrap the update in `flushSync` or scroll in an effect.

→ [Advanced](advanced.en.md)

</details>

## 7. 🟡 A button inside a portal is clicked. Which React `onClick` handlers run?

- A) Only the button's
- B) The button's and its React ancestors'
- C) Those of DOM ancestors only
- D) None

<details><summary>Answer</summary>

**B.**

→ [Advanced](advanced.en.md)

</details>

## 8. 🟡 Select all valid uses of refs

- [ ] A) Focusing a field
- [ ] B) Storing a list shown on screen
- [ ] C) Keeping an interval id
- [ ] D) Scrolling to an element

<details><summary>Answer</summary>

**A, C, D.**

→ [Advanced](advanced.en.md)

</details>

## 9. 🔴 What does an inline callback ref do on a re-render?

- A) Nothing
- B) Cleans up and attaches again
- C) Runs only cleanup
- D) Throws

<details><summary>Answer</summary>

**B.**

→ [Expert](expert.en.md)

</details>

## 10. 🔴 A component without a `ref` prop receives `ref={r}`. `r.current`?

- A) The component function
- B) `null`
- C) The first DOM node
- D) An error

<details><summary>Answer</summary>

**B.**

→ [Expert](expert.en.md)

</details>

## 11. 🔴 Which hook limits what a parent can do through a ref?

- A) `useRef`
- B) `useImperativeHandle`
- C) `useLayoutEffect`
- D) `useId`

<details><summary>Answer</summary>

**B.**

→ [Expert](expert.en.md)

</details>

## 12. 🔴 Open question: how would you build a modal dialog?

<details><summary>Key points</summary>

- Consider the native `<dialog>` with `showModal()` through a ref.
- Or a portal with manual focus management, Escape handling and an inert background.
- Return focus to the opening button on close.

→ [Expert](expert.en.md)

</details>
