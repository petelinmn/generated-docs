---
title: "BenchmarkDotNet"
lang: ru
track: csharp
kind: library
doc: overview
slug: benchmarkdotnet
part: 2
levels: [1, 2, 3, 4]
tags: [benchmarkdotnet, benchmarking, performance, memory-diagnoser]
versions: ".NET 11, C# 15; BenchmarkDotNet 0.16.0-preview.2"
status: draft
updated: 2026-11-22
---

# BenchmarkDotNet

🌐 **Русский** · [English](overview.en.md)

## Коротко

BenchmarkDotNet — стандартная библиотека микробенчмарков для .NET: сама прогревает код, запускает бенчмарки в отдельном процессе, считает статистику и (с `MemoryDiagnoser`) выделения памяти. Сравнивать нужно относительные величины (`Ratio`, `Allocated`), а не абсолютные времена. Запуск — только в Release.

## Ключевые идеи

- `[Benchmark]`, `[Params]`, `[GlobalSetup]`, `[MemoryDiagnoser]`, `Baseline = true`.
- `Ratio` и `Allocated` переносимы между машинами; `Mean` — нет.
- Проверено: конкатенация в цикле при N=1000 ≈ в 65 раз медленнее `StringBuilder` и выделяет ≈1 МБ; `Span` быстрее `Split` ≈ в 6 раз без выделений.
- Только Release; версия библиотеки должна знать версию рантайма.
- Каждый бенчмарк — отдельный процесс и сгенерированный проект.
- Микробенчмарк не заменяет профилирование приложения.

## Минимальный пример

```csharp
BenchmarkRunner.Run<Bench>();

[MemoryDiagnoser]
public class Bench
{
    [Benchmark] public int Sum() { var x = 0; for (var i = 0; i < 1000; i++) x += i; return x; }
}
```

## Когда применять, а когда нет

- ✅ Сравнение двух реализаций горячего метода.
- ✅ Проверка, что оптимизация действительно уменьшает аллокации.
- ✅ Выбор между структурами данных на реалистичных размерах.
- ❌ Код, зависящий от сети/диска/БД.
- ❌ Оптимизация без профилирования.
- ❌ Абсолютные пороги производительности в CI.

## Уровни материала

| Уровень | Что внутри |
|---|---|
| [1. Основы](basics.ru.md) | первый бенчмарк, чтение отчёта, атрибуты |
| [2. Продвинутый](advanced.ru.md) | `GlobalSetup`, параметры, DCE, окружение |
| [3. Эксперт](expert.ru.md) | как устроен запуск, подвохи, альтернативы, 11 вопросов с собеседований |
| [4. Самопроверка](quiz.ru.md) | 10 вопросов и [4 упражнения](exercises.ru.md) с решениями |
| 5. Запускаемые примеры | не сгенерирован — `/learn csharp benchmarkdotnet level 5` |
| 6. Тесты | не сгенерирован — `/learn csharp benchmarkdotnet level 6` |

## Связанные материалы

- Пререквизиты: [19 · Производительность](../../modules/19-performance/overview.ru.md), [12 · Память и GC](../../modules/12-memory-and-gc/overview.ru.md)
- Рядом: [12a · Span и память](../../modules/12a-span-and-memory/overview.ru.md), [Mapperly](../mapperly/overview.ru.md), [Dapper](../dapper/overview.ru.md)
- В других треках: [Python](../../../python/overview.ru.md) — `timeit`, `pytest-benchmark`
