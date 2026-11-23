---
title: "Quiz: browser — the DOM and events"
lang: en
track: javascript
kind: module
doc: quiz
slug: 17-browser-dom-and-events
part: 3
tags: [dom, events, fetch, local-storage, cors]
status: draft
updated: 2026-11-23
---

# Quiz: browser — the DOM and events

🌐 [Русский](quiz.ru.md) · **English**

> Answer on your own first, then open the answer. The label shows the question level: 🟢 [basic](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 What does `document.querySelectorAll('.item')` return after another `.item` is added to the document?

- A) An updated list with the new element
- B) The old static list without the new element
- C) `null`
- D) An error

<details><summary>Answer</summary>

**B.** `querySelectorAll` returns a static `NodeList`; `getElementsByClassName` is live.
→ [Basic theory](basics.en.md#searching-for-elements)

</details>

## 2. 🟢 Which is safer for inserting user text?

- A) `el.innerHTML = text`
- B) `el.textContent = text`
- C) `document.write(text)`
- D) `el.outerHTML = text`

<details><summary>Answer</summary>

**B.** `textContent` inserts text, `innerHTML` parses markup (an XSS risk).
→ [Basic theory](basics.en.md#textcontent-and-innerhtml)

</details>

## 3. 🟢 In what order do listeners fire on a click of a button inside `#inner` inside `#outer`, if `#outer` has a capture and a bubble listener and `#inner` a bubble one?

- A) outer-bubble, inner-bubble, outer-capture
- B) outer-capture, inner-bubble, outer-bubble
- C) inner-bubble, outer-capture, outer-bubble
- D) outer-capture, outer-bubble, inner-bubble

<details><summary>Answer</summary>

**B.** First capture top down, then the target, then bubbling bottom up.
→ [Basic theory](basics.en.md#events)

</details>

## 4. 🟢 What does `fetch('/missing')` return if the server answered `404`?

- A) A rejected promise
- B) A fulfilled promise with `response.ok === false`
- C) `null`
- D) An exception on the call

<details><summary>Answer</summary>

**B.** `fetch` is rejected only on network errors; check `response.ok`.
→ [Basic theory](basics.en.md#fetch)

</details>

## 5. 🟡 What happens with `localStorage.setItem('n', 5)` and `typeof localStorage.getItem('n')`?

- A) `'number'`
- B) `'string'`
- C) `'undefined'`
- D) An error on writing

<details><summary>Answer</summary>

**B.** `localStorage` stores only strings.
→ [Basic theory](basics.en.md#storages)

</details>

## 6. 🟡 What happens after `input.value = 'x'`?

- A) `input` fires
- B) `change` fires
- C) Neither event fires
- D) Both fire

<details><summary>Answer</summary>

**C.** An assignment from code produces no events; they are fired by the user or `dispatchEvent`.
→ [Advanced theory](advanced.en.md#input-and-focus-events)

</details>

## 7. 🟡 For which event does delegation through a parent not work without capture?

- A) `click`
- B) `input`
- C) `focus`
- D) `keydown`

<details><summary>Answer</summary>

**C.** `focus` does not bubble; use `focusin` or capture.
→ [Advanced theory](advanced.en.md#input-and-focus-events)

</details>

## 8. 🟡 What does `<script defer>…code…</script>` (inline, no `src`) do?

- A) Postpones execution until parsing ends
- B) Nothing: `defer` is ignored, the code runs at once
- C) Runs the code after `load`
- D) A parse error

<details><summary>Answer</summary>

**B.** `defer` applies only to external scripts; a module script is deferred by itself.
→ [Advanced theory](advanced.en.md#script-loading-order)

</details>

## 9. 🟡 Choose all true statements

- [ ] A) `form.requestSubmit()` fires the `submit` event
- [ ] B) `form.submit()` fires the `submit` event
- [ ] C) `new FormData(form)` gives string values
- [ ] D) An unchecked checkbox goes into `FormData` with the value `'off'`

<details><summary>Answer</summary>

**A, C.** B is wrong; D is wrong: an unchecked checkbox is not included in the data.
→ [Advanced theory](advanced.en.md#forms)

</details>

## 10. 🔴 Find the mistake

> "`fetch(url, { mode: 'no-cors' })` lets you read a foreign site's response without the server's permission."

<details><summary>Answer</summary>

The response will be opaque (`type: 'opaque'`, `status: 0`): it cannot be read. `no-cors` does not bypass CORS.
→ [Gotcha 10](expert.en.md#10-no-cors-does-not-bypass-cors)

</details>

## 11. 🔴 What does `document.querySelectorAll('my-card p')` return if the `<p>` lives in the Shadow DOM of `my-card`?

- A) That `<p>`
- B) An empty list (`0`)
- C) An error
- D) `null`

<details><summary>Answer</summary>

**B.** Document selectors do not reach into the shadow; access goes through `card.shadowRoot`.
→ [Advanced theory](advanced.en.md#custom-elements-and-shadow-dom)

</details>

## 12. 🔴 Why does `removeEventListener('click', () => count++)` not work?

- A) `removeEventListener` does not exist
- B) It is a different function: the same reference is needed (or `{ signal }`)
- C) You must specify `capture`
- D) Clicks cannot be removed

<details><summary>Answer</summary>

**B.** A listener is removed only by a reference to the same function.
→ [Gotcha 4](expert.en.md#4-an-anonymous-function-is-not-removed)

</details>

## 13. 🔴 Open question

Explain how to build a task list with adding, deleting and saving to `localStorage`: which events and APIs you use and what you watch for in security.

<details><summary>Key points of a good answer</summary>

- One click listener on the list with delegation (`closest`), a form with `submit` and `preventDefault`/`requestSubmit`.
- Inserting text with `textContent` (not `innerHTML`).
- Saving: `JSON.stringify` into `localStorage`, loading with `JSON.parse` and a default value.
- Do not keep secrets in `localStorage`; check server responses through `response.ok`.

</details>

→ [Basic theory](basics.en.md)
