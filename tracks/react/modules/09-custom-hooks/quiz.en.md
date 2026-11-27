---
title: "Quiz: custom hooks"
lang: en
track: react
kind: module
doc: quiz
slug: 09-custom-hooks
part: 2
tags: [react, custom-hooks, hooks, use-sync-external-store, reuse]
versions: "React 19.3"
status: draft
updated: 2026-11-27
---

# Quiz: custom hooks

🌐 [Русский](quiz.ru.md) · **English**

> Answer first, then open the answer. The mark shows the level: 🟢 [basics](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 What makes a function a custom hook?

- A) It returns JSX
- B) It starts with `use` and calls hooks
- C) It is exported
- D) It is async

<details><summary>Answer</summary>

**B.**

→ [Basics](basics.en.md)

</details>

## 2. 🟢 Two components call `useToggle()`. Their state is

- A) Shared
- B) Separate
- C) Shared until unmount
- D) Stored globally

<details><summary>Answer</summary>

**B.**

→ [Basics](basics.en.md)

</details>

## 3. 🟢 Where may a hook be called?

- A) In an event handler
- B) At the top level of a component or hook
- C) Inside `if`
- D) In a loop

<details><summary>Answer</summary>

**B.**

→ [Basics](basics.en.md)

</details>

## 4. 🟢 A function formats a date and calls no hooks. Its name should be

- A) `useFormatDate`
- B) `formatDate`
- C) `UseDate`
- D) `hookDate`

<details><summary>Answer</summary>

**B.**

→ [Basics](basics.en.md)

</details>

## 5. 🟡 `useDebounced(value, 20)`: the value goes a, ab, abc at once. What is shown immediately?

- A) `abc:abc`
- B) `abc:a`
- C) `a:a`
- D) `ab:ab`

<details><summary>Answer</summary>

**B.**

→ [Advanced](advanced.en.md)

</details>

## 6. 🟡 Why wrap a returned function in `useCallback`?

- A) It is required
- B) So that it keeps its identity for those who depend on it
- C) To make it faster
- D) To share state

<details><summary>Answer</summary>

**B.**

→ [Advanced](advanced.en.md)

</details>

## 7. 🟡 Find the bug

```tsx
function useThing(on) {
  if (on) {
    const [v] = useState(0);
    return v;
  }
  return null;
}
```

<details><summary>Answer</summary>

A hook is called conditionally; call `useState` at the top and branch afterwards.

→ [Advanced](advanced.en.md)

</details>

## 8. 🟡 Select good candidates for a custom hook

- A) Subscribing to window size
- B) A pure price calculation
- C) Syncing a value with storage
- D) Rendering a button

<details><summary>Answer</summary>

**A, C.**

→ [Advanced](advanced.en.md)

</details>

## 9. 🔴 What happens if `getSnapshot` returns a new object each time?

- A) Nothing
- B) An infinite loop error
- C) A stale value
- D) One extra render

<details><summary>Answer</summary>

**B.**

→ [Expert](expert.en.md)

</details>

## 10. 🔴 What happens without `getServerSnapshot` during server rendering?

- A) It uses `getSnapshot`
- B) An error
- C) `undefined` is used
- D) Nothing is rendered silently

<details><summary>Answer</summary>

**B.**

→ [Expert](expert.en.md)

</details>

## 11. 🔴 A hook returns `{ value: 1 }` and an effect depends on it. Three renders cause

- A) 1 effect run
- B) 3 effect runs
- C) 0
- D) 2

<details><summary>Answer</summary>

**B.**

→ [Expert](expert.en.md)

</details>

## 12. 🔴 Open question: how would you share the number of unread messages between a header and a page?

<details><summary>Key points</summary>

- A custom hook alone would give each its own copy.
- Lift state to a common parent, use context, or an external store read with `useSyncExternalStore`.
- For server data, a data library with a shared cache.

→ [Expert](expert.en.md)

</details>
