---
title: "Упражнения: Производительность"
lang: ru
track: csharp
kind: module
doc: exercises
slug: 19-performance
part: 3
tags: [performance, allocations, span, array-pool, linq, boxing, gc]
status: draft
updated: 2026-11-22
---

# Упражнения: Производительность

🌐 **Русский** · [English](exercises.en.md)

> Решай сам, прежде чем открывать подсказки. Все упражнения измеряют выделения помощником `Alloc` из [базовой теории](basics.ru.md#выделения-памяти-и-давление-на-gc); запускай в **Release** (`dotnet run -c Release`). Числа проверены на .NET 11. Запускаемых версий в `code/` пока нет (уровень 5 не создан).

## 1. 🟢 Измерь и сравни склейку строк

**Задача:** склей 50 строк тремя способами (`+=`, `StringBuilder`, `string.Concat`) и выведи выделенные байты каждого.

**Ожидаемый результат (порядок величин):**

```text
string +=        7968 B
StringBuilder    1176 B
string.Concat     304 B
```

<details><summary>Подсказка</summary>

`Alloc(() => { var s = ""; foreach (var p in parts) s += p; })` и аналогично для остальных; `Alloc` вызывает действие дважды для прогрева.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 1: measure allocations of three ways to join 50 strings.
string[] parts = Enumerable.Range(0, 50).Select(i => "p" + i).ToArray();

Console.WriteLine(Alloc(() => { var s = ""; foreach (var p in parts) s += p; }));
Console.WriteLine(Alloc(() => { var sb = new StringBuilder(); foreach (var p in parts) sb.Append(p); _ = sb.ToString(); }));
Console.WriteLine(Alloc(() => { _ = string.Concat(parts); }));
```

Проверено: `7968`, `1176`, `304`. Склейка в цикле через `+=` создаёт 50 промежуточных строк.

</details>

## 2. 🟢 Убери LINQ из горячего пути

**Задача:** замени `arr.Where(v => v > 2).Sum()` на цикл без выделений и убедись в этом измерением.

**Ожидаемый результат:**

```text
LINQ  → 48 B
цикл  → 0 B
```

<details><summary>Подсказка</summary>

`int s = 0; foreach (var v in arr) if (v > 2) s += v;`.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 2: replace a LINQ chain with a loop in a hot path.
int[] arr = [1, 2, 3, 4, 5, 6, 7, 8];
Console.WriteLine(Alloc(() => { _ = arr.Where(v => v > 2).Sum(); }));                    // 48
Console.WriteLine(Alloc(() => { int s = 0; foreach (var v in arr) if (v > 2) s += v; })); // 0
```

Проверено: `48` и `0`. Не делай этого повсюду: выигрыш окупается только в горячих участках.

</details>

## 3. 🟡 Избавься от упаковки через `params`

**Задача:** метод `static int P(params object[] xs) => xs.Length;` вызывается с тремя числами. Покажи выделение и перепиши на `params ReadOnlySpan<int>`.

**Ожидаемый результат:**

```text
P(1, 2, 3) c object[]           → 120 B
Q(1, 2, 3) c ReadOnlySpan<int>  → 0 B
```

<details><summary>Подсказка</summary>

C# 13 допускает `params` для `ReadOnlySpan<T>`: `static int Q(params ReadOnlySpan<int> xs) => xs.Length;`.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 3: params ReadOnlySpan<T> avoids both the array and the boxing.
static int P(params object[] xs) => xs.Length;
static int Q(params ReadOnlySpan<int> xs) => xs.Length;

Console.WriteLine(Alloc(() => { P(1, 2, 3); }));   // 120
Console.WriteLine(Alloc(() => { Q(1, 2, 3); }));   // 0
```

Проверено: `120` и `0`. Выигрыш двойной: нет массива и нет упаковки трёх чисел.

</details>

## 4. 🟡 Перечислитель без упаковки

**Задача:** измерь `foreach` по `List<int>` через переменную типа `List<int>` и через `IEnumerable<int>`; объясни разницу.

**Ожидаемый результат:**

```text
через List<int>        → 0 B
через IEnumerable<int> → 40 B
```

<details><summary>Подсказка</summary>

`List<T>.GetEnumerator()` возвращает структуру; при приведении к интерфейсу `IEnumerator<T>` она упаковывается.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 4: the struct enumerator is boxed when the list is seen as IEnumerable<T>.
var list = new List<int> { 1, 2, 3 };
Console.WriteLine(Alloc(() => { int s = 0; foreach (var i in list) s += i; }));                       // 0
Console.WriteLine(Alloc(() => { int s = 0; foreach (var i in (IEnumerable<int>)list) s += i; }));     // 40
```

Проверено: `0` и `40`. Если метод должен принимать любую коллекцию, это цена обобщённости; для горячих путей принимайте `List<T>`/`T[]`/`ReadOnlySpan<T>`.

</details>

## 5. 🔴 Бенчмарк буферов

**Задача:** напиши класс BenchmarkDotNet с тремя методами получения буфера 4096 байт: `new byte[4096]` (база), `ArrayPool<byte>.Shared.Rent`/`Return`, `stackalloc`. Включи `[MemoryDiagnoser]`, запусти в Release и сравни.

**Ожидаемый результат (порядок, `ShortRun`, i5-12400):**

```text
NewArray    ~131 нс  4120 B
Pooled      ~7.6 нс     0 B
StackAlloc  ~21 нс      0 B
```

<details><summary>Подсказка</summary>

Метод должен возвращать значение (например, `b.Length`), чтобы JIT не выбросил код. Для .NET 11 нужна версия BenchmarkDotNet 0.16.0-preview.2 или новее. Проект держи в обычном каталоге.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 5: a BenchmarkDotNet comparison of three buffer strategies.
[MemoryDiagnoser]
public class Buffers
{
    [Benchmark(Baseline = true)]
    public int NewArray() { var b = new byte[4096]; b[0] = 1; return b.Length; }

    [Benchmark]
    public int Pooled()
    {
        var b = ArrayPool<byte>.Shared.Rent(4096);
        try { b[0] = 1; return b.Length; }
        finally { ArrayPool<byte>.Shared.Return(b); }
    }

    [Benchmark]
    public int StackAlloc() { Span<byte> b = stackalloc byte[4096]; b[0] = 1; return b.Length; }
}

BenchmarkRunner.Run<Buffers>();   // dotnet run -c Release
```

Проверено (BenchmarkDotNet 0.16.0-preview.2): `NewArray` 131.6 нс / 4120 Б, `Pooled` 7.6 нс / 0, `StackAlloc` 20.9 нс / 0. Время зависит от машины и режима, поэтому важен порядок: ни пул, ни стек не выделяют в куче; `stackalloc` по умолчанию обнуляет 4 КБ, а буфер из пула — нет.

</details>
