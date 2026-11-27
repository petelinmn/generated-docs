---
title: "Производительность рендера"
lang: ru
track: react
kind: module
doc: overview
slug: 11-performance
part: 3
levels: [1, 2, 3, 4]
tags: [react, performance, memoization, react-compiler, profiling]
versions: "React 19.3"
status: draft
updated: 2026-11-26
---

# Производительность рендера

🌐 **Русский** · [English](overview.en.md)

## Коротко

React рендерит компонент заново, когда меняется его состояние, когда рендерится родитель или меняется context, который он читает. Обычно это достаточно дёшево. Когда нет, сначала находят, что именно медленно, а затем рендерят меньше: опускают состояние ближе к месту использования, передают готовые элементы, мемоизируют или поручают мемоизацию React Compiler при сборке.

## Ключевые идеи

- Измеряйте, прежде чем оптимизировать.
- Рендер родителя рендерит дочерние компоненты, если ничто не позволяет React их пропустить.
- Состояние рядом с пользователями затрагивает меньше компонентов.
- `memo` сравнивает props по ссылке.
- `useMemo` и `useCallback` сохраняют значения и функции между рендерами.
- Мемоизация — это оптимизация, а не часть корректности.
- React Compiler мемоизирует автоматически при сборке.

## Минимальный пример

```tsx
// state at the top: typing re-renders Expensive
function Up() {
  const [t, setT] = useState('');
  return <><button onClick={() => setT(t + 'x')}>{t}</button><Expensive /></>;
}

// state moved into the field: Expensive is not rendered again
function Field() {
  const [t, setT] = useState('');
  return <button onClick={() => setT(t + 'x')}>{t}</button>;
}
function Down() { return <><Field /><Expensive /></>; }
```

## Когда применять, а когда нет

- ✅ Измеренное медленное взаимодействие.
- ✅ Тяжёлые компоненты или длинные списки, рендерящиеся часто.
- ❌ Заранее оборачивать всё в `memo`.
- ❌ Полагаться на мемоизацию ради корректности кода.

## Уровни материала

| Уровень | Что внутри |
|---|---|
| [1. Базовая теория](basics.ru.md) | почему компоненты рендерятся, как опустить состояние |
| [2. Продвинутая теория](advanced.ru.md) | `memo`, `useMemo`, `useCallback`, измерение |
| [3. Экспертная теория](expert.ru.md) | React Compiler, справочник, подвохи, вопросы с собеседований |
| [4. Самопроверка](quiz.ru.md) | квиз и [упражнения](exercises.ru.md) |
| 5. Запускаемые примеры | ещё не создан — `/learn react 11-performance level 5` |
| 6. Тесты | ещё не создан — `/learn react 11-performance level 6` |

## Связанные материалы

- Пререквизиты: [02 · Props и композиция](../02-props-and-composition/overview.ru.md), [04 · Списки и ключи](../04-lists-and-keys/overview.ru.md), [08 · Context](../08-context-and-reducers/overview.ru.md)
- Измерения и перцентили: [процесс проектирования](../../../system-design/modules/01-design-process/overview.ru.md)
- Главный поток и долгие задачи: [event loop в JavaScript](../../../javascript/modules/13-event-loop-and-concurrency/overview.ru.md)
- Дальше: следующий модуль по [дорожной карте React](../../overview.ru.md)
- [12 · Конкурентные возможности](../12-concurrent-features/overview.ru.md)
