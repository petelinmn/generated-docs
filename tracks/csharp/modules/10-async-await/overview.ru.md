---
title: "Async/await"
lang: ru
track: csharp
kind: module
doc: overview
slug: 10-async-await
part: 2
levels: [1, 2, 3, 4]
tags: [async-await, task, valuetask, cancellation-token, configureawait, deadlock, async-streams]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-22
---

# Async/await

🌐 **Русский** · [English](overview.en.md)

## Коротко

`async`/`await` — модель асинхронного программирования C#: метод приостанавливается на ожидании операции (сеть, диск, БД), освобождая поток, и продолжается после её завершения. Результат ожидания представляет `Task` или `Task<T>`. Код пишется почти как последовательный, а компилятор превращает его в конечный автомат. Подход нужен для масштабируемых серверов и отзывчивых приложений: он экономит потоки, но не ускоряет вычисления.

## Ключевые идеи

- **`await` не блокирует поток:** метод приостанавливается, а продолжение запускается после завершения задачи.
- **Задачи «горячие»:** `Task` стартует при вызове метода; независимые операции запускают вместе и ждут через `Task.WhenAll`.
- **Исключения** сохраняются в задаче и бросаются при `await`; `.Result`/`.Wait()` оборачивают их в `AggregateException`.
- **Отмена кооперативная:** `CancellationToken` передают в каждый долгий метод; таймаут делают через `CancelAfter` и связанные токены.
- **Sync-over-async** (`.Result`, `.Wait()`) даёт deadlock в контексте синхронизации и исчерпание пула потоков; правило — «async all the way».
- **`ConfigureAwait(false)`** ставят в библиотеках; в ASP.NET Core контекста синхронизации нет.
- **`ValueTask<T>`** экономит выделения при синхронном завершении, но допускает ровно одно `await`.
- **Асинхронные потоки:** `IAsyncEnumerable<T>` с `await foreach` и `IAsyncDisposable` с `await using`.

## Минимальный пример

```csharp
static async Task<int> Work(int id) { await Task.Delay(200); return id * 10; }

// Start both operations first, then wait for both: ~200 ms instead of ~400 ms.
int[] results = await Task.WhenAll(Work(1), Work(2));
Console.WriteLine(string.Join(",", results));   // 10,20
```

## Когда применять, а когда нет

- ✅ Ввод-вывод: HTTP, файлы, БД, очереди.
- ✅ Серверный код с большим числом одновременных запросов.
- ✅ UI: не блокировать главный поток.
- ❌ Чистые вычисления: `async` их не ускоряет, нужны `Task.Run`/`Parallel`.
- ❌ Блокирующие вызовы (`.Result`, `.Wait()`) внутри асинхронного кода.
- ❌ `async void` везде, кроме обработчиков событий.

## Уровни материала

| Уровень | Что внутри |
|---|---|
| [1. Базовая теория](basics.ru.md) | зачем async, `Task`/`Task<T>`, что происходит на `await`, `WhenAll`/`WhenAny`, исключения, `CancellationToken`, `Task.Run` |
| [2. Продвинутая теория](advanced.ru.md) | `ConfigureAwait` и контекст синхронизации, deadlock, исключения и `AggregateException`, таймауты, `WhenEach`, `ValueTask`, async-потоки, `TaskCompletionSource`, `PeriodicTimer` |
| [3. Экспертная теория](expert.ru.md) | конечный автомат и awaitable-шаблон, состояния задач, `Run` и `StartNew`, `ExecutionContext`/`AsyncLocal`, синхронные продолжения, исчерпание пула, справочник, подвохи, 17 вопросов с собеседований |
| [4. Самопроверка](quiz.ru.md) | 12 вопросов и [5 упражнений](exercises.ru.md) с решениями |
| 5. Запускаемые примеры | ещё не создан — `/learn csharp 10-async-await level 5` |
| 6. Тесты | ещё не создан — `/learn csharp 10-async-await level 6` |

## Связанные материалы

- Пререквизиты: [08 · Исключения](../08-exceptions/overview.ru.md), [09 · Делегаты, события, лямбды](../09-delegates-events-lambdas/overview.ru.md)
- Дальше: [18 · Многопоточность](../18-concurrency/overview.ru.md), [15 · EF Core](../15-ef-core/overview.ru.md) (асинхронные запросы)
- Ресурсы: [12 · Память и GC](../12-memory-and-gc/overview.ru.md) — `IAsyncDisposable`
- Асинхронные обработчики: [14 · ASP.NET Core: Minimal API](../14-aspnet-core-minimal-api/overview.ru.md)
- Асинхронные тесты: [16 · Unit-тесты](../16-testing-unit/overview.ru.md)
- Контекст трассы через `await`: [22 · Наблюдаемость](../22-observability/overview.ru.md)
- То же в других треках: [Python](../../../python/overview.ru.md) — `asyncio`, `gather` ↔ `Task.WhenAll`
