---
title: "Конкурентные возможности: базовая теория"
lang: ru
track: react
kind: module
doc: basics
slug: 12-concurrent-features
part: 3
tags: [react, suspense, transitions, concurrent-rendering, error-boundaries]
versions: "React 19.3"
status: draft
updated: 2026-11-27
---

# Конкурентные возможности: базовая теория

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [обзор](overview.ru.md) · **1. Базовая** · [2. Продвинутая](advanced.ru.md) · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [эффекты](../06-effects/overview.ru.md), промисы · **Время:** ~2 ч

Все результаты получены на React 19.3.1.

## Загрузка кода по требованию

```tsx
const Chart = lazy(() => import('./Chart'));

<Suspense fallback={<p>loading</p>}>
  <Chart />
</Suspense>
// first <p>loading</p>, then the chart
```

`lazy` принимает функцию, возвращающую динамический импорт модуля с экспортом по умолчанию. Пока код загружается, ближайшая граница `Suspense` показывает fallback.

## Где ставить границу

Заменяется всё, что внутри границы:

```tsx
<Suspense fallback={<p>loading</p>}>
  <h1>title</h1>
  <Wait />               {/* suspends */}
</Suspense>
// <p>loading</p>  — the title is hidden too
```

Ставьте границы вокруг частей, которые могут загружаться, а не вокруг всей страницы.

## Ошибки

Error boundary показывает запасной вывод, когда рендер ниже бросает исключение. Её можно написать только классом (или взять из библиотеки).

```tsx
class Boundary extends Component<{ children: ReactNode }, { error: string }> {
  state = { error: '' };
  static getDerivedStateFromError(e: Error) { return { error: e.message }; }
  render() {
    return this.state.error ? <p role="alert">{this.state.error}</p> : this.props.children;
  }
}

<Boundary><Suspense fallback={<p>loading</p>}><Fail /></Suspense></Boundary>
// a rejected promise read by use() renders <p role="alert">no data</p>
```

## Ошибки новичка

- ❌ Одна граница вокруг всего приложения → ✅ границы вокруг разделов.
- ❌ Загружать в эффекте и ждать `Suspense` → ✅ он этого не замечает.
- ❌ Нет error boundary рядом с загрузкой → ✅ добавить: загрузка может не удаться.

## Связанные материалы

- [Продвинутая теория](advanced.ru.md)

## Источники

- [React: Suspense](https://react.dev/reference/react/Suspense)
- [React: useTransition](https://react.dev/reference/react/useTransition)
- [React: useDeferredValue](https://react.dev/reference/react/useDeferredValue)
- [React: use](https://react.dev/reference/react/use)
- [React: error boundaries](https://react.dev/reference/react/Component)
