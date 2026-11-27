---
title: "Actions: продвинутая теория"
lang: ru
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

# Actions: продвинутая теория

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · **2. Продвинутая** · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

## useActionState

```tsx
const [total, add, isPending] = useActionState(
  async (prev: number, data: FormData) => {
    await delay();
    return prev + Number(data.get('n'));
  }, 0);

<form action={add}><input name="n" defaultValue="5" /><button>add</button></form>
```

Возвращает состояние, функцию запуска action и `isPending`. Reducer получает предыдущее состояние и переданные данные. Вызовы выстраиваются в очередь:

```text
two submits in a row (value 5)
  right away:           total 0, isPending true, only the first action has started
  first one finished:   total 0 still shown, the second action has started
  second one finished:  total 10, isPending false
```

## Возврат ошибок и сохранение ввода

Форма сбрасывается, даже если action вернул ошибку валидации, ведь он не бросил исключение. Верните введённое через `defaultValue`:

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

Оптимистичное значение живёт, только пока выполняется action; затем React показывает настоящий `value`. Поэтому при неудаче откат происходит сам, но сообщение пользователю всё равно нужно.

## Вызов вне transition

```tsx
<button onClick={() => setOptimistic('b')}>{value}</button>
// stays "a"; warning: An optimistic state update occurred outside a transition or action.

<button onClick={() => dispatch()}>{count}</button>
// warning: An async function with useActionState was called outside of a transition.
```

`action` формы уже является transition, обработчик клика — нет.

## Лучшие практики

- Ожидаемые ошибки возвращайте как состояние; неожиданные пусть доходят до error boundary.
- Отключайте кнопку на время ожидания.
- Проверяйте данные и на сервере.

## Частые ошибки

- ❌ Вызывать dispatch в обычном обработчике → ✅ обернуть в `startTransition`.
- ❌ Обновлять состояние после `await` без `startTransition` → ✅ обернуть.
- ❌ Независимые параллельные запросы через один `useActionState` → ✅ они встанут в очередь; используйте transitions.

## Связанные материалы

- [Базовая теория](basics.ru.md)
- [Экспертная теория](expert.ru.md)

## Источники

- [React: `useActionState`](https://react.dev/reference/react/useActionState)
- [React: `useOptimistic`](https://react.dev/reference/react/useOptimistic)
- [React: `useFormStatus`](https://react.dev/reference/react-dom/hooks/useFormStatus)
- [React: `<form>`](https://react.dev/reference/react-dom/components/form)
- [React: `useTransition`](https://react.dev/reference/react/useTransition)
