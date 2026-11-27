---
title: "Actions: экспертная теория"
lang: ru
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

# Actions: экспертная теория

🌐 **Русский** · [English](expert.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · [2. Продвинутая](advanced.ru.md) · **3. Экспертная** · [4. Квиз](quiz.ru.md)

## Под капотом

Action — это transition: React считает обновление ожидающим, пока не завершится возвращённый промис, и сохраняет отзывчивость интерфейса. `useActionState` добавляет очередь; `useOptimistic` — временный слой поверх настоящего значения, живущий столько же, сколько action.

## Справочник

| API | Сигнатура или поведение |
|---|---|
| `<form action={fn}>` | `fn(formData)` в transition; при успехе поля сбрасываются |
| `<form action="/url">` | обычная HTML-отправка; действует `method` |
| `formAction` | замена для конкретной кнопки |
| `useActionState(fn, initial, permalink?)` | `[state, dispatch, isPending]`; `fn(prev, payload)` |
| `useOptimistic(value, reducer?)` | `[shown, setOptimistic]` |
| `useFormStatus()` | `{ pending, data, method, action }` родительской формы |
| `requestFormReset(form)` | сброс после ручной отправки |

Server Functions (`'use server'`) требуют фреймворка с Server Components; переданные форме, они позволяют ей работать до загрузки JavaScript.

## Подвохи

### Статус собственной формы

```tsx
function Form() {
  const { pending } = useFormStatus();   // always false: the form is not a parent
  return <form action={save}><button disabled={pending}>Save</button></form>;
}
```

### Очередь ничего не показывает до конца

```text
two submits in a row (value 5)
  right away:           total 0, isPending true, only the first action has started
  first one finished:   total 0 still shown, the second action has started
  second one finished:  total 10, isPending false
```

### Возвращённая ошибка всё равно сбрасывает форму

```tsx
const [state, action] = useActionState(register, { error: '', name: '' });

<form action={action}>
  <input name="name" defaultValue={state.name} />
  <p role="alert">{state.error}</p>
  <button>Register</button>
</form>
// register returns { error: 'taken', name: 'Bob' }: after the reset the input shows Bob
```

### Вне transition

```tsx
<button onClick={() => setOptimistic('b')}>{value}</button>
// stays "a"; warning: An optimistic state update occurred outside a transition or action.

<button onClick={() => dispatch()}>{count}</button>
// warning: An async function with useActionState was called outside of a transition.
```

### onSubmit с preventDefault

```tsx
<form onSubmit={e => { e.preventDefault(); log('onSubmit'); }} action={() => log('action')}>
// only "onSubmit": preventDefault cancels the action
```

### method в useFormStatus

При прогоне в jsdom `useFormStatus().method` был `"get"` у формы с функцией в `action` без `method`. Не определяйте по нему, как отправляются данные.

## Компромиссы и альтернативы

| Подход | Плюс | Минус |
|---|---|---|
| Action формы | мало кода, `FormData` | поля сбрасываются |
| `onSubmit` + состояние | полный контроль | ожидание и ошибки вручную |
| `useActionState` | порядок и `isPending` | нет параллельности |
| Библиотека форм | валидация полей | зависимость |
| Мутации библиотеки данных | кэш, повторы | зависимость |

## Вопросы с собеседований

**1. Что такое action?**

<details><summary>Ответ</summary>

Функция, выполняемая внутри transition, в том числе переданная в `action` формы.

</details>

**2. Что получает action формы?**

<details><summary>Ответ</summary>

Объект `FormData`.

</details>

**3. Когда форма сбрасывается?**

<details><summary>Ответ</summary>

После того как action-функция завершилась без исключения, даже если вернула ошибку.

</details>

**4. Что возвращает `useActionState`?**

<details><summary>Ответ</summary>

Состояние, функцию запуска и `isPending`.

</details>

**5. Как выполняются повторные вызовы?**

<details><summary>Ответ</summary>

Последовательно; каждый получает предыдущий результат.

</details>

**6. Что если reducer бросил исключение?**

<details><summary>Ответ</summary>

Очередь отменяется, ошибка уходит в error boundary.

</details>

**7. Гарантирован ли порядок обычных transitions?**

<details><summary>Ответ</summary>

Нет; асинхронные запросы могут завершиться не по порядку.

</details>

**8. Как откатывается оптимистичное состояние?**

<details><summary>Ответ</summary>

Когда action заканчивается, снова показывается настоящее значение.

</details>

**9. Подвох: почему `pending` из `useFormStatus` всегда false?**

<details><summary>Ответ</summary>

Он вызван вне формы — в компоненте, который её отрисовывает.

</details>

**10. Подвох: `onSubmit` вызвал `preventDefault`. Выполнится ли `action`?**

<details><summary>Ответ</summary>

Нет.

</details>

**11. Для чего `permalink`?**

<details><summary>Ответ</summary>

Progressive enhancement с Server Functions: куда перейти, если форму отправили до загрузки JavaScript.

</details>

## Связанные материалы

- [Продвинутая теория](advanced.ru.md)
- [Квиз](quiz.ru.md) и [упражнения](exercises.ru.md)

## Источники

- [React: `useActionState`](https://react.dev/reference/react/useActionState)
- [React: `useOptimistic`](https://react.dev/reference/react/useOptimistic)
- [React: `useFormStatus`](https://react.dev/reference/react-dom/hooks/useFormStatus)
- [React: `<form>`](https://react.dev/reference/react-dom/components/form)
- [React: `useTransition`](https://react.dev/reference/react/useTransition)
