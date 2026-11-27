---
title: "Браузер: DOM и события"
lang: ru
track: javascript
kind: module
doc: overview
slug: 17-browser-dom-and-events
part: 3
levels: [1, 2, 3, 4]
tags: [dom, events, event-bubbling, event-delegation, fetch, web-storage, shadow-dom, custom-elements, cors, web-workers]
versions: "WHATWG DOM и HTML Living Standard; проверено в Chrome 157 (headless)"
status: draft
updated: 2026-11-26
---

# Браузер: DOM и события

🌐 **Русский** · [English](overview.en.md)

## Коротко

В браузере JavaScript работает с документом через DOM (Document Object Model): страница — дерево узлов, которое можно искать, менять и слушать. События (нажатия, ввод, загрузка) проходят по дереву: сначала вниз (перехват), потом вверх (всплытие). Сетевые запросы делает `fetch`, данные на стороне клиента хранят `localStorage` и другие хранилища, а Web API (воркеры, наблюдатели, пользовательские элементы) расширяют возможности страницы.

## Ключевые идеи

- **DOM — дерево;** `querySelector` находит элемент, `createElement` и `append` меняют дерево.
- **`textContent` безопасен, `innerHTML` разбирает разметку:** вставка пользовательского текста через `innerHTML` — основа XSS ([модуль 20](../../overview.ru.md)).
- **Живые и статические коллекции:** `getElementsByClassName` — живая, `querySelectorAll` — статическая.
- **Событие проходит три фазы:** перехват (capture), цель (target), всплытие (bubble).
- **`stopPropagation` останавливает путь, `preventDefault` отменяет действие по умолчанию** (переход по ссылке, отправку формы).
- **Делегирование событий:** один обработчик на родителе и `e.target.closest(...)`.
- **`fetch` не отклоняется на ответах 4xx/5xx:** проверяйте `response.ok`; тело читается один раз.
- **`localStorage` хранит только строки;** объекты сериализуют в JSON.
- **CORS ограничивает запросы между источниками;** без разрешающих заголовков браузер блокирует ответ.
- **Тяжёлое — в воркер;** DOM доступен только из главного потока.

## Минимальный пример

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

Один обработчик на `<ul>` обслуживает все `<li>`, в том числе добавленные позже.

## Когда применять и когда нет

| Задача | Выбор |
|---|---|
| Найти и изменить элемент | `querySelector`, `textContent`, `classList` |
| Много однотипных элементов с реакцией на клик | делегирование событий |
| Вставить HTML из неподтверждённого источника | не `innerHTML`: `textContent` или очистка |
| Запрос к серверу | `fetch` с проверкой `ok` |
| Хранить настройки на клиенте | `localStorage` (строки, до нескольких МБ) |
| Большие структурированные данные | IndexedDB |
| Тяжёлое вычисление в браузере | Web Worker |
| Сложный интерфейс с состоянием | фреймворк (React, Vue, Svelte), см. [TypeScript](../../../typescript/overview.ru.md) |

- ❌ Не работайте с DOM в цикле по одному элементу при больших объёмах: собирайте во фрагмент.
- ❌ Не храните секреты и токены в `localStorage`: они доступны любому скрипту страницы.

## Уровни материала

| Уровень | Что внутри |
|---|---|
| [1. Базовая теория](basics.ru.md) | DOM, поиск и изменение, события, всплытие, делегирование, `fetch`, хранилища |
| [2. Продвинутая теория](advanced.ru.md) | порядок загрузки, формы, пользовательские элементы и Shadow DOM, CORS, воркеры, история, IndexedDB, наблюдатели |
| [3. Экспертная теория](expert.ru.md) | справочник, внутреннее устройство, подвохи, вопросы с собеседований |
| [4. Самопроверка](quiz.ru.md) | квиз и [упражнения](exercises.ru.md) |
| 5. Запускаемые примеры | пока не создан — `/learn javascript 17-browser-dom-and-events level 5` |
| 6. Тесты | пока не создан — `/learn javascript 17-browser-dom-and-events level 6` |

## Связанные материалы

- Пререквизиты: [09 · Модули](../09-modules/overview.ru.md), [12 · Промисы и async/await](../12-promises-and-async-await/overview.ru.md), [13 · Цикл событий и параллелизм](../13-event-loop-and-concurrency/overview.ru.md)
- Дальше: [18 · Метапрограммирование](../18-metaprogramming/overview.ru.md)
- Аналоги в других треках: [TypeScript](../../../typescript/overview.ru.md) (типы DOM, фреймворки), [C#: Blazor и ASP.NET Core](../../../csharp/overview.ru.md) (серверная сторона)
- [React: компоненты и JSX](../../../react/modules/01-components-and-jsx/overview.ru.md)
- [События в React](../../../react/modules/03-state-and-events/overview.ru.md)
- [Формы в React](../../../react/modules/05-forms/overview.ru.md)
