---
title: "Forms: advanced"
lang: en
track: react
kind: module
doc: advanced
slug: 05-forms
part: 1
tags: [react, forms, controlled-components, validation, form-data]
versions: "React 19.3"
status: draft
updated: 2026-11-27
---

# Forms: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

## Numbers are strings

```tsx
<input type="number" value={v} onChange={e => setV(e.target.value)} />
// after typing 5:  typeof v === 'string', v === '5'

Number('');       // 0
Number(' ');      // 0
Number('1,5');    // NaN
parseFloat('1,5');// 1
```

Keep the string in state and convert when you use it. Otherwise an empty field turns into `0`, and the user cannot type intermediate values such as `-` or `1.`.

## Reading a whole form

```tsx
<input name="email" defaultValue="a@b.c" />
<input name="age" type="number" defaultValue="30" />
<input type="checkbox" name="news" />
<input type="checkbox" name="topic" value="x" defaultChecked />
<input type="checkbox" name="topic" value="y" defaultChecked />
<input name="off" defaultValue="no" disabled />
<input defaultValue="noname" />

Object.fromEntries(data);   // {"email":"a@b.c","age":"30","topic":"y"}
data.getAll('topic');       // ["x","y"]
```

Unchecked boxes, disabled fields and fields without a `name` are absent. `Object.fromEntries` keeps only the last of repeated names, so use `getAll` for them.

## Validation

The browser checks `required`, `type`, `min`, `pattern` and the like before submitting:

```tsx
<form onSubmit={handleSubmit}>
  <input name="email" type="email" required />
  <button>Send</button>
</form>
// with the field empty the browser does not submit: handleSubmit is not called
```

Rules the browser does not know are ordinary functions of the values. Compute errors during render instead of keeping them in state:

```tsx
function validate(values: { name: string; age: string }) {
  const errors: { name?: string; age?: string } = {};
  if (!values.name.trim()) errors.name = 'Name is required';
  const age = Number(values.age);
  if (!values.age.trim() || !Number.isInteger(age) || age < 18) errors.age = 'Age must be 18 or more';
  return errors;
}
// validate({ name: ' ', age: '' })  ->  both errors
// validate({ name: 'Ann', age: '30' })  ->  {}
```

`noValidate` on the form switches the native check off when you show your own messages.

## Accessible error messages

```tsx
<input id={id} aria-invalid={Boolean(error)} aria-describedby={error ? `${id}-error` : undefined} />
{error && <p id={`${id}-error`} role="alert">{error}</p>}
```

## Form actions

React 19 lets a form take a function in `action`:

```tsx
function Action() {
  return (
    <form action={(data: FormData) => { save(String(data.get('q'))); }}>
      <input name="q" defaultValue="" />
      <button>go</button>
    </form>
  );
}
// type "react" and submit:  save receives 'react', and the field is empty again
```

It receives `FormData`, runs in a transition, and uncontrolled fields are reset when the function completes without throwing. This includes the case when the function returns a validation error, so return the entered values if they must stay. Actions are covered in depth in a later module.

## Best practices

- One object of values or `FormData`; derive errors and `disabled` from it.
- Show errors after the user tried to submit or left the field.
- Validate again on the server.
- Reset a whole form by changing its `key`.

## Common mistakes

- ❌ `Number(value)` in `onChange` → ✅ keep the string.
- ❌ Errors stored in state and updated by effects → ✅ compute them.
- ❌ Disabling submit without saying why → ✅ show what is missing.

## Related materials

- [Basics](basics.en.md)
- [Expert theory](expert.en.md)

## Sources

- [React: input](https://react.dev/reference/react-dom/components/input)
- [React: select](https://react.dev/reference/react-dom/components/select)
- [React: textarea](https://react.dev/reference/react-dom/components/textarea)
- [React: form](https://react.dev/reference/react-dom/components/form)
- [React: useId](https://react.dev/reference/react/useId)
- [MDN: FormData](https://developer.mozilla.org/en-US/docs/Web/API/FormData/FormData)
- [MDN: requestSubmit](https://developer.mozilla.org/en-US/docs/Web/API/HTMLFormElement/requestSubmit)
