---
title: "Упражнения: Async/await"
lang: ru
track: csharp
kind: module
doc: exercises
slug: 10-async-await
part: 2
tags: [async-await, task-whenall, cancellation-token, retry, semaphore-slim, timeout]
status: draft
updated: 2026-11-22
---

# Упражнения: Async/await

🌐 **Русский** · [English](exercises.en.md)

> Решай сам, прежде чем открывать подсказки. Решения — под спойлерами. Код проверен запуском; запускаемых версий в `code/` пока нет (уровень 5 не создан).

## 1. 🟢 Параллельная сумма

**Задача:** напиши `static Task<int> SumAsync(IEnumerable<Func<Task<int>>> sources)`: запускает все источники одновременно и возвращает сумму их результатов.

**Ожидаемый результат:**

```text
SumAsync([ delay50 → 1, delay50 → 2, FromResult(3) ])   → 6   (~50 ms, не ~100 ms)
```

<details><summary>Подсказка</summary>

Сначала запусти все (`Select(s => s())` + `ToList()`), потом один раз `await Task.WhenAll`. Ленивый `Select` без `ToList` запустит операции дважды, если перечислить его повторно.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 1: sum results of several async sources in parallel.
static async Task<int> SumAsync(IEnumerable<Func<Task<int>>> sources)
{
    var tasks = sources.Select(s => s()).ToList(); // start all, then await together
    int[] results = await Task.WhenAll(tasks);
    return results.Sum();
}
```

Проверено: на трёх источниках результат `6`. Ключевая идея — запустить всё до первого `await`.

</details>

## 2. 🟡 Таймаут с отменой операции

**Задача:** напиши `WithTimeoutAsync<T>(Func<CancellationToken, Task<T>> op, TimeSpan timeout, CancellationToken ct = default)`. Если операция не уложилась в `timeout`, она должна быть **отменена**, а метод бросает `TimeoutException`. Если отменил вызывающий (`ct`), пробрасывается обычная отмена.

**Ожидаемый результат:**

```text
операция 1000 мс, timeout 50 мс      → TimeoutException "Operation timed out after 00:00:00.0500000."
операция 10 мс,   timeout 1 с        → результат "ok"
ct уже отменён                       → TaskCanceledException
```

<details><summary>Подсказка</summary>

Нужен связанный источник токена: `CreateLinkedTokenSource(ct)` + `CancelAfter(timeout)`. Таймаут отличается от отмены вызывающим по состоянию внешнего `ct` — используй фильтр `catch … when`.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 2: timeout that also cancels the underlying operation.
static async Task<T> WithTimeoutAsync<T>(Func<CancellationToken, Task<T>> op, TimeSpan timeout, CancellationToken ct = default)
{
    using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
    cts.CancelAfter(timeout);
    try
    {
        return await op(cts.Token);
    }
    catch (OperationCanceledException) when (!ct.IsCancellationRequested)
    {
        throw new TimeoutException($"Operation timed out after {timeout}.");
    }
}
```

`Task.WaitAsync(timeout)` не подошёл бы: он перестаёт ждать, но операция продолжает работать. Связанный токен реально прерывает операцию.

</details>

## 3. 🟡 Ограничение параллелизма

**Задача:** напиши `ForEachLimitedAsync<T>(IEnumerable<T> items, int maxConcurrency, Func<T, CancellationToken, Task> body, CancellationToken ct = default)`: выполняет `body` для каждого элемента, но одновременно — не больше `maxConcurrency`. Без `Parallel.ForEachAsync`.

**Ожидаемый результат:** на 12 элементах и `maxConcurrency = 3` максимум одновременно работающих — `3`.

<details><summary>Подсказка</summary>

`SemaphoreSlim(maxConcurrency)`: перед запуском — `await gate.WaitAsync(ct)`, после завершения (в `finally`) — `Release()`. В конце — `Task.WhenAll`.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 3: bounded parallelism.
static async Task ForEachLimitedAsync<T>(IEnumerable<T> items, int maxConcurrency, Func<T, CancellationToken, Task> body, CancellationToken ct = default)
{
    using var gate = new SemaphoreSlim(maxConcurrency);
    var tasks = new List<Task>();
    foreach (var item in items)
    {
        await gate.WaitAsync(ct);
        tasks.Add(Run(item));
    }
    await Task.WhenAll(tasks);

    async Task Run(T item)
    {
        try { await body(item, ct); }
        finally { gate.Release(); }
    }
}
```

Проверено: счётчик одновременных вызовов (через `Interlocked`) показал максимум `3`. `Release` в `finally` обязателен, иначе при исключении слот не вернётся.

</details>

## 4. 🟡 Повтор с задержкой

**Задача:** напиши `RetryAsync<T>(Func<int, CancellationToken, Task<T>> op, int maxAttempts, TimeSpan delay, CancellationToken ct = default)`: вызывает `op(номерПопытки, ct)`; при исключении ждёт `delay` и повторяет, до `maxAttempts` попыток. Последнее исключение пробрасывается. После отмены `ct` повторов нет.

**Ожидаемый результат:**

```text
op падает на попытках 1 и 2, на 3-й успех, maxAttempts = 5 → "ok on attempt 3"
op падает всегда, maxAttempts = 3                           → InvalidOperationException "fail 3"
```

<details><summary>Подсказка</summary>

`for (int attempt = 1; ; attempt++)` + `try/catch` с фильтром `when (attempt < maxAttempts && !ct.IsCancellationRequested)`: на последней попытке фильтр ложен — исключение выходит наружу.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 4: retry with fixed delay.
static async Task<T> RetryAsync<T>(Func<int, CancellationToken, Task<T>> op, int maxAttempts, TimeSpan delay, CancellationToken ct = default)
{
    for (int attempt = 1; ; attempt++)
    {
        try
        {
            return await op(attempt, ct);
        }
        catch (Exception) when (attempt < maxAttempts && !ct.IsCancellationRequested)
        {
            await Task.Delay(delay, ct);
        }
    }
}
```

Фильтр `when` не перехватывает исключение последней попытки, поэтому стек вызовов сохраняется. В реальных проектах такую логику даёт библиотека Polly (ещё не создана).

</details>

## 5. 🔴 Первый успешный результат

**Задача:** напиши `FirstSuccessfulAsync<T>(IReadOnlyList<Func<CancellationToken, Task<T>>> ops, CancellationToken ct = default)`: запускает все операции, возвращает результат **первой успешной**, а остальные отменяет. Если упали все — `AggregateException` со всеми ошибками.

**Ожидаемый результат:**

```text
a: падает через 10 мс; b: "b" через 40 мс; c: "c" через 500 мс  → "b", операция c отменена
все падают                                                         → AggregateException с 2 внутренними
```

<details><summary>Подсказка</summary>

Цикл `Task.WhenAny(pending)`; завершившуюся задачу убирай из списка. Для победителя вызови `await cts.CancelAsync()`. Операции передавай один общий токен связанного источника.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 5: first successful result, cancel the rest.
static async Task<T> FirstSuccessfulAsync<T>(IReadOnlyList<Func<CancellationToken, Task<T>>> ops, CancellationToken ct = default)
{
    using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
    var pending = ops.Select(op => op(cts.Token)).ToList();
    var errors = new List<Exception>();
    while (pending.Count > 0)
    {
        var done = await Task.WhenAny(pending);
        pending.Remove(done);
        if (done.IsCompletedSuccessfully)
        {
            await cts.CancelAsync();   // stop the losers
            return done.Result;
        }
        errors.Add(done.Exception!.GetBaseException());
    }
    throw new AggregateException("All operations failed.", errors);
}
```

Проверено: на примере из задания вернулось `b`, операция `c` получила отмену; при двух падающих операциях бросается `AggregateException` с 2 ошибками. Ограничение решения: делегат, который бросает исключение **синхронно** (не `async`), оборвёт метод на `Select`; в продакшен-коде оборачивайте вызов в `Task.Run` или `try/catch`.

</details>
