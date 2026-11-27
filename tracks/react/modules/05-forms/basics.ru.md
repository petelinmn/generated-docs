---
title: "Формы: базовая теория"
lang: ru
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

# Формы: базовая теория

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [обзор](overview.ru.md) · **1. Базовая** · [2. Продвинутая](advanced.ru.md) · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [состояние и события](../03-state-and-events/overview.ru.md) · **Время:** ~2 ч

Все результаты получены на React 19.3.1.

## Управляемые поля

```tsx
function Upper() {
  const [v, setV] = useState('');
  return <input value={v} onChange={e => setV(e.target.value.toUpperCase())} />;
}
// typing "abc" shows "ABC"
```

Значение хранит состояние; каждое нажатие проходит через `onChange`, и поле показывает то, что говорит состояние. Обновляйте его синхронно, иначе курсор может прыгать.

## Неуправляемые поля

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

Значение хранит браузер. `defaultValue` задаёт только начальное. Чтобы поле попало в `FormData`, ему нужен `name`.

## Все элементы

```tsx
<input type="checkbox" checked={agree} onChange={e => setAgree(e.target.checked)} />
<input type="radio" name="plan" value="pro" checked={plan === 'pro'} onChange={e => setPlan(e.target.value)} />
<select value={city} onChange={e => setCity(e.target.value)}>...</select>
<select multiple value={tags} onChange={e => setTags(Array.from(e.target.selectedOptions, o => o.value))}>...</select>
<textarea value={bio} onChange={e => setBio(e.target.value)} />
```

Флажки и переключатели используют `checked`; `select` и `textarea` — `value` (без атрибута `selected` и без текста между тегами). Список с `multiple` принимает массив.

## Подписи

Каждому полю нужна подпись: оберните поле в `<label>` или свяжите по id. `useId` делает идентификаторы, не пересекающиеся, когда компонент встречается несколько раз.

```tsx
function Field({ label }: { label: string }) {
  const id = useId();
  return <><label htmlFor={id}>{label}</label><input id={id} /></>;
}
// <label for="_R_1_">A</label><input id="_R_1_"/><label for="_R_2_">B</label><input id="_R_2_"/>
```

## Ошибки новичка

- ❌ `value` без `onChange` → ✅ добавить обработчик или использовать `defaultValue`.
- ❌ Читать `e.target.value` у флажка → ✅ `e.target.checked`.
- ❌ Поле без подписи → ✅ `<label>` или `htmlFor` с id.
- ❌ Кнопка «отмена» без `type="button"` → ✅ она отправляет форму.

## Связанные материалы

- [Продвинутая теория](advanced.ru.md)

## Источники

- [React: input](https://react.dev/reference/react-dom/components/input)
- [React: select](https://react.dev/reference/react-dom/components/select)
- [React: textarea](https://react.dev/reference/react-dom/components/textarea)
- [React: form](https://react.dev/reference/react-dom/components/form)
- [React: useId](https://react.dev/reference/react/useId)
