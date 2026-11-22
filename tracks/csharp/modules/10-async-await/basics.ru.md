---
title: "Async/await: базовая теория"
lang: ru
track: csharp
kind: module
doc: basics
slug: 10-async-await
part: 2
tags: [async-await, task, cancellation-token, task-whenall, asynchronous-programming]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-22
---

# Async/await: базовая теория

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [обзор](overview.ru.md) · **1. Базовая** · [2. Продвинутая](advanced.ru.md) · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [08 · Исключения](../08-exceptions/overview.ru.md), [09 · Делегаты, события, лямбды](../09-delegates-events-lambdas/overview.ru.md). **Время:** ~1 ч.

## Зачем это нужно

Многие операции большую часть времени **ждут**: ответ сети, диск, база данных. Если поток при этом блокируется, он ничего не делает, но занят: сервер с тысячами таких запросов упирается в число потоков. Асинхронный код отдаёт поток, пока операция ждёт, и продолжается (возможно, на другом потоке), когда она завершилась. Ключевые слова `async`/`await` позволяют писать такой код почти как обычный последовательный.

Важно: async/await нужен для **ожидания ввода-вывода**, а не для ускорения вычислений. Для тяжёлых вычислений нужны потоки и [многопоточность](../18-concurrency/overview.ru.md).

## `Task` и `Task<T>`

`Task` — объект, представляющий операцию, которая завершится позже; `Task<T>` — такая операция с результатом типа `T`. Метод с модификатором `async` возвращает `Task`/`Task<T>`, а внутри ожидает другие задачи через `await`:

```csharp
static async Task<int> GetNumberAsync()
{
    await Task.Delay(50);      // the thread is free while we wait
    return 42;                 // becomes the Task<int> result
}

Console.WriteLine(await GetNumberAsync());   // 42
```

Правила:

- `await task` — «подожди завершения без блокировки потока и дай результат (или исключение)»;
- `await` можно использовать только внутри `async`-метода; точка входа тоже может быть `async Task Main`;
- по соглашению имена асинхронных методов заканчиваются на `Async`;
- `return 42` в `async Task<int>` становится результатом задачи — оборачивать вручную не надо.

## Что происходит на await

Если задача ещё не завершена, метод **приостанавливается** и возвращает управление вызывающему (тот получает незавершённый `Task`). Когда ожидание закончится, метод продолжается с места `await`. Код до первого незавершённого `await` выполняется синхронно в потоке вызывающего:

```csharp
static async Task Sync()
{
    Console.WriteLine("before await");
    await Task.Yield();                 // forces the method to suspend
    Console.WriteLine("after await");
}

var t = Sync();
Console.WriteLine("returned to caller");
await t;
```

```text
before await
returned to caller
after await
```

Задача «горячая»: она стартует в момент вызова метода, а не в момент `await`:

```csharp
static async Task Hot(string n)
{
    Console.WriteLine($"{n} started");
    await Task.Delay(30);
    Console.WriteLine($"{n} finished");
}

var t1 = Hot("A");
Console.WriteLine("created A, not awaited yet");
await t1;
```

```text
A started
created A, not awaited yet
A finished
```

## Последовательно и параллельно

Два `await` подряд выполняются друг за другом. Чтобы операции шли одновременно, сначала запустите обе, затем дождитесь вместе через `Task.WhenAll`:

```csharp
static async Task<int> Work(int id) { await Task.Delay(200); return id * 10; }

var a = await Work(1);                                  // ~200 ms
var b = await Work(2);                                  // + ~200 ms = ~400 ms

int[] results = await Task.WhenAll(Work(1), Work(2));   // ~200 ms in total
Console.WriteLine(string.Join(",", results));           // 10,20
```

Проверено: последовательный вариант занимает ~400 мс, `WhenAll` — ~200 мс. `Task.WhenAll` возвращает массив результатов в порядке аргументов. `Task.WhenAny` завершается, когда завершилась первая из задач, и возвращает **её** (`Task`), а не результат:

```csharp
var slow = Task.Delay(1000);
var fast = Task.Delay(50);
var winner = await Task.WhenAny(slow, fast);
Console.WriteLine(winner == fast);   // True
```

## Исключения

Исключение внутри `async`-метода сохраняется в задаче и **пробрасывается при `await`**, как обычное:

```csharp
static async Task Fail() { await Task.Delay(10); throw new InvalidOperationException("boom"); }

try { await Fail(); }
catch (Exception e) { Console.WriteLine($"caught {e.GetType().Name}: {e.Message}"); }
// caught InvalidOperationException: boom
```

Если не сделать `await`, исключение останется внутри задачи и никто его не заметит — не вызывайте асинхронный метод «в пустоту». Подробнее о `Wait()`/`Result` и `AggregateException` — в [продвинутой теории](advanced.ru.md#исключения-и-aggregateexception).

## Отмена: CancellationToken

Асинхронные операции отменяют **кооперативно**: источник `CancellationTokenSource` выдаёт токен, операция принимает его и сама реагирует:

```csharp
static async Task Loop(CancellationToken ct)
{
    while (true)
    {
        ct.ThrowIfCancellationRequested();   // cooperative check
        await Task.Delay(30, ct);            // the delay itself reacts to the token
    }
}

using var cts = new CancellationTokenSource(100);        // cancels after 100 ms
try { await Loop(cts.Token); }
catch (OperationCanceledException e)
{
    Console.WriteLine(e.GetType().Name);                 // TaskCanceledException
}
```

Проверено: брошено `TaskCanceledException` — наследник `OperationCanceledException`, поэтому ловите базовый тип. Задача, прерванная отменой, получает статус `Canceled`, а не `Faulted`. Правило: любой метод, который может работать долго, принимает `CancellationToken ct` последним параметром и передаёт его дальше.

## Task.Run: вычисления в фоне

`Task.Run` запускает делегат на потоке пула — так тяжёлые вычисления выносят из потока, который нельзя занимать (например, UI):

```csharp
Console.WriteLine(Thread.CurrentThread.IsThreadPoolThread);                         // False in Main
Console.WriteLine(await Task.Run(() => Thread.CurrentThread.IsThreadPoolThread));   // True
```

`Task.Run` не делает ввод-вывод асинхронным и нужен только для CPU-работы. Для I/O используйте настоящие асинхронные API (`HttpClient.GetStringAsync`, `File.ReadAllTextAsync`).

## Ошибки новичка

- ❌ `async void` → ✅ `async Task`: из `async void` нельзя дождаться и нельзя поймать исключение (допустимо только для обработчиков событий).
- ❌ `.Result` / `.Wait()` в асинхронном коде → ✅ `await`: блокировка может привести к deadlock ([продвинутая теория](advanced.ru.md#sync-over-async-и-deadlock)).
- ❌ `await` по очереди для независимых операций → ✅ `Task.WhenAll`.
- ❌ Забытый `await` → ✅ всегда `await`: иначе исключения теряются, а код идёт дальше, не дождавшись.
- ❌ `Task.Run` вокруг I/O «чтобы стало асинхронно» → ✅ вызывать асинхронный API напрямую.
- ❌ Не принимать `CancellationToken` → ✅ добавлять его в каждый долгий метод.

## Связанные материалы

- Дальше: [продвинутая теория](advanced.ru.md) — `ConfigureAwait`, deadlock, `ValueTask`, async-потоки.
- [09 · Делегаты, события, лямбды](../09-delegates-events-lambdas/overview.ru.md) — лямбды, которые можно сделать `async`.
- [18 · Многопоточность](../18-concurrency/overview.ru.md) — потоки, `SemaphoreSlim`, `Channel<T>`.
- [08 · Исключения](../08-exceptions/overview.ru.md) — как работают `try`/`catch`.
- [Python](../../../python/overview.ru.md) — `asyncio`: похожая модель `async`/`await`, но один поток и цикл событий.

## Источники

- [Асинхронное программирование (C#)](https://learn.microsoft.com/dotnet/csharp/asynchronous-programming/)
- [Task-based asynchronous pattern (TAP)](https://learn.microsoft.com/dotnet/standard/asynchronous-programming-patterns/task-based-asynchronous-pattern-tap)
- [Отмена в управляемых потоках](https://learn.microsoft.com/dotnet/standard/threading/cancellation-in-managed-threads)
