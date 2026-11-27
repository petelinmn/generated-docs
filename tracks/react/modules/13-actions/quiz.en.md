---
title: "Quiz: actions"
lang: en
track: react
kind: module
doc: quiz
slug: 13-actions
part: 3
tags: [react, actions, forms, optimistic-updates, transitions]
versions: "React 19.3"
status: draft
updated: 2026-11-27
---

# Quiz: actions

🌐 [Русский](quiz.ru.md) · **English**

> Answer first, then open the answer. The mark shows the level: 🟢 [basics](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 What argument does a function in `<form action>` get?

- A) A submit event
- B) A `FormData`
- C) The form element
- D) Nothing

<details><summary>Answer</summary>

**B.**

→ [Basics](basics.en.md)

</details>

## 2. 🟢 What happens to uncontrolled fields after a successful action?

- A) They keep the text
- B) They are reset to defaults
- C) They are disabled
- D) They are removed

<details><summary>Answer</summary>

**B.**

→ [Basics](basics.en.md)

</details>

## 3. 🟢 Where must `useFormStatus` be called?

- A) In the component rendering `<form>`
- B) In a component inside the form
- C) Anywhere
- D) Only in the root

<details><summary>Answer</summary>

**B.**

→ [Basics](basics.en.md)

</details>

## 4. 🟢 What does `formAction` on a button do?

- A) Adds a second action to run
- B) Overrides the form action for that button
- C) Resets the form
- D) Disables validation

<details><summary>Answer</summary>

**B.**

→ [Basics](basics.en.md)

</details>

## 5. 🟡 Find the bug

```tsx
const [count, add] = useActionState(async (n: number) => n + 1, 0);
return <button onClick={() => add()}>{count}</button>;
```

<details><summary>Answer</summary>

`add` is called outside a transition: wrap it in `startTransition` or pass it as a form action.

→ [Advanced](advanced.en.md)

</details>

## 6. 🟡 The server rejects an optimistic like and the real state is not changed. What is shown after the action?

- A) The optimistic value
- B) The previous real value
- C) Zero
- D) Nothing

<details><summary>Answer</summary>

**B.**

→ [Advanced](advanced.en.md)

</details>

## 7. 🟡 Select all true statements about `useActionState`

- [ ] A) Calls run one after another
- [ ] B) The reducer gets the previous state
- [ ] C) Calls run in parallel
- [ ] D) It returns `isPending`

<details><summary>Answer</summary>

**A, B, D.**

→ [Advanced](advanced.en.md)

</details>

## 8. 🟡 An action returns a validation error. How do you keep the typed name?

- A) It is kept automatically
- B) Return it and use it as `defaultValue`
- C) Call `preventDefault` in the action
- D) Throw the error

<details><summary>Answer</summary>

**B.**

→ [Advanced](advanced.en.md)

</details>

## 9. 🔴 The `useActionState` reducer throws while two more calls are queued. Result?

- A) They run anyway
- B) They are cancelled and the error reaches a boundary
- C) The state becomes undefined
- D) They are retried

<details><summary>Answer</summary>

**B.**

→ [Expert](expert.en.md)

</details>

## 10. 🔴 `onSubmit` calls `preventDefault` and the form also has `action`. What runs?

- A) Both
- B) Only `onSubmit`
- C) Only `action`
- D) Neither

<details><summary>Answer</summary>

**B.**

→ [Expert](expert.en.md)

</details>

## 11. 🔴 Open question: design a "like" button with instant feedback.

<details><summary>Key points</summary>

- `useOptimistic` over the real count, updated inside a transition.
- Update real state on success; show a message on failure.
- Make the server operation idempotent (set liked, not increment).
- Think about repeated clicks and response order.

→ [Expert](expert.en.md)

</details>
