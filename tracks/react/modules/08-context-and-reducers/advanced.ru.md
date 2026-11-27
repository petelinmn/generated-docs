---
title: "Context и редьюсеры: продвинутая теория"
lang: ru
track: react
kind: module
doc: advanced
slug: 08-context-and-reducers
part: 2
tags: [react, context, use-reducer, state-management, hooks]
versions: "React 19.3"
status: draft
updated: 2026-11-27
---

# Context и редьюсеры: продвинутая теория

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · **2. Продвинутая** · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

## Ленивое начальное состояние

```tsx
useReducer(reducer, 5, n => ({ count: n * 2 }));   // the third argument runs once
```

## Редьюсер вместе с context

Разложите состояние и `dispatch` по двум context:

```tsx
const TasksContext = createContext<Task[] | null>(null);
const DispatchContext = createContext<Dispatch<Action> | null>(null);

function TasksProvider({ children }: { children: ReactNode }) {
  const [tasks, dispatch] = useReducer(tasksReducer, []);
  return (
    <TasksContext value={tasks}>
      <DispatchContext value={dispatch}>{children}</DispatchContext>
    </TasksContext>
  );
}
```

Компоненты, только отправляющие actions, читают второй. Поскольку `dispatch` не меняется, из-за изменений состояния они не рендерятся.

## Безопасный собственный хук

```tsx
const Auth = createContext<{ user: string } | null>(null);

function useAuth() {
  const value = useContext(Auth);
  if (!value) throw new Error('useAuth must be used inside AuthProvider');
  return value;
}
```

## Повторные рендеры

```tsx
<User value={{ name: 'Ann' }}>            {/* a new object on every render */}
const value = useMemo(() => ({ name: 'Ann' }), []);
<User value={value}>                      {/* the same object */}
```

```text
two re-renders of the provider's owner:
new object every time:  consumer 2, memo consumer 2, memo component without context 0
memoized value:         consumer 0, memo consumer 0, memo component without context 0
```

React сравнивает значения context через `Object.is`. Все потребители изменившегося context рендерятся заново, и `memo` этому не мешает. Мемоизированное значение оставляет потребителей в покое, когда по сути ничего не изменилось.

Компоненты, переданные компоненту-провайдеру через `children`, не рендерятся из-за его состояния, если сами не читают context.

## Лучшие практики

- Называйте actions по тому, что произошло (`added`), а не что установить.
- Один редьюсер на область состояния.
- Разделяйте context по частоте изменения значений.
- Экспортируйте провайдер и хуки, а не объект context.

## Частые ошибки

- ❌ Литерал объекта в качестве значения → ✅ `useMemo`.
- ❌ Быстро меняющиеся значения (позиция мыши) в context → ✅ держать локально или использовать внешнее хранилище.
- ❌ Редьюсер, молча возвращающий состояние по умолчанию → ✅ проверка полноты в TypeScript или исключение.

## Связанные материалы

- [Базовая теория](basics.ru.md)
- [Экспертная теория](expert.ru.md)

## Источники

- [React: useReducer](https://react.dev/reference/react/useReducer)
- [React: createContext](https://react.dev/reference/react/createContext)
- [React: useContext](https://react.dev/reference/react/useContext)
- [React: передача данных вглубь через context](https://react.dev/learn/passing-data-deeply-with-context)
