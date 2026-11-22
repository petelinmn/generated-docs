---
title: "Память и GC"
lang: ru
track: csharp
kind: module
doc: overview
slug: 12-memory-and-gc
part: 2
levels: [1, 2, 3, 4, 5, 6]
tags: [gc, garbage-collection, managed-heap, generations, large-object-heap, idisposable, using, finalizer, safehandle, memory-leak, weak-reference, pinning, server-gc]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-21
---

# Память и GC

🌐 **Русский** · [English](overview.en.md)

## Коротко

В .NET память под объекты выделяет `new`, а освобождает сборщик мусора (garbage collector, GC): он находит объекты, недостижимые из корней программы, и освобождает их память. Чтобы сборки были быстрыми, куча разделена на поколения: молодые объекты проверяются часто, старые — редко. Но GC управляет только памятью: файлы, сокеты и подключения освобождают детерминированно — через `IDisposable` и `using`. А утечки памяти в .NET тоже бывают: это ссылки, о которых забыли.

## Ключевые идеи

- **Достижимость, а не подсчёт ссылок:** GC обходит граф от корней (стеки потоков, статические поля, GC-дескрипторы); недостижимые циклы собираются.
- **Поколения 0, 1, 2:** выживший объект продвигается в следующее поколение. Объекты от 85 000 байт сразу попадают в LOH, который собирается вместе с поколением 2.
- **`IDisposable` и `using`** освобождают ресурсы в известный момент, даже при исключении; `using var` — в конце блока, в обратном порядке.
- **Финализатор — крайняя мера:** выполняется неизвестно когда, продлевает жизнь объекта на сборку, при завершении процесса не вызывается. Для дескрипторов ОС — `SafeHandle`.
- **Утечки в управляемом коде** — события долгоживущих объектов, статические коллекции, таймеры, незакрытые `GCHandle`.
- **Время жизни зависит от JIT:** в оптимизированном коде объект собираем сразу после последнего использования, в Debug — живёт до конца метода.
- **GC настраивается:** Workstation или Server, фоновая сборка, режимы задержки, лимиты кучи в контейнере.

## Минимальный пример

```csharp
// using declarations: disposed at the end of the enclosing block, in reverse order.
public static List<string> Declarations()
{
    var log = new List<string>();
    {
        using var a = new Logged("a", log);
        using var b = new Logged("b", log);
        log.Add("body end");
    }
    log.Add("after block");
    return log;
}
```

Результат: `["body end", "dispose b", "dispose a", "after block"]` — ресурсы освобождены в конце блока, в обратном порядке, без участия GC.

## Когда применять, а когда нет

- ✅ `using` — для всего, что реализует `IDisposable`: потоки, подключения, ответы HTTP, `CancellationTokenSource`.
- ✅ Свой `IDisposable` — если класс владеет освобождаемым объектом или неуправляемым ресурсом.
- ✅ `SafeHandle` — для дескрипторов ОС и нативных библиотек.
- ✅ Измерять выделения памяти (`GC.GetAllocatedBytesForCurrentThread`, `dotnet-counters`) перед оптимизацией.
- ❌ `GC.Collect()` в рабочем коде.
- ❌ Свой финализатор «на всякий случай».
- ❌ Обнулять локальные переменные «для GC».

## Уровни материала

| Уровень | Что внутри |
|---|---|
| [1. Базовая теория](basics.ru.md) | стек и управляемая куча; корни и достижимость; когда запускается сборка; поколения и LOH; `IDisposable`, `using`, `using var` |
| [2. Продвинутая теория](advanced.ru.md) | реализация `IDisposable` и шаблон `Dispose(bool)`; `IAsyncDisposable`; финализаторы; `SafeHandle`; утечки памяти; слабые ссылки и `ConditionalWeakTable`; измерение выделений; Workstation и Server GC |
| [3. Экспертная теория](expert.ru.md) | как проходит сборка, барьер записи и регионы; время жизни объектов и JIT; финализация и воскрешение; LOH и POH; закрепление; режимы GC и задержки; настройки; справочник, подвохи, 18 вопросов с собеседований |
| [4. Самопроверка](quiz.ru.md) | квиз и [упражнения](exercises.ru.md) с решениями |
| [5. Запускаемые примеры](code/) | `code/Examples/` и `code/Solutions/` — решение `Code.slnx` |
| [6. Тесты](code/) | `code/Tests/` — `dotnet test` в папке `code/` |

## Связанные материалы

- Пререквизиты: [02 · Типы и переменные](../02-types-and-variables/overview.ru.md) — значимые и ссылочные типы, упаковка
- Runtime и его настройки: [01 · Платформа .NET и dotnet CLI](../01-dotnet-platform/overview.ru.md)
- `Dispose` на практике: [15 · EF Core](../15-ef-core/overview.ru.md) — время жизни `DbContext`
- Дальше: [12a · Span и Memory](../12a-span-and-memory/overview.ru.md); модуль 19 «Производительность» (ещё не создан)
- `IAsyncDisposable` и выделения конечным автоматом: [10 · Async/await](../10-async-await/overview.ru.md)
- То же в других треках: [Python](../../../python/overview.ru.md) — подсчёт ссылок и сборщик циклов CPython, `with`
