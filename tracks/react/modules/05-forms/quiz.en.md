---
title: "Quiz: forms"
lang: en
track: react
kind: module
doc: quiz
slug: 05-forms
part: 1
tags: [react, forms, controlled-components, validation, form-data]
versions: "React 19.3"
status: draft
updated: 2026-11-27
---

# Quiz: forms

🌐 [Русский](quiz.ru.md) · **English**

> Answer first, then open the answer. The mark shows the level: 🟢 [basics](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 What makes an input controlled?

- A) A `name`
- B) A `value` coming from state
- C) `defaultValue`
- D) A `ref`

<details><summary>Answer</summary>

**B.**

→ [Basics](basics.en.md)

</details>

## 2. 🟢 How do you read a checkbox?

- A) `e.target.value`
- B) `e.target.checked`
- C) `e.checked`
- D) `e.target.selected`

<details><summary>Answer</summary>

**B.**

→ [Basics](basics.en.md)

</details>

## 3. 🟢 How do you preselect an option?

- A) `selected` on `<option>`
- B) `value` or `defaultValue` on `<select>`
- C) `checked` on `<select>`
- D) Order of options

<details><summary>Answer</summary>

**B.**

→ [Basics](basics.en.md)

</details>

## 4. 🟢 What does `<button>` without `type` do inside a form?

- A) Nothing
- B) Submits it
- C) Resets it
- D) Closes it

<details><summary>Answer</summary>

**B.**

→ [Basics](basics.en.md)

</details>

## 5. 🟡 What is `v` after typing 5 into a number input?

- A) The number `5`
- B) The string `'5'`
- C) `undefined`
- D) `NaN`

<details><summary>Answer</summary>

**B.**

→ [Advanced](advanced.en.md)

</details>

## 6. 🟡 What does this print?

```tsx
Number('')
```

<details><summary>Answer</summary>

```text
0
```

So an empty field silently becomes zero.

→ [Advanced](advanced.en.md)

</details>

## 7. 🟡 Select what is absent from `FormData`

- A) An unchecked checkbox
- B) A disabled input
- C) An input without `name`
- D) An input with `defaultValue`

<details><summary>Answer</summary>

**A, B, C.**

→ [Advanced](advanced.en.md)

</details>

## 8. 🟡 Find the bug

```tsx
const [name, setName] = useState();
return <input value={name} onChange={e => setName(e.target.value)} />;
```

<details><summary>Answer</summary>

The first value is `undefined`, so the input starts uncontrolled. Use `useState('')`.

→ [Advanced](advanced.en.md)

</details>

## 9. 🔴 `<input value="fixed" onChange={() => {}} />` — the user types. Result?

- A) The text changes
- B) The field keeps `fixed`
- C) An error is thrown
- D) The field clears

<details><summary>Answer</summary>

**B.**

→ [Expert](expert.en.md)

</details>

## 10. 🔴 `<input defaultValue={initial} />`: `initial` changes from "one" to "two". The field shows

- A) two
- B) one
- C) nothing
- D) onetwo

<details><summary>Answer</summary>

**B.**

→ [Expert](expert.en.md)

</details>

## 11. 🔴 A form with `action` returns a validation error. What happens to uncontrolled fields?

- A) They are kept
- B) They are reset
- C) The page reloads
- D) They become disabled

<details><summary>Answer</summary>

**B.** The function completed without throwing.

→ [Expert](expert.en.md)

</details>

## 12. 🔴 Open question: a long form re-renders slowly on each keystroke. What would you do?

<details><summary>Key points</summary>

- Make fields uncontrolled and read them with `FormData`.
- Move state down into the field components.
- Measure before memoizing; consider a form library.

→ [Expert](expert.en.md)

</details>
