---
title: "Эффекты: продвинутая теория"
lang: ru
track: react
kind: module
doc: advanced
slug: 06-effects
part: 2
tags: [react, effects, use-effect, hooks, cleanup]
versions: "React 19.3"
status: draft
updated: 2026-11-27
---

# Эффекты: продвинутая теория

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · **2. Продвинутая** · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

## Порядок вызовов

```tsx
function Order({ id }: { id: string }) {
  log.push('render ' + id);
  useLayoutEffect(() => { log.push('layout ' + id); return () => { log.push('layout cleanup ' + id); }; }, [id]);
  useEffect(() => { log.push('effect ' + id); return () => { log.push('cleanup ' + id); }; }, [id]);
  return null;
}
```

```text
mount a:      render a | layout a | effect a
change to b:  render b | layout cleanup a | layout b | cleanup a | effect b
same b again: render b
unmount:      layout cleanup b | cleanup b
```

Layout-эффекты выполняются до перерисовки браузером, обычные — после фиксации. Эффекты потомков выполняются раньше эффектов родителя:

```text
render parent | render child | effect child | effect parent
```

## Зависимости по ссылке

```tsx
const options = { v };                               // a new object on every render
useEffect(() => { counts.none++; });                 // no array
useEffect(() => { counts.empty++; }, []);
useEffect(() => { counts.obj++; }, [options]);
useEffect(() => { counts.prim++; }, [v]);
// three renders with the same v:  {"none":3,"empty":1,"obj":3,"prim":1}
```

Объект или функция, созданные при рендере, каждый раз новые. Создавайте их внутри эффекта, выносите из компонента или зависьте от их примитивных полей.

## Устаревшие значения

```tsx
useEffect(() => {
  const id = setInterval(() => setN(n + 1), 5);      // n is frozen at 0
  return () => clearInterval(id);
}, []);
// the screen stays at 1

useEffect(() => {
  const id = setInterval(() => setN(x => x + 1), 5); // no stale value
  return () => clearInterval(id);
}, []);
// the number keeps growing
```

Первый вариант захватил `n` первого рендера. Добавьте значение в зависимости или не читайте его, используя функцию-обновление.

## Гонки

```tsx
useEffect(() => {
  let ignore = false;
  load(id).then(data => { if (!ignore) setData(data); });
  return () => { ignore = true; };
}, [id]);
// id changes from a slow request to a fast one:
// without the flag the screen ends up with the old, slow data; with it, with the new data
```

Для `fetch` объект `AbortController` ещё и отменяет сам запрос.

## Возможно, эффект не нужен

```tsx
// avoid: an extra render with an outdated value
const [full, setFull] = useState('');
useEffect(() => { setFull(first + ' Smith'); }, [first]);
// renders:  "" then "Ann Smith"

// instead
const full = first + ' Smith';
```

| Что нужно | Без эффекта |
|---|---|
| значение, вычислимое из props или состояния | вычислить при рендере |
| сбросить состояние для другого объекта | `key` |
| отреагировать на действие пользователя | обработчик события |
| сообщить родителю об изменении | вызвать функцию родителя в том же обработчике |

## Лучшие практики

- Один эффект на один объект синхронизации.
- Пишите очистку одновременно с установкой.
- Не отключайте правило линтера о зависимостях.
- Повторяющийся эффект выносите в пользовательский хук.

## Частые ошибки

- ❌ Цепочка эффектов, задающих состояние друг для друга → ✅ вычислять при рендере или в обработчике.
- ❌ Не учитывать ответы не по порядку → ✅ флаг или отмена.
- ❌ Заглушать предупреждение о зависимостях → ✅ изменить код так, чтобы значение не требовалось.

## Связанные материалы

- [Базовая теория](basics.ru.md)
- [Экспертная теория](expert.ru.md)

## Источники

- [React: useEffect](https://react.dev/reference/react/useEffect)
- [React: возможно, эффект не нужен](https://react.dev/learn/you-might-not-need-an-effect)
- [React: useLayoutEffect](https://react.dev/reference/react/useLayoutEffect)
