---
title: "Квиз: формы"
lang: ru
track: react
kind: module
doc: quiz
slug: 05-forms
part: 1
tags: [react, forms, controlled-components, validation, form-data]
versions: "React 19.3"
status: draft
updated: 2026-11-27
---

# Квиз: формы

🌐 **Русский** · [English](quiz.en.md)

> Сначала ответь сам, потом раскрывай ответ. Отметка показывает уровень: 🟢 [базовая теория](basics.ru.md) · 🟡 [продвинутая](advanced.ru.md) · 🔴 [экспертная](expert.ru.md).

## 1. 🟢 Что делает поле управляемым?

- A) `name`
- B) `value` из состояния
- C) `defaultValue`
- D) `ref`

<details><summary>Ответ</summary>

**B.**

→ [Базовая теория](basics.ru.md)

</details>

## 2. 🟢 Как прочитать флажок?

- A) `e.target.value`
- B) `e.target.checked`
- C) `e.checked`
- D) `e.target.selected`

<details><summary>Ответ</summary>

**B.**

→ [Базовая теория](basics.ru.md)

</details>

## 3. 🟢 Как заранее выбрать вариант в списке?

- A) `selected` на `<option>`
- B) `value` или `defaultValue` на `<select>`
- C) `checked` на `<select>`
- D) Порядком вариантов

<details><summary>Ответ</summary>

**B.**

→ [Базовая теория](basics.ru.md)

</details>

## 4. 🟢 Что делает `<button>` без `type` внутри формы?

- A) Ничего
- B) Отправляет
- C) Сбрасывает
- D) Закрывает

<details><summary>Ответ</summary>

**B.**

→ [Базовая теория](basics.ru.md)

</details>

## 5. 🟡 Чему равно `v` после ввода 5 в числовое поле?

- A) Число `5`
- B) Строка `'5'`
- C) `undefined`
- D) `NaN`

<details><summary>Ответ</summary>

**B.**

→ [Продвинутая теория](advanced.ru.md)

</details>

## 6. 🟡 Что выведет это?

```tsx
Number('')
```

<details><summary>Ответ</summary>

```text
0
```

Поэтому пустое поле молча становится нулём.

→ [Продвинутая теория](advanced.ru.md)

</details>

## 7. 🟡 Выбери всё, чего нет в `FormData`

- [ ] A) Неотмеченный флажок
- [ ] B) Отключённое поле
- [ ] C) Поле без `name`
- [ ] D) Поле с `defaultValue`

<details><summary>Ответ</summary>

**A, B, C.**

→ [Продвинутая теория](advanced.ru.md)

</details>

## 8. 🟡 Найди ошибку

```tsx
const [name, setName] = useState();
return <input value={name} onChange={e => setName(e.target.value)} />;
```

<details><summary>Ответ</summary>

Первое значение `undefined`, поле начинает как неуправляемое. Нужно `useState('')`.

→ [Продвинутая теория](advanced.ru.md)

</details>

## 9. 🔴 `<input value="fixed" onChange={() => {}} />` — пользователь печатает. Итог?

- A) Текст меняется
- B) В поле остаётся `fixed`
- C) Ошибка
- D) Поле очищается

<details><summary>Ответ</summary>

**B.**

→ [Экспертная теория](expert.ru.md)

</details>

## 10. 🔴 `<input defaultValue={initial} />`: `initial` меняется с «one» на «two». В поле

- A) two
- B) one
- C) пусто
- D) onetwo

<details><summary>Ответ</summary>

**B.**

→ [Экспертная теория](expert.ru.md)

</details>

## 11. 🔴 Форма с `action` вернула ошибку проверки. Что с неуправляемыми полями?

- A) Сохраняются
- B) Сбрасываются
- C) Страница перезагружается
- D) Отключаются

<details><summary>Ответ</summary>

**B.** Функция завершилась без исключения.

→ [Экспертная теория](expert.ru.md)

</details>

## 12. 🔴 Открытый вопрос: длинная форма медленно перерисовывается на каждое нажатие. Что сделать?

<details><summary>Ключевые пункты ответа</summary>

- Сделать поля неуправляемыми и читать через `FormData`.
- Опустить состояние в компоненты полей.
- Измерить до мемоизации; рассмотреть библиотеку форм.

→ [Экспертная теория](expert.ru.md)

</details>
