---
title: "Actions: advanced"
lang: en
track: react
kind: module
doc: advanced
slug: 13-actions
part: 3
tags: [react, actions, forms, optimistic-updates, transitions]
versions: "React 19.3"
status: draft
updated: 2026-11-27
---

# Actions: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

## useActionState

```tsx
const [total, add, isPending] = useActionState(
  async (prev: number, data: FormData) => {
    await delay();
    return prev + Number(data.get('n'));
  }, 0);

<form action={add}><input name="n" defaultValue="5" /><button>add</button></form>
```

It returns the state, a function to start the action and `isPending`. The reducer receives the previous state and the payload. Calls are queued:

```text
two submits in a row (value 5)
  right away:           total 0, isPending true, only the first action has started
  first one finished:   total 0 still shown, the second action has started
  second one finished:  total 10, isPending false
```

## Returning errors and keeping input

The form is reset even if the action returns a validation error, because it did not throw. Give the typed values back as `defaultValue`:

```tsx
const [state, action] = useActionState(register, { error: '', name: '' });

<form action={action}>
  <input name="name" defaultValue={state.name} />
  <p role="alert">{state.error}</p>
  <button>Register</button>
</form>
// register returns { error: 'taken', name: 'Bob' }: after the reset the input shows Bob
```

## useOptimistic

```tsx
const [likes, setLikes] = useState(1);
const [shown, addOptimistic] = useOptimistic(likes, (current, delta: number) => current + delta);
const [message, setMessage] = useState('');

const like = () => startTransition(async () => {
  addOptimistic(1);
  try {
    await sendLike();
    startTransition(() => setLikes(n => n + 1));
  } catch (e) {
    setMessage((e as Error).message);
  }
});
// success: 2 at once, then stays 2
// failure: 2 at once, then back to 1, and the message appears
```

The optimistic value exists only while the action runs; afterwards React shows the real `value`. So a failure rolls back by itself, but the user still needs a message.

## Calling outside a transition

```tsx
<button onClick={() => setOptimistic('b')}>{value}</button>
// stays "a"; warning: An optimistic state update occurred outside a transition or action.

<button onClick={() => dispatch()}>{count}</button>
// warning: An async function with useActionState was called outside of a transition.
```

A form `action` is already a transition; a click handler is not.

## Best practices

- Return expected errors as state; let unexpected ones reach an error boundary.
- Disable the button while pending.
- Validate on the server too.

## Common mistakes

- ❌ Calling dispatch in a plain handler → ✅ wrap in `startTransition`.
- ❌ State updates after `await` without `startTransition` → ✅ wrap them.
- ❌ Independent parallel requests through one `useActionState` → ✅ they are queued; use transitions.

## Related materials

- [Basics](basics.en.md)
- [Expert theory](expert.en.md)

## Sources

- [React: `useActionState`](https://react.dev/reference/react/useActionState)
- [React: `useOptimistic`](https://react.dev/reference/react/useOptimistic)
- [React: `useFormStatus`](https://react.dev/reference/react-dom/hooks/useFormStatus)
- [React: `<form>`](https://react.dev/reference/react-dom/components/form)
- [React: `useTransition`](https://react.dev/reference/react/useTransition)
