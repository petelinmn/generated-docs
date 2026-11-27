---
title: "Forms: expert"
lang: en
track: react
kind: module
doc: expert
slug: 05-forms
part: 1
tags: [react, forms, controlled-components, validation, form-data]
versions: "React 19.3"
status: draft
updated: 2026-11-27
---

# Forms: expert

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · **3. Expert** · [4. Quiz](quiz.en.md)

## Under the hood

For a controlled field React writes your value back into the DOM after each update, which is why a state that does not change keeps the old text. For an uncontrolled field React only sets the initial value and leaves the rest to the browser. The two modes are decided by whether `value` (or `checked`) is a defined value.

### Warnings that tell you what is wrong

```tsx
<input value="x" />
// You provided a `value` prop to a form field without an `onChange` handler. This will render a read-only field. ...

const [v, setV] = useState<string | undefined>(undefined);
<input value={v} onChange={e => setV(e.target.value)} />
// A component is changing an uncontrolled input to be controlled. ...
```

## Reference

| Control | Controlled | Uncontrolled | Read in a handler |
|---|---|---|---|
| text, email, number, textarea | `value` | `defaultValue` | `e.target.value` |
| checkbox | `checked` | `defaultChecked` | `e.target.checked` |
| radio | `checked` + `name` + `value` | `defaultChecked` | `e.target.value` |
| select | `value` | `defaultValue` | `e.target.value` |
| select multiple | `value` (array) | `defaultValue` (array) | `selectedOptions` |
| file | — | read `files` | `e.target.files` |

| Form feature | Effect |
|---|---|
| `onSubmit` | called after native validation passes |
| `action={fn}` | receives `FormData`, runs in a transition, resets uncontrolled fields |
| `noValidate` | no native validation |
| `<button>` | `type="submit"` by default |
| `form.reset()` | restore default values |
| `form.requestSubmit()` | submit with validation, unlike `submit()` |
| `readOnly` / `disabled` | not editable; disabled fields are not submitted |

A file input's value cannot be set from code, so it is used as an uncontrolled field.

## Gotchas

### A value that never changes

```tsx
<input value="fixed" onChange={() => {}} />   // typing "other" leaves "fixed"
```

### defaultValue is used once

```tsx
function Def({ initial }: { initial: string }) { return <input defaultValue={initial} />; }
// render initial="one", then initial="two":  the field still shows "one"
```

To show another record, change the `key` of the form.

### Every button submits

```tsx
<form onSubmit={count}>
  <button>default</button>              {/* submits */}
  <button type="button">plain</button>  {/* does not */}
</form>
// click both: one submission
```

### Resetting

```tsx
const ref = useRef<HTMLFormElement>(null);
ref.current!.reset();      // uncontrolled fields return to their defaultValue
```

State of controlled fields is not touched by `reset()`.

## Trade-offs and alternatives

| Approach | Pro | Con |
|---|---|---|
| Controlled | full control while typing | a render per keystroke |
| Uncontrolled with `FormData` | little code, fast | values only on demand |
| `action` | pending and reset handled by React | needs React 19 |
| A form library | validation, field arrays, many fields | a dependency and its API |
| Schema validation shared with the server | one set of rules | build setup |

## Interview questions

**1. Controlled versus uncontrolled?**

<details><summary>Answer</summary>

A controlled field takes its value from state; an uncontrolled one keeps it in the DOM.

</details>

**2. Why can typing be ignored?**

<details><summary>Answer</summary>

The field is controlled and state is not updated in `onChange`.

</details>

**3. What causes "changing an uncontrolled input to be controlled"?**

<details><summary>Answer</summary>

`value` was `undefined` (or `null`) first and a string later. Start with `''`.

</details>

**4. What type is the value of `<input type="number">`?**

<details><summary>Answer</summary>

A string.

</details>

**5. Which controls are missing from `FormData`?**

<details><summary>Answer</summary>

Disabled ones, ones without `name`, and unchecked checkboxes.

</details>

**6. What does `defaultValue` do after the first render?**

<details><summary>Answer</summary>

Changing it does not change what the field shows.

</details>

**7. How do you reset a form?**

<details><summary>Answer</summary>

`form.reset()` for uncontrolled fields, set state for controlled ones, or change `key`.

</details>

**8. What is `useId` for, and what is it not for?**

<details><summary>Answer</summary>

For ids connecting elements; not for list keys.

</details>

**9. Trick question: the form `action` returned an error. Are the fields kept?**

<details><summary>Answer</summary>

Uncontrolled fields are reset if the function did not throw.

</details>

**10. Trick question: is client validation enough?**

<details><summary>Answer</summary>

No: requests can be sent without your form.

</details>

**11. What does a `<button>` inside a form do by default?**

<details><summary>Answer</summary>

Submits it.

</details>

## Related materials

- [Advanced theory](advanced.en.md)
- [Quiz](quiz.en.md) and [exercises](exercises.en.md)

## Sources

- [React: input](https://react.dev/reference/react-dom/components/input)
- [React: select](https://react.dev/reference/react-dom/components/select)
- [React: textarea](https://react.dev/reference/react-dom/components/textarea)
- [React: form](https://react.dev/reference/react-dom/components/form)
- [React: useId](https://react.dev/reference/react/useId)
- [MDN: FormData](https://developer.mozilla.org/en-US/docs/Web/API/FormData/FormData)
- [MDN: requestSubmit](https://developer.mozilla.org/en-US/docs/Web/API/HTMLFormElement/requestSubmit)
