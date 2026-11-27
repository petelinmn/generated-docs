---
title: "Пользовательские хуки: продвинутая теория"
lang: ru
track: react
kind: module
doc: advanced
slug: 09-custom-hooks
part: 2
tags: [react, custom-hooks, hooks, use-sync-external-store, reuse]
versions: "React 19.3"
status: draft
updated: 2026-11-27
---

# Пользовательские хуки: продвинутая теория

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · **2. Продвинутая** · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

## Хук с эффектом

```tsx
export function useDebounced<T>(value: T, ms: number): T {
  const [debounced, setDebounced] = useState(value);
  useEffect(() => {
    const id = setTimeout(() => setDebounced(value), ms);
    return () => clearTimeout(id);
  }, [value, ms]);
  return debounced;
}
// value goes a, ab, abc quickly:  immediately "abc:a", after the delay "abc:abc"
```

Очистка при каждом изменении отменяет прежний таймер.

## Сохранение в хранилище

```tsx
export function useStoredState(key: string, initial: string) {
  const [value, setValue] = useState(() => {
    try { return localStorage.getItem(key) ?? initial; } catch { return initial; }
  });
  useEffect(() => {
    try { localStorage.setItem(key, value); } catch { /* storage may be unavailable */ }
  }, [key, value]);
  return [value, setValue] as const;
}
// first render shows "guest" and stores it; after set('Ann') a remounted component shows "Ann"
```

Хранилище может быть недоступно, поэтому чтение и запись обёрнуты. На сервере, где нет `localStorage`, хук отрендерит `initial`, что может отличаться от того, что позже покажет браузер.

## Подписка на внешнее

```tsx
function subscribe(callback: () => void) {             // outside the component: stable
  window.addEventListener('online', callback);
  window.addEventListener('offline', callback);
  return () => {
    window.removeEventListener('online', callback);
    window.removeEventListener('offline', callback);
  };
}

export function useOnline() {
  return useSyncExternalStore(subscribe, () => navigator.onLine, () => true);
}
// server render uses the third argument and prints "online"
```

Для этого и сделан `useSyncExternalStore`. Он же позволяет нескольким компонентам делить хранилище:

```tsx
function useCount() { return useSyncExternalStore(store.subscribe, store.get); }
// two components using useCount(), click one:  both show 1
```

## Проектирование хука

| Вопрос | Ориентир |
|---|---|
| Что возвращать | пару для значений вроде состояния, объект для нескольких именованных вещей |
| Возвращаемые функции | оборачивать в `useCallback`, если их могут класть в зависимости |
| Параметры | принимать значения, а не ref, если ref не нужен |
| Имя | что даёт (`useOnline`), а не когда выполняется |

## Лучшие практики

- Один хук — одна забота.
- Собирайте крупные хуки из мелких.
- Тестируйте хуки через использующий их компонент.

## Частые ошибки

- ❌ `useMount(fn)` → ✅ использовать `useEffect` напрямую и назвать назначение.
- ❌ Возвращать каждый раз новый объект или функцию → ✅ мемоизировать то, от чего зависят.
- ❌ Спрятать запрос в хук без обработки гонок → ✅ как в [эффектах](../06-effects/advanced.ru.md).

## Связанные материалы

- [Базовая теория](basics.ru.md)
- [Экспертная теория](expert.ru.md)

## Источники

- [React: повторное использование логики в собственных хуках](https://react.dev/learn/reusing-logic-with-custom-hooks)
- [React: правила хуков](https://react.dev/reference/rules/rules-of-hooks)
- [React: useSyncExternalStore](https://react.dev/reference/react/useSyncExternalStore)
