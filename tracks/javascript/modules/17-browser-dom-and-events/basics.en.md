---
title: "Browser: the DOM and events: basics"
lang: en
track: javascript
kind: module
doc: basics
slug: 17-browser-dom-and-events
part: 3
tags: [dom, query-selector, class-list, dataset, events, bubbling, capturing, event-delegation, fetch, local-storage]
versions: "WHATWG DOM and HTML Living Standard; checked in Chrome 157 (headless)"
status: draft
updated: 2026-11-23
---

# Browser: the DOM and events: basics

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [09 · Modules](../09-modules/overview.en.md), [12 · Promises and async/await](../12-promises-and-async-await/overview.en.md) · **Time:** ~3 h

All examples were run in Chrome 157 (headless): the page was served by a local Node.js server, and the script collected the results and sent them back to the server. The code is in `<script type="module">` ([module 09](../09-modules/overview.en.md)); in a browser console it behaves the same way. Objects and arrays in the output are shown in JSON format (as the script collected them); a browser console formats them differently.

## Why it matters

Any interactive site is reading and changing the DOM, reacting to events and exchanging data with a server.

## The DOM: a tree of nodes

The browser parses HTML into a tree of **nodes**: elements, text, comments. The root is `document`. An element has a parent, children, siblings:

```html
<ul id="list" class="items"><li class="item" data-id="1">One</li><li class="item active" data-id="2">Two</li></ul>
```

- `element.parentElement`, `children`, `nextElementSibling` — navigating elements.
- `element.tagName` (`'LI'`), `id`, `className`, `classList`, `dataset` — properties.
- `data-*` attributes are available through `dataset` in camelCase: `data-new-key` ↔ `dataset.newKey`.

## Searching for elements

```js
const items = document.querySelectorAll('.item'); const live = document.getElementsByClassName('item');
console.log('qsa', items.length, items.constructor.name, 'live', live.length, live.constructor.name);
document.getElementById('list').append(Object.assign(document.createElement('li'), { className: 'item', textContent: 'Three' }));
console.log('after append: qsa', items.length, 'live', live.length);
const li = document.querySelector('li.active');
console.log(li.dataset.id, li.dataset, li.classList.contains('active'), li.closest('ul').id, li.matches('.item'), li.parentElement.tagName, li.nextElementSibling?.textContent);
```

```text
qsa 2 NodeList live 2 HTMLCollection
after append: qsa 2 live 3
2 {"id":"2"} true list true UL Three
```

- `querySelector(css)` is the first matching element or `null`, `querySelectorAll(css)` is a **static** `NodeList`: after adding an element it still holds `2`.
- `getElementsByClassName` and `getElementsByTagName` return a **live** `HTMLCollection`: it updated (`3`).
- `closest(css)` goes up through the ancestors to the first matching one, `matches(css)` checks the element itself.
- `getElementById('id')` is faster but searches only by id.

## Changing the DOM

```js
li.classList.toggle('active'); li.classList.add('x', 'y'); li.dataset.newKey = 'v';
console.log(li.className, li.getAttribute('data-new-key'), li.outerHTML);
const first = document.querySelector('.item'); document.getElementById('list').append(first);
console.log('moved', [...document.querySelectorAll('.item')].map(x => x.textContent).join(','));
first.remove(); console.log('removed connected?', first.isConnected, document.querySelectorAll('.item').length);
```

```text
item x y v <li class="item x y" data-id="2" data-new-key="v">Two</li>
moved Two,Three,One
removed connected? false 2
```

- `classList.add/remove/toggle/contains` manage classes without manual string handling.
- `append(node)` inserts at the end; inserting an already existing node **moves** it (`One` went to the end).
- `remove()` takes the node out of the document: `isConnected` is `false`, but the object remains and can be inserted again.
- `createElement` creates an element, `createDocumentFragment` a light container for batch insertion (after `append` the fragment is emptied).

## textContent and innerHTML

```js
const d = document.createElement('div'); d.innerHTML = '<b>bold</b> & <i>it</i>';
console.log(d.textContent, d.innerHTML, d.children.length);
d.textContent = '<b>not bold</b>'; console.log(d.innerHTML, d.children.length);
```

```text
bold & it <b>bold</b> &amp; <i>it</i> 2
&lt;b&gt;not bold&lt;/b&gt; 0
```

- `innerHTML` **parses** the string as markup: two elements appeared (`<b>`, `<i>`).
- `textContent` inserts **text**: `<b>` turned into `&lt;b&gt;` and did not become an element.
- If a string has user data, insert it with `textContent`: `innerHTML` executes markup and opens the way to XSS.
- `insertAdjacentHTML('beforeend', html)` inserts markup without overwriting the existing children.

## Events

A listener is attached with `addEventListener(type, function, options)`:

```js
const order = [];
const outer = document.getElementById('outer'), inner = document.getElementById('inner'), btn = document.getElementById('btn');
outer.addEventListener('click', () => order.push('outer-bubble'));
outer.addEventListener('click', () => order.push('outer-capture'), true);
inner.addEventListener('click', () => order.push('inner-bubble'));
btn.addEventListener('click', e => order.push('btn-target:' + (e.target === e.currentTarget)));
btn.click(); console.log('order', order);
```

```text
order ["outer-capture","btn-target:true","inner-bubble","outer-bubble"]
```

The markup: `#outer > #inner > #btn`. An event passes three phases:

1. **Capture** — top down; listeners with the third argument `true` (`outer-capture`).
2. **Target** — the element itself (`btn`).
3. **Bubble** — bottom up (`inner-bubble`, then `outer-bubble`).

- `e.target` is the element where the event happened; `e.currentTarget` (and `this` in an ordinary function) is the element the current listener is attached to.
- Listeners are called **synchronously**: the order `before`, `handler`, `after` with `btn.click()` (in the experiment).

## Controlling an event

```js
let onceCount = 0; btn.addEventListener('click', () => onceCount++, { once: true }); btn.click(); btn.click(); console.log('once', onceCount);
const a = document.createElement('a'); a.href = '#x'; let prevented;
a.addEventListener('click', e => { e.preventDefault(); prevented = e.defaultPrevented; }); document.body.append(a); a.click();
console.log('preventDefault', prevented, JSON.stringify(location.hash));
```

```text
once 1
preventDefault true ""
```

- `{ once: true }` — the listener fires once and is removed.
- `e.preventDefault()` cancels the default action: following the link did not happen (`location.hash` is empty).
- `e.stopPropagation()` stops the further path: the `outer` listener did not receive the event (in the experiment `stopped true`, `outer-after` was `false`).
- A listener can be removed only with the same function: `removeEventListener('click', () => …)` with a new arrow does not work (in the experiment the counter kept growing). A convenient alternative is `{ signal }` from an `AbortController`: after `abort()` the listener is removed.

## Event delegation

Instead of a listener on each `<li>`, you attach one to the parent and determine the target with `closest`:

```js
const ul = document.getElementById('list'); const hits = [];
ul.addEventListener('click', e => { const el = e.target.closest('li.item'); if (el && ul.contains(el)) hits.push(el.textContent); });
for (const x of ul.querySelectorAll('li')) x.click(); console.log('delegation', hits);
```

```text
delegation ["Two","Three"]
```

Elements added later (like `Three`) are handled without new subscriptions; fewer listeners, less memory.

## fetch

```js
const r404 = await fetch('/nope'); console.log('404', r404.ok, r404.status, await r404.text());
const rj = await fetch('/json'); console.log('json', rj.ok, rj.headers.get('content-type'), await rj.json());
const rp = await fetch('/echo', { method: 'POST', headers: { 'content-type': 'application/json' }, body: JSON.stringify({ a: 1 }) });
console.log('post', await rp.json());
```

```text
404 false 404 not found
json true application/json {"ok":true,"n":1}
post {"method":"POST","ct":"application/json","body":"{\"a\":1}"}
```

- `fetch` returns a promise of a `Response`; **a 404 response does not reject the promise**: check `response.ok` and `status`.
- The body is read with `text()`, `json()`, `blob()`; it cannot be read **twice** (in the experiment a second `text()` threw a `TypeError`).
- A rejection happens only on a network error (`TypeError: Failed to fetch`).
- The method, headers and body go in the second argument; JSON must be serialized by hand.

## Storages

```js
localStorage.clear(); localStorage.setItem('n', 5); localStorage.setItem('o', JSON.stringify({ a: 1 }));
console.log('storage', typeof localStorage.getItem('n'), JSON.parse(localStorage.getItem('o')), localStorage.getItem('missing'), localStorage.length);
```

```text
storage string {"a":1} null 2
```

- `localStorage` stores **only strings** (the number `5` came back as a string), a nonexistent key gives `null`; objects go through `JSON.stringify`/`parse`.
- `localStorage` lives between sessions, `sessionStorage` until the tab is closed; both are tied to an origin (protocol + host + port).
- The data is available to any script of the page: **do not store secrets**.
- Cookies (`document.cookie`) are sent to the server with requests; cookies with the `HttpOnly` flag are not visible to JavaScript (in the experiment after the server set `sid`, it was not in `document.cookie`).

## Beginner mistakes

- ❌ Thinking `fetch` rejects on `404` → ✅ check `response.ok`.
- ❌ `innerHTML` with user text → ✅ `textContent`.
- ❌ Reading a response body twice → ✅ save the result or `response.clone()`.
- ❌ A listener on each list element → ✅ delegation.
- ❌ Expecting `removeEventListener` to remove an anonymous function → ✅ keep the reference or use `{ signal }`.
- ❌ Storing objects in `localStorage` without JSON → ✅ `JSON.stringify`/`parse`.

## Related materials

- Next: [advanced theory](advanced.en.md) — load order, forms, Shadow DOM, CORS, workers
- Earlier: [13 · The event loop and concurrency](../13-event-loop-and-concurrency/basics.en.md#macrotasks-and-microtasks)

## Sources

- [MDN: Document Object Model](https://developer.mozilla.org/en-US/docs/Web/API/Document_Object_Model)
- [MDN: Introduction to events](https://developer.mozilla.org/en-US/docs/Learn_web_development/Core/Scripting/Events)
- [MDN: Event bubbling](https://developer.mozilla.org/en-US/docs/Learn_web_development/Core/Scripting/Event_bubbling)
- [MDN: Fetch API](https://developer.mozilla.org/en-US/docs/Web/API/Fetch_API/Using_Fetch)
- [MDN: Web Storage API](https://developer.mozilla.org/en-US/docs/Web/API/Web_Storage_API)
- [WHATWG DOM: Events](https://dom.spec.whatwg.org/#events)
