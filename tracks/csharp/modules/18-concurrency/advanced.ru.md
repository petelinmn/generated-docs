---
title: "Многопоточность и синхронизация: продвинутая теория"
lang: ru
track: csharp
kind: module
doc: advanced
slug: 18-concurrency
part: 3
tags: [concurrency, semaphore, reader-writer-lock, events, barrier, channels, producer-consumer, deadlock, parallel]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-21
---

# Многопоточность и синхронизация: продвинутая теория

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · **2. Продвинутая** · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md) · [упражнения](exercises.ru.md) · [5–6. Код и тесты](code/)

## Какой примитив выбрать

| Задача | Инструмент |
|---|---|
| Защитить общие данные внутри процесса | `lock` по `System.Threading.Lock` |
| Атомарно изменить одну переменную | `Interlocked` |
| Защитить данные в асинхронном коде (с `await` внутри) | `SemaphoreSlim(1, 1)` + `WaitAsync` |
| Ограничить число одновременных операций | `SemaphoreSlim(N, N)` |
| Много читателей, редкие записи | `ReaderWriterLockSlim` |
| Дождаться события или открыть «ворота» | `ManualResetEventSlim`, `AutoResetEvent` |
| Дождаться завершения N участников | `CountdownEvent` |
| Синхронизировать фазы алгоритма | `Barrier` |
| Передать данные от производителей потребителям | `Channel<T>` |
| Синхронизация между процессами | именованный `Mutex` |

Названия с суффиксом `Slim` — облегчённые версии: они не используют объекты ядра ОС, работают быстрее, но действуют только внутри одного процесса.

## SemaphoreSlim и асинхронный код

`lock` нельзя использовать вокруг `await`: `await` в теле `lock` — ошибка компиляции CS1996. Для асинхронного кода используют `SemaphoreSlim`: его метод `WaitAsync` ждёт, не блокируя поток. Семафор со счётчиком N пропускает не больше N участников одновременно:

```csharp
// Runs all jobs, at most `maxConcurrency` at a time; returns the peak concurrency actually observed.
public static async Task<int> RunAsync(int jobs, int maxConcurrency, TimeSpan jobDuration)
{
    using var semaphore = new SemaphoreSlim(maxConcurrency, maxConcurrency);
    int running = 0;
    int peak = 0;

    async Task RunJobAsync()
    {
        await semaphore.WaitAsync(); // unlike lock, waiting does not block a thread
        try
        {
            int now = Interlocked.Increment(ref running);
            InterlockedMax(ref peak, now);
            await Task.Delay(jobDuration);
        }
        finally
        {
            Interlocked.Decrement(ref running);
            semaphore.Release();
        }
    }

    await Task.WhenAll(Enumerable.Range(0, jobs).Select(_ => RunJobAsync()));
    return peak;
}
```

12 задач с лимитом 3 дают пик ровно `3`. `Release()` всегда стоит в `finally`: иначе исключение «съест» разрешение семафора, и остальные будут ждать вечно. `SemaphoreSlim(1, 1)` — асинхронный аналог `lock`. Код: [Ex04_Throttling.cs](code/Examples/Ex04_Throttling.cs).

## ReaderWriterLockSlim

Если данные часто читают и редко меняют, обычный `lock` заставляет читателей ждать друг друга без причины. `ReaderWriterLockSlim` пускает много читателей одновременно и одного писателя — в одиночку:

```csharp
// Upgradeable read: only one thread at a time may hold it, so check-then-add is safe.
public TValue GetOrAdd(TKey key, Func<TKey, TValue> factory)
{
    _lock.EnterUpgradeableReadLock();
    try
    {
        if (_items.TryGetValue(key, out TValue? existing))
        {
            return existing;
        }
        _lock.EnterWriteLock();
        try
        {
            TValue created = factory(key);
            _items[key] = created;
            return created;
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }
    finally
    {
        _lock.ExitUpgradeableReadLock();
    }
}
```

| Режим | Сколько потоков одновременно | Совместим с |
|---|---|---|
| `EnterReadLock` | много | другими чтениями и одним upgradeable |
| `EnterUpgradeableReadLock` | один | чтениями; может перейти в запись |
| `EnterWriteLock` | один | ни с чем |

100 параллельных `GetOrAdd` для одного ключа вызывают фабрику ровно один раз. Важно: обычную блокировку чтения нельзя «повысить» до записи. По умолчанию (`LockRecursionPolicy.NoRecursion`) это бросает `LockRecursionException` с сообщением «Write lock may not be acquired with read lock held». Для «прочитать и, возможно, записать» есть upgradeable-режим. Код: [Ex05_ReaderWriterCache.cs](code/Examples/Ex05_ReaderWriterCache.cs).

## Сигналы между потоками

Иногда нужно не защитить данные, а **сообщить** другому потоку, что что-то произошло.

| Тип | Поведение |
|---|---|
| `ManualResetEventSlim` | после `Set()` остаётся открытым и пропускает всех, пока не вызван `Reset()` |
| `AutoResetEvent` | после `Set()` пропускает **одного** ожидающего и сам закрывается |
| `CountdownEvent` | открывается, когда счётчик дошёл до нуля (`Signal()` уменьшает его) |
| `Barrier` | все N участников ждут друг друга в конце каждой фазы |

«Ворота» на старт и «финишная черта»:

```csharp
public StartGate(int workers)
{
    _finished = new CountdownEvent(workers);
    for (int i = 0; i < workers; i++)
    {
        new Thread(() =>
        {
            _gate.Wait(); // every worker blocks here until Open is called
            Interlocked.Increment(ref _passed);
            _finished.Signal();
        }).Start();
    }
}
```

Пока ворота закрыты, `Passed` равно `0` при любом раскладе потоков. После `Open()` (то есть `_gate.Set()`) все 5 рабочих проходят, и `WaitAll` возвращает `true`. Для сравнения: с `AutoResetEvent` один `Set()` выпускает ровно одного из трёх ожидающих. Код: [Ex06_Signaling.cs](code/Examples/Ex06_Signaling.cs).

## Производитель и потребитель: Channel\<T\>

`System.Threading.Channels` — асинхронная очередь для передачи данных между частями программы. Ограниченный (bounded) канал сам притормаживает производителя, когда потребители не успевают:

```csharp
Channel<int> channel = Channel.CreateBounded<int>(capacity);

Task producer = Task.Run(async () =>
{
    for (int i = 1; i <= items; i++)
    {
        await channel.Writer.WriteAsync(i); // waits while the channel is full
    }
    channel.Writer.Complete(); // lets ReadAllAsync finish
});

Task<long>[] readers = Enumerable.Range(0, consumers).Select(_ => Task.Run(async () =>
{
    long sum = 0;
    await foreach (int item in channel.Reader.ReadAllAsync())
    {
        sum += item;
    }
    return sum;
})).ToArray();
```

10 000 элементов через канал ёмкостью 16 и 4 потребителя дают сумму ровно `50005000`: каждый элемент прочитан один раз. Режим переполнения по умолчанию — `BoundedChannelFullMode.Wait`: `WriteAsync` ждёт места, а `TryWrite` в полный канал возвращает `false` (`[true, true, false]` для ёмкости 2). `Complete()` обязателен, иначе `ReadAllAsync` будет ждать новых элементов вечно. Код: [Ex07_ProducerConsumer.cs](code/Examples/Ex07_ProducerConsumer.cs).

## ConcurrentDictionary: GetOrAdd и Lazy

Методы `ConcurrentDictionary` атомарны, **кроме делегатов**, которые им передают: фабрика в `GetOrAdd` выполняется вне внутренней блокировки. Если несколько потоков одновременно не нашли ключ, фабрику вызовет каждый из них, а в словарь попадёт один результат:

```csharp
// The value factory runs outside the dictionary's lock: concurrent callers can all run it.
// The barrier forces `callers` threads to be inside the factory at the same time.
public static int FactoryCallsWithGetOrAdd(int callers)
{
    var cache = new ConcurrentDictionary<string, int>();
    int calls = 0;
    using var barrier = new Barrier(callers);
    RunConcurrently(callers, () => cache.GetOrAdd("key", _ =>
    {
        Interlocked.Increment(ref calls);
        barrier.SignalAndWait();
        return 42;
    }));
    return calls;
}
```

8 потоков — 8 вызовов фабрики. Если фабрика дорогая или имеет побочные эффекты, значение оборачивают в `Lazy<T>`: все потоки получают один и тот же объект `Lazy`, и его фабрика выполняется ровно **1** раз. Код: [Ex03_ConcurrentCollections.cs](code/Examples/Ex03_ConcurrentCollections.cs).

## Взаимоблокировки

Взаимоблокировка (deadlock) возникает, когда потоки ждут друг друга по кругу: первый держит `A` и ждёт `B`, второй держит `B` и ждёт `A`. Пример ниже создаёт такую ситуацию гарантированно и обнаруживает её через `Monitor.TryEnter` с таймаутом:

```csharp
var t1 = new Thread(() =>
{
    lock (a)
    {
        bothHoldOne.SignalAndWait();
        first = Monitor.TryEnter(b, timeout);
        if (first) Monitor.Exit(b);
        bothTried.SignalAndWait();
    }
});
var t2 = new Thread(() =>
{
    lock (b)
    {
        bothHoldOne.SignalAndWait();
        second = Monitor.TryEnter(a, timeout);
        if (second) Monitor.Exit(a);
        bothTried.SignalAndWait();
    }
});
```

Результат — `(False, False)`: ни один поток не получил второй замок. С обычным `lock` вместо `TryEnter` программа зависла бы навсегда. Лечение — **единый порядок захвата**: все берут замки в одном и том же порядке.

```csharp
// Both accounts are always locked in the same order (by Id), so no cycle of waits can form.
public static void Transfer(Account from, Account to, decimal amount)
{
    Account first = from.Id < to.Id ? from : to;
    Account second = from.Id < to.Id ? to : from;
    lock (first.Gate)
    {
        lock (second.Gate)
        {
            from.Balance -= amount;
            to.Balance += amount;
        }
    }
}
```

10 000 встречных переводов между двумя счетами проходят без зависания, сумма остаётся `2000`. Другие способы: держать только один замок за раз, использовать таймауты (`TryEnter`, `Wait(timeout)`) и не вызывать чужой код (события, колбэки) под блокировкой. Код: [Ex08_Deadlock.cs](code/Examples/Ex08_Deadlock.cs).

## Parallel и PLINQ

Для параллельной обработки коллекций не нужно создавать потоки вручную:

| API | Для чего |
|---|---|
| `Parallel.For` / `Parallel.ForEach` | CPU-задачи над коллекцией; `MaxDegreeOfParallelism` ограничивает параллелизм |
| `Parallel.ForEachAsync` | асинхронная обработка с ограничением параллелизма (.NET 6+) |
| PLINQ: `.AsParallel()` | параллельный LINQ-запрос |

Проверенные примеры: `Parallel.ForEachAsync` по 20 элементам с `MaxDegreeOfParallelism = 4` никогда не держит больше 4 одновременных задач. `Enumerable.Range(1, 1_000_000).AsParallel().Select(x => (long)x).Sum()` даёт `500000500000`. Перегрузка `Parallel.For` с локальным состоянием накапливает сумму в каждом потоке отдельно и объединяет результаты одним `Interlocked.Add` на поток — это быстрее, чем `Interlocked` на каждой итерации.

## Лучшие практики

- Сначала избегай общего изменяемого состояния: неизменяемые данные, локальные результаты потоков, передача сообщений через `Channel<T>`.
- Если синхронизация нужна — бери самый простой подходящий инструмент: `Interlocked` → `lock` → специализированные примитивы.
- Держи блокировку как можно короче и не вызывай под ней чужой код.
- Захватывай несколько замков в едином порядке.
- `Release`/`Exit`/`Dispose` — только в `finally`.
- В асинхронном коде — `SemaphoreSlim.WaitAsync` и `Channel<T>`, а не блокирующие `Wait()`.

## Частые ошибки

- ❌ `await` внутри `lock` → ✅ ошибка CS1996; используй `SemaphoreSlim(1, 1)` и `WaitAsync`.
- ❌ `semaphore.Release()` не в `finally` → ✅ исключение навсегда уменьшит число разрешений.
- ❌ Повышать read-блокировку до write → ✅ `LockRecursionException`; используй upgradeable-режим.
- ❌ Дорогая фабрика в `ConcurrentDictionary.GetOrAdd` → ✅ `Lazy<T>`: фабрика может выполниться несколько раз.
- ❌ Забыть `Writer.Complete()` → ✅ потребители канала будут ждать вечно.
- ❌ Общий `new Random()` из нескольких потоков → ✅ `Random` не потокобезопасен, при гонке методы начинают возвращать 0; используй `Random.Shared`.
- ❌ Блокировать потоки пула ожиданием (`.Result`, `.Wait()`, `Thread.Sleep`) → ✅ пул ограничен, и задачи перестают запускаться; см. [экспертную теорию](expert.ru.md).

## Сравнение с другими треками

| | C# | Python |
|---|---|---|
| Взаимное исключение | `lock`, `Lock` | `threading.Lock` |
| Асинхронная блокировка | `SemaphoreSlim.WaitAsync` | `asyncio.Lock` |
| Очередь производитель–потребитель | `Channel<T>` | `queue.Queue`, `asyncio.Queue` |
| Параллельные вычисления на CPU | потоки работают параллельно на всех ядрах | в CPython потоки ограничены GIL; для CPU используют процессы |

Подробно Python — в [треке Python](../../../python/overview.ru.md), модули 14 «Asyncio» и 15 «Потоки и процессы».

## Связанные материалы

- Раньше: [базовая теория](basics.ru.md)
- Дальше: [экспертная теория](expert.ru.md) — модель памяти, `Monitor.Wait/Pulse`, lock-free, справочник примитивов
- Пример из практики: один `DbContext` из нескольких потоков — [15 · EF Core](../15-ef-core/advanced.ru.md#жизненный-цикл-dbcontext)
- То же в других треках: [Python](../../../python/overview.ru.md)

## Источники

- [SemaphoreSlim и Semaphore](https://learn.microsoft.com/dotnet/standard/threading/semaphore-and-semaphoreslim)
- [`ReaderWriterLockSlim`](https://learn.microsoft.com/dotnet/api/system.threading.readerwriterlockslim)
- [Обзор примитивов синхронизации](https://learn.microsoft.com/dotnet/standard/threading/overview-of-synchronization-primitives)
- [System.Threading.Channels](https://learn.microsoft.com/dotnet/core/extensions/channels)
- [`ConcurrentDictionary<TKey, TValue>`](https://learn.microsoft.com/dotnet/api/system.collections.concurrent.concurrentdictionary-2)
- [Оператор lock](https://learn.microsoft.com/dotnet/csharp/language-reference/statements/lock)
- [`System.Random`: потокобезопасность](https://learn.microsoft.com/dotnet/api/system.random)
