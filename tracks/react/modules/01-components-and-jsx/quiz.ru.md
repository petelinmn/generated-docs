---
title: "Квиз: компоненты и JSX"
lang: ru
track: react
kind: module
doc: quiz
slug: 01-components-and-jsx
part: 1
tags: [react, jsx, components, rendering]
versions: "React 19.3"
status: draft
updated: 2026-11-27
---

# Квиз: компоненты и JSX

🌐 **Русский** · [English](quiz.en.md)

> Сначала ответь сам, потом раскрывай ответ. Отметка показывает уровень: 🟢 [базовая теория](basics.ru.md) · 🟡 [продвинутая](advanced.ru.md) · 🔴 [экспертная](expert.ru.md).

## 1. 🟢 Что такое компонент React?

- A) HTML-файл
- B) Функция, возвращающая JSX
- C) CSS-класс
- D) Узел DOM

<details><summary>Ответ</summary>

**B.**

→ [Базовая теория](basics.ru.md)

</details>

## 2. 🟢 Какой JSX верен?

- A) `<div class="a"></div>`
- B) `<img className="a">`
- C) `<div className="a" />`
- D) `<div style="color:red" />`

<details><summary>Ответ</summary>

**C.** A использует `class`, B не закрыт, D передаёт в `style` строку.

→ [Базовая теория](basics.ru.md)

</details>

## 3. 🟢 Что принимают фигурные скобки `{}`?

- A) Любую инструкцию
- B) Выражение
- C) Только строки
- D) Только переменные

<details><summary>Ответ</summary>

**B.**

→ [Базовая теория](basics.ru.md)

</details>

## 4. 🟢 Как вернуть два элемента без узла-обёртки?

- A) Фрагмент `<>...</>`
- B) `<div>`
- C) Два `return`
- D) Это невозможно

<details><summary>Ответ</summary>

**A.**

→ [Базовая теория](basics.ru.md)

</details>

## 5. 🟡 Что выведет это?

```tsx
<p>{0}{false}{null}{'x'}</p>
```

<details><summary>Ответ</summary>

```text
<p>0x</p>
```

`false` и `null` пропускаются, `0` выводится текстом.

→ [Продвинутая теория](advanced.ru.md)

</details>

## 6. 🟡 Найди ошибку

```tsx
const messages = [];
return <div>{messages.length && <ul>...</ul>}</div>;
```

<details><summary>Ответ</summary>

Выведется `0`. Нужно `messages.length > 0 &&`.

→ [Продвинутая теория](advanced.ru.md)

</details>

## 7. 🟡 Что будет с `<p>{'<b>x</b>'}</p>`?

- A) Жирный текст
- B) Теги покажутся как текст
- C) Ошибка
- D) Ничего

<details><summary>Ответ</summary>

**B.** Строки экранируются.

→ [Продвинутая теория](advanced.ru.md)

</details>

## 8. 🟡 Выбери все верные утверждения о чистых компонентах

- [ ] A) Один и тот же вход даёт один и тот же выход
- [ ] B) Они могут менять глобальный счётчик при рендере
- [ ] C) React может вызвать их не один раз
- [ ] D) Случайность при рендере нарушает чистоту

<details><summary>Ответ</summary>

**A, C, D.**

→ [Продвинутая теория](advanced.ru.md)

</details>

## 9. 🟡 `const tag = "h2"`. Как вывести такой тег?

- A) `<tag>`
- B) `const Tag = tag; <Tag />`
- C) `<{tag}>`
- D) `<"h2">`

<details><summary>Ответ</summary>

**B.** `<tag>` со строчной создал бы элемент с именем `tag`.

→ [Продвинутая теория](advanced.ru.md)

</details>

## 10. 🔴 Во что компилируется JSX?

- A) Узлы DOM
- B) Вызовы, создающие объекты-элементы
- C) Строки HTML
- D) Веб-компоненты

<details><summary>Ответ</summary>

**B.**

→ [Экспертная теория](expert.ru.md)

</details>

## 11. 🔴 Что выведет это?

```tsx
<p>
  one <i>two</i>
  three
</p>
```

<details><summary>Ответ</summary>

```text
<p>one <i>two</i>three</p>
```

Перенос строки после тега удаляется вместе с пробелами.

→ [Экспертная теория](expert.ru.md)

</details>

## 12. 🔴 Открытый вопрос: почему глобальная переменная плохо подходит как данные компонента?

<details><summary>Ключевые пункты ответа</summary>

- Её изменение не запускает рендер.
- Она общая для всех экземпляров.
- Рендер становится нечистым, и результат зависит от того, сколько раз React вызвал компонент.

→ [Экспертная теория](expert.ru.md)

</details>
