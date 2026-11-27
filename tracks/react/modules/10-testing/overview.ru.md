---
title: "Тестирование"
lang: ru
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

# Тестирование

🌐 **Русский** · [English](overview.en.md)

## Коротко

Тест компонента рендерит его, действует с ним так, как действовал бы пользователь, и проверяет то, что пользователь увидит. Обычный набор: Vitest запускает тесты, jsdom даёт DOM в Node.js, Testing Library находит элементы и имитирует взаимодействие. Такие тесты переживают переписывание кода, потому что не заглядывают внутрь компонентов.

## Ключевые идеи

- Проверяйте поведение, видимое пользователю, а не состояние и внутренние методы.
- Ищите элементы прежде всего по роли и доступному имени.
- `getBy` ждёт элемент сейчас, `queryBy` допускает отсутствие, `findBy` ожидает.
- `userEvent` имитирует настоящее взаимодействие; его вызовы ждут через `await`.
- Подменяйте внешнее: функции, запросы, время.
- Хуки проверяют через `renderHook`, обновления оборачивают в `act`.
- Каждый тест начинается с чистого документа.

## Минимальный пример

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

## Когда применять, а когда нет

- ✅ Поведение компонентов, формы, состояния загрузки и ошибки.
- ✅ Чистая логика (редьюсеры, проверки) — обычными модульными тестами.
- ❌ Внешний вид и раскладка: jsdom страницу не размещает.
- ❌ Полные сценарии через несколько страниц: нужен end-to-end инструмент в браузере.

## Уровни материала

| Уровень | Что внутри |
|---|---|
| [1. Базовая теория](basics.ru.md) | настройка, рендер, запросы, взаимодействие |
| [2. Продвинутая теория](advanced.ru.md) | асинхронные состояния, подмены, context, хуки, таймеры |
| [3. Экспертная теория](expert.ru.md) | справочник, подвохи, компромиссы, вопросы с собеседований |
| [4. Самопроверка](quiz.ru.md) | квиз и [упражнения](exercises.ru.md) |
| 5. Запускаемые примеры | ещё не создан — `/learn react 10-testing level 5` |
| 6. Тесты | ещё не создан — `/learn react 10-testing level 6` |

## Связанные материалы

- Пререквизиты: [05 · Формы](../05-forms/overview.ru.md), [09 · Пользовательские хуки](../09-custom-hooks/overview.ru.md)
- Тестирование на чистом JavaScript: [тестирование в JavaScript](../../../javascript/modules/15-testing/overview.ru.md)
- Vitest с типами: [трек TypeScript](../../../typescript/overview.ru.md)
- Те же идеи на стороне сервера: [unit-тесты в C#](../../../csharp/modules/16-testing-unit/overview.ru.md)
- Дальше: следующий модуль по [дорожной карте React](../../overview.ru.md)
