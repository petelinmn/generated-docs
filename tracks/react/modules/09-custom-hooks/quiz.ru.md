---
title: "Квиз: пользовательские хуки"
lang: ru
track: react
kind: module
doc: quiz
slug: 09-custom-hooks
part: 2
tags: [react, custom-hooks, hooks, use-sync-external-store, reuse]
versions: "React 19.3"
status: draft
updated: 2026-11-27
---

# Квиз: пользовательские хуки

🌐 **Русский** · [English](quiz.en.md)

> Сначала ответь сам, потом раскрывай ответ. Отметка показывает уровень: 🟢 [базовая теория](basics.ru.md) · 🟡 [продвинутая](advanced.ru.md) · 🔴 [экспертная](expert.ru.md).

## 1. 🟢 Что делает функцию пользовательским хуком?

- A) Возвращает JSX
- B) Имя на `use` и вызовы хуков
- C) Она экспортирована
- D) Она async

<details><summary>Ответ</summary>

**B.**

→ [Базовая теория](basics.ru.md)

</details>

## 2. 🟢 Два компонента вызывают `useToggle()`. Их состояние

- A) Общее
- B) Раздельное
- C) Общее до размонтирования
- D) Хранится глобально

<details><summary>Ответ</summary>

**B.**

→ [Базовая теория](basics.ru.md)

</details>

## 3. 🟢 Откуда можно вызывать хук?

- A) В обработчике события
- B) С верхнего уровня компонента или хука
- C) Внутри `if`
- D) В цикле

<details><summary>Ответ</summary>

**B.**

→ [Базовая теория](basics.ru.md)

</details>

## 4. 🟢 Функция форматирует дату и не вызывает хуки. Как её назвать?

- A) `useFormatDate`
- B) `formatDate`
- C) `UseDate`
- D) `hookDate`

<details><summary>Ответ</summary>

**B.**

→ [Базовая теория](basics.ru.md)

</details>

## 5. 🟡 `useDebounced(value, 20)`: значение сразу меняется a, ab, abc. Что показано немедленно?

- A) `abc:abc`
- B) `abc:a`
- C) `a:a`
- D) `ab:ab`

<details><summary>Ответ</summary>

**B.**

→ [Продвинутая теория](advanced.ru.md)

</details>

## 6. 🟡 Зачем оборачивать возвращаемую функцию в `useCallback`?

- A) Это обязательно
- B) Чтобы она сохраняла идентичность для зависящих от неё
- C) Ради скорости
- D) Ради общего состояния

<details><summary>Ответ</summary>

**B.**

→ [Продвинутая теория](advanced.ru.md)

</details>

## 7. 🟡 Найди ошибку

```tsx
function useThing(on) {
  if (on) {
    const [v] = useState(0);
    return v;
  }
  return null;
}
```

<details><summary>Ответ</summary>

Хук вызывается по условию; вызовите `useState` наверху и ветвитесь после.

→ [Продвинутая теория](advanced.ru.md)

</details>

## 8. 🟡 Выбери подходящих кандидатов в пользовательский хук

- A) Подписка на размер окна
- B) Чистый расчёт цены
- C) Синхронизация значения с хранилищем
- D) Вывод кнопки

<details><summary>Ответ</summary>

**A, C.**

→ [Продвинутая теория](advanced.ru.md)

</details>

## 9. 🔴 Что будет, если `getSnapshot` каждый раз возвращает новый объект?

- A) Ничего
- B) Ошибка бесконечного цикла
- C) Устаревшее значение
- D) Один лишний рендер

<details><summary>Ответ</summary>

**B.**

→ [Экспертная теория](expert.ru.md)

</details>

## 10. 🔴 Что будет без `getServerSnapshot` при серверном рендере?

- A) Возьмётся `getSnapshot`
- B) Ошибка
- C) Подставится `undefined`
- D) Молча ничего не выведется

<details><summary>Ответ</summary>

**B.**

→ [Экспертная теория](expert.ru.md)

</details>

## 11. 🔴 Хук возвращает `{ value: 1 }`, и эффект зависит от него. Три рендера дадут

- A) 1 запуск эффекта
- B) 3 запуска
- C) 0
- D) 2

<details><summary>Ответ</summary>

**B.**

→ [Экспертная теория](expert.ru.md)

</details>

## 12. 🔴 Открытый вопрос: как разделить число непрочитанных сообщений между шапкой и страницей?

<details><summary>Ключевые пункты ответа</summary>

- Один только пользовательский хук дал бы каждому свою копию.
- Поднять состояние в общего родителя, взять context или внешнее хранилище через `useSyncExternalStore`.
- Для серверных данных — библиотеку с общим кэшем.

→ [Экспертная теория](expert.ru.md)

</details>
