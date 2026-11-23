---
title: "Exercises: browser — the DOM and events"
lang: en
track: javascript
kind: module
doc: exercises
slug: 17-browser-dom-and-events
part: 3
tags: [dom, events, delegation, form, local-storage, fetch]
status: draft
updated: 2026-11-23
---

# Exercises: browser — the DOM and events

🌐 [Русский](exercises.ru.md) · **English**

> Solve on your own; hints and solutions are under spoilers. The page: `<ul id="todo"></ul><form id="add"><input name="title"><button>Add</button></form>`; the code is in `<script type="module">`. The solutions were run in Chrome 157 (headless); objects in the output are shown in JSON format.

## 1. 🟢 Safe list rendering

**Task:** the function `render(list, titles)` replaces the contents of a `<ul>` with `<li>` elements with text from the array. Pass `['a', '<b>b</b>']` and make sure the markup is not executed.

**Expected result:** `2 <li>a</li><li>&lt;b&gt;b&lt;/b&gt;</li>`.

<details><summary>Hint</summary>

`list.replaceChildren(...)` and `textContent`.

</details>

<details><summary>Solution</summary>

```js
function render(list, titles) { list.replaceChildren(...titles.map(t => Object.assign(document.createElement('li'), { textContent: t }))); }
const todo = document.getElementById('todo'); render(todo, ['a', '<b>b</b>']);
console.log(todo.children.length, todo.innerHTML);
```

```text
2 <li>a</li><li>&lt;b&gt;b&lt;/b&gt;</li>
```

</details>

## 2. 🟢 Delegation

**Task:** attach one click listener to the list and collect the texts of the clicked `<li>`s. Redraw the list (`['x', 'y']`) and "click" every element; the listener must not be added again.

**Expected result:** `["x","y"]`.

<details><summary>Hint</summary>

`todo.addEventListener('click', e => e.target.closest('li'))`; a click from code is `li.click()`.

</details>

<details><summary>Solution</summary>

```js
const clicked = [];
todo.addEventListener('click', e => { const li = e.target.closest('li'); if (li) clicked.push(li.textContent); });
render(todo, ['x', 'y']); [...todo.children].forEach(li => li.click());
console.log(clicked);
```

```text
["x","y"]
```

The new elements are handled by the same listener.

</details>

## 3. 🟡 Adding through a form

**Task:** on the form's `submit` event cancel the reload, take the value of the `title` field (trimmed), add an `<li>` and clear the form. Ignore an empty value. Check: the value `'  new item '`, then an empty field.

**Expected result:** three elements in the list (`x`, `y`, `new item`), the form is empty.

<details><summary>Hint</summary>

`new FormData(form).get('title').trim()`, `form.reset()`, and for the check — `form.requestSubmit()`.

</details>

<details><summary>Solution</summary>

```js
const form = document.getElementById('add');
form.addEventListener('submit', e => { e.preventDefault(); const t = new FormData(form).get('title').trim(); if (!t) return; todo.append(Object.assign(document.createElement('li'), { textContent: t })); form.reset(); });
form.elements.title.value = '  new item '; form.requestSubmit(); form.requestSubmit();
console.log(todo.children.length, todo.lastElementChild.textContent, JSON.stringify(form.elements.title.value));
```

```text
3 new item ""
```

The second `requestSubmit()` with an empty field added nothing.

</details>

## 4. 🟡 Saving to `localStorage`

**Task:** write `save()` (a JSON array of the elements' texts) and `load()` (the array, or `[]` if there is no key). Save, clear the list, load and redraw.

**Expected result:** `["x","y","new item"] 3`.

<details><summary>Hint</summary>

`JSON.stringify`/`JSON.parse`, `localStorage.getItem(KEY) ?? '[]'`.

</details>

<details><summary>Solution</summary>

```js
const KEY = 'todo-v1';
const save = () => localStorage.setItem(KEY, JSON.stringify([...todo.children].map(li => li.textContent)));
const load = () => JSON.parse(localStorage.getItem(KEY) ?? '[]');
save(); todo.replaceChildren(); render(todo, load());
console.log(load(), todo.children.length);
```

```text
["x","y","new item"] 3
```

</details>

## 5. 🔴 `fetch` with a check and a timeout

**Task:** write `getJson(url, ms)`: a request with a timeout via `AbortSignal.timeout(ms)`, an `HTTP <code>` error for non-2xx responses, JSON parsing. Check a successful response, a `404` and a slow server (a 50 ms timeout).

**Expected result:** `{"ok":true,"n":1}`, `HTTP 404`, `TimeoutError`.

<details><summary>Hint</summary>

`fetch(url, { signal: AbortSignal.timeout(ms) })`, `if (!r.ok) throw new Error('HTTP ' + r.status)`.

</details>

<details><summary>Solution</summary>

```js
async function getJson(url, ms) { const r = await fetch(url, { signal: AbortSignal.timeout(ms) }); if (!r.ok) throw new Error('HTTP ' + r.status); return r.json(); }
console.log(await getJson('/json', 1000));
console.log(await getJson('/nope', 1000).then(() => 'ok', e => e.message));
console.log(await getJson('/slow', 50).then(() => 'ok', e => e.name));
```

```text
{"ok":true,"n":1}
HTTP 404
TimeoutError
```

The addresses `/json`, `/nope`, `/slow` are endpoints of the test server (a fast JSON, a `404`, a response after 500 ms).

</details>
