---
title: "Quiz: testing"
lang: en
track: react
kind: module
doc: quiz
slug: 10-testing
part: 2
tags: [react, testing, vitest, testing-library, unit-testing]
versions: "React 19.3"
status: draft
updated: 2026-11-27
---

# Quiz: testing

🌐 [Русский](quiz.ru.md) · **English**

> Answer first, then open the answer. The mark shows the level: 🟢 [basics](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 What should a component test check?

- A) State variable names
- B) What the user sees and can do
- C) The number of renders
- D) CSS class names

<details><summary>Answer</summary>

**B.**

→ [Basics](basics.en.md)

</details>

## 2. 🟢 Which query is preferred?

- A) `getByTestId`
- B) `getByRole`
- C) `querySelector`
- D) `getByClassName`

<details><summary>Answer</summary>

**B.**

→ [Basics](basics.en.md)

</details>

## 3. 🟢 What provides the DOM in these tests?

- A) React
- B) jsdom
- C) TypeScript
- D) Node.js itself

<details><summary>Answer</summary>

**B.**

→ [Basics](basics.en.md)

</details>

## 4. 🟢 Which is right with user-event?

- A) `user.click(el)` without waiting
- B) `await user.click(el)`
- C) `click(el)` of React
- D) `el.value = ...`

<details><summary>Answer</summary>

**B.**

→ [Basics](basics.en.md)

</details>

## 5. 🟡 How do you assert that something is NOT on screen?

- A) `getByText` in a try
- B) `queryByText` and `not.toBeInTheDocument`
- C) `findByText`
- D) `getAllByText`

<details><summary>Answer</summary>

**B.**

→ [Advanced](advanced.en.md)

</details>

## 6. 🟡 Find the bug

```tsx
render(<User load={load} />);
expect(screen.getByText('Ann')).toBeInTheDocument();
```

<details><summary>Answer</summary>

The name arrives later; use `await screen.findByText('Ann')`.

→ [Advanced](advanced.en.md)

</details>

## 7. 🟡 Select good ways to deal with a network request in a test

- A) Pass the loading function as a prop and mock it
- B) Call the real production server
- C) Intercept requests with a mock server
- D) Wait five seconds

<details><summary>Answer</summary>

**A, C.**

→ [Advanced](advanced.en.md)

</details>

## 8. 🟡 What does the `wrapper` option do?

- A) Wraps the test in try
- B) Renders the component inside another, such as a provider
- C) Adds a div for styling
- D) Mocks the component

<details><summary>Answer</summary>

**B.**

→ [Advanced](advanced.en.md)

</details>

## 9. 🔴 What happens to the second test without cleanup (no Vitest globals)?

- A) It passes
- B) It finds two buttons and fails
- C) It hangs
- D) Nothing is rendered

<details><summary>Answer</summary>

**B.**

→ [Expert](expert.en.md)

</details>

## 10. 🔴 A hook setter is called without `act`; what is `result.current` right after?

- A) The new value
- B) The old value
- C) `undefined`
- D) An error is thrown

<details><summary>Answer</summary>

**B.** The update has not been applied yet.

→ [Expert](expert.en.md)

</details>

## 11. 🔴 `vi.useFakeTimers()` and `await user.click()` hangs. What is missing?

- A) `await render`
- B) `advanceTimers` in `userEvent.setup`
- C) `cleanup`
- D) `act` around render

<details><summary>Answer</summary>

**B.**

→ [Expert](expert.en.md)

</details>

## 12. 🔴 Open question: what would you test in a search form with debounce and results?

<details><summary>Key points</summary>

- No request before the delay passes; one request after typing stops (fake timers).
- Loading, results, empty and error states.
- A stale response does not replace a newer one.
- Accessible names of the field and messages.

→ [Expert](expert.en.md)

</details>
