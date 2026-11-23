---
title: "Браузер: DOM и события: экспертный уровень"
lang: ru
track: javascript
kind: module
doc: expert
slug: 17-browser-dom-and-events
part: 3
tags: [dom, event-dispatch, event-loop-rendering, live-collections, cors, storage-limits, shadow-dom, secure-context]
versions: "WHATWG DOM и HTML Living Standard; проверено в Chrome 157 (headless)"
status: draft
updated: 2026-11-23
---

# Браузер: DOM и события: экспертный уровень

🌐 **Русский** · [English](expert.en.md)

**Уровни:** [overview](overview.ru.md) · [1. Основы](basics.ru.md) · [2. Продвинутый](advanced.ru.md) · **3. Эксперт** · [4. Квиз](quiz.ru.md)

Опирается на [базовую](basics.ru.md) и [продвинутую](advanced.ru.md) теорию. Результаты, помеченные «в опыте», получены в Chrome 157 (headless).

## Справочник

### Опции `addEventListener`

| Опция | Смысл |
|---|---|
| `capture: true` (или третий аргумент `true`) | слушать в фазе перехвата |
| `once: true` | вызвать один раз и снять (в опыте `once 1`) |
| `passive: true` | обещание не вызывать `preventDefault` (ускоряет прокрутку) |
| `signal` | `AbortSignal`: `abort()` снимает слушатель (в опыте после `abort()` вызовов нет) |

### Свойства события

| Свойство | Значение |
|---|---|
| `target` / `currentTarget` | где произошло / на ком висит слушатель |
| `bubbles`, `cancelable` | всплывает ли, можно ли отменить |
| `defaultPrevented` | вызвали ли `preventDefault()` |
| `isTrusted` | `true` для действий пользователя, `false` для созданных скриптом (в опыте `false`) |
| `composedPath()` | путь события, включая узлы внутри открытых теней |
| `stopPropagation()` / `stopImmediatePropagation()` | остановить путь / ещё и остальные слушатели на этом элементе |

### Поведение типов событий

| Событие | Всплывает | Примечание |
|---|---|---|
| `click`, `input`, `change`, `submit`, `keydown` | да | `submit` — на форме |
| `focus`, `blur` | **нет** | `focusin`/`focusout` всплывают (в опыте `focus-capture`, `focusin`) |
| `mouseenter`, `mouseleave` | нет | `mouseover`/`mouseout` всплывают |
| `DOMContentLoaded`, `load` | — | слушают на `document` и `window`; порядок `DOMContentLoaded` → `load` (в опыте) |

### Хранилища

| Хранилище | Время жизни | Отправляется на сервер | Доступ из JS | Тип данных |
|---|---|---|---|---|
| `localStorage` | пока не очистят | нет | да | строки |
| `sessionStorage` | вкладка | нет | да | строки |
| Куки | по `Expires`/`Max-Age` | **да, с каждым запросом** | не при `HttpOnly` (в опыте не видна) | строки, малый размер |
| IndexedDB | пока не очистят | нет | да (асинхронно) | структурированные объекты |

> ⚠️ Проверить: лимиты размера (`localStorage` порядка нескольких МБ, куки порядка 4 КБ), правила вытеснения данных и режим приватного просмотра в этом материале не проверялись.

### Параметры `fetch`

| Параметр | Назначение |
|---|---|
| `method`, `headers`, `body` | запрос |
| `signal` | отмена / `AbortSignal.timeout(ms)` (в опыте `TimeoutError`) |
| `mode` | `cors` (по умолчанию), `same-origin`, `no-cors` (непрозрачный ответ) |
| `credentials` | `same-origin` (по умолчанию), `include`, `omit` — куки |
| `cache`, `redirect`, `referrer` | политики |

Методы ответа: `json()`, `text()`, `blob()`, `arrayBuffer()`, `formData()`, `clone()`; `ok`, `status`, `headers`, `type`.

## Внутреннее устройство

- **Разбор HTML.** Браузер разбирает документ и достраивает DOM по мере поступления данных. Обычный скрипт останавливает разбор (в опыте в `<head>` нет `document.body`); `defer` и модули выполняются после разбора, но до `DOMContentLoaded` (в опыте порядок `interactive` → `module script` → `DOMContentLoaded`).
- **Три фазы события.** Путь от `window` к цели строится заранее: сначала слушатели перехвата сверху вниз, затем цель, затем всплытие снизу вверх (в опыте `outer-capture`, `btn-target`, `inner-bubble`, `outer-bubble`). Слушатели вызываются синхронно в порядке подписки.
- **Живые и статические списки.** `HTMLCollection` (`getElementsBy*`, `children`) следит за деревом, `NodeList` из `querySelectorAll` — снимок на момент запроса (в опыте `2` и `3` после вставки).
- **Цикл событий и отрисовка.** Задача выполняется, затем очередь микрозадач; затем (примерно 60 раз в секунду) браузер может выполнить колбэки `requestAnimationFrame`, пересчитать раскладку, перерисовать. Поэтому длинная синхронная работа «замораживает» страницу ([модуль 13](../13-event-loop-and-concurrency/expert.ru.md)).
- **Принудительная раскладка.** Чтение `offsetWidth`, `getBoundingClientRect()`, `getComputedStyle(...)` после изменения стиля заставляет браузер пересчитать раскладку немедленно (в опыте чтение сразу вернуло `100`). Чередование записи и чтения в цикле даёт повторные пересчёты.
- **Shadow DOM.** У элемента есть собственное поддерево; селекторы документа его не видят (`document.querySelectorAll('my-card p')` — `0`), события перенацеливаются на хозяина.
- **Модель безопасности.** Источник = протокол + хост + порт; «одинаковое происхождение» разрешает чтение; CORS расширяет это сервером. `HttpOnly`-куки скрыты от JavaScript, но уходят на сервер.

> ⚠️ Проверить: частота кадров, точный порядок «задача → микрозадачи → rAF → раскладка → отрисовка» и поведение `requestAnimationFrame` в фоновых вкладках в опытах не проверялись; порядок rAF и таймера нулевой задержки в опыте не стабилен.

## Подвохи

### 1. `defer` у встроенного скрипта

Игнорируется: скрипт выполняется сразу (в опыте `defer script` напечатан до скрипта в теле).

### 2. Статический и живой списки

`querySelectorAll` не обновляется (`2`), `getElementsByClassName` обновляется (`3`).

### 3. `innerHTML` исполняет разметку

Пользовательский текст в `innerHTML` — путь к XSS; `textContent` вставляет текст (в опыте `&lt;b&gt;`).

### 4. Анонимная функция не снимается

`removeEventListener('click', () => …)` не снимает слушатель (в опыте счётчик вырос).

### 5. `focus` не всплывает

Делегирование — через `focusin`.

### 6. Запись `value` не порождает событий

`input.value = …` не вызывает `input`/`change` (в опыте `[]`).

### 7. `fetch` и статусы

`404` не отклоняет промис (в опыте `ok false`); отклоняется только сетевая ошибка (`TypeError`).

### 8. Тело читается один раз

Повторный `text()` бросает `TypeError`.

### 9. `localStorage` — только строки

`setItem('n', 5)` → `'5'` (в опыте `string`).

### 10. `no-cors` не «обходит» CORS

Ответ непрозрачный: `type: 'opaque'`, `status: 0`, `ok: false`.

### 11. `form.submit()` не вызывает событие

Используйте `requestSubmit()`.

## Компромиссы и альтернативы

| Подход | Плюсы | Минусы |
|---|---|---|
| Чистый DOM | без зависимостей, быстро стартует | много ручного кода для сложного интерфейса |
| Шаблоны и `<template>` | быстрые клоны разметки | ручная привязка данных |
| Веб-компоненты | стандарт, инкапсуляция | многословно, свои нюансы с формами и SSR |
| Фреймворки (React, Vue, Svelte) | компоненты, реактивность | сборка, зависимость; не запускались |
| `fetch` | стандарт, промисы | нет таймаута и повторов «из коробки» |
| `XMLHttpRequest` | старые среды | устаревший API |
| `localStorage` | просто | синхронно, строки, нет защиты |
| IndexedDB | объём, индексы | сложный API |

## Вопросы с собеседований

<details><summary>1. Что такое всплытие и перехват событий?</summary>

Событие идёт сверху вниз (перехват), попадает в цель и поднимается вверх (всплытие). Слушатели по умолчанию работают на всплытии.

→ [Базовая теория](basics.ru.md#события)

</details>

<details><summary>2. Чем <code>target</code> отличается от <code>currentTarget</code>?</summary>

`target` — элемент, где событие возникло; `currentTarget` — элемент, на котором сработал слушатель.

→ [Базовая теория](basics.ru.md#события)

</details>

<details><summary>3. Что такое делегирование событий и зачем оно?</summary>

Один слушатель на родителе и `e.target.closest(...)`: меньше слушателей, работает для элементов, добавленных позже.

→ [Базовая теория](basics.ru.md#делегирование-событий)

</details>

<details><summary>4. Подвох: чем <code>textContent</code> отличается от <code>innerHTML</code>?</summary>

`innerHTML` разбирает разметку (риск XSS), `textContent` вставляет текст.

→ [Подвох 3](#3-innerhtml-исполняет-разметку)

</details>

<details><summary>5. Подвох: <code>fetch</code> получил <code>404</code>. Промис отклонён?</summary>

Нет. Нужно проверять `response.ok`.

→ [Подвох 7](#7-fetch-и-статусы)

</details>

<details><summary>6. В чём разница между <code>localStorage</code>, <code>sessionStorage</code> и куками?</summary>

Время жизни и отправка на сервер: куки уходят с каждым запросом, остальные — нет; `sessionStorage` — только вкладка.

→ [Справочник](#хранилища)

</details>

<details><summary>7. Что такое CORS и от чего он защищает?</summary>

Политика, по которой сервер разрешает чтение ответа другому источнику; защищает пользователя от чтения его данных чужим сайтом.

→ [Продвинутая теория](advanced.ru.md#cors)

</details>

<details><summary>8. Подвох: сработает ли <code>defer</code> у встроенного скрипта?</summary>

Нет, атрибут действует только у скриптов с `src`.

→ [Подвох 1](#1-defer-у-встроенного-скрипта)

</details>

<details><summary>9. Чем живая коллекция отличается от статической?</summary>

Живая (`getElementsBy*`) отражает изменения дерева, статическая (`querySelectorAll`) — снимок.

→ [Подвох 2](#2-статический-и-живой-списки)

</details>

<details><summary>10. Зачем нужен Shadow DOM?</summary>

Изоляция разметки и стилей внутри компонента; внешние селекторы не проникают внутрь.

→ [Продвинутая теория](advanced.ru.md#пользовательские-элементы-и-shadow-dom)

</details>

<details><summary>11. Что такое принудительная раскладка?</summary>

Чтение геометрии после записи стилей заставляет браузер немедленно пересчитать раскладку; в цикле это тормозит.

→ [Внутреннее устройство](#внутреннее-устройство)

</details>

<details><summary>12. Как выполнить тяжёлую работу, не замораживая страницу?</summary>

Web Worker (без DOM, с сообщениями) или дробление на порции.

→ [Продвинутая теория](advanced.ru.md#веб-воркеры)

</details>

## Шпаргалка

```text
document.querySelector(css) / querySelectorAll (статический) / getElementsByClassName (живой)
el.append(...) / remove() / classList / dataset / textContent (безопасно) / innerHTML (разметка)
el.addEventListener(type, fn, { capture, once, passive, signal }); e.target / currentTarget / preventDefault / stopPropagation
delegation: parent.addEventListener('click', e => e.target.closest('.item'))
const r = await fetch(url, { method, headers, body, signal }); if (!r.ok) throw …; await r.json()
localStorage.setItem(k, JSON.stringify(v)); JSON.parse(localStorage.getItem(k))
new Worker(url); worker.postMessage(x); form.requestSubmit(); new FormData(form)
```

- Событие: перехват → цель → всплытие.
- `fetch` не отклоняется на `4xx`/`5xx`.
- Секреты — не в `localStorage`.

## Связанные материалы

- Предыдущий уровень: [продвинутая теория](advanced.ru.md)
- Цикл событий и отрисовка: [13 · Цикл событий и параллелизм](../13-event-loop-and-concurrency/expert.ru.md)
- Модули в браузере: [09 · Модули](../09-modules/expert.ru.md)
- Типы DOM в TypeScript: [TypeScript](../../../typescript/overview.ru.md)

## Источники

- [WHATWG DOM Standard: Events](https://dom.spec.whatwg.org/#events)
- [WHATWG HTML Standard: Event loops](https://html.spec.whatwg.org/multipage/webappapis.html#event-loops)
- [WHATWG Fetch Standard](https://fetch.spec.whatwg.org/)
- [MDN: Event](https://developer.mozilla.org/en-US/docs/Web/API/Event)
- [MDN: Same-origin policy](https://developer.mozilla.org/en-US/docs/Web/Security/Same-origin_policy)
- [MDN: Storage quotas and eviction criteria](https://developer.mozilla.org/en-US/docs/Web/API/Storage_API/Storage_quotas_and_eviction_criteria)
