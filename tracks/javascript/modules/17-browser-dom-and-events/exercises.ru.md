---
title: "Упражнения: браузер — DOM и события"
lang: ru
track: javascript
kind: module
doc: exercises
slug: 17-browser-dom-and-events
part: 3
tags: [dom, events, delegation, form, local-storage, fetch]
status: draft
updated: 2026-11-23
---

# Упражнения: браузер — DOM и события

🌐 **Русский** · [English](exercises.en.md)

> Решайте самостоятельно, подсказки и решения под спойлерами. Страница: `<ul id="todo"></ul><form id="add"><input name="title"><button>Add</button></form>`; код — в `<script type="module">`. Решения запускались в Chrome 157 (headless); объекты в выводе показаны в формате JSON.

## 1. 🟢 Безопасная отрисовка списка

**Задача:** функция `render(list, titles)` заменяет содержимое `<ul>` элементами `<li>` с текстом из массива. Передайте `['a', '<b>b</b>']` и убедитесь, что разметка не исполняется.

**Ожидаемый результат:** `2 <li>a</li><li>&lt;b&gt;b&lt;/b&gt;</li>`.

<details><summary>Подсказка</summary>

`list.replaceChildren(...)` и `textContent`.

</details>

<details><summary>Решение</summary>

```js
function render(list, titles) { list.replaceChildren(...titles.map(t => Object.assign(document.createElement('li'), { textContent: t }))); }
const todo = document.getElementById('todo'); render(todo, ['a', '<b>b</b>']);
console.log(todo.children.length, todo.innerHTML);
```

```text
2 <li>a</li><li>&lt;b&gt;b&lt;/b&gt;</li>
```

</details>

## 2. 🟢 Делегирование

**Задача:** повесьте один слушатель клика на список и собирайте тексты кликнутых `<li>`. Перерисуйте список (`['x', 'y']`) и «кликните» по каждому элементу; слушатель не должен добавляться заново.

**Ожидаемый результат:** `["x","y"]`.

<details><summary>Подсказка</summary>

`todo.addEventListener('click', e => e.target.closest('li'))`; клик из кода — `li.click()`.

</details>

<details><summary>Решение</summary>

```js
const clicked = [];
todo.addEventListener('click', e => { const li = e.target.closest('li'); if (li) clicked.push(li.textContent); });
render(todo, ['x', 'y']); [...todo.children].forEach(li => li.click());
console.log(clicked);
```

```text
["x","y"]
```

Новые элементы обработаны тем же слушателем.

</details>

## 3. 🟡 Добавление через форму

**Задача:** по событию `submit` формы отмените перезагрузку, возьмите значение поля `title` (без пробелов по краям), добавьте `<li>` и очистите форму. Пустое значение игнорируйте. Проверьте: значение `'  new item '`, затем пустое поле.

**Ожидаемый результат:** три элемента в списке (`x`, `y`, `new item`), форма пуста.

<details><summary>Подсказка</summary>

`new FormData(form).get('title').trim()`, `form.reset()`, а для проверки — `form.requestSubmit()`.

</details>

<details><summary>Решение</summary>

```js
const form = document.getElementById('add');
form.addEventListener('submit', e => { e.preventDefault(); const t = new FormData(form).get('title').trim(); if (!t) return; todo.append(Object.assign(document.createElement('li'), { textContent: t })); form.reset(); });
form.elements.title.value = '  new item '; form.requestSubmit(); form.requestSubmit();
console.log(todo.children.length, todo.lastElementChild.textContent, JSON.stringify(form.elements.title.value));
```

```text
3 new item ""
```

Второй `requestSubmit()` с пустым полем ничего не добавил.

</details>

## 4. 🟡 Сохранение в `localStorage`

**Задача:** напишите `save()` (JSON-массив текстов элементов) и `load()` (массив или `[]`, если ключа нет). Сохраните, очистите список, загрузите и перерисуйте.

**Ожидаемый результат:** `["x","y","new item"] 3`.

<details><summary>Подсказка</summary>

`JSON.stringify`/`JSON.parse`, `localStorage.getItem(KEY) ?? '[]'`.

</details>

<details><summary>Решение</summary>

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

## 5. 🔴 `fetch` с проверкой и таймаутом

**Задача:** напишите `getJson(url, ms)`: запрос с таймаутом через `AbortSignal.timeout(ms)`, ошибка `HTTP <код>` для ответов не из диапазона 2xx, разбор JSON. Проверьте успешный ответ, `404` и медленный сервер (таймаут 50 мс).

**Ожидаемый результат:** `{"ok":true,"n":1}`, `HTTP 404`, `TimeoutError`.

<details><summary>Подсказка</summary>

`fetch(url, { signal: AbortSignal.timeout(ms) })`, `if (!r.ok) throw new Error('HTTP ' + r.status)`.

</details>

<details><summary>Решение</summary>

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

Адреса `/json`, `/nope`, `/slow` — конечные точки тестового сервера (быстрый JSON, `404`, ответ через 500 мс).

</details>
