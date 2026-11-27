---
title: "Server rendering"
lang: en
track: react
kind: module
doc: overview
slug: 14-server-rendering
part: 3
levels: [1, 2, 3, 4]
tags: [react, server-side-rendering, hydration, server-components, streaming]
versions: "React 19.3"
status: draft
updated: 2026-11-27
---

# Server rendering

🌐 [Русский](overview.ru.md) · **English**

## In a nutshell

React can render on the server to HTML. The browser shows the page before JavaScript arrives, and hydration then attaches React to that HTML. Streaming sends parts of the page as they become ready. Server Components go further: some components run only on the server and their code never reaches the browser.

## Key ideas

- Server rendering produces HTML; effects and event handlers do not work there.
- `hydrateRoot` makes the server HTML interactive and expects the same output.
- Streaming with `Suspense` sends the shell first and the slow parts later.
- A hydration mismatch is a bug, even when React recovers.
- Server Components have no state or effects; `'use client'` marks the border.
- Server Functions are public endpoints: check the user and the arguments.

## Minimal example

```tsx
function Counter() {
  const [n, setN] = useState(0);
  useEffect(() => { console.log('effect ran'); }, []);
  return <button onClick={() => setN(n + 1)}>count {n}</button>;
}

renderToString(<Counter />);
// <button>count <!-- -->0</button>      — no handler, the effect did not run
```

```tsx
// browser; the container already holds the server HTML
hydrateRoot(document.getElementById('root')!, <Counter />);
// the same <button> node is kept, the effect runs now, clicks work
```

## When to use and when not

- ✅ Content pages, search engines, slow devices and networks.
- ✅ Through a framework that handles routing, data and bundling.
- ❌ An internal tool behind a login where a client-only app is enough.
- ❌ Writing your own Server Components setup: it needs a compatible bundler.

## Levels of this material

| Level | What's inside |
|---|---|
| [1. Basics](basics.en.md) | rendering to a string, hydration |
| [2. Advanced](advanced.en.md) | streaming, mismatches, Server Components |
| [3. Expert](expert.en.md) | reference, gotchas, interview questions |
| [4. Self-check](quiz.en.md) | a quiz and [exercises](exercises.en.md) |
| 5. Runnable examples | not created yet — `/learn react 14-server-rendering level 5` |
| 6. Tests | not created yet — `/learn react 14-server-rendering level 6` |

## Related materials

- Prerequisites: [06 · Effects](../06-effects/overview.en.md), [12 · Concurrent features](../12-concurrent-features/overview.en.md), [13 · Actions](../13-actions/overview.en.md)
- Further topics: the [React roadmap](../../overview.en.md)
