---
title: "Формы: продвинутая теория"
lang: ru
track: react
kind: module
doc: advanced
slug: 05-forms
part: 1
tags: [react, forms, controlled-components, validation, form-data]
versions: "React 19.3"
status: draft
updated: 2026-11-27
---

# Формы: продвинутая теория

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · **2. Продвинутая** · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

## Числа — это строки

```tsx
<input type="number" value={v} onChange={e => setV(e.target.value)} />
// after typing 5:  typeof v === 'string', v === '5'

Number('');       // 0
Number(' ');      // 0
Number('1,5');    // NaN
parseFloat('1,5');// 1
```

Держите в состоянии строку и преобразуйте при использовании. Иначе пустое поле превратится в `0`, а пользователь не сможет набрать промежуточные значения вроде `-` или `1.`.

## Чтение всей формы

```tsx
<input name="email" defaultValue="a@b.c" />
<input name="age" type="number" defaultValue="30" />
<input type="checkbox" name="news" />
<input type="checkbox" name="topic" value="x" defaultChecked />
<input type="checkbox" name="topic" value="y" defaultChecked />
<input name="off" defaultValue="no" disabled />
<input defaultValue="noname" />

Object.fromEntries(data);   // {"email":"a@b.c","age":"30","topic":"y"}
data.getAll('topic');       // ["x","y"]
```

Неотмеченные флажки, отключённые поля и поля без `name` отсутствуют. `Object.fromEntries` оставляет только последнее из повторяющихся имён, поэтому для них используйте `getAll`.

## Проверка

Браузер проверяет `required`, `type`, `min`, `pattern` и подобное до отправки:

```tsx
<form onSubmit={handleSubmit}>
  <input name="email" type="email" required />
  <button>Send</button>
</form>
// with the field empty the browser does not submit: handleSubmit is not called
```

Правила, которых браузер не знает, — обычные функции от значений. Ошибки вычисляйте при рендере, а не храните в состоянии:

```tsx
function validate(values: { name: string; age: string }) {
  const errors: { name?: string; age?: string } = {};
  if (!values.name.trim()) errors.name = 'Name is required';
  const age = Number(values.age);
  if (!values.age.trim() || !Number.isInteger(age) || age < 18) errors.age = 'Age must be 18 or more';
  return errors;
}
// validate({ name: ' ', age: '' })  ->  both errors
// validate({ name: 'Ann', age: '30' })  ->  {}
```

`noValidate` на форме отключает встроенную проверку, когда вы показываете свои сообщения.

## Доступные сообщения об ошибках

```tsx
<input id={id} aria-invalid={Boolean(error)} aria-describedby={error ? `${id}-error` : undefined} />
{error && <p id={`${id}-error`} role="alert">{error}</p>}
```

## Actions форм

React 19 позволяет передать форме функцию в `action`:

```tsx
function Action() {
  return (
    <form action={(data: FormData) => { save(String(data.get('q'))); }}>
      <input name="q" defaultValue="" />
      <button>go</button>
    </form>
  );
}
// type "react" and submit:  save receives 'react', and the field is empty again
```

Она получает `FormData`, выполняется в transition, а неуправляемые поля сбрасываются, когда функция завершается без исключения. Это касается и случая, когда она возвращает ошибку проверки, поэтому возвращайте введённые значения, если они должны остаться. Подробно actions разбираются в отдельном модуле.

## Лучшие практики

- Один объект значений или `FormData`; ошибки и `disabled` вычисляйте из него.
- Показывайте ошибки после попытки отправки или ухода с поля.
- Проверяйте повторно на сервере.
- Всю форму сбрасывайте сменой `key`.

## Частые ошибки

- ❌ `Number(value)` в `onChange` → ✅ хранить строку.
- ❌ Ошибки в состоянии, обновляемые эффектами → ✅ вычислять.
- ❌ Отключать отправку без объяснения → ✅ показывать, чего не хватает.

## Связанные материалы

- [Базовая теория](basics.ru.md)
- [Экспертная теория](expert.ru.md)

## Источники

- [React: input](https://react.dev/reference/react-dom/components/input)
- [React: select](https://react.dev/reference/react-dom/components/select)
- [React: textarea](https://react.dev/reference/react-dom/components/textarea)
- [React: form](https://react.dev/reference/react-dom/components/form)
- [React: useId](https://react.dev/reference/react/useId)
- [MDN: FormData](https://developer.mozilla.org/en-US/docs/Web/API/FormData/FormData)
- [MDN: requestSubmit](https://developer.mozilla.org/en-US/docs/Web/API/HTMLFormElement/requestSubmit)
