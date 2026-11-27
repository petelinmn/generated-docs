---
title: "Пользовательские хуки: базовая теория"
lang: ru
track: react
kind: module
doc: basics
slug: 09-custom-hooks
part: 2
tags: [react, custom-hooks, hooks, use-sync-external-store, reuse]
versions: "React 19.3"
status: draft
updated: 2026-11-27
---

# Пользовательские хуки: базовая теория

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [обзор](overview.ru.md) · **1. Базовая** · [2. Продвинутая](advanced.ru.md) · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [эффекты](../06-effects/overview.ru.md) · **Время:** ~1,5 ч

Все результаты получены на React 19.3.1; хуки проходят `tsc --strict`.

## Первый свой хук

```tsx
export function useToggle(initial = false) {
  const [on, setOn] = useState(initial);
  const toggle = useCallback(() => setOn(v => !v), []);
  return [on, toggle] as const;
}
```

`as const` сохраняет тип кортежа, поэтому вызывающий деструктурирует с правильными типами.

## Каждый вызов отдельный

```tsx
function Two({ id }: { id: string }) {
  const [on, toggle] = useToggle();
  return <button id={id} onClick={toggle}>{String(on)}</button>;
}
// <Two id="a" /><Two id="b" />, click a:  true false
```

## Правила

- Вызывайте хуки на верхнем уровне: не в условиях, циклах, вложенных функциях и блоках `try` и не после раннего `return`.
- Вызывайте их только из компонентов или других хуков.
- Называйте хук `use` плюс заглавная буква, чтобы его узнавали люди и линтер.

## Ошибки новичка

- ❌ Ждать, что два компонента разделят состояние хука → ✅ поднять его или взять хранилище.
- ❌ Помощник без хуков с именем `useFormat` → ✅ `format`.
- ❌ Вызывать хук в обработчике события → ✅ вызвать в компоненте и использовать результат в обработчике.

## Связанные материалы

- [Продвинутая теория](advanced.ru.md)

## Источники

- [React: повторное использование логики в собственных хуках](https://react.dev/learn/reusing-logic-with-custom-hooks)
- [React: правила хуков](https://react.dev/reference/rules/rules-of-hooks)
- [React: useSyncExternalStore](https://react.dev/reference/react/useSyncExternalStore)
