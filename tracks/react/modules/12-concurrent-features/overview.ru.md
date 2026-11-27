---
title: "Конкурентные возможности"
lang: ru
track: react
kind: module
doc: overview
slug: 12-concurrent-features
part: 3
levels: [1, 2, 3, 4]
tags: [react, suspense, transitions, concurrent-rendering, error-boundaries]
versions: "React 19.3"
status: draft
updated: 2026-11-26
---

# Конкурентные возможности

🌐 **Русский** · [English](overview.en.md)

## Коротко

Рендер в React можно прерывать и откладывать. Это позволяет сохранять отзывчивость интерфейса: пометить обновление как transition, чтобы срочные шли первыми, показывать заглушку через `Suspense`, пока что-то не готово, и держать на экране прежнее содержимое, пока готовится новое.

## Ключевые идеи

- `Suspense` показывает fallback, пока что-то внутри не готово.
- `lazy` загружает код компонента по требованию; `use` читает кэшированный промис.
- Transition помечает обновление как несрочное и прерываемое.
- Transition сохраняет уже показанное содержимое вместо замены на fallback.
- `useDeferredValue` позволяет медленной части экрана отставать.
- Ошибки рендера ловят error boundaries.
- Данные, загружаемые в эффекте, `Suspense` не активируют.

## Минимальный пример

```tsx
const Chart = lazy(() => import('./Chart'));

<Suspense fallback={<p>loading</p>}>
  <Chart />
</Suspense>
// first <p>loading</p>, then the chart
```

## Когда применять, а когда нет

- ✅ Разделение кода, переключение видов с загрузкой данных, медленные списки при вводе.
- ✅ Сохранить состояние скрытой панели через `Activity`.
- ❌ Transition для собственного значения управляемого поля.
- ❌ Ждать, что эти средства ускорят медленный код: они меняют порядок и приоритет.

## Уровни материала

| Уровень | Что внутри |
|---|---|
| [1. Базовая теория](basics.ru.md) | `Suspense`, `lazy`, error boundaries |
| [2. Продвинутая теория](advanced.ru.md) | transitions, отложенные значения, `use` |
| [3. Экспертная теория](expert.ru.md) | `Activity`, справочник, подвохи, вопросы с собеседований |
| [4. Самопроверка](quiz.ru.md) | квиз и [упражнения](exercises.ru.md) |
| 5. Запускаемые примеры | ещё не создан — `/learn react 12-concurrent-features level 5` |
| 6. Тесты | ещё не создан — `/learn react 12-concurrent-features level 6` |

## Связанные материалы

- Пререквизиты: [06 · Эффекты](../06-effects/overview.ru.md), [11 · Производительность рендера](../11-performance/overview.ru.md)
- Промисы: [асинхронность в JavaScript](../../../javascript/modules/12-promises-and-async-await/overview.ru.md)
- Динамический импорт: [модули JavaScript](../../../javascript/modules/09-modules/overview.ru.md)
- Дальше: следующий модуль по [дорожной карте React](../../overview.ru.md)
- [13 · Actions](../13-actions/overview.ru.md)
- [14 · Серверный рендеринг](../14-server-rendering/overview.ru.md)
