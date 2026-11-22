---
title: "Async/await: продвинутая теория"
lang: ru
track: csharp
kind: module
doc: advanced
slug: 10-async-await
part: 2
tags: [async-await, configureawait, synchronization-context, deadlock, valuetask, async-streams, cancellation-token, task-whenall]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-22
---

# Async/await: продвинутая теория

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · **2. Продвинутая** · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [базовая теория](basics.ru.md). **Время:** ~1,5 ч.

## Контекст синхронизации и ConfigureAwait

После `await` метод продолжается там, где «положено»: если на потоке есть `SynchronizationContext` (UI-потоки WPF, WinForms и подобные), продолжение возвращается в него; иначе — на поток пула (`TaskScheduler.Current`). В консольном приложении и в ASP.NET Core контекста синхронизации нет. `ConfigureAwait(false)` говорит «мне всё равно, где продолжить» и пропускает возврат в контекст.

Правило для кода:

- **библиотеки** ставят `ConfigureAwait(false)` на каждом `await`: они не знают, в каком окружении их вызовут;
- **код приложений** (контроллеры ASP.NET Core, обработчики UI, которым нужен UI-поток) пишут без него.

## Sync-over-async и deadlock

Блокирующее ожидание асинхронного метода (`.Result`, `.Wait()`, `GetAwaiter().GetResult()`) называют sync-over-async. Если вызывающий поток занят блокировкой, а продолжение метода должно вернуться именно на него, получается **deadlock**: поток ждёт задачу, задача ждёт поток. Воспроизведение с однопоточным контекстом (как у UI-потока):

```csharp
using System.Collections.Concurrent;

// A context with one worker thread, like a UI thread.
class SingleThreadContext : SynchronizationContext
{
    private readonly BlockingCollection<(SendOrPostCallback, object?)> _queue = new();

    public SingleThreadContext()
    {
        new Thread(() =>
        {
            SetSynchronizationContext(this);
            foreach (var (callback, state) in _queue.GetConsumingEnumerable()) callback(state);
        }) { IsBackground = true }.Start();
    }

    public override void Post(SendOrPostCallback d, object? state) => _queue.Add((d, state));
}

static async Task<int> Blocking() { await Task.Delay(50); return 1; }                          // continuation returns to the context
static async Task<int> BlockingCfg() { await Task.Delay(50).ConfigureAwait(false); return 1; } // continuation runs on the pool

var ctx = new SingleThreadContext();
ctx.Post(_ => Console.WriteLine($"Wait finished within 500 ms: {Blocking().Wait(500)}"), null);
// the lambda blocks the only context thread, so the continuation can never run
await Task.Delay(1000);
ctx.Post(_ => Console.WriteLine($"with ConfigureAwait(false): {BlockingCfg().Wait(500)}"), null);
await Task.Delay(300);
```

```text
Wait finished within 500 ms: False
with ConfigureAwait(false): True
```

Лекарство: не блокироваться, а делать `await` на всём пути вызовов («async all the way»). В ASP.NET Core контекста синхронизации нет, поэтому классический deadlock там не возникает, зато sync-over-async занимает потоки пула и вызывает **исчерпание пула** (thread-pool starvation) под нагрузкой.

## Исключения и AggregateException

`await` разворачивает исключение, `Wait()`/`Result` — оборачивают в `AggregateException`:

```csharp
static async Task Fail() { await Task.Yield(); throw new InvalidOperationException("boom"); }
static async Task<int> Fail2() { await Task.Yield(); throw new InvalidOperationException("boom"); }

try { Fail().GetAwaiter().GetResult(); } catch (Exception e) { Console.WriteLine(e.GetType().Name); }  // InvalidOperationException
try { _ = Fail2().Result; }             catch (Exception e) { Console.WriteLine(e.GetType().Name); }  // AggregateException
```

Если у `Task.WhenAll` падают несколько задач, `await` бросает **только первое** исключение, а все лежат в `Task.Exception`:

```csharp
static async Task F(string m) { await Task.Delay(10); throw new Exception(m); }

var all = Task.WhenAll(F("one"), F("two"));
try { await all; }
catch (Exception e)
{
    Console.WriteLine($"{e.Message}; all.Exception count = {all.Exception!.InnerExceptions.Count}");
}
// one; all.Exception count = 2
```

Не ждавший `await` задачу с ошибкой теряет исключение: оно останется в `Task.Exception`. Необработанные исключения задач не завершают процесс (с .NET 4.5), но потеря ошибок — всё равно баг.

`await` разрешён в `catch` и `finally`; внутри `lock` — нет (`CS1996`), в асинхронных методах нельзя параметры `ref`, `in`, `out` (`CS1988`): для ожидания внутри критической секции используйте `SemaphoreSlim` ([18 · Многопоточность](../18-concurrency/advanced.ru.md)).

## Отмена

### Таймаут и связанные токены

```csharp
using var cts = new CancellationTokenSource();
cts.CancelAfter(50);                                 // cancel in 50 ms
try { await Task.Delay(1000, cts.Token); }
catch (TaskCanceledException) { Console.WriteLine("canceled by timeout"); }

using var a = new CancellationTokenSource();
using var b = new CancellationTokenSource();
using var linked = CancellationTokenSource.CreateLinkedTokenSource(a.Token, b.Token);
b.Cancel();
Console.WriteLine(linked.IsCancellationRequested);   // True: any parent cancels the linked source
```

Связанный источник удобен, чтобы добавить таймаут к токену, пришедшему извне. Для готового ожидания с ограничением есть `Task.WaitAsync` — он **не отменяет** саму операцию, только прекращает её ждать:

```csharp
try { await Task.Delay(1000).WaitAsync(TimeSpan.FromMilliseconds(50)); }
catch (TimeoutException e) { Console.WriteLine(e.Message); }   // The operation has timed out.
```

### Как отличить таймаут от отмены

Таймаут через связанный токен и отмена вызывающим дают одно и то же `OperationCanceledException`. Различают по внешнему токену: если он не отменён, значит, сработал таймаут. Полный шаблон — в [упражнении 2](exercises.ru.md).

## WhenAll, WhenAny, WhenEach

`Task.WhenEach` (с .NET 9) отдаёт задачи **по мере завершения**, а не в порядке передачи:

```csharp
var tasks = new[] { Delay(300, "c"), Delay(100, "a"), Delay(200, "b") };
await foreach (var done in Task.WhenEach(tasks))
    Console.WriteLine($"done: {await done}");
// done: a, done: b, done: c

static async Task<string> Delay(int ms, string s) { await Task.Delay(ms); return s; }
```

`Task.WhenAny` в цикле работает похоже, но стоит O(n²) — для больших наборов используйте `WhenEach`.

## Ограничение параллелизма

`Task.WhenAll` по тысяче задач запустит тысячу операций сразу. Для ограничения — `Parallel.ForEachAsync` (.NET 6+):

```csharp
await Parallel.ForEachAsync(Enumerable.Range(1, 10),
    new ParallelOptions { MaxDegreeOfParallelism = 3 },
    async (i, ct) => await Task.Delay(30, ct));
```

Проверено: одновременно выполнялось не больше 3 итераций. Вариант с `SemaphoreSlim` — в [упражнении 3](exercises.ru.md).

## ValueTask

`Task` — класс: каждое выделение стоит памяти. Если метод **часто** завершается синхронно (кэш, буфер), возвращайте `ValueTask<T>` — структуру, которая не выделяет память в синхронном случае:

```csharp
static ValueTask<int> Cached(Dictionary<int, int> cache, int key) =>
    cache.TryGetValue(key, out var v)
        ? new ValueTask<int>(v)                  // no allocation
        : new ValueTask<int>(SlowAsync(cache, key));

static async Task<int> SlowAsync(Dictionary<int, int> cache, int key)
{
    await Task.Delay(10);
    return cache[key] = key * 2;
}

var cache = new Dictionary<int, int>();
var v1 = Cached(cache, 5); Console.WriteLine(v1.IsCompletedSuccessfully);   // False
await v1;
var v2 = Cached(cache, 5); Console.WriteLine(v2.IsCompletedSuccessfully);   // True
```

Ограничения `ValueTask`: **одно** `await` на экземпляр, нельзя `await` параллельно из нескольких мест, нельзя `.Result` до завершения. Нужно «подержать» результат — вызывайте `AsTask()`. По умолчанию возвращайте `Task`; `ValueTask` — только после замеров ([19 · Производительность](../19-performance/overview.ru.md)).

## Асинхронные потоки и ресурсы

`IAsyncEnumerable<T>` и `await foreach` — последовательность, элементы которой появляются асинхронно:

```csharp
static async IAsyncEnumerable<int> Numbers([EnumeratorCancellation] CancellationToken ct = default)
{
    for (int i = 1; i <= 3; i++)
    {
        await Task.Delay(10, ct);
        yield return i;
    }
}

await foreach (var n in Numbers()) Console.Write($"{n} ");   // 1 2 3
```

Атрибут `[EnumeratorCancellation]` связывает токен, переданный через `.WithCancellation(ct)`, с параметром метода. Ресурс с асинхронной очисткой реализует `IAsyncDisposable` и закрывается через `await using` (подробности `Dispose` — в [12 · Память и GC](../12-memory-and-gc/overview.ru.md)):

```csharp
await using (var r = new Res()) { Console.WriteLine("using body"); }   // using body, then DisposeAsync

class Res : IAsyncDisposable
{
    public async ValueTask DisposeAsync() { await Task.Delay(1); Console.WriteLine("DisposeAsync"); }
}
```

## TaskCompletionSource и PeriodicTimer

`TaskCompletionSource<T>` превращает событие или callback в `Task`:

```csharp
var tcs = new TaskCompletionSource<string>();
_ = Task.Run(async () => { await Task.Delay(30); tcs.SetResult("from callback"); });
Console.WriteLine(await tcs.Task);   // from callback
```

`PeriodicTimer` — асинхронная замена цикла `while + Task.Delay`: тики не накапливаются и не дрейфуют, если обработка быстрая:

```csharp
using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(30));
int ticks = 0;
while (await timer.WaitForNextTickAsync())
    if (++ticks == 3) break;
Console.WriteLine(ticks);   // 3
```

## Fire-and-forget и async void

Запуск без ожидания допустим, только если ошибки обработаны внутри:

```csharp
static async Task DoWorkAsync() { await Task.Delay(10); throw new InvalidOperationException("boom"); }

_ = Task.Run(async () =>
{
    try { await DoWorkAsync(); }
    catch (Exception ex) { Console.WriteLine($"background work failed: {ex.Message}"); }   // otherwise the error is lost
});
await Task.Delay(100);   // background work failed: boom
```

`async void` выбрасывает исключение прямо в `SynchronizationContext` (или в пул, где оно завершит процесс). Проверено на контексте, который ловит исключения: исключение из `async void` попало в `Post` — из `await` оно недоступно. Для долгоживущих фоновых задач в приложениях используйте `BackgroundService` (рецепт «Фоновые задачи» ещё не создан).

## Лучшие практики

| Правило | Почему |
|---|---|
| `async` до конца (async all the way) | иначе deadlock или исчерпание пула |
| `CancellationToken` — последний параметр, передавать дальше | иначе отмена не доходит до операции |
| `Task.WhenAll` для независимых операций | сумма времён → максимум |
| `ConfigureAwait(false)` в библиотеках | независимость от контекста |
| `async Task`, а не `async void` | можно ждать и ловить ошибки |
| Не оборачивать I/O в `Task.Run` | это лишний поток без пользы |
| `await` вместо `.Result`/`.Wait()` | см. выше |
| Не убирать `async`/`await` «для оптимизации» там, где есть `using` или `try` | возврат `Task` без `await` закрывает ресурс или выходит из `try` до завершения операции |

## Связанные материалы

- Назад: [базовая теория](basics.ru.md). Дальше: [экспертная теория](expert.ru.md) — конечный автомат, `ExecutionContext`, `ValueTask` изнутри.
- [18 · Многопоточность](../18-concurrency/overview.ru.md) — `SemaphoreSlim`, `Channel<T>`, `Parallel`.
- [12 · Память и GC](../12-memory-and-gc/overview.ru.md) — `IDisposable`, `IAsyncDisposable`.
- [08 · Исключения](../08-exceptions/overview.ru.md) — `AggregateException`, фильтры `when`.
- [15 · EF Core](../15-ef-core/overview.ru.md) — асинхронные запросы (`ToListAsync`).
- [Python](../../../python/overview.ru.md) — `asyncio`: `gather` ↔ `Task.WhenAll`, `TaskGroup`.

## Источники

- [ConfigureAwait FAQ](https://devblogs.microsoft.com/dotnet/configureawait-faq/)
- [Async streams (`IAsyncEnumerable<T>`)](https://learn.microsoft.com/dotnet/csharp/asynchronous-programming/generate-consume-asynchronous-stream)
- [`ValueTask<T>`](https://learn.microsoft.com/dotnet/api/system.threading.tasks.valuetask-1)
- [`Task.WhenEach`](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task.wheneach)
- [`Parallel.ForEachAsync`](https://learn.microsoft.com/dotnet/api/system.threading.tasks.parallel.foreachasync)
- [`PeriodicTimer`](https://learn.microsoft.com/dotnet/api/system.threading.periodictimer)
