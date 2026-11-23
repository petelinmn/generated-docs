---
title: "Browser: the DOM and events: expert"
lang: en
track: javascript
kind: module
doc: expert
slug: 17-browser-dom-and-events
part: 3
tags: [dom, event-dispatch, event-loop-rendering, live-collections, cors, storage-limits, shadow-dom, secure-context]
versions: "WHATWG DOM and HTML Living Standard; checked in Chrome 157 (headless)"
status: draft
updated: 2026-11-23
---

# Browser: the DOM and events: expert

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · **3. Expert** · [4. Quiz](quiz.en.md)

Builds on [basic](basics.en.md) and [advanced](advanced.en.md) theory. Results marked "in the experiment" were obtained in Chrome 157 (headless).

## Reference

### `addEventListener` options

| Option | Meaning |
|---|---|
| `capture: true` (or the third argument `true`) | listen in the capture phase |
| `once: true` | call once and remove (in the experiment `once 1`) |
| `passive: true` | a promise not to call `preventDefault` (speeds up scrolling) |
| `signal` | an `AbortSignal`: `abort()` removes the listener (in the experiment there were no calls after `abort()`) |

### Event properties

| Property | Meaning |
|---|---|
| `target` / `currentTarget` | where it happened / what the listener is attached to |
| `bubbles`, `cancelable` | whether it bubbles, whether it can be cancelled |
| `defaultPrevented` | whether `preventDefault()` was called |
| `isTrusted` | `true` for user actions, `false` for script-created ones (in the experiment `false`) |
| `composedPath()` | the event path, including nodes inside open shadows |
| `stopPropagation()` / `stopImmediatePropagation()` | stop the path / also the other listeners on this element |

### Behavior of event types

| Event | Bubbles | Note |
|---|---|---|
| `click`, `input`, `change`, `submit`, `keydown` | yes | `submit` — on the form |
| `focus`, `blur` | **no** | `focusin`/`focusout` bubble (in the experiment `focus-capture`, `focusin`) |
| `mouseenter`, `mouseleave` | no | `mouseover`/`mouseout` bubble |
| `DOMContentLoaded`, `load` | — | listened for on `document` and `window`; the order `DOMContentLoaded` → `load` (in the experiment) |

### Storages

| Storage | Lifetime | Sent to the server | Access from JS | Data type |
|---|---|---|---|---|
| `localStorage` | until cleared | no | yes | strings |
| `sessionStorage` | the tab | no | yes | strings |
| Cookies | by `Expires`/`Max-Age` | **yes, with every request** | not with `HttpOnly` (in the experiment not visible) | strings, small size |
| IndexedDB | until cleared | no | yes (asynchronously) | structured objects |

> ⚠️ Verify: size limits (`localStorage` on the order of a few MB, cookies about 4 KB), eviction rules and private browsing mode were not checked in this material.

### `fetch` parameters

| Parameter | Purpose |
|---|---|
| `method`, `headers`, `body` | the request |
| `signal` | cancellation / `AbortSignal.timeout(ms)` (in the experiment `TimeoutError`) |
| `mode` | `cors` (default), `same-origin`, `no-cors` (an opaque response) |
| `credentials` | `same-origin` (default), `include`, `omit` — cookies |
| `cache`, `redirect`, `referrer` | policies |

Response methods: `json()`, `text()`, `blob()`, `arrayBuffer()`, `formData()`, `clone()`; `ok`, `status`, `headers`, `type`.

## Internals

- **HTML parsing.** The browser parses the document and builds the DOM as data arrives. A classic script stops parsing (in the experiment `document.body` is missing in `<head>`); `defer` and modules run after parsing but before `DOMContentLoaded` (in the experiment the order `interactive` → `module script` → `DOMContentLoaded`).
- **The three phases of an event.** The path from `window` to the target is built in advance: first the capture listeners top down, then the target, then bubbling bottom up (in the experiment `outer-capture`, `btn-target`, `inner-bubble`, `outer-bubble`). Listeners are called synchronously in subscription order.
- **Live and static lists.** An `HTMLCollection` (`getElementsBy*`, `children`) follows the tree, a `NodeList` from `querySelectorAll` is a snapshot at the moment of the query (in the experiment `2` and `3` after the insertion).
- **The event loop and rendering.** A task runs, then the microtask queue; then (about 60 times a second) the browser may run `requestAnimationFrame` callbacks, recompute the layout and repaint. So long synchronous work "freezes" the page ([module 13](../13-event-loop-and-concurrency/expert.en.md)).
- **Forced layout.** Reading `offsetWidth`, `getBoundingClientRect()`, `getComputedStyle(...)` after a style change makes the browser recompute the layout at once (in the experiment the read returned `100` immediately). Alternating writes and reads in a loop causes repeated recomputations.
- **Shadow DOM.** An element has its own subtree; document selectors do not see it (`document.querySelectorAll('my-card p')` is `0`), events are retargeted to the host.
- **The security model.** An origin = protocol + host + port; "same origin" allows reading; CORS extends this by the server. `HttpOnly` cookies are hidden from JavaScript but go to the server.

> ⚠️ Verify: the frame rate, the exact order "task → microtasks → rAF → layout → paint" and the behavior of `requestAnimationFrame` in background tabs were not checked in the experiments; the order of rAF and a zero-delay timer was not stable in the experiment.

## Gotchas

### 1. `defer` on an inline script

It is ignored: the script runs at once (in the experiment `defer script` was printed before the script in the body).

### 2. Static and live lists

`querySelectorAll` does not update (`2`), `getElementsByClassName` does (`3`).

### 3. `innerHTML` executes markup

User text in `innerHTML` is a path to XSS; `textContent` inserts text (in the experiment `&lt;b&gt;`).

### 4. An anonymous function is not removed

`removeEventListener('click', () => …)` does not remove the listener (in the experiment the counter grew).

### 5. `focus` does not bubble

Delegation goes through `focusin`.

### 6. Writing `value` produces no events

`input.value = …` fires no `input`/`change` (in the experiment `[]`).

### 7. `fetch` and statuses

`404` does not reject the promise (in the experiment `ok false`); only a network error is rejected (`TypeError`).

### 8. The body is read once

A second `text()` throws a `TypeError`.

### 9. `localStorage` is strings only

`setItem('n', 5)` → `'5'` (in the experiment `string`).

### 10. `no-cors` does not "bypass" CORS

The response is opaque: `type: 'opaque'`, `status: 0`, `ok: false`.

### 11. `form.submit()` does not fire the event

Use `requestSubmit()`.

## Trade-offs and alternatives

| Approach | Pros | Cons |
|---|---|---|
| Plain DOM | no dependencies, a fast start | a lot of manual code for a complex UI |
| Templates and `<template>` | fast clones of markup | manual data binding |
| Web components | a standard, encapsulation | verbose, own nuances with forms and SSR |
| Frameworks (React, Vue, Svelte) | components, reactivity | a build, a dependency; not run |
| `fetch` | a standard, promises | no timeout and retries out of the box |
| `XMLHttpRequest` | old environments | a legacy API |
| `localStorage` | simple | synchronous, strings, no protection |
| IndexedDB | volume, indexes | a complex API |

## Interview questions

<details><summary>1. What are event bubbling and capturing?</summary>

An event goes top down (capture), reaches the target and rises up (bubbling). Listeners work on bubbling by default.

→ [Basic theory](basics.en.md#events)

</details>

<details><summary>2. How does <code>target</code> differ from <code>currentTarget</code>?</summary>

`target` is the element where the event arose; `currentTarget` is the element the listener fired on.

→ [Basic theory](basics.en.md#events)

</details>

<details><summary>3. What is event delegation and why?</summary>

One listener on the parent and `e.target.closest(...)`: fewer listeners, works for elements added later.

→ [Basic theory](basics.en.md#event-delegation)

</details>

<details><summary>4. Trick: how does <code>textContent</code> differ from <code>innerHTML</code>?</summary>

`innerHTML` parses markup (an XSS risk), `textContent` inserts text.

→ [Gotcha 3](#3-innerhtml-executes-markup)

</details>

<details><summary>5. Trick: <code>fetch</code> received a <code>404</code>. Is the promise rejected?</summary>

No. You must check `response.ok`.

→ [Gotcha 7](#7-fetch-and-statuses)

</details>

<details><summary>6. What is the difference between <code>localStorage</code>, <code>sessionStorage</code> and cookies?</summary>

Lifetime and sending to the server: cookies go with every request, the others do not; `sessionStorage` — the tab only.

→ [Reference](#storages)

</details>

<details><summary>7. What is CORS and what does it protect against?</summary>

A policy by which a server allows another origin to read a response; it protects the user from another site reading their data.

→ [Advanced theory](advanced.en.md#cors)

</details>

<details><summary>8. Trick: will <code>defer</code> work on an inline script?</summary>

No, the attribute works only for scripts with `src`.

→ [Gotcha 1](#1-defer-on-an-inline-script)

</details>

<details><summary>9. How does a live collection differ from a static one?</summary>

A live one (`getElementsBy*`) reflects tree changes, a static one (`querySelectorAll`) is a snapshot.

→ [Gotcha 2](#2-static-and-live-lists)

</details>

<details><summary>10. What is Shadow DOM for?</summary>

Isolating markup and styles inside a component; outer selectors do not reach inside.

→ [Advanced theory](advanced.en.md#custom-elements-and-shadow-dom)

</details>

<details><summary>11. What is forced layout?</summary>

Reading geometry after writing styles makes the browser recompute the layout at once; in a loop this slows things down.

→ [Internals](#internals)

</details>

<details><summary>12. How do you do heavy work without freezing the page?</summary>

A Web Worker (no DOM, with messages) or splitting into chunks.

→ [Advanced theory](advanced.en.md#web-workers)

</details>

## Cheat sheet

```text
document.querySelector(css) / querySelectorAll (static) / getElementsByClassName (live)
el.append(...) / remove() / classList / dataset / textContent (safe) / innerHTML (markup)
el.addEventListener(type, fn, { capture, once, passive, signal }); e.target / currentTarget / preventDefault / stopPropagation
delegation: parent.addEventListener('click', e => e.target.closest('.item'))
const r = await fetch(url, { method, headers, body, signal }); if (!r.ok) throw …; await r.json()
localStorage.setItem(k, JSON.stringify(v)); JSON.parse(localStorage.getItem(k))
new Worker(url); worker.postMessage(x); form.requestSubmit(); new FormData(form)
```

- An event: capture → target → bubble.
- `fetch` does not reject on `4xx`/`5xx`.
- Secrets do not go in `localStorage`.

## Related materials

- Previous level: [advanced theory](advanced.en.md)
- The event loop and rendering: [13 · The event loop and concurrency](../13-event-loop-and-concurrency/expert.en.md)
- Modules in the browser: [09 · Modules](../09-modules/expert.en.md)
- DOM types in TypeScript: [TypeScript](../../../typescript/overview.en.md)

## Sources

- [WHATWG DOM Standard: Events](https://dom.spec.whatwg.org/#events)
- [WHATWG HTML Standard: Event loops](https://html.spec.whatwg.org/multipage/webappapis.html#event-loops)
- [WHATWG Fetch Standard](https://fetch.spec.whatwg.org/)
- [MDN: Event](https://developer.mozilla.org/en-US/docs/Web/API/Event)
- [MDN: Same-origin policy](https://developer.mozilla.org/en-US/docs/Web/Security/Same-origin_policy)
- [MDN: Storage quotas and eviction criteria](https://developer.mozilla.org/en-US/docs/Web/API/Storage_API/Storage_quotas_and_eviction_criteria)
