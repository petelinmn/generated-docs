---
title: "Квиз: тестирование"
lang: ru
track: react
kind: module
doc: quiz
slug: 10-testing
part: 2
tags: [react, testing, vitest, testing-library, unit-testing]
versions: "React 19.3"
status: draft
updated: 2026-11-27
---

# Квиз: тестирование

🌐 **Русский** · [English](quiz.en.md)

> Сначала ответь сам, потом раскрывай ответ. Отметка показывает уровень: 🟢 [базовая теория](basics.ru.md) · 🟡 [продвинутая](advanced.ru.md) · 🔴 [экспертная](expert.ru.md).

## 1. 🟢 Что должен проверять тест компонента?

- A) Имена переменных состояния
- B) Что пользователь видит и может сделать
- C) Число рендеров
- D) Имена CSS-классов

<details><summary>Ответ</summary>

**B.**

→ [Базовая теория](basics.ru.md)

</details>

## 2. 🟢 Какой запрос предпочтителен?

- A) `getByTestId`
- B) `getByRole`
- C) `querySelector`
- D) `getByClassName`

<details><summary>Ответ</summary>

**B.**

→ [Базовая теория](basics.ru.md)

</details>

## 3. 🟢 Что даёт DOM в этих тестах?

- A) React
- B) jsdom
- C) TypeScript
- D) Сам Node.js

<details><summary>Ответ</summary>

**B.**

→ [Базовая теория](basics.ru.md)

</details>

## 4. 🟢 Что верно для user-event?

- A) `user.click(el)` без ожидания
- B) `await user.click(el)`
- C) `click(el)` из React
- D) `el.value = ...`

<details><summary>Ответ</summary>

**B.**

→ [Базовая теория](basics.ru.md)

</details>

## 5. 🟡 Как проверить, что чего-то НЕТ на экране?

- A) `getByText` в try
- B) `queryByText` и `not.toBeInTheDocument`
- C) `findByText`
- D) `getAllByText`

<details><summary>Ответ</summary>

**B.**

→ [Продвинутая теория](advanced.ru.md)

</details>

## 6. 🟡 Найди ошибку

```tsx
render(<User load={load} />);
expect(screen.getByText('Ann')).toBeInTheDocument();
```

<details><summary>Ответ</summary>

Имя приходит позже; нужно `await screen.findByText('Ann')`.

→ [Продвинутая теория](advanced.ru.md)

</details>

## 7. 🟡 Выбери хорошие способы работать с сетевым запросом в тесте

- A) Передать функцию загрузки через props и подменить
- B) Обращаться к настоящему production-серверу
- C) Перехватывать запросы тестовым сервером
- D) Ждать пять секунд

<details><summary>Ответ</summary>

**A, C.**

→ [Продвинутая теория](advanced.ru.md)

</details>

## 8. 🟡 Что делает опция `wrapper`?

- A) Оборачивает тест в try
- B) Рендерит компонент внутри другого, например провайдера
- C) Добавляет div для стилей
- D) Подменяет компонент

<details><summary>Ответ</summary>

**B.**

→ [Продвинутая теория](advanced.ru.md)

</details>

## 9. 🔴 Что будет со вторым тестом без очистки (без globals в Vitest)?

- A) Пройдёт
- B) Найдёт две кнопки и упадёт
- C) Зависнет
- D) Ничего не отрендерится

<details><summary>Ответ</summary>

**B.**

→ [Экспертная теория](expert.ru.md)

</details>

## 10. 🔴 Функцию хука вызвали без `act`; что в `result.current` сразу после?

- A) Новое значение
- B) Старое значение
- C) `undefined`
- D) Бросается ошибка

<details><summary>Ответ</summary>

**B.** Обновление ещё не применено.

→ [Экспертная теория](expert.ru.md)

</details>

## 11. 🔴 `vi.useFakeTimers()` и `await user.click()` зависает. Чего не хватает?

- A) `await render`
- B) `advanceTimers` в `userEvent.setup`
- C) `cleanup`
- D) `act` вокруг render

<details><summary>Ответ</summary>

**B.**

→ [Экспертная теория](expert.ru.md)

</details>

## 12. 🔴 Открытый вопрос: что вы бы проверили в форме поиска с задержкой и результатами?

<details><summary>Ключевые пункты ответа</summary>

- Нет запроса до истечения задержки; один после окончания ввода (поддельные таймеры).
- Состояния загрузки, результатов, пустоты и ошибки.
- Устаревший ответ не заменяет новый.
- Доступные имена поля и сообщений.

→ [Экспертная теория](expert.ru.md)

</details>
