---
title: "Эффекты: базовая теория"
lang: ru
track: react
kind: module
doc: basics
slug: 06-effects
part: 2
tags: [react, effects, use-effect, hooks, cleanup]
versions: "React 19.3"
status: draft
updated: 2026-11-27
---

# Эффекты: базовая теория

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [обзор](overview.ru.md) · **1. Базовая** · [2. Продвинутая](advanced.ru.md) · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [состояние и события](../03-state-and-events/overview.ru.md) · **Время:** ~2 ч

Все результаты получены на React 19.3.1.

## Три части эффекта

```tsx
useEffect(() => {
  const connection = createConnection(room);
  connection.connect();
  return () => connection.disconnect();      // cleanup
}, [room]);                                   // run again when room changes
```

| Часть | Смысл |
|---|---|
| установка (setup) | что запустить |
| очистка (cleanup) | как это остановить |
| зависимости | когда начать заново |

Эффект выполняется после фиксации и только на клиенте: при серверном рендере он не запускается.

## Зависимости

| Записано | Запускается |
|---|---|
| без массива | после каждой фиксации |
| `[]` | после первой |
| `[a, b]` | когда `a` или `b` отличаются (`Object.is`) |

Каждое значение из компонента, которое читает эффект, должно быть в массиве.

## Очистка

Перед повторным запуском эффекта React вызывает прежнюю очистку со старыми значениями; вызывает он её и при удалении компонента.

## Двойной запуск при разработке

```text
<StrictMode> in development, an effect with [] that connects and disconnects:
connect | disconnect | connect
```

Так проверяется, что очистка действительно отменяет установку. Не подавляйте это, а исправьте очистку.

## Ошибки новичка

- ❌ Подписка без очистки → ✅ вернуть функцию отписки.
- ❌ Не указать используемое значение в массиве → ✅ добавить или избавиться от нужды в нём.
- ❌ `useEffect(async () => ...)` → ✅ объявить async-функцию внутри и вызвать.
- ❌ Бороться с двойным запуском флагом → ✅ сделать установку и очистку симметричными.

## Связанные материалы

- [Продвинутая теория](advanced.ru.md)

## Источники

- [React: useEffect](https://react.dev/reference/react/useEffect)
- [React: возможно, эффект не нужен](https://react.dev/learn/you-might-not-need-an-effect)
- [React: useLayoutEffect](https://react.dev/reference/react/useLayoutEffect)
