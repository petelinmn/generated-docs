---
title: "BenchmarkDotNet: продвинутый уровень"
lang: ru
track: csharp
kind: library
doc: advanced
slug: benchmarkdotnet
part: 2
tags: [benchmarkdotnet, globalsetup, params, baseline, dead-code-elimination, jobs]
versions: ".NET 11, C# 15; BenchmarkDotNet 0.16.0-preview.2"
status: draft
updated: 2026-11-22
---

# BenchmarkDotNet: продвинутый уровень

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Основы](basics.ru.md) · **2. Продвинутый** · [3. Эксперт](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [основы](basics.ru.md). **Время:** ~45 мин.

## Подготовка и параметры

Данные для замера готовят в `[GlobalSetup]` — он выполняется один раз и **не входит** в измеряемое время:

```csharp
[MemoryDiagnoser]
public class Bench
{
    private string _text = "";
    [GlobalSetup] public void Setup() => _text = string.Join(",", Enumerable.Range(0, 1000));

    [Benchmark(Baseline = true)] public int SplitCount() => _text.Split(',').Length;
    [Benchmark] public int SpanCount() { var n = 1; foreach (var c in _text.AsSpan()) if (c == ',') n++; return n; }
}
```

Реальный вывод (строка из 1000 чисел через запятую):

| Method | Mean | Ratio | Allocated |
|---|---|---|---|
| `SplitCount` | 6 350,5 нс | 1,00 | 39 944 B |
| `SpanCount` | 1 042,0 нс | 0,16 | **0 B** |

Подсчёт запятых по `Span` в ~6 раз быстрее и не выделяет память: `Split` создаёт массив строк. Это типичный выигрыш от оптимизаций из [12a · Span и память](../../modules/12a-span-and-memory/overview.ru.md). `[Params]` перебирает значения параметров (см. [основы](basics.ru.md#первый-бенчмарк)), давая отдельную строку на каждое.

## Возвращайте результат

BenchmarkDotNet «потребляет» возвращаемое значение метода, поэтому JIT не может выбросить вычисление как мёртвый код (dead code elimination, DCE). Проверка: цикл суммирования `x += i` без возврата и с возвратом дал близкие значения (`239,2 нс` и `279,4 нс`) — в этом случае JIT результат не выбросил, но полагаться на это нельзя: возвращайте значение либо используйте `Consumer`.

> ⚠️ Проверить: сценарии, в которых JIT действительно удаляет неиспользованное вычисление, и класс `Consumer`/`Blackhole` в этом материале не запускали.

## Настройка запуска

`[SimpleJob(warmupCount: 2, iterationCount: 5, launchCount: 1)]` сокращает прогон: так проверялись примеры этого материала (36 секунд на 4 бенчмарка). Для достоверных замеров оставляют значения по умолчанию — BenchmarkDotNet сам подбирает число итераций до стабильного результата.

| Параметр | Что делает |
|---|---|
| `warmupCount` | число прогревочных итераций |
| `iterationCount` | число измеряемых итераций |
| `launchCount` | число запусков процесса |
| `[Params]` | матрица параметров |

В выводе в разделе «Hints» BenchmarkDotNet сообщает о выбросах (`Outliers … 1 outlier was removed`) — шум измерений.

## Когда бенчмарк врёт

- **Debug-сборка** — отказ запуска (проверено).
- **Слишком короткий метод** — шум таймера превышает время метода; BenchmarkDotNet увеличивает число операций на итерацию автоматически.
- **Разный код с разной JIT-оптимизацией** — измеряйте на целевом .NET (tiered compilation, PGO).
- **Кэш процессора и данные** — малые наборы лежат в кэше и не показывают реальную стоимость доступа к памяти.
- **Зависимость от внешнего мира** — сеть/диск дают разброс, не относящийся к вашему коду.

## Окружение (из практики этого материала)

- BenchmarkDotNet **0.15.8** не распознал .NET 11 (`NotImplementedException: GetRuntimeVersion … NotRecognized`); 0.16.0-preview.2 запустился.
- Запуск проекта из глубокой временной папки привёл к ошибке сборки дочернего проекта («Unable to find … in C:\Users\… and its subfolders») — держите проект в обычном каталоге.
- В Debug запуск отклоняется; для проверки примеров на скорую руку использовался `DefaultConfig.Instance.WithOptions(ConfigOptions.DisableOptimizationsValidator)` — **не** для реальных замеров.

## Best practices

| Правило | Почему |
|---|---|
| Release и целевая версия .NET | реальные условия |
| `Baseline = true` и `Ratio` | сравнимость между машинами |
| `MemoryDiagnoser` всегда | выделения — самое стабильное число |
| Подготовка в `[GlobalSetup]` | замеряем только нужное |
| Возвращать результат | защита от DCE |
| Не доверять одному запуску | смотреть `StdDev`, выбросы |

## Связанные материалы

- Назад: [основы](basics.ru.md). Дальше: [эксперт](expert.ru.md).
- [19 · Производительность](../../modules/19-performance/advanced.ru.md) — стратегия и пулы.
- [12a · Span и память](../../modules/12a-span-and-memory/overview.ru.md)
- [Mapperly](../mapperly/overview.ru.md), [Dapper](../dapper/overview.ru.md) — что имеет смысл сравнивать бенчмарками.

## Источники

- [BenchmarkDotNet: Setup and cleanup](https://benchmarkdotnet.org/articles/features/setup-and-cleanup.html)
- [BenchmarkDotNet: Baseline](https://benchmarkdotnet.org/articles/features/baselines.html)
- [BenchmarkDotNet: Good practices](https://benchmarkdotnet.org/articles/guides/good-practices.html)
