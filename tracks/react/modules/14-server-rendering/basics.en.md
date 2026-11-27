---
title: "Server rendering: basics"
lang: en
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

# Server rendering: basics

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [effects](../06-effects/overview.en.md), [Suspense](../12-concurrent-features/overview.en.md) · **Time:** ~2 h

Results were produced with React 19.3.1 on Node.js, hydration in jsdom.

## Rendering to HTML

```tsx
function Counter() {
  const [n, setN] = useState(0);
  useEffect(() => { console.log('effect ran'); }, []);
  return <button onClick={() => setN(n + 1)}>count {n}</button>;
}

renderToString(<Counter />);
// <button>count <!-- -->0</button>      — no handler, the effect did not run
```

The server has no DOM and no events. `renderToStaticMarkup` gives the same without React's helper comments, for HTML that will never be hydrated. Text is escaped:

```tsx
renderToString(<p>{'<script>x</script>'}</p>);
// <p>&lt;script&gt;x&lt;/script&gt;</p>
```

## Hydration

```tsx
// browser; the container already holds the server HTML
hydrateRoot(document.getElementById('root')!, <Counter />);
// the same <button> node is kept, the effect runs now, clicks work
```

`hydrateRoot` replaces `createRoot(...).render` for server-rendered pages. The component tree and its data must match what the server used.

## Browser objects

```tsx
function Width() { return <p>{window.innerWidth}</p>; }
renderToString(<Width />);   // ReferenceError: window is not defined
```

Read browser APIs in effects or event handlers, which do not run on the server.

## Beginner mistakes

- ❌ `window`, `localStorage`, `document` during render → ✅ in an effect.
- ❌ Random numbers or current time in render → ✅ pass them from the server as data.
- ❌ `createRoot` over server HTML → ✅ `hydrateRoot`.

## Related materials

- [Advanced theory](advanced.en.md)

## Sources

- [React: hydrateRoot](https://react.dev/reference/react-dom/client/hydrateRoot)
- [React: renderToString](https://react.dev/reference/react-dom/server/renderToString)
- [React: renderToPipeableStream](https://react.dev/reference/react-dom/server/renderToPipeableStream)
- [React: renderToReadableStream](https://react.dev/reference/react-dom/server/renderToReadableStream)
- [React: prerender](https://react.dev/reference/react-dom/static/prerender)
- [React: Server Components](https://react.dev/reference/rsc/server-components)
- [React: Server Functions](https://react.dev/reference/rsc/server-functions)
- [React: 'use client'](https://react.dev/reference/rsc/use-client)
- [React: 'use server'](https://react.dev/reference/rsc/use-server)
