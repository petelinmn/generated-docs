---
title: "Серверный рендеринг: базовая теория"
lang: ru
track: react
kind: module
doc: basics
slug: 14-server-rendering
part: 3
tags: [react, server-side-rendering, hydration, server-components, streaming]
versions: "React 19.3"
status: draft
updated: 2026-11-27
---

# Серверный рендеринг: базовая теория

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [обзор](overview.ru.md) · **1. Базовая** · [2. Продвинутая](advanced.ru.md) · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [эффекты](../06-effects/overview.ru.md), [Suspense](../12-concurrent-features/overview.ru.md) · **Время:** ~2 ч

Результаты получены на React 19.3.1 в Node.js, гидратация — в jsdom.

## Рендер в HTML

```tsx
function Counter() {
  const [n, setN] = useState(0);
  useEffect(() => { console.log('effect ran'); }, []);
  return <button onClick={() => setN(n + 1)}>count {n}</button>;
}

renderToString(<Counter />);
// <button>count <!-- -->0</button>      — no handler, the effect did not run
```

На сервере нет DOM и событий. `renderToStaticMarkup` даёт то же без служебных комментариев React — для HTML, который не будут гидратировать. Текст экранируется:

```tsx
renderToString(<p>{'<script>x</script>'}</p>);
// <p>&lt;script&gt;x&lt;/script&gt;</p>
```

## Гидратация

```tsx
// browser; the container already holds the server HTML
hydrateRoot(document.getElementById('root')!, <Counter />);
// the same <button> node is kept, the effect runs now, clicks work
```

Для страниц с серверным рендером `hydrateRoot` заменяет `createRoot(...).render`. Дерево компонентов и данные должны совпадать с теми, что использовал сервер.

## Браузерные объекты

```tsx
function Width() { return <p>{window.innerWidth}</p>; }
renderToString(<Width />);   // ReferenceError: window is not defined
```

Обращайтесь к браузерным API в эффектах и обработчиках событий: на сервере они не выполняются.

## Ошибки новичка

- ❌ `window`, `localStorage`, `document` во время рендера → ✅ в эффекте.
- ❌ Случайные числа или текущее время в рендере → ✅ передавать с сервера как данные.
- ❌ `createRoot` поверх серверного HTML → ✅ `hydrateRoot`.

## Связанные материалы

- [Продвинутая теория](advanced.ru.md)

## Источники

- [React: hydrateRoot](https://react.dev/reference/react-dom/client/hydrateRoot)
- [React: renderToString](https://react.dev/reference/react-dom/server/renderToString)
- [React: renderToPipeableStream](https://react.dev/reference/react-dom/server/renderToPipeableStream)
- [React: renderToReadableStream](https://react.dev/reference/react-dom/server/renderToReadableStream)
- [React: prerender](https://react.dev/reference/react-dom/static/prerender)
- [React: Server Components](https://react.dev/reference/rsc/server-components)
- [React: Server Functions](https://react.dev/reference/rsc/server-functions)
- [React: 'use client'](https://react.dev/reference/rsc/use-client)
- [React: 'use server'](https://react.dev/reference/rsc/use-server)
