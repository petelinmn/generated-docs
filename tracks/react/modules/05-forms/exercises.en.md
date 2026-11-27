---
title: "Exercises: forms"
lang: en
track: react
kind: module
doc: exercises
slug: 05-forms
part: 1
tags: [react, forms, controlled-components, validation, form-data]
versions: "React 19.3"
status: draft
updated: 2026-11-27
---

# Exercises: forms

🌐 [Русский](exercises.ru.md) · **English**

> Solve it yourself before opening the hints.

## 1. 🟢 A sign-in form

**Task:** email and "remember me"; the button is disabled while the email is empty.

<details><summary>Hint</summary>

Two pieces of state; `checked` for the checkbox.

</details>

<details><summary>Solution</summary>

```tsx
function Login({ onLogin }: { onLogin: (email: string, remember: boolean) => void }) {
  const [email, setEmail] = useState('');
  const [remember, setRemember] = useState(false);

  return (
    <form onSubmit={e => { e.preventDefault(); onLogin(email, remember); }}>
      <label>Email <input type="email" value={email} onChange={e => setEmail(e.target.value)} required /></label>
      <label><input type="checkbox" checked={remember} onChange={e => setRemember(e.target.checked)} /> Remember</label>
      <button disabled={!email}>Sign in</button>
    </form>
  );
}
```

</details>

## 2. 🟡 Several checkboxes with one name

**Task:** without any state, submit a title and all selected tags.

<details><summary>Hint</summary>

`FormData` and the method for repeated names.

</details>

<details><summary>Solution</summary>

```tsx
function Tags({ onSave }: { onSave: (title: string, tags: string[]) => void }) {
  return (
    <form
      onSubmit={e => {
        e.preventDefault();
        const data = new FormData(e.currentTarget);
        onSave(String(data.get('title')), data.getAll('tag').map(String));
      }}
    >
      <input name="title" defaultValue="" />
      <label><input type="checkbox" name="tag" value="news" /> news</label>
      <label><input type="checkbox" name="tag" value="docs" /> docs</label>
      <button>Save</button>
    </form>
  );
}
```

</details>

## 3. 🔴 Errors after submit

**Task:** a name and an age (18 or more); show messages only after the first attempt to submit, and keep them up to date while the user fixes the fields.

<details><summary>Hint</summary>

Errors can be computed from the values. What has to be state?

</details>

<details><summary>Solution</summary>

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

```tsx
function Profile() {
  const [values, setValues] = useState({ name: '', age: '' });
  const [submitted, setSubmitted] = useState(false);
  const errors = validate(values);                      // derived, not stored
  const set = (key: 'name' | 'age') => (e: ChangeEvent<HTMLInputElement>) =>
    setValues(v => ({ ...v, [key]: e.target.value }));

  return (
    <form noValidate onSubmit={e => { e.preventDefault(); setSubmitted(true); }}>
      <input aria-label="name" value={values.name} onChange={set('name')} />
      {submitted && errors.name && <p role="alert">{errors.name}</p>}
      <input aria-label="age" inputMode="numeric" value={values.age} onChange={set('age')} />
      {submitted && errors.age && <p role="alert">{errors.age}</p>}
      <button>Save</button>
    </form>
  );
}
```

</details>
