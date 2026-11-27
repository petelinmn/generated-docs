---
title: "Forms: basics"
lang: en
track: react
kind: module
doc: basics
slug: 05-forms
part: 1
tags: [react, forms, controlled-components, validation, form-data]
versions: "React 19.3"
status: draft
updated: 2026-11-27
---

# Forms: basics

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [state and events](../03-state-and-events/overview.en.md) · **Time:** ~2 h

All results were produced with React 19.3.1.

## Controlled fields

```tsx
function Upper() {
  const [v, setV] = useState('');
  return <input value={v} onChange={e => setV(e.target.value.toUpperCase())} />;
}
// typing "abc" shows "ABC"
```

State holds the value; every keystroke goes through `onChange`, and the field shows whatever state says. Update it synchronously, or the caret may jump.

## Uncontrolled fields

```tsx
function Search() {
  function submit(e: SubmitEvent<HTMLFormElement>) {     // SubmitEvent from 'react'
    e.preventDefault();
    const data = new FormData(e.currentTarget);
    console.log(data.get('q'));
  }
  return <form onSubmit={submit}><input name="q" defaultValue="" /><button>go</button></form>;
}
```

The browser keeps the value. `defaultValue` sets only the starting one. Fields need a `name` to appear in `FormData`.

## Every control

```tsx
<input type="checkbox" checked={agree} onChange={e => setAgree(e.target.checked)} />
<input type="radio" name="plan" value="pro" checked={plan === 'pro'} onChange={e => setPlan(e.target.value)} />
<select value={city} onChange={e => setCity(e.target.value)}>...</select>
<select multiple value={tags} onChange={e => setTags(Array.from(e.target.selectedOptions, o => o.value))}>...</select>
<textarea value={bio} onChange={e => setBio(e.target.value)} />
```

Checkboxes and radio buttons use `checked`; `select` and `textarea` use `value` (no `selected` attribute, no text between tags). A `multiple` select takes an array.

## Labels

Every field needs a label: wrap the input in `<label>`, or link them by id. `useId` makes ids that do not collide when the component appears several times.

```tsx
function Field({ label }: { label: string }) {
  const id = useId();
  return <><label htmlFor={id}>{label}</label><input id={id} /></>;
}
// <label for="_R_1_">A</label><input id="_R_1_"/><label for="_R_2_">B</label><input id="_R_2_"/>
```

## Beginner mistakes

- ❌ `value` without `onChange` → ✅ add the handler, or use `defaultValue`.
- ❌ Reading `e.target.value` of a checkbox → ✅ `e.target.checked`.
- ❌ A field without a label → ✅ `<label>` or `htmlFor` with an id.
- ❌ A "cancel" button without `type="button"` → ✅ it submits the form.

## Related materials

- [Advanced theory](advanced.en.md)

## Sources

- [React: input](https://react.dev/reference/react-dom/components/input)
- [React: select](https://react.dev/reference/react-dom/components/select)
- [React: textarea](https://react.dev/reference/react-dom/components/textarea)
- [React: form](https://react.dev/reference/react-dom/components/form)
- [React: useId](https://react.dev/reference/react/useId)
