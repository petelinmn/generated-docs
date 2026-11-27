---
title: "Квиз: серверный рендеринг"
lang: ru
track: react
kind: module
doc: quiz
slug: 14-server-rendering
part: 3
tags: [react, server-side-rendering, hydration, server-components, streaming]
versions: "React 19.3"
status: draft
updated: 2026-11-27
---

# Квиз: серверный рендеринг

🌐 **Русский** · [English](quiz.en.md)

> Сначала ответь сам, потом раскрывай ответ. Отметка показывает уровень: 🟢 [базовая теория](basics.ru.md) · 🟡 [продвинутая](advanced.ru.md) · 🔴 [экспертная](expert.ru.md).

## 1. 🟢 Что возвращает `renderToString`?

- A) Узел DOM
- B) Строку HTML
- C) JavaScript-бандл
- D) Промис

<details><summary>Ответ</summary>

**B.**

→ [Базовая теория](basics.ru.md)

</details>

## 2. 🟢 Что выполняется при серверном рендере?

- A) `useEffect`
- B) Функция компонента
- C) `onClick`
- D) `useLayoutEffect`

<details><summary>Ответ</summary>

**B.**

→ [Базовая теория](basics.ru.md)

</details>

## 3. 🟢 Как запустить React на HTML с сервера?

- A) `createRoot().render`
- B) `hydrateRoot`
- C) `renderToString`
- D) `flushSync`

<details><summary>Ответ</summary>

**B.**

→ [Базовая теория](basics.ru.md)

</details>

## 4. 🟢 Найди ошибку (компонент с серверным рендером)

```tsx
function Theme() {
  const theme = localStorage.getItem('theme');
  return <p>{theme}</p>;
}
```

<details><summary>Ответ</summary>

На сервере нет `localStorage`; читайте на клиенте, например в эффекте.

→ [Базовая теория](basics.ru.md)

</details>

## 5. 🟡 Что отправляется первым при streaming с ожидающей границей?

- A) Ничего до полной готовности
- B) Каркас с fallback
- C) Только скрипты
- D) Только медленная часть

<details><summary>Ответ</summary>

**B.**

→ [Продвинутая теория](advanced.ru.md)

</details>

## 6. 🟡 Сервер отрисовал `class="a"`, клиент — `className="b"`. Вероятный DOM после гидратации?

- A) `b`
- B) `a` и ошибка в консоли
- C) Без класса
- D) Оба

<details><summary>Ответ</summary>

**B.** Исправление атрибутов не гарантировано.

→ [Продвинутая теория](advanced.ru.md)

</details>

## 7. 🟡 Выбери всё, что может Server Component

- [ ] A) Быть `async`
- [ ] B) Обращаться к базе данных
- [ ] C) Использовать `useState`
- [ ] D) Обрабатывать клики

<details><summary>Ответ</summary>

**A, B.**

→ [Продвинутая теория](advanced.ru.md)

</details>

## 8. 🟡 Что отмечает `'use server'`?

- A) Server Components
- B) Server Functions
- C) Статические страницы
- D) Серверный CSS

<details><summary>Ответ</summary>

**B.**

→ [Продвинутая теория](advanced.ru.md)

</details>

## 9. 🔴 Поток уже идёт, и компонент внутри `Suspense` падает. Код HTTP

- A) станет 500
- B) изменить уже нельзя
- C) станет 404
- D) выбирает клиент

<details><summary>Ответ</summary>

**B.**

→ [Экспертная теория](expert.ru.md)

</details>

## 10. 🔴 У `<time suppressHydrationWarning>` серверный текст 1, клиентский 2. Что показано после гидратации?

- A) 2
- B) 1
- C) Пусто
- D) 12

<details><summary>Ответ</summary>

**B.**

→ [Экспертная теория](expert.ru.md)

</details>

## 11. 🔴 Открытый вопрос: как показать дату в локали пользователя на странице с серверным рендером?

<details><summary>Ключевые пункты ответа</summary>

- На сервере и при гидратации выводить нейтральный вид (ISO или фиксированные локаль и часовой пояс).
- Переключить на местный вид вторым рендером (серверный снимок `useSyncExternalStore` или эффект).
- Или передавать серверу локаль и часовой пояс пользователя.

→ [Экспертная теория](expert.ru.md)

</details>
