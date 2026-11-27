---
title: "Квиз: context и редьюсеры"
lang: ru
track: react
kind: module
doc: quiz
slug: 08-context-and-reducers
part: 2
tags: [react, context, use-reducer, state-management, hooks]
versions: "React 19.3"
status: draft
updated: 2026-11-27
---

# Квиз: context и редьюсеры

🌐 **Русский** · [English](quiz.en.md)

> Сначала ответь сам, потом раскрывай ответ. Отметка показывает уровень: 🟢 [базовая теория](basics.ru.md) · 🟡 [продвинутая](advanced.ru.md) · 🔴 [экспертная](expert.ru.md).

## 1. 🟢 Что возвращает редьюсер?

- A) Ничего
- B) Следующее состояние
- C) Action
- D) Компонент

<details><summary>Ответ</summary>

**B.**

→ [Базовая теория](basics.ru.md)

</details>

## 2. 🟢 Как компонент просит обновление при `useReducer`?

- A) Меняет `state`
- B) Вызывает `dispatch(action)`
- C) Вызывает редьюсер напрямую
- D) Задаёт ref

<details><summary>Ответ</summary>

**B.**

→ [Базовая теория](basics.ru.md)

</details>

## 3. 🟢 Что читает потребитель без провайдера?

- A) `undefined`
- B) Значение по умолчанию из `createContext`
- C) Ошибку
- D) Всегда `null`

<details><summary>Ответ</summary>

**B.**

→ [Базовая теория](basics.ru.md)

</details>

## 4. 🟢 Два вложенных провайдера одного context: какое значение читается внутри внутреннего?

- A) Внешнее
- B) Внутреннее
- C) Объединённое
- D) По умолчанию

<details><summary>Ответ</summary>

**B.**

→ [Базовая теория](basics.ru.md)

</details>

## 5. 🟡 `state.count` равно 0. Три `dispatch({type:'inc'})`, затем `console.log(state.count)`. Вывод?

- A) 3
- B) 0
- C) 1
- D) undefined

<details><summary>Ответ</summary>

**B.** Новое состояние принадлежит следующему рендеру.

→ [Продвинутая теория](advanced.ru.md)

</details>

## 6. 🟡 Найди ошибку

```tsx
case 'add':
  state.items.push(action.item);
  return state;
```

<details><summary>Ответ</summary>

Редьюсер мутирует и возвращает тот же объект, поэтому повторного рендера нет. Верните `{ ...state, items: [...state.items, action.item] }`.

→ [Продвинутая теория](advanced.ru.md)

</details>

## 7. 🟡 Выбери способы уменьшить рендеры из-за context

- A) Мемоизировать значение
- B) Разделить на несколько context
- C) Обернуть потребителей в `memo`
- D) Опустить состояние ближе к пользователям

<details><summary>Ответ</summary>

**A, B, D.** `memo` не блокирует обновления из context.

→ [Продвинутая теория](advanced.ru.md)

</details>

## 8. 🟡 Зачем выносить `dispatch` в отдельный context?

- A) Это обязательно
- B) Только отправляющие компоненты не будут рендериться при изменении состояния
- C) Это делает редьюсер чистым
- D) Только ради TypeScript

<details><summary>Ответ</summary>

**B.**

→ [Продвинутая теория](advanced.ru.md)

</details>

## 9. 🔴 Что даст потребителю `<Theme value={undefined}>`?

- A) Значение по умолчанию
- B) `undefined`
- C) Внешнее значение
- D) Ошибку

<details><summary>Ответ</summary>

**B.**

→ [Экспертная теория](expert.ru.md)

</details>

## 10. 🔴 В StrictMode редьюсер делает `s.items.push(a)`. Один dispatch добавит

- A) Один элемент
- B) Два элемента
- C) Ничего
- D) Ошибку

<details><summary>Ответ</summary>

**B.** Редьюсер вызывается дважды над одним объектом.

→ [Экспертная теория](expert.ru.md)

</details>

## 11. 🔴 Что можно вызвать внутри `if`?

- A) `useContext`
- B) `use`
- C) `useReducer`
- D) `useState`

<details><summary>Ответ</summary>

**B.**

→ [Экспертная теория](expert.ru.md)

</details>

## 12. 🔴 Открытый вопрос: ввод в поиске перерисовывает всё приложение, потому что запрос лежит в глобальном context. Что изменить?

<details><summary>Ключевые пункты ответа</summary>

- Держать запрос локальным состоянием рядом с поиском и результатами.
- Если он нужен в нескольких местах, дать отдельный context и мемоизировать значение.
- Для выборочных подписок рассмотреть внешнее хранилище.

→ [Экспертная теория](expert.ru.md)

</details>
