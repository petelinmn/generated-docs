---
title: "Testing"
lang: en
track: react
kind: module
doc: overview
slug: 10-testing
part: 2
levels: [1, 2, 3, 4]
tags: [react, testing, vitest, testing-library, unit-testing]
versions: "React 19.3"
status: draft
updated: 2026-11-27
---

# Testing

🌐 [Русский](overview.ru.md) · **English**

## In a nutshell

A component test renders the component, acts on it the way a user would, and checks what the user would see. The usual tools are Vitest to run tests, jsdom to provide a DOM in Node.js, and Testing Library to find elements and simulate interaction. Such tests survive refactoring because they do not look inside components.

## Key ideas

- Test behaviour that users see, not state or internal methods.
- Find elements by role and accessible name first.
- `getBy` expects the element now, `queryBy` allows absence, `findBy` waits.
- `userEvent` imitates real interaction; its calls are awaited.
- Replace what is outside: functions, requests, time.
- Hooks are tested with `renderHook`, updates are wrapped in `act`.
- Each test starts with a clean document.

## Minimal example

```tsx
function Counter() {
  const [n, setN] = useState(0);
  return (
    <>
      <p>Count: {n}</p>
      <button onClick={() => setN(n + 1)}>Add</button>
    </>
  );
}

test('increments on click', async () => {
  const user = userEvent.setup();
  render(<Counter />);

  await user.click(screen.getByRole('button', { name: 'Add' }));

  expect(screen.getByText('Count: 1')).toBeInTheDocument();
});
```

## When to use and when not

- ✅ Component behaviour, forms, loading and error states.
- ✅ Pure logic (reducers, validators) as plain unit tests.
- ❌ Visual appearance and layout: jsdom does not lay out the page.
- ❌ Full user journeys across pages: use a browser-based end-to-end tool.

## Levels of this material

| Level | What's inside |
|---|---|
| [1. Basics](basics.en.md) | setup, rendering, queries, interaction |
| [2. Advanced](advanced.en.md) | async states, mocks, context, hooks, timers |
| [3. Expert](expert.en.md) | reference, gotchas, trade-offs, interview questions |
| [4. Self-check](quiz.en.md) | a quiz and [exercises](exercises.en.md) |
| 5. Runnable examples | not created yet — `/learn react 10-testing level 5` |
| 6. Tests | not created yet — `/learn react 10-testing level 6` |

## Related materials

- Prerequisites: [05 · Forms](../05-forms/overview.en.md), [09 · Custom hooks](../09-custom-hooks/overview.en.md)
- Testing in plain JavaScript: [JavaScript testing](../../../javascript/modules/15-testing/overview.en.md)
- Vitest with types: [TypeScript track](../../../typescript/overview.en.md)
- The same ideas on the server side: [unit testing in C#](../../../csharp/modules/16-testing-unit/overview.en.md)
- Next: the following module of the [React roadmap](../../overview.en.md)
