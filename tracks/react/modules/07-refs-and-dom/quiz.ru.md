---
title: "Квиз: refs и DOM"
lang: ru
track: react
kind: module
doc: quiz
slug: 07-refs-and-dom
part: 2
tags: [react, refs, dom, portals, hooks]
versions: "React 19.3"
status: draft
updated: 2026-11-27
---

# Квиз: refs и DOM

🌐 **Русский** · [English](quiz.en.md)

> Сначала ответь сам, потом раскрывай ответ. Отметка показывает уровень: 🟢 [базовая теория](basics.ru.md) · 🟡 [продвинутая](advanced.ru.md) · 🔴 [экспертная](expert.ru.md).

## 1. 🟢 Что происходит при изменении `ref.current`?

- A) Рендер
- B) Само по себе ничего видимого
- C) Запускается эффект
- D) Ошибка

<details><summary>Ответ</summary>

**B.**

→ [Базовая теория](basics.ru.md)

</details>

## 2. 🟢 Как поставить фокус в поле по кнопке?

- A) `ref.current.focus()` в обработчике
- B) Задать состояние `focus`
- C) `document.write`
- D) В теле рендера

<details><summary>Ответ</summary>

**A.**

→ [Базовая теория](basics.ru.md)

</details>

## 3. 🟢 Чему равен `ref.current` ref на DOM при первом рендере?

- A) Узлу
- B) `null`
- C) Всегда `undefined`
- D) Пустому объекту

<details><summary>Ответ</summary>

**B.**

→ [Базовая теория](basics.ru.md)

</details>

## 4. 🟢 Где хранить id таймера?

- A) Переменная модуля
- B) Ref
- C) Состояние на экране
- D) Prop

<details><summary>Ответ</summary>

**B.**

→ [Базовая теория](basics.ru.md)

</details>

## 5. 🟡 Кнопка показывает `{ref.current}`, а обработчик делает `ref.current++`. После двух кликов на ней

- A) 2
- B) 0
- C) 1
- D) ничего

<details><summary>Ответ</summary>

**B.** Рендер не запрашивался.

→ [Продвинутая теория](advanced.ru.md)

</details>

## 6. 🟡 Найди ошибку

```tsx
setItems(list => [...list, item]);
listRef.current.lastElementChild.scrollIntoView();
```

<details><summary>Ответ</summary>

DOM ещё не обновлён; оберните обновление в `flushSync` или прокручивайте в эффекте.

→ [Продвинутая теория](advanced.ru.md)

</details>

## 7. 🟡 Нажата кнопка внутри портала. Какие обработчики `onClick` в React выполнятся?

- A) Только кнопки
- B) Кнопки и её предков по дереву React
- C) Только предков по DOM
- D) Никакие

<details><summary>Ответ</summary>

**B.**

→ [Продвинутая теория](advanced.ru.md)

</details>

## 8. 🟡 Выбери все уместные применения ref

- [ ] A) Фокус в поле
- [ ] B) Хранение списка на экране
- [ ] C) Хранение id интервала
- [ ] D) Прокрутка к элементу

<details><summary>Ответ</summary>

**A, C, D.**

→ [Продвинутая теория](advanced.ru.md)

</details>

## 9. 🔴 Что делает встроенный callback ref при повторном рендере?

- A) Ничего
- B) Очищается и подключается снова
- C) Только очищается
- D) Бросает ошибку

<details><summary>Ответ</summary>

**B.**

→ [Экспертная теория](expert.ru.md)

</details>

## 10. 🔴 Компоненту без prop `ref` передали `ref={r}`. Что в `r.current`?

- A) Функция компонента
- B) `null`
- C) Первый узел DOM
- D) Ошибка

<details><summary>Ответ</summary>

**B.**

→ [Экспертная теория](expert.ru.md)

</details>

## 11. 🔴 Какой хук ограничивает то, что родитель может сделать через ref?

- A) `useRef`
- B) `useImperativeHandle`
- C) `useLayoutEffect`
- D) `useId`

<details><summary>Ответ</summary>

**B.**

→ [Экспертная теория](expert.ru.md)

</details>

## 12. 🔴 Открытый вопрос: как бы вы сделали модальный диалог?

<details><summary>Ключевые пункты ответа</summary>

- Рассмотреть нативный `<dialog>` с `showModal()` через ref.
- Или портал с ручным управлением фокусом, обработкой Escape и неактивным фоном.
- Возвращать фокус на открывшую кнопку при закрытии.

→ [Экспертная теория](expert.ru.md)

</details>
