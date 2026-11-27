---
title: "Серверный рендеринг: продвинутая теория"
lang: ru
track: react
kind: module
doc: advanced
slug: 14-server-rendering
part: 3
tags: [react, server-side-rendering, hydration, server-components, streaming]
versions: "React 19.3"
status: draft
updated: 2026-11-27
---

# Серверный рендеринг: продвинутая теория

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · **2. Продвинутая** · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

## Streaming

```tsx
const { pipe } = renderToPipeableStream(
  <div><h1>title</h1><Suspense fallback={<i>loading</i>}><Name /></Suspense></div>,
  { onShellReady() { pipe(response); } });
```

Что приходит (скрипты сокращены):

```text
shell ready
chunk 1: <div><h1>title</h1><!--$?--><template id="B:0"></template><i>loading</i><!--/$--></div><script>…</script>
all ready
chunk 2: <div hidden id="S:0"><b>Ann</b></div><script>…</script>
```

Каркас — всё, что вне ожидающих границ. Следующая порция — скрытый HTML и скрипт, переносящий его на место. Для Web Streams (edge-среды, Deno) есть `renderToReadableStream`.

`renderToString` ждать не умеет:

```tsx
renderToString(<Suspense fallback={<i>loading</i>}><LazyThing /></Suspense>);
// the output contains the fallback <i>loading</i> and a note that renderToString
// does not support Suspense; the client renders the content itself
```

## Статическая генерация

`prerender` из `react-dom/static` дожидается всех данных, прежде чем вернуть HTML.

## Расхождения

```tsx
// server rendered  <p>on server</p>, the client renders <p>on client</p>
hydrateRoot(container, <Where w="client" />, {
  onRecoverableError: error => report(error),
});
// error: Hydration failed because the server rendered text didn't match the client.
// the screen shows "on client", and the <p> is a new DOM node
```

```tsx
// server: <p class="server">x</p>   client: <p className="client">x</p>
// after hydration the DOM still says class="server"; a console error is printed
```

Расхождение в тексте стоит повторного рендера на клиенте, а расхождение в атрибутах может так и остаться неверным.

## Значения, отличающиеся на клиенте

```tsx
const value = useSyncExternalStore(subscribe, () => 'client', () => 'server');
// server render: "server"; hydration: "server", then a second render with "client"
```

Для гидратации берётся серверный снимок, затем React рендерит ещё раз с клиентским значением.

## Server Components

> Два листинга ниже следуют документации и не запускались: им нужен фреймворк с совместимым сборщиком.

```tsx
// ProductPage.tsx — a Server Component (no directive)
import { db } from './db';
import { AddToCart } from './AddToCart';

export default async function ProductPage({ id }: { id: string }) {
  const product = await db.products.find(id);
  return <>
    <h1>{product.name}</h1>
    <AddToCart productId={product.id} />
  </>;
}

// AddToCart.tsx — a Client Component
'use client';
export function AddToCart({ productId }: { productId: string }) {
  const [count, setCount] = useState(1);
  /* ... */
}
```

Server Component может быть `async` и читать данные напрямую; состояние, эффекты и обработчики событий ему недоступны. `'use client'` в начале файла делает этот модуль и его импорты клиентским кодом. Props, пересекающие границу, должны быть сериализуемыми. Директивы для самих Server Components нет.

## Server Functions

```tsx
'use server';

export async function addToCart(productId: string, count: number) {
  const user = await currentUser();
  if (!user) throw new Error('unauthorized');
  if (!Number.isInteger(count) || count < 1) throw new Error('bad count');
  await db.cart.add(user.id, productId, count);
}
```

`'use server'` отмечает функции, которые клиент может вызвать по сети. Их аргументы полностью контролируются клиентом.

## Лучшие практики

- Выносите медленные данные за отдельную границу `Suspense`.
- Опускайте `'use client'` как можно ниже по дереву.
- Проверяйте права и данные в каждой Server Function.

## Частые ошибки

- ❌ Понимать `'use server'` как «это Server Component» → ✅ она отмечает Server Functions.
- ❌ Передавать обычную функцию в Client Component → ✅ она не сериализуется.
- ❌ Прятать расхождение через `suppressHydrationWarning` → ✅ устранить причину.

## Связанные материалы

- [Базовая теория](basics.ru.md)
- [Экспертная теория](expert.ru.md)

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
