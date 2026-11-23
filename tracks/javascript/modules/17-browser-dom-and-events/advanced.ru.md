---
title: "Браузер: DOM и события: продвинутый уровень"
lang: ru
track: javascript
kind: module
doc: advanced
slug: 17-browser-dom-and-events
part: 3
tags: [script-loading, forms, form-data, custom-elements, shadow-dom, cors, web-workers, history-api, indexeddb, mutation-observer]
versions: "WHATWG DOM и HTML Living Standard; проверено в Chrome 157 (headless)"
status: draft
updated: 2026-11-23
---

# Браузер: DOM и события: продвинутый уровень

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [overview](overview.ru.md) · [1. Основы](basics.ru.md) · **2. Продвинутый** · [3. Эксперт](expert.ru.md) · [4. Квиз](quiz.ru.md)

Опирается на [базовую теорию](basics.ru.md). Все примеры запускались в Chrome 157 (headless) со страницами, которые отдавал локальный сервер Node.js. Объекты и массивы в выводе показаны в формате JSON, как их собирал скрипт.

## Порядок загрузки скриптов

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

- Обычный скрипт выполняется **сразу при разборе**, блокируя страницу; в `<head>` у него ещё нет `document.body` (`body? false`).
- Атрибут `defer` работает **только у скриптов с `src`**: у встроенного он игнорируется, и скрипт выполнился немедленно (`defer script` до скрипта в теле). Для внешних файлов `defer` откладывает выполнение до конца разбора.
- Скрипт `type="module"` откладывается автоматически (как `defer`): он выполнился после `readyState = interactive` и **до** `DOMContentLoaded`.
- `DOMContentLoaded` — дерево готово; `load` — загружены и ресурсы (картинки, стили). `readyState` проходит `loading` → `interactive` → `complete`.
- Модули выполняются один раз ([модуль 09](../09-modules/basics.ru.md)); `await` на верхнем уровне допустим.

## Формы

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

- Событие `submit` приходит на **форму**, а не на кнопку; `e.preventDefault()` отменяет перезагрузку страницы.
- `new FormData(form)` собирает поля: чекбокс даёт `'on'`, снятый чекбокс в данные не попадает; значения — **строки** (`age` — `'30'`, число — `valueAsNumber`).
- `form.elements` — доступ к полям по имени; `checkValidity()` проверяет ограничения (`required`, `min`, …).
- `form.requestSubmit()` запускает отправку **с проверкой и событием** (`submitter` пуст); `form.submit()` событие `submit` не вызывает.

## События ввода и фокуса

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

- Присвоение `value` из кода **не вызывает** `input` и `change`; их порождают пользователь или `dispatchEvent`.
- События, созданные скриптом, имеют `isTrusted === false`.
- `input` приходит при каждом изменении, `change` — по завершении (потеря фокуса, `Enter`).
- `focus` и `blur` **не всплывают** (пойманы только в фазе перехвата); для делегирования используют `focusin`/`focusout`, которые всплывают. Так же не всплывают `mouseenter`/`mouseleave`.

## Пользовательские события

```js
let detail; document.addEventListener('app:ping', e => detail = e.detail);
document.dispatchEvent(new CustomEvent('app:ping', { detail: { n: 1 } })); console.log(detail);
```

```text
{"n":1}
```

`CustomEvent` переносит данные в `detail`; чтобы событие всплывало, укажите `{ bubbles: true }`.

## Пользовательские элементы и Shadow DOM

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

- Имя пользовательского элемента **обязано содержать дефис** (`my-card`); класс наследует `HTMLElement` и регистрируется `customElements.define`.
- Жизненный цикл: `connectedCallback` (вставлен в документ), `disconnectedCallback`, `attributeChangedCallback` (для атрибутов из `observedAttributes`).
- **Shadow DOM** изолирует разметку и стили: `p` внутри тени есть у `card.shadowRoot`, но `document.querySelectorAll('my-card p')` его не находит (`0`), а стиль `p{color:red}` не утекает наружу.
- `<slot>` показывает «светлых» детей элемента внутри тени.
- Событие изнутри тени **перенацеливается** (retargeting): снаружи `e.target` — сам `MY-CARD`, а `e.composedPath()` содержит и внутренние узлы (в опыте `retargeted MY-CARD composedPath>3:true`).

## CORS

Браузер запрещает JavaScript читать ответ **другого источника** (протокол + хост + порт), если сервер не разрешил это заголовком `Access-Control-Allow-Origin`:

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

- Без заголовка `fetch` отклоняется `TypeError` (причина видна только в консоли браузера).
- Сервер с `Access-Control-Allow-Origin: *` разрешает чтение.
- `mode: 'no-cors'` отправляет запрос, но ответ «непрозрачный» (`opaque`, статус `0`): его нельзя прочитать. Это не способ обойти CORS, а режим для отправки данных «вслепую».
- Запросы с нестандартными методами или заголовками предваряются **предзапросом** `OPTIONS` (preflight).
- CORS защищает **пользователя** в браузере; серверные запросы (Node.js, curl) ему не подчиняются.

> ⚠️ Проверить: предзапрос `OPTIONS`, `credentials: 'include'` и ограничения `SameSite` для кук в этом материале не запускались.

## Веб-воркеры

Воркер выполняется в отдельном потоке без доступа к DOM; общение — сообщениями ([модуль 13](../13-event-loop-and-concurrency/advanced.ru.md#рабочие-потоки-worker-threads)):

```js
const blob = new Blob(['onmessage = e => postMessage(e.data * 2)'], { type: 'text/javascript' });
const w = new Worker(URL.createObjectURL(blob));
console.log('worker', await new Promise(r => { w.onmessage = e => r(e.data); w.postMessage(21); })); w.terminate();
```

```text
worker 42
```

Данные копируются (structured clone), буферы можно передавать (`transfer`). Воркер из файла: `new Worker('worker.js', { type: 'module' })`.

## История и адрес

```js
history.pushState({ p: 1 }, '', '/page2?x=1'); console.log(location.pathname, location.search, history.state);
history.replaceState(null, '', '/');
let pop = new Promise(r => addEventListener('hashchange', r, { once: true })); location.hash = 'abc'; await pop; console.log(location.hash);
```

```text
/page2 ?x=1 {"p":1}
#abc
```

- `pushState` меняет адрес и добавляет запись в историю **без перезагрузки**: основа маршрутизации одностраничных приложений; `replaceState` заменяет текущую запись.
- Событие `popstate` приходит при переходах «назад/вперёд»; `hashchange` — при смене `#`.

## BroadcastChannel и IndexedDB

```js
const bc1 = new BroadcastChannel('c'), bc2 = new BroadcastChannel('c');
console.log('broadcast', await new Promise(r => { bc2.onmessage = e => r(e.data); bc1.postMessage({ hi: 1 }); }));
```

```text
broadcast {"hi":1}
```

- `BroadcastChannel` передаёт сообщения между вкладками и воркерами одного источника.
- **IndexedDB** — асинхронное хранилище объектов (в опыте: открыть базу, создать хранилище с `keyPath: 'id'`, записать `{ id: 1, v: 'a' }` и прочитать обратно). Подходит для больших данных, оффлайн-режима и индексов; API на событиях неудобен, обычно используют обёртки.

## Наблюдатели и порядок задач

```js
const t0 = []; setTimeout(() => t0.push('timeout'), 0); requestAnimationFrame(() => t0.push('raf')); Promise.resolve().then(() => t0.push('micro'));
const mo = new MutationObserver(() => t0.push('mutation')); mo.observe(document.body, { childList: true });
document.body.append(document.createElement('hr')); t0.push('sync');
await new Promise(r => setTimeout(r, 100)); console.log(t0);
```

```text
[ "sync", "micro", "mutation", "raf", "timeout" ]
```

- Синхронный код выполняется первым, затем **микрозадачи**: `then` и колбэк `MutationObserver` (он тоже микрозадача).
- Порядок `requestAnimationFrame` и `setTimeout(…, 0)` зависит от момента кадра: не рассчитывайте на него.
- `requestAnimationFrame` вызывается перед отрисовкой кадра: подходит для анимаций.
- `IntersectionObserver` (видимость элемента) и `ResizeObserver` (изменение размера) доступны как функции (в опыте `function`).
- Чтение геометрии (`offsetWidth`, `getComputedStyle`) **заставляет** браузер пересчитать раскладку (в опыте `100`, `block` сразу после записи стиля): чередование записей и чтений в цикле тормозит страницу.

Другие Web API (проверено наличие): `matchMedia`, `requestIdleCallback`, `navigator.clipboard`, `Notification`, `navigator.serviceWorker`, `WebSocket`, `EventSource`; многие из них требуют **защищённого контекста** (`isSecureContext`; `http://localhost` считается защищённым).

## Лучшие практики

- Делегируйте события; снимайте слушатели (`{ signal }`) при удалении компонентов.
- Скрипты — модулями или с `defer`.
- Вставляйте текст через `textContent`, разметку из чужих данных — только очищенную.
- Проверяйте `response.ok` и обрабатывайте сетевые ошибки.
- Тяжёлое — в воркер; анимации — через `requestAnimationFrame`.
- Не смешивайте чтения и записи DOM в цикле: сначала читайте, потом пишите.
- Секреты не храните в `localStorage`.

## Частые ошибки

- ❌ `defer` у встроенного скрипта → ✅ внешний файл или `type="module"`.
- ❌ Ждать `input` после `input.value = …` → ✅ вызвать `dispatchEvent` или обработать значение напрямую.
- ❌ `focus` для делегирования → ✅ `focusin`.
- ❌ Обходить CORS через `no-cors` → ✅ настроить заголовки на сервере.
- ❌ Обращаться к `document` из воркера → ✅ передавать данные сообщениями.
- ❌ `form.submit()` и ждать `submit` → ✅ `requestSubmit()`.

## Связанные материалы

- Дальше: [экспертная теория](expert.ru.md)
- Рабочие потоки и очередь задач: [13 · Цикл событий и параллелизм](../13-event-loop-and-concurrency/advanced.ru.md)
- Безопасность браузера (XSS, CSP): модуль 20 дорожной карты ([трек](../../overview.ru.md))

## Источники

- [MDN: `<script>`: async и defer](https://developer.mozilla.org/en-US/docs/Web/HTML/Reference/Elements/script#defer)
- [MDN: Custom elements](https://developer.mozilla.org/en-US/docs/Web/API/Web_components/Using_custom_elements)
- [MDN: Using shadow DOM](https://developer.mozilla.org/en-US/docs/Web/API/Web_components/Using_shadow_DOM)
- [MDN: Cross-Origin Resource Sharing (CORS)](https://developer.mozilla.org/en-US/docs/Web/HTTP/Guides/CORS)
- [MDN: Web Workers API](https://developer.mozilla.org/en-US/docs/Web/API/Web_Workers_API)
- [MDN: History API](https://developer.mozilla.org/en-US/docs/Web/API/History_API)
- [MDN: IndexedDB API](https://developer.mozilla.org/en-US/docs/Web/API/IndexedDB_API)
- [MDN: MutationObserver](https://developer.mozilla.org/en-US/docs/Web/API/MutationObserver)
