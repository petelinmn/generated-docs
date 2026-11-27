---
title: "Actions: базовая теория"
lang: ru
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

# Actions: базовая теория

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [обзор](overview.ru.md) · **1. Базовая** · [2. Продвинутая](advanced.ru.md) · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [формы](../05-forms/overview.ru.md), [transitions](../12-concurrent-features/overview.ru.md) · **Время:** ~2 ч

Все результаты получены на React 19.3.1 в jsdom.

## Функция в качестве action формы

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

Функция получает `FormData` с именованными полями. `preventDefault` не нужен. Когда она завершается без исключения, React сбрасывает неуправляемые поля.

## Показ хода выполнения

```tsx
function Submit() {
  const { pending, data } = useFormStatus();   // from 'react-dom'
  return <button disabled={pending}>{pending ? `Saving ${data?.get('name')}` : 'Save'}</button>;
}
<form action={save}><input name="name" /><Submit /></form>
```

`useFormStatus` возвращает `pending`, `data`, `method` и `action` **родительской** формы. Вызывайте его в компоненте, отрисованном внутри формы:

```tsx
function Form() {
  const { pending } = useFormStatus();   // always false: the form is not a parent
  return <form action={save}><button disabled={pending}>Save</button></form>;
}
```

## Несколько кнопок

```tsx
<form action={publish}>
  <button>Publish</button>
  <button formAction={saveDraft}>Save draft</button>
</form>
```

`formAction` кнопки заменяет `action` формы для этой кнопки.

## Ошибки новичка

- ❌ Поля без `name` → ✅ их нет в `FormData`.
- ❌ `useFormStatus` рядом с тегом `<form>` → ✅ в потомке формы.
- ❌ Ждать, что текст в полях останется → ✅ они сбрасываются.

## Связанные материалы

- [Продвинутая теория](advanced.ru.md)

## Источники

- [React: `useActionState`](https://react.dev/reference/react/useActionState)
- [React: `useOptimistic`](https://react.dev/reference/react/useOptimistic)
- [React: `useFormStatus`](https://react.dev/reference/react-dom/hooks/useFormStatus)
- [React: `<form>`](https://react.dev/reference/react-dom/components/form)
- [React: `useTransition`](https://react.dev/reference/react/useTransition)
