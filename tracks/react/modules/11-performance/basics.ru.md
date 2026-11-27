---
title: "Производительность рендера: базовая теория"
lang: ru
track: react
kind: module
doc: basics
slug: 11-performance
part: 3
tags: [react, performance, memoization, react-compiler, profiling]
versions: "React 19.3"
status: draft
updated: 2026-11-27
---

# Производительность рендера: базовая теория

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [обзор](overview.ru.md) · **1. Базовая** · [2. Продвинутая](advanced.ru.md) · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [props и композиция](../02-props-and-composition/overview.ru.md), [состояние](../03-state-and-events/overview.ru.md) · **Время:** ~2 ч

Все результаты получены на React 19.3.1.

## Почему компонент рендерится

- изменилось его состояние;
- отрендерился родитель;
- изменился context, который он читает.

Рендер — это вызов функции. DOM меняется только там, где отличается результат, но функция и её дочерние компоненты всё равно выполняются.

## Опустить состояние

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

После одного клика в `Up` компонент `Expensive` выполняется ещё раз; в `Down` — не выполняется вовсе.

## Передавать элементы, а не создавать их

Обёртка с состоянием, получающая `children`, не рендерит их заново: см. [композицию](../02-props-and-composition/advanced.ru.md).

## Ошибки новичка

- ❌ Оптимизировать без измерений → ✅ сначала найти медленное взаимодействие.
- ❌ Всё состояние в верхнем компоненте → ✅ держать там, где используется.
- ❌ Думать, что рендер всегда меняет DOM.

## Связанные материалы

- [Продвинутая теория](advanced.ru.md)

## Источники

- [React: memo](https://react.dev/reference/react/memo)
- [React: useMemo](https://react.dev/reference/react/useMemo)
- [React: useCallback](https://react.dev/reference/react/useCallback)
- [React Compiler: введение](https://react.dev/learn/react-compiler/introduction)
