---
title: "Actions"
lang: en
track: react
kind: module
doc: overview
slug: 13-actions
part: 3
levels: [1, 2, 3, 4]
tags: [react, actions, forms, optimistic-updates, transitions]
versions: "React 19.3"
status: draft
updated: 2026-11-26
---

# Actions

🌐 [Русский](overview.ru.md) · **English**

## In a nutshell

An action is a function, often asynchronous, that runs inside a transition. A form can take such a function in `action`; React then tracks the pending state, and hooks help to keep the result, show progress and show the expected outcome before the server answers.

## Key ideas

- `<form action={fn}>` calls the function with `FormData` inside a transition.
- After a successful action uncontrolled fields are reset.
- `useActionState` keeps the result and a pending flag, and runs calls one after another.
- `useFormStatus` reads the state of a parent form.
- `useOptimistic` shows a temporary value while an action runs.
- Expected errors are returned as state; thrown ones go to an error boundary.

## Minimal example

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

## When to use and when not

- ✅ Submitting forms, buttons that change data on a server.
- ✅ Instant feedback with a safe rollback.
- ❌ Live filtering as the user types: that is state, not an action.
- ❌ Optimistic updates for operations where a false success is harmful, such as payments.

## Levels of this material

| Level | What's inside |
|---|---|
| [1. Basics](basics.en.md) | form actions, `useFormStatus` |
| [2. Advanced](advanced.en.md) | `useActionState`, `useOptimistic` |
| [3. Expert](expert.en.md) | reference, gotchas, interview questions |
| [4. Self-check](quiz.en.md) | a quiz and [exercises](exercises.en.md) |
| 5. Runnable examples | not created yet — `/learn react 13-actions level 5` |
| 6. Tests | not created yet — `/learn react 13-actions level 6` |

## Related materials

- Prerequisites: [05 · Forms](../05-forms/overview.en.md), [12 · Concurrent features](../12-concurrent-features/overview.en.md)
- Promises: [JavaScript async](../../../javascript/modules/12-promises-and-async-await/overview.en.md)
- Next: the following module of the [React roadmap](../../overview.en.md)
- [14 · Server rendering](../14-server-rendering/overview.en.md)
