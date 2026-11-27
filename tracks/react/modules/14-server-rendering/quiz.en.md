---
title: "Quiz: server rendering"
lang: en
track: react
kind: module
doc: quiz
slug: 14-server-rendering
part: 3
tags: [react, server-side-rendering, hydration, server-components, streaming]
versions: "React 19.3"
status: draft
updated: 2026-11-27
---

# Quiz: server rendering

🌐 [Русский](quiz.ru.md) · **English**

> Answer first, then open the answer. The mark shows the level: 🟢 [basics](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 What does `renderToString` return?

- A) A DOM node
- B) An HTML string
- C) A JavaScript bundle
- D) A promise

<details><summary>Answer</summary>

**B.**

→ [Basics](basics.en.md)

</details>

## 2. 🟢 Which runs during server rendering?

- A) `useEffect`
- B) The component function
- C) `onClick`
- D) `useLayoutEffect`

<details><summary>Answer</summary>

**B.**

→ [Basics](basics.en.md)

</details>

## 3. 🟢 How do you start React on server-rendered HTML?

- A) `createRoot().render`
- B) `hydrateRoot`
- C) `renderToString`
- D) `flushSync`

<details><summary>Answer</summary>

**B.**

→ [Basics](basics.en.md)

</details>

## 4. 🟢 Find the bug (server-rendered component)

```tsx
function Theme() {
  const theme = localStorage.getItem('theme');
  return <p>{theme}</p>;
}
```

<details><summary>Answer</summary>

There is no `localStorage` on the server; read it on the client, for example in an effect.

→ [Basics](basics.en.md)

</details>

## 5. 🟡 What is sent first by streaming with a pending boundary?

- A) Nothing until all is ready
- B) The shell with the fallback
- C) Only scripts
- D) Only the slow part

<details><summary>Answer</summary>

**B.**

→ [Advanced](advanced.en.md)

</details>

## 6. 🟡 The server rendered `class="a"`, the client renders `className="b"`. Likely DOM after hydration?

- A) `b`
- B) `a`, with an error in the console
- C) No class
- D) Both

<details><summary>Answer</summary>

**B.** Patching attributes is not guaranteed.

→ [Advanced](advanced.en.md)

</details>

## 7. 🟡 Select all things a Server Component can do

- [ ] A) Be `async`
- [ ] B) Query a database
- [ ] C) Use `useState`
- [ ] D) Handle clicks

<details><summary>Answer</summary>

**A, B.**

→ [Advanced](advanced.en.md)

</details>

## 8. 🟡 What does `'use server'` mark?

- A) Server Components
- B) Server Functions
- C) Static pages
- D) Server-only CSS

<details><summary>Answer</summary>

**B.**

→ [Advanced](advanced.en.md)

</details>

## 9. 🔴 Streaming has started and a component inside `Suspense` fails. The HTTP status

- A) becomes 500
- B) cannot be changed any more
- C) becomes 404
- D) is chosen by the client

<details><summary>Answer</summary>

**B.**

→ [Expert](expert.en.md)

</details>

## 10. 🔴 `<time suppressHydrationWarning>` has server text 1, client text 2. Shown after hydration?

- A) 2
- B) 1
- C) Empty
- D) 12

<details><summary>Answer</summary>

**B.**

→ [Expert](expert.en.md)

</details>

## 11. 🔴 Open question: how do you show a date in the user's locale on a server-rendered page?

<details><summary>Key points</summary>

- Render a neutral form (ISO or a fixed locale and time zone) on the server and during hydration.
- Switch to the local form in a second render (`useSyncExternalStore` server snapshot or an effect).
- Or send the user locale and time zone to the server.

→ [Expert](expert.en.md)

</details>
