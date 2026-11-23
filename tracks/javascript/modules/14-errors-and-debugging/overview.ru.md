---
title: "Ошибки и отладка"
lang: ru
track: javascript
kind: module
doc: overview
slug: 14-errors-and-debugging
part: 2
levels: [1, 2, 3, 4]
tags: [error, try-catch, error-cause, aggregate-error, custom-error, stack-trace, debugging, unhandled-rejection, explicit-resource-management]
versions: "ECMAScript 2025+, Node.js 24.15 LTS"
status: draft
updated: 2026-11-23
---

# Ошибки и отладка

🌐 **Русский** · [English](overview.en.md)

## Коротко

Ошибки в JavaScript — объекты `Error` (и его подклассы), которые бросают `throw` и ловят `try/catch`. В объекте есть `message`, `name`, `stack` и необязательная причина `cause`. Хорошая обработка ошибок — это своя иерархия классов, цепочки причин, аккуратное «пробрасывание» и обработчики верхнего уровня. Отладка — это читаемый стек вызовов, `console`, оператор `debugger` и отладчик Node.js.

## Ключевые идеи

- **Бросать можно что угодно, но нужно бросать `Error`:** только у него есть `stack`.
- **`try/catch/finally`:** `finally` выполняется всегда; `return` в нём перекрывает остальное.
- **`cause`** хранит причину: ошибки складываются в цепочку.
- **`AggregateError`** собирает несколько ошибок (`Promise.any`).
- **Свой класс ошибки** наследуют от `Error` и задают `name`; `instanceof` позволяет ловить по типу.
- **`try/catch` не ловит ошибки асинхронных колбэков;** для промисов и `await` — `catch` и `try/catch` вокруг `await`.
- **`stack` строится при создании объекта,** а не при `throw`.
- **Обработчики верхнего уровня** (`uncaughtException`, `unhandledRejection`) — страховка, а не способ обработки.
- **`Error` не сериализуется `JSON.stringify`:** нужны `toJSON` или явное копирование полей.
- **Отладка:** `console.*`, `debugger`, `node --inspect`, асинхронные кадры стека.

## Минимальный пример

```js
try {
  try { JSON.parse('{') } catch (c) { throw new Error('config failed', { cause: c }) }
} catch (err) {
  console.log(err.message, '<-', err.cause.name);
}
```

```text
config failed <- SyntaxError
```

Исходная ошибка не теряется: она в `cause`.

## Когда применять и когда нет

| Задача | Выбор |
|---|---|
| Ошибка программиста или непредвиденная ситуация | `throw new Error(...)` |
| Ожидаемый «отказ» (не найдено, неверный ввод) | свой подкласс или возвращаемое значение, а не исключение |
| Добавить контекст к пойманной ошибке | `new Error('…', { cause })` |
| Несколько независимых ошибок | `AggregateError` |
| Освобождение ресурса | `finally` или `using` |
| Падение процесса | обработчик верхнего уровня: залогировать и завершиться |

- ❌ Не глотайте ошибки пустым `catch`.
- ❌ Не бросайте строки и «голые» объекты.

## Уровни материала

| Уровень | Что внутри |
|---|---|
| [1. Базовая теория](basics.ru.md) | `Error`, типы ошибок, `try/catch/finally`, `throw`, чтение стека |
| [2. Продвинутая теория](advanced.ru.md) | свои классы, `cause`, `AggregateError`, ловля по типу, асинхронные ошибки, обработчики верхнего уровня, коды ошибок Node.js, отладка |
| [3. Экспертная теория](expert.ru.md) | справочник, внутреннее устройство, подвохи, вопросы с собеседований |
| [4. Самопроверка](quiz.ru.md) | квиз и [упражнения](exercises.ru.md) |
| 5. Запускаемые примеры | пока не создан — `/learn javascript 14-errors-and-debugging level 5` |
| 6. Тесты | пока не создан — `/learn javascript 14-errors-and-debugging level 6` |

## Связанные материалы

- Пререквизиты: [12 · Промисы и async/await](../12-promises-and-async-await/overview.ru.md), [13 · Цикл событий и параллелизм](../13-event-loop-and-concurrency/overview.ru.md)
- Дальше: модуль 15 · Тестирование (в [дорожной карте](../../overview.ru.md))
- Аналоги в других треках: [C#: исключения](../../../csharp/modules/08-exceptions/overview.ru.md), [Python](../../../python/overview.ru.md) (исключения)
