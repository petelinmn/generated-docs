---
title: "Quiz: effects"
lang: en
track: react
kind: module
doc: quiz
slug: 06-effects
part: 2
tags: [react, effects, use-effect, hooks, cleanup]
versions: "React 19.3"
status: draft
updated: 2026-11-27
---

# Quiz: effects

🌐 [Русский](quiz.ru.md) · **English**

> Answer first, then open the answer. The mark shows the level: 🟢 [basics](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 What are effects for?

- A) Computing values for display
- B) Synchronising with systems outside React
- C) Handling clicks
- D) Styling

<details><summary>Answer</summary>

**B.**

→ [Basics](basics.en.md)

</details>

## 2. 🟢 When does an effect with `[]` run (without StrictMode)?

- A) Before the first render
- B) After the first commit only
- C) After every commit
- D) Only on unmount

<details><summary>Answer</summary>

**B.**

→ [Basics](basics.en.md)

</details>

## 3. 🟢 When is cleanup called?

- A) Only on unmount
- B) Before the next run and on unmount
- C) Before the first run
- D) Never automatically

<details><summary>Answer</summary>

**B.**

→ [Basics](basics.en.md)

</details>

## 4. 🟢 Do effects run during server rendering?

- A) Yes
- B) No

<details><summary>Answer</summary>

**B.**

→ [Basics](basics.en.md)

</details>

## 5. 🟡 An effect depends on `[options]`, where `options = { v }` is created in the component. How often does it run?

- A) Once
- B) When `v` changes
- C) After every render
- D) Never

<details><summary>Answer</summary>

**C.** The object is new each time.

→ [Advanced](advanced.en.md)

</details>

## 6. 🟡 What does the screen show after a second?

```tsx
useEffect(() => {
  const id = setInterval(() => setN(n + 1), 100);
  return () => clearInterval(id);
}, []);
```

<details><summary>Answer</summary>

`1`: the callback keeps `n` equal to 0.

→ [Advanced](advanced.en.md)

</details>

## 7. 🟡 Select all cases where an effect is not needed

- [ ] A) Computing a full name from two fields
- [ ] B) Opening a connection
- [ ] C) Sending a request after a button click
- [ ] D) Subscribing to window resize

<details><summary>Answer</summary>

**A, C.**

→ [Advanced](advanced.en.md)

</details>

## 8. 🟡 Find the bug

```tsx
useEffect(() => {
  load(id).then(setData);
}, [id]);
```

<details><summary>Answer</summary>

A slow response for an old `id` can overwrite the new one. Add an ignore flag or abort.

→ [Advanced](advanced.en.md)

</details>

## 9. 🟡 The id changes from a to b. Which comes first for the ordinary effect?

- A) setup b, then cleanup a
- B) cleanup a, then setup b
- C) only setup b
- D) only cleanup a

<details><summary>Answer</summary>

**B.**

→ [Advanced](advanced.en.md)

</details>

## 10. 🔴 What does an `async` effect function cause?

- A) Nothing
- B) A warning, and a TypeError on unmount
- C) A faster render
- D) A compile error in React

<details><summary>Answer</summary>

**B.**

→ [Expert](expert.en.md)

</details>

## 11. 🔴 Where may an effect event be called?

- A) During render
- B) From effects and other effect events
- C) From a child component
- D) From the dependency array

<details><summary>Answer</summary>

**B.**

→ [Expert](expert.en.md)

</details>

## 12. 🔴 Open question: a chat reconnects every time the theme changes. Why, and how to fix it?

<details><summary>Key points</summary>

- The effect that connects reads `theme`, so it is a dependency.
- Move the use of `theme` into `useEffectEvent`, leaving only `room` as a dependency.
- Do not remove `theme` from the array by silencing the linter.

→ [Expert](expert.en.md)

</details>
