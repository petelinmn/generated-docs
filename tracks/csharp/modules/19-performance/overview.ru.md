---
title: "Производительность"
lang: ru
track: csharp
kind: module
doc: overview
slug: 19-performance
part: 3
levels: [1, 2, 3, 4]
tags: [performance, allocations, benchmarkdotnet, array-pool, native-aot, gc-pressure, simd]
versions: ".NET 11, C# 15; BenchmarkDotNet 0.16.0-preview.2"
status: draft
updated: 2026-11-22
---

# Производительность

🌐 **Русский** · [English](overview.en.md)

## Коротко

Производительность .NET-кода — это время, пропускная способность, память и нагрузка на сборщик мусора. Главное правило: сначала измерять, потом оптимизировать. Микробенчмарки делает BenchmarkDotNet (в Release, с `MemoryDiagnoser`), узкие места в приложении находит профилировщик. Чаще всего выигрыш даёт сокращение выделений памяти: `StringBuilder`/`Concat`, отказ от упаковки, `Span`, `ArrayPool`, структуры вместо мелких классов, цикл вместо LINQ в горячем пути; для старта и размера — ReadyToRun и Native AOT.

## Ключевые идеи

- **Измеряй:** Release-сборка, прогрев, много повторов; сравнивай отношения и выделения, а не абсолютные числа.
- **Выделения важнее микросекунд:** каждое выделение — давление на GC; проверка — `GC.GetAllocatedBytesForCurrentThread` и `MemoryDiagnoser`.
- **Типичные расходы:** упаковка (24 Б), лямбда с захватом (80 Б), `params object[]` (120 Б на три числа), интерфейсный `foreach` по `List<T>` (40 Б), `s += x` в цикле.
- **Нулевые выделения:** `static`-лямбда, `Span`/`AsSpan`, `stackalloc`, `ArrayPool`, `params ReadOnlySpan<T>`.
- **BenchmarkDotNet:** `[Benchmark]`, `Baseline`, `[MemoryDiagnoser]`, `[Params]`; результат использовать; в Debug отказывается работать.
- **JIT:** ступенчатая компиляция и динамический PGO; **Native AOT** даёт быстрый старт и малый размер ценой динамического кода.
- **SIMD и GC-режимы:** `Vector<T>`, векторизованные методы BCL; Workstation/Server GC.

## Минимальный пример

```csharp
static long Alloc(Action a)
{
    a(); a();                                   // warm up
    long before = GC.GetAllocatedBytesForCurrentThread();
    a();
    return GC.GetAllocatedBytesForCurrentThread() - before;
}

string[] parts = Enumerable.Range(0, 50).Select(i => "p" + i).ToArray();
Console.WriteLine(Alloc(() => { var s = ""; foreach (var p in parts) s += p; }));   // 7968
Console.WriteLine(Alloc(() => { _ = string.Concat(parts); }));                      // 304
```

## Когда применять, а когда нет

- ✅ Горячие пути: цикл по миллионам элементов, парсинг, обработка запроса.
- ✅ Сервисы с высокой нагрузкой, где GC-паузы заметны.
- ✅ Консольные утилиты и serverless — быстрый старт (Native AOT).
- ❌ Оптимизация без замера и профиля.
- ❌ Ухудшение читаемости ради микровыигрыша вне горячих путей.
- ❌ Выводы по бенчмарку в Debug или с одним запуском.

## Уровни материала

| Уровень | Что внутри |
|---|---|
| [1. Базовая теория](basics.ru.md) | что и чем измерять, выделения и давление на GC, таблица стоимости, простые приёмы |
| [2. Продвинутая теория](advanced.ru.md) | BenchmarkDotNet, пулы и стековая память, LINQ против цикла, структуры, async, профилирование |
| [3. Экспертная теория](expert.ru.md) | JIT и ступени, Native AOT, SIMD, режимы GC, ловушки микробенчмарков, подвохи, 12 вопросов с собеседований |
| [4. Самопроверка](quiz.ru.md) | 11 вопросов и [5 упражнений](exercises.ru.md) с решениями |
| 5. Запускаемые примеры | ещё не создан — `/learn csharp 19-performance level 5` |
| 6. Тесты | ещё не создан — `/learn csharp 19-performance level 6` |

## Связанные материалы

- Пререквизиты: [12 · Память и GC](../12-memory-and-gc/overview.ru.md), [12a · Span и Memory](../12a-span-and-memory/overview.ru.md)
- Рядом: [10 · Async/await](../10-async-await/overview.ru.md), [07 · LINQ](../07-linq/overview.ru.md)
- Дальше: [22 · Наблюдаемость](../22-observability/overview.ru.md)
- Регрессии производительности: [16 · Unit-тесты](../16-testing-unit/overview.ru.md)
- То же в других треках: [Python](../../../python/overview.ru.md) — профилирование
- Рецепты и библиотеки: [BenchmarkDotNet](../../libraries/benchmarkdotnet/overview.ru.md), [Dapper](../../libraries/dapper/overview.ru.md), [Mapperly](../../libraries/mapperly/overview.ru.md), [Serilog](../../libraries/serilog/overview.ru.md), [Spectre.Console](../../libraries/spectre-console/overview.ru.md), [Кэширование](../../recipes/caching/overview.ru.md), [Загрузка файлов](../../recipes/file-upload/overview.ru.md), [Пагинация и фильтрация](../../recipes/pagination-filtering/overview.ru.md)
