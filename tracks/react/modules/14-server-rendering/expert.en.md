---
title: "Server rendering: expert"
lang: en
track: react
kind: module
doc: expert
slug: 14-server-rendering
part: 3
tags: [react, server-side-rendering, hydration, server-components, streaming]
versions: "React 19.3"
status: draft
updated: 2026-11-27
---

# Server rendering: expert

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · **3. Expert** · [4. Quiz](quiz.en.md)

## Under the hood

The server runs components once and writes HTML. On the client React walks the existing DOM while rendering and adopts matching nodes instead of creating them. `Suspense` boundaries are marked in the HTML with comments, which lets parts hydrate and arrive separately.

## Reference

| API | Environment | Notes |
|---|---|---|
| `renderToString` | any | synchronous, limited `Suspense` |
| `renderToStaticMarkup` | any | no hydration |
| `renderToPipeableStream` | Node.js | `onShellReady`, `onShellError`, `onAllReady`, `onError`, `abort` |
| `renderToReadableStream` | Web Streams | promise of a stream with `allReady` |
| `prerender` | Web Streams | waits for all data; `prerenderToNodeStream` for Node |
| `hydrateRoot(node, el, options)` | browser | `onRecoverableError`, `onCaughtError`, `onUncaughtError`, `identifierPrefix`, `formState` |
| `'use client'` | bundler | module border |
| `'use server'` | bundler | Server Functions, `async` only |

`useId` gives matching identifiers on both sides (here `_R_0_`); with several roots set the same `identifierPrefix`.

## Gotchas

### renderToString and Suspense

```tsx
renderToString(<Suspense fallback={<i>loading</i>}><LazyThing /></Suspense>);
// the output contains the fallback <i>loading</i> and a note that renderToString
// does not support Suspense; the client renders the content itself
```

### A text mismatch replaces DOM nodes

```tsx
// server rendered  <p>on server</p>, the client renders <p>on client</p>
hydrateRoot(container, <Where w="client" />, {
  onRecoverableError: error => report(error),
});
// error: Hydration failed because the server rendered text didn't match the client.
// the screen shows "on client", and the <p> is a new DOM node
```

### An attribute mismatch is not patched

```tsx
// server: <p class="server">x</p>   client: <p className="client">x</p>
// after hydration the DOM still says class="server"; a console error is printed
```

### suppressHydrationWarning keeps the server text

```tsx
<time suppressHydrationWarning>{value}</time>
// server value 1, client value 2: no warning, and the text stays 1
```

### Two renders with useSyncExternalStore

```tsx
const value = useSyncExternalStore(subscribe, () => 'client', () => 'server');
// server render: "server"; hydration: "server", then a second render with "client"
```

## Trade-offs and alternatives

| Approach | First content | Server cost | Complexity |
|---|---|---|---|
| Client only | after JavaScript | low | low |
| Static generation | immediate | at build | medium, data may be stale |
| Server rendering | fast | per request | higher |
| Streaming | shell first | per request | higher |
| Server Components | less client code | per request or build | needs a framework |

## Interview questions

**1. What does hydration do?**

<details><summary>Answer</summary>

Attaches React and event handlers to server-rendered HTML.

</details>

**2. Do effects run during server rendering?**

<details><summary>Answer</summary>

No.

</details>

**3. `renderToString` or `renderToPipeableStream`?**

<details><summary>Answer</summary>

The second streams and supports `Suspense`; the first is synchronous and limited.

</details>

**4. `onShellReady` or `onAllReady`?**

<details><summary>Answer</summary>

The first to stream to users; the second when the complete HTML is needed, such as for crawlers.

</details>

**5. Name causes of mismatches.**

<details><summary>Answer</summary>

Browser checks and APIs in render, different data, extra whitespace around the root HTML.

</details>

**6. Trick question: can the status code change after streaming starts?**

<details><summary>Answer</summary>

No.

</details>

**7. What can a Server Component not do?**

<details><summary>Answer</summary>

Use state, effects, event handlers or create context.

</details>

**8. Trick question: does `'use server'` make a Server Component?**

<details><summary>Answer</summary>

No, it marks Server Functions.

</details>

**9. Can a Client Component render a Server Component?**

<details><summary>Answer</summary>

It cannot import it as such, but can receive it as `children`.

</details>

**10. Which props cross the border?**

<details><summary>Answer</summary>

Serializable ones: primitives, plain objects, arrays, Date, Map, Set, JSX, promises, Server Functions.

</details>

**11. Why are Server Functions a security matter?**

<details><summary>Answer</summary>

Anyone can call them with any arguments.

</details>

**12. How do you put data into the page safely for hydration?**

<details><summary>Answer</summary>

Serialize to JSON and escape `<` so the data cannot close the script tag.

</details>

## Related materials

- [Advanced theory](advanced.en.md)
- [Quiz](quiz.en.md) and [exercises](exercises.en.md)

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
