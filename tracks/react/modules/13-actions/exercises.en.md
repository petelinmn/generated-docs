---
title: "Exercises: actions"
lang: en
track: react
kind: module
doc: exercises
slug: 13-actions
part: 3
tags: [react, actions, forms, optimistic-updates, transitions]
versions: "React 19.3"
status: draft
updated: 2026-11-27
---

# Exercises: actions

🌐 [Русский](exercises.ru.md) · **English**

> Solve it yourself before opening the hints.

## 1. 🟢 Subscribe form

**Task:** submit an email and disable the button while sending.

<details><summary>Hint</summary>

`useFormStatus` in a child component.

</details>

<details><summary>Solution</summary>

```tsx
function Submit() {
  const { pending } = useFormStatus();
  return <button disabled={pending}>{pending ? 'Sending' : 'Send'}</button>;
}

function Subscribe({ send }: { send: (email: string) => Promise<void> }) {
  return <form action={async data => { await send(String(data.get('email'))); }}>
    <input name="email" aria-label="email" />
    <Submit />
  </form>;
}
```

</details>

## 2. 🟡 Registration with a server error

**Task:** show the error returned by `create` and keep the typed name.

<details><summary>Hint</summary>

Return both from the `useActionState` reducer.

</details>

<details><summary>Solution</summary>

```tsx
type State = { error: string; name: string };

function Register({ create }: { create: (name: string) => Promise<string | null> }) {
  const [state, action, isPending] = useActionState(
    async (_: State, data: FormData): Promise<State> => {
      const name = String(data.get('name'));
      const error = await create(name);
      return error ? { error, name } : { error: '', name: '' };
    }, { error: '', name: '' });
  return <form action={action}>
    <input name="name" aria-label="name" defaultValue={state.name} />
    {state.error && <p role="alert">{state.error}</p>}
    <button disabled={isPending}>Register</button>
  </form>;
}
```

</details>

## 3. 🔴 Optimistic todo list

**Task:** a new item appears at once, marked as sending; on failure it disappears and an error is shown.

<details><summary>Hint</summary>

`useOptimistic` over the list; a form action is already a transition.

</details>

<details><summary>Solution</summary>

```tsx
type Todo = { text: string; sending?: boolean };

function Todos({ save }: { save: (text: string) => Promise<void> }) {
  const [todos, setTodos] = useState<Todo[]>([]);
  const [error, setError] = useState('');
  const [shown, addOptimistic] = useOptimistic(todos,
    (list, text: string) => [...list, { text, sending: true }]);

  async function add(data: FormData) {
    const text = String(data.get('text'));
    addOptimistic(text);
    setError('');
    try {
      await save(text);
      startTransition(() => setTodos(list => [...list, { text }]));
    } catch (e) {
      startTransition(() => setError((e as Error).message));
    }
  }

  return <>
    <form action={add}><input name="text" aria-label="text" /><button>Add</button></form>
    <ul>{shown.map((t, i) => <li key={i}>{t.text}{t.sending ? ' (sending)' : ''}</li>)}</ul>
    {error && <p role="alert">{error}</p>}
  </>;
}
```

</details>
