---
title: "Тестирование: экспертная теория"
lang: ru
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

# Тестирование: экспертная теория

🌐 **Русский** · [English](expert.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · [2. Продвинутая](advanced.ru.md) · **3. Экспертная** · [4. Квиз](quiz.ru.md)

## Под капотом

Vitest выполняет файлы тестов в Node.js. Окружение `jsdom` (по умолчанию — `node`) даёт `window` и `document`. `render` создаёт контейнер в `document.body` и монтирует туда компонент; `screen` ищет по всему body. Testing Library оборачивает рендер и взаимодействия в `act`, поэтому к моменту проверки React уже применил обновления.

## Справочник

| Вызов | Назначение |
|---|---|
| `render(ui, { wrapper })` | смонтировать; возвращает `rerender`, `unmount`, `container` |
| `screen.getBy* / queryBy* / findBy*` | один элемент; варианты `All` для многих |
| `userEvent.setup()`, затем `click`, `type`, `keyboard`, `selectOptions` | взаимодействие |
| `renderHook(fn)` | выполнить хук; читать `result.current` |
| `act(fn)` | применить обновления, вызванные вне Testing Library |
| `vi.fn()`, `mockResolvedValue`, `mockRejectedValue` | заглушки |
| `vi.useFakeTimers()`, `advanceTimersByTime(Async)` | управление временем |
| `expect(el).toBeInTheDocument / toHaveTextContent / toBeDisabled` | проверки jest-dom |

## Подвохи

### getBy не ждёт

```tsx
screen.getByText('Count: 5');
// TestingLibraryElementError: Unable to find an element with the text: Count: 5. ...

render(<User load={() => Promise.resolve('Ann')} />);
screen.getByText('Ann');                       // fails the same way: the data is not there yet
await screen.findByText('Ann');                // passes
```

### Обновление состояния без act

```tsx
const { result } = renderHook(() => useToggle());
result.current[1]();                           // no act
expect(result.current[0]).toBe(true);
// AssertionError: expected false to be true
```

### Нет очистки между тестами

```tsx
// without cleanup() in afterEach (and without Vitest globals)
test('first',  () => { render(<Counter />); screen.getByRole('button'); });
test('second', () => { render(<Counter />); screen.getByRole('button'); });
// second:  TestingLibraryElementError: Found multiple elements with the role "button"
```

### user-event и поддельные таймеры

```tsx
vi.useFakeTimers();
const user = userEvent.setup();                // no advanceTimers
render(<Counter />);
await user.click(screen.getByRole('button'));  // TestTimeoutError: Test timed out

// instead
vi.useFakeTimers({ shouldAdvanceTime: true });
const user = userEvent.setup({ advanceTimers: vi.advanceTimersByTime });
```

## Компромиссы и альтернативы

| Уровень | Плюс | Минус |
|---|---|---|
| Модульный тест чистой логики | быстро, точно | ничего не говорит об интерфейсе |
| Тест компонента в jsdom | быстро, на уровне поведения | не настоящий браузер |
| Тест компонента в настоящем браузере | раскладка и события настоящие | медленнее, сложнее настройка |
| End-to-end | вся система | медленно и более хрупко |
| Снимочный тест (snapshot) | дёшево написать | легко принять не читая |

## Вопросы с собеседований

**1. Зачем искать по роли?**

<details><summary>Ответ</summary>

Так элементы находят пользователи и вспомогательные технологии, заодно проверяется доступность.

</details>

**2. `getBy`, `queryBy`, `findBy`?**

<details><summary>Ответ</summary>

`getBy` падает при отсутствии, `queryBy` возвращает `null`, `findBy` — промис с повторными попытками.

</details>

**3. `userEvent` или `fireEvent`?**

<details><summary>Ответ</summary>

`fireEvent` отправляет одно событие DOM; `userEvent` имитирует всё взаимодействие с проверками.

</details>

**4. Что такое `act`?**

<details><summary>Ответ</summary>

Помощник, применяющий отложенные обновления React до проверок; Testing Library оборачивает в него свои вызовы.

</details>

**5. Как тестировать пользовательский хук?**

<details><summary>Ответ</summary>

`renderHook` или через использующий его компонент.

</details>

**6. Как тестировать код с таймерами?**

<details><summary>Ответ</summary>

Поддельные таймеры и перемотка времени.

</details>

**7. Как дать context в тесте?**

<details><summary>Ответ</summary>

Опция `wrapper` у `render`.

</details>

**8. Что не проверить в jsdom?**

<details><summary>Ответ</summary>

Раскладку, размеры, отрисовку и поведение настоящего браузера.

</details>

**9. Подвох: тест проходит один и падает в наборе. Почему?**

<details><summary>Ответ</summary>

Общее состояние: не очищен DOM, не возвращены таймеры, осталась подмена или значение модуля.

</details>

**10. Подвох: с поддельными таймерами `user.click` не завершается. Почему?**

<details><summary>Ответ</summary>

user-event ждёт таймеры; передайте ему `advanceTimers`.

</details>

**11. Высокое покрытие — признак хороших тестов?**

<details><summary>Ответ</summary>

Нет: код можно выполнить, не проверив ничего существенного.

</details>

## Связанные материалы

- [Продвинутая теория](advanced.ru.md)
- [Квиз](quiz.ru.md) и [упражнения](exercises.ru.md)

## Источники

- [Testing Library: о запросах](https://testing-library.com/docs/queries/about)
- [Testing Library: user-event](https://testing-library.com/docs/user-event/intro)
- [React Testing Library: API](https://testing-library.com/docs/react-testing-library/api)
- [Vitest: environment](https://vitest.dev/config/environment)
- [Vitest: vi](https://vitest.dev/api/vi)
- [jest-dom: with Vitest](https://github.com/testing-library/jest-dom#with-vitest)
- [React: act](https://react.dev/reference/react/act)
