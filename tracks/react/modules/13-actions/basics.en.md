---
title: "Actions: basics"
lang: en
track: react
kind: module
doc: basics
slug: 13-actions
part: 3
tags: [react, actions, forms, optimistic-updates, transitions]
versions: "React 19.3"
status: draft
updated: 2026-11-27
---

# Actions: basics

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [forms](../05-forms/overview.en.md), [transitions](../12-concurrent-features/overview.en.md) · **Time:** ~2 h

All results were produced with React 19.3.1 in jsdom.

## A function as the form action

```tsx
async function save(data: FormData) {
  await send(String(data.get('name')));
}

<form action={save}>
  <input name="name" />
  <button>Save</button>
</form>
// while save is running the input keeps "Ann"; after it finishes the input is empty
```

The function receives a `FormData` with the named fields. No `preventDefault` is needed. When it finishes without throwing, React resets uncontrolled fields.

## Showing progress

```tsx
function Submit() {
  const { pending, data } = useFormStatus();   // from 'react-dom'
  return <button disabled={pending}>{pending ? `Saving ${data?.get('name')}` : 'Save'}</button>;
}
<form action={save}><input name="name" /><Submit /></form>
```

`useFormStatus` returns `pending`, `data`, `method` and `action` of the **parent** form. Put it in a component rendered inside the form:

```tsx
function Form() {
  const { pending } = useFormStatus();   // always false: the form is not a parent
  return <form action={save}><button disabled={pending}>Save</button></form>;
}
```

## Several buttons

```tsx
<form action={publish}>
  <button>Publish</button>
  <button formAction={saveDraft}>Save draft</button>
</form>
```

A button's `formAction` replaces the form's `action` for that button.

## Beginner mistakes

- ❌ Inputs without `name` → ✅ they are absent from `FormData`.
- ❌ `useFormStatus` next to the `<form>` tag → ✅ in a child of the form.
- ❌ Expecting the fields to keep their text → ✅ they are reset.

## Related materials

- [Advanced theory](advanced.en.md)

## Sources

- [React: `useActionState`](https://react.dev/reference/react/useActionState)
- [React: `useOptimistic`](https://react.dev/reference/react/useOptimistic)
- [React: `useFormStatus`](https://react.dev/reference/react-dom/hooks/useFormStatus)
- [React: `<form>`](https://react.dev/reference/react-dom/components/form)
- [React: `useTransition`](https://react.dev/reference/react/useTransition)
