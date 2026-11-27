---
title: "Quiz: state and events"
lang: en
track: react
kind: module
doc: quiz
slug: 03-state-and-events
part: 1
tags: [react, state, use-state, events, hooks]
versions: "React 19.3"
status: draft
updated: 2026-11-27
---

# Quiz: state and events

🌐 [Русский](quiz.ru.md) · **English**

> Answer first, then open the answer. The mark shows the level: 🟢 [basics](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 Why does changing a local variable not update the screen?

- A) It is read-only
- B) It triggers no render and is recreated on the next one
- C) Browsers cache it
- D) Only globals update the screen

<details><summary>Answer</summary>

**B.**

→ [Basics](basics.en.md)

</details>

## 2. 🟢 What does `useState` return?

- A) Only the value
- B) A pair: the value and a setter
- C) A promise
- D) An object with `get` and `set`

<details><summary>Answer</summary>

**B.**

→ [Basics](basics.en.md)

</details>

## 3. 🟢 Which passes a handler correctly?

- A) `onClick={save()}`
- B) `onClick={save}`
- C) `onClick="save()"`
- D) `onclick={save}`

<details><summary>Answer</summary>

**B.**

→ [Basics](basics.en.md)

</details>

## 4. 🟢 What stops a form submission from reloading the page?

- A) `stopPropagation`
- B) `preventDefault`
- C) `return null`
- D) `onClickCapture`

<details><summary>Answer</summary>

**B.**

→ [Basics](basics.en.md)

</details>

## 5. 🟡 `n` is 0. What does the next render show?

```tsx
setN(n + 1); setN(n + 1); setN(n + 1);
```

<details><summary>Answer</summary>

```text
1
```

Each call uses the same snapshot.

→ [Advanced](advanced.en.md)

</details>

## 6. 🟡 `n` is 4. And here?

```tsx
setN(n + 5); setN(x => x + 1);
```

<details><summary>Answer</summary>

```text
10
```

→ [Advanced](advanced.en.md)

</details>

## 7. 🟡 The screen shows 0. What is logged after the click?

```tsx
setN(n + 1);
setTimeout(() => console.log(n), 5);
```

<details><summary>Answer</summary>

```text
0
```

The timeout closed over the old snapshot.

→ [Advanced](advanced.en.md)

</details>

## 8. 🟡 Find the bug

```tsx
items.push(newItem);
setItems(items);
```

<details><summary>Answer</summary>

The same array is set, so no render is requested. Use `setItems([...items, newItem])`.

→ [Advanced](advanced.en.md)

</details>

## 9. 🟡 Select all things that reset a component's state

- [ ] A) A new `key`
- [ ] B) A change of props
- [ ] C) A different element type at that position
- [ ] D) Removing it from the tree

<details><summary>Answer</summary>

**A, C, D.** Props alone keep the state.

→ [Advanced](advanced.en.md)

</details>

## 10. 🔴 Why does React require the same order of Hooks on every render?

- A) For performance only
- B) State is matched to Hooks by call order
- C) Because of TypeScript
- D) It does not

<details><summary>Answer</summary>

**B.**

→ [Expert](expert.en.md)

</details>

## 11. 🔴 After the first render and two updates, how many times is `expensive` called in `useState(expensive())`?

- A) 1
- B) 2
- C) 3
- D) 0

<details><summary>Answer</summary>

**C.** The call is part of every render; `useState(expensive)` would call it once.

→ [Expert](expert.en.md)

</details>

## 12. 🔴 Open question: a chat window keeps the draft of the previous contact after switching. Explain and fix.

<details><summary>Key points</summary>

- The same component stays at the same position, so state is kept.
- Give it `key={contact.id}`.
- If drafts must be remembered per contact, keep them in a parent object keyed by id.

→ [Expert](expert.en.md)

</details>
