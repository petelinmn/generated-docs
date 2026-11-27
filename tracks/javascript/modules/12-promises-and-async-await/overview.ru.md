---
title: "Промисы и async/await"
lang: ru
track: javascript
kind: module
doc: overview
slug: 12-promises-and-async-await
part: 2
levels: [1, 2, 3, 4]
tags: [promise, async-await, promise-all, promise-allsettled, promise-any, microtask, unhandled-rejection, abort-signal]
versions: "ECMAScript 2025+, Node.js 24.15 LTS"
status: draft
updated: 2026-11-26
---

# Промисы и async/await

🌐 **Русский** · [English](overview.en.md)

## Коротко

Промис (promise) — объект, представляющий результат асинхронной операции, который появится позже: успех (fulfilled), ошибку (rejected) или ожидание (pending). Из промисов собирают цепочки через `then`/`catch`/`finally`, а `async`/`await` позволяет писать то же самое линейным кодом с обычным `try/catch`. Комбинаторы `Promise.all`, `allSettled`, `race`, `any` объединяют несколько операций.

## Ключевые идеи

- **Промис меняет состояние один раз:** pending → fulfilled или rejected; повторные `resolve`/`reject` игнорируются.
- **Обработчики `then` всегда асинхронны:** они выполняются как микрозадачи после текущего синхронного кода.
- **`then` возвращает новый промис,** поэтому из промисов строятся цепочки; ошибка проходит по цепочке до ближайшего `catch`.
- **`async`-функция всегда возвращает промис;** `await` приостанавливает её до результата.
- **`return promise` и `return await promise` различаются** внутри `try/catch`.
- **`Promise.all` — параллельно и «всё или ничего»,** `allSettled` — результаты всех, `race` — первый, `any` — первый успех.
- **`await` в цикле последователен;** `forEach(async …)` не ждёт.
- **Необработанное отклонение завершает процесс** Node.js: каждый промис должен иметь обработчик ошибок.
- **Отмена и таймаут** делаются через `AbortSignal` и `Promise.race`.

## Минимальный пример

```js
const sleep = (ms, v) => new Promise(r => setTimeout(r, ms, v));
async function main() {
  const [a, b] = await Promise.all([sleep(5, 'a'), sleep(5, 'b')]);
  console.log(a, b);
}
main();
```

```text
a b
```

Две операции идут одновременно, а `await` дожидается обеих.

## Когда применять и когда нет

| Задача | Выбор |
|---|---|
| Последовательные зависимые шаги | `await` один за другим |
| Независимые операции | `Promise.all` |
| Нужны результаты всех, даже ошибочных | `Promise.allSettled` |
| Первый успешный из нескольких источников | `Promise.any` |
| Таймаут, отмена | `Promise.race` + `AbortSignal` |
| Поток значений во времени | асинхронные итераторы ([модуль 11](../11-iterators-and-generators/overview.ru.md)) |

- ❌ Не используйте `forEach` с `async`-функцией, если нужно дождаться её.
- ❌ Не оставляйте промисы без обработчика ошибок.

## Уровни материала

| Уровень | Что внутри |
|---|---|
| [1. Базовая теория](basics.ru.md) | состояния, `then`/`catch`/`finally`, цепочки, `async`/`await`, ошибки |
| [2. Продвинутая теория](advanced.ru.md) | комбинаторы, параллелизм и последовательность, таймаут и отмена, пул, повтор, необработанные отклонения |
| [3. Экспертная теория](expert.ru.md) | справочник, порядок микрозадач, внутреннее устройство, подвохи, вопросы с собеседований |
| [4. Самопроверка](quiz.ru.md) | квиз и [упражнения](exercises.ru.md) |
| 5. Запускаемые примеры | пока не создан — `/learn javascript 12-promises-and-async-await level 5` |
| 6. Тесты | пока не создан — `/learn javascript 12-promises-and-async-await level 6` |

## Связанные материалы

- Пререквизиты: [04 · Функции](../04-functions/overview.ru.md), [11 · Итераторы и генераторы](../11-iterators-and-generators/overview.ru.md)
- Дальше: [13 · Цикл событий и параллелизм](../13-event-loop-and-concurrency/overview.ru.md)
- Аналоги в других треках: [C#: async/await](../../../csharp/modules/10-async-await/overview.ru.md), [Python](../../../python/overview.ru.md) (asyncio)
- [Эффекты и отмена в React](../../../react/modules/06-effects/overview.ru.md)
- [Suspense и transitions в React](../../../react/modules/12-concurrent-features/overview.ru.md)
