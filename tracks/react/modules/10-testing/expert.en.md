---
title: "Testing: expert"
lang: en
track: react
kind: module
doc: expert
slug: 10-testing
part: 2
tags: [react, testing, vitest, testing-library, unit-testing]
versions: "React 19.3"
status: draft
updated: 2026-11-27
---

# Testing: expert

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · **3. Expert** · [4. Quiz](quiz.en.md)

## Under the hood

Vitest runs test files in Node.js. The `jsdom` environment (the default is `node`) provides `window` and `document`. `render` creates a container in `document.body` and mounts the component there; `screen` searches the whole body. Testing Library wraps rendering and interactions in `act`, so React has applied updates before you assert.

## Reference

| Call | Purpose |
|---|---|
| `render(ui, { wrapper })` | mount; returns `rerender`, `unmount`, `container` |
| `screen.getBy* / queryBy* / findBy*` | one element; `All` variants for many |
| `userEvent.setup()` then `click`, `type`, `keyboard`, `selectOptions` | interaction |
| `renderHook(fn)` | run a hook; read `result.current` |
| `act(fn)` | apply updates caused outside Testing Library |
| `vi.fn()`, `mockResolvedValue`, `mockRejectedValue` | stand-ins |
| `vi.useFakeTimers()`, `advanceTimersByTime(Async)` | control time |
| `expect(el).toBeInTheDocument / toHaveTextContent / toBeDisabled` | jest-dom matchers |

## Gotchas

### getBy does not wait

```tsx
screen.getByText('Count: 5');
// TestingLibraryElementError: Unable to find an element with the text: Count: 5. ...

render(<User load={() => Promise.resolve('Ann')} />);
screen.getByText('Ann');                       // fails the same way: the data is not there yet
await screen.findByText('Ann');                // passes
```

### A state update without act

```tsx
const { result } = renderHook(() => useToggle());
result.current[1]();                           // no act
expect(result.current[0]).toBe(true);
// AssertionError: expected false to be true
```

### No cleanup between tests

```tsx
// without cleanup() in afterEach (and without Vitest globals)
test('first',  () => { render(<Counter />); screen.getByRole('button'); });
test('second', () => { render(<Counter />); screen.getByRole('button'); });
// second:  TestingLibraryElementError: Found multiple elements with the role "button"
```

### user-event and fake timers

```tsx
vi.useFakeTimers();
const user = userEvent.setup();                // no advanceTimers
render(<Counter />);
await user.click(screen.getByRole('button'));  // TestTimeoutError: Test timed out

// instead
vi.useFakeTimers({ shouldAdvanceTime: true });
const user = userEvent.setup({ advanceTimers: vi.advanceTimersByTime });
```

## Trade-offs and alternatives

| Level | Pro | Con |
|---|---|---|
| Unit test of pure logic | fast, precise | says nothing about the interface |
| Component test in jsdom | fast, behaviour-level | not a real browser |
| Component test in a real browser | layout and events are real | slower, more setup |
| End-to-end | whole system | slow and more brittle |
| Snapshot test | cheap to write | easy to accept without reading |

## Interview questions

**1. Why query by role?**

<details><summary>Answer</summary>

It matches how users and assistive technology find elements, and checks accessibility on the way.

</details>

**2. `getBy`, `queryBy`, `findBy`?**

<details><summary>Answer</summary>

`getBy` throws if absent, `queryBy` returns `null`, `findBy` returns a promise that retries.

</details>

**3. `userEvent` or `fireEvent`?**

<details><summary>Answer</summary>

`fireEvent` dispatches one DOM event; `userEvent` simulates a whole interaction with its checks.

</details>

**4. What is `act`?**

<details><summary>Answer</summary>

A helper that applies pending React updates before assertions; Testing Library wraps its own calls in it.

</details>

**5. How do you test a custom hook?**

<details><summary>Answer</summary>

`renderHook`, or through a component that uses it.

</details>

**6. How do you test code with timers?**

<details><summary>Answer</summary>

Fake timers and advancing time.

</details>

**7. How do you provide context in a test?**

<details><summary>Answer</summary>

The `wrapper` option of `render`.

</details>

**8. What can jsdom not test?**

<details><summary>Answer</summary>

Layout, sizes, painting and real browser behaviour.

</details>

**9. Trick question: the test passes alone and fails in the suite. Why might that be?**

<details><summary>Answer</summary>

Shared state: no cleanup of the DOM, fake timers not restored, a mock or module value left over.

</details>

**10. Trick question: with fake timers `user.click` never finishes. Why?**

<details><summary>Answer</summary>

user-event waits on timers; give it `advanceTimers`.

</details>

**11. Is high coverage proof of good tests?**

<details><summary>Answer</summary>

No: code can be executed without anything meaningful being asserted.

</details>

## Related materials

- [Advanced theory](advanced.en.md)
- [Quiz](quiz.en.md) and [exercises](exercises.en.md)

## Sources

- [Testing Library: about queries](https://testing-library.com/docs/queries/about)
- [Testing Library: user-event](https://testing-library.com/docs/user-event/intro)
- [React Testing Library: API](https://testing-library.com/docs/react-testing-library/api)
- [Vitest: environment](https://vitest.dev/config/environment)
- [Vitest: vi](https://vitest.dev/api/vi)
- [jest-dom: with Vitest](https://github.com/testing-library/jest-dom#with-vitest)
- [React: act](https://react.dev/reference/react/act)
