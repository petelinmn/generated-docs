---
title: "React"
lang: ru
track: react
kind: track
doc: overview
slug: react
tags: [react, jsx, components, hooks, frontend]
versions: "React 19.3"
default_level: 4
status: draft
updated: 2026-11-27
---

# React

🌐 **Русский** · [English](overview.en.md)

## Коротко

React — библиотека JavaScript для построения пользовательских интерфейсов из компонентов. Компонент — функция, возвращающая описание экрана; когда его данные меняются, React вызывает его снова и обновляет на странице то, что отличается. Его применяют для веб-приложений, обычно вместе с фреймворком вроде Next.js или React Router, и для мобильных приложений через React Native.

**Пререквизиты:** [JavaScript](../javascript/overview.ru.md): функции, массивы, объекты, модули, промисы. В типизированных примерах используется TypeScript.

## Окружение

| Что | Инструмент |
|---|---|
| Runtime | Node.js — для сборки и тестов |
| Проект | Vite: `npm create vite@latest my-app -- --template react-ts` |
| Библиотека | React 19.3 |
| Редактор | VS Code с поддержкой TypeScript |
| Код модуля | на уровнях 5–6: `code/` с `package.json`, `examples/exNN-name.tsx`, `solutions/`, `tests/*.test.tsx` |
| Запустить все тесты модуля | `npm test` (`vitest run`) в папке `code/`, с Testing Library |
| Проверить типы | `npx tsc --noEmit` |

## Дорожная карта

Прогресс изучения — в [PROGRESS.md](../../PROGRESS.md). Модули по умолчанию генерируются с уровнем детализации **4** (теория и самопроверка).

### Часть 1. Основы

- [01 · Компоненты и JSX](modules/01-components-and-jsx/overview.ru.md) — компоненты, правила JSX, чистота, рендер и фиксация
- [02 · Props и композиция](modules/02-props-and-composition/overview.ru.md) — props, значения по умолчанию, `children`, композиция
- [03 · Состояние и события](modules/03-state-and-events/overview.ru.md) — `useState`, снимки, пакетные обновления, события
- [04 · Списки, ключи и идентичность](modules/04-lists-and-keys/overview.ru.md) — `key`, сохранение и сброс состояния
- [05 · Формы](modules/05-forms/overview.ru.md) — управляемые и неуправляемые поля

### Часть 2. Продвинутые темы

- [06 · Эффекты](modules/06-effects/overview.ru.md) — `useEffect`, очистка, когда эффект не нужен
- [07 · Refs и DOM](modules/07-refs-and-dom/overview.ru.md) — `useRef`, фокус, измерения, порталы
- [08 · Context и редьюсеры](modules/08-context-and-reducers/overview.ru.md) — `useReducer`, `createContext`
- [09 · Пользовательские хуки](modules/09-custom-hooks/overview.ru.md) — правила хуков, повторное использование логики
- [10 · Тестирование](modules/10-testing/overview.ru.md) — Vitest и Testing Library

### Часть 3. Темы для опытных

- [11 · Производительность рендера](modules/11-performance/overview.ru.md) — профилирование, мемоизация, React Compiler
- [12 · Конкурентные возможности](modules/12-concurrent-features/overview.ru.md) — transitions, отложенные значения, Suspense
- [13 · Actions](modules/13-actions/overview.ru.md) — асинхронные transitions, actions форм, оптимистичные обновления
- [14 · Серверный рендеринг](modules/14-server-rendering/overview.ru.md) — SSR, гидратация, Server Components

## Библиотеки

- [React Router](libraries/react-router/overview.ru.md) — маршрутизация
- [TanStack Query](libraries/tanstack-query/overview.ru.md) — серверные данные и кэширование
- [Zustand](libraries/zustand/overview.ru.md) — небольшое хранилище состояния

## Рецепты

- [Загрузка данных](recipes/data-fetching/overview.ru.md) — загрузка, ошибки, отмена
- [Проверка форм](recipes/form-validation/overview.ru.md) — ошибки клиента и сервера

## Проекты

- [Доска задач](projects/task-board/overview.ru.md) — состояние, формы, маршруты, тесты

## Связанные треки

- [JavaScript](../javascript/overview.ru.md) — язык, на котором пишут на React
- [TypeScript](../typescript/overview.ru.md) — типы для props и хуков
- [C# / .NET](../csharp/overview.ru.md) — типичный backend для приложения

## Источники

- [Документация React: learn](https://react.dev/learn)
- [Версии React](https://react.dev/versions)
- [React: создание приложения](https://react.dev/learn/creating-a-react-app)
