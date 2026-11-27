---
title: "Server rendering: advanced"
lang: en
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

# Server rendering: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

## Streaming

```tsx
const { pipe } = renderToPipeableStream(
  <div><h1>title</h1><Suspense fallback={<i>loading</i>}><Name /></Suspense></div>,
  { onShellReady() { pipe(response); } });
```

What arrives (scripts shortened):

```text
shell ready
chunk 1: <div><h1>title</h1><!--$?--><template id="B:0"></template><i>loading</i><!--/$--></div><script>…</script>
all ready
chunk 2: <div hidden id="S:0"><b>Ann</b></div><script>…</script>
```

The shell is everything outside the pending boundaries. The later chunk is hidden HTML plus a script that moves it into place. For Web Streams (edge runtimes, Deno) use `renderToReadableStream`.

`renderToString` cannot wait:

```tsx
renderToString(<Suspense fallback={<i>loading</i>}><LazyThing /></Suspense>);
// the output contains the fallback <i>loading</i> and a note that renderToString
// does not support Suspense; the client renders the content itself
```

## Static generation

`prerender` from `react-dom/static` waits for all data before returning the HTML.

## Mismatches

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

So text mismatches cost a client re-render, and attribute mismatches may stay wrong.

## Values that differ on the client

```tsx
const value = useSyncExternalStore(subscribe, () => 'client', () => 'server');
// server render: "server"; hydration: "server", then a second render with "client"
```

The server snapshot is used for hydration, then React renders again with the client value.

## Server Components

> The two listings below follow the documentation and were not run: they need a framework with a compatible bundler.

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

A Server Component may be `async` and read data directly; it cannot use state, effects or event handlers. `'use client'` at the top of a file makes that module and its imports client code. Props crossing the border must be serializable. There is no directive for Server Components themselves.

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

`'use server'` marks functions the client may call over the network. Their arguments are fully controlled by the client.

## Best practices

- Put slow data behind its own `Suspense` boundary.
- Keep `'use client'` as low in the tree as possible.
- Authorize and validate in every Server Function.

## Common mistakes

- ❌ Taking `'use server'` for "this is a Server Component" → ✅ it marks Server Functions.
- ❌ Passing a plain function to a Client Component → ✅ not serializable.
- ❌ Hiding a mismatch with `suppressHydrationWarning` → ✅ fix the cause.

## Related materials

- [Basics](basics.en.md)
- [Expert theory](expert.en.md)

## Sources

- [React: hydrateRoot](https://react.dev/reference/react-dom/client/hydrateRoot)
- [React: renderToString](https://react.dev/reference/react-dom/server/renderToString)
- [React: renderToPipeableStream](https://react.dev/reference/react-dom/server/renderToPipeableStream)
- [React: renderToReadableStream](https://react.dev/reference/react-dom/server/renderToReadableStream)
- [React: prerender](https://react.dev/reference/react-dom/static/prerender)
- [React: Server Components](https://react.dev/reference/rsc/server-components)
- [React: Server Functions](https://react.dev/reference/rsc/server-functions)
- [React: 'use client'](https://react.dev/reference/rsc/use-client)
- [React: 'use server'](https://react.dev/reference/rsc/use-server)
