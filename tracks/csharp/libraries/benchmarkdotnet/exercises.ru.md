---
title: "Упражнения: BenchmarkDotNet"
lang: ru
track: csharp
kind: library
doc: exercises
slug: benchmarkdotnet
part: 2
tags: [benchmarkdotnet, memory-diagnoser, globalsetup, params, baseline]
status: draft
updated: 2026-11-22
---

# Упражнения: BenchmarkDotNet

🌐 **Русский** · [English](exercises.en.md)

> Решайте сами. Решения проверены запуском (BenchmarkDotNet 0.16.0-preview.2, .NET 11, укороченный `SimpleJob`; проект лежал в обычном каталоге, не во временной папке). Запускаемых версий в `code/` нет (уровня 5 нет). Абсолютные времена зависят от машины.

## 1. 🟢 Первый бенчмарк

**Задача:** сравните конкатенацию строки в цикле и `StringBuilder` при `N = 100` и `N = 1000` с `MemoryDiagnoser`.

**Ожидаемый результат:** `StringBuilder` быстрее (≈5× при 100, ≈65× при 1000) и выделяет на порядки меньше.

<details><summary>Подсказка</summary>

`[MemoryDiagnoser]`, `[Params(100, 1000)]`, `[Benchmark(Baseline = true)]` для конкатенации; `dotnet run -c Release`.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 1: string concatenation vs StringBuilder.
[MemoryDiagnoser]
[SimpleJob(warmupCount: 2, iterationCount: 5, launchCount: 1)]
public class Bench
{
    [Params(100, 1000)] public int N;
    [Benchmark(Baseline = true)] public string Concat() { var s = ""; for (var i = 0; i < N; i++) s += "x"; return s; }
    [Benchmark] public string Builder() { var sb = new StringBuilder(); for (var i = 0; i < N; i++) sb.Append('x'); return sb.ToString(); }
}
```

Проверено: `N=100` — 904,1 нс / 12 576 B против 178,1 нс / 768 B (`Ratio 0,20`); `N=1000` — 48 979 нс / 1 025 976 B против 752,2 нс / 4 576 B (`Ratio 0,02`).

</details>

## 2. 🟡 Подготовка данных

**Задача:** сравните `Split(',').Length` и подсчёт запятых через `Span` для строки из 1000 чисел. Подготовьте строку один раз.

**Ожидаемый результат:** `Span` ≈ в 6 раз быстрее и без выделений.

<details><summary>Подсказка</summary>

`[GlobalSetup]` создаёт строку; в `SpanCount` — цикл по `_text.AsSpan()`.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 2: setup outside the measurement; Span vs Split.
private string _text = "";
[GlobalSetup] public void Setup() => _text = string.Join(",", Enumerable.Range(0, 1000));

[Benchmark(Baseline = true)] public int SplitCount() => _text.Split(',').Length;
[Benchmark] public int SpanCount() { var n = 1; foreach (var c in _text.AsSpan()) if (c == ',') n++; return n; }
```

Проверено: `SplitCount` — 6350,5 нс / 39 944 B; `SpanCount` — 1042,0 нс / 0 B (`Ratio 0,16`).

</details>

## 3. 🟡 Чтение отчёта

**Задача:** по таблице из упражнения 1 ответьте: какой столбец самый надёжный для сравнения между машинами? Что говорит `Error` при `N=1000` у `Concat`?

**Ожидаемый результат:** `Allocated` (детерминирован) и `Ratio`; `Error 4950 нс` при `Mean 48 979 нс` — широкий интервал из-за малого числа итераций.

<details><summary>Подсказка</summary>

`Error` — половина 99,9% доверительного интервала; в проверке `iterationCount = 5`.

</details>

<details><summary>Решение</summary>

Время зависит от машины и нагрузки, поэтому для переноса между машинами сравнивают **`Ratio`** и **`Allocated`**. Широкий `Error` (≈10% от `Mean`) означает недостаточное число итераций: для достоверных замеров не сокращайте `SimpleJob`.

Проверено по выводу отчёта: `Concat, N=1000: Mean 48 979,0 нс, Error 4950,35 нс, StdDev 1285,59 нс`; отчёт также пометил выбросы (`1 outlier was removed`).

</details>

## 4. 🔴 Ловушки окружения

**Задача:** что произойдёт при запуске в Debug? Какую версию BenchmarkDotNet брать для .NET 11 и почему важно расположение проекта?

**Ожидаемый результат:** Debug — отказ «Benchmark was built without optimization enabled»; 0.15.8 не распознал .NET 11, 0.16.0-preview.2 работает; из глубокой временной папки дочерняя сборка не нашла `.csproj`.

<details><summary>Подсказка</summary>

Вспомните, что BenchmarkDotNet генерирует и собирает отдельный проект для бенчмарка.

</details>

<details><summary>Решение</summary>

- Запускайте `dotnet run -c Release`. В Debug — отказ с сообщением выше (проверено).
- Версия **0.15.8** на .NET 11 дала `NotImplementedException: GetRuntimeVersion … NotRecognized`; **0.16.0-preview.2** запустилась.
- Дочерняя сборка ищет `.csproj` вверх/вниз от каталога; в глубокой временной папке — «Unable to find … and its subfolders». Держите проект в обычном каталоге.

Проверено в этой среде: все три ситуации воспроизводились.

</details>
