---
title: "Quiz: props and composition"
lang: en
track: react
kind: module
doc: quiz
slug: 02-props-and-composition
part: 1
tags: [react, props, composition, children, typescript]
versions: "React 19.3"
status: draft
updated: 2026-11-27
---

# Quiz: props and composition

🌐 [Русский](quiz.ru.md) · **English**

> Answer first, then open the answer. The mark shows the level: 🟢 [basics](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 Who may change a component's props?

- A) The component itself
- B) The parent, by rendering with new values
- C) Any sibling
- D) Nobody, ever

<details><summary>Answer</summary>

**B.**

→ [Basics](basics.en.md)

</details>

## 2. 🟢 What is `<input disabled />` passing?

- A) `disabled={true}`
- B) `disabled=""`, a false value
- C) Nothing
- D) `disabled={undefined}`

<details><summary>Answer</summary>

**A.**

→ [Basics](basics.en.md)

</details>

## 3. 🟢 Where does the text between `<Card>` and `</Card>` arrive?

- A) `props.content`
- B) `props.children`
- C) `props.body`
- D) It is discarded

<details><summary>Answer</summary>

**B.**

→ [Basics](basics.en.md)

</details>

## 4. 🟢 How does a child tell the parent that a button was clicked?

- A) It writes into props
- B) It calls a function from props
- C) It returns a value from render
- D) Through a global variable

<details><summary>Answer</summary>

**B.**

→ [Basics](basics.en.md)

</details>

## 5. 🟡 What width does `<Avatar name="E" size={null} />` get with `size = 40` as default?

- A) 40
- B) No width attribute
- C) 0
- D) An error at run time

<details><summary>Answer</summary>

**B.** The default applies only to `undefined`.

→ [Advanced](advanced.en.md)

</details>

## 6. 🟡 What `type` does the button get?

```tsx
<button type="button" {...{ type: 'submit' }} />
```

<details><summary>Answer</summary>

`submit`: the spread written later wins.

→ [Advanced](advanced.en.md)

</details>

## 7. 🟡 Select all good reasons to pass elements as props

- [ ] A) The caller decides the content
- [ ] B) Fewer configuration flags
- [ ] C) The component can freely rewrite the content
- [ ] D) Passed elements may not need re-rendering

<details><summary>Answer</summary>

**A, B, D.**

→ [Advanced](advanced.en.md)

</details>

## 8. 🟡 Find the bug

```tsx
function Field(props) {
  props.value = props.value.trim();
  return <input defaultValue={props.value} />;
}
```

<details><summary>Answer</summary>

Props are read-only; compute a local variable instead.

→ [Advanced](advanced.en.md)

</details>

## 9. 🟡 In React 19, how does a function component get a `ref`?

- A) Only through `forwardRef`
- B) As an ordinary prop
- C) It cannot
- D) Through context

<details><summary>Answer</summary>

**B.**

→ [Advanced](advanced.en.md)

</details>

## 10. 🔴 A wrapper with state renders `{children}`. How many times does `Slow` run after two updates of the wrapper, if it was passed as a child?

- A) 1
- B) 2
- C) 3
- D) 0

<details><summary>Answer</summary>

**A.** Only the first render: the element stays the same object.

→ [Expert](expert.en.md)

</details>

## 11. 🔴 What does this show after the parent changes `name` from Ann to Bob?

```tsx
const [copy] = useState(name);
return <p>{copy}|{name}</p>;
```

<details><summary>Answer</summary>

```text
Ann|Bob
```

The initial value of state is used once.

→ [Expert](expert.en.md)

</details>

## 12. 🔴 Open question: a `theme` value is passed through six components that do not use it. What can you do?

<details><summary>Key points</summary>

- Pass ready elements from the top, so the middle layers only place `children`.
- Use context for data truly needed in many places.
- Check whether the component tree can be flattened.

→ [Expert](expert.en.md)

</details>
