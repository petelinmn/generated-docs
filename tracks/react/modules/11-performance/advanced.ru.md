---
title: "Производительность рендера: продвинутая теория"
lang: ru
track: react
kind: module
doc: advanced
slug: 11-performance
part: 3
tags: [react, performance, memoization, react-compiler, profiling]
versions: "React 19.3"
status: draft
updated: 2026-11-27
---

# Производительность рендера: продвинутая теория

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · **2. Продвинутая** · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

## memo

`memo(Component)` пропускает рендер, когда все props равны по `Object.is`. Помогает только если props действительно не меняются:

```tsx
function Parent() {
  const [n, setN] = useState(0);
  const style = useMemo(() => ({ color: 'red' }), []);
  const on = useCallback(() => {}, []);
  return <>
    <button onClick={() => setN(n + 1)}>{n}</button>
    <Plain />                                  {/* renders */}
    <Memo n={1} />                             {/* skipped */}
    <MemoObj style={{ color: 'red' }} />       {/* renders: new object */}
    <MemoFn on={() => {}} />                   {/* renders: new function */}
    <MemoStable style={style} on={on} />       {/* skipped */}
    <MemoChildren><b /></MemoChildren>         {/* renders: new element */}
  </>;
}
```

## useMemo и useCallback

`useMemo` сохраняет вычисленное значение, `useCallback` — функцию; оба пересчитываются при изменении зависимости.

```tsx
const total = useMemo(() => list.reduce((a, b) => a + b, 0), [list]);
// render with the same list array and another prop: not recomputed
// render with a new array with the same numbers:    recomputed
```

Их применяют, чтобы props мемоизированных компонентов оставались прежними, или чтобы не повторять дорогое вычисление. Это оптимизации: в отдельных случаях React может сбросить кэш, поэтому код должен работать и без них.

## Измерение

- Профилировщик React DevTools показывает, что рендерилось и почему.
- `<Profiler>` сообщает время из кода:

```tsx
<Profiler id="app" onRender={(id, phase, actualDuration, baseDuration, startTime, commitTime) => report(id, phase, actualDuration)}>
  <App />
</Profiler>
// phase: 'mount', 'update' or 'nested-update'
```

В production-сборке он по умолчанию отключён. Сборки для разработки также добавляют дорожки React на панель Performance браузера. Настоящие цифры измеряйте на production-сборке.

## Длинные списки

Рендер тысяч строк медленен даже с мемоизацией. Виртуализированный список рендерит только видимые.

## Лучшие практики

- Сначала структура, потом мемоизация.
- Давайте устойчивые ключи.
- Производные данные вычисляйте при рендере и мемоизируйте только дорогое.

## Частые ошибки

- ❌ `memo` на компоненте, чьи props каждый раз новые → ✅ стабилизировать их или убрать `memo`.
- ❌ Свой компаратор, пропускающий prop → устаревший вывод.
- ❌ Судить о скорости по сборке для разработки.

## Связанные материалы

- [Базовая теория](basics.ru.md)
- [Экспертная теория](expert.ru.md)

## Источники

- [React: memo](https://react.dev/reference/react/memo)
- [React: useMemo](https://react.dev/reference/react/useMemo)
- [React: useCallback](https://react.dev/reference/react/useCallback)
- [React Compiler: введение](https://react.dev/learn/react-compiler/introduction)
- [React Compiler 1.0](https://react.dev/blog/2025/10/07/react-compiler-1)
- [React: Profiler](https://react.dev/reference/react/Profiler)
- [React: Performance tracks](https://react.dev/reference/dev-tools/react-performance-tracks)
