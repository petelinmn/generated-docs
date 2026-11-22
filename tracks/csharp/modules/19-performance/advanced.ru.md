---
title: "Производительность: продвинутая теория"
lang: ru
track: csharp
kind: module
doc: advanced
slug: 19-performance
part: 3
tags: [performance, benchmarkdotnet, array-pool, stackalloc, struct, profiling, memory-diagnoser]
versions: ".NET 11, C# 15; BenchmarkDotNet 0.16.0-preview.2"
status: draft
updated: 2026-11-22
---

# Производительность: продвинутая теория

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · **2. Продвинутая** · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [базовая теория](basics.ru.md). **Время:** ~1,5 ч.

## BenchmarkDotNet

Микробенчмарки измеряют не `Stopwatch`, а BenchmarkDotNet: он сам прогревает код, делает много итераций, оценивает разброс и умеет считать выделения. Пакет `BenchmarkDotNet`; проект — консольный, запуск в **Release**: `dotnet run -c Release`.

```csharp
[MemoryDiagnoser]
public class Strings
{
    private readonly string[] _parts = Enumerable.Range(0, 100).Select(i => "item" + i).ToArray();

    [Benchmark(Baseline = true)]
    public string Concat() { var s = ""; foreach (var p in _parts) s += p; return s; }

    [Benchmark]
    public string StringBuilder_() { var sb = new StringBuilder(); foreach (var p in _parts) sb.Append(p); return sb.ToString(); }

    [Benchmark]
    public string Join() => string.Concat(_parts);
}

BenchmarkRunner.Run<Strings>();
```

Реальный запуск (Intel Core i5-12400, Windows 11, .NET 11.0.0, быстрый режим `ShortRun`, поэтому погрешность велика):

```text
| Method         | Mean       | Ratio | Gen0   | Allocated | Alloc Ratio |
| Concat         | 3,104.0 ns |  1.00 | 6.5079 |  59.81 KB |        1.00 |
| StringBuilder_ |   403.0 ns |  0.13 | 0.3996 |   3.67 KB |        0.06 |
| Join           |   275.9 ns |  0.09 | 0.1283 |   1.18 KB |        0.02 |
```

Читайте столбцы: `Mean` — среднее время вызова, `Ratio` — отношение к базовому (`Baseline = true`), `Allocated` — память на вызов (самое надёжное число: оно детерминировано), `Gen0` — число сборок поколения 0 на 1000 операций. Время зависит от процессора и версии, поэтому сравнивайте **отношения**.

| Атрибут | Назначение |
|---|---|
| `[Benchmark]`, `[Benchmark(Baseline = true)]` | метод-замер, база сравнения |
| `[MemoryDiagnoser]` | выделения и сборки |
| `[Params(10, 1000)]` | один бенчмарк на нескольких размерах входа |
| `[GlobalSetup]` / `[IterationSetup]` | подготовка данных вне замера |
| `[SimpleJob(...)]`, `ShortRun` | профиль запуска (точность против времени) |

Результат бенчмарка нужно **использовать** (вернуть из метода): иначе компилятор может выбросить «неиспользуемый» вычисленный код. Подготовку данных выносят в `GlobalSetup`.

Нюансы окружения (проверено): в Debug BenchmarkDotNet отказывается работать («Benchmark was built without optimization enabled»); версия 0.15.8 не распознала .NET 11 (`NotImplementedException: GetRuntimeVersion not implemented for NotRecognized`), а 0.16.0-preview.2 запустилась; при запуске проекта из глубокой временной папки дочерняя сборка не нашла `.csproj` («Unable to find … in C:\Users\… and its subfolders») — держите проект в обычном каталоге.

## Пулы и стековая память

Для временных буферов:

| Подход | Время | Выделено |
|---|---|---|
| `new byte[4096]` | 131.6 нс | 4120 Б |
| `ArrayPool<byte>.Shared.Rent(4096)` + `Return` | 7.6 нс | 0 |
| `stackalloc byte[4096]` | 20.9 нс | 0 |

(Те же условия `ShortRun`; относительный порядок надёжнее абсолютных чисел.) Пул возвращает массив, **не обнулённый** и, возможно, большей длины; `stackalloc` годится для небольших размеров (4 КБ допустимо, мегабайты — нет). Подробности — в [12a · Span и Memory](../12a-span-and-memory/advanced.ru.md#arraypool-переиспользование-массивов).

## LINQ против цикла

На 1000 элементов (`ShortRun`):

| Метод | Время | Выделено |
|---|---|---|
| `Where(x => x % 2 == 0).Sum()` | 733.6 нс | 48 Б |
| цикл `foreach` с условием | 254.2 нс | 0 |
| цикл по `Span` с побитовым `&` | 254.4 нс | 0 |

Цикл быстрее в ~3 раза и не выделяет, но выигрыш абсолютный мал: оправдан в горячем пути, который выполняется миллионы раз. Цикл по `Span` не дал выигрыша против цикла по массиву: JIT и так убирает лишние проверки границ ([12a · Span и Memory](../12a-span-and-memory/expert.ru.md#как-jit-работает-со-span)).

## Структуры и данные

- Массив структур хранит данные подряд — хорошая локальность кэша; массив ссылок на классы — указатели на разбросанные объекты (проверено: 824 против 3224 байт для 100 элементов).
- Структуры должны быть небольшими (около 16 байт) и неизменяемыми (`readonly struct`); передача `in` избегает копирования, но неосторожно создаёт защитные копии ([05 · Интерфейсы, структуры, записи](../05-interfaces-structs-records/advanced.ru.md#изменяемые-структуры)).
- `sealed` у классов позволяет JIT девиртуализировать вызовы.
- `static`-лямбды и кэшируемые делегаты не выделяют память ([09 · Делегаты, события, лямбды](../09-delegates-events-lambdas/expert.ru.md#лямбда-без-захвата-и-кеш)).

## Асинхронность и производительность

Каждый `async`-метод — конечный автомат; при реальной приостановке выделяется объект. На синхронно завершающихся путях помогает `ValueTask` ([10 · Async/await](../10-async-await/advanced.ru.md#valuetask)). Главное для пропускной способности сервера — не блокировать потоки: блокирующий код вызывает исчерпание пула ([10 · Async/await](../10-async-await/expert.ru.md#пул-потоков-и-исчерпание)).

## Профилирование

Микробенчмарки отвечают на вопрос «какой из двух вариантов быстрее», а **профилировщик** — «где в моём приложении тратится время». Инструменты .NET: `dotnet-counters` (метрики в реальном времени: аллокации, GC, пул потоков), `dotnet-trace` (трассировка и CPU-профиль), `dotnet-dump` (анализ дампов), PerfView, профилировщики Visual Studio и Rider.

> ⚠️ Проверить: команды и вывод `dotnet-counters`/`dotnet-trace` в этом материале не запускались (инструменты не устанавливались). Типичный порядок: `dotnet tool install -g dotnet-counters`, затем `dotnet-counters monitor --process-id <PID>`; трассу снимают `dotnet-trace collect --process-id <PID>` и открывают в PerfView/Speedscope. Сверьте параметры с документацией перед использованием.

## Лучшие практики

| Правило | Почему |
|---|---|
| Профиль → гипотеза → правка → замер | иначе оптимизируете не то |
| Релизная сборка, прогрев, много повторов | достоверные числа |
| Смотрите на `Allocated`, а не только на время | выделения влияют на всё приложение через GC |
| Оптимизируйте горячие пути | остальное оставьте читаемым |
| Отношения важнее абсолютных чисел | цифры зависят от машины |
| Фиксируйте регрессии бенчмарками | производительность ломается незаметно |

## Связанные материалы

- Назад: [базовая теория](basics.ru.md). Дальше: [экспертная теория](expert.ru.md) — JIT, Native AOT, SIMD.
- [12a · Span и Memory](../12a-span-and-memory/overview.ru.md) — буферы без копий.
- [10 · Async/await](../10-async-await/overview.ru.md) — async и пул потоков.
- [06 · Коллекции и обобщения](../06-collections-and-generics/overview.ru.md) — ёмкость и сложность операций.
- [22 · Наблюдаемость](../22-observability/overview.ru.md) — метрики и трассировка в продакшене.

## Источники

- [BenchmarkDotNet: документация](https://benchmarkdotnet.org/articles/overview.html)
- [`ArrayPool<T>`](https://learn.microsoft.com/dotnet/api/system.buffers.arraypool-1)
- [Инструменты диагностики .NET](https://learn.microsoft.com/dotnet/core/diagnostics/)
- [dotnet-counters](https://learn.microsoft.com/dotnet/core/diagnostics/dotnet-counters)
