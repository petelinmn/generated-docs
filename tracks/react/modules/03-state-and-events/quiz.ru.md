---
title: "Квиз: состояние и события"
lang: ru
track: react
kind: module
doc: quiz
slug: 03-state-and-events
part: 1
tags: [react, state, use-state, events, hooks]
versions: "React 19.3"
status: draft
updated: 2026-11-27
---

# Квиз: состояние и события

🌐 **Русский** · [English](quiz.en.md)

> Сначала ответь сам, потом раскрывай ответ. Отметка показывает уровень: 🟢 [базовая теория](basics.ru.md) · 🟡 [продвинутая](advanced.ru.md) · 🔴 [экспертная](expert.ru.md).

## 1. 🟢 Почему изменение локальной переменной не обновляет экран?

- A) Она только для чтения
- B) Рендер не запускается, а при следующем она создаётся заново
- C) Браузер её кэширует
- D) Экран обновляют только глобальные

<details><summary>Ответ</summary>

**B.**

→ [Базовая теория](basics.ru.md)

</details>

## 2. 🟢 Что возвращает `useState`?

- A) Только значение
- B) Пару: значение и функцию установки
- C) Промис
- D) Объект с `get` и `set`

<details><summary>Ответ</summary>

**B.**

→ [Базовая теория](basics.ru.md)

</details>

## 3. 🟢 Что передаёт обработчик правильно?

- A) `onClick={save()}`
- B) `onClick={save}`
- C) `onClick="save()"`
- D) `onclick={save}`

<details><summary>Ответ</summary>

**B.**

→ [Базовая теория](basics.ru.md)

</details>

## 4. 🟢 Что не даёт отправке формы перезагрузить страницу?

- A) `stopPropagation`
- B) `preventDefault`
- C) `return null`
- D) `onClickCapture`

<details><summary>Ответ</summary>

**B.**

→ [Базовая теория](basics.ru.md)

</details>

## 5. 🟡 `n` равно 0. Что покажет следующий рендер?

```tsx
setN(n + 1); setN(n + 1); setN(n + 1);
```

<details><summary>Ответ</summary>

```text
1
```

Каждый вызов берёт один и тот же снимок.

→ [Продвинутая теория](advanced.ru.md)

</details>

## 6. 🟡 `n` равно 4. А здесь?

```tsx
setN(n + 5); setN(x => x + 1);
```

<details><summary>Ответ</summary>

```text
10
```

→ [Продвинутая теория](advanced.ru.md)

</details>

## 7. 🟡 На экране 0. Что выведется после клика?

```tsx
setN(n + 1);
setTimeout(() => console.log(n), 5);
```

<details><summary>Ответ</summary>

```text
0
```

Таймер замкнул старый снимок.

→ [Продвинутая теория](advanced.ru.md)

</details>

## 8. 🟡 Найди ошибку

```tsx
items.push(newItem);
setItems(items);
```

<details><summary>Ответ</summary>

Устанавливается тот же массив, рендер не запрашивается. Нужно `setItems([...items, newItem])`.

→ [Продвинутая теория](advanced.ru.md)

</details>

## 9. 🟡 Выбери, что сбрасывает состояние компонента

- A) Новый `key`
- B) Изменение props
- C) Другой тип элемента в этой позиции
- D) Удаление из дерева

<details><summary>Ответ</summary>

**A, C, D.** Одни только props состояние не сбрасывают.

→ [Продвинутая теория](advanced.ru.md)

</details>

## 10. 🔴 Почему React требует одинакового порядка хуков при каждом рендере?

- A) Только ради скорости
- B) Состояние сопоставляется с хуками по порядку вызовов
- C) Из-за TypeScript
- D) Не требует

<details><summary>Ответ</summary>

**B.**

→ [Экспертная теория](expert.ru.md)

</details>

## 11. 🔴 Сколько раз вызовется `expensive` в `useState(expensive())` после первого рендера и двух обновлений (без StrictMode)?

- A) 1
- B) 2
- C) 3
- D) 0

<details><summary>Ответ</summary>

**C.** Вызов входит в каждый рендер; `useState(expensive)` вызвал бы один раз.

→ [Экспертная теория](expert.ru.md)

</details>

## 12. 🔴 Открытый вопрос: окно чата сохраняет черновик прошлого собеседника после переключения. Объясните и исправьте.

<details><summary>Ключевые пункты ответа</summary>

- Тот же компонент остаётся в той же позиции, состояние сохраняется.
- Дать `key={contact.id}`.
- Если черновики нужно помнить по собеседникам, держать их в объекте родителя по id.

→ [Экспертная теория](expert.ru.md)

</details>
