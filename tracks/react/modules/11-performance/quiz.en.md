---
title: "Quiz: rendering performance"
lang: en
track: react
kind: module
doc: quiz
slug: 11-performance
part: 3
tags: [react, performance, memoization, react-compiler, profiling]
versions: "React 19.3"
status: draft
updated: 2026-11-27
---

# Quiz: rendering performance

🌐 [Русский](quiz.ru.md) · **English**

> Answer first, then open the answer. The mark shows the level: 🟢 [basics](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 Which does NOT by itself cause a component to re-render?

- A) Its state changes
- B) Its parent renders
- C) A context it reads changes
- D) A ref changes

<details><summary>Answer</summary>

**D.**

→ [Basics](basics.en.md)

</details>

## 2. 🟢 First step of optimisation?

- A) Add `memo` everywhere
- B) Measure
- C) Rewrite in another library
- D) Use `useRef`

<details><summary>Answer</summary>

**B.**

→ [Basics](basics.en.md)

</details>

## 3. 🟢 Why move state down?

- A) To save memory
- B) So fewer components re-render
- C) It is required
- D) To share it

<details><summary>Answer</summary>

**B.**

→ [Basics](basics.en.md)

</details>

## 4. 🟢 Does every render change the DOM?

- A) Yes
- B) No, only differences are applied

<details><summary>Answer</summary>

**B.**

→ [Basics](basics.en.md)

</details>

## 5. 🟡 A memoized child gets `style={{ color: "red" }}`. When the parent renders, the child

- A) is skipped
- B) renders
- C) is unmounted
- D) throws

<details><summary>Answer</summary>

**B.** The object is new.

→ [Advanced](advanced.en.md)

</details>

## 6. 🟡 Select props that keep `memo` effective

- A) A number that does not change
- B) A function from `useCallback` with stable deps
- C) An inline arrow function
- D) A value from `useMemo`

<details><summary>Answer</summary>

**A, B, D.**

→ [Advanced](advanced.en.md)

</details>

## 7. 🟡 Find the bug

```tsx
memo(Row, (a, b) => a.item.id === b.item.id)  // item also has a name
```

<details><summary>Answer</summary>

A changed `name` is ignored and the old one stays on screen.

→ [Advanced](advanced.en.md)

</details>

## 8. 🟡 Where should timings be judged?

- A) Development build
- B) Production build
- C) Unit tests
- D) Server logs

<details><summary>Answer</summary>

**B.**

→ [Advanced](advanced.en.md)

</details>

## 9. 🔴 What does React Compiler do?

- A) Renders on the server
- B) Adds memoization at build time
- C) Removes effects
- D) Compiles to native code

<details><summary>Answer</summary>

**B.**

→ [Expert](expert.en.md)

</details>

## 10. 🔴 May code depend on `useMemo` for correctness?

- A) Yes
- B) No

<details><summary>Answer</summary>

**B.** It is an optimisation only.

→ [Expert](expert.en.md)

</details>

## 11. 🔴 Keys are built as `${salt}-${i}` and `salt` changes. What happens to the rows?

- A) Updated in place
- B) Unmounted and mounted again
- C) Nothing
- D) Reordered

<details><summary>Answer</summary>

**B.**

→ [Expert](expert.en.md)

</details>

## 12. 🔴 Open question: typing in a filter of a 5000-row table lags. What do you do?

<details><summary>Key points</summary>

- Measure to see what is slow.
- Keep input state local; defer the table update (`useDeferredValue`).
- Virtualise the rows; memoize filtering and rows.
- Consider filtering on the server.

→ [Expert](expert.en.md)

</details>
