---
title: "Квиз: actions"
lang: ru
track: react
kind: module
doc: quiz
slug: 13-actions
part: 3
tags: [react, actions, forms, optimistic-updates, transitions]
versions: "React 19.3"
status: draft
updated: 2026-11-27
---

# Квиз: actions

🌐 **Русский** · [English](quiz.en.md)

> Сначала ответь сам, потом раскрывай ответ. Отметка показывает уровень: 🟢 [базовая теория](basics.ru.md) · 🟡 [продвинутая](advanced.ru.md) · 🔴 [экспертная](expert.ru.md).

## 1. 🟢 Какой аргумент получает функция в `<form action>`?

- A) Событие submit
- B) `FormData`
- C) Элемент формы
- D) Ничего

<details><summary>Ответ</summary>

**B.**

→ [Базовая теория](basics.ru.md)

</details>

## 2. 🟢 Что происходит с неуправляемыми полями после успешного action?

- A) Сохраняют текст
- B) Сбрасываются к значениям по умолчанию
- C) Отключаются
- D) Удаляются

<details><summary>Ответ</summary>

**B.**

→ [Базовая теория](basics.ru.md)

</details>

## 3. 🟢 Где нужно вызывать `useFormStatus`?

- A) В компоненте, рисующем `<form>`
- B) В компоненте внутри формы
- C) Где угодно
- D) Только в корне

<details><summary>Ответ</summary>

**B.**

→ [Базовая теория](basics.ru.md)

</details>

## 4. 🟢 Что делает `formAction` у кнопки?

- A) Добавляет второй action
- B) Заменяет action формы для этой кнопки
- C) Сбрасывает форму
- D) Отключает проверку

<details><summary>Ответ</summary>

**B.**

→ [Базовая теория](basics.ru.md)

</details>

## 5. 🟡 Найди ошибку

```tsx
const [count, add] = useActionState(async (n: number) => n + 1, 0);
return <button onClick={() => add()}>{count}</button>;
```

<details><summary>Ответ</summary>

`add` вызван вне transition: оберните в `startTransition` или передайте как action формы.

→ [Продвинутая теория](advanced.ru.md)

</details>

## 6. 🟡 Сервер отклонил оптимистичный лайк, настоящее состояние не менялось. Что показано после action?

- A) Оптимистичное значение
- B) Прежнее настоящее значение
- C) Ноль
- D) Ничего

<details><summary>Ответ</summary>

**B.**

→ [Продвинутая теория](advanced.ru.md)

</details>

## 7. 🟡 Выбери все верные утверждения о `useActionState`

- [ ] A) Вызовы выполняются по очереди
- [ ] B) Reducer получает предыдущее состояние
- [ ] C) Вызовы выполняются параллельно
- [ ] D) Возвращает `isPending`

<details><summary>Ответ</summary>

**A, B, D.**

→ [Продвинутая теория](advanced.ru.md)

</details>

## 8. 🟡 Action вернул ошибку валидации. Как сохранить введённое имя?

- A) Сохранится само
- B) Вернуть его и использовать как `defaultValue`
- C) Вызвать `preventDefault` в action
- D) Бросить ошибку

<details><summary>Ответ</summary>

**B.**

→ [Продвинутая теория](advanced.ru.md)

</details>

## 9. 🔴 Reducer `useActionState` бросил ошибку, ещё два вызова в очереди. Итог?

- A) Всё равно выполнятся
- B) Отменяются, ошибка доходит до границы
- C) Состояние станет undefined
- D) Повторяются

<details><summary>Ответ</summary>

**B.**

→ [Экспертная теория](expert.ru.md)

</details>

## 10. 🔴 `onSubmit` вызывает `preventDefault`, у формы есть и `action`. Что выполнится?

- A) Оба
- B) Только `onSubmit`
- C) Только `action`
- D) Ничего

<details><summary>Ответ</summary>

**B.**

→ [Экспертная теория](expert.ru.md)

</details>

## 11. 🔴 Открытый вопрос: спроектируй кнопку «лайк» с мгновенным откликом.

<details><summary>Ключевые пункты ответа</summary>

- `useOptimistic` поверх настоящего счётчика внутри transition.
- При успехе обновить настоящее состояние; при неудаче показать сообщение.
- Сделать серверную операцию идемпотентной (поставить лайк, а не увеличить).
- Продумать повторные нажатия и порядок ответов.

→ [Экспертная теория](expert.ru.md)

</details>
