---
title: "BenchmarkDotNet: основы"
lang: ru
track: csharp
kind: library
doc: basics
slug: benchmarkdotnet
part: 2
tags: [benchmarkdotnet, benchmarking, performance, memory-diagnoser, microbenchmarks]
versions: ".NET 11, C# 15; BenchmarkDotNet 0.16.0-preview.2"
status: draft
updated: 2026-11-22
---

# BenchmarkDotNet: основы

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [обзор](overview.ru.md) · **1. Основы** · [2. Продвинутый](advanced.ru.md) · [3. Эксперт](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [19 · Производительность](../../modules/19-performance/overview.ru.md), [12 · Память и GC](../../modules/12-memory-and-gc/overview.ru.md). **Время:** ~30 мин.

## Что это и зачем

BenchmarkDotNet — стандартный инструмент **микробенчмарков** для .NET. Простой замер `Stopwatch` ненадёжен: нужен прогрев JIT, много повторов, учёт сборки мусора и разброса. BenchmarkDotNet запускает бенчмарк в отдельном процессе, прогревает, повторяет, считает статистику и (при `MemoryDiagnoser`) выделения памяти.

```bash
dotnet add package BenchmarkDotNet
dotnet run -c Release          # always Release
```

## Первый бенчмарк

```csharp
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

BenchmarkRunner.Run<Bench>();

[MemoryDiagnoser]
public class Bench
{
    [Params(100, 1000)] public int N;

    [Benchmark(Baseline = true)]
    public string Concat() { var s = ""; for (var i = 0; i < N; i++) s += "x"; return s; }

    [Benchmark]
    public string Builder() { var sb = new StringBuilder(); for (var i = 0; i < N; i++) sb.Append('x'); return sb.ToString(); }
}
```

Реальный вывод (процессор x64, .NET 11, укороченный `SimpleJob(warmupCount: 2, iterationCount: 5)`):

```text
| Method  | N    | Mean        | Error       | StdDev      | Ratio | Gen0     | Gen1   | Allocated | Alloc Ratio |
|-------- |----- |------------:|------------:|------------:|------:|---------:|-------:|----------:|------------:|
| Concat  | 100  |    904.1 ns |    17.56 ns |     2.72 ns |  1.00 |   1.3361 |      - |   12576 B |        1.00 |
| Builder | 100  |    178.1 ns |     3.37 ns |     0.88 ns |  0.20 |   0.0815 |      - |     768 B |        0.06 |
| Concat  | 1000 | 48,979.0 ns | 4,950.35 ns | 1,285.59 ns |  1.00 | 109.0088 | 0.5493 | 1025976 B |       1.000 |
| Builder | 1000 |    752.2 ns |    39.30 ns |     6.08 ns |  0.02 |   0.4864 | 0.0038 |    4576 B |       0.004 |
```

## Как читать таблицу

| Столбец | Смысл |
|---|---|
| `Mean` | среднее время одного вызова |
| `Error`, `StdDev` | половина 99,9% доверительного интервала; стандартное отклонение |
| `Ratio` | отношение к базовому (`Baseline = true`) |
| `Gen0`/`Gen1` | число сборок мусора на 1000 операций |
| `Allocated` | память, выделенная за один вызов (самое детерминированное число) |

Вывод из примера: при `N = 1000` конкатенация строк в цикле в **~65 раз медленнее** `StringBuilder` и выделяет ~1 МБ против ~4,5 КБ. **Абсолютные времена зависят от машины** — сравнивайте отношения и выделения.

## Атрибуты

| Атрибут | Назначение |
|---|---|
| `[Benchmark]` | метод-бенчмарк; `Baseline = true` — база сравнения |
| `[Params(...)]` | параметризация поля/свойства |
| `[MemoryDiagnoser]` | столбцы `Gen0/Gen1/Allocated` |
| `[SimpleJob(...)]` | параметры прогрева/итераций/запусков |
| `[GlobalSetup]` | подготовка данных один раз (вне замера) |

## Частые ошибки новичка

- ❌ Запускать в Debug → ✅ только `-c Release` (в Debug BenchmarkDotNet отказывается работать: «Benchmark was built without optimization enabled»).
- ❌ Сравнивать абсолютные числа с другой машины → ✅ сравнивайте `Ratio` и `Allocated`.
- ❌ Готовить данные внутри `[Benchmark]` → ✅ `[GlobalSetup]` ([продвинутый](advanced.ru.md#подготовка-и-параметры)).
- ❌ Не возвращать результат → ✅ возвращайте значение, чтобы JIT не вырезал вычисление.
- ❌ Измерять то, что зависит от сети/диска → ✅ микробенчмарк — для чистой CPU-логики.

## Связанные материалы

- Дальше: [продвинутый](advanced.ru.md) — подготовка, параметры, сравнение реализаций, ловушки.
- [19 · Производительность](../../modules/19-performance/overview.ru.md) — стратегия оптимизации.
- [12 · Память и GC](../../modules/12-memory-and-gc/overview.ru.md) — аллокации и поколения.
- [12a · Span и память](../../modules/12a-span-and-memory/overview.ru.md) — типичные оптимизации.
- [Python](../../../python/overview.ru.md) — `timeit` и `pytest-benchmark`.

## Источники

- [BenchmarkDotNet: Getting started](https://benchmarkdotnet.org/articles/guides/getting-started.html)
- [BenchmarkDotNet: Good practices](https://benchmarkdotnet.org/articles/guides/good-practices.html)
- [BenchmarkDotNet: Memory diagnoser](https://benchmarkdotnet.org/articles/configs/diagnosers.html)
