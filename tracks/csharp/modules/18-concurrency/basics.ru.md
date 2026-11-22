---
title: "Многопоточность и синхронизация: базовая теория"
lang: ru
track: csharp
kind: module
doc: basics
slug: 18-concurrency
part: 3
tags: [concurrency, threads, thread-pool, race-condition, lock, monitor, interlocked, concurrent-collections]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-21
---

# Многопоточность и синхронизация: базовая теория

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [обзор](overview.ru.md) · **1. Базовая** · [2. Продвинутая](advanced.ru.md) · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md) · [упражнения](exercises.ru.md) · [5–6. Код и тесты](code/)

**Пререквизиты:** [03 · Управляющие конструкции и методы](../03-control-flow-and-methods/overview.ru.md) — лямбды и замыкания; [02 · Типы и переменные](../02-types-and-variables/overview.ru.md) — значимые и ссылочные типы.

## Зачем это нужно

Современный процессор имеет много ядер, и программа, которая всё делает в одном потоке, использует только одно из них. Несколько потоков (threads) позволяют выполнять работу параллельно и не блокировать интерфейс или обработку запросов, пока идёт долгая операция.

Цена — **общее состояние** (shared state). Пока данные читает и меняет один поток, всё просто. Как только к одной переменной обращаются несколько потоков и хотя бы один из них пишет, нужна **синхронизация**. Без неё результат зависит от того, в каком порядке операционная система переключила потоки, а такие ошибки воспроизводятся случайно.

## Потоки, пул потоков и задачи

| Способ | Что это | Когда использовать |
|---|---|---|
| `new Thread(...)` | отдельный поток ОС, по умолчанию foreground | редко: долгая фоновая работа с особыми требованиями |
| `ThreadPool` / `Task.Run` | работа ставится в очередь пула переиспользуемых потоков | короткие вычисления, основная масса фоновой работы |
| `Task.Factory.StartNew(…, TaskCreationOptions.LongRunning)` | задача, для которой планировщик выделяет отдельный поток | долгая блокирующая работа, чтобы не занимать поток пула |

```csharp
// A new Thread is a foreground thread by default: the process waits for it before exiting.
public static ThreadKind DedicatedThread()
{
    ThreadKind result = default;
    var thread = new Thread(() => result = Current());
    thread.Start();
    thread.Join();
    return result;
}

// Task.Run queues work to the thread pool; pool threads are background threads.
public static ThreadKind PoolTask() => Task.Run(Current).GetAwaiter().GetResult();

// LongRunning tells the default scheduler not to occupy a pool thread with a long blocking job.
public static ThreadKind LongRunningTask() =>
    Task.Factory.StartNew(Current, TaskCreationOptions.LongRunning).GetAwaiter().GetResult();
```

| Вызов | `IsThreadPoolThread` | `IsBackground` |
|---|---|---|
| `DedicatedThread()` | `False` | `False` |
| `PoolTask()` | `True` | `True` |
| `LongRunningTask()` | `False` | `True` |

- `Start()` запускает поток, `Join()` ждёт его завершения.
- Процесс не завершится, пока работает хотя бы один **foreground**-поток. Background-потоки (в том числе все потоки пула) завершаются вместе с процессом.
- В обычном коде пишут `Task.Run` и `await`. Асинхронность подробно разбирается в модуле [10 · Async/await](../10-async-await/overview.ru.md), здесь — синхронизация потоков.

Код: [Ex02_ThreadKinds.cs](code/Examples/Ex02_ThreadKinds.cs).

## Гонка данных

Счётчик, который увеличивают четыре потока по миллиону раз:

```csharp
// count++ is read-modify-write: two threads can read the same value and lose an update.
public sealed class UnsafeCounter : ICounter
{
    private int _value;

    public void Increment() => _value++;

    public int Value => _value;
}
```

```text
CounterRunner.Run(new UnsafeCounter(), threads: 4, iterations: 1_000_000)
1966527
```

Ожидалось 4 000 000, получилось меньше половины, и при каждом запуске число разное. `_value++` — это три шага: прочитать значение, прибавить единицу, записать. Если два потока прочитали одно и то же значение, оба запишут одинаковый результат, и одно увеличение потеряется. Это **гонка данных** (race condition).

## lock: взаимное исключение

Оператор `lock` пускает в блок кода только один поток за раз. Остальные ждут, пока он выйдет:

```csharp
// The classic lock: any private reference-type object works as a Monitor.
public sealed class MonitorCounter : ICounter
{
    private readonly object _gate = new();
    private int _value;

    public void Increment()
    {
        lock (_gate)
        {
            _value++;
        }
    }
```

Начиная с .NET 9 и C# 13 для блокировки рекомендуется отдельный объект типа `System.Threading.Lock` — так быстрее:

```csharp
// .NET 9+: a dedicated Lock object; the compiler turns lock(_gate) into using (_gate.EnterScope()).
public sealed class LockCounter : ICounter
{
    private readonly Lock _gate = new();
    private int _value;

    public void Increment()
    {
        lock (_gate)
        {
            _value++;
        }
    }
```

Оба счётчика дают ровно `4000000`. Правила:

- Блокируют **приватный** объект, который существует только для этого. Не блокируют `this`, объекты `Type` и строки: их могут заблокировать в чужом коде, а строковые литералы ещё и интернированы (см. [02 · Типы и переменные](../02-types-and-variables/expert.ru.md)).
- Чтение защищённых данных тоже идёт под тем же `lock`, иначе можно увидеть промежуточное состояние.
- Внутри `lock` делают как можно меньше работы: остальные потоки в это время ждут.
- `lock` освобождается даже при исключении: компилятор оборачивает блок в `try/finally`.

Код: [Ex01_Counters.cs](code/Examples/Ex01_Counters.cs).

## Interlocked: атомарные операции

Для простых операций над одной переменной блокировка не нужна. Класс `Interlocked` выполняет их атомарно — как неделимую операцию процессора:

```csharp
// An atomic hardware operation: no lock needed for a simple counter.
public sealed class InterlockedCounter : ICounter
{
    private int _value;

    public void Increment() => Interlocked.Increment(ref _value);

    public int Value => Volatile.Read(ref _value);
}
```

| Метод | Что делает | Возвращает |
|---|---|---|
| `Increment(ref x)` / `Decrement(ref x)` | `x + 1` / `x - 1` | новое значение |
| `Add(ref x, n)` | `x + n` | новое значение |
| `Exchange(ref x, v)` | записывает `v` | **старое** значение |
| `CompareExchange(ref x, v, expected)` | записывает `v`, только если `x == expected` | **старое** значение |

Пример: при `x = 5` вызов `Exchange(ref x, 7)` возвращает `5`. Затем `CompareExchange(ref x, 9, 7)` возвращает `7` и записывает 9, а `CompareExchange(ref x, 1, 0)` возвращает `9` и ничего не меняет, потому что `x` не равен 0.

`Interlocked` подходит для одной переменной. Если несколько полей должны меняться согласованно, нужен `lock` ([упражнение 1](exercises.ru.md)).

## Потокобезопасные коллекции

Обычные `List<T>` и `Dictionary<TKey, TValue>` не рассчитаны на одновременную запись из нескольких потоков. В `System.Collections.Concurrent` есть коллекции, которые синхронизируются сами: `ConcurrentDictionary`, `ConcurrentQueue`, `ConcurrentStack`, `ConcurrentBag`.

```csharp
// AddOrUpdate retries the update delegate until it wins, so no increment is lost.
public static IReadOnlyDictionary<string, int> CountWords(IEnumerable<string> words, int threads)
{
    var counts = new ConcurrentDictionary<string, int>();
    Parallel.ForEach(words, new ParallelOptions { MaxDegreeOfParallelism = threads }, word =>
        counts.AddOrUpdate(word, 1, (_, current) => current + 1));
    return counts;
}
```

30 000 слов (`a`, `b`, `a` × 10 000) в 8 потоков дают точно `a = 20000`, `b = 10000`. Код: [Ex03_ConcurrentCollections.cs](code/Examples/Ex03_ConcurrentCollections.cs).

## Ошибки новичка

- ❌ «У меня всё работает» без синхронизации → ✅ гонка проявляется случайно; общие изменяемые данные защищают всегда.
- ❌ `lock (this)` или `lock ("key")` → ✅ `private readonly Lock _gate = new();`.
- ❌ Защитить запись, но читать без `lock` → ✅ чтение и запись под одной блокировкой.
- ❌ Долгая работа (сеть, файлы) внутри `lock` → ✅ внутри — только изменение общих данных.
- ❌ `Dictionary` из нескольких потоков → ✅ `ConcurrentDictionary` или `lock`.
- ❌ `new Thread` на каждую мелкую задачу → ✅ `Task.Run`: потоки пула переиспользуются.

## Связанные материалы

- Дальше: [продвинутая теория](advanced.ru.md) — семафоры, `ReaderWriterLockSlim`, сигналы, каналы, взаимоблокировки
- Пререквизиты: [03 · Управляющие конструкции и методы](../03-control-flow-and-methods/overview.ru.md), [02 · Типы и переменные](../02-types-and-variables/overview.ru.md)
- То же в других треках: [Python](../../../python/overview.ru.md) — модуль 15 «Потоки и процессы», GIL и `threading.Lock`

## Источники

- [Оператор lock](https://learn.microsoft.com/dotnet/csharp/language-reference/statements/lock)
- [`System.Threading.Lock`](https://learn.microsoft.com/dotnet/api/system.threading.lock)
- [Обзор примитивов синхронизации](https://learn.microsoft.com/dotnet/standard/threading/overview-of-synchronization-primitives)
- [Управляемый пул потоков](https://learn.microsoft.com/dotnet/standard/threading/the-managed-thread-pool)
- [`ConcurrentDictionary<TKey, TValue>`](https://learn.microsoft.com/dotnet/api/system.collections.concurrent.concurrentdictionary-2)
