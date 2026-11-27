---
title: "Упражнения: пользовательские хуки"
lang: ru
track: react
kind: module
doc: exercises
slug: 09-custom-hooks
part: 2
tags: [react, custom-hooks, hooks, use-sync-external-store, reuse]
versions: "React 19.3"
status: draft
updated: 2026-11-27
---

# Упражнения: пользовательские хуки

🌐 **Русский** · [English](exercises.en.md)

> Решай сам, прежде чем открывать подсказки.

## 1. 🟢 Хук счётчика

**Задача:** вернуть `count`, `increment` и `reset`; функции должны сохранять идентичность.

<details><summary>Подсказка</summary>

`useCallback` и функция-обновление.

</details>

<details><summary>Решение</summary>

```tsx
function useCounter(initial = 0) {
  const [count, setCount] = useState(initial);
  const increment = useCallback(() => setCount(c => c + 1), []);
  const reset = useCallback(() => setCount(initial), [initial]);
  return { count, increment, reset };
}
```

</details>

## 2. 🟡 useInterval

**Задача:** вызывать свежий callback каждые `ms` миллисекунд; `null` ставит на паузу. Смена callback не должна перезапускать таймер.

<details><summary>Подсказка</summary>

Для части «свежий callback» в React 19.2 есть хук.

</details>

<details><summary>Решение</summary>

```tsx
function useInterval(callback: () => void, ms: number | null) {
  const onTick = useEffectEvent(callback);
  useEffect(() => {
    if (ms === null) return;
    const id = setInterval(() => onTick(), ms);
    return () => clearInterval(id);
  }, [ms]);
}
```

</details>

## 3. 🔴 useMediaQuery

**Задача:** сообщать, совпадает ли CSS media query, и следить за изменениями.

<details><summary>Подсказка</summary>

Внешнее хранилище: `window.matchMedia` и `useSyncExternalStore`.

</details>

<details><summary>Решение</summary>

```tsx
function subscribeQuery(query: string) {
  return (callback: () => void) => {
    const list = window.matchMedia(query);
    list.addEventListener('change', callback);
    return () => list.removeEventListener('change', callback);
  };
}

function useMediaQuery(query: string) {
  const subscribe = useMemo(() => subscribeQuery(query), [query]);
  return useSyncExternalStore(subscribe, () => window.matchMedia(query).matches, () => false);
}
```

Серверный снимок равен `false`, поэтому разметка с сервера показывает вариант без совпадения, пока не возьмётся браузер.

</details>
