---
title: "Browser: the DOM and events: advanced"
lang: en
track: javascript
kind: module
doc: advanced
slug: 17-browser-dom-and-events
part: 3
tags: [script-loading, forms, form-data, custom-elements, shadow-dom, cors, web-workers, history-api, indexeddb, mutation-observer]
versions: "WHATWG DOM and HTML Living Standard; checked in Chrome 157 (headless)"
status: draft
updated: 2026-11-23
---

# Browser: the DOM and events: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

Builds on [basic theory](basics.en.md). All examples were run in Chrome 157 (headless) with pages served by a local Node.js server. Objects and arrays in the output are shown in JSON format, as the script collected them.

## Script loading order

```html
<head>
<script>LL('classic script in head');</script>
<script defer>LL('defer script');</script>
<script type="module">LL('module script');</script>
</head>
<body> … <script>LL('inline script in body');</script> …
```

```text
classic script in head: readyState loading body? false
defer script
inline script in body: readyState loading
readystatechange interactive
module script
DOMContentLoaded
readystatechange complete
load
```

- A classic script runs **right during parsing**, blocking the page; in `<head>` it has no `document.body` yet (`body? false`).
- The `defer` attribute works **only for scripts with `src`**: for an inline one it is ignored, and the script ran at once (`defer script` before the script in the body). For external files `defer` postpones execution until parsing ends.
- A `type="module"` script is deferred automatically (like `defer`): it ran after `readyState = interactive` and **before** `DOMContentLoaded`.
- `DOMContentLoaded` — the tree is ready; `load` — resources (images, styles) are loaded as well. `readyState` goes `loading` → `interactive` → `complete`.
- Modules run once ([module 09](../09-modules/basics.en.md)); top-level `await` is allowed.

## Forms

```js
const f = document.getElementById('f');
f.addEventListener('submit', e => { e.preventDefault(); console.log('submit event', Object.fromEntries(new FormData(f)), 'submitter', e.submitter?.id); });
document.getElementById('send').click(); f.requestSubmit();
console.log(f.elements.length, f.elements.user.value, f.elements.agree.checked, f.elements.role.value, typeof f.elements.age.value, f.elements.age.valueAsNumber, f.checkValidity());
```

```text
submit event {"user":"ann","age":"30","agree":"on","role":"b"} submitter send
submit event {"user":"ann","age":"30","agree":"on","role":"b"} submitter 
5 ann true b string 30 true
```

- The `submit` event arrives at the **form**, not the button; `e.preventDefault()` cancels the page reload.
- `new FormData(form)` collects the fields: a checkbox gives `'on'`, an unchecked checkbox does not go into the data; the values are **strings** (`age` is `'30'`, the number is `valueAsNumber`).
- `form.elements` — access to fields by name; `checkValidity()` checks constraints (`required`, `min`, …).
- `form.requestSubmit()` starts submission **with validation and the event** (`submitter` is empty); `form.submit()` does not fire the `submit` event.

## Input and focus events

```js
const ev = []; txt.addEventListener('input', () => ev.push('input')); txt.addEventListener('change', () => ev.push('change'));
txt.value = 'set by script'; console.log(ev); txt.dispatchEvent(new Event('input', { bubbles: true })); console.log(ev, new Event('x').isTrusted);
const fe = []; document.body.addEventListener('focusin', () => fe.push('focusin')); document.body.addEventListener('focus', () => fe.push('focus-bubble')); document.body.addEventListener('focus', () => fe.push('focus-capture'), true); txt.focus(); console.log(fe);
```

```text
[]
[ "input" ] false
[ "focus-capture", "focusin" ]
```

- Assigning `value` from code **does not fire** `input` and `change`; they are produced by the user or `dispatchEvent`.
- Events created by a script have `isTrusted === false`.
- `input` arrives on every change, `change` on completion (loss of focus, `Enter`).
- `focus` and `blur` **do not bubble** (caught only in the capture phase); for delegation use `focusin`/`focusout`, which bubble. `mouseenter`/`mouseleave` do not bubble either.

## Custom events

```js
let detail; document.addEventListener('app:ping', e => detail = e.detail);
document.dispatchEvent(new CustomEvent('app:ping', { detail: { n: 1 } })); console.log(detail);
```

```text
{"n":1}
```

`CustomEvent` carries data in `detail`; for the event to bubble, pass `{ bubbles: true }`.

## Custom elements and Shadow DOM

```js
class MyCard extends HTMLElement {
  connectedCallback() { const r = this.attachShadow({ mode: 'open' }); r.innerHTML = '<style>p{color:red}</style><p part="x">shadow <slot></slot></p>'; }
  static get observedAttributes() { return ['title']; }
  attributeChangedCallback(n, o, v) { console.log('attr changed', n, o, v); }
}
customElements.define('my-card', MyCard);
const card = document.createElement('my-card'); card.setAttribute('title', 'T'); card.textContent = 'light'; document.getElementById('host').append(card);
console.log(card.shadowRoot.querySelectorAll('p').length, document.querySelectorAll('my-card p').length, card instanceof MyCard);
```

```text
attr changed title null T
1 0 true
```

- A custom element's name **must contain a hyphen** (`my-card`); the class extends `HTMLElement` and is registered with `customElements.define`.
- Lifecycle: `connectedCallback` (inserted into the document), `disconnectedCallback`, `attributeChangedCallback` (for the attributes in `observedAttributes`).
- **Shadow DOM** isolates markup and styles: the `p` inside the shadow is in `card.shadowRoot`, but `document.querySelectorAll('my-card p')` does not find it (`0`), and the style `p{color:red}` does not leak out.
- `<slot>` shows the element's "light" children inside the shadow.
- An event from inside the shadow is **retargeted**: from outside `e.target` is the `MY-CARD` itself, while `e.composedPath()` contains the inner nodes too (in the experiment `retargeted MY-CARD composedPath>3:true`).

## CORS

The browser forbids JavaScript to read a response from **another origin** (protocol + host + port) unless the server permits it with the `Access-Control-Allow-Origin` header:

```js
const other = location.origin.replace('localhost', '127.0.0.1');
try { await fetch(other + '/json') } catch (e) { console.log('cors blocked', e.name) }
console.log('cors ok', await (await fetch(other + '/cors-ok')).json());
const rn = await fetch(other + '/json', { mode: 'no-cors' }); console.log('no-cors', rn.type, rn.status, rn.ok);
```

```text
cors blocked TypeError
cors ok {"cors":true}
no-cors opaque 0 false
```

- Without the header `fetch` is rejected with a `TypeError` (the reason is visible only in the browser console).
- A server with `Access-Control-Allow-Origin: *` allows reading.
- `mode: 'no-cors'` sends the request, but the response is "opaque" (`opaque`, status `0`): it cannot be read. This is not a way around CORS but a mode for sending data "blind".
- Requests with non-standard methods or headers are preceded by a **preflight** `OPTIONS` request.
- CORS protects the **user** in the browser; server-side requests (Node.js, curl) are not subject to it.

> ⚠️ Verify: the `OPTIONS` preflight, `credentials: 'include'` and `SameSite` restrictions for cookies were not run in this material.

## Web workers

A worker runs in a separate thread with no access to the DOM; communication is by messages ([module 13](../13-event-loop-and-concurrency/advanced.en.md#worker-threads)):

```js
const blob = new Blob(['onmessage = e => postMessage(e.data * 2)'], { type: 'text/javascript' });
const w = new Worker(URL.createObjectURL(blob));
console.log('worker', await new Promise(r => { w.onmessage = e => r(e.data); w.postMessage(21); })); w.terminate();
```

```text
worker 42
```

Data is copied (structured clone), buffers can be transferred (`transfer`). A worker from a file: `new Worker('worker.js', { type: 'module' })`.

## History and the address

```js
history.pushState({ p: 1 }, '', '/page2?x=1'); console.log(location.pathname, location.search, history.state);
history.replaceState(null, '', '/');
let pop = new Promise(r => addEventListener('hashchange', r, { once: true })); location.hash = 'abc'; await pop; console.log(location.hash);
```

```text
/page2 ?x=1 {"p":1}
#abc
```

- `pushState` changes the address and adds a history entry **without reloading**: the basis of single-page-app routing; `replaceState` replaces the current entry.
- The `popstate` event arrives on "back/forward" navigation; `hashchange` on a change of `#`.

## BroadcastChannel and IndexedDB

```js
const bc1 = new BroadcastChannel('c'), bc2 = new BroadcastChannel('c');
console.log('broadcast', await new Promise(r => { bc2.onmessage = e => r(e.data); bc1.postMessage({ hi: 1 }); }));
```

```text
broadcast {"hi":1}
```

- `BroadcastChannel` passes messages between tabs and workers of one origin.
- **IndexedDB** is an asynchronous object store (in the experiment: open a database, create a store with `keyPath: 'id'`, write `{ id: 1, v: 'a' }` and read it back). It suits large data, offline mode and indexes; the event-based API is inconvenient, so wrappers are usually used.

## Observers and task order

```js
const t0 = []; setTimeout(() => t0.push('timeout'), 0); requestAnimationFrame(() => t0.push('raf')); Promise.resolve().then(() => t0.push('micro'));
const mo = new MutationObserver(() => t0.push('mutation')); mo.observe(document.body, { childList: true });
document.body.append(document.createElement('hr')); t0.push('sync');
await new Promise(r => setTimeout(r, 100)); console.log(t0);
```

```text
[ "sync", "micro", "mutation", "raf", "timeout" ]
```

- Synchronous code goes first, then **microtasks**: `then` and the `MutationObserver` callback (it is a microtask too).
- The order of `requestAnimationFrame` and `setTimeout(…, 0)` depends on the moment of the frame: do not rely on it.
- `requestAnimationFrame` is called before painting a frame: suitable for animations.
- `IntersectionObserver` (element visibility) and `ResizeObserver` (size change) are available as functions (in the experiment `function`).
- Reading geometry (`offsetWidth`, `getComputedStyle`) **forces** the browser to recompute the layout (in the experiment `100`, `block` right after writing a style): alternating writes and reads in a loop slows the page.

Other Web APIs (presence checked): `matchMedia`, `requestIdleCallback`, `navigator.clipboard`, `Notification`, `navigator.serviceWorker`, `WebSocket`, `EventSource`; many of them need a **secure context** (`isSecureContext`; `http://localhost` counts as secure).

## Best practices

- Delegate events; remove listeners (`{ signal }`) when components are removed.
- Scripts as modules or with `defer`.
- Insert text with `textContent`, markup from foreign data only sanitized.
- Check `response.ok` and handle network errors.
- Heavy work in a worker; animations through `requestAnimationFrame`.
- Do not mix DOM reads and writes in a loop: read first, then write.
- Do not keep secrets in `localStorage`.

## Common mistakes

- ❌ `defer` on an inline script → ✅ an external file or `type="module"`.
- ❌ Expecting `input` after `input.value = …` → ✅ call `dispatchEvent` or handle the value directly.
- ❌ `focus` for delegation → ✅ `focusin`.
- ❌ Working around CORS with `no-cors` → ✅ configure headers on the server.
- ❌ Touching `document` from a worker → ✅ pass data by messages.
- ❌ `form.submit()` and waiting for `submit` → ✅ `requestSubmit()`.

## Related materials

- Next: [expert theory](expert.en.md)
- Worker threads and the task queue: [13 · The event loop and concurrency](../13-event-loop-and-concurrency/advanced.en.md)
- Browser security (XSS, CSP): module 20 of the roadmap ([track](../../overview.en.md))

## Sources

- [MDN: `<script>`: async and defer](https://developer.mozilla.org/en-US/docs/Web/HTML/Reference/Elements/script#defer)
- [MDN: Custom elements](https://developer.mozilla.org/en-US/docs/Web/API/Web_components/Using_custom_elements)
- [MDN: Using shadow DOM](https://developer.mozilla.org/en-US/docs/Web/API/Web_components/Using_shadow_DOM)
- [MDN: Cross-Origin Resource Sharing (CORS)](https://developer.mozilla.org/en-US/docs/Web/HTTP/Guides/CORS)
- [MDN: Web Workers API](https://developer.mozilla.org/en-US/docs/Web/API/Web_Workers_API)
- [MDN: History API](https://developer.mozilla.org/en-US/docs/Web/API/History_API)
- [MDN: IndexedDB API](https://developer.mozilla.org/en-US/docs/Web/API/IndexedDB_API)
- [MDN: MutationObserver](https://developer.mozilla.org/en-US/docs/Web/API/MutationObserver)
