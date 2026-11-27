---
title: "Actions"
lang: ru
track: react
kind: module
doc: overview
slug: 13-actions
part: 3
levels: [1, 2, 3, 4]
tags: [react, actions, forms, optimistic-updates, transitions]
versions: "React 19.3"
status: draft
updated: 2026-11-26
---

# Actions

🌐 **Русский** · [English](overview.en.md)

## Коротко

Action — это функция, часто асинхронная, выполняемая внутри transition. Форма может принять такую функцию в `action`; тогда React отслеживает состояние ожидания, а хуки помогают хранить результат, показывать ход выполнения и ожидаемый итог до ответа сервера.

## Ключевые идеи

- `<form action={fn}>` вызывает функцию с `FormData` внутри transition.
- После успешного action неуправляемые поля сбрасываются.
- `useActionState` хранит результат и флаг ожидания и выполняет вызовы по очереди.
- `useFormStatus` читает состояние родительской формы.
- `useOptimistic` показывает временное значение, пока выполняется action.
- Ожидаемые ошибки возвращают как состояние; брошенные уходят в error boundary.

## Минимальный пример

```tsx
async function save(data: FormData) {
  await send(String(data.get('name')));
}

<form action={save}>
  <input name="name" />
  <button>Save</button>
</form>
// while save is running the input keeps "Ann"; after it finishes the input is empty
```

## Когда применять, а когда нет

- ✅ Отправка форм, кнопки, меняющие данные на сервере.
- ✅ Мгновенный отклик с безопасным откатом.
- ❌ Живая фильтрация при вводе: это состояние, а не action.
- ❌ Оптимистичные обновления там, где ложный успех вреден, например в платежах.

## Уровни материала

| Уровень | Что внутри |
|---|---|
| [1. Базовая теория](basics.ru.md) | actions форм, `useFormStatus` |
| [2. Продвинутая теория](advanced.ru.md) | `useActionState`, `useOptimistic` |
| [3. Экспертная теория](expert.ru.md) | справочник, подвохи, вопросы с собеседований |
| [4. Самопроверка](quiz.ru.md) | квиз и [упражнения](exercises.ru.md) |
| 5. Запускаемые примеры | ещё не создан — `/learn react 13-actions level 5` |
| 6. Тесты | ещё не создан — `/learn react 13-actions level 6` |

## Связанные материалы

- Пререквизиты: [05 · Формы](../05-forms/overview.ru.md), [12 · Конкурентные возможности](../12-concurrent-features/overview.ru.md)
- Промисы: [асинхронность в JavaScript](../../../javascript/modules/12-promises-and-async-await/overview.ru.md)
- Дальше: следующий модуль по [дорожной карте React](../../overview.ru.md)
- [14 · Серверный рендеринг](../14-server-rendering/overview.ru.md)
