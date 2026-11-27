---
title: "Browser: the DOM and events"
lang: en
track: javascript
kind: module
doc: overview
slug: 17-browser-dom-and-events
part: 3
levels: [1, 2, 3, 4]
tags: [dom, events, event-bubbling, event-delegation, fetch, web-storage, shadow-dom, custom-elements, cors, web-workers]
versions: "WHATWG DOM and HTML Living Standard; checked in Chrome 157 (headless)"
status: draft
updated: 2026-11-26
---

# Browser: the DOM and events

🌐 [Русский](overview.ru.md) · **English**

## In short

In a browser JavaScript works with the document through the DOM (Document Object Model): a page is a tree of nodes that can be searched, changed and listened to. Events (clicks, input, loading) travel through the tree: first down (capture), then up (bubbling). Network requests are made by `fetch`, client-side data is kept in `localStorage` and other storages, and Web APIs (workers, observers, custom elements) extend what a page can do.

## Key ideas

- **The DOM is a tree;** `querySelector` finds an element, `createElement` and `append` change the tree.
- **`textContent` is safe, `innerHTML` parses markup:** inserting user text with `innerHTML` is the basis of XSS ([module 20](../../overview.en.md)).
- **Live and static collections:** `getElementsByClassName` is live, `querySelectorAll` is static.
- **An event goes through three phases:** capture, target, bubble.
- **`stopPropagation` stops the path, `preventDefault` cancels the default action** (following a link, submitting a form).
- **Event delegation:** one handler on the parent and `e.target.closest(...)`.
- **`fetch` is not rejected on 4xx/5xx responses:** check `response.ok`; the body is read once.
- **`localStorage` stores only strings;** objects are serialized to JSON.
- **CORS restricts cross-origin requests;** without permitting headers the browser blocks the response.
- **Heavy work goes to a worker;** the DOM is available only from the main thread.

## Minimal example

```html
<ul id="list"><li class="item">One</li><li class="item">Two</li></ul>
<script type="module">
  const list = document.getElementById('list');
  list.addEventListener('click', e => {
    const li = e.target.closest('li.item');
    if (li) console.log('clicked', li.textContent);
  });
  list.querySelector('li').click();
</script>
```

```text
clicked One
```

One handler on `<ul>` serves all the `<li>`s, including those added later.

## When to use and when not

| Task | Choice |
|---|---|
| Find and change an element | `querySelector`, `textContent`, `classList` |
| Many similar elements reacting to a click | event delegation |
| Inserting HTML from an untrusted source | not `innerHTML`: `textContent` or sanitizing |
| A request to a server | `fetch` with an `ok` check |
| Storing settings on the client | `localStorage` (strings, up to a few MB) |
| Large structured data | IndexedDB |
| A heavy computation in the browser | a Web Worker |
| A complex UI with state | a framework (React, Vue, Svelte), see [TypeScript](../../../typescript/overview.en.md) |

- ❌ Do not work with the DOM one element at a time in a loop for large volumes: assemble in a fragment.
- ❌ Do not keep secrets and tokens in `localStorage`: any script of the page can read them.

## Material levels

| Level | What is inside |
|---|---|
| [1. Basic theory](basics.en.md) | the DOM, searching and changing, events, bubbling, delegation, `fetch`, storages |
| [2. Advanced theory](advanced.en.md) | load order, forms, custom elements and Shadow DOM, CORS, workers, history, IndexedDB, observers |
| [3. Expert theory](expert.en.md) | a reference, internals, pitfalls, interview questions |
| [4. Self-check](quiz.en.md) | quiz and [exercises](exercises.en.md) |
| 5. Runnable examples | not created yet — `/learn javascript 17-browser-dom-and-events level 5` |
| 6. Tests | not created yet — `/learn javascript 17-browser-dom-and-events level 6` |

## Related materials

- Prerequisites: [09 · Modules](../09-modules/overview.en.md), [12 · Promises and async/await](../12-promises-and-async-await/overview.en.md), [13 · The event loop and concurrency](../13-event-loop-and-concurrency/overview.en.md)
- Next: [18 · Metaprogramming](../18-metaprogramming/overview.en.md)
- Analogues in other tracks: [TypeScript](../../../typescript/overview.en.md) (DOM types, frameworks), [C#: Blazor and ASP.NET Core](../../../csharp/overview.en.md) (the server side)
- [React: components and JSX](../../../react/modules/01-components-and-jsx/overview.en.md)
- [Events in React](../../../react/modules/03-state-and-events/overview.en.md)
