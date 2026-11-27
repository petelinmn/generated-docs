---
title: "Quiz: context and reducers"
lang: en
track: react
kind: module
doc: quiz
slug: 08-context-and-reducers
part: 2
tags: [react, context, use-reducer, state-management, hooks]
versions: "React 19.3"
status: draft
updated: 2026-11-27
---

# Quiz: context and reducers

🌐 [Русский](quiz.ru.md) · **English**

> Answer first, then open the answer. The mark shows the level: 🟢 [basics](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 What does a reducer return?

- A) Nothing
- B) The next state
- C) An action
- D) A component

<details><summary>Answer</summary>

**B.**

→ [Basics](basics.en.md)

</details>

## 2. 🟢 How does a component ask for an update with `useReducer`?

- A) Changes `state`
- B) Calls `dispatch(action)`
- C) Calls the reducer directly
- D) Sets a ref

<details><summary>Answer</summary>

**B.**

→ [Basics](basics.en.md)

</details>

## 3. 🟢 What does a consumer read without a provider?

- A) `undefined`
- B) The default of `createContext`
- C) An error
- D) `null` always

<details><summary>Answer</summary>

**B.**

→ [Basics](basics.en.md)

</details>

## 4. 🟢 Two nested providers of one context: which value is read inside the inner one?

- A) The outer
- B) The inner
- C) Both merged
- D) The default

<details><summary>Answer</summary>

**B.**

→ [Basics](basics.en.md)

</details>

## 5. 🟡 `state.count` is 0. Three `dispatch({type:'inc'})` calls, then `console.log(state.count)`. Output?

- A) 3
- B) 0
- C) 1
- D) undefined

<details><summary>Answer</summary>

**B.** The new state belongs to the next render.

→ [Advanced](advanced.en.md)

</details>

## 6. 🟡 Find the bug

```tsx
case 'add':
  state.items.push(action.item);
  return state;
```

<details><summary>Answer</summary>

The reducer mutates and returns the same object, so nothing re-renders. Return `{ ...state, items: [...state.items, action.item] }`.

→ [Advanced](advanced.en.md)

</details>

## 7. 🟡 Select ways to reduce re-renders caused by context

- A) Memoize the value
- B) Split into several contexts
- C) Wrap consumers in `memo`
- D) Move state closer to its users

<details><summary>Answer</summary>

**A, B, D.** `memo` does not block context updates.

→ [Advanced](advanced.en.md)

</details>

## 8. 🟡 Why put `dispatch` into its own context?

- A) It is required
- B) Components that only dispatch need not re-render on state changes
- C) It makes the reducer pure
- D) For TypeScript only

<details><summary>Answer</summary>

**B.**

→ [Advanced](advanced.en.md)

</details>

## 9. 🔴 What does `<Theme value={undefined}>` give a consumer?

- A) The default
- B) `undefined`
- C) The outer value
- D) An error

<details><summary>Answer</summary>

**B.**

→ [Expert](expert.en.md)

</details>

## 10. 🔴 In StrictMode a reducer does `s.items.push(a)`. One dispatch adds

- A) One item
- B) Two items
- C) Nothing
- D) An error

<details><summary>Answer</summary>

**B.** The reducer is called twice on the same object.

→ [Expert](expert.en.md)

</details>

## 11. 🔴 Which may be called inside an `if`?

- A) `useContext`
- B) `use`
- C) `useReducer`
- D) `useState`

<details><summary>Answer</summary>

**B.**

→ [Expert](expert.en.md)

</details>

## 12. 🔴 Open question: typing in a search box makes the whole app re-render because the query is in a global context. What would you change?

<details><summary>Key points</summary>

- Keep the query as local state near the search and its results.
- If several places need it, give it its own context and memoize the value.
- For selective subscriptions consider an external store.

→ [Expert](expert.en.md)

</details>
