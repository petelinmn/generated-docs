---
title: "Память и GC: базовая теория"
lang: ru
track: csharp
kind: module
doc: basics
slug: 12-memory-and-gc
part: 2
tags: [gc, garbage-collection, managed-heap, generations, large-object-heap, idisposable, using]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-21
---

# Память и GC: базовая теория

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [обзор](overview.ru.md) · **1. Базовая** · [2. Продвинутая](advanced.ru.md) · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md) · [упражнения](exercises.ru.md) · [5–6. Код и тесты](code/)

**Пререквизиты:** [02 · Типы и переменные](../02-types-and-variables/overview.ru.md) — значимые и ссылочные типы, упаковка.

## Зачем это нужно

В C# память под объекты выделяет `new`, а освобождает **сборщик мусора** (garbage collector, GC) — часть runtime (см. [01 · Платформа .NET](../01-dotnet-platform/basics.ru.md)). Не нужно вызывать `free` или `delete`, поэтому нет висячих указателей и двойного освобождения. Но у автоматической памяти есть цена и границы:

- сборка останавливает или замедляет программу, а частые выделения памяти заставляют GC работать чаще;
- объект живёт, пока на него есть ссылка, — забытая ссылка становится утечкой памяти;
- GC управляет только памятью. Файлы, сокеты, подключения к базе он не закрывает вовремя — для них есть `IDisposable` и `using`.

## Стек и управляемая куча

- **Управляемая куча** (managed heap) — память, которой распоряжается GC. Там лежат объекты ссылочных типов: экземпляры классов, массивы, строки, делегаты, упакованные значения.
- **Стек** потока хранит локальные переменные и параметры методов. Он освобождается сам при выходе из метода, GC им не управляет.

Значение значимого типа хранится там, где объявлена переменная: локальная `int` — в стеке, поле `int` внутри объекта — в куче. Подробнее — в [02 · Типы и переменные](../02-types-and-variables/expert.ru.md), а о том, как упаковка создаёт объекты в куче, — в [продвинутой теории модуля 02](../02-types-and-variables/advanced.ru.md).

## Как GC решает, что объект больше не нужен

GC не считает ссылки. Он начинает с **корней** (roots) и обходит все объекты, достижимые по ссылкам. Корни — это статические поля, локальные переменные в стеке потоков, регистры процессора, GC-дескрипторы (GC handles) и очередь финализации. Всё, до чего обход не дошёл, — мусор.

```csharp
// The object is referenced only from holder.Item: no local variable of this method points to it,
// so the result does not depend on how long the JIT keeps locals alive (Debug vs Release).
public static (bool WhileReferenced, bool AfterRelease) Demo()
{
    var holder = new Holder();
    WeakReference weak = PutNewObject(holder);

    GC.Collect();
    bool whileReferenced = weak.IsAlive;   // reachable: local holder → holder.Item

    holder.Item = null;                    // drop the only strong reference
    GC.Collect();
    return (whileReferenced, weak.IsAlive);
}
```

`WeakReference` («слабая ссылка») позволяет наблюдать за объектом, не удерживая его. Результат: `(True, False)` — пока объект достижим через `holder.Item`, GC его не трогает; после обнуления ссылки он собран.

Поскольку GC ищет достижимые объекты, а не считает ссылки, **циклы ему не мешают**: два объекта, ссылающиеся друг на друга, но недостижимые из корней, собираются (`CycleIsCollected()` → `True`). Код: [Ex01_Reachability.cs](code/Examples/Ex01_Reachability.cs).

> `GC.Collect()` в примерах нужен, чтобы показать поведение. В обычном коде его не вызывают: GC сам решает, когда собирать мусор.

## Когда происходит сборка

Сборка мусора запускается, когда:

- память, занятая объектами в управляемой куче, превышает порог, который GC подбирает сам;
- в системе мало физической памяти;
- вызван `GC.Collect()`.

Чаще всего сборку вызывает первое: программа выделяет память, и в какой-то момент очередной `new` запускает сборку.

## Поколения

Большинство объектов живут очень недолго: временная строка, итератор, DTO запроса. Поэтому куча делится на **поколения** (generations), и молодые объекты проверяются чаще старых:

| Поколение | Что там | Как часто собирается |
|---|---|---|
| 0 | новые объекты | чаще всего, быстро |
| 1 | пережили одну сборку — буфер между молодыми и старыми | реже |
| 2 | долгоживущие: кеши, синглтоны, статические данные | редко; полная сборка проверяет все поколения |
| LOH | большие объекты, от 85 000 байт | вместе с поколением 2 |

Объект, переживший сборку, **продвигается** (promotion) в следующее поколение:

```csharp
// A new object starts in gen 0; every collection it survives promotes it, up to gen 2.
public static List<int> Promotion()
{
    var survivor = new object();
    var generations = new List<int> { GC.GetGeneration(survivor) };
    for (int i = 0; i < 3; i++)
    {
        GC.Collect();
        generations.Add(GC.GetGeneration(survivor));
    }
    return generations;
}
```

Результат: `[0, 1, 2, 2]` — выше второго поколения нет. Сборка поколения 0 обходит только молодые объекты, поэтому она быстрая. Массив `byte[1_000]` создаётся в поколении 0, а `byte[84_976]` (вместе с заголовком массива — ровно 85 000 байт) сразу попадает в **кучу больших объектов** (large object heap, LOH), и `GC.GetGeneration` возвращает для него `2`. Код: [Ex02_Generations.cs](code/Examples/Ex02_Generations.cs).

## IDisposable и using

GC освобождает память, но не знает, что объект держит файл, сокет или подключение к базе. Такой ресурс нужно освободить **детерминированно** — сразу, как он больше не нужен. Для этого класс реализует `IDisposable` с единственным методом `Dispose()`, а код вызывает его через `using`:

```csharp
// using statement: Dispose runs when the block ends.
public static List<string> Statement()
{
    var log = new List<string>();
    using (var file = new Logged("file", log))
    {
        log.Add("body");
    }
    log.Add("after");
    return log;
}
```

Результат: `["body", "dispose file", "after"]`. Компилятор превращает `using` в `try/finally`, поэтому `Dispose` вызывается и при исключении: пример `WithException` дал `["dispose resource", "caught"]` — ресурс освобождён до того, как исключение поймано.

**Объявление `using`** (using declaration) обходится без блока: ресурс освобождается в конце охватывающего блока, а несколько ресурсов — в обратном порядке:

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

Результат: `["body end", "dispose b", "dispose a", "after block"]`. Обратный порядок логичен: `b` создан позже и может зависеть от `a`.

Для ресурсов, которые освобождаются асинхронно (например, сбрасывают буфер в сеть), есть `IAsyncDisposable` и `await using` — они разобраны в [продвинутой теории](advanced.ru.md). Код: [Ex03_Using.cs](code/Examples/Ex03_Using.cs).

Что реализует `IDisposable` в стандартной библиотеке: `FileStream`, `StreamReader`/`StreamWriter`, `HttpResponseMessage`, `SqlConnection` и другие подключения к базам, `DbContext` из [EF Core](../15-ef-core/advanced.ru.md#жизненный-цикл-dbcontext), `CancellationTokenSource`, `Timer`, `SemaphoreSlim`.

## Ошибки новичка

- ❌ Вызывать `GC.Collect()`, чтобы «почистить память» → ✅ GC сам выбирает момент; `GC.Collect()` — полная блокирующая сборка всех поколений, обычно это лишняя пауза.
- ❌ Думать, что `Dispose` освобождает память объекта → ✅ он освобождает ресурс (файл, сокет), память освободит GC.
- ❌ Создавать `FileStream`, `HttpClient`-ответ или подключение без `using` → ✅ `using` или `using var`.
- ❌ Считать, что `obj = null` немедленно освобождает память → ✅ объект станет кандидатом на сборку, а освободится при ближайшей сборке его поколения.
- ❌ Бояться циклических ссылок → ✅ GC собирает недостижимые циклы.
- ❌ Держать в статических коллекциях то, что больше не нужно → ✅ статическое поле — корень: всё, на что оно ссылается, живёт до конца программы.

## Связанные материалы

- Дальше: [продвинутая теория](advanced.ru.md) — шаблон Dispose, финализаторы, `SafeHandle`, утечки через события, слабые ссылки, режимы GC
- Значимые и ссылочные типы, упаковка: [02 · Типы и переменные](../02-types-and-variables/expert.ru.md)
- `Dispose` контекста базы данных: [15 · EF Core](../15-ef-core/advanced.ru.md#жизненный-цикл-dbcontext)
- То же в других треках: [Python](../../../python/overview.ru.md) — подсчёт ссылок и циклический сборщик CPython, `with` вместо `using`

## Источники

- [Основы сборки мусора](https://learn.microsoft.com/dotnet/standard/garbage-collection/fundamentals)
- [Куча больших объектов](https://learn.microsoft.com/dotnet/standard/garbage-collection/large-object-heap)
- [Очистка неуправляемых ресурсов](https://learn.microsoft.com/dotnet/standard/garbage-collection/unmanaged)
- [Оператор using](https://learn.microsoft.com/dotnet/csharp/language-reference/statements/using)
- [IDisposable](https://learn.microsoft.com/dotnet/api/system.idisposable)
