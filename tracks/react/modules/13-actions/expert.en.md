---
title: "Actions: expert"
lang: en
track: react
kind: module
doc: expert
slug: 13-actions
part: 3
tags: [react, actions, forms, optimistic-updates, transitions]
versions: "React 19.3"
status: draft
updated: 2026-11-27
---

# Actions: expert

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · **3. Expert** · [4. Quiz](quiz.en.md)

## Under the hood

An action is a transition: React treats the update as pending until the returned promise settles and keeps the interface responsive. `useActionState` adds a queue; `useOptimistic` adds a temporary layer over the real value that lives as long as the action.

## Reference

| API | Signature or behaviour |
|---|---|
| `<form action={fn}>` | `fn(formData)` in a transition; fields reset on success |
| `<form action="/url">` | ordinary HTML submission; `method` applies |
| `formAction` | per-button override |
| `useActionState(fn, initial, permalink?)` | `[state, dispatch, isPending]`; `fn(prev, payload)` |
| `useOptimistic(value, reducer?)` | `[shown, setOptimistic]` |
| `useFormStatus()` | `{ pending, data, method, action }` of the parent form |
| `requestFormReset(form)` | reset after a manual submission |

Server Functions (`'use server'`) need a framework with Server Components; passed to a form they let it work before JavaScript loads.

## Gotchas

### The status of your own form

```tsx
function Form() {
  const { pending } = useFormStatus();   // always false: the form is not a parent
  return <form action={save}><button disabled={pending}>Save</button></form>;
}
```

### The queue shows nothing until it is done

```text
two submits in a row (value 5)
  right away:           total 0, isPending true, only the first action has started
  first one finished:   total 0 still shown, the second action has started
  second one finished:  total 10, isPending false
```

### A returned error still resets the form

```tsx
const [state, action] = useActionState(register, { error: '', name: '' });

<form action={action}>
  <input name="name" defaultValue={state.name} />
  <p role="alert">{state.error}</p>
  <button>Register</button>
</form>
// register returns { error: 'taken', name: 'Bob' }: after the reset the input shows Bob
```

### Outside a transition

```tsx
<button onClick={() => setOptimistic('b')}>{value}</button>
// stays "a"; warning: An optimistic state update occurred outside a transition or action.

<button onClick={() => dispatch()}>{count}</button>
// warning: An async function with useActionState was called outside of a transition.
```

### onSubmit with preventDefault

```tsx
<form onSubmit={e => { e.preventDefault(); log('onSubmit'); }} action={() => log('action')}>
// only "onSubmit": preventDefault cancels the action
```

### method in useFormStatus

In the jsdom run `useFormStatus().method` was `"get"` for a form with a function in `action` and no `method`. Do not use it to learn how the data is sent.

## Trade-offs and alternatives

| Approach | Pro | Con |
|---|---|---|
| Form action | little code, `FormData` | fields reset |
| `onSubmit` + state | full control | pending and errors by hand |
| `useActionState` | ordered, has `isPending` | no parallelism |
| Form library | field validation | dependency |
| Data library mutations | cache, retries | dependency |

## Interview questions

**1. What is an action?**

<details><summary>Answer</summary>

A function run inside a transition, including the one passed to a form `action`.

</details>

**2. What does a form action receive?**

<details><summary>Answer</summary>

A `FormData` object.

</details>

**3. When is a form reset?**

<details><summary>Answer</summary>

After a function action finishes without throwing, even if it returned an error.

</details>

**4. What does `useActionState` return?**

<details><summary>Answer</summary>

State, a dispatcher and `isPending`.

</details>

**5. How are repeated calls executed?**

<details><summary>Answer</summary>

Sequentially; each gets the previous result.

</details>

**6. What if the reducer throws?**

<details><summary>Answer</summary>

Queued actions are cancelled and the error goes to an error boundary.

</details>

**7. Is the order of plain transitions guaranteed?**

<details><summary>Answer</summary>

No; async requests may finish out of order.

</details>

**8. How does optimistic state roll back?**

<details><summary>Answer</summary>

When the action ends, the real value is shown again.

</details>

**9. Trick question: why is `pending` from `useFormStatus` always false?**

<details><summary>Answer</summary>

It is called outside the form, in the component that renders it.

</details>

**10. Trick question: `onSubmit` calls `preventDefault`. Does `action` run?**

<details><summary>Answer</summary>

No.

</details>

**11. What is `permalink` for?**

<details><summary>Answer</summary>

Progressive enhancement with Server Functions: where to navigate if the form is submitted before JavaScript loads.

</details>

## Related materials

- [Advanced theory](advanced.en.md)
- [Quiz](quiz.en.md) and [exercises](exercises.en.md)

## Sources

- [React: `useActionState`](https://react.dev/reference/react/useActionState)
- [React: `useOptimistic`](https://react.dev/reference/react/useOptimistic)
- [React: `useFormStatus`](https://react.dev/reference/react-dom/hooks/useFormStatus)
- [React: `<form>`](https://react.dev/reference/react-dom/components/form)
- [React: `useTransition`](https://react.dev/reference/react/useTransition)
