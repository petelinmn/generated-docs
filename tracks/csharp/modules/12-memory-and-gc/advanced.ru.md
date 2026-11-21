---
title: "Память и GC: продвинутая теория"
lang: ru
track: csharp
kind: module
doc: advanced
slug: 12-memory-and-gc
part: 2
tags: [idisposable, dispose-pattern, iasyncdisposable, finalizer, safehandle, memory-leak, weak-reference, conditional-weak-table, allocations, server-gc]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-21
---

# Память и GC: продвинутая теория

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · **2. Продвинутая** · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md) · [упражнения](exercises.ru.md) · [5–6. Код и тесты](code/)

## Реализация IDisposable

Самый частый случай — `sealed`-класс, который владеет другими освобождаемыми объектами. Финализатор ему не нужен:

```csharp
// The common case: a sealed class that owns other disposable objects. No finalizer needed.
public sealed class ReportWriter : IDisposable
{
    private readonly StringWriter _writer = new();
    private bool _disposed;

    public int DisposeCalls { get; private set; }

    public void WriteLine(string line)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        _writer.WriteLine(line);
    }

    // Idempotent: a second call does nothing and does not throw.
    public void Dispose()
    {
        DisposeCalls++;
        if (_disposed)
        {
            return;
        }
        _writer.Dispose();
        _disposed = true;
    }
}
```

Правила:

- `Dispose` должен быть **идемпотентным**: повторный вызов ничего не делает и не бросает исключений. В тесте два вызова прошли без ошибок.
- После `Dispose` методы объекта бросают `ObjectDisposedException`. Короткая форма — `ObjectDisposedException.ThrowIf(condition, this)` (появилась в .NET 7).
- Освобождай то, чем **владеешь**: объекты, которые класс создал сам. То, что пришло снаружи (через конструктор или DI), освобождает его владелец.
- `Dispose` не должен бросать исключений: он часто вызывается из `finally`, и его исключение заменит исходное (см. [экспертную теорию](expert.ru.md#исключение-в-dispose-подменяет-исходное)).

Код: [Ex04_DisposePattern.cs](code/Examples/Ex04_DisposePattern.cs).

### Шаблон Dispose(bool) для иерархии

Если у класса могут быть наследники, применяют шаблон с виртуальным `Dispose(bool disposing)`:

```csharp
// The pattern for a class that may have derived classes: a virtual Dispose(bool).
public class ResourceBase : IDisposable
{
    private bool _disposed;

    public List<string> Log { get; } = [];

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this); // a derived class may add a finalizer
    }

    // disposing == true: called from Dispose, managed objects may be touched.
    // disposing == false: called from a finalizer, other managed objects may already be finalized.
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }
        if (disposing)
        {
            Log.Add("base: managed");
        }
        _disposed = true;
    }
}
```

Наследник переопределяет `Dispose(bool)`, освобождает своё и в конце вызывает `base.Dispose(disposing)`. Результат для `DerivedResource` после двух вызовов `Dispose`: `["derived: managed", "base: managed"]` — каждая часть освобождена один раз, наследник раньше базы.

## IAsyncDisposable и await using

Если освобождение требует ввода-вывода — сбросить буфер в файл или сеть, закрыть соединение вежливо, — его делают асинхронным:

```csharp
// Implements both interfaces: await using calls DisposeAsync, a plain using calls Dispose.
public sealed class BufferedSink(List<string> log) : IAsyncDisposable, IDisposable
{
    private readonly List<string> _buffer = [];

    public void Write(string item) => _buffer.Add(item);

    // Flushing may need I/O, so the asynchronous path is the main one.
    public async ValueTask DisposeAsync()
    {
        await Task.Yield(); // stands in for an asynchronous flush
        log.Add($"flushed {_buffer.Count} async");
        _buffer.Clear();
    }

    public void Dispose()
    {
        log.Add($"flushed {_buffer.Count} sync");
        _buffer.Clear();
    }
}
```

`await using (var sink = …)` дал `["body", "flushed 2 async"]`, а обычный `using` — `["flushed 1 sync"]`. Компилятор проверяет соответствие: `using` для типа только с `IAsyncDisposable` — ошибка CS8418, `await using` для типа только с `IDisposable` — ошибка CS8417. Поэтому классу, который освобождается асинхронно, полезно реализовать оба интерфейса: синхронный вызывающий код тоже сможет его освободить. Код: [Ex05_AsyncDispose.cs](code/Examples/Ex05_AsyncDispose.cs).

## Финализаторы

Финализатор (`~ClassName()`) — метод, который GC вызывает перед освобождением памяти объекта. Это последний шанс освободить неуправляемый ресурс, если `Dispose` не вызвали.

```csharp
public class FinalizableResource(FinalizerProbe probe) : IDisposable
{
    public void Dispose()
    {
        // release the resource here...
        GC.SuppressFinalize(this); // ...and tell the GC that the finalizer is no longer needed
    }

    // Runs on the finalizer thread at an unknown moment after the object becomes unreachable.
    ~FinalizableResource() => probe.Finalized = true;
}
```

Что показали примеры:

| Сценарий | Результат |
|---|---|
| объект брошен без `Dispose`, затем `GC.Collect()` + `GC.WaitForPendingFinalizers()` | финализатор выполнился |
| перед этим вызван `Dispose` с `GC.SuppressFinalize` | финализатор не выполнился |
| объект с финализатором после первой сборки | ещё в памяти; освобождён только следующей сборкой |

Почему финализаторы — крайняя мера:

- **Неизвестно когда.** Финализатор выполняется в отдельном потоке финализации, когда-нибудь после сборки. Ресурс может «висеть» долго.
- **Дорого.** Объект с финализатором переживает лишнюю сборку и продвигается в старшее поколение — вместе со всем, на что ссылается.
- **Порядок не гарантирован.** Объекты, на которые ссылается финализируемый объект, могут быть уже финализированы — поэтому при `disposing == false` к другим управляемым объектам не обращаются.
- **При завершении процесса финализаторы не вызываются.** Проверено: объект с финализатором, который печатает строку, остался недостижимым к концу `Main` — строка так и не появилась.

Код: [Ex06_Finalizers.cs](code/Examples/Ex06_Finalizers.cs).

## SafeHandle вместо финализатора

Неуправляемый ресурс (дескриптор файла, сокета, указатель из нативной библиотеки) почти никогда не оборачивают своим финализатором. Для этого есть `SafeHandle`: у него уже есть надёжный финализатор, подсчёт ссылок на время вызовов в нативный код и гарантия освобождения ровно один раз. Писать нужно только `ReleaseHandle`:

```csharp
// SafeHandle already has a critical finalizer, reference counting and "release exactly once".
// All we write is how to release the handle.
public sealed class FakeFileHandle : SafeHandleZeroOrMinusOneIsInvalid
{
    private readonly FakeOs _os;

    public FakeFileHandle(FakeOs os, nint handle) : base(ownsHandle: true)
    {
        _os = os;
        SetHandle(handle);
    }

    protected override bool ReleaseHandle()
    {
        _os.Close(handle);
        return true;
    }
}
```

Двойной `Dispose` закрыл дескриптор один раз (`Closed = [100]`, `IsClosed = True`), а для значения `-1` («нет дескриптора») `ReleaseHandle` не вызывался вовсе. Класс, который хранит `SafeHandle`, реализует обычный простой `Dispose` из первого раздела — без своего финализатора. Код: [Ex07_SafeHandles.cs](code/Examples/Ex07_SafeHandles.cs).

## Утечки памяти в управляемом коде

GC не освобождает то, что достижимо. Утечка в .NET — это ссылка, о которой забыли. Самый частый пример — событие долгоживущего объекта:

```csharp
// A short-lived object, e.g. a view model of a closed window.
public sealed class PriceView : IDisposable
{
    private readonly PriceFeed _feed;

    public PriceView(PriceFeed feed)
    {
        _feed = feed;
        _feed.PriceChanged += OnPriceChanged; // the feed now references this view
    }

    public decimal LastPrice { get; private set; }

    public void Dispose() => _feed.PriceChanged -= OnPriceChanged;

    private void OnPriceChanged(object? sender, decimal price) => LastPrice = price;
}
```

Подписка `+=` сохраняет в событии `PriceFeed` делегат, а делегат ссылается на `PriceView`. Пока жив источник, жив и подписчик. Результат: без отписки `(ViewAlive: True, Subscribers: 1)` после `GC.Collect()`, с отпиской в `Dispose` — `(False, 0)`. Код: [Ex08_EventLeaks.cs](code/Examples/Ex08_EventLeaks.cs).

Типичные источники утечек:

| Источник | Почему держит объекты | Что делать |
|---|---|---|
| событие долгоживущего объекта | делегат ссылается на подписчика | отписываться в `Dispose`; токен подписки |
| статическая коллекция, кеш без вытеснения | статическое поле — корень | ограничивать размер и время жизни (`MemoryCache` с политиками) |
| `Timer`, фоновая задача | колбэк ссылается на объект | `Dispose` таймера, отмена через `CancellationToken` |
| лямбда, сохранённая в долгоживущем делегате | замыкание захватывает переменные и `this` | не захватывать лишнего; отписываться |
| `GCHandle` без `Free` | дескриптор — корень | `Free` в `finally` |

Найти утечку помогает снимок кучи: `dotnet-gcdump collect --process-id <PID>` сохраняет граф объектов в файл `.gcdump`, а `dotnet-gcdump report <файл>` выводит, какие типы занимают память. Снятие снимка запускает полную сборку и может надолго приостановить процесс. Следить за кучей в реальном времени — `dotnet-counters monitor --process-id <PID> --counters System.Runtime`.

## Слабые ссылки

`WeakReference<T>` ссылается на объект, не удерживая его:

```csharp
// TryGetTarget returns a strong reference while the object is alive, and false after it is collected.
public static (bool BeforeCollect, bool AfterCollect) TryGetTarget()
{
    WeakReference<byte[]> weak = CreateWeak();
    bool before = HasTarget(weak);
    GC.Collect();
    bool after = HasTarget(weak);
    return (before, after);
}
```

Результат: `(True, False)`. `ConditionalWeakTable<TKey, TValue>` прикрепляет данные к чужим объектам: запись живёт, пока жив ключ. В примере из двух записей после сборки осталась одна — та, ключ которой ещё используется (`(2, 1, "kept")`). Код: [Ex09_WeakReferences.cs](code/Examples/Ex09_WeakReferences.cs).

Слабые ссылки не заменяют кеш: объект пропадёт при ближайшей сборке своего поколения, а молодые объекты собираются очень часто. Для кешей — `MemoryCache` с ограничением размера и временем жизни.

## Измерение выделений памяти

Меньше выделений — реже сборки. `GC.GetAllocatedBytesForCurrentThread()` показывает, сколько байт выделил текущий поток:

```csharp
// Runs the action once to warm it up (JIT, static constructors), then measures the second run.
public static long Measure(Action action)
{
    action();
    long before = GC.GetAllocatedBytesForCurrentThread();
    action();
    return GC.GetAllocatedBytesForCurrentThread() - before;
}
```

Реальные результаты на 64-битном runtime:

| Действие | Выделено байт |
|---|---|
| добавить 1000 `int` в `ArrayList` (упаковка каждого) | 24 000 |
| добавить 1000 `int` в `List<int>` с готовой ёмкостью | 0 |
| `new object()` | 24 |
| `new byte[0]` | 24 |
| `new string('x', 10)` | 48 |

Даже пустой объект занимает 24 байта: 8 байт заголовка, 8 байт указателя на тип и место под данные — меньше 24 байт объект на 64-битном runtime не бывает. Другие счётчики: `GC.GetTotalAllocatedBytes()` — по всему процессу, `GC.CollectionCount(generation)` — сколько было сборок. Код: [Ex10_Allocations.cs](code/Examples/Ex10_Allocations.cs).

## Workstation и Server GC

| | Workstation GC | Server GC |
|---|---|---|
| По умолчанию | консольные и настольные приложения | нет; включается настройкой |
| Куча и потоки GC | одна куча | куча и поток GC на каждое ядро |
| Цель | короткие паузы, мало памяти | пропускная способность сервера |

Включение в `.csproj`: `<ServerGarbageCollection>true</ServerGarbageCollection>`; или переменной окружения `DOTNET_gcServer=1`. Проверено: с этой переменной `GCSettings.IsServerGC` вернул `True`. В обоих вариантах по умолчанию работает **фоновая сборка** (background GC): поколение 2 собирается параллельно с работой программы. С .NET 9 для Server GC по умолчанию включён DATAS — режим, который подстраивает число куч под нагрузку. Подробная таблица настроек — в [экспертной теории](expert.ru.md#настройки-gc).

## Лучшие практики

- Реализуй `IDisposable`, если класс владеет освобождаемым объектом или неуправляемым ресурсом.
- Делай класс `sealed`, если наследование не нужно: тогда хватает простого `Dispose`.
- Неуправляемые ресурсы — через `SafeHandle`, а не свой финализатор.
- Отписывайся от событий долгоживущих объектов; возвращай из `Subscribe` токен `IDisposable`.
- Не вызывай `GC.Collect()` в рабочем коде.
- Измеряй выделения памяти, прежде чем оптимизировать.

## Частые ошибки

- ❌ Финализатор «на всякий случай» в каждом `IDisposable` → ✅ финализатор нужен только классу, который напрямую держит неуправляемый ресурс без `SafeHandle`.
- ❌ `Dispose` бросает исключение при повторном вызове → ✅ идемпотентный `Dispose`.
- ❌ Освобождать объект, полученный через конструктор или DI → ✅ освобождает владелец.
- ❌ В финализаторе обращаться к другим управляемым объектам → ✅ они могут быть уже финализированы.
- ❌ Подписаться на событие синглтона и не отписаться → ✅ утечка: подписчик живёт столько же, сколько синглтон.
- ❌ Только `IAsyncDisposable` у класса, которым пользуются синхронно → ✅ реализовать и `IDisposable`.

## Сравнение с другими треками

| | C# / .NET | Python (CPython) |
|---|---|---|
| Освобождение памяти | трассирующий GC с поколениями | подсчёт ссылок + сборщик циклов (модуль `gc`) |
| Когда освобождается объект | при сборке его поколения | обычно сразу, когда счётчик ссылок стал 0 |
| Детерминированное освобождение ресурсов | `IDisposable` + `using` | контекстный менеджер + `with` |
| Финализатор | `~ClassName()` | `__del__` |
| Слабые ссылки | `WeakReference<T>` | модуль `weakref` |

Подробнее — в [треке Python](../../../python/overview.ru.md).

## Связанные материалы

- Раньше: [базовая теория](basics.ru.md)
- Дальше: [экспертная теория](expert.ru.md) — устройство GC, время жизни объектов в JIT, LOH и POH, закрепление, настройки и режимы задержки, вопросы с собеседований
- Освобождение примитивов синхронизации (`SemaphoreSlim`, `Release` в `finally`): [18 · Многопоточность](../18-concurrency/advanced.ru.md)
- Пул контекстов и `Dispose` в EF Core: [15 · EF Core](../15-ef-core/expert.ru.md#производительность)

## Источники

- [Реализация метода Dispose](https://learn.microsoft.com/dotnet/standard/garbage-collection/implementing-dispose)
- [Реализация метода DisposeAsync](https://learn.microsoft.com/dotnet/standard/garbage-collection/implementing-disposeasync)
- [Object.Finalize](https://learn.microsoft.com/dotnet/fundamentals/runtime-libraries/system-object-finalize)
- [SafeHandle](https://learn.microsoft.com/dotnet/api/system.runtime.interopservices.safehandle)
- [ObjectDisposedException.ThrowIf](https://learn.microsoft.com/dotnet/api/system.objectdisposedexception.throwif)
- [ConditionalWeakTable](https://learn.microsoft.com/dotnet/api/system.runtime.compilerservices.conditionalweaktable-2)
- [Workstation и Server GC](https://learn.microsoft.com/dotnet/standard/garbage-collection/workstation-server-gc)
- [Фоновая сборка мусора](https://learn.microsoft.com/dotnet/standard/garbage-collection/background-gc)
- [dotnet-counters](https://learn.microsoft.com/dotnet/core/diagnostics/dotnet-counters)
- [dotnet-gcdump](https://learn.microsoft.com/dotnet/core/diagnostics/dotnet-gcdump)
