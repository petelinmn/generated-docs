---
title: "Компоненты и JSX"
lang: ru
track: react
kind: module
doc: overview
slug: 01-components-and-jsx
part: 1
levels: [1, 2, 3, 4]
tags: [react, jsx, components, rendering]
versions: "React 19.3"
status: draft
updated: 2026-11-27
---

# Компоненты и JSX

🌐 **Русский** · [English](overview.en.md)

## Коротко

React строит пользовательский интерфейс из компонентов — функций, которые получают данные и возвращают описание того, что должно быть на экране. Описание пишут на JSX: синтаксисе, похожем на HTML внутри JavaScript. Когда данные меняются, React снова вызывает компоненты и обновляет только те части страницы, которые отличаются. Этот модуль — о компонентах и JSX, основе всего остального.

## Ключевые идеи

- Компонент — функция с именем с заглавной буквы, возвращающая JSX.
- JSX строже HTML: один корень, все теги закрыты, атрибуты в camelCase.
- Фигурные скобки вставляют выражения JavaScript.
- `false`, `null`, `undefined` и `true` ничего не выводят; `0` и `NaN` выводятся текстом.
- Вставляемые строки экранируются, поэтому ввод пользователя не становится разметкой.
- Рендер должен быть чистым: один и тот же вход даёт один и тот же выход.
- React сначала рендерит (вызывает компоненты), затем фиксирует (меняет DOM).

## Минимальный пример

```tsx
import { renderToStaticMarkup } from 'react-dom/server';

function Greeting() {
  const name = 'Ann';
  return <h1 className="title">Hello, {name}!</h1>;
}

console.log(renderToStaticMarkup(<Greeting />));
// <h1 class="title">Hello, Ann!</h1>
```

## Когда применять, а когда нет

- ✅ Интерфейсы с меняющимися данными и повторно используемыми частями.
- ✅ Растущие приложения: компоненты держат их управляемыми.
- ❌ Статичная страница без взаимодействия: достаточно HTML.
- ❌ Один небольшой скрипт на готовой странице: сборка может не окупиться.

## Уровни материала

| Уровень | Что внутри |
|---|---|
| [1. Базовая теория](basics.ru.md) | создание проекта, компоненты, правила JSX, списки и условия |
| [2. Продвинутая теория](advanced.ru.md) | что можно выводить, динамические компоненты, экранирование, чистота |
| [3. Экспертная теория](expert.ru.md) | элементы изнутри, рендер и фиксация, справочник, подвохи, вопросы с собеседований |
| [4. Самопроверка](quiz.ru.md) | квиз и [упражнения](exercises.ru.md) |
| 5. Запускаемые примеры | ещё не создан — `/learn react 01-components-and-jsx level 5` |
| 6. Тесты | ещё не создан — `/learn react 01-components-and-jsx level 6` |

## Связанные материалы

- Пререквизиты: [функции в JavaScript](../../../javascript/modules/04-functions/overview.ru.md), [модули](../../../javascript/modules/09-modules/overview.ru.md), [массивы](../../../javascript/modules/07-arrays-and-collections/overview.ru.md)
- Типы для кода на React: [трек TypeScript](../../../typescript/overview.ru.md)
- DOM, которым управляет React: [DOM и события в браузере](../../../javascript/modules/17-browser-dom-and-events/overview.ru.md)
- Дальше: следующий модуль по [дорожной карте React](../../overview.ru.md)
