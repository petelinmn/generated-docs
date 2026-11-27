---
title: "Context и редьюсеры"
lang: ru
track: react
kind: module
doc: overview
slug: 08-context-and-reducers
part: 2
levels: [1, 2, 3, 4]
tags: [react, context, use-reducer, state-management, hooks]
versions: "React 19.3"
status: draft
updated: 2026-11-26
---

# Context и редьюсеры

🌐 **Русский** · [English](overview.en.md)

## Коротко

Редьюсер (reducer) переносит правила изменения состояния в одну функцию: компоненты сообщают, что произошло, а функция решает, каким станет состояние. Context делает значение доступным целому поддереву без передачи через каждый уровень. Их часто используют вместе, но задачи у них разные: один организует обновления, другой доставляет данные.

## Ключевые идеи

- Редьюсер получает состояние и action и возвращает следующее состояние.
- Он должен быть чистым и ничего не мутировать.
- `dispatch` сохраняет идентичность между рендерами.
- Потребитель читает ближайший провайдер выше себя, а без него — значение по умолчанию.
- Все потребители рендерятся заново, когда меняется переданное значение.
- Новый объект в качестве значения считается изменением.
- Прежде чем брать context, попробуйте props и `children`.

## Минимальный пример

```tsx
type State = { count: number };
type Action = { type: 'inc' } | { type: 'add'; by: number };

function reducer(state: State, action: Action): State {
  switch (action.type) {
    case 'inc': return { count: state.count + 1 };
    case 'add': return { count: state.count + action.by };
  }
}

function Counter() {
  const [state, dispatch] = useReducer(reducer, { count: 0 });
  return <button onClick={() => dispatch({ type: 'inc' })}>{state.count}</button>;
}
```

## Когда применять, а когда нет

- ✅ Редьюсер: несколько связанных значений, много видов обновлений.
- ✅ Context: тема, текущий пользователь, язык, общий dispatch.
- ❌ Context для данных на один уровень ниже: передайте prop.
- ❌ Один огромный context со всем: каждое изменение перерендерит всех читателей.

## Уровни материала

| Уровень | Что внутри |
|---|---|
| [1. Базовая теория](basics.ru.md) | `useReducer`, `createContext`, `useContext` |
| [2. Продвинутая теория](advanced.ru.md) | совместное использование, повторные рендеры, разделение context |
| [3. Экспертная теория](expert.ru.md) | `use`, справочник, подвохи, вопросы с собеседований |
| [4. Самопроверка](quiz.ru.md) | квиз и [упражнения](exercises.ru.md) |
| 5. Запускаемые примеры | ещё не создан — `/learn react 08-context-and-reducers level 5` |
| 6. Тесты | ещё не создан — `/learn react 08-context-and-reducers level 6` |

## Связанные материалы

- Пререквизиты: [03 · Состояние и события](../03-state-and-events/overview.ru.md), [02 · Props и композиция](../02-props-and-composition/overview.ru.md)
- Та же идея «события дают состояние»: [event sourcing и CQRS](../../../system-design/modules/13-event-sourcing-and-cqrs/overview.ru.md)
- Размеченные объединения для actions: [трек TypeScript](../../../typescript/overview.ru.md)
- Дальше: следующий модуль по [дорожной карте React](../../overview.ru.md)
- [09 · Пользовательские хуки](../09-custom-hooks/overview.ru.md)
- [11 · Производительность рендера](../11-performance/overview.ru.md)
