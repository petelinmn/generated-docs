---
title: "Браузер: DOM и события: основы"
lang: ru
track: javascript
kind: module
doc: basics
slug: 17-browser-dom-and-events
part: 3
tags: [dom, query-selector, class-list, dataset, events, bubbling, capturing, event-delegation, fetch, local-storage]
versions: "WHATWG DOM и HTML Living Standard; проверено в Chrome 157 (headless)"
status: draft
updated: 2026-11-23
---

# Браузер: DOM и события: основы

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [overview](overview.ru.md) · **1. Основы** · [2. Продвинутый](advanced.ru.md) · [3. Эксперт](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [09 · Модули](../09-modules/overview.ru.md), [12 · Промисы и async/await](../12-promises-and-async-await/overview.ru.md) · **Время:** ~3 ч

Все примеры запускались в Chrome 157 (headless): страницу отдавал локальный сервер Node.js, а скрипт собирал результаты и отправлял их обратно на сервер. Код — в `<script type="module">` ([модуль 09](../09-modules/overview.ru.md)); в консоли браузера он ведёт себя так же. Объекты и массивы в выводе показаны в формате JSON (так их собирал скрипт); консоль браузера оформляет их иначе.

## Зачем это нужно

Любой интерактивный сайт — это чтение и изменение DOM, реакция на события и обмен данными с сервером.

## DOM: дерево узлов

Браузер разбирает HTML в дерево **узлов** (nodes): элементы, текст, комментарии. Корень — `document`. У элемента есть родитель, дети, соседи:

```html
<ul id="list" class="items"><li class="item" data-id="1">One</li><li class="item active" data-id="2">Two</li></ul>
```

- `element.parentElement`, `children`, `nextElementSibling` — навигация по элементам.
- `element.tagName` (`'LI'`), `id`, `className`, `classList`, `dataset` — свойства.
- Атрибуты `data-*` доступны через `dataset` в camelCase: `data-new-key` ↔ `dataset.newKey`.

## Поиск элементов

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

- `querySelector(css)` — первый подходящий элемент или `null`, `querySelectorAll(css)` — **статический** `NodeList`: после добавления элемента в нём по-прежнему `2`.
- `getElementsByClassName` и `getElementsByTagName` возвращают **живой** `HTMLCollection`: он обновился (`3`).
- `closest(css)` поднимается вверх по предкам до подходящего, `matches(css)` проверяет сам элемент.
- `getElementById('id')` быстрее, но ищет только по id.

## Изменение DOM

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

- `classList.add/remove/toggle/contains` управляют классами без ручной работы со строкой.
- `append(node)` вставляет в конец; вставка уже существующего узла **перемещает** его (`One` ушёл в конец).
- `remove()` убирает узел из документа: `isConnected` — `false`, но объект остаётся и его можно вставить снова.
- `createElement` создаёт элемент, `createDocumentFragment` — лёгкий контейнер для пакетной вставки (после `append` фрагмент опустошается).

## textContent и innerHTML

```js
const d = document.createElement('div'); d.innerHTML = '<b>bold</b> & <i>it</i>';
console.log(d.textContent, d.innerHTML, d.children.length);
d.textContent = '<b>not bold</b>'; console.log(d.innerHTML, d.children.length);
```

```text
bold & it <b>bold</b> &amp; <i>it</i> 2
&lt;b&gt;not bold&lt;/b&gt; 0
```

- `innerHTML` **разбирает** строку как разметку: появились два элемента (`<b>`, `<i>`).
- `textContent` вставляет **текст**: `<b>` превратился в `&lt;b&gt;` и не стал элементом.
- Если в строке есть пользовательские данные, вставляйте их через `textContent`: `innerHTML` исполняет разметку и открывает путь к XSS.
- `insertAdjacentHTML('beforeend', html)` вставляет разметку без перезаписи существующих детей.

## События

Слушатель подписывают `addEventListener(тип, функция, параметры)`:

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

Разметка: `#outer > #inner > #btn`. Событие проходит три фазы:

1. **Перехват** (capture) — сверху вниз; слушатели с третьим аргументом `true` (`outer-capture`).
2. **Цель** (target) — сам элемент (`btn`).
3. **Всплытие** (bubble) — снизу вверх (`inner-bubble`, затем `outer-bubble`).

- `e.target` — элемент, где событие произошло; `e.currentTarget` (и `this` в обычной функции) — элемент, на котором висит текущий слушатель.
- Слушатели вызываются **синхронно**: порядок `before`, `handler`, `after` при `btn.click()` (в опыте).

## Управление событием

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

- `{ once: true }` — слушатель срабатывает один раз и снимается.
- `e.preventDefault()` отменяет действие по умолчанию: переход по ссылке не произошёл (`location.hash` пуст).
- `e.stopPropagation()` останавливает дальнейший путь: слушатель `outer` не получил событие (в опыте `stopped true`, `outer-after` — `false`).
- Снять слушатель можно только той же функцией: `removeEventListener('click', () => …)` с новой стрелкой не сработает (в опыте счётчик продолжал расти). Удобная альтернатива — `{ signal }` от `AbortController`: после `abort()` слушатель снят.

## Делегирование событий

Вместо слушателя на каждом `<li>` вешают один на родителя и определяют цель по `closest`:

```js
const ul = document.getElementById('list'); const hits = [];
ul.addEventListener('click', e => { const el = e.target.closest('li.item'); if (el && ul.contains(el)) hits.push(el.textContent); });
for (const x of ul.querySelectorAll('li')) x.click(); console.log('delegation', hits);
```

```text
delegation ["Two","Three"]
```

Элементы, добавленные позже (как `Three`), обрабатываются без новых подписок; слушателей меньше, памяти — тоже.

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

- `fetch` возвращает промис `Response`; **ответ 404 не отклоняет промис**: проверяйте `response.ok` и `status`.
- Тело читают `text()`, `json()`, `blob()`; прочитать **дважды** нельзя (в опыте повторный `text()` бросил `TypeError`).
- Отклонение — только при сетевой ошибке (`TypeError: Failed to fetch`).
- Метод, заголовки и тело задаются вторым аргументом; JSON нужно сериализовать вручную.

## Хранилища

```js
localStorage.clear(); localStorage.setItem('n', 5); localStorage.setItem('o', JSON.stringify({ a: 1 }));
console.log('storage', typeof localStorage.getItem('n'), JSON.parse(localStorage.getItem('o')), localStorage.getItem('missing'), localStorage.length);
```

```text
storage string {"a":1} null 2
```

- `localStorage` хранит **только строки** (число `5` вернулось строкой), несуществующий ключ даёт `null`; объекты — через `JSON.stringify`/`parse`.
- `localStorage` живёт между сессиями, `sessionStorage` — до закрытия вкладки; оба привязаны к источнику (протокол + хост + порт).
- Данные доступны любому скрипту страницы: **не храните секреты**.
- Куки (`document.cookie`) отправляются на сервер с запросами; куки с флагом `HttpOnly` из JavaScript не видны (в опыте после установки `sid` в `document.cookie` его не было).

## Ошибки новичков

- ❌ Считать, что `fetch` отклоняется на `404` → ✅ проверять `response.ok`.
- ❌ `innerHTML` с пользовательским текстом → ✅ `textContent`.
- ❌ Читать тело ответа дважды → ✅ сохранить результат или `response.clone()`.
- ❌ Вешать слушатель на каждый элемент списка → ✅ делегирование.
- ❌ Ждать, что `removeEventListener` снимет анонимную функцию → ✅ сохранять ссылку или `{ signal }`.
- ❌ Хранить объекты в `localStorage` без JSON → ✅ `JSON.stringify`/`parse`.

## Связанные материалы

- Дальше: [продвинутая теория](advanced.ru.md) — порядок загрузки, формы, Shadow DOM, CORS, воркеры
- Ранее: [13 · Цикл событий и параллелизм](../13-event-loop-and-concurrency/basics.ru.md#макрозадачи-и-микрозадачи)

## Источники

- [MDN: Document Object Model](https://developer.mozilla.org/en-US/docs/Web/API/Document_Object_Model)
- [MDN: Introduction to events](https://developer.mozilla.org/en-US/docs/Learn_web_development/Core/Scripting/Events)
- [MDN: Event bubbling](https://developer.mozilla.org/en-US/docs/Learn_web_development/Core/Scripting/Event_bubbling)
- [MDN: Fetch API](https://developer.mozilla.org/en-US/docs/Web/API/Fetch_API/Using_Fetch)
- [MDN: Web Storage API](https://developer.mozilla.org/en-US/docs/Web/API/Web_Storage_API)
- [WHATWG DOM: Events](https://dom.spec.whatwg.org/#events)
