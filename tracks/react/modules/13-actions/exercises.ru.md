---
title: "Упражнения: actions"
lang: ru
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

# Упражнения: actions

🌐 **Русский** · [English](exercises.en.md)

> Решай сам, прежде чем открывать подсказки.

## 1. 🟢 Форма подписки

**Задача:** отправить email и отключать кнопку на время отправки.

<details><summary>Подсказка</summary>

`useFormStatus` в дочернем компоненте.

</details>

<details><summary>Решение</summary>

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

## 2. 🟡 Регистрация с ошибкой сервера

**Задача:** показать ошибку, возвращённую `create`, и сохранить введённое имя.

<details><summary>Подсказка</summary>

Вернуть и то и другое из reducer `useActionState`.

</details>

<details><summary>Решение</summary>

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

## 3. 🔴 Оптимистичный список дел

**Задача:** новый пункт появляется сразу с пометкой отправки; при неудаче исчезает, а на экране ошибка.

<details><summary>Подсказка</summary>

`useOptimistic` поверх списка; action формы уже transition.

</details>

<details><summary>Решение</summary>

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
