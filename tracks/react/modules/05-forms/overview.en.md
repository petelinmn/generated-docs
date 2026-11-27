---
title: "Forms"
lang: en
track: react
kind: module
doc: overview
slug: 05-forms
part: 1
levels: [1, 2, 3, 4]
tags: [react, forms, controlled-components, validation, form-data]
versions: "React 19.3"
status: draft
updated: 2026-11-26
---

# Forms

🌐 [Русский](overview.ru.md) · **English**

## In a nutshell

A form field already has its own value inside the browser. In React you decide who owns it. A **controlled** field takes its value from state, so React is the source of truth. An **uncontrolled** field keeps the value itself, and you read it when needed, usually with `FormData` on submit.

## Key ideas

- Controlled: `value` (or `checked`) plus `onChange`.
- Uncontrolled: `defaultValue` (or `defaultChecked`), read on submit.
- A field must not switch between the two.
- The value of an input is a string, even for numbers.
- `FormData` collects named, enabled controls; unchecked boxes are absent.
- Native validation runs before your submit handler.
- `useId` connects a label with its field.

## Minimal example

```tsx
function Upper() {
  const [v, setV] = useState('');
  return <input value={v} onChange={e => setV(e.target.value.toUpperCase())} />;
}
// typing "abc" shows "ABC"
```

## When to use and when not

- ✅ Controlled: live validation, formatting, dependent fields, disabling buttons.
- ✅ Uncontrolled: simple forms that are read only on submit.
- ❌ Controlled state for dozens of fields you never look at while typing.
- ❌ Client-side validation as the only protection: the server must check too.

## Levels of this material

| Level | What's inside |
|---|---|
| [1. Basics](basics.en.md) | controlled and uncontrolled fields, all control types, labels, submit |
| [2. Advanced](advanced.en.md) | numbers, `FormData`, validation, accessible errors, `action` |
| [3. Expert](expert.en.md) | reference, warnings, gotchas, interview questions |
| [4. Self-check](quiz.en.md) | a quiz and [exercises](exercises.en.md) |
| 5. Runnable examples | not created yet — `/learn react 05-forms level 5` |
| 6. Tests | not created yet — `/learn react 05-forms level 6` |

## Related materials

- Prerequisites: [03 · State and events](../03-state-and-events/overview.en.md), [04 · Lists, keys and identity](../04-lists-and-keys/overview.en.md)
- Forms in the DOM: [browser DOM and events](../../../javascript/modules/17-browser-dom-and-events/overview.en.md)
- Validating on the server: [API design](../../../system-design/modules/05-api-design/overview.en.md)
- Next: the following module of the [React roadmap](../../overview.en.md)
- [10 · Testing](../10-testing/overview.en.md)
- [13 · Actions](../13-actions/overview.en.md)
