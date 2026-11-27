---
title: "Серверный рендеринг: экспертная теория"
lang: ru
track: react
kind: module
doc: expert
slug: 14-server-rendering
part: 3
tags: [react, server-side-rendering, hydration, server-components, streaming]
versions: "React 19.3"
status: draft
updated: 2026-11-27
---

# Серверный рендеринг: экспертная теория

🌐 **Русский** · [English](expert.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · [2. Продвинутая](advanced.ru.md) · **3. Экспертная** · [4. Квиз](quiz.ru.md)

## Под капотом

Сервер один раз выполняет компоненты и пишет HTML. На клиенте React при рендере проходит по существующему DOM и принимает подходящие узлы вместо создания новых. Границы `Suspense` помечены в HTML комментариями, что позволяет частям приходить и гидратироваться по отдельности.

## Справочник

| API | Среда | Примечания |
|---|---|---|
| `renderToString` | любая | синхронный, `Suspense` ограничен |
| `renderToStaticMarkup` | любая | без гидратации |
| `renderToPipeableStream` | Node.js | `onShellReady`, `onShellError`, `onAllReady`, `onError`, `abort` |
| `renderToReadableStream` | Web Streams | промис потока с `allReady` |
| `prerender` | Web Streams | ждёт все данные; для Node — `prerenderToNodeStream` |
| `hydrateRoot(node, el, options)` | браузер | `onRecoverableError`, `onCaughtError`, `onUncaughtError`, `identifierPrefix`, `formState` |
| `'use client'` | сборщик | граница модулей |
| `'use server'` | сборщик | Server Functions, только `async` |

`useId` даёт совпадающие идентификаторы на обеих сторонах (здесь `_R_0_`); при нескольких корнях задайте одинаковый `identifierPrefix`.

## Подвохи

### renderToString и Suspense

```tsx
renderToString(<Suspense fallback={<i>loading</i>}><LazyThing /></Suspense>);
// the output contains the fallback <i>loading</i> and a note that renderToString
// does not support Suspense; the client renders the content itself
```

### Расхождение в тексте заменяет узлы DOM

```tsx
// server rendered  <p>on server</p>, the client renders <p>on client</p>
hydrateRoot(container, <Where w="client" />, {
  onRecoverableError: error => report(error),
});
// error: Hydration failed because the server rendered text didn't match the client.
// the screen shows "on client", and the <p> is a new DOM node
```

### Расхождение в атрибуте не исправляется

```tsx
// server: <p class="server">x</p>   client: <p className="client">x</p>
// after hydration the DOM still says class="server"; a console error is printed
```

### suppressHydrationWarning оставляет серверный текст

```tsx
<time suppressHydrationWarning>{value}</time>
// server value 1, client value 2: no warning, and the text stays 1
```

### Два рендера с useSyncExternalStore

```tsx
const value = useSyncExternalStore(subscribe, () => 'client', () => 'server');
// server render: "server"; hydration: "server", then a second render with "client"
```

## Компромиссы и альтернативы

| Подход | Первое содержимое | Нагрузка на сервер | Сложность |
|---|---|---|---|
| Только клиент | после JavaScript | низкая | низкая |
| Статическая генерация | сразу | при сборке | средняя, данные устаревают |
| Серверный рендер | быстро | на каждый запрос | выше |
| Streaming | сначала каркас | на каждый запрос | выше |
| Server Components | меньше клиентского кода | на запрос или при сборке | нужен фреймворк |

## Вопросы с собеседований

**1. Что делает гидратация?**

<details><summary>Ответ</summary>

Подключает React и обработчики к HTML с сервера.

</details>

**2. Выполняются ли эффекты при серверном рендере?**

<details><summary>Ответ</summary>

Нет.

</details>

**3. `renderToString` или `renderToPipeableStream`?**

<details><summary>Ответ</summary>

Второй отдаёт потоком и поддерживает `Suspense`; первый синхронный и ограниченный.

</details>

**4. `onShellReady` или `onAllReady`?**

<details><summary>Ответ</summary>

Первый — чтобы отдавать потоком пользователям; второй — когда нужен полный HTML, например для краулеров.

</details>

**5. Назовите причины расхождений.**

<details><summary>Ответ</summary>

Проверки и API браузера в рендере, разные данные, лишние пробелы вокруг HTML корня.

</details>

**6. Подвох: можно ли сменить код ответа после начала потока?**

<details><summary>Ответ</summary>

Нет.

</details>

**7. Что не может Server Component?**

<details><summary>Ответ</summary>

Использовать состояние, эффекты, обработчики и создавать context.

</details>

**8. Подвох: делает ли `'use server'` компонент серверным?**

<details><summary>Ответ</summary>

Нет, она отмечает Server Functions.

</details>

**9. Может ли Client Component отрисовать Server Component?**

<details><summary>Ответ</summary>

Импортировать его как серверный нельзя, но можно получить как `children`.

</details>

**10. Какие props пересекают границу?**

<details><summary>Ответ</summary>

Сериализуемые: примитивы, обычные объекты, массивы, Date, Map, Set, JSX, промисы, Server Functions.

</details>

**11. Почему Server Functions — вопрос безопасности?**

<details><summary>Ответ</summary>

Их может вызвать кто угодно с любыми аргументами.

</details>

**12. Как безопасно вложить данные в страницу для гидратации?**

<details><summary>Ответ</summary>

Сериализовать в JSON и экранировать `<`, чтобы данные не закрыли тег script.

</details>

## Связанные материалы

- [Продвинутая теория](advanced.ru.md)
- [Квиз](quiz.ru.md) и [упражнения](exercises.ru.md)

## Источники

- [React: hydrateRoot](https://react.dev/reference/react-dom/client/hydrateRoot)
- [React: renderToString](https://react.dev/reference/react-dom/server/renderToString)
- [React: renderToPipeableStream](https://react.dev/reference/react-dom/server/renderToPipeableStream)
- [React: renderToReadableStream](https://react.dev/reference/react-dom/server/renderToReadableStream)
- [React: prerender](https://react.dev/reference/react-dom/static/prerender)
- [React: Server Components](https://react.dev/reference/rsc/server-components)
- [React: Server Functions](https://react.dev/reference/rsc/server-functions)
- [React: 'use client'](https://react.dev/reference/rsc/use-client)
- [React: 'use server'](https://react.dev/reference/rsc/use-server)
