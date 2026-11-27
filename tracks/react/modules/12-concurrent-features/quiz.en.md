---
title: "Quiz: concurrent features"
lang: en
track: react
kind: module
doc: quiz
slug: 12-concurrent-features
part: 3
tags: [react, suspense, transitions, concurrent-rendering, error-boundaries]
versions: "React 19.3"
status: draft
updated: 2026-11-27
---

# Quiz: concurrent features

🌐 [Русский](quiz.ru.md) · **English**

> Answer first, then open the answer. The mark shows the level: 🟢 [basics](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 What does `Suspense` show while a child is loading?

- A) Nothing
- B) The `fallback`
- C) An error
- D) The old DOM always

<details><summary>Answer</summary>

**B.**

→ [Basics](basics.en.md)

</details>

## 2. 🟢 What does `lazy` do?

- A) Delays rendering by a time
- B) Loads component code when needed
- C) Caches data
- D) Memoizes

<details><summary>Answer</summary>

**B.**

→ [Basics](basics.en.md)

</details>

## 3. 🟢 Which is caught by an error boundary?

- A) An error in `onClick`
- B) An error during rendering
- C) An error in `setTimeout`
- D) Its own error

<details><summary>Answer</summary>

**B.**

→ [Basics](basics.en.md)

</details>

## 4. 🟢 A heading and a loading component are in one `Suspense`. While loading the heading is

- A) visible
- B) replaced by the fallback
- C) duplicated
- D) greyed out

<details><summary>Answer</summary>

**B.**

→ [Basics](basics.en.md)

</details>

## 5. 🟡 A tab switch with `useTransition`, the new tab is not ready. What is on screen?

- A) The fallback
- B) The old tab, with `isPending` true
- C) An empty page
- D) The new tab partially

<details><summary>Answer</summary>

**B.**

→ [Advanced](advanced.en.md)

</details>

## 6. 🟡 Select all true statements about `useDeferredValue`

- [ ] A) It re-renders in the background with the new value
- [ ] B) It waits a fixed 300 ms
- [ ] C) It helps when the slow child can be skipped
- [ ] D) It reduces the number of server requests

<details><summary>Answer</summary>

**A, C.**

→ [Advanced](advanced.en.md)

</details>

## 7. 🟡 Find the bug

```tsx
function Name() {
  const name = use(fetchName());
  return <b>{name}</b>;
}
```

<details><summary>Answer</summary>

A new promise on every render; create it outside or cache it.

→ [Advanced](advanced.en.md)

</details>

## 8. 🟡 Which update must NOT be a transition?

- A) Switching a tab
- B) The value of a controlled input
- C) Filtering a big list
- D) Navigating

<details><summary>Answer</summary>

**B.**

→ [Advanced](advanced.en.md)

</details>

## 9. 🔴 What does a hidden `<Activity>` keep?

- A) Effects running
- B) State
- C) Nothing
- D) Only the DOM

<details><summary>Answer</summary>

**B.** Effects are cleaned up.

→ [Expert](expert.en.md)

</details>

## 10. 🔴 State is set after `await` inside `startTransition`. What is needed to keep it a transition?

- A) Nothing
- B) Wrap it in `startTransition` again
- C) `flushSync`
- D) `useRef`

<details><summary>Answer</summary>

**B.**

→ [Expert](expert.en.md)

</details>

## 11. 🔴 A button handler throws inside a boundary. Result?

- A) Fallback shown
- B) The boundary does not catch it
- C) The page reloads
- D) The button disappears

<details><summary>Answer</summary>

**B.**

→ [Expert](expert.en.md)

</details>

## 12. 🔴 Open question: how do you make switching between data-heavy tabs feel smooth?

<details><summary>Key points</summary>

- Switch in a transition and show `isPending`.
- Keep boundaries around the content, not the tab bar; add error boundaries.
- Preload on hover and cache promises.
- Keep hidden tabs with `Activity` if their state matters.

→ [Expert](expert.en.md)

</details>
