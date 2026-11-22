---
title: "Производительность: базовая теория"
lang: ru
track: csharp
kind: module
doc: basics
slug: 19-performance
part: 3
tags: [performance, allocations, gc-pressure, boxing, stringbuilder, benchmarking]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-22
---

# Производительность: базовая теория

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [обзор](overview.ru.md) · **1. Базовая** · [2. Продвинутая](advanced.ru.md) · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [12 · Память и GC](../12-memory-and-gc/overview.ru.md), [12a · Span и Memory](../12a-span-and-memory/overview.ru.md), [07 · LINQ](../07-linq/overview.ru.md). **Время:** ~1 ч.

## Зачем это нужно

Производительность — это время ответа (latency), пропускная способность (throughput), потребление памяти и нагрузка на сборщик мусора. Большинству приложений достаточно «обычного» кода; но в горячих путях (циклы, обработка запросов, парсинг) лишние выделения памяти и неудачные приёмы заметно замедляют программу. Главное правило: **сначала измеряй, потом оптимизируй**. Угадывание «узкого места» почти всегда ошибается, а оптимизация без измерения усложняет код без выгоды.

## Что измерять

| Метрика | Чем измерять |
|---|---|
| время выполнения | BenchmarkDotNet (микро), профилировщик (в приложении) |
| выделения памяти | `GC.GetAllocatedBytesForCurrentThread()`, `MemoryDiagnoser` |
| нагрузка на GC | число сборок (`GC.CollectionCount`), счётчики `dotnet-counters` |
| задержка и пропускная способность | нагрузочные тесты (k6, bombardier) |

Всегда измеряйте **Release-сборку**: в Debug код не оптимизируется, и цифры бессмысленны (BenchmarkDotNet в Debug отказывается работать — проверено: «Benchmark was built without optimization enabled»).

## Выделения памяти и давление на GC

Каждое выделение в куче — работа для [сборщика мусора](../12-memory-and-gc/overview.ru.md): чем больше мусора, тем чаще сборки. В проверенном цикле из 200 000 выделений массивов по 1 КБ произошло 22 сборки поколения 0. Поэтому в горячем коде важно знать, **что выделяет память**. Измерить это просто:

```csharp
static long Alloc(Action a)
{
    a(); a();                                   // warm up (JIT, caches)
    long before = GC.GetAllocatedBytesForCurrentThread();
    a();
    return GC.GetAllocatedBytesForCurrentThread() - before;
}
```

Проверено на .NET 11 (Release), выделения за один вызов:

| Операция | Байт |
|---|---|
| `object o = n;` (упаковка `int`) | 24 |
| `$"v={n}"` (интерполяция с числом) | 32 |
| `"a" + s` (склейка двух строк) | 32 |
| лямбда, захватывающая локальную переменную | 80 |
| `static`-лямбда (без захвата) | 0 |
| `foreach` по `List<int>` (переменная типа `List<int>`) | 0 |
| `foreach` по тому же списку через `IEnumerable<int>` | 40 |
| `arr.Where(...).Sum()` на массиве из 8 | 48 |
| тот же цикл `foreach` с `if` по массиву | 0 |
| `new int[256]` | 1048 |
| `ArrayPool<int>.Shared.Rent(256)` + `Return` | 0 |
| `stackalloc int[256]` | 0 |
| `Substring` (5 символов) | 32 |
| `AsSpan` срез | 0 |
| вызов `params object[]` с 3 числами | 120 |
| вызов `params ReadOnlySpan<int>` с 3 числами | 0 |

Числа — для 64-разрядного .NET 11; они иллюстрируют порядок величин, а не гарантию.

## Простые приёмы

**Строки.** `s += x` в цикле каждый раз создаёт новую строку: 50 склеек выделили 7968 байт, `StringBuilder` — 1176, `string.Concat(массив)` — 304 (проверено). Для многих кусков — `StringBuilder` или `string.Concat/Join`.

**LINQ в горячем пути.** Цепочка `Where(...).Sum()` выделяет объекты-итераторы и делегат (48 байт на малом массиве); эквивалентный цикл — 0. Вне горячих путей LINQ нормален и читабелен ([07 · LINQ](../07-linq/overview.ru.md)).

**Упаковка.** Приведение значимого типа к `object` (в том числе через `params object[]` или нетипизированные коллекции) выделяет память — используйте обобщения и типизированные методы.

**Коллекции.** Задавайте ёмкость, если размер известен (`new List<int>(1000)`): иначе список растёт удвоением с копированием ([06 · Коллекции и обобщения](../06-collections-and-generics/overview.ru.md)).

**Срезы.** `Substring`, `Split` — копии; `AsSpan`/`ReadOnlySpan<char>` — без выделений ([12a · Span и Memory](../12a-span-and-memory/overview.ru.md)).

**Структуры вместо мелких классов.** Массив из 100 экземпляров небольшого класса — 3224 байта и 100 объектов в куче; массив из 100 структур тех же полей — 824 байта (проверено): меньше объектов, лучше локальность данных. Но большие и изменяемые структуры копируются дорого ([05 · Интерфейсы, структуры, записи](../05-interfaces-structs-records/overview.ru.md)).

## Ошибки новичка

- ❌ Оптимизировать «на глаз» → ✅ измерить, найти узкое место, оптимизировать, измерить снова.
- ❌ Измерять `Stopwatch` одним запуском в Debug → ✅ Release, прогрев, много повторов, BenchmarkDotNet.
- ❌ Убирать LINQ повсюду → ✅ только в горячих путях, подтверждённых замером.
- ❌ Склейка строк в цикле через `+=` → ✅ `StringBuilder`.
- ❌ Игнорировать выделения и GC → ✅ смотреть `MemoryDiagnoser`.
- ❌ Жертвовать читаемостью ради микровыигрыша → ✅ оптимизация должна окупаться на измеренной нагрузке.

## Связанные материалы

- Дальше: [продвинутая теория](advanced.ru.md) — BenchmarkDotNet, пулы, структуры, профилирование.
- [12 · Память и GC](../12-memory-and-gc/overview.ru.md) — поколения и сборки.
- [12a · Span и Memory](../12a-span-and-memory/overview.ru.md) — выделения без копий.
- [07 · LINQ](../07-linq/overview.ru.md) — где LINQ дорог.
- [Python](../../../python/overview.ru.md) — профилирование и оптимизация в Python.

## Источники

- [Рекомендации по производительности .NET](https://learn.microsoft.com/dotnet/core/performance/)
- [`GC.GetAllocatedBytesForCurrentThread`](https://learn.microsoft.com/dotnet/api/system.gc.getallocatedbytesforcurrentthread)
- [Сборка мусора](https://learn.microsoft.com/dotnet/standard/garbage-collection/fundamentals)
- [BenchmarkDotNet](https://benchmarkdotnet.org/)
