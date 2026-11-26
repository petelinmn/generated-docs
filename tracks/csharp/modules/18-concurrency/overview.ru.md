---
title: "Многопоточность и синхронизация"
lang: ru
track: csharp
kind: module
doc: overview
slug: 18-concurrency
part: 3
levels: [1, 2, 3, 4, 5, 6]
tags: [concurrency, threads, thread-pool, race-condition, lock, monitor, interlocked, semaphore, reader-writer-lock, channels, deadlock, memory-model]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-21
---

# Многопоточность и синхронизация

🌐 **Русский** · [English](overview.en.md)

## Коротко

Несколько потоков выполняют работу параллельно на разных ядрах. Как только они обращаются к общим данным и хотя бы один пишет, без синхронизации возникают гонки: результат зависит от случайного порядка выполнения. .NET предлагает набор инструментов синхронизации — от оператора `lock` и атомарных операций `Interlocked` до семафоров, сигналов и каналов. Главное умение — выбрать самый простой инструмент, который решает задачу.

## Ключевые идеи

- **Гонка данных:** `count++` из нескольких потоков теряет обновления — 4 потока по миллиону итераций дали 1 966 527 вместо 4 000 000.
- **`lock`** пускает в блок один поток за раз. С .NET 9 для него есть тип `System.Threading.Lock`. Внутри `lock` нельзя писать `await` (CS1996).
- **`Interlocked`** атомарно изменяет одну переменную без блокировки. Для нескольких связанных полей нужен `lock`.
- **`SemaphoreSlim`** ограничивает число одновременных операций и работает с `await` через `WaitAsync`.
- **`ReaderWriterLockSlim`, события, `CountdownEvent`, `Barrier`, `Channel<T>`** решают специальные задачи: много читателей, сигналы, передачу данных.
- **Взаимоблокировка** возникает при захвате замков в разном порядке; лечится единым порядком и таймаутами.
- **Модель памяти:** без синхронизации другой поток может видеть изменения в другом порядке; `volatile` даёт порядок, но не атомарность.

## Минимальный пример

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

Четыре потока по миллиону вызовов `Increment()` дают ровно `4000000`.

## Когда применять, а когда нет

- ✅ `lock` по `System.Threading.Lock` — для общих изменяемых данных внутри процесса.
- ✅ `Interlocked` — для счётчиков и флагов.
- ✅ `SemaphoreSlim` и `Channel<T>` — в асинхронном коде.
- ✅ Concurrent-коллекции — когда нужна общая коллекция.
- ❌ Общее изменяемое состояние там, где можно без него: неизменяемые данные и передача сообщений проще синхронизации.
- ❌ `lock (this)`, строки и `Type` как объект блокировки.
- ❌ Блокирующие ожидания (`.Result`, `.Wait()`) на потоках пула.

## Уровни материала

| Уровень | Что внутри |
|---|---|
| [1. Базовая теория](basics.ru.md) | потоки, пул и задачи; гонка данных; `lock` и `System.Threading.Lock`; `Interlocked`; concurrent-коллекции |
| [2. Продвинутая теория](advanced.ru.md) | выбор примитива; `SemaphoreSlim` и `async`; `ReaderWriterLockSlim`; события, `CountdownEvent`, `Barrier`; `Channel<T>`; `GetOrAdd` и `Lazy`; взаимоблокировки; `Parallel` и PLINQ |
| [3. Экспертная теория](expert.ru.md) | устройство `lock`; модель памяти и `volatile`; `Monitor.Wait/Pulse`; lock-free и CAS; `SpinLock`; `AsyncLocal`; пул потоков; справочник примитивов, подвохи, вопросы с собеседований |
| [4. Самопроверка](quiz.ru.md) | квиз и [упражнения](exercises.ru.md) с решениями |
| [5. Запускаемые примеры](code/) | `code/Examples/` и `code/Solutions/` — решение `Code.slnx` |
| [6. Тесты](code/) | `code/Tests/` — `dotnet test` в папке `code/` |

## Связанные материалы

- Пререквизиты: [03 · Управляющие конструкции и методы](../03-control-flow-and-methods/overview.ru.md), [02 · Типы и переменные](../02-types-and-variables/overview.ru.md)
- Применение: `DbContext` в EF Core не потокобезопасен — [15 · EF Core](../15-ef-core/advanced.ru.md#жизненный-цикл-dbcontext)
- `Task`, `async`/`await`, отмена: [10 · Async/await](../10-async-await/overview.ru.md)
- `ArrayPool`, `Pipelines`: [12a · Span и Memory](../12a-span-and-memory/overview.ru.md)
- Пул потоков и производительность: [19 · Производительность](../19-performance/overview.ru.md)
- То же в других треках: [Python](../../../python/overview.ru.md) — GIL, `threading` и `asyncio`
- Рецепты и библиотеки: [Фоновые задачи](../../recipes/background-jobs/overview.ru.md)
- [System design: согласованность и CAP](../../../system-design/modules/07-consistency-and-cap/overview.ru.md)
